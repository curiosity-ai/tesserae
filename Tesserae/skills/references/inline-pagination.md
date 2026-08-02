---
name: inline-pagination
description: The compact "3 of 7" pill with a chevron either side, for stepping through a set one at a time from a toolbar or a modal header. Use when a Tesserae (C#/Transpose) app needs previous/next beside other commands rather than a row of numbered page buttons.
---

# InlinePagination

`‹ 3 of 7 ›` in one rounded pill: a chevron either side of where you are in a set. It is for stepping
through things one at a time — the result open in a preview, the photo in a lightbox, the record in an
editor — beside other commands in a toolbar or a header. For a list that pages, with numbered buttons
and a page size, use `Pagination` instead.

## Create

`UI.InlinePagination(int position = 0, int count = 0)` — both 1-based; a count of zero leaves the label
out and the control is the two chevrons alone. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetPosition(int position, int count)` / `Position` / `Count` — what the label says.
- `.OnPrevious(Action<InlinePagination>)` / `.OnNext(Action<InlinePagination>)` — what each chevron
  does. **A chevron is enabled by having a handler**: passing null greys it out, which is how the first
  and the last of a set say so. The position and count only write the label, so a set that loads more
  as it goes stays in charge of when there is a next.
- `CanGoPrevious` / `CanGoNext` — whether each chevron is enabled.
- `.SetLabel(string)` — arbitrary text between the chevrons ("March", the name of the thing you are
  on) in place of the counted label. Null goes back to the count.
- `.SetFormat(Func<int, int, string>)` — how the position and count are written, for another language
  or for `3 / 7`.
- `.SetTooltips(string previous, string next)` — what each chevron is called for a screen reader and
  in its tooltip; "Previous" and "Next" by default.

Figures are tabular, so stepping through a set never shuffles the chevrons sideways.

## Example

```csharp
using static Tesserae.UI;

var pager = InlinePagination(index + 1, results.Count);

void Step(InlinePagination p, int by)
{
    index += by;
    Show(results[index]);

    p.SetPosition(index + 1, results.Count)
     .OnPrevious(index > 0                 ? new Action<InlinePagination>(x => Step(x, -1)) : null)
     .OnNext    (index < results.Count - 1 ? new Action<InlinePagination>(x => Step(x, +1)) : null);
}

pager.OnPrevious(p => Step(p, -1)).OnNext(p => Step(p, +1)).SetTooltips("Previous result", "Next result");
```

## Related

- Pagination — numbered page buttons for a list that pages — `pagination.md`
- OmniResult — its modal header uses this for previous/next through the results — `omni-result.md`
- Full docs & API: `/tesserae/components/inline-pagination`
