# Routing in Tesserae

This guide covers the built-in `Router` helper for SPA-style navigation, and `RoutePath` and `RouteQuery`, which read and write the URL as app state. The router is intentionally lightweight: it listens for URL changes, matches routes against registered patterns, and calls your handlers with parsed parameters.

## Key concepts

- **Hash-based paths**: Routing reads from `window.location.hash` (e.g. `#/view/details`).
- **Route registration**: Map a unique identifier and a path to a callback with `Router.Register`.
- **Initialization**: Call `Router.Initialize()` once, then `Router.Refresh(...)` after registering routes to build the match list.
- **Matching**: by segment, ignoring case and empty segments. A segment that starts with `:` captures what is in that position.

## Basic setup

```csharp
Router.Register("home", _ => LoadHome());
Router.Register("view/:id", parameters => LoadDetail(parameters["id"]));

Router.Initialize();
Router.Refresh(onDone: Router.ForceMatchCurrent);
```

- A leading `#` or `/` on the path is normalised, so `"view/:id"` and `"#/view/:id"` are the same route.
- Only the first registration of an identifier is kept, unless you pass `replace: true`.
- `ForceMatchCurrent()` re-evaluates the current hash after routes are registered, without forcing a URL change.

The sample app uses this same pattern to bind routes to samples and initialize routing on app start.

## Navigation APIs

- `Router.Navigate(path, reload: false)` goes to `path` as a new history entry and runs the matching route's handler. It does nothing when the browser is already at `path`, unless `reload` is `true`, which re-runs the handler without adding an entry.
- `Router.Push(path)` pushes a new history entry and updates the URL without re-matching routes.
- `Router.Replace(path)` updates the URL in place without adding a history entry or re-matching routes.

`Push` and `Replace` return `false` when an `OnBeforeNavigate` guard refused; the URL is then left alone and the caller must not show the new view either.

```csharp
if (!Router.Push("#/view/Details")) return;

// or, to replace without a new history entry:
Router.Replace("#/view/Details");
```

After a `Push`/`Replace`, either call `ForceMatchCurrent()` or update your view directly in the click handler, and stay consistent about which.

## Route parameters and query strings

A handler receives a `Parameters` collection holding both the route's `:variables` and the keys of the hash's query string (`?key=value`). If a query key has the name of a `:variable`, the query value wins.

```csharp
Router.Register("search", "/search", parameters =>
{
    var term = parameters.TryGetValue("term", out var t) ? t : "";

    ShowSearchResults(term);
    return true;
});
```

Navigate with a query string:

```
#/search?term=computer&page=2
```

- Keys and values are URI-encoded when written and decoded when parsed. A value is split on the first `=` only, `+` stays a plus, and a pair without a value (`?flag`) parses as an empty string. A malformed escape (`?q=%`) is kept as written instead of throwing.
- `:variable` values are the raw hash segment and are not URI-decoded.
- The handler gets a copy of the parameters. Changing it does not change the URL.
- Returning `false` from the handler (the `Func<Parameters, bool>` overload) refuses the route and puts the URL back to the previous one.

## Reflecting view state in the query string

Views can round-trip their state (open panels, selected tabs, filters) through the URL and get shareable, refresh-safe deep links. Use `RouteQuery` for this (see below); it is built on these `Router` members:

- `Router.GetQueryParameters()` returns a copy of the current `Parameters` (path captures plus query keys).
- `Router.SetQueryParameters(parameters, pushToHistory: false)` rewrites only the query segment of the hash, leaving the route path and everything before the `#` untouched.
- `Router.ReplaceQueryParameters(update, pushToHistory: false)` clones the current parameters, applies your update, and does nothing when nothing changed.

```csharp
// Reflect an opened detail panel in the URL:
Router.ReplaceQueryParameters(p => p.With("preview", id));

// And remove it again when the panel closes:
Router.ReplaceQueryParameters(p => p.Remove("preview"));
```

Both update the URL **silently**: the registered route handler is not re-invoked, `OnNavigated` does not fire and the guard is not asked, so the view that wrote the state keeps running undisturbed. With the default `pushToHistory: false` the URL is rewritten in place (`replaceState`, no history entry); `pushToHistory: true` adds a history entry instead. On the next full navigation or page load the keys come back through the handler's `Parameters`, which is where the view should restore the state from. Before the first route has matched there is no URL to anchor to, and both do nothing.

A route's `:variables` are not written into the query: on `#/node/:uid`, a write gives `#/node/abc?k=v`, not `#/node/abc?uid=abc&k=v`.

`Push` and `Replace` re-derive the router's current parameters from the path you pass them, so `GetQueryParameters()` stays in sync with the URL even when no route re-match happens. They carry no route `:variables`.

## RoutePath and RouteQuery

See `Tesserae/skills/references/route-state.md` for asking where the page is (`RoutePath.Current`, `IsExactly`, `IsDescendantOf`) and for reading and writing view state in the query (`RouteQuery.Get`, `Set`, `Clear`, `Consume`, `SetWithHistory`, ...).

## Navigation guards and events

- `Router.OnBeforeNavigate(...)` lets you block navigation (return `false` to cancel). There is one handler; the last one registered wins. It sees `Navigate`, `ForceMatchCurrent`, hash changes (and so the browser's Back and Forward) and `Push`/`Replace`. Its `isBack` argument is true only for a `popstate` that returns to the path the previous navigation left.
- `Router.OnNavigated(...)` lets you respond after navigation completes. It fires after the handler's synchronous part, so before an `async` handler has finished.
- `Router.OnNotMatched(...)` is invoked when no route matches the new URL. The router's state still follows the address bar: `GetQueryParameters()` holds that URL's query, and a `Navigate` back to the page the user came from is not mistaken for "already there".
- `Router.OnTransformRoutes(...)` rewrites the hash before it is matched.
- `Router.OnWíllNavigate(...)` can veto `Router.Navigate` before anything happens.

## Recommendations

- Register routes early in the application initialization, then call `Refresh` and `ForceMatchCurrent` so the initial URL is matched without being changed.
- Use `Navigate` to go somewhere, `RoutePath` to ask where you are and `RouteQuery` to keep state in the URL. `Push`/`Replace` set the whole hash, so a key you do not repeat is gone.
- Keep a key's name to one meaning: query keys and `:variable` names share one collection.
