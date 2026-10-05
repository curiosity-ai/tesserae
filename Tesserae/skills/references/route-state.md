---
name: route-state
description: Read and write the URL as app state - which page the hash is on, which query keys it holds, and what a write does to the browser history. Use when highlighting a nav item from the URL, keeping a tab, filter or open panel in the hash query string, reading a one-shot parameter once, or testing code that parses a hash, in a Tesserae (C#/Transpose) app.
---

# RouteState / RouteLocation

`RouteState` is the one place to read and write the **hash query string** of the current page
(`#/spaces/app?uid=U&preview=a,b`). `RouteLocation` is the reading half as a plain value: it is built
from a hash string and does not look at the browser or the router, so it works before the first route
has matched and in a test. `RouteState.Current` is the `RouteLocation` of the address bar; `RouteState.Path`,
`Get` and `Has` are shortcuts for it.

Both sit on top of `Router` (see `routing.md`). Use `Router.Navigate`/`Push` to go somewhere; use
`RouteState` to describe where you already are.

## Reading

All of these are members of `RouteLocation`. Read them for the address bar through `RouteState.Current`
(`RouteState.Path`, `RouteState.Get` and `RouteState.Has` also exist as statics):

- `Path` - `#/spaces/app`: no query, no trailing `/`, `#/` when there is no path. Case is kept.
- `Query` - a copy of the query keys as `Parameters`. A route `:variable` is part of the path, not of `Query`.
- `RouteLocation.PathOf(hashOrRoute)` - the same normalised path for any hash or route string.
- `IsOn(route, params childQueryKeys)` - the path is exactly `route`. A query does not matter, except the
  keys in `childQueryKeys`: while one is present (with any value, even empty) the page belongs to that
  child, so `IsOn` is `false`.
- `IsUnder(route)` - the path is `route` or below it, on a segment boundary (`#/a/b` is under `#/a`, `#/ab` is not).
  Every path is under `#/`.
- `Matches(url, params childQueryKeys)` - the path equals the one in `url` and every `key=value` pair in `url`
  is here with an equal decoded value. Extra keys and key order are ignored.
- `Deepest(params routes)` - of the routes the path is under, the one with the most segments, or `null`.
  Routes naming the same path give the same answer.
- `Mentions(text)` - `text` appears anywhere in the hash (path, route variable, key or value), compared
  exactly, case included. For "is this node open here in any role".
- `Has(key)` - the key has a value that is not empty or whitespace. `Get(key)` is `null` when the key is absent
  and `""` for `?x` and `?x=`, so `Get(key) != null` is true for any key that is present.

Matching of paths is by segment and ignores case, empty segments and a trailing `/`, the way the router
matches a route. Query keys and values are case-sensitive. A value is split on the **first** `=` only
(`?a=b=c` gives `b=c`) and is not turned from `+` into a space. A malformed percent escape (`?a=%`) never
throws; the value is kept as written.

```csharp
// A nav item is selected on its page whatever the query holds (an open ?preview=...)
var mailSelected = RouteState.Current.IsOn("#/inboxes");

// A page with children: Calendar is selected unless a connected calendar (?uid=) owns the page
var calendarSelected = RouteState.Current.IsOn("#/calendar", "uid");

// A list of routes where the most specific wins
var item = RouteState.Current.Deepest("#/manage/shell", "#/manage/shell/curio");

// Not tied to the browser: any string, so it can be tested
var location = new RouteLocation("#/preferences?id=user&preview=x");
location.Matches("#/preferences?id=user");   // true
location.Matches("#/preferences?id=use");    // false
```

## Writing

Every write goes through `Router.ReplaceQueryParameters`: only the query segment changes, every other key
stays, **no route handler runs**, `Router.OnNavigated` does not fire and `OnBeforeNavigate` is not asked.
A write that changes nothing does nothing. Before the first route has matched there is nothing to anchor
to and a write does nothing (a read still works).

| Member | Effect |
|---|---|
| `Set(key, value)` / `Clear(key)` | one key, replaces the history entry |
| `Update(p => ...)` | several keys in one write |
| `Consume(key, out value)` | reads a key and removes it; for something that is asked for once (a toast, a dialog) so a refresh or a shared link does not ask again |
| `Consume(params keys)` | the same for keys that belong together, removed in one write; true when any was there |
| `SetWithHistory(key, value)` | for a key that picks a tab or a filter: the first time it is written it replaces the entry, and once it has a value every change adds an entry, so Back returns to the previous choice |
| `UpdateWithHistory(update, params pushWhenPresent)` | one write that adds an entry only when every key in `pushWhenPresent` already has a value, and replaces the entry otherwise; several keys changed together are one step back |
| `ReplacePath(route)` | moves the address bar to `route`, keeping the query, in place of the current entry; the view stays, no handler runs; false when a guard refused |

```csharp
// A tab strip: arriving on a tab replaces the entry, switching adds one
void OnTabChosen(string tab) => RouteState.SetWithHistory("show", tab);

// An open panel
RouteState.Set("preview", id);
RouteState.Clear("preview");

// A toast the server asked for in the link, shown once
if (RouteState.Consume("toast", out var toast)) ShowToast(toast);
```

Keys and values are URI-encoded once on write and decoded on read, so pass them unencoded.

## Rules worth knowing

- **State goes through `RouteState`, navigation through `Router`.** `Router.Push`/`Replace` set the whole hash and
  drop every key you did not repeat; `RouteState.Set` keeps the others.
- **Route `:variables` are not query keys.** On `#/node/:uid`, `Set("k", "v")` writes `#/node/abc?k=v`, not
  `?uid=abc&k=v`. The handler's `Parameters` still carries `uid`.
- **A handler and `Router.GetQueryParameters()` get a copy.** Removing a key from it does not change the URL.
- **A key that two parts of the app both use is a clash.** The query is one flat collection; give each part its own key names.
- **Read before the router is up with `RouteState.Current`** (it applies the `OnTransformRoutes` transform, so it
  sees what the router will match). Do not write before the first match.

## Related

- Routing (`Router`, guards, `OnNavigated`) - `routing.md`
- UnsavedChangesGuard (blocks navigation while an editor is dirty) - `unsaved-changes-guard.md`
- Pivot (tabs whose selection belongs in the URL) - `pivot.md`
- The gallery's "Route State" sample runs about 200 cases against these members and checks itself.
