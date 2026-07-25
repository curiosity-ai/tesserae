---
name: pixel-avatar
description: An animated pixel-art cat avatar drawn as one absolutely-positioned div per pixel, with eight coat designs and thirteen animations, attachable to any other component. Use when adding a small animated mascot or decorative character to a Tesserae (C#/Transpose) app.
---

# PixelAvatar

`PixelAvatar` renders a 10x8 pixel-art sprite as a grid of absolutely positioned square
divs. The artwork lives in the library as a byte grid of palette indices
(`PixelAvatarSprites`), so all eight designs share the same frames and differ only in
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
- `.Facing(PixelAvatarFacing)` — `Right` (the artwork's own direction) or `Left` to mirror it.
- `.Speed(double)` — playback multiplier; values above 1 play faster.
- `.Play(PixelAvatarAnimation)` — restart on a new animation. `.Pause()` / `.Resume()` / `.IsPaused`, and `.GoToFrame(int)` to hold a specific frame.
- `.SetDesign(PixelAvatarDesign)` — swap the coat. `.SetPalette(PixelAvatarPalette)` takes a custom palette of eleven CSS colors for indices 1..11.
- `.OnAnimationStarted((avatar, animation) => ...)` / `.OnAnimationFinished((avatar, animation) => ...)` — the second fires when a non-looping animation reaches its last frame, just before its follow-up takes over; calling `Play` from the handler suppresses that hand-over.

`PixelAvatarDesign`: `Black`, `Orange`, `White`, `Beige`, `Siamese`, `SpottedGrey`,
`SpottedOrange`, `Tuxedo`. `PixelAvatarPalettes.All` enumerates them and
`PixelAvatarPalettes.Get(design)` returns the palette.

`PixelAvatarAnimation`: `Move`, `Idle`, `Interact`, `JumpUp`, `JumpDown`, `Startle`,
`Stretch`, `Sit`, `SitIdle`, `Crouch`, `CrouchIdle`, `Sleep`, `SleepIdle`.
`PixelAvatarSprites.All` enumerates them and `PixelAvatarSprites.Get(animation)` returns
the frames, frame duration, and whether it loops.

`Move`, `Idle`, `SitIdle`, `CrouchIdle` and `SleepIdle` loop forever. The rest play once
and hand over: `Sit` settles into `SitIdle`, `Crouch` into `CrouchIdle`, `Sleep` into
`SleepIdle`, `Stretch` into `Sit`, `JumpUp` into `JumpDown`, and `Interact`, `JumpDown`
and `Startle` return to `Idle`.

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
