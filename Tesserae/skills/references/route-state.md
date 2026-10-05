---
name: route-state
description: Read and write the URL as app state - which path the hash is on, which query keys it holds, and what a write does to the browser history. Use when highlighting a nav item from the URL, keeping a tab, filter or open panel in the hash query string, reading a one-shot parameter once, or testing code that parses a hash, in a Tesserae (C#/Transpose) app.
---

# RoutePath / RouteQuery

The hash of the address bar (`#/spaces/app?uid=U&preview=a,b`) is two things, and they are read separately:

- **`RoutePath`** is *where you are*: `#/spaces/app`. It never looks at the query.
- **`RouteQuery`** is *state kept in the query string*: `uid=U&preview=a,b`. It never looks at the path.

Both read the address bar as the router does, so they work before the first route has matched. Use
`Router.Navigate`/`Push` to go somewhere (see `routing.md`); use `RoutePath` to ask where you are and
`RouteQuery` to keep view state.

## RoutePath

| Member | Meaning |
|---|---|
| `Current` | the path as `#/ab/c/de/ef`: no query, no trailing `/`, `#/` when there is none. Case is kept. |
| `IsExactly(route)` | the path is exactly `route`. `#/ab/c/de/ef` is exactly `#/ab/c/de/ef` and nothing else. |
| `IsDescendantOf(route)` | the path is below `route` at any depth, and **not** `route` itself. `#/ab/c/de/ef` is a descendant of `#/ab/c`; `#/ab/c` and `#/ab/cx` are not. |
| `ReplacePath(route)` | see Writing below |

Paths are compared by segment, ignoring case, empty segments and a trailing `/`, the way the router matches a
route. A leading `#` or `/` on a route you pass in does not matter: `#/ab/c`, `/ab/c` and `ab/c` are the same.
`IsDescendantOf("#/")` is true for every path except the root.

```csharp
// A leaf nav item is selected on its own page, whatever the query holds (an open ?preview=...)
var mailSelected = RoutePath.IsExactly("#/inboxes");

// A section stays selected on its page and everywhere below it
var manageSelected = RoutePath.IsExactly("#/manage") || RoutePath.IsDescendantOf("#/manage");

// A page with a child that has its own place: Calendar is selected unless a connected calendar (?uid=) owns the page.
// The path and the query are read separately, so the app says how they combine.
var calendarSelected = RoutePath.IsExactly("#/calendar") && !RouteQuery.TryGet("uid", out _);

var where = RoutePath.Current;   // "#/ab/c/de/ef"
```

## RouteQuery

| Member | Meaning |
|---|---|
| `Get(key)` | the value, `""` for `?x` and `?x=`, `null` when the key is absent |
| `TryGet(key, out value)` | true for any key that is present, whatever its value |

Keys and values are case-sensitive. A value is split on the **first** `=` only (`?a=b=c` gives `b=c`) and `+`
is not turned into a space. A malformed percent escape (`?a=%`) never throws; the value is kept as written.

## Writing

Every query write goes through `Router.ReplaceQueryParameters`: only the query segment changes, every other key
stays, **no route handler runs**, `Router.OnNavigated` does not fire and `OnBeforeNavigate` is not asked.
A write that changes nothing does nothing. Before the first route has matched there is nothing to anchor
to and a write does nothing (a read still works). Pass keys and values unencoded; they are URI-encoded once on write.

| Member | Effect |
|---|---|
| `RouteQuery.Set(key, value)` | one key, replaces the history entry |
| `RouteQuery.Clear(key)` | removes one key, replaces the history entry |
| `RouteQuery.Update(p => ...)` | several keys in one write |
| `RouteQuery.Consume(key, out value)` | reads a key and removes it; for something that is asked for once (a toast, a dialog) so a refresh or a shared link does not ask again |
| `RouteQuery.Consume(params keys)` | the same for keys that belong together, removed in one write; true when any was there |
| `RouteQuery.SetWithHistory(key, value)` | for a key that picks a tab or a filter: the first time it is written it replaces the entry, and once it has a value every change adds an entry, so Back returns to the previous choice |
| `RouteQuery.UpdateWithHistory(update, params pushWhenPresent)` | one write that adds an entry only when every key in `pushWhenPresent` already has a value, and replaces the entry otherwise; several keys changed together are one step back |
| `RoutePath.ReplacePath(route)` | moves the address bar to `route`, keeping the query, in place of the current entry; the view stays, no handler runs; true without writing when the path is already `route`; false when a guard refused |

```csharp
// A tab strip: arriving on a tab replaces the entry, switching adds one
void OnTabChosen(string tab) => RouteQuery.SetWithHistory("show", tab);

// An open panel
RouteQuery.Set("preview", id);
RouteQuery.Clear("preview");

// A toast the server asked for in the link, shown once
if (RouteQuery.Consume("toast", out var toast)) ShowToast(toast);

// A page that finds its canonical address once it knows what it shows
RoutePath.ReplacePath("#/node/" + node.Id);
```

`ReplacePath` is the one write that changes the path. It is `Router.Replace` underneath, so it asks the
guard and, like `Push` and `Replace`, moves the router's previous-state record that back-detection reads.
The query writes deliberately leave that record alone.

## Testing code that parses a hash

`RouteLocation` is a hash read as a plain value: built from any string, it does not look at the browser or the
router. It has `Path`, `IsExactly`, `IsDescendantOf`, `Get` and `TryGet`, the same reads as above, and the
query is only parsed when a key is asked for.

```csharp
var location = new RouteLocation("#/preferences?id=user&preview=x");
location.IsExactly("#/preferences");   // true
location.Get("id");                    // "user"
```

## Rules worth knowing

- **State goes through `RouteQuery`, navigation through `Router`.** `Router.Push`/`Replace` set the whole hash and
  drop every key you did not repeat; `RouteQuery.Set` keeps the others.
- **Route `:variables` are not query keys.** On `#/node/:uid`, `Set("k", "v")` writes `#/node/abc?k=v`, not
  `?uid=abc&k=v`. The handler's `Parameters` still carries `uid`, and `RouteQuery.Get("uid")` is `null`.
- **A handler and `Router.GetQueryParameters()` get a copy.** Removing a key from it does not change the URL.
- **A key that two parts of the app both use is a clash.** The query is one flat collection; give each part its own key names.
- **Read before the router is up with `RoutePath` and `RouteQuery`** (they apply the `OnTransformRoutes` transform, so they
  see what the router will match). Do not write before the first match.

## Related

- Routing (`Router`, guards, `OnNavigated`) - `routing.md`
- UnsavedChangesGuard (blocks navigation while an editor is dirty) - `unsaved-changes-guard.md`
- Pivot (tabs whose selection belongs in the URL) - `pivot.md`
- The gallery's "Route State" sample runs about 150 cases against these members and checks itself.
