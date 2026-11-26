---
name: search-ask-box
description: A text field with two named buttons at its edge, Search and Ask, so one box offers both a search and a question to an assistant without a mode switch, with @ mentions (places to search in, assistants to ask) and key:value filter chips. Use for a home page or start screen in a Tesserae (C#/Transpose) app where the user types once and then chooses what to do with it.
---

# SearchAskBox

One text field with two "doors": **Search** and **Ask**. What to do with the text is chosen when it is sent,
so there is no mode toggle. The text is plain: nothing is parsed or highlighted (use `OmniBox` for query
syntax, snaps and a chat composer). The field grows with the text up to six lines, then scrolls.

Keys: **Enter** searches, **Ctrl+Enter** (Cmd+Enter on a Mac) asks, **Shift+Enter** starts a new line. Enter
while an input method is composing a word (Japanese, Chinese, Korean) is left to the input method. Both doors
are disabled while the box is empty.

## Create

`UI.SearchAskBox(string placeholder = null)`, returns a `SearchAskBox`. Also `new SearchAskBox(placeholder)`.
The placeholder is also the field's accessible name. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.OnSearch((sender, text) => ...)` / `.OnAsk((sender, text) => ...)`: raised with the trimmed text.
- `.Layout(SearchAskBox.DoorLayout)`: `SideBySide` (default; both doors the box's full height at its right
  end), `Stacked` (Search above Ask, each half the height), `Outside` (the doors in their own frame beside
  the field).
- `.ShowKeys(bool = true)`: prints each door's key on it (`↵` on Search, `Ctrl ↵` / `⌘ ↵` on Ask). Off by default.
- `.Primary(SearchAskBox.PrimaryDoor)`: `None` (default; both doors grey, the primary colour under the
  pointer), `Ask` or `Search` (that door filled at rest).
- `.AskEnabled(bool enabled, string reason = null)`: keeps Ask disabled whatever is typed, with the reason
  as its tooltip, for a host with nothing to answer with (no AI model). Search and Enter still work.
- `.DoorTexts(string search, string ask)`: renames the doors (translations).
- `.Placeholder(string)`, `.Text` (get/set), `.Clear()`, `.Focus()`, `.TabIndex`.
- The root element is `.tss-searchaskbox-host`; the frame inside it is `.tss-searchaskbox`.

## @ mentions and filter chips

Typing `@` lists the box's mentions, grouped by door (Search in / Ask) with the filters after them; typing a
filter key and a colon (`ext:`) lists its values. Arrow keys move, Enter or Tab picks, Escape closes. A pick
becomes a chip in the field; Backspace at the start of the field removes the last one, and each chip has a ×.

**A chip belongs to one door, and the other door folds away** (an animated collapse: its width side by side, its
row when stacked). Enter then does what is left: Search while it is open, otherwise Ask. Suggestions for the folded
door are not offered, so chips never conflict. Filters belong to Search; a search may go out with chips and no text.

- `new SearchAskBox.Mention(id, name, SearchAskBox.Door.Search | .Ask, icon, description, group, tag)`.
- `.Mentions(params Mention[])`, and `.Mentions(Func<Task<IEnumerable<Mention>>>)` for ones fetched the first time
  `@` is typed (the assistants a user may talk to).
- `new SearchAskBox.Filter(id, name, keys, string[] values | Func<Task<string[]>> values, icon, description)`;
  `.Filters(params Filter[])`. Typing a full `key:value` and a space also makes the chip.
- Read them back in `OnSearch` / `OnAsk`: `sender.Chips` (each a `Mention`, or a `Filter` with its `Value`),
  `sender.SelectedMentions`, `sender.SelectedFilters`, `sender.AvailableDoors`. `.OnChipsChanged(...)`.
- `.AddChip(mention)` / `.AddChip(filter, value)` / `.ClearChips()`; `.Clear()` empties text and chips.
- `.SuggestionGroupNames(searchIn, ask, filters)` for translations.
- The list is mounted on the body while open (no container clips it) and placed under the box, or above it.
- **Name the nested types in full** (`Tesserae.SearchAskBox.Mention`, `.Filter`, `.Door`, `.Chip`) in code outside the
  `Tesserae` namespace that has both `using Tesserae;` and `using static Tesserae.UI;`: there `SearchAskBox` is both the
  type and the factory method, and `SearchAskBox.Door` does not compile (CS0119).

```csharp
var box = SearchAskBox("Search or ask")
    .Mentions(new SearchAskBox.Mention("files", "Files", SearchAskBox.Door.Search, UIcons.Document, "Only files"))
    .Mentions(async () => (await LoadAssistantsAsync())
        .Select(a => new SearchAskBox.Mention(a.Id, a.Name, SearchAskBox.Door.Ask, UIcons.Robot, a.Description, tag: a)))
    .Filters(new SearchAskBox.Filter("ext", "Extension", new[] { "ext" }, new[] { "pdf", "docx" }, UIcons.FileCode))
    .OnSearch((s, text) => ShowResults(text, s.Chips))
    .OnAsk((s, text)    => StartChat(text, s.SelectedMentions.FirstOrDefault()?.Tag));
```

## Example

```csharp
using static Tesserae.UI;

var box = SearchAskBox("Search or ask about your workspace")
    .AskEnabled(hasModel, "No AI model is configured")
    .OnSearch((_, text) => ShowResults(text))
    .OnAsk((_, text)    => StartChat(text));

var home = VStack().S().AlignItemsCenter().JustifyContent(ItemJustify.Center).Children(
    TextBlock("What are you looking for?").Large().SemiBold().PB(20),
    box.MaxWidth(720.px()).WS());
```

## Related

- OmniBox: one input for search and chat, with query syntax, snaps and a chat composer. `omni-box.md`
- SearchBox: a single-line search input. `search-box.md`
- TextArea: the plain multi-line input. `text-area.md`
- Full docs & API: `/tesserae/search/search-ask-box`
