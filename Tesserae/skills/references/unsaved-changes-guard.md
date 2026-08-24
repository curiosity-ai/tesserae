---
name: unsaved-changes-guard
description: Stops an editor's unsaved changes from being lost silently — warns on tab close/reload and blocks in-app navigation until the user saves, discards, or stays. Use when building an editor (form, code editor, settings panel) whose changes can be navigated or reloaded away from in a Tesserae (C#/Transpose) app.
---

# UnsavedChangesGuard / TabSaveIndicator

Two static helpers that cooperate to stop unsaved editor state from disappearing
without the user choosing that:

- **`UnsavedChangesGuard`** — the guard itself. Listens for `beforeunload` (tab
  close/reload) and, when wired into `Router.OnBeforeNavigate`, intercepts
  in-app navigation, shows a "Save and leave / Leave without saving / Stay
  here" dialog, and only lets the navigation through once the user decides.
- **`TabSaveIndicator`** — a companion for editors hosted as `Pivot` tabs. It
  toggles a CSS class on the tab's title element to show an unsaved-changes dot
  and lets the editor register a save handler the guard can call, so a view with
  many open tabs doesn't need its own bookkeeping of which tabs are dirty and how
  to save each one.

Use `UnsavedChangesGuard` directly for a single editor (a `Modal`, a
standalone view). Add `TabSaveIndicator` on top when the editor is one of
several tabs in a `Pivot` and the host wants "leaving loses everything that's
dirty" semantics across all of them at once.

## UnsavedChangesGuard

- `Track(string key, Func<string> name, Func<bool> isDirty, Func<Task<bool>> save)`
  — registers a single editor for as long as it's on screen. `key` identifies
  it (re-registering the same key replaces the entry), `name` is what the user
  is told has unsaved changes, `isDirty` is polled on every navigation attempt
  and unload, `save` returns whether the save actually went through.
- `Forget(string key)` — call on the editor's close/removal path. Always pair
  every `Track` with a matching `Forget`, or the guard keeps asking about an
  editor that's gone.
- `TrackOpenTabs()` / `ForgetOpenTabs()` — call when a view hosting editors as
  `Pivot` tabs mounts/unmounts. While active, the guard also reads
  `TabSaveIndicator.DirtyTabIds()` off the DOM instead of requiring every tab
  to register itself individually.
- `CanNavigateAway(Router.State target, Router.State from = null)` — the
  `Router.OnBeforeNavigate` answer. Returns `true` when nothing would be lost.
  Otherwise it cancels this navigation attempt, shows the confirmation dialog,
  and — if the user chooses to leave (with or without saving) — re-issues the
  navigation itself once they've decided, since the router can't `await` inside
  its own hook. Pass the handler's second argument as `from` so that a
  "navigation" to the route already showing isn't taken for leaving it.

`Router` only keeps a single `OnBeforeNavigate` handler (see `routing.md`), so
an app with other before-navigate logic (e.g. closing a preview panel) has to
call `CanNavigateAway` explicitly from inside its own handler rather than
relying on the guard to register itself:

```csharp
Router.OnBeforeNavigate((newState, currentState, isBack) =>
{
    if (SomeOtherReasonToBlock(newState)) return false;

    return UnsavedChangesGuard.CanNavigateAway(newState, currentState);
});
```

That one handler covers every navigation the router performs — `Router.Navigate`,
a hash change from a link or the address bar, the browser's back/forward buttons,
and `Router.Push`/`Replace`. `Push`/`Replace` skip route matching but still ask
the guard, and return `false` when it refuses, so an app whose nav pushes the URL
and then swaps the view itself (a sidebar, a breadcrumb) must respect that answer
instead of rendering anyway:

```csharp
sidebarItem.OnClick(() =>
{
    if (!Router.Push($"#/view/{item.Name}")) return; // guard said no

    currentPage.Value = item;
});
```

Once the user chooses to leave, the guard re-issues the navigation through
`Router.Navigate`, so the route registered for that path is what actually shows
the new view — an app that navigates by `Push` alone needs that route registered
for the "leave" path to land anywhere.

Pass the handler's `fromState` through to `CanNavigateAway` as well: a
"navigation" to the route already showing (re-clicking the current nav item)
loses nothing, and the guard uses `fromState` to tell that apart from leaving.

Two exits don't go through the router, and are handled differently:

- **Leaving the page** (tab close, reload, a link to another site) — the guard's
  own `beforeunload` listener covers this, with the browser's native prompt. It
  is installed while anything is tracked, so there is nothing to wire up.
- **Closing a modal directly** (its own close button, a light-dismiss click) or
  **closing a pivot tab** — nothing reaches `Router`, so the surface has to ask
  first: a `Pivot` tab takes an `onBeforeClose` guard for exactly this, and
  `TabSaveIndicator.IsDirty`/`CanSave`/`SaveAsync` are what it needs to answer.

## TabSaveIndicator

- `TabId(string itemType, object uid)` — a stable id convention, e.g.
  `tab-endpoint-{uid}`. Any stable string works; this is just a helper for the
  common "one editor per graph node" case.
- `Title(string tabIndicatorId, string text)` /
  `Title(string tabIndicatorId, string text, UIcons icon)` — the tab title
  `Func<IComponent>` to pass to `.Pivot(...)`. Same styling as `PivotTitle`, plus
  the `id` the indicator needs.
- `MarkDirty(string tabIndicatorId)` / `MarkClean(string tabIndicatorId)` —
  toggle the tab's unsaved-changes dot. Call from the editor's own change-tracking
  (a `TextArea.OnChange`, a `Validator`, a `SettingsHolder.HasChanged`
  check, …). On a `closeable` tab the dot stands in for the close cross and gives
  way to it while the tab is pointed at, so the tab never changes width; on a tab
  without a close button it sits beside the label. See `pivot.md`.
- `IsDirty(string tabIndicatorId)` — read back the current state, e.g. to
  decide whether Ctrl+S has anything to do.
- `OnSave(string tabIndicatorId, Func<Task<bool>> saveAsync)` — registers the
  tab's save handler, so `UnsavedChangesGuard` (and anything else) can save it
  without knowing what kind of editor it is. `SaveAsync(tabIndicatorId)` runs
  it back; `CanSave(tabIndicatorId)` reports whether one was registered.
- `Forget(string tabIndicatorId)` — drop the save handler when the tab closes.
- `DirtyTabIds()` / `TitleOf(string tabIndicatorId)` — used internally by
  `UnsavedChangesGuard.TrackOpenTabs()`; reading the dirty set off the DOM
  (rather than a registry) means a torn-down tab can't leave a stale entry
  behind.

The tab title element needs the matching `id` for any of this to find it, which
is what `TabSaveIndicator.Title(...)` is for. Building the title by hand works
too, as long as it carries the `id` (see `.Id(...)` in `icomponent.md`) — but
build it from `Button(text).NoBackground().Regular()` like `PivotTitle` does,
because a bare `TextBlock` gets none of the tab strip's padding and sits wrong
in the strip. The `*` itself is a `::after` on the title element, so it needs no
room made for it.

## Example

```csharp
using static Tesserae.UI;

// One editor hosted in a Modal — no Pivot involved.
var isDirty = new SettableObservable<bool>(false);

Func<Task<bool>> saveAsync = async () =>
{
    var ok = await SaveDocumentAsync();
    if (ok) isDirty.Value = false;
    return ok;
};

editor.WhenMounted(() => UnsavedChangesGuard.Track(
    key:     "doc-editor",
    name:    () => "My document",
    isDirty: () => isDirty.Value,
    save:    saveAsync));

editor.WhenRemoved(() => UnsavedChangesGuard.Forget("doc-editor"));
```

```csharp
// A Pivot-hosted editor tab: the host tracks all open tabs at once,
// each editor only has to maintain its own indicator + save handler.
const string tabIndicatorId = "tab-endpoint-abc123";

hostView.WhenMounted(() => UnsavedChangesGuard.TrackOpenTabs());
hostView.WhenRemoved(() => UnsavedChangesGuard.ForgetOpenTabs());

pivot.Pivot("endpoint-abc123",
    TabSaveIndicator.Title(tabIndicatorId, "Endpoint", UIcons.CodeSimple),
    () => EndpointEditor(), cached: true, closeable: true);

codeEditor.OnChange((_, __) => TabSaveIndicator.MarkDirty(tabIndicatorId));
TabSaveIndicator.OnSave(tabIndicatorId, SaveEndpointAsync);

// when the tab closes:
TabSaveIndicator.Forget(tabIndicatorId);
```

## Related

- Routing (`Router.OnBeforeNavigate`, `Router.Navigate`) — `routing.md`
- Pivot (tabbed hosting) — `pivot.md`
- Dialog (what the confirmation prompt is built from) — `dialog.md`
- Validator (a common `isDirty` source) — `validator.md`
