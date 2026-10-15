---
name: layout-stability
description: How to keep content that arrives after the first paint (error and validation messages, loading states, refreshed Defer regions, counters, streamed rows) from shifting what is already on screen in a Tesserae (C#/Transpose) app, and how to check it with screenshots.
---

# Layout stability

Anything that appears, disappears or changes size after the page is drawn can move what
the user is reading or about to click. A button that jumps the moment an error renders
above it gets mis-clicked. A view that shuffles while it loads reads as broken, whatever
it ends up looking like. Keep this in mind whenever a component shows something
conditionally or re-renders on a change.

## Where new content goes

- **At the end or the bottom.** Append rows after the existing ones, put a message under
  the field it belongs to, put a status line at the foot of the list. Nothing above the
  insertion point moves.
- **Or over the page.** Something that must be seen right away floats instead of being
  inserted into the flow: `Toast()`, a `Float(...)` anchored to an edge, a `Tooltip` on the
  control, a `Layer`.
- **An overlay hides nothing important.** It must not cover the control being used, the
  field the message is about, the primary action, an input the user is typing into, or the
  content being read. Anchor it to an edge or beside its subject, size it to its text, and
  let it be dismissed.

## Reserve the space you know you will need

- **Messages.** A field that can fail validation keeps room for its message, so the form
  does not grow a line when it fails. An input with `.Validation(...)` reserves its error
  line from the start; `.Error("…")` + `.IsInvalid()` alone only takes the space while the
  message shows, which moves the fields below (`text-box.md`). A message you render
  yourself gets the same treatment: keep its line in place and change its text.
- **Counters and changing text.** Give a number that changes a width that fits its largest
  value (`.MinWidth(...)`), and update it with `TextBlock(observable)` / `.Text(observable)`
  so the element is rewritten, not replaced.
- **Content still loading.** Show a `Skeleton(...)` shaped and sized like the result, so the
  arrival fills a box instead of growing one.

## `Defer` and `DeferSync` unmount and remount

Every refresh of a `Defer` / `DeferSync` (an observable changing, or `.Refresh()`) clears the
container and puts the loading message in, then clears it again and appends the new
component. The old content is unmounted and the new one mounted. Without a size, the region
collapses to the loading message for the length of the refresh and grows back afterwards,
and everything below it jumps twice.

So give a deferred region a size:

```csharp
// A region whose content has a known height: fix it.
Defer(selected, async item => await RenderDetailsAsync(item))
   .Height(320.px());

// A region whose content varies: floor it at the size of a typical result.
Defer(filters, async f => await RenderResultsAsync(f), loadMessage: Skeleton().H(240))
   .MinHeight(240.px());
```

- Use `.Height(...)` when the content has a known size, `.MinHeight(...)` when it varies,
  and size the `loadMessage` like the result so the region does not change size while it
  is loading.
- Put a deferred region **below** what it should not move, or at the end of its container,
  so what does move is only what comes after it.
- Rebuilding is the cost being paid here. For text that changes use `TextBlock(observable)`,
  for a list use `ObservableStack` / `KeyedObservableStack` / `ItemsList`, for streaming or
  re-rendered content use `DeltaComponent`. Those update the element that is already there
  and do not shift anything (`observables.md`, `delta-component.md`). Reach for `Defer` when
  content has to be fetched before it can be shown, and for `DeferSync` only when the *shape*
  of the content changes.

## Check it with screenshots

Reading the code does not show a layout shift. Capture the surface before the dynamic content
appears and after it does: the error raised, the list refreshed, the toast shown, a `Defer`
mid-refresh while its loading message is up. Compare the pair:

- everything present in the first capture is at the same position in the second, and
- the overlay in the second covers nothing the user needs.

Do it in every theme and in light and dark mode, since a theme can change the height of a line
or the padding of a card.

## Related

- Defer — `defer.md`, DeferWithProgress — `defer-with-progress.md`
- Skeleton — `skeleton.md`, Toast — `toast.md`, Float — `float.md`
- Observables and in-place updates — `observables.md`, `delta-component.md`, `text-block.md`
