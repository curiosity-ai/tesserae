---
name: grid-picker
description: An interactive grid of multi-state buttons with drag-to-select, configured by column/row names and a per-state formatter. Use for schedule pickers or grid state selectors in a Tesserae (C#/h5) app.
---

# GridPicker

A labelled grid of buttons where each cell cycles through a fixed number of
states on click, and supports drag-to-select across a rectangle of cells. You
supply the column/row headers, the state count, the initial states, and a
formatter that styles each button for its state. Typical use: weekly/hourly
schedule selectors.

## Create

`UI.GridPicker(string[] columnNames, string[] rowNames, int states, int[][] initialStates, Action<Button,int,int> formatState, UnitSize[] columns = null, UnitSize rowHeight = null)`.
`initialStates` is `[row][column]`. `formatState` is `(Button btn, int state, int previousState)` —
`previousState` is `-1` outside a drag/hover transition. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `formatState` callback — set the button's look per state, e.g. `btn.SetText(...)` or `btn.Background(color)`.
- `.GetState()` — current states as `int[][]` (a copy).
- `.SetState(int[][])` — replace states.
- `.OnChange(ComponentEventHandler<GridPicker, Event>)` — fires after a click or drag completes.
- `columns` / `rowHeight` — optional `UnitSize[]` track widths and row height (default `1.fr()`).

## Example

```csharp
using static Tesserae.UI;

var columns = new[] { "Mon", "Tue", "Wed", "Thu", "Fri" };
var rows    = new[] { "Morning", "Afternoon", "Evening" };
var initial = Enumerable.Range(0, rows.Length).Select(_ => new int[columns.Length]).ToArray();

Action<Button, int, int> format = (btn, state, prev) =>
    btn.SetText(state == 0 ? "Off" : "On");

var picker = GridPicker(columns, rows, states: 2, initialStates: initial, formatState: format)
    .OnChange((s, _) => Console.WriteLine("changed"));
```

## Related

- Button — `button.md`
- Full docs & API: `/tesserae/components/grid-picker`
