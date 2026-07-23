---
name: markdown-block
description: Renders Markdown source as sanitized HTML using the bundled marked + DOMPurify libraries, with inline and block LaTeX math rendered via the bundled KaTeX. Use when displaying user-supplied or stored Markdown (including assistant/chat output with formulas) safely in a Tesserae (C#/h5) app.
---

# MarkdownBlock

Renders a Markdown string into sanitized HTML, so untrusted input is safe to
display. Setting `Text` re-renders. LaTeX math is rendered with the bundled
KaTeX library.

## Create

`MarkdownBlock(string text = "")` — renders the given Markdown. Bring the factory
into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Text` — get/set the Markdown source; assigning re-renders the sanitized HTML.
- `.HTML` — read the rendered, sanitized HTML.
- `.CanWrap` — whether the rendered text may wrap (false adds `tss-text-nowrap`).

## Math (KaTeX)

LaTeX math is supported out of the box — no extra setup, the KaTeX library and
its CSS/fonts ship with Tesserae:

- **Inline math**: wrap in single dollar signs, e.g. `$E = mc^2$`. For the `$`
  to open inline math it must be at the start of the text or preceded by a
  space, and the closing `$` must be followed by whitespace/punctuation — so a
  lone price such as `$5 or $20` in prose is left as plain text.
- **Block (display) math**: put `$$` on their own lines around the formula:

  ```markdown
  $$
  c = \pm\sqrt{a^2 + b^2}
  $$
  ```

Invalid LaTeX renders as a KaTeX error message (in red) rather than throwing, so
a bad formula never breaks the surrounding document. Math inside a `` `code
span` `` or fenced code block is not parsed.

## Example

```csharp
using static Tesserae.UI;

var md = MarkdownBlock(@"# Hello

This is **Markdown** rendered inside a Tesserae component.

- It supports lists
- And `inline code`
- And [links](https://curiosity.ai)
- And inline math like $a^2 + b^2 = c^2$

$$
\int_0^\infty e^{-x^2}\,dx = \frac{\sqrt\pi}{2}
$$
");

// Update later:
md.Text = "Updated **content**.";
```

## Related

- TextBlock — `/tesserae/components/text-block`
- AnnotatedTextEditor — `/tesserae/components/annotated-text-editor`
- Full docs & API: `/tesserae/components/markdown-block`
