---
name: pagination
description: A page-number navigation strip for walking through pages of results. Use when paging a large result set into fixed-size pages in a Tesserae (C#/h5) app.
---

# Pagination

A navigation strip with First/Previous, numbered page buttons (with ellipses), Next/Last, and a "Page X of Y" status label. It tracks `currentPage` and raises an event on change — you supply the data slicing.

## Create

`UI.Pagination(int totalItems = 0, int pageSize = 10, int currentPage = 1)` — returns a `Pagination`. Also `new Pagination(...)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.OnPageChange(Action<Pagination>)` — callback fired when the page changes; read `p.CurrentPage` inside.
- `.SetPage(int page, bool raiseEvent = true)` / `.CurrentPage` — go to a page.
- `.Next()` / `.Previous()` / `.First()` / `.Last()` — relative navigation.
- `.SetTotalItems(int)` / `.TotalItems`, `.SetPageSize(int)` / `.PageSize` — reconfigure; page is clamped.
- `.TotalPages` — computed read-only count.
- `.MaxPageButtons` — max numbered buttons before ellipses kick in (minimum 5, default 7).
- `.ShowStatus` — toggle the "Page X of Y" label.

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

## Related

- Full docs & API: `/tesserae/components/pagination`
