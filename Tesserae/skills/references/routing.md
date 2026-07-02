---
name: routing
description: The built-in lightweight hash-based Router for SPA-style navigation, route parameters, and guards. Use when setting up routing/navigation between views in a Tesserae (C#/h5) app.
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
- `h5.json` reflection must stay enabled for the router to work.
- The Tesserae package ships a Roslyn analyzer (`TSS0001`) that warns when a
  constant path passed to `Router.Navigate` matches no `Router.Register` route
  in the project (skipped when any registration path is non-constant, or when
  the project registers no routes itself).

## Related

- Core Concepts (observables, Defer) — `.core-concepts.md`
- Full docs & API: `/tesserae/get-started/routing`
