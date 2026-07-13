# Routing in Tesserae

This guide covers the built-in `Router` helper for SPA-style navigation. The router is intentionally lightweight: it listens for URL changes, matches routes against registered patterns, and calls your handlers with parsed parameters.

## Key concepts

- **Hash-based paths**: Routing reads from `window.location.hash` (e.g. `#/view/details`).
- **Route registration**: Map a unique identifier and a path to a callback with `Router.Register`.
- **Initialization**: Call `Router.Initialize()` once, then `Router.Refresh(...)` after registering routes to build the match list.

## Basic setup

```csharp
Router.Register("home" _ => LoadHome());
Router.Register("view", parameters => LoadDetail(parameters["id"]));

Router.Initialize();
Router.Refresh(onDone: Router.ForceMatchCurrent);
```

- `ForceMatchCurrent()` re-evaluates the current hash after routes are registered, without forcing a URL change.

The sample app uses this same pattern to bind routes to samples and initialize routing on app start.

## Navigation APIs

- `Router.Push(path)` pushes a new history entry and updates the URL without forcing a reload.
- `Router.Replace(path)` updates the URL without adding history entries.
- `Router.Navigate(path, reload: false)` forces a navigation check and can reload the view if `reload` is `true`.

Example usage:

```csharp
Router.Push("#/view/Details");
// or, to replace without a new history entry:
Router.Replace("#/view/Details");
```

## Route parameters and query strings

Route parameters are parsed from query strings appened to the current hash. You don't need to declare parameters in the route. During matching, values are stored in the `Parameters` dictionary and passed to your handler.

Query strings are parsed into the parameter collection when a `?key=value` portion is present in the hash.

Example:

```csharp
Router.Register("search", "/search", parameters =>
{
    var term = parameters["term"];
    var page = parameters["page"];
    ShowSearchResults(term, page);
    return true;
});
```

Navigate with a query string:

```
#/search?term=computer&page=2
```

## Reflecting view state in the query string

The router owns the hash's query segment, so views can round-trip their state (open panels, selected tabs, filters) through the URL and get shareable / refresh-safe deep links:

- `Router.GetQueryParameters()` returns the current `Parameters` (path captures plus query keys).
- `Router.SetQueryParameters(parameters, pushToHistory: false)` rewrites only the query segment of the hash, leaving the route path and everything before the `#` untouched.
- `Router.ReplaceQueryParameters(update, pushToHistory: false)` is the preferred entry point: it clones the current parameters, applies your update, and no-ops when nothing changed.

```csharp
// Reflect an opened detail panel in the URL:
Router.ReplaceQueryParameters(p => p.With("preview", id));

// And remove it again when the panel closes:
Router.ReplaceQueryParameters(p => p.Remove("preview"));
```

Both methods update the URL **silently**: the registered route handler is not re-invoked, so the view that wrote the state keeps running undisturbed. With the default `pushToHistory: false` the URL is rewritten in place (`replaceState`, no history entry); `pushToHistory: true` adds a history entry instead. On the next full navigation or page load the keys come back through the handler's `Parameters`, which is where the view should restore the state from.

Keys and values are URI-encoded when written and decoded when parsed. A query pair without a value (`?flag`) parses as an empty string. Note that query keys and `:segment` capture names share one `Parameters` collection, so avoid reusing a route variable name as a query key.

`Push` and `Replace` also re-derive the router's current parameters from the path you pass them, so `GetQueryParameters()` stays in sync with the URL even when no route re-match happens.

## Navigation guards and events

- `Router.OnBeforeNavigate(...)` lets you block navigation (return `false` to cancel).
- `Router.OnNavigated(...)` lets you respond after navigation completes.
- `Router.OnNotMatched(...)` is invoked when no route matches the new URL.

## Recommendations

- Keep route strings normalized. Do not  register routes with a leading `/` or `#/`.
- Register routes first, then call `Refresh` and `ForceMatchCurrent` to ensure the initial URL is matched without changing it.
- Register routes early in the application initialization.
- Prefer `Push`/`Replace` for navigation to preserve SPA behavior and a clean history stack.
