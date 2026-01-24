# Using Custom CSS Classes

Tesserae lets you attach custom CSS classes to any `IComponent` via fluent helpers on the `UI` static class.

## Adding and removing classes

```csharp
var card = Card(TextBlock("Hello"))
    .Class("my-card")
    .Class("is-highlighted");

// Later, remove a class
card.RemoveClass("is-highlighted");
```

`Class(...)` and `RemoveClass(...)` are extension helpers in `UI.Components.cs`, and they work with any component implementing `IComponent`.【F:Tesserae/src/Base/UI.Components.cs†L66-L120】

## Example stylesheet

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

- Use CSS classes for complex visuals or when you want to reuse styles across components.
- Keep layout rules in CSS when they are shared across multiple screens; use fluent APIs for per-component adjustments.
