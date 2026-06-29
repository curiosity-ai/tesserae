---
name: details-list
description: A virtualised, sortable, multi-column table for typed items (Fluent-UI-style DetailsList) with column sorting, pagination, and an empty-state message. Use when building a data table / file-explorer-style list in a Tesserae (C#/h5) app.
---

# DetailsList

A multi-column table. Each row is a typed object implementing `IDetailsListItem<T>`;
columns are declared up front and can be sortable.

## Create

`DetailsList<T>(params IDetailsListColumn[] columns)` where `T : class, IDetailsListItem<T>`.
Build columns with `DetailsListColumn(...)` and `IconColumn(...)`.
Bring factories into scope with `using static Tesserae.UI;`.

`T` must implement: `CompareTo(T other, string key)`, `EnableOnListItemClickEvent`,
`OnListItemClick(int)`, and `Render(columns, cell)` yielding one `cell(column, () => component)` per column.

## Key configuration

- `DetailsListColumn(title, width, isRowHeader:, enableColumnSorting:, sortingKey:)` — text column.
- `IconColumn(Icon, width:, enableColumnSorting:, sortingKey:)` — single-icon column.
- `.WithListItems(params T[])` — set rows (call again to refresh).
- `.SortedBy(string sortingKey)` — initial sort (before headers exist).
- `.WithPaginatedItems(Func<Task<T[]>>)` — append more rows when scrolled to the bottom.
- `.WithEmptyMessage(Func<IComponent>)` — placeholder shown when there are no rows.
- `.Compact()` — denser row height.
- `.Height(unitSize)` — needed for scrolling/virtualisation.

## Example

```csharp
using static Tesserae.UI;

var list = DetailsList<FileRow>(
        IconColumn(Icon(UIcons.File), width: 32.px()),
        DetailsListColumn(title: "File Name", width: 280.px(), enableColumnSorting: true, sortingKey: "FileName", isRowHeader: true),
        DetailsListColumn(title: "File Size", width: 120.px(), enableColumnSorting: true, sortingKey: "FileSize"))
    .Height(400.px())
    .WithListItems(rows)
    .SortedBy("FileName");

// FileRow : IDetailsListItem<FileRow>
//   int CompareTo(FileRow o, string key) => key == "FileSize" ? Size.CompareTo(o.Size) : ...;
//   IEnumerable<IComponent> Render(IList<IDetailsListColumn> cols, Func<...> cell) {
//       yield return cell(cols[0], () => Icon(UIcons.FileWord));
//       yield return cell(cols[1], () => TextBlock(Name));
//   }
```

## Related

- VirtualizedList — `virtualized-list.md` (untyped, very large lists)
- ItemsList — `items-list.md`
- Full docs & API: `/tesserae/collections/details-list`
