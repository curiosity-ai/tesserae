---
name: pixel-avatar
description: An animated pixel-art cat avatar drawn as one absolutely-positioned div per pixel, with eleven coat designs and thirteen animations, attachable to any other component. Use when adding a small animated mascot or decorative character to a Tesserae (C#/Transpose) app.
---

# PixelAvatar

`PixelAvatar` renders a 10x8 pixel-art sprite as a grid of absolutely positioned square
divs. The artwork lives in the library as a byte grid of palette indices
(`PixelAvatarSprites`), so all eleven designs share the same frames and differ only in
their `PixelAvatarPalette` — switching design rewrites eleven CSS variables and repaints
nothing.

It is decorative, not a user representation. For user images and initials use
`Avatar` / `Persona` instead (`avatar.md`).

## Create

`UI.PixelAvatar(PixelAvatarDesign design = PixelAvatarDesign.Black, PixelAvatarAnimation animation = PixelAvatarAnimation.Idle)`.
Bring factories into scope with `using static Tesserae.UI;`.

The animation timer only runs while the avatar is mounted, so an avatar inside a hidden
tab or a removed subtree costs nothing.

## Key configuration

- `.PixelSize(int)` — CSS pixels per sprite pixel; default `PixelAvatar.DefaultPixelSize` (4), giving a 40x32 avatar. `RenderedWidth` / `RenderedHeight` report the result.
- `.Facing(PixelAvatarFacing)` — `Right` (the artwork's own direction) or `Left` to mirror it, instantly. `FacingValue` reads it back.
- `.Turn(PixelAvatarFacing, int durationMs = PixelAvatar.DefaultTurnDurationMs)` / `.TurnAround(...)` — change direction by pivoting the sprite about its vertical axis under a perspective scaled to the avatar's own width, so it reads as the cat physically turning rather than its pixels swapping sides. Turning to the direction it already faces does nothing.
- `.Speed(double)` — playback multiplier; values above 1 play faster.
- `.Play(PixelAvatarAnimation)` — restart on a new animation. `.Pause()` / `.Resume()` / `.IsPaused`, and `.GoToFrame(int)` to hold a specific frame.
- `.SetDesign(PixelAvatarDesign)` — swap the coat. See **Custom palettes** below for the ways to supply your own colors.
- `.Outline(bool = true)` — a hairline halo in the theme's contrasting color, **on by default**. Several palettes contain pure white (`White`, `SpottedGrey`, `SpottedOrange`) and several near-black (`Black`, `Tuxedo`, `Siamese`), so without it those designs disappear against one theme or the other. `.OutlineColor(string)` overrides the color, which defaults to translucent black in light mode and translucent white in dark mode.
- `.OnAnimationStarted((avatar, animation) => ...)` / `.OnAnimationFinished((avatar, animation) => ...)` — the second fires when a non-looping animation reaches its last frame, just before its follow-up takes over; calling `Play` from the handler suppresses that hand-over.

`PixelAvatarDesign`: `Black`, `Orange`, `White`, `Beige`, `Siamese`, `SpottedGrey`,
`SpottedOrange`, `Tuxedo` (extracted from the source sprite sheets), plus `Grey`, `Sparkle`
(violet with magenta markings) and `Lynx` (tawny with dark ear tufts and spots), which are
authored against the same palette indices. `PixelAvatarPalettes.All` enumerates them and
`PixelAvatarPalettes.Get(design)` returns the palette.

`PixelAvatarAnimation`: `Move`, `Idle`, `Interact`, `JumpUp`, `JumpDown`, `Startle`,
`Stretch`, `Sit`, `SitIdle`, `Crouch`, `CrouchIdle`, `Sleep`, `SleepIdle`.
`PixelAvatarSprites.All` enumerates them and `PixelAvatarSprites.Get(animation)` returns
the frames, frame duration, and whether it loops.

`Move`, `Idle`, `SitIdle`, `CrouchIdle` and `SleepIdle` loop forever. The rest play once
and hand over: `Sit` settles into `SitIdle`, `Crouch` into `CrouchIdle`, `Sleep` into
`SleepIdle`, `Stretch` into `Sit`, `JumpUp` into `JumpDown`, and `Interact`, `JumpDown`
and `Startle` return to `Idle`.

## Custom palettes

A palette is eleven CSS colors, one per palette index. The indices are ordered by shading
level, so each shade is a contiguous run — `1..PixelAvatarSprites.LastHighlightIndex` (3) is
the highlight, up to `LastBaseIndex` (9) the base, and the rest the shadow.
`PixelAvatarSprites.ShadeOf(byte)` returns a `PixelAvatarShade` (`Highlight`, `Base`,
`Shadow`) for an index. That grouping is why the single-hue built-in designs are just three
colors repeated, and why three colors are enough to describe a whole coat.

On the avatar:

- `.SetPalette(PixelAvatarPalette)` — apply a palette object.
- `.SetPalette(string colors, string name = "Custom")` — import from a list of CSS colors separated by commas, semicolons or whitespace. Unparseable input is ignored.
- `.SetShades(string highlight, string baseColor, string shadow, string name = "Custom")` — build a coat from the three shading levels.
- `.SetColor(byte index, string color)` — recolor a single index. This only rewrites that index's CSS variable, so it is cheap enough to drive from a color picker's input event.

On `PixelAvatarPalette` (immutable):

- `PixelAvatarPalette.Parse(string colors, string name = "Custom")` — accepts either all eleven colors or exactly three (read as highlight/base/shadow). Returns `null` for anything else, so an editor can report a bad paste instead of rendering a broken cat.
- `PixelAvatarPalette.FromShades(highlight, baseColor, shadow, name)` — the three-color form.
- `.WithColor(byte index, string color)` / `.WithName(string)` — return modified copies.
- `.Adjust(int hueDelta, int saturationDelta, int lightnessDelta)` — shift every color together in HSL space, which keeps the shading relationships that make the sprite read as one coat. The deltas are relative to the palette you call it on, so all-zero returns the same colors and a UI can re-apply from the unshifted palette on every slider move instead of accumulating drift. Hue wraps in degrees; saturation and lightness are percentage points and clamp at 0 and 100.
- `.ColorAt(byte index)` — the color for an index (empty string for the transparent index 0).
- `.ToString()` — the comma-separated color list that `Parse` reads back.
- `.ToCode()` — C# source that reconstructs the palette, for pasting into an application.

```csharp
// Three colors are enough for a whole coat.
var mint = PixelAvatar(PixelAvatarDesign.White)
   .SetShades("#D6F5E3", "#8FD9B6", "#3F8F6E", "Mint");

// Round-trips through text.
var copied = mint.Palette.ToString();          // "#D6F5E3, #D6F5E3, ..."
var back   = PixelAvatarPalette.Parse(copied); // null if it isn't 11 or 3 colors
```

## Attaching to another component

`.AttachTo(IComponent target, PixelAvatarAnchor anchor = PixelAvatarAnchor.TopLeft)` — or
the equivalent extension `target.WithPixelAvatar(avatar, anchor)` — returns a
`PixelAvatarAttachment` that renders the target with the avatar perched on one of its
edges.

- `PixelAvatarAnchor` values: `TopLeft`, `TopCenter`, `TopRight`, `BottomLeft`, `BottomCenter`, `BottomRight`, `LeftCenter`, `RightCenter`.
- `.Anchor(PixelAvatarAnchor)` — move it to a different edge later.
- `.Offset(int x, int y)` — nudge it in CSS pixels; positive values move right and down.
- `.Overlap(bool = true)` — by default the wrapper reserves room for the avatar on the anchored side, so it can never be clipped by a scrolling ancestor. Overlap mode drops that room and lets the avatar hang outside the wrapper, keeping the target's footprint identical to the bare component — but any ancestor that scrolls or hides overflow will then clip it.
- `.Avatar` / `.Target` — the two wrapped components.

The attachment implements `ISpecialCaseStyling`, so sizing helpers such as `.WS()` apply
to the wrapper and the avatar stays anchored.

## Example

```csharp
using static Tesserae.UI;

// A cat sitting on top of a button, cycling animations when clicked.
var cat    = PixelAvatar(PixelAvatarDesign.Tuxedo, PixelAvatarAnimation.SitIdle).PixelSize(5);
var button = Button("Feed the cat");

cat.OnAnimationStarted((_, animation) => button.SetText($"Feed the cat ({animation})"));
button.OnClick(() => cat.Play(PixelAvatarAnimation.Interact));

var perched = cat.AttachTo(button, PixelAvatarAnchor.TopCenter);

// A larger avatar on its own, walking to the left.
var walker = PixelAvatar(PixelAvatarDesign.SpottedOrange, PixelAvatarAnimation.Move)
   .PixelSize(8)
   .Facing(PixelAvatarFacing.Left)
   .Speed(1.5);
```

## Related

- User avatars and personas — `avatar.md`
- Corner-anchored overlays in general — `float.md`
- Full docs & API: `/tesserae/components/pixel-avatar`
