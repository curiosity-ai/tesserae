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

## Navigation guards and events

- `Router.OnBeforeNavigate(...)` lets you block navigation (return `false` to cancel).
- `Router.OnNavigated(...)` lets you respond after navigation completes.
- `Router.OnNotMatched(...)` is invoked when no route matches the new URL.

## Compile-time route checking (TSS0001)

The Tesserae NuGet package ships a Roslyn analyzer (`Tesserae.Analyzers`, rule `TSS0001`) that warns when a compile-time constant path passed to `Router.Navigate` does not match any route registered with `Router.Register` in the same project. Matching mirrors the runtime rules: leading `#`/`/` are ignored, comparison is per-segment and case-insensitive, `:name` segments match any value, and query strings are ignored.

### Where the analyzer gets the route table

A Roslyn analyzer only sees the *current* compilation, so it collects known routes from two places:

1. **Constant `Router.Register` paths in this project.** As before.
2. **A route manifest** — an `AdditionalFiles` entry named `TesseraeRoutes.txt`. This lets a project
   validate `Router.Navigate` calls against routes that are registered in a *different* assembly
   (the common case where one project owns `Router.Register` and others only navigate). The file is
   one route pattern per line; blank lines and lines starting with `;` or `//` are ignored (a leading
   `#` is a real hash route, so it is *not* a comment):

   ```
   ; app routes (generated from DefaultRoutes)
   #/home
   #/space/:uid
   #/manage/users
   ```

   Wire it up in the projects that navigate:

   ```xml
   <ItemGroup>
     <AdditionalFiles Include="TesseraeRoutes.txt" />
   </ItemGroup>
   ```

### Dynamic registrations no longer mute the whole project

A single `Router.Register` call built at runtime used to disable `TSS0001` for the entire project.
Now the analyzer is prefix-aware:

- A dynamic registration with a knowable constant prefix (e.g. `Register("#/admin/" + suffix, …)`)
  only suppresses `Router.Navigate` paths that fall *under* that prefix. Navigations elsewhere are
  still checked.
- A fully-opaque dynamic registration (no knowable prefix, e.g. `Register(pathVariable, …)`) falls
  back to the old conservative behavior and suppresses otherwise-unmatched navigations — declare
  those routes in the manifest (above) so they become known, or opt in below.

### Opting in to strict checking

If you know your constant/manifest route table is complete, set this in `.editorconfig` (or a
`.globalconfig`) to report mismatches even in the presence of dynamic registrations:

```ini
dotnet_diagnostic.TSS0001.route_table_is_authoritative = true
```

### When the analyzer still stays silent

To avoid false positives (a false "unregistered route" warning is worse than silence), the check does
nothing when it knows *no* routes at all — no constant `Router.Register` in the project **and** no
manifest. `Router.Navigate` calls with non-constant paths, and navigations to absolute/external URLs,
are never checked.

## Recommendations

- Keep route strings normalized. Do not  register routes with a leading `/` or `#/`.
- Register routes first, then call `Refresh` and `ForceMatchCurrent` to ensure the initial URL is matched without changing it.
- Register routes early in the application initialization.
- Prefer `Push`/`Replace` for navigation to preserve SPA behavior and a clean history stack.
