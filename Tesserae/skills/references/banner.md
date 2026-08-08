---
name: banner
description: A notice strip with an icon tile, a title and badge, a message, an action button and a dismiss - in Primary, Secondary, Success, Warning or Danger tones. Renders inline or floats as a Toast in a Tesserae (C#/Transpose) app.
---

# Banner

A `Banner` is a notice the user should read but doesn't have to answer: an
`IconTile` saying what kind of notice it is, a title with an optional badge, a
message under it, an action at the far end and a dismiss button after that.

It is a plain `IComponent`, so it renders wherever you put it — at the top of a
page, in a card, above a list. It is also exactly what a `Toast` floats over the
page: `Toast().Show(banner)`. The same strip reads the same in both places.

For a larger, centred empty-state block with an illustration, use `Message`
instead; for a persistent inbox of notices, `NotificationCenter`.

## Create

`UI.Banner(string title = null, string message = null)` — either may be left out.
Also `new Banner(...)`. Bring factories into scope with `using static Tesserae.UI;`.

## Tones

`.Secondary()` (default), `.Primary()`, `.Success()`, `.Warning()`, `.Danger()` —
or `.Style(BannerStyle)`. Every colour the strip draws is derived from that one
accent: the wash behind it, its border, the tile, the badge and the text. Each
tone also brings a default icon (`Info`, `CheckCircle`, `TriangleWarning`,
`CircleXmark`), which any `SetIcon` call replaces.

`.CurrentStyle` reads the tone back.

## Key configuration

- `.SetTitle(string)` / `.SetTitle(IComponent)` — the bold first line. Null or empty drops the line.
- `.SetText(string)` / `.SetText(IComponent)` — the message under it. Null or empty drops the line.
- `.SetBadge(string)` / `.SetBadge(IComponent)` — a pill beside the title: the reference the notice is about.
- `.SetIcon(UIcons, color, weight)` / `.SetIcon(string text, color, TextSize?)` / `.SetIcon(IComponent, color)` — the leading tile. Without a colour it follows the banner's tone.
- `.NoIcon()` — drop the tile.
- `.Action(string text, Action onClick)` — a button at the far end, drawn in the banner's own tone.
- `.Action(IComponent)` — whatever you build instead: a pair of buttons, a link, a dropdown.
- `.OnDismiss(Action onDismiss, bool hide = true)` — show the `[x]`; pressing it runs the handler and takes the banner out of the page. Pass `hide: false` when something else removes it.
- `.Dismiss()` — dismiss programmatically, as though the `[x]` had been pressed.
- `.Compact(bool = true)` — tighter strip, smaller tile.
- `.Flat(bool = true)` — no rounding, no side rules, for one pinned edge to edge across a page.

## Example

```csharp
using static Tesserae.UI;

// Inline
var notice = Banner("1 new DAILY MUST criterion in your queue",
                    "High-priority items require immediate pre-qualification.")
    .Danger()
    .SetIcon(UIcons.Flame)
    .SetBadge("PAH1.1.6")
    .Action("Review now", () => OpenQueue())
    .OnDismiss(() => Remember("queue-banner-dismissed"));

// The same strip, floated over the page
Toast().Show(Banner("Export finished", "18 documents, 42 MB.")
    .Success()
    .SetIcon(UIcons.Download)
    .Action("Download", () => StartDownload()));

// Edge to edge across the top of the page
Toast().TopFull().Banner().Show(
    Banner("Scheduled maintenance tonight, 23:00 – 01:00 UTC",
           "Search stays available; indexing is paused for the window.")
        .Primary().Flat());
```

## Shown as a toast

`Toast().Show(banner)` hooks the banner's dismiss button to the toast's own hiding,
chained *after* whatever `OnDismiss` handler you already set — so the `[x]` closes
the toast and your handler still runs. Whether there is a button at all follows the
toast's settings: an edge-to-edge banner follows its `showHideButton`, an ordinary
toast shows one unless `NoDismiss()` said it cannot be dismissed at all.

## Related

- Toast — floats a banner over the page — `toast.md`
- IconTile — the leading tile — `icon-tile.md`
- Message — the larger empty-state block — `message.md`
- NotificationCenter — a persistent inbox rather than a strip — `notification-center.md`
- Full docs & API: `/tesserae/components/banner`
