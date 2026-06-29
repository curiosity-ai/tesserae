---
name: delta-component
description: A wrapper that diff-patches its DOM to match new content instead of fully re-rendering, animating only what changed. Use for incremental/streaming UI updates in a Tesserae (C#/h5) app.
---

# DeltaComponent

Holds an initial component and, on each `ReplaceContent`, diffs the new DOM tree
against the current one and patches only the differences. It detects text
appends and adds them as new spans (handy for streaming/typing effects),
avoiding a full re-render. Can optionally render inside a Shadow DOM root.

## Create

`UI.DeltaComponent(IComponent initial, bool useShadowDom = false)` — wraps the initial content.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Animated()` — adds a `tss-fade-in` class to newly inserted nodes.
- `.ReplaceContent(IComponent newContent)` — diff against current content and patch in place.

## Example

```csharp
using static Tesserae.UI;

var delta = DeltaComponent(TextBlock("Starting...")).Animated();

// later, stream/update — only the changed/appended parts re-render
delta.ReplaceContent(TextBlock("Starting... done"));

var ui = VStack().WS().Children(delta);
```

## Related

- TextBlock — `text-block.md`
- Full docs & API: `/tesserae/components/delta-component`
