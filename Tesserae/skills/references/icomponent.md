---
name: icomponent
description: The IComponent interface and the complete catalog of fluent extension methods available on every component (sizing, layout, spacing, text, styling, tooltips, accessibility, gestures, binding, visibility, lifecycle). Use when configuring any Tesserae component fluently or looking up which `.Method()` exists in a Tesserae (C#/Transpose) app.
---

# IComponent and its extension methods

`IComponent` is the one interface every Tesserae component implements:

```csharp
namespace Tesserae
{
    public interface IComponent
    {
        HTMLElement Render();   // produce the component's root DOM element
    }
}
```

Almost all of the fluent API you call on components — `.WS()`, `.Margin(8)`,
`.Tooltip(...)`, `.Class(...)`, `.Bind(...)` — are **extension methods on
`IComponent`**, not members of the individual components. They are generic
(`T Method<T>(this T component) where T : IComponent`) so they return the
concrete type and keep the chain going. Bring them into scope with:

```csharp
using static Tesserae.UI;
```

Call them before adding the component to a container — sizing/placement markers
are transferred onto the container's item wrapper (the wrap-and-transfer
protocol). See `tesserae-overview` and the repo `CLAUDE.md` "Layout system".

## Sizing — `IComponentExtensions.cs`

| Method | Effect |
| --- | --- |
| `.Width(UnitSize)` / `.W(...)` | fixed `width` |
| `.Height(UnitSize)` / `.H(...)` | fixed `height` |
| `.MinWidth` / `.MaxWidth` / `.MinHeight` / `.MaxHeight` | bounds |
| `.WidthAuto()` / `.HeightAuto()` | `width`/`height: auto` |
| `.WidthStretch()` / `.WS()` | `width: 100%` |
| `.HeightStretch()` / `.HS()` | `height: 100%` |
| `.MinHeightStretch()` | `min-height: 100%` |
| `.Stretch()` / `.S()` | both `width` and `height` `100%` |
| `.Grow(int = 1)` | `flex-grow` (inside a `Stack`) |
| `.Shrink()` / `.NoShrink()` | `flex-shrink` `1` / `0` |

`UnitSize` comes from helpers like `100.px()`, `50.percent()`, `1.em()`,
`2.fr()` (see `styling`).

## Spacing — `IComponentExtensions.cs`

`.Margin(UnitSize)` / `.MarginLeft` / `.MarginRight` / `.MarginTop` /
`.MarginBottom`, with shorthands `.M()` / `.ML()` / `.MR()` / `.MT()` / `.MB()`.
Same set for padding: `.Padding(...)` / `.PaddingLeft…` and `.P()` / `.PL()` /
`.PR()` / `.PT()` / `.PB()`. (Direct CSS forms also live in `StyleExtensions`:
`.Margin(string)`, `.Padding(string)`.)

## Alignment & grid placement — `IComponentExtensions.cs`

- Self cross-axis: `.AlignAuto()`, `.AlignStretch()`, `.AlignBaseline()`,
  `.AlignStart()`, `.AlignCenter()`, `.AlignEnd()`.
- Self main-axis (justify-self): `.JustifyStart()`, `.JustifyCenter()`,
  `.JustifyEnd()`.
- Grid: `.GridColumn(start, end)`, `.GridColumnStretch()`, `.GridRow(start, end)`,
  `.GridRowStretch()` (call before `.Add`).

## Visibility & animation — `IComponentExtensions.cs`

`.Show()`, `.Collapse()`, `.Fade()` / `.Fade(Action andThen)` /
`.Fade(Func<Task>)`, `.LightFade()`, `.FadeThenCollapse()`.

## Text formatting — `ITextFormatingExtensions.cs`

For text-bearing components (`TextBlock`, `Button`, `Label`, …):

- Size presets: `.Tiny()`, `.XSmall()`, `.Small()`, `.SmallPlus()`, `.Medium()`,
  `.MediumPlus()`, `.Large()`, `.XLarge()`, `.XXLarge()`, `.Mega()`.
- Weight: `.Regular()`, `.SemiBold()`, `.Bold()`.
- Align: `.TextLeft()`, `.TextCenter()`, `.TextRight()`.
- Explicit: `.SetTextSize(TextSize)`, `.SetTextWeight(TextWeight)`,
  `.SetTextAlign(TextAlign)`.
- Wrapping: `.SetCanWrap(bool)` (`IHaveTextWrappingOptionsExtensions`).

## Styling — `StyleExtensions.cs` / `UI.Components.cs`

- `.Class(string)` / `.RemoveClass(string)` — add/remove a CSS class.
- `.Style(Action<CSSStyleDeclaration>)` — set inline CSS directly:
  `.Style(s => s.color = "red")`.
- `.Background(...)`, `.Foreground(...)`, `.Rounded(...)`.
- `.Id(string)` — set the element id.

## Tooltips — `IComponentExtensions.cs`

```csharp
.Tooltip("Plain or <b>HTML</b> text", placement: TooltipPlacement.Top)
.Tooltip(componentTooltip, interactive: true)   // rich IComponent tooltip
.RemoveTooltip()
```

(See `tippy` for the underlying Tippy.js wrapper.)

## Accessibility — `IAccessibilityExtensions.cs`

`.AriaLabel`, `.AriaLabelledBy`, `.AriaDescribedBy`, `.AriaRole`, `.AriaHidden`,
`.AriaExpanded`, `.AriaSelected`, `.AriaChecked`, `.AriaDisabled`, `.AriaBusy`,
`.AriaCurrent`, `.AriaLive`, `.AriaAtomic`, `.AriaControls`, `.AriaHasPopup`.
Focus order: `.TabIndex(int)`, `.SkipTab()`. (See `accessibility`.)

## Gestures — `GestureExtensions.cs`

`.OnTapped(...)`, `.OnDoubleTapped(...)`, `.OnLongPress(...)`, `.OnPan(...)`,
`.OnPinch(...)`, `.OnRotate(...)`. (See `gestures`.)

## Reactive binding — `BindingExtensions.cs`

`.Bind(SettableObservable<T> source)` — keep the component in sync with an
observable (also overloads for `IReadOnlyList<T>` and an explicit
`SubscriptionScope`). (See `observables`.)

## Validation — `ICanValidateExtensions.cs` / `ValidationExtensions.cs`

On validatable inputs: `.Validation(c => error-or-null)`, `.Error("msg")`,
`.IsInvalid()`. (See `validator`.)

## Lifecycle & utility — `IComponentExtensions.cs` / `UI.Components.cs`

- `.WhenMounted(Action)`, `.WhenMountedDelayed(TimeSpan, Action, bool)`,
  `.WhenRemoved(Action)` — run code when the element enters/leaves the DOM.
- `.ScrollIntoView()`.
- `.Do(Action<T>)` — run an arbitrary action on the component inside a chain.
- `.Var(out T var)` — capture the component into a variable mid-chain.
- `.ToToggle()` — wrap a `Button` as a `ToggleButton`.

## Example

```csharp
using static Tesserae.UI;

var name = TextBox().SetPlaceholder("Name");

var row = HStack().Children(
    TextBlock("Profile").Bold().Large(),
    name.WS().Validation(t => string.IsNullOrEmpty(t.Text) ? "Required" : null),
    Button("Save")
        .Primary()
        .Margin(8.px())
        .Tooltip("Save changes")
        .AriaLabel("Save profile")
        .Do(b => b.WidthStretch())
).Grow().AlignCenter();
```

## Related

- `creating-a-component` — implementing `IComponent` yourself.
- `styling`, `layout-alignment`, `accessibility`, `gestures`, `observables`,
  `tippy`, `validator` — deeper dives on each extension group.
- `tesserae-overview` — the wrap-and-transfer protocol and layout cheat-sheet.
