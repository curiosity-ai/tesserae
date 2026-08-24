---
name: creating-a-component
description: How to build a custom Tesserae UI component by implementing IComponent (or deriving from ComponentBase). Use when adding a new widget/control to the Tesserae toolkit or composing a reusable component in a Tesserae (C#/Transpose) app.
---

# Creating a component (IComponent)

Every Tesserae component is just a class that knows how to produce a DOM
element. The contract is one method:

```csharp
namespace Tesserae
{
    public interface IComponent
    {
        HTMLElement Render();   // return the root element of this component
    }
}
```

`Render()` is called by parent containers (and by the mount helpers) to splice
your element into the page. Build your DOM once (usually in the constructor),
keep the root in a field, and return it from `Render()`.

## Two ways to implement

### 1. Implement `IComponent` directly (lightweight, no events needed)

Best for display-only widgets. Model after `Sparkline.cs` / `Raw.cs`.

```csharp
using static Transpose.Core.dom;
using static Tesserae.UI;   // brings in Div/Span/I and the Att( ) attributes helper

namespace Tesserae
{
    [Transpose.Name("tss.MyBadge")]                 // names the generated JS class (conventional)
    public class MyBadge : IComponent
    {
        private readonly HTMLElement _inner;

        public MyBadge(string text)
        {
            _inner = Div(Att("tss-mybadge"), Span(Att(text: text)));
        }

        public HTMLElement Render() => _inner;
    }
}
```

`Att( )` builds an `Attributes` object: `Att("css-class")`, `Att(text: "...")`,
`Att(id: "x", styles: s => s.color = "red")`. DOM builders live in
`UI.HtmlUtil` — `Div`, `Span`, `I`, `DIV()` (children-only), `Raw(html)`, etc.

### 2. Derive from `ComponentBase<T, THTML>` (fluent + events)

Best when you want click/focus/change events, margin/padding, ARIA, and a
fluent `T`-returning API. Model after `Button.cs`.

```csharp
[Transpose.Name("tss.MyToggle")]
public class MyToggle : ComponentBase<MyToggle, HTMLDivElement>
{
    public MyToggle()
    {
        InnerElement = Div(Att("tss-mytoggle"));   // set the base's backing element
        AttachClick();                            // wire base event plumbing
    }

    public override HTMLElement Render() => InnerElement;

    public MyToggle On() { InnerElement.classList.add("tss-on"); return this; }  // fluent helper
}
```

`ComponentBase` gives you `InnerElement`, `OnClick`/`OnChange`/`OnFocus`/…,
`Margin`/`Padding`, and `AriaLabel`/`AriaRole`. Return `this` from configuration
methods to keep the fluent chain.

The `Attach*` calls declare which events the component takes part in; the DOM
listener itself is only added once someone subscribes, so a component nobody wires
a handler to costs nothing. That is why a subclass that wants to listen to its own
base events calls `SubscribeClicked(...)`, `SubscribeChanged(...)`,
`SubscribeInputUpdated(...)` and friends rather than `Clicked += …` — the
subscribe methods are what trigger the wiring.

`AttachClick()` already leaves modified clicks on a link alone: when the component
is — or sits inside — an anchor with an href, a ctrl/cmd-click, shift-click or
middle-click raises nothing, so the browser opens the address in a new tab or
window instead of a handler swallowing the event. A component that dispatches its
own clicks (a raw `element.onclick`) should make the same check first with
`UI.IsModifiedLinkClick(element, mouseEvent)`, and return without calling
`StopEvent` when it is true — the open is the anchor's own default action.

## Wiring it up

In your own app a component is just a class — `new MyBadge("…")` is enough. If you want it
to read like the built-ins, add a static factory of your own beside it:

```csharp
public static class MyUI
{
    public static MyBadge MyBadge(string text) => new MyBadge(text);
}
```

and bring it into scope with `using static MyApp.MyUI;` next to `using static Tesserae.UI;`.
(Contributing the component to the toolkit itself instead? The class goes under
`Tesserae/src/Components/`, the factory in `Tesserae/src/Base/UI.Components.cs`, fluent
helpers in `Tesserae/src/Extensions/`, and a sample in `Tesserae.Tests/`.)

## Sizing, containers, mounting

- Sizing helpers (`.W()`, `.WS()`, `.Grow()`, …) work on any `IComponent`: they write the
  CSS onto the element `Render()` returns, which is the flex/grid item a `Stack` or `Grid`
  measures. (`Masonry` and `SectionStack` build a wrapper for their items and move the
  properties onto it.) A component that sizes a container of its own implements
  `ISpecialCaseStyling` and exposes a `StylingContainer`, and the helpers write there
  instead.
- **Don't spend `padding` on the element `Render()` returns.** `.P()` / `.PL()` and
  friends write an inline padding onto exactly that element, and an inline value beats
  any stylesheet, so a caller asking for room around your component silently deletes
  whatever inner offset the padding was holding. Put the component's own spacing on a
  child instead — a `margin` between the parts, or padding on an inner element — and the
  two compose: the caller's padding moves the whole control, your margin keeps its
  pieces apart. `CheckBox`, `ChoiceGroup.Choice` and `Toggle` lay their mark and their
  text out side by side in a flex row for this reason.
- To accept children, implement `IContainer<T, TChild>` and wrap each child with
  the stack-item protocol; most custom components instead *compose* existing
  components (return a `Stack().Children(...)`).
- Mount a top-level component with `MountToBody(component)` or
  `MountCenteredToBody(component)`.

## Related

- `javascript-interop` — call JS from C# when you need browser APIs in `Render()`.
- `wrap-a-javascript-library` — back a component with an existing JS library.
- Layout/sizing — `icomponent.md`, `stack.md`, `grid.md`.
- Docs: `/tesserae/extending/creating-a-component`
