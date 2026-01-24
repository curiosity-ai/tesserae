# Routing in Tesserae

This guide covers the built-in `Router` helper for SPA-style navigation. The router is intentionally lightweight: it listens for URL changes, matches routes against registered patterns, and calls your handlers with parsed parameters.

## Key concepts

- **Hash-based paths**: Routing reads from `window.location.hash` (e.g. `#/view/details`).
- **Route registration**: Map a unique identifier and a path to a callback with `Router.Register`.
- **Initialization**: Call `Router.Initialize()` once, then `Router.Refresh(...)` after registering routes to build the match list.

## Basic setup

```csharp
Router.Register("home", "/", _ => LoadHome());
Router.Register("#/view/:id", parameters => LoadDetail(parameters["id"]));

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

Route parameters are declared using `:name` tokens. During matching, values are stored in the `Parameters` object and passed to your handler.

Query strings are parsed into the same parameter collection when a `?key=value` portion is present in the hash. (Parsing is done manually today, so URL encoding is decoded via `Script.DecodeURIComponent`.)

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

## Navigation guards and events

- `Router.OnBeforeNavigate(...)` lets you block navigation (return `false` to cancel).
- `Router.OnNavigated(...)` lets you respond after navigation completes.
- `Router.OnNotMatched(...)` is invoked when no route matches the new URL.

## Recommendations

- Keep route strings normalized and include a leading `/` or `#/` consistently.
- Register routes first, then call `Refresh` and `ForceMatchCurrent` to ensure the initial URL is matched without changing it.
- Register routes early in the application initialization.
- Prefer `Push`/`Replace` for navigation to preserve SPA behavior and a clean history stack.
