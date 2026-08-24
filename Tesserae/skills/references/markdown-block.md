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
- TextBlock — `/tesserae/components/text-block`
- AnnotatedTextEditor — `/tesserae/components/annotated-text-editor`
- Full docs & API: `/tesserae/components/markdown-block`
