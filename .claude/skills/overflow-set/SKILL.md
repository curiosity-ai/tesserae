---
name: overflow-set
description: A horizontal set of items that automatically collapses any items that don't fit into an overflow ("...") menu. Use when building a toolbar or command bar that must adapt to constrained widths in a Tesserae (C#/h5) app.
---

# OverflowSet

Lays items in a row with separators; when the row is too narrow, trailing items collapse
into a chevron overflow menu (a `ContextMenu`). Same collapse mechanism as `Breadcrumb`,
but for generic items (typically link-styled buttons).

## Create

`OverflowSet()` — returns an `OverflowSet`. Add items with `.Items(...)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Items(params IComponent[])` — adds items (auto-inserts separators).
- `.SetOverflowIndex(int)` — index after which items may collapse.
- `.JustifyContent(ItemJustify)` — horizontal distribution.
- `.Small()` — compact variant.
- `.DisableSizeCache()` — recompute item widths on every layout (use if item widths change).
- `.Add(IComponent)` / `.Clear()` / `.Replace(new, old)` — mutate items.

## Example

```csharp
using static Tesserae.UI;

var message = TextBlock();

var bar = OverflowSet()
    .Items(
        Button("Folder 1").Link().OnClick((s, e) => message.Text("Folder 1")),
        Button("Folder 2").Link().OnClick((s, e) => message.Text("Folder 2")).Disabled(),
        Button("Folder 3").Link().OnClick((s, e) => message.Text("Folder 3")),
        Button("Folder 4").Link().OnClick((s, e) => message.Text("Folder 4")),
        Button("Folder 5").Link().OnClick((s, e) => message.Text("Folder 5")))
    .Small();
```

## Related

- Breadcrumb — `../breadcrumb/SKILL.md` (hierarchical trail, same collapse logic)
- Stack — `../stack/SKILL.md`
- Full docs & API: `/tesserae/collections/overflow-set`
