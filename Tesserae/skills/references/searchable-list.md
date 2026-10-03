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
- `.WithPagination(int pageSize)` — paginate filtered results. Adds a `Pagination` footer below
  the list, which hides itself while everything fits on one page (`pagination.md`).
- `.Virtualize(UnitSize itemHeight)` — virtualise rows (fixed height) for large lists.
- `.HideSearchBoxIfLessThan(int)` — hide the box unless the list holds at least N items **total**. The threshold is measured against the full list, not the current query's results, so narrowing the results (or a background search) never hides the box out from under an active query.
- `.ShowNotMatching()` — keep non-matching rows visible (dimmed) instead of removing them.
- `.BeforeSearchBox(...)` / `.AfterSearchBox(...)` — add controls around the search box.
- `.Progress(percent)` / `.Progress(position, total)` / `.ProgressIndeterminate()` / `.HideProgress()` /
  `.ShowProgressWhile(Task)` — a progress bar on the built-in search box while items load into the list; the
  list stays searchable and renders each batch as it lands. The box is shown while it carries progress, even under `.HideSearchBoxIfLessThan`. A `.WithBackgroundSearch` query shows the sweep on its own while it runs.
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

Loading items in the background, with progress on the search box:

```csharp
var items = new ObservableList<Contact>();
var list  = new SearchableList<Contact>(items).Height(400.px());

async Task LoadAsync()
{
    list.Progress(0, total);
    foreach (var batch in await FetchBatchesAsync())
    {
        items.AddRange(batch);
        list.Progress(items.Count, total);
    }
    list.HideProgress();
}

// Or, when the size is not known: list.ShowProgressWhile(LoadAsync());
```

## Related

- SearchableGroupedList — `searchable-grouped-list.md` (grouped variant)
- ItemsList — `items-list.md`
- Full docs & API: `/tesserae/collections/searchable-list`
