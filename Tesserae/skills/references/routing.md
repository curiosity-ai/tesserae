---
name: routing
description: The built-in lightweight hash-based Router for SPA-style navigation, route parameters, and guards. Use when setting up routing/navigation between views in a Tesserae (C#/Transpose) app.
---

# Routing

`Router` is a process-wide singleton that listens for hash URL changes
(`window.location.hash`, e.g. `#/view/details`), matches them against
registered patterns, and calls handlers with parsed parameters.

## Key APIs / patterns

Register routes once at startup (not inside `Render()`), then initialize:

- `Router.Register(string id, Action<Parameters> handler)` — map an id + path
  to a callback. A segment prefixed with `:` (e.g. `:id`) is a captured
  parameter. Leading `#`/`/` are normalized, so `"view/:id"` == `"#/view/:id"`.
  Only the **first** registration per id is kept.
- `Router.Initialize()` — start listening (call once).
- `Router.Refresh(onDone: Router.ForceMatchCurrent)` — build the match list and
  match the URL the app loaded on without changing it.

Navigate (changes the URL only — does **not** re-run the callback by itself):

- `Router.Push(path)` — push a new history entry. Returns `false` when an
  `OnBeforeNavigate` guard refused: the URL is left alone and the caller must not
  show the new view either.
- `Router.Replace(path)` — update the URL in place; same `bool` contract.
- `Router.Navigate(path, reload: true)` / `Router.ForceMatchCurrent()` — re-run
  the matcher and re-activate the matching route.

After a programmatic `Push`/`Replace`, either call `ForceMatchCurrent()` or
update your view directly in the click handler — pick one and stay consistent.

Parameters: path `:segments` are captured positionally; query-string pairs
(`?term=x&page=2`) land in the same `Parameters` collection — just read the keys.
Avoid reusing a `:segment` name as a query key (one shared collection).

Reflect view state (open panel, selected tab, filters) in the URL's query
segment so it survives refresh and can be shared as a deep link. Prefer
`RouteState` (`route-state.md`) for reading and writing it: `Set`/`Clear`/`Update`,
`Consume` for one-shot keys, `SetWithHistory` for tabs, and `RouteState.Current.IsOn`/`IsUnder`/`Matches` to ask
where the page is. The members below are what it is built on:

- `Router.GetQueryParameters()` — a **copy** of the current `Parameters` (route
  `:variables` and query keys). Changing it changes nothing.
- `Router.ReplaceQueryParameters(p => p.With("preview", id))` — clone, update,
  rewrite only the hash's query segment; no-ops when nothing changed. Use
  `.Remove(key)` to clear. The route handler is **not** re-invoked — the URL
  updates silently under the running view. Default is `replaceState` (no
  history entry); pass `pushToHistory: true` for one. It does nothing before the
  first route has matched. A route's `:variables` are not written into the query
  (`#/node/abc?k=v`, not `?uid=abc&k=v`).
- On the next navigation or page load the keys arrive in the handler's
  `Parameters` — restore the state from there.
- `Push`/`Replace` re-derive the current `Parameters` from the path you pass,
  so `GetQueryParameters()` stays in sync even without a route re-match.

- `Router.CurrentHash` — the hash the router matches (`window.location.hash`
  after the `OnTransformRoutes` transform). Readable before the first match.
- The handler receives a copy of the `Parameters`: `p.Remove("x")` inside it
  does not change the URL or what the next write keeps.
- `:variable` values are the raw hash segment, not URI-decoded. A query key with
  the name of a `:variable` wins over it in the merged `Parameters`.

Guards / events: `Router.OnBeforeNavigate(...)` (return `false` to cancel; one
handler, the last registered), `Router.OnNavigated(...)` (many; fires after the
handler's synchronous part, so before an `async` handler has finished),
`Router.OnNotMatched(...)`. `Router.OnTransformRoutes(url => ...)` rewrites the
hash before it is matched (to cut a token off the end, say) and
`Router.OnWíllNavigate(url => ...)` can veto `Navigate` before anything happens.
A handler that returns `false` (the `Func<Parameters, bool>` overload) refuses
its route: the URL is put back to the previous one.

The guard's `isBack` argument is true only for a `popstate` (the browser's Back
and Forward, or a hash change) that returns to the path the previous navigation
left. A programmatic `Push`/`Replace`, or a write to the query, is never "back".
`Router.Navigate(path)` to the address already shown does nothing; with
`reload: true` it re-runs the handler without adding a history entry.

`OnBeforeNavigate` sees every navigation the router performs: `Router.Navigate`,
`ForceMatchCurrent`, a hash change from a link or the address bar (and so the
browser's back/forward buttons), and `Push`/`Replace` — those two skip route
matching but still ask the guard, and report the refusal by returning `false` so
a caller that renders the new view itself can stop too:

```csharp
if (!Router.Push($"#/view/{item.Name}")) return; // guard said no
currentPage.Value = item;
```

Leaving the page altogether (tab close, reload, a link to another site) never
reaches the router — that needs a `beforeunload` listener, which is what
`UnsavedChangesGuard` installs (see `unsaved-changes-guard.md`).

## Example

```csharp
using static Tesserae.UI;

private static readonly Stack Content = Stack();

private static void Main()
{
    Router.Register("home", _ => Show(HomePage()));
    Router.Register("view/:id", p => Show(DocumentPage(p["id"])));
    Router.Register("search", p => Show(SearchPage(p.ContainsKey("term") ? p["term"] : "")));

    Router.Initialize();
    Router.Refresh(onDone: Router.ForceMatchCurrent);

    var nav = HStack().Children(
        Button("Home").OnClick((s, e) => { Router.Push("#/home"); Router.ForceMatchCurrent(); }),
        Button("Doc 42").OnClick((s, e) => { Router.Push("#/view/42"); Router.ForceMatchCurrent(); })
    );
    MountToBody(Stack().Children(nav, Content));
}

private static void Show(IComponent page) { Content.Clear(); Content.Add(page); }
```

## Notes

- Render routes into a long-lived container (clear + add), or bind an
  `Observable` and render it with `DeferSync` for reactive updates.
- The History API is unavailable inside the sandboxed docs preview iframe — real
  pages use `Router.Push`/`Replace` directly.
- `tps.json` reflection must stay enabled for the router to work.

## Related

- RouteState / RouteLocation (the URL as app state: reads, matching, writes, one-shot keys) — `route-state.md`
- Core Concepts (observables, Defer) — `core-concepts.md`
- UnsavedChangesGuard (blocks navigation while an editor is dirty, via `OnBeforeNavigate`) — `unsaved-changes-guard.md`
- Full docs & API: `/tesserae/get-started/routing`
