---
name: wrap-a-javascript-library
description: How to bundle an existing JavaScript library into Tesserae and wrap it as an IComponent (h5.json resources + Script.Write against the global). Use when adding a feature backed by a third-party JS library (charting, layout, editors, etc.).
---

# Wrapping an existing JavaScript library

Tesserae ships several components that are thin C# wrappers over JS libraries —
`Masonry` (masonry-layout), tooltips (tippy/popper), `CodeDiff` (diff2html),
`MarkdownBlock` (marked + purify), `SortableStack` (sortable), `Diagram`
(baklava). The pattern is always the same: **bundle the script, then drive its
global from C# through `Script.Write`.**

## 1. Bundle the library

Put the minified library under `Tesserae/h5/assets/js/` and add it to the
resource bundles in `Tesserae/h5.json`. It must appear in **both** the
`tss-dep.js` and `tss-dep.min.js` bundles (keep the two file lists in sync —
h5 swaps between them for Debug vs Release builds):

```jsonc
{
  "name": "tss-dep.js",
  "files": [
    "h5/assets/js/popper.min.js",
    "h5/assets/js/tippy.min.js",
    "h5/assets/js/masonry.min.js",
    "h5/assets/js/yourlib.min.js"     // <-- add here
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
using H5;
using static H5.Core.dom;
using static Tesserae.UI;

[H5.Name("tss.Masonry")]
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

## Key points

- **Instantiate against a real element.** Pass your host element as `{0}`.
- **Defer DOM-measuring calls to mount.** Use `DomObserver.WhenMounted(el, …)`
  (and `DomObserver.WhenRemoved` to tear down) — the element has no size until
  it is in the document.
- **Debounce expensive relayouts** with `window.setTimeout`/`clearTimeout`.
- **Hold the instance as `object`** and reach its methods/properties via
  `Script.Write("{0}.method({1})", _instance, arg)`.
- For a *typed* surface over the library instead of inline strings, declare an
  `[External]` / `[H5.Name]` binding — see `javascript-interop`.
- Add a `UI.Components.cs` factory and a sample, like any other component.

## Related

- `javascript-interop` — the `Script.Write` / `[External]` mechanics in detail.
- `creating-a-component` — the `IComponent` shell you are filling in.
- `masonry`, `tippy`, `charts` — existing wrappers to copy from.
- Docs: `/tesserae/extending/wrapping-a-javascript-library`
