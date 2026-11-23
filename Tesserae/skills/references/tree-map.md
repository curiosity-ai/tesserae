---
name: tree-map
description: A zoomable, canvas-drawn tree map of nested rectangles sized by value, leaves coloured by category, with a share-per-category legend and highlighting. Use when showing what a hierarchy is made of (code by directory, disk usage, budget by department) in a Tesserae (C#/Transpose) app.
---

# TreeMap

Each leaf is a rectangle whose area is its value; each group is the sum of what
it holds (squarified layout). Leaves are coloured by category, groups are
separated by outlines and named on a plate in their top-left corner, and the
legend lists each category with its share of the total. It is drawn on a canvas,
so it stays smooth with tens of thousands of leaves.

## Create

`UI.TreeMap(TreeMapNode root = null)` — the root is not drawn; its children are the top-level groups.
`new TreeMapNode(string label, double value = 0, string category = null)` — one node. A node with
children ignores its own `Value` and is sized by their sum.

```csharp
var root = new TreeMapNode("disk").Add(
    new TreeMapNode("Videos").Add(
        new TreeMapNode("Holiday.mp4", 18_400, "Video"),
        new TreeMapNode("Talk.mov", 9_200, "Video")),
    new TreeMapNode("Photos").Add(
        new TreeMapNode("2025", 9_800, "Images")),
    new TreeMapNode("Documents", 1_800, "Documents"));
```

`TreeMapNode` also has `Color` (beats the category's colour), `Tooltip` (an extra
tooltip line), `Data` (anything you want back in a click), and read-only
`Parent`, `Children`, `IsLeaf`, `Total`, `Depth` and `Path` (labels joined with `/`).
`.Add(params TreeMapNode[])` and `.Add(label, value, category)` return the node itself.

## Configuration

- `.Data(TreeMapNode root)` — replaces the tree.
- `.Category(string name, string color)` — fixes a category's colour (any CSS colour, tokens included) and its place in the colour order; `color: null` fixes only the order. Other categories take the palette in order of first appearance.
- `.Colors(params string[] palette)` — the palette; defaults to `--tss-chart-series-1` … `8`, so custom themes recolour it.
- `.Legend(bool show = true)` — on by default; entries sorted by share, a click highlights that category (again to clear).
- `.Tooltips(bool)`, `.LeafLabels(bool)` — hover tooltip and leaf names (both on).
- `.FormatValues(Func<double, string>, string unit = null)` / `.Unit("lines")` — how values read in the tooltip.
- `.Zoomable(bool = true)`, `.MaxZoom(double)` — wheel zooms around the pointer, drag pans, two fingers pinch, double click zooms in (shift: out); keys `+` `-` `0` and arrows. A "Show all" button appears while zoomed.
- `.ResetZoom()`, `.ZoomTo(TreeMapNode)` — animated (instant under reduced motion).

## Highlighting

- `.Highlight(Func<TreeMapNode, bool> predicate, string label = null)` — matching leaves keep full colour, the rest fade. Re-applied when the data changes.
- `.HighlightCategory(string)`, `.ClearHighlight()`.
- `.OnHighlightChanged(Action<TreeMap>)` — fires from code and from the legend; read `IsHighlighted`, `HighlightedCount`, `HighlightLabel`.

## Events

- `.OnNodeClick(Action<TreeMapNode>)` — a leaf, or a group when its name plate is clicked. A press that moved is a pan, not a click.

## Sizing

The map fills its box; give it a height (`.H(560.px())`). It lays itself out again when the box resizes.

## Example

```csharp
var map = TreeMap(root).Unit("MB").H(480.px());
map.OnNodeClick(n => { if (!n.IsLeaf) map.ZoomTo(n); });

var find = SearchBox("Find").SearchAsYouType().OnSearch((_, q) =>
{
    if (string.IsNullOrWhiteSpace(q)) map.ClearHighlight();
    else map.Highlight(n => n.Label.Contains(q), q);
});
```

## Related

- Charts (bar, line, pie, heat map) — `charts.md`
- Tree — the same hierarchy as an expandable list — `tree.md`
- Full docs & API: `/tesserae/charts/tree-map`
