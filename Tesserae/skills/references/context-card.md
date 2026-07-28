---
name: context-card
description: A compact card naming one piece of context attached to a conversation (a file, a page, a dataset) with a colored icon or image tile, a label, an optional second line, and a hover-revealed remove button. Use for the attachment row above a chat composer in a Tesserae (C#/Transpose) app.
---

# ContextCard

`ContextCard` is the small card an assistant UI shows for each piece of context the user attached:
an icon tile on a colored background, a label, and an optional second line ("PDF", "Dataset",
"42.109 rows"). It sizes itself to its content (`inline-flex`, never grows), so a wrapping
`HStack` above a `ChatArea` composer is the usual home for a row of them.

Give it a remove handler and it grows a round (x) button over its top-right corner that fades in
while the card is hovered or focused — and stays visible on touch screens, where nothing hovers.
The button overlays the card, so revealing it never shifts the layout.

## Create

`UI.ContextCard(string label, UIcons icon = UIcons.File, UIconsWeight weight = UIconsWeight.Regular)`
`UI.ContextCard(string label, IComponent iconOrImage)` — any component on the tile: an `Icon` with
its own color, an emoji (`Icon(Emoji.Sparkles)`), a small badge.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetLabel(string)` — the main line. Ellipsized to the width the card has, with the full text as
  its native tooltip.
- `.SetSubLabel(string)` — the second line. Null or empty hides it, leaving one centered row.
- `.SetIcon(UIcons, UIconsWeight = Regular)` / `.SetIcon(IComponent)` — what sits on the tile.
- `.SetImage(string url)` — fill the tile with a thumbnail (cropped to cover it) for context that
  has a preview of its own: an image, a screenshot, a favicon.
- `.IconBackground(string)` / `.IconForeground(string)` — tile colors, any CSS color
  (`"#ef4444"`, `"var(--tss-danger-background-color)"`). Defaults to the theme's primary colors.
- `.NoIconBackground()` — drop the colored square, letting the glyph sit on the card.
- `.Background(string)` — the card's own background.
- `.OnRemove(Action<ContextCard>)` / `.OnRemove(Action)` — adds the (x) button and calls back when
  it is clicked. The card does **not** remove itself: the handler owns the list it lives in, so it
  usually calls `stack.Remove(card)` and drops the underlying context at the same time.
- `.Removable(bool = true)` / `.NoRemove()` — show or hide the button without forgetting the handler.
- `.Compact()` — one tighter row, with the second line beside the label instead of below it. For a
  composer carrying many pieces of context at once.
- `.OnClick(...)` — makes the whole card open the context it stands for, and makes it keyboard
  reachable (Enter or Space). Clicking the remove button never reads as a click on the card.
- `Label`, `SubLabel`, `IsRemovable` — read state.

Sizing helpers work as usual: `.MaxWidth(260.px())` is the normal way to cap a card whose label may
be long, since the label ellipsizes to whatever width the card ends up with.

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

// A thumbnail instead of a glyph, and a compact card for a crowded composer.
var shot = ContextCard("screenshot.png", UIcons.FileImage).SetSubLabel("Image").SetImage(url);
var page = ContextCard("tesserae.dev/components", UIcons.Globe).SetSubLabel("Web page").Compact();

var composer = VStack().WS().Children(attached, OmniBox());
```

## Related

- Chat (ChatArea / ChatMessage) — `chat.md`
- ResourceCard (the larger, full resource summary) — `resource-card.md`
- Badge / Tag / Chip (removable inline tokens) — `badge.md`
- Icon and UIcons — `icon.md`, `uicons.md`
- Full docs & API: `/tesserae/components/context-card`
