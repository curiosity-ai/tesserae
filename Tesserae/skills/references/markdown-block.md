---
name: markdown-block
description: Renders Markdown source as sanitized HTML using the bundled marked + DOMPurify libraries. Use when displaying user-supplied or stored Markdown safely in a Tesserae (C#/Transpose) app.
---

# MarkdownBlock

Renders a Markdown string into sanitized HTML, so untrusted input is safe to
display. Setting `Text` re-renders.

## Create

`MarkdownBlock(string text = "", MarkdownSanitization sanitization = MarkdownSanitization.Default)`
— renders the given Markdown. Bring the factory into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Text` — get/set the Markdown source; assigning re-renders the sanitized HTML.
- `.HTML` — read the rendered, sanitized HTML.
- `.CanWrap` — whether the rendered text may wrap (false adds `tss-text-nowrap`).
- `.Sanitization(MarkdownSanitization)` — change how strictly the HTML is sanitized
  and re-render the current source with it.
- `.OnAfterRender(Action<HTMLElement>)` — called with the inner element every time
  the source is re-parsed, for post-processing the rendered tree (wrapping code
  blocks, attaching copy buttons, …).
- `.TableCopyButtons(bool show = true)` — on by default: every rendered table is
  wrapped in a `div.tss-markdown-table` with two buttons beside it, **Copy Table**
  (puts it on the clipboard as HTML plus tab-separated text, so it pastes as a table
  into a document, spreadsheet or email) and **Copy as CSV**. Pass `false` to render
  bare tables. The buttons are added after sanitization, so they appear under either
  `MarkdownSanitization` mode.

The rich copy is also available on its own as `Clipboard.CopyHtml(html, plainText)`
(call it from a click handler — browsers only allow a copy inside a user gesture).

## Sanitization

Both modes run the parsed HTML through DOMPurify; the mode picks its configuration.

- `MarkdownSanitization.Default` — DOMPurify's default profile: scripts and event
  handlers go, links and images stay.
- `MarkdownSanitization.NoLinksOrEmbeddedContent` — also removes every link and
  every piece of embedded content (anchors, images, SVG, media) and the attributes
  that fetch a remote URL. A link keeps its label as plain text and is not
  clickable; an image is dropped entirely. Use it for Markdown from a source you
  don't trust to link or to load a remote URL — an LLM reply, for instance, where a
  rendered image is a call out to a third-party server and a rendered link is a way
  to phish the reader.

```csharp
// An assistant reply: rendered, but with nothing clickable and nothing fetched.
var reply = MarkdownBlock(assistantText, MarkdownSanitization.NoLinksOrEmbeddedContent);
```

The same option exists on the static helpers behind the component:
`Markdown.ConvertMarkdownSanitized(text, sanitization)` returns the sanitized HTML
string and `Markdown.RenderMarkdownSanitized(text, sanitization)` returns a rendered
element.

## Mermaid diagrams

A fenced code block tagged `mermaid` is drawn as a diagram by [Pele](https://pele.run),
which renders Mermaid syntax (every diagram type of Mermaid 12.1) to SVG. It is not the
Mermaid library. Nothing to call: it happens in `MarkdownBlock` and in
`Markdown.RenderMarkdownSanitized`.

````csharp
MarkdownBlock(@"```mermaid
flowchart LR
  A[Markdown] --> B[Diagram]
```");
````

- **Loaded on demand.** Pele ships as its own file (`assets/js/tss-pele.min.js`, not
  linked from `index.html`) and is fetched the first time a source contains a diagram.
  Until then, and for good when a diagram cannot be drawn (a block still being streamed,
  a syntax error), the fence stays a plain code block; the reason is its tooltip.
- **Themed.** Colors, fonts, radius and series colors come from Tesserae's tokens
  (`tss.mermaid.css` sets the `--pele-*` variables on `.tss-mermaid`), so a diagram
  follows light/dark mode and custom themes without being redrawn (the Curiosity, Stratus,
  RetroOS and Rechenwerk packages each add a few rules so a diagram is framed like their code blocks). Override a
  `--pele-*` variable on `.tss-mermaid` to restyle.
- **Safe.** The SVG goes through DOMPurify; `MarkdownSanitization.NoLinksOrEmbeddedContent`
  also turns off links and images inside diagrams.
- `Markdown.ConvertMarkdownSanitized` returns a string, so it leaves the fence as a
  `language-mermaid` code block.

## Example

```csharp
using static Tesserae.UI;

var md = MarkdownBlock(@"# Hello

This is **Markdown** rendered inside a Tesserae component.

- It supports lists
- And `inline code`
- And [links](https://curiosity.ai)
");

// Update later:
md.Text = "Updated **content**.";
```

## Related

- CodeDiff — a unified diff rather than prose — `code-diff.md`
- Mermaid — the same diagram renderer as a standalone component — `mermaid.md`
- TextBlock — `/tesserae/text/text-block`
- AnnotatedTextEditor — `/tesserae/inputs/annotated-text-editor`
- Full docs & API: `/tesserae/text/markdown-block`
