---
name: breadcrumb
description: A navigational trail showing the user's position within a hierarchy, collapsing extra levels into an overflow menu when space runs out. Use when building a hierarchical breadcrumb nav in a Tesserae (C#/Transpose) app.
---

# Breadcrumb

A horizontal trail of clickable crumbs (Home / Products / Electronics). When the
container is too narrow it collapses middle crumbs behind a "..." overflow menu.

## Create

`Breadcrumb()` — returns a `Breadcrumb`. Add crumbs with `Crumb(string)` (a link-styled
`Button`) inside `.Items(...)`. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Items(params IComponent[])` — adds the crumbs (auto-inserts chevron separators).
- `Crumb(text).OnClick((s,e) => …)` — per-crumb click handler; omit on the last (current) crumb.
- `.SetChevron(UIcons icon)` — replace the default separator icon (e.g. `UIcons.AngleDoubleRight`).
- `.SetOverflowIndex(int)` — index after which crumbs may collapse into the overflow menu.
- `.Small()` — compact spacing.
- `.MaxWidth(unitSize)` — constrain width to trigger collapsing.
- `.Add(IComponent)` / `.Clear()` / `.Replace(new, old)` — mutate items.

## Example

```csharp
using static Tesserae.UI;

var msg = TextBlock("(none)");

var nav = Breadcrumb()
    .MaxWidth(280.px())
    .SetChevron(UIcons.AngleRight)
    .SetOverflowIndex(1)
    .Items(
        Crumb("Home").OnClick((s, e) => msg.Text("Home")),
        Crumb("Products").OnClick((s, e) => msg.Text("Products")),
        Crumb("Electronics") // last crumb: no handler
    );
```

## Related

- OverflowSet — `overflow-set.md` (same collapse mechanism, generic items)
- Stack — `stack.md`
- Full docs & API: `/tesserae/collections/breadcrumb`
