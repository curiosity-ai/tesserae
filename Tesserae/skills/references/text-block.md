---
name: text-block
description: A display component for rendering styled text. Use when showing static or dynamic labels, headings, or inline text in a Tesserae (C#/Transpose) app.
---

# TextBlock

Renders text with consistent, configurable styling (size, weight, alignment, colour, wrapping). The most common building block for labels and copy.

## Create

`UI.TextBlock(text, treatAsHTML: false, selectable: false, textSize, textWeight, afterText: null)` — returns a `TextBlock`. The simplest form is `TextBlock("Hello")`.
Bring the factories into scope with `using static Tesserae.UI;`. `.Render()` returns the `HTMLElement`.

Text that changes: `UI.TextBlock(IObservable<string> text, selectable: false, textSize, textWeight)` or
`UI.TextBlock<T>(IObservable<T> source, Func<T, string> format, …)`. See **Live text** below.

## Live text

When the text depends on state, give the `TextBlock` the observable. **This is the way to do it.**
Each change is written into the same element, so nothing is rebuilt and nothing flickers:

```csharp
var count = new SettableObservable<int>(0);

TextBlock(count, c => $"Count: {c}").Large().SemiBold();   // formatted
TextBlock(statusObservable);                              // IObservable<string> as-is
TextBlock().Secondary().Text(count, c => $"{c} items"); // on an existing block
```

Do **not** write `DeferSync(count, c => TextBlock($"Count: {c}"))` for this. `Defer`/`DeferSync`
construct a new component on every change and swap the element in: the block is remounted,
loses its styling state and flickers. Keep `Defer` for when the *shape* of the content changes,
not its words.

Details of `.Text(source)` / `.Text(source, format)`:

- One-way: source to text. It is an overload of the plain `.Text(string)`, the same shape the rest
  of the toolkit uses for observable inputs (`NotificationCenter.BadgeCount(IObservable<int>)`,
  `ToolCall.SetProgress(IObservable<string>)`, `Chart.Series(IObservable<double[]>)`). It is not
  `.Bind`, which is two-way and only for input components over a `SettableObservable<T>`.
- Writes the current value immediately, so the first paint is already right.
- Subscribes only while the block is mounted: dropped on removal, re-taken (with the current value)
  if it is mounted again. No manual cleanup.
- The last `Text(...)` wins: another observable replaces the previous one, and a plain
  `.Text("...")` (or setting the `Text` / `HTML` property) stops following it.
- It sets plain text (`textContent`), not HTML; do not combine it with `afterText`.
- Any `IObservable<T>` works: `SettableObservable<T>`, a `ReadOnlyObservable<T>` subclass, the
  combined observables, `ObservableList<T>` (as `IObservable<IReadOnlyList<T>>`).

## Key configuration

Sizes and weights come from `ITextFormating` fluent helpers:

- `.Tiny()` / `.Small()` / `.Medium()` / `.Large()` / `.XLarge()` — text size.
- `.Regular()` / `.SemiBold()` / `.Bold()` — text weight.
- `.NoWrap()` — disable wrapping (sets `CanWrap = false`).
- `.Primary()` / `.Secondary()` / `.Success()` / `.Warning()` / `.Danger()` — colour variant (`IsWarning` / `IsDanger` read and set it).
- `.Title(string)` — tooltip (hover) text.
- `.AI(bool strongEffect = false)` — paints the words with the purple-to-blue AI gradient. For **short** strings: a title, a heading over generated output, a one-line summary.
- `.AISurface(bool strongEffect = false)` — generated prose: the theme's own text colour on a faint tinted panel with an accent edge, for a paragraph a gradient would make unreadable. See `ai-variants.md`.

Useful properties:

- `Text` — get/set the plain text. For text that changes over time, pass an observable to `.Text(...)` instead (see Live text).
- `HTML` — get/set inner HTML (when `treatAsHTML`).
- `IsSelectable` — allow text selection.
- `EnableEllipsis` / `EnableBreakSpaces` — overflow behaviour.

### Text that is cut short gets a tooltip

A block whose text is cut short — `EnableEllipsis`, a line clamp, any `overflow` that hides the
rest — shows the whole text in a tooltip when the pointer rests on it. There is nothing to call: the
check runs on hover, so text that fits gets no tooltip, and one that is cut short in a narrow window
and fits in a wide one only gets it while it is cut. The tooltip shows the text the block holds at
that moment.

It stays out of the way of anything that already says something on hover: a block with its own
`.Tooltip(...)` or `.Title(...)`, or one inside an element that has one (a button, a card), shows only
that. The same goes for a `Label`'s caption and for an `InlineLabel`.

## Example

```csharp
using static Tesserae.UI;

var heading = TextBlock("Hello, Tesserae!").Medium().SemiBold();

var note = TextBlock("The quick brown fox.", selectable: true)
    .Large()
    .NoWrap()
    .Title("Example tooltip");
```

## Related

- Observables — `observables.md`
- Defer — rebuilds content on change; not for text — `defer.md`
- Label — `label.md`
- ListItemText — a title with a subtitle under it — `list-item-text.md`
- AI variants — the `.AI()` / `.AISurface()` variant — `ai-variants.md`
- Full docs & API: `/tesserae/text/text-block`
