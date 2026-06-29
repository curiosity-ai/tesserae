---
name: creating-a-component
description: How to build a custom Tesserae UI component by implementing IComponent (or deriving from ComponentBase). Use when adding a new widget/control to the Tesserae toolkit or composing a reusable component in a Tesserae (C#/h5) app.
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
using static H5.Core.dom;
using static Tesserae.UI;   // brings in Div/Span/I and the _( ) attributes helper

namespace Tesserae
{
    [H5.Name("tss.MyBadge")]                 // names the generated JS class (conventional)
    public class MyBadge : IComponent
    {
        private readonly HTMLElement _inner;

        public MyBadge(string text)
        {
            _inner = Div(_("tss-mybadge"), Span(_(text: text)));
        }

        public HTMLElement Render() => _inner;
    }
}
```

`_( )` builds an `Attributes` object: `_("css-class")`, `_(text: "...")`,
`_(id: "x", styles: s => s.color = "red")`. DOM builders live in
`UI.HtmlUtil` — `Div`, `Span`, `I`, `DIV()` (children-only), `Raw(html)`, etc.

### 2. Derive from `ComponentBase<T, THTML>` (fluent + events)

Best when you want click/focus/change events, margin/padding, ARIA, and a
fluent `T`-returning API. Model after `Button.cs`.

```csharp
[H5.Name("tss.MyToggle")]
public class MyToggle : ComponentBase<MyToggle, HTMLDivElement>
{
    public MyToggle()
    {
        InnerElement = Div(_("tss-mytoggle"));   // set the base's backing element
        AttachClick();                            // wire base event plumbing
    }

    public override HTMLElement Render() => InnerElement;

    public MyToggle On() { InnerElement.classList.add("tss-on"); return this; }  // fluent helper
}
```

`ComponentBase` gives you `InnerElement`, `OnClick`/`OnChange`/`OnFocus`/…,
`Margin`/`Padding`, and `AriaLabel`/`AriaRole`. Return `this` from configuration
methods to keep the fluent chain.

## Wiring it into the toolkit (the repo convention)

1. Add the class under `Tesserae/src/Components/`.
2. Add a factory in `Tesserae/src/Base/UI.Components.cs`:
   `public static MyBadge MyBadge(string text) => new MyBadge(text);`
3. Add fluent helpers/extensions in `Tesserae/src/Extensions/` if needed.
4. Add a sample in `Tesserae.Tests/`.

## Sizing, containers, mounting

- Sizing helpers (`.W()`, `.WS()`, `.Grow()`, …) work on any `IComponent` via
  the wrap-and-transfer protocol — see `Stack.CopyStylesDefinedWithExtension`.
  A component can opt out of wrapping by implementing `ISpecialCaseStyling` and
  exposing a `StylingContainer`.
- To accept children, implement `IContainer<T, TChild>` and wrap each child with
  the stack-item protocol; most custom components instead *compose* existing
  components (return a `Stack().Children(...)`).
- Mount a top-level component with `MountToBody(component)` or
  `MountCenteredToBody(component)`.

## Related

- `javascript-interop` — call JS from C# when you need browser APIs in `Render()`.
- `wrap-a-javascript-library` — back a component with an existing JS library.
- Layout/sizing internals — see the toolkit `CLAUDE.md` "Layout system" section.
- Docs: `/tesserae/extending/creating-a-component`
