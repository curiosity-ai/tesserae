# Color Styling

Tesserae uses CSS variables for theme colors and exposes helpers for custom color management.

## Theme colors

Use `Theme` constants for consistent colors across components:

```csharp
var badge = Badge("New")
    .Background(Theme.Primary.Background)
    .Foreground(Theme.Primary.Foreground);
```

The theme exposes default, primary, secondary, danger, and success color variables through nested classes like `Theme.Default` and `Theme.Primary`.【F:Tesserae/src/Base/UI.Theme.cs†L230-L310】

## Switching light/dark mode

```csharp
Theme.Dark();
// ...later
Theme.Light();
```

Theme mode toggles a `tss-dark-mode` CSS class on the document body.【F:Tesserae/src/Base/UI.Theme.cs†L15-L64】

## Customizing theme colors

To override primary and background colors globally, call:

```csharp
Theme.SetPrimary(Color.FromString("#0063B1"), Color.FromString("#2899F5"));
Theme.SetBackground(Color.FromString("#FFFFFF"), Color.FromString("#1B1A19"));
```

`SetPrimary` and `SetBackground` generate CSS variables for both light and dark themes at runtime.【F:Tesserae/src/Base/UI.Theme.cs†L74-L229】

## Working with Color

The `Color` helper supports parsing from hex/rgba strings and constructing from ARGB components:

```csharp
var primary = Color.FromArgb(0, 120, 212);
var custom = Color.FromString("rgba(16, 110, 190, 1)");
```

`Color` also supports evaluating CSS variables via `Color.EvalVar`.【F:Tesserae/src/Base/Color.cs†L19-L73】
