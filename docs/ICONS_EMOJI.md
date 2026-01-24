# Icons & Emoji

Tesserae exposes a unified icon story through `UIcons` (Fluent-inspired icon set) and `Emoji` enums, plus helpers to render them in components.

## Icon components

Use `Icon(...)` to render icons as standalone components:

```csharp
var settings = Icon(UIcons.Settings, UIconsWeight.Regular, TextSize.Small);
var party = Icon(Emoji.ConfettiBall, TextSize.Medium);
```

Factory helpers and icon overloads are defined in `UI.Components.cs`, with dedicated `Icon` component support for both `UIcons` and `Emoji`.【F:Tesserae/src/Base/UI.Components.cs†L401-L409】【F:Tesserae/src/Components/Icon.cs†L17-L66】

## Using icons in buttons and commands

Many components accept icons directly. For example, `Button.SetIcon` supports both enums:

```csharp
var save = Button("Save").SetIcon(UIcons.Save, color: Theme.Primary.Foreground);
var celebrate = Button("Yay").SetIcon(Emoji.ConfettiBall);
```

`Button.SetIcon` overloads for `UIcons` and `Emoji` are available on the `Button` component.【F:Tesserae/src/Components/Button.cs†L532-L553】

## Icon enums

- `UIcons` is a large icon enum used throughout the component library (sidebar buttons, dropdowns, toolbars, etc.).【F:Tesserae/src/Icons/UIcons.cs†L1-L40】
- `Emoji` is an enum representing emoji glyphs for quick inline usage.【F:Tesserae/src/Icons/Emoji.cs†L1-L40】

When you pick an enum value, Tesserae converts it into the appropriate CSS class or glyph at render time.
