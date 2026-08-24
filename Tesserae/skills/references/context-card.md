---
name: context-card
description: A compact card naming one piece of context attached to a conversation (a file, a page, a dataset) with a colored icon or image tile, a label, an optional second line, a hover-revealed remove button and an optional right-click menu. Use for the attachment row above a chat composer in a Tesserae (C#/Transpose) app.
---

# ContextCard

`ContextCard` is the small card an assistant UI shows for each piece of context the user attached:
an icon tile on a colored background, a label, and an optional second line ("PDF", "Dataset",
"42.109 rows"). It sizes itself to its content (`inline-flex`, never grows), so a wrapping
`HStack` above a `ChatArea` composer is the usual home for a row of them.

Give it a remove handler and it grows a round (x) button just off its top-right corner that fades in
while the card is hovered or focused — and stays visible on touch screens, where nothing hovers. The
button is absolutely positioned and overhangs the card by a few pixels, so revealing it never shifts
the layout; if the row lives in a container that clips (`overflow: hidden`), give the row a few
pixels of padding so the disc isn't cut off.

Right-clicking a card opens the `ContextMenu` given to `OnContextMenu`, at the pointer. The text on a
card is not selectable, so a right-click (or a drag across a row of cards) never leaves half a file
name highlighted — `Selectable()` opts back in.

## Create

`UI.ContextCard(string label, UIcons icon = UIcons.File, UIconsWeight weight = UIconsWeight.Regular)`
`UI.ContextCard(string label, IComponent iconOrImage)` — any component on the tile: an `Icon` with
its own color, an emoji (`Icon(Emoji.Sparkles)`), a small badge.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetLabel(string)` — the main line. Ellipsized to the width the card has, with the full text as
  its native tooltip.
- `.SetSubLabel(string)` — the second line. Null or empty hides it, leaving one centered row.
  `.MonospaceSubLabel()` renders it in the monospace font, for a path, a table name or a size — the
  same treatment `ToolCall` gives the command it names.
- `.SetBadge(string)` / `.SetBadge(IComponent)` — a small pill at the end of the card. The card says
  nothing about what belongs there — what a piece of context *is* is carried by the icon you pass — so
  it takes whatever the app wants: a source, a count, a status. The `IComponent` overload drops the pill
  chrome so a `Badge`, a `Spinner` or a small button keeps its own styling.
- `.SetIcon(UIcons, UIconsWeight = Regular)` / `.SetIcon(IComponent)` — what sits on the tile.
- `.SetImage(string url)` — fill the tile with a thumbnail (cropped to cover it) for context that
  has a preview of its own: an image, a screenshot, a favicon.
- `.IconTint(string color, int percent = 14)` — a wash of the color behind the glyph with the glyph in
  full strength. The quiet option, and what a row of many cards usually wants so the colors read as
  file types rather than as decoration.
- `.IconBackground(string)` / `.IconForeground(string)` — tile colors, any CSS color
  (`"#ef4444"`, `"var(--tss-danger-background-color)"`). Defaults to the theme's primary colors.
- `.NoIconBackground()` — drop the colored square, letting the glyph sit on the card.
- `.MaxLabelWidth(UnitSize)` — cap where the label is cut. A trailing file extension is held outside
  that width and the ellipsis is placed by measuring the text, so a narrow card reads
  "Quarterly repo….pdf" rather than "Quarterly repor…"; `.KeepExtensionVisible(false)` opts out.
- `.WithChevron()` — a chevron at the end, the hint that clicking the card opens what it stands for.
- `.Tag` — an arbitrary payload (the document, record or row the card stands for), so a click or remove
  handler can act on it without a lookup.
- `.Background(string)` — the card's own background.
- `.OnRemove(Action<ContextCard>)` / `.OnRemove(Action)` — adds the (x) button and calls back when
  it is clicked. The card does **not** remove itself: the handler owns the list it lives in, so it
  usually calls `stack.Remove(card)` and drops the underlying context at the same time. The disc is a
  solid neutral (`--tss-colors-neutral-1000`, a mid grey in the dark theme) with a white glyph, and
  turns `--tss-danger-background-color` while hovered.
- `.Removable(bool = true)` / `.NoRemove()` — show or hide the button without forgetting the handler.
- `.Compact(bool = true)` — a one-line pill, with the second line beside the label instead of below it.
  For a composer carrying many pieces of context at once, or one file named inline.
- `.OnClick(...)` — makes the whole card open the context it stands for, and makes it keyboard
  reachable (Enter or Space). Clicking the remove button never reads as a click on the card.
- `.OnContextMenu(Func<ContextMenu.Item[]>)` — attaches a `ContextMenu` opened by right-clicking the
  card, at the pointer, in place of the browser's own. The generator runs on every open, so the items
  can describe the card as it stands. The card also takes a tab stop and answers the keyboard menu key
  (or Shift+F10) with the same menu, anchored to the card. Returning `null` or an empty array opens
  nothing.
- `.OnContextMenu(Action<ContextCard>)` / `.OnContextMenu(Action)` — hand the right-click to a plain
  handler instead, suppressing the browser menu. The inherited `(card, event)` overload leaves the
  browser menu alone, for a handler that would rather decide for itself (call `StopEvent(e)` in it).
- `.ShowMenu()` — open the attached menu anchored to the card, the way the keyboard menu key does.
- `.Selectable(bool = true)` — let the text on the card be selected. It cannot be by default: a card
  is a token standing for one piece of context, not prose, so dragging across a row of them or
  right-clicking one never leaves half a file name highlighted.
- `Label`, `SubLabel`, `IsRemovable` — read state.

Sizing helpers work as usual: `.MaxWidth(260.px())` is the normal way to cap a card whose label may
be long, since the label ellipsizes to whatever width the card ends up with. Give the card a width of
its own (`.WS()` in a side panel listing sources, say) and the label takes the extra space, so the
second line, the badge and the chevron sit against the card's end and the label is what gets cut.

## Example

```csharp
using static Tesserae.UI;

var attached = HStack().Wrap().Gap(8.px()).WS();

void Attach(string name, string kind, UIcons icon, string color)
{
    var card = ContextCard(name, icon).SetSubLabel(kind).IconBackground(color);

    card.OnRemove(c => attached.Remove(c));   // the handler owns the row

    attached.Add(card);
}

Attach("Kindersonnenschutzmittel-NEU.pdf", "PDF",     UIcons.FilePdf,   "#ef4444");
Attach("customers",                        "Dataset", UIcons.Database,  "#f59e0b");

// Right-click a card for the actions on the context it stands for. The items are generated per open,
// so they can reflect the card's current state.
var doc = ContextCard("Q3-forecast.xlsx", UIcons.FileExcel).SetSubLabel("Spreadsheet").IconTint("#16a34a");

doc.OnContextMenu(() => new[]
{
    ContextMenuItem(doc.Label).Header(),
    ContextMenuItem("Open").OnClick(() => Open(doc.Tag)),
    ContextMenuItem().Divider(),
    ContextMenuItem("Detach").OnClick(() => attached.Remove(doc))
});

// A thumbnail instead of a glyph, and a compact card for a crowded composer.
var shot = ContextCard("screenshot.png", UIcons.FileImage).SetSubLabel("Image").SetImage(url);
var page = ContextCard("tesserae.dev/components", UIcons.Globe).SetSubLabel("Web page").Compact();

var composer = VStack().WS().Children(attached, OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)));
```

An `OmniBox` in chat mode hosts the row itself, inside the box below the input — that is the usual
place for these cards, and it wires each card's (x) to the row for you:

```csharp
var omni = new OmniBox(new OmniBox.Config(OmniBox.Mode.Chat))
    .WithContextToAdd(ContextCard("report-2026.pdf", UIcons.FilePdf).SetSubLabel("PDF").IconBackground("#ef4444"))
    .OnChat((s, m) => { Send(m.Text, s.ContextToAdd); s.ClearContext(); });

omni.AddContext(ContextCard("customers", UIcons.Database).SetSubLabel("Dataset"));
```

Several cards belonging together go in a `ContextCards` group — one summary pill that expands into a
list of rows, or a compact row of pills with a "+N more". See `context-cards.md`.

## Related

- ContextCards — the group — `context-cards.md`
- ContextMenu — the menu `OnContextMenu` opens, and its items — `context-menu.md`
- Chat (ChatArea / ChatMessage) — `chat.md`
- OmniBox — hosts a row of these below its chat input via `WithContextToAdd` — `omni-box.md`
- ResourceCard (the larger, full resource summary) — `resource-card.md`
- Badge / Tag / Chip (removable inline tokens) — `badge.md`
- Icon and UIcons — `icon.md`, `uicons.md`
- OmniResult (the search-result row a hit is drawn as) — `omni-result.md`
- Full docs & API: `/tesserae/components/context-card`
