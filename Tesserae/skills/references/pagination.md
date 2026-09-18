---
name: pagination
description: A page-number navigation strip for walking through pages of results. Use when paging a large result set into fixed-size pages in a Tesserae (C#/Transpose) app.
---

# Pagination

A navigation strip that reads as a footer under the set it pages: the range on the left
(`1-25 of 118`), Previous, the numbered page buttons (with ellipses) and Next on the right. It
tracks `currentPage` and raises an event on change — you supply the data slicing.

Two defaults are worth knowing before you configure anything:

- **A set that fits on one page renders nothing.** A lone `1` button between two greyed chevrons
  says only that there is nothing to navigate, so the strip hides itself. `.AlwaysVisible()` keeps
  its place — use it where the footer's height must not change as a list is filtered.
- **There are no jump-to-first/last chevrons.** The strip always numbers the first and last page,
  so `«`/`»` would duplicate the two buttons beside them. `.WithFirstLastButtons()` adds them back.

## Create

`UI.Pagination(int totalItems = 0, int pageSize = 10, int currentPage = 1)` — returns a `Pagination`. Also `new Pagination(...)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.OnPageChange(Action<Pagination>)` — callback fired when the page changes; read `p.CurrentPage` inside.
- `.SetPage(int page, bool raiseEvent = true)` / `.CurrentPage` — go to a page.
- `.Next()` / `.Previous()` / `.First()` / `.Last()` — relative navigation. `First`/`Last` work
  whether or not their buttons are rendered.
- `.SetTotalItems(int)` / `.TotalItems`, `.SetPageSize(int)` / `.PageSize` — reconfigure; page is clamped.
- `.TotalPages` — computed read-only count.
- `.MaxPageButtons` — max numbered buttons before ellipses kick in (minimum 5, default 7).
- `.ShowStatus` — toggle the range label.
- `.SetFormat(Func<int, int, int, string>)` — rewrite that label from `(from, to, total)`, for
  another language or for `1 to 25 of 118`.
- `.AlwaysVisible()` / `.ShowForSinglePage` — render even when everything fits on one page.
- `.WithFirstLastButtons()` / `.ShowFirstLastButtons` — add the jump-to-end chevrons.
- `.AsListFooter()` — the footer treatment: a rule along the top and padding matching the rows
  above. `SearchableList.WithPagination` / `SearchableGroupedList.WithPagination` apply it for you.

Paging with the keyboard keeps its place: the strip rebuilds its buttons on every change, and
focus moves to the new button doing the same job — or to the current page when stepping onto the
last page disables `Next` under the cursor.

## Example

```csharp
using static Tesserae.UI;

var status = TextBlock("Showing page 1").Medium();

var view = VStack().Children(
    Card(status).MB(16),
    Pagination(totalItems: 120, pageSize: 10, currentPage: 1)
        .OnPageChange(p => status.Text = $"Showing page {p.CurrentPage}")
);
```

Inside a list, `WithPagination` builds and places the strip — you do not construct one:

```csharp
SearchableList(items).WithPagination(25).S();
```

## Related

- InlinePagination — the compact "3 of 7" pill, for stepping one at a time from a toolbar — `inline-pagination.md`
- SearchableList / SearchableGroupedList — `.WithPagination(pageSize)` pages them for you — `searchable-list.md`

- Full docs & API: `/tesserae/components/pagination`
