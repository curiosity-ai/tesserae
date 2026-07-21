---
name: searchable-grouped-list
description: A searchable, scrollable list whose items are organised into named groups with per-group headers, filtered live as the user types. Use when displaying a categorised, filterable list in a Tesserae (C#/Transpose) app.
---

# SearchableGroupedList

A list with a built-in search box; items are grouped by `Group` and each group gets a
header you generate. Items implement `ISearchableGroupedItem` (`IsMatch(term)`, `Group`,
`Render()`).

## Create

`SearchableGroupedList<T>(T[] items, Func<string, IComponent> groupHeaderGenerator, params UnitSize[] columns)`
where `T : ISearchableGroupedItem`. An overload takes an `ObservableList<T>`.
Bring factories into scope with `using static Tesserae.UI;`.

The header generator receives the group name and returns its header component.
Multiple `columns` → grid layout for items; otherwise a stack.

## Key configuration

- `.WithNoResultsMessage(Func<IComponent>)` — placeholder when nothing matches.
- `.WithGroupOrdering(IComparer<string>)` — control group display order (default alphabetical).
- `.WithPagination(int pageSize)` — paginate filtered results.
- `.Virtualize(UnitSize itemHeight)` — virtualise rows (fixed height) for large groups.
- `.BeforeSearchBox(...)` / `.AfterSearchBox(...)` — add controls around the search box.
- `.SearchBox(Action<SearchBox>)` / `.CaptureSearchBox(out SearchBox)` / `.SetKeyboardShortcut(keys)`.
- `.Height(unitSize)` — fixes height for scrolling.

## Example

```csharp
using static Tesserae.UI;

var items = data.Select(d => new GroupedItem(d.Name, d.Group)).ToArray();

return SearchableGroupedList(items,
        group => HorizontalSeparator(TextBlock(group).Primary().SemiBold()).Left())
    .WithGroupOrdering(Comparer<string>.Create((a, b) => priority[a].CompareTo(priority[b])))
    .WithNoResultsMessage(() => Card(TextBlock("No matching items").Padding(16.px())))
    .Height(400.px())
    .Render();

// class GroupedItem : ISearchableGroupedItem {
//   public string Group { get; }
//   public bool IsMatch(string t) => Name.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0;
//   public IComponent Render() => Card(TextBlock(Name));
// }
```

## Related

- SearchableList — `searchable-list.md` (ungrouped)
- ItemsList — `items-list.md`
- SearchBox — `/tesserae/components/search-box`
- Full docs & API: `/tesserae/collections/searchable-grouped-list`
