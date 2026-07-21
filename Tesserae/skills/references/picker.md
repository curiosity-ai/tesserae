---
name: picker
description: A multi-select input that filters a list of typed items via a text box and renders selections as removable tags. Use when letting the user search and pick one or more items (recipients, tags) in a Tesserae (C#/Transpose) app.
---

# Picker

A generic `Picker<TPickerItem>` where `TPickerItem` implements `IPickerItem` (a `Name`, an `IsSelected` flag, and a `Render()` returning the item's display component). Typing filters suggestions; clicking a suggestion adds a removable selection chip.

## Create

`UI.Picker<TItem>(int maximumAllowedSelections = int.MaxValue, bool duplicateSelectionsAllowed = false, int suggestionsTolerance = 0, bool renderSelectionsInline = true, string suggestionsTitleText = null)` — returns `Picker<TItem>` where `TItem : class, IPickerItem`.
Bring factories into scope with `using static Tesserae.UI;`.

`IPickerItem` requires: `string Name { get; }`, `bool IsSelected { get; set; }`, `IComponent Render()`.

## Key configuration

- `.Items(params TItem[])` / `.Items(IEnumerable<TItem>)` — add candidate items (throws on duplicate `Name`).
- `.OnItemSelected((sender, e) => ...)` — fired on selection; `e.Item` is the picked item.
- Constructor `suggestionsTolerance` — minimum typed chars before suggestions show (0 = show on focus).
- Constructor `renderSelectionsInline` — chips inline with the text box (`true`) or below it (`false`).
- `.SelectedPickerItems` / `.UnselectedPickerItems` / `.PickerItems` — enumerate state.
- `.AsObservable()` — observe the item list.

## Example

```csharp
using static Tesserae.UI;

public class TagItem : IPickerItem
{
    public TagItem(string name) { Name = name; }
    public string Name { get; }
    public bool IsSelected { get; set; }
    public IComponent Render() => TextBlock(Name);
}

var picker = Picker<TagItem>(maximumAllowedSelections: 3, suggestionsTitleText: "Suggested")
    .Items(new TagItem("Alpha"), new TagItem("Beta"), new TagItem("Gamma"))
    .OnItemSelected((s, e) => console.log("Selected: " + e.Item.Name));
```

## Related

- Dropdown — `.dropdown.md`
- Full docs & API: `/tesserae/components/picker`
