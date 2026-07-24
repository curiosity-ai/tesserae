---
name: searchable-list
description: A searchable, scrollable list whose items are filtered live as the user types into a built-in search box, with optional async background search. Use when displaying a filterable list of items in a Tesserae (C#/Transpose) app.
---

# SearchableList

A list with a built-in "type to search" box. Items implement `ISearchableItem`
(`IsMatch(searchTerm)` and `Render()`); a row shows when every search term matches.

## Create

`SearchableList<T>(T[] items, params UnitSize[] columns)` where `T : ISearchableItem`.
An overload takes an `ObservableList<T>`. Multiple `columns` → grid layout.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.WithNoResultsMessage(Func<IComponent>)` — placeholder when nothing matches.
- `.WithBackgroundSearch(Func<string, Task<T[]>>)` — merge async (e.g. remote) results with local matches.
- `.WithPagination(int pageSize)` — paginate filtered results.
- `.Virtualize(UnitSize itemHeight)` — virtualise rows (fixed height) for large lists.
- `.HideSearchBoxIfLessThan(int)` — hide the box unless the list holds at least N items **total**. The threshold is measured against the full list, not the current query's results, so narrowing the results (or a background search) never hides the box out from under an active query.
- `.ShowNotMatching()` — keep non-matching rows visible (dimmed) instead of removing them.
- `.BeforeSearchBox(...)` / `.AfterSearchBox(...)` — add controls around the search box.
- `.SearchBox(Action<SearchBox>)` / `.CaptureSearchBox(out SearchBox)` / `.SetKeyboardShortcut(keys)`.
- `.Items` — the backing `ObservableList<T>`; mutate to update the list. The list re-filters live against any active query, and the query text is preserved across updates.
- `.Height(unitSize)` — fixes height for scrolling.

## Example

```csharp
using static Tesserae.UI;

var items = contacts.Select(c => new Contact(c.Name, c.Role, c.Email)).ToArray();

return SearchableList(items)
    .WithNoResultsMessage(() => Card(TextBlock("No Results").Padding(16.px())))
    .Height(400.px())
    .Render();

// class Contact : ISearchableItem {
//   public bool IsMatch(string t) => Name.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0;
//   public IComponent Render() => Card(TextBlock(Name));
// }
```

## Related

- SearchableGroupedList — `searchable-grouped-list.md` (grouped variant)
- ItemsList — `items-list.md`
- Full docs & API: `/tesserae/collections/searchable-list`
