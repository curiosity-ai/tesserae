---
name: wrap-a-javascript-library
description: How to bundle an existing JavaScript library into Tesserae and wrap it as an IComponent (tps.json resources + Script.Write against the global). Use when adding a feature backed by a third-party JS library (charting, layout, editors, etc.).
---

# Wrapping an existing JavaScript library

Tesserae ships several components that are thin C# wrappers over JS libraries —
`Masonry` (masonry-layout), tooltips (tippy/popper), `CodeDiff` (diff2html),
`MarkdownBlock` (marked + purify), `SortableStack` (sortable), `Diagram`
(baklava). The pattern is always the same: **bundle the script, then drive its
global from C# through `Script.Write`.**

## 1. Bundle the library

Put the minified library under `Tesserae/tps/assets/js/` and add it to the
resource bundles in `Tesserae/tps.json`. It must appear in **both** the
`tss-dep.js` and `tss-dep.min.js` bundles (keep the two file lists in sync —
Transpose swaps between them for Debug vs Release builds):

```jsonc
{
  "name": "tss-dep.js",
  "files": [
    "tps/assets/js/popper.min.js",
    "tps/assets/js/tippy.min.js",
    "tps/assets/js/masonry.min.js",
    "tps/assets/js/yourlib.min.js"     // <-- add here
  ],
  "output": "assets/js"
}
// …and the identical addition in the "tss-dep.min.js" bundle
```

Any CSS the library needs goes into the `tss.css` bundle the same way. Once
bundled, the library's global (e.g. `Masonry`, `tippy`, `Diff2HtmlUI`) is
available at runtime.

## 2. Wrap it in an `IComponent`

Build a host element, instantiate the library against it, and keep the JS
instance in an `object` field so you can call back into it. `Masonry.cs` is the
canonical example:

```csharp
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

[Transpose.Name("tss.Masonry")]
public class Masonry : IContainer<Masonry, IComponent>, ISpecialCaseStyling
{
    private readonly HTMLElement _host;
    private readonly object      _instance;   // the JS object
    private double               _timeout;

    public Masonry(int columns, int gutter = 10)
    {
        _host     = Div(_("tss-masonry"));
        _instance = Script.Write<object>(
            "new Masonry({0}, { itemSelector: '.tss-masonry-item', gutter: {1}, percentPosition: true })",
            _host, gutter);

        // libraries that measure the DOM must (re)run once the element is on-screen
        DomObserver.WhenMounted(_host, () => Layout());
    }

    public void Add(IComponent component)
    {
        var el = GetItem(component, true);
        _host.appendChild(el);
        Script.Write("{0}.appended({1})", _instance, el);   // call a JS method
        Layout();
    }

    private void Layout()
    {
        if (!_host.IsMounted()) return;
        window.clearTimeout(_timeout);                       // debounce relayout
        _timeout = window.setTimeout((_) => Script.Write("{0}.layout()", _instance), 16);
    }

    public HTMLElement Render() => _host;
}
```

## 3. Lifecycle: the mount callbacks fire once

`Masonry` above gets away with a single `WhenMounted` because it holds no state the
user can lose. A wrapper whose instance owns *content* — an editor, a chart with a
selection, anything the person on the page can change — has to handle removal and
re-addition, and the two observers make that a trap with no error message:
`DomObserver.WhenMounted` and `DomObserver.WhenRemoved` each fire **once** and then
forget the element. If teardown does not re-arm the mount observer, a component
removed from the DOM and re-added renders an **empty container ever after**, silently.
Tearing down without re-arming leaks the wrapped instance; re-arming without tearing
down leaks harder. Do both, and keep `Dispose()` as the one-way door:

```csharp
public HTMLElement Render()
{
    if (!_mountRequested)
    {
        _mountRequested = true;
        DomObserver.WhenMounted(_host, OnMounted);
    }
    return _host;
}

private void OnMounted()
{
    if (_instance is object) return;   // a second mount signal must not create a second instance

    _instance = CreateInstance(_host);
    Replay();                          // options, subscriptions and captured state
    DomObserver.WhenRemoved(_host, OnRemoved);
}

private void OnRemoved()
{
    if (_disposed) return;

    Capture();                         // read back text / scroll / selection before it is gone
    Teardown();                        // dispose the JS instance, release every subscription
    _instance = null;
    DomObserver.WhenMounted(_host, OnMounted);   // re-arm: a re-added host rebuilds
}

public void Dispose()                  // the deliberate, final release
{
    if (_disposed) return;

    _disposed = true;
    Teardown();
}
```

Three things this has to get right:

- **Guard the create.** A second mount signal for an instance that already exists
  produces two of them, and the first one leaks with the DOM it drew.
- **Capture what the user can change.** Text, scroll offset, caret, selection: read
  them during teardown and restore them after the next create, or a remount silently
  reverts the user's work.
- **Replay standing configuration, drop transient acts.** Options, event subscriptions
  and content configured before the first mount (or before a remount) have to be
  replayed after every create. Focus, reveal and scroll-to calls made while there is
  no instance should be **dropped** — replaying them later is a bug, not a nicety.

Tesserae ships no `WhenMountedOrRemoved` helper on purpose — there is a commented-out
one in `IComponentExtensions.cs` with the reasoning — so this loop is yours to own.

## 4. Handing theme colors to the library

Tesserae's theme values are CSS variables, so a library that wants a concrete
`#rrggbb` needs one resolved. `Color.FromString` resolves a `var(...)` token itself:

```csharp
var background = Color.FromString(Theme.Secondary.Background).ToHex();
var isDark     = Theme.IsDark;
```

Resolve when you create or refresh the instance rather than baking the value into a
static field at load time, and rebuild on `Theme.OnThemeChanged` — a host can switch
theme at runtime and whatever you handed the library will not follow. Subscribe where
you create the instance and unsubscribe in the teardown above, so a removed component
stops rebuilding themes:

```csharp
private void OnMounted()
{
    // …create the instance…
    ApplyTheme();
    Theme.OnThemeChanged += ApplyTheme;
}

private void Teardown()
{
    Theme.OnThemeChanged -= ApplyTheme;
    // …dispose the instance…
}
```

`Theme.OnThemeChanged` is a static event: a subscription you never remove keeps the
component (and the DOM it captured) alive for the life of the page.

## Key points

- **Instantiate against a real element.** Pass your host element as `{0}`.
- **Defer DOM-measuring calls to mount.** Use `DomObserver.WhenMounted(el, …)`
  (and `DomObserver.WhenRemoved` to tear down) — the element has no size until
  it is in the document. Both fire once each; see the lifecycle section above.
- **Debounce expensive relayouts** with `window.setTimeout`/`clearTimeout`.
- **Hold the instance as `object`** and reach its methods/properties via
  `Script.Write("{0}.method({1})", _instance, arg)`.
- For a *typed* surface over the library instead of inline strings, declare an
  `[External]` / `[Transpose.Name]` binding — see `javascript-interop`.
- Add a `UI.Components.cs` factory and a sample, like any other component.

## Related

- `javascript-interop` — the `Script.Write` / `[External]` mechanics in detail.
- `creating-a-component` — the `IComponent` shell you are filling in.
- `masonry`, `tippy`, `charts` — existing wrappers to copy from.
- `colors`, `theme-colors` — resolving a theme token to a concrete color, and
  `Theme.OnThemeChanged`.
- `icomponent` — the mount/removal extensions (`.WhenMounted`, `.WhenRemoved`).
- Docs: `/tesserae/extending/wrapping-a-javascript-library`
