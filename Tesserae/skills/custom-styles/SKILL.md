---
name: custom-styles
description: Fluent helpers for attaching and removing custom CSS classes on any Tesserae component. Use when applying custom CSS classes or reusable stylesheet rules to components in a Tesserae (C#/h5) app.
---

# Custom CSS Classes

Attach your own CSS classes to any `IComponent` and pair them with a regular
stylesheet. Use classes for complex or reusable visuals; use the fluent sizing
APIs for per-component adjustments. Bring factories into scope with
`using static Tesserae.UI;`.

## Key APIs

Extension helpers (in `UI.Components.cs`) that work on any `IComponent`:

- `.Class(string name)` — add a CSS class. Chainable; call repeatedly to add
  several classes.
- `.RemoveClass(string name)` — remove a class later.

## Example

```csharp
using static Tesserae.UI;

var card = Card(TextBlock("Hello"))
    .Class("my-card")
    .Class("is-highlighted");

// Later, remove a class
card.RemoveClass("is-highlighted");
```

Matching stylesheet:

```css
.my-card {
  border: 1px solid rgba(0, 0, 0, 0.1);
  border-radius: 12px;
  padding: 16px;
}

.my-card.is-highlighted {
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.12);
}
```

## Tips

- Keep layout rules shared across screens in CSS; use fluent APIs for one-offs.
- For a single inline tweak, prefer `.Style(s => ...)` (see Styling) over a
  one-shot class.

## Related

- Styling — `.../styling/SKILL.md`
- Full docs & API: `/tesserae/get-started/custom-styles`
