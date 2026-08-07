---
name: pages-stack
description: A macOS-Downloads-style stack of page thumbnails that fans out on hover, with a plus-N badge for the pages it does not draw, inside a rail sized so opening the fan never widens the row. Use as the document preview beside a search result or file row in a Tesserae (C#/Transpose) app.
---

# PagesStack

`PagesStack` is the little pile of pages a document preview is drawn as: up to five overlapping,
slightly rotated pages that fan out along a shallow arc when the pointer is over them, with a `+N`
badge over the top-right counting whatever the stack doesn't draw.

The important part is the geometry. The stack lives in a **holder sized to the width the fan needs**
and is pinned to that holder's right edge, so the fan opens into reserved space: it draws on top of
whatever is beside it instead of pushing the row around. That is what lets it sit right next to a
result's title and metadata.

Pages are either image thumbnails or blank ruled placeholders, for a document whose thumbnails
haven't been generated — or aren't worth generating — yet.

## Create

- `UI.PagesStack(params string[] imageUrls)` — one page per url, all cropped to the same page size.
- `UI.PagesStack(int pages)` — that many blank ruled pages.

Also `new PagesStack(...)`. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetPages(params string[])` / `.SetPages(int)` — replace the pages.
- `.TotalPages(int)` — how many pages the document has: the ones past those drawn are counted by the
  `+N` badge. A stack given thumbnails draws only those, so `PagesStack(4 urls).TotalPages(9)` is four
  thumbnails and a `+5`.
- `.MaxVisible(int)` — how many pages are drawn before the rest collapse into the badge (5 by default).
- `.PageSize(int width, int height)` — the size every page is drawn at, portrait (48×62 by default).
  The rail width follows from it, so a larger page reserves more room for the fan.
- `.MatchThumbnailShape(bool = true)` — on by default: the first thumbnail to load that is wider than
  it is tall turns the pages landscape, keeping the long side of `PageSize` and taking the short one
  from that thumbnail's aspect ratio (clamped at 3:1, so a panorama doesn't draw the stack as
  slivers). All the pages of one document share the shape the first measured thumbnail reports —
  reshaping per thumbnail would rewrite the row's layout on every image that arrived. The rail and the
  page overlap are rewritten in place, without rebuilding the pages. Pass `false` to keep the
  configured size whatever loads.
- `IsLandscape` / `DrawnPageWidth` / `DrawnPageHeight` — the shape the pages are actually drawn at
  right now, which is `PageSize`'s until a landscape thumbnail has reshaped them.
- `.Fanned(bool = true)` — hold the stack open, for a host that wants the fan to follow hovering
  something larger than the stack. `OmniResult` does exactly this for the row it sits in
  (`PagesFanOnHover`, on by default).
- `.OnPageClick(Action<int>)` — makes each drawn page clickable, handing the handler its 0-based
  index, so "open the document at the page the user pointed at" is one call. The click is the page's
  alone — it never also counts as a click on the row the stack sits in — and each page takes a tab
  stop of its own and answers Enter and Space. Pass null to make the pages plain again.
- `TotalPageCount` / `VisiblePageCount` / `IsFanned` — what it is showing right now.

Motion is 240ms and honours `prefers-reduced-motion`. Pages read as paper in both themes: a pale fill
with hairline rules under a light theme, lifted off the background under a dark one.

## Example

```csharp
using static Tesserae.UI;

// A 24-page PDF: five pages drawn, "+19" over the corner.
var preview = PagesStack(5).TotalPages(24);

// Real thumbnails, and more pages than there are thumbnails for.
var thumbs = PagesStack(page1Url, page2Url, page3Url).TotalPages(12);

// A deck: the slides load landscape, so the pages turn landscape with them.
var deck = PagesStack(slideUrls).TotalPages(32);

// Where it usually goes: the preview rail of a search result.
var row = OmniResult(hit, hit.Name)
    .SetIcon(UIcons.FilePdf, "#ef4444")
    .SetText(hit.Excerpt)
    .SetPages(PagesStack(thumbnailUrls).TotalPages(hit.Pages).OnPageClick(page => Open(hit, page)));
```

Standalone, in a row of its own, remember it is right-aligned inside its rail — put it in an
`HStack().AlignItems(ItemAlign.End)` if you are lining several up next to their labels.

## Related

- OmniResult — the search-result row this is the preview for — `omni-result.md`
- Image — what a thumbnail page is drawn with — `image.md`
- Carousel (when the pages should be paged through rather than previewed) — `carousel.md`
- Full docs & API: `/tesserae/components/pages-stack`
