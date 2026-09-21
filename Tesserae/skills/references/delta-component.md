---
name: delta-component
description: A wrapper that diff-patches its DOM to match new content instead of fully re-rendering, animating only what changed. Use for incremental/streaming UI updates in a Tesserae (C#/Transpose) app.
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

## What is kept, and what is swapped

A node that stays keeps the event listeners its component attached to it, and no patch can move
those. So two nodes are reconciled only when they are the same component; a node whose component
changed between renders is swapped out whole. Without that, a control re-rendered as a different
component looks right and still answers to the one it used to be - a tool call that became a
"tools used" group mid-stream opening the chip it was.

Nodes that are not elements carry no listeners and are always reconciled, which is what keeps the
text-append path (the streaming effect) intact.

Which component a node belongs to is read off the element, in this order:

1. The `tss-c` attribute, if either node carries one. `.ReconcileAs("Name")` writes it, and it is
   taken at its word: an element that names itself and one that does not are not the same
   component either.
2. Otherwise the element's **first CSS class**, which is the class a component that builds its own
   root element puts on it before anything else is added.

The first class cannot tell two components apart when **neither of them owns its root** - a
component that composes a `Stack` and returns the stack's element answers `tss-stack`, the same as
every other component built that way. The ones in the toolkit that do this name themselves
(`SectionTitle`, `SearchableList`, `ToggleButton`, `SaveButton`, …); a component of your own that
does the same, and that may end up in the same slot as another, should too:

```csharp
_stack = VStack().Class("tss-tool-chip").Children(header).ReconcileAs(nameof(ToolChip));
```

## Sizing a DeltaComponent

`Render()` hands out whatever the content rendered - this component has no element of its own - so
a `.WS()`, a `.Grow()` or the stack-item class its parent added all live on the content's root. A
swap carries them onto the new root, so sizing set on the `DeltaComponent` survives a change of
component. An `.Id()` or a `.Class()` set on it does not: put those on a wrapper if you need them
to outlive a swap.

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
