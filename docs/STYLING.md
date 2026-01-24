# Styling Components

Tesserae encourages fluent, strongly-typed styling. Many components expose convenience APIs for text formatting and visual adjustments, and extension methods provide layout-related styling (spacing, sizing, alignment).

## Text formatting

Components that implement `ITextFormating` can be styled using text size, weight, and alignment helpers:

```csharp
var title = TextBlock("Dashboard")
    .Large()
    .Bold()
    .TextCenter();
```

Text formatting helpers are defined in `ITextFormatingExtensions`, including fluent shortcuts like `.Tiny()`, `.Small()`, `.Large()`, `.Bold()`, and `.TextCenter()`.【F:Tesserae/src/Extensions/ITextFormatingExtensions.cs†L14-L140】

You can also set explicit values:

```csharp
var caption = TextBlock("Updated just now")
    .SetTextSize(TextSize.Small)
    .SetTextWeight(TextWeight.SemiBold);
```

## Layout-based styling

Spacing and sizing helpers are part of `IComponentExtensions` and map to `Stack`/`Grid` layout properties:

```csharp
var card = Card(TextBlock("Summary"))
    .Padding(16.px())
    .MarginBottom(12.px())
    .Stretch();
```

These helpers set margin, padding, width/height, and flex behavior when used inside stacks or grids.【F:Tesserae/src/Extensions/IComponentExtensions.cs†L148-L328】

### Shorthand helpers

Tesserae provides short forms for common sizing and spacing APIs:

```csharp
var card = Card(TextBlock("Summary"))
    .P(16)        // Padding(16.px())
    .MT(12)       // MarginTop(12.px())
    .MB(12)       // MarginBottom(12.px())
    .W(320.px())  // Width(320.px())
    .H(200.px())  // Height(200.px())
    .S();         // Stretch()
```

Short forms include: `P`, `PT`, `PB`, `PL`, `PR`, `M`, `MT`, `MB`, `ML`, `MR`, `W`, `H`, `S`, `WS`, `HS`. They accept either `UnitSize` or an `int` (pixels).【F:Tesserae/src/Extensions/IComponentExtensions.cs†L573-L664】

## UnitSize basics

`UnitSize` represents CSS sizes and has fluent helpers for common units:

```csharp
var grid = Grid(1.fr(), 2.fr())
    .Gap(12.px())
    .Rows(64.px(), 1.fr())
    .ColumnGap(2.percent());
```

Available units include `px()`, `percent()`, `fr()`, `vw()`, and `vh()`, along with convenience helpers like `UnitSize.Auto()` and `UnitSize.FitContent()`.【F:Tesserae/src/Extensions/UnitSizeExtensions.cs†L10-L48】【F:Tesserae/src/Helpers/UnitSize.cs†L59-L64】

## Direct DOM styling (advanced)

For cases where you need a custom inline style, you can access the rendered element and apply styles directly:

```csharp
var button = Button("Download");
button.Render().style.borderRadius = "12px";
```

You can also use the fluent `Style(...)` helper:

```csharp
var button = Button("Download")
    .Style(s => s.borderRadius = "12px");
```

> Prefer Tesserae’s fluent APIs first; direct DOM styling is useful for one-off customization or for styles not yet covered by the helper methods.【F:Tesserae/src/Extensions/IComponentExtensions.cs†L573-L578】
