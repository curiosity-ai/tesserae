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

Which component a node belongs to is worked out by the toolkit, not declared by the component.
Nothing has to be added to a component of your own to make this work:

1. **The component recorded on the element.** Every child of a `Stack` or a `Grid` passes through
   one place that holds it as an `IComponent`, and the component behind it is noted on the element
   there. So a component of yours is identified by the fact that it was added to a container, which
   is also what separates two components that borrow the same kind of root: a `SectionTitle` and a
   `SearchableList` are both a `Stack`'s element, and so is anything composed that way.
2. **Otherwise the element's first CSS class.** This catches an element built straight into its
   parent rather than added through a container, and the markup inside a component.

Both are checked, so the pair is never weaker than either.

### What this does not cover

**Two instances of the same component keep the first one's handlers.** Identity is the component,
not the instance, so re-rendering `MyChip(onPicked: a)` as `MyChip(onPicked: b)` reconciles the two
and the node goes on calling `a`. Nothing about a rendered element says which closure is behind its
listener, so no reconciler can see this. If a handler closes over something that changes between
renders, read the current value inside the handler rather than capturing it:

```csharp
//Wrong: the node keeps the first render's handler, and with it the first render's index.
_stack.Render().addEventListener("click", _ => Select(index));

//Right: the handler is the same either way, and reads what is current when it runs.
_stack.Render().addEventListener("click", _ => Select(_currentIndex));
```

**An element spliced into a parent by hand carries no component.** `Div(Att("list"),
chip.Render(), group.Render())` never passes a container, so those two are told apart only by their
first class, which for anything built on a `Stack` is the same. Build the list with
`VStack().Children(chip, group)` and both are identified.

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
