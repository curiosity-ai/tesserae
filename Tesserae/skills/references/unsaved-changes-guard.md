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
  toggles a CSS class on the tab's title element to show a "*" and lets the
  editor register a save handler the guard can call, so a view with many open
  tabs doesn't need its own bookkeeping of which tabs are dirty and how to
  save each one.

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
- `CanNavigateAway(Router.State target)` — the `Router.OnBeforeNavigate` answer.
  Returns `true` when nothing would be lost. Otherwise it cancels this
  navigation attempt, shows the confirmation dialog, and — if the user
  chooses to leave (with or without saving) — re-issues the navigation itself
  once they've decided, since the router can't `await` inside its own hook.

`Router` only keeps a single `OnBeforeNavigate` handler (see `routing.md`), so
an app with other before-navigate logic (e.g. closing a preview panel) has to
call `CanNavigateAway` explicitly from inside its own handler rather than
relying on the guard to register itself:

```csharp
Router.OnBeforeNavigate((newState, currentState, isBack) =>
{
    if (SomeOtherReasonToBlock(newState)) return false;

    return UnsavedChangesGuard.CanNavigateAway(newState);
});
```

Closing a modal directly (its own close button, a light-dismiss click) is
**not** covered by the guard — that path never reaches `Router`, so it has to
ask its own confirmation before hiding the modal.

## TabSaveIndicator

- `TabId(string itemType, object uid)` — a stable id convention, e.g.
  `tab-endpoint-{uid}`. Any stable string works; this is just a helper for the
  common "one editor per graph node" case.
- `MarkDirty(string tabIndicatorId)` / `MarkClean(string tabIndicatorId)` —
  toggle the tab's "*" indicator. Call from the editor's own change-tracking
  (a `TextArea.OnChanged`, a `Validator`, a `SettingsHolder.HasChanged`
  check, …).
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

The tab title element needs the matching `id` for any of this to find it —
give the `IComponent` returned by your `PivotTitle`-equivalent that `id` (see
`.Id(...)` in `icomponent.md`).

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

codeEditor.OnChanged(() => TabSaveIndicator.MarkDirty(tabIndicatorId));
TabSaveIndicator.OnSave(tabIndicatorId, SaveEndpointAsync);

// when the tab closes:
TabSaveIndicator.Forget(tabIndicatorId);
```

## Related

- Routing (`Router.OnBeforeNavigate`, `Router.Navigate`) — `routing.md`
- Pivot (tabbed hosting) — `pivot.md`
- Dialog (what the confirmation prompt is built from) — `dialog.md`
- Validator (a common `isDirty` source) — `validator.md`
