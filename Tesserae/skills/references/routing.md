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

- `Router.Push(path)` — push a new history entry.
- `Router.Replace(path)` — update the URL in place.
- `Router.Navigate(path, reload: true)` / `Router.ForceMatchCurrent()` — re-run
  the matcher and re-activate the matching route.

After a programmatic `Push`/`Replace`, either call `ForceMatchCurrent()` or
update your view directly in the click handler — pick one and stay consistent.

Parameters: path `:segments` are captured positionally; query-string pairs
(`?term=x&page=2`) land in the same `Parameters` collection — just read the keys.
Avoid reusing a `:segment` name as a query key (one shared collection).

Reflect view state (open panel, selected tab, filters) in the URL's query
segment so it survives refresh and can be shared as a deep link:

- `Router.GetQueryParameters()` — the current `Parameters`.
- `Router.ReplaceQueryParameters(p => p.With("preview", id))` — clone, update,
  rewrite only the hash's query segment; no-ops when nothing changed. Use
  `.Remove(key)` to clear. The route handler is **not** re-invoked — the URL
  updates silently under the running view. Default is `replaceState` (no
  history entry); pass `pushToHistory: true` for one.
- On the next navigation or page load the keys arrive in the handler's
  `Parameters` — restore the state from there.
- `Push`/`Replace` re-derive the current `Parameters` from the path you pass,
  so `GetQueryParameters()` stays in sync even without a route re-match.

Guards / events: `Router.OnBeforeNavigate(...)` (return `false` to cancel),
`Router.OnNavigated(...)`, `Router.OnNotMatched(...)`.

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

- Core Concepts (observables, Defer) — `.core-concepts.md`
- Full docs & API: `/tesserae/get-started/routing`
