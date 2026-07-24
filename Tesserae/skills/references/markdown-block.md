---
name: markdown-block
description: Renders Markdown source as sanitized HTML using the bundled marked + DOMPurify libraries. Use when displaying user-supplied or stored Markdown safely in a Tesserae (C#/Transpose) app.
---

# MarkdownBlock

Renders a Markdown string into sanitized HTML, so untrusted input is safe to
display. Setting `Text` re-renders.

## Create

`MarkdownBlock(string text = "")` — renders the given Markdown. Bring the factory
into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Text` — get/set the Markdown source; assigning re-renders the sanitized HTML.
- `.HTML` — read the rendered, sanitized HTML.
- `.CanWrap` — whether the rendered text may wrap (false adds `tss-text-nowrap`).

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

- TextBlock — `/tesserae/components/text-block`
- AnnotatedTextEditor — `/tesserae/components/annotated-text-editor`
- Full docs & API: `/tesserae/components/markdown-block`
