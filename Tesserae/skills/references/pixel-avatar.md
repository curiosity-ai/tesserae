---
name: pixel-avatar
description: An animated pixel-art cat avatar drawn as one absolutely-positioned div per pixel, with twelve coat designs and thirteen animations, attachable to any other component. Use when adding a small animated mascot or decorative character to a Tesserae (C#/Transpose) app.
---

# PixelAvatar

`PixelAvatar` renders a 10x8 pixel-art sprite as a grid of absolutely positioned square
divs. The artwork lives in the library as a byte grid of palette indices
(`PixelAvatarSprites`), so all twelve designs share the same frames and differ only in
their `PixelAvatarPalette` — switching design rewrites eleven CSS variables and repaints
nothing.

It is decorative, not a user representation. For user images and initials use
`Avatar` / `Persona` instead (`avatar.md`).

## Create

`UI.PixelAvatar(byte key, PixelAvatarDesign design = PixelAvatarDesign.Black, PixelAvatarAnimation animation = PixelAvatarAnimation.Idle)`.
Bring factories into scope with `using static Tesserae.UI;`.

The **key** is the first argument because the artwork is not shipped in the clear: the sprite
sheet lives in the library run-length-encoded and scrambled, and the library does not carry the
key to unscramble it. Your application does, and passes it here — the first avatar you construct
decodes the sheet for every one after it. Keep it in one constant of your own:

```csharp
internal static class SpriteKey { internal const byte Value = 42; }

var cat = PixelAvatar(SpriteKey.Value, PixelAvatarDesign.Orange);
```

This is obfuscation, not security. A key the browser has to see is readable by anyone who looks;
it only keeps the artwork out of casual sight in the shipped JavaScript. Every entry point that
builds an avatar for you takes the same key: `UI.PixelAvatarBadge`, the `ChatMessage` design
overload, and `WithPixelAvatar(key, design, anchor)`. The ones handed an avatar you already
built do not.

`PixelAvatarSprites.Get` throws until some avatar has supplied the key.

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
(violet with magenta markings), `Lynx` (tawny with dark ear tufts and spots) and `Sudo`
(near-black navy with an electric blue accent on the ear tips), which are authored against the
same palette indices. `PixelAvatarPalettes.All` enumerates them and
`PixelAvatarPalettes.Get(design)` returns the palette.

`PixelAvatarAnimation`: `Move`, `Idle`, `Interact`, `JumpUp`, `JumpDown`, `Startle`,
`Stretch`, `Sit`, `SitIdle`, `Crouch`, `CrouchIdle`, `Sleep`, `SleepIdle`, plus `AutoIdle`.
`PixelAvatarSprites.All` enumerates the thirteen that have artwork and
`PixelAvatarSprites.Get(animation)` returns the frames, frame duration, and whether it loops.

`Move`, `Idle`, `SitIdle`, `CrouchIdle` and `SleepIdle` loop forever. The rest play once
and hand over: `Sit` settles into `SitIdle`, `Crouch` into `CrouchIdle`, `Sleep` into
`SleepIdle`, `Stretch` into `Sit`, `JumpUp` into `JumpDown`, and `Interact`, `JumpDown`
and `Startle` return to `Idle`.

### Resting

The three resting poses — `Idle`, `SitIdle` and `CrouchIdle` — **hold their first frame for a
random 5 to 10 seconds** before playing their cycle and settling again, rather than looping
continuously. A cat cycling three frames forever reads as fidgeting; one that holds still and
twitches every few seconds reads as resting. `PixelSpriteAnimation.Rests` / `.RestMinMs` /
`.RestMaxMs` expose it, and `.Speed()` scales the hold along with everything else.

**`AutoIdle`** is a resting *behaviour* rather than an animation: it has no artwork of its own,
starts from `Idle`, and at the end of each rest either stays put or drifts to another resting
pose, wandering between standing, sitting and crouching. Use it wherever a cat is just hanging
around; use plain `Idle` when you want exactly that pose and nothing else.

Sleeping is part of the same behaviour. Once an auto-idling cat has rested for `SleepAfter`
(60s by default) it curls up on its own — `Sleep`, then `SleepIdle` — and `Wake()` brings it
back with a `Stretch` and a `Startle` before handing it to `AutoIdle` again. Playing an
animation directly never puts a cat to sleep; only `AutoIdle` does.

While auto-idling, `CurrentAnimation` reports whichever pose is actually showing,
`IsAutoIdling` reports the behaviour, and `IsAsleep` covers `Sleep` and `SleepIdle`. Any
explicit `Play` of something else turns auto-idling off; the internal hand-overs do not.

Every delay in the flow is configurable, and every one is drawn at random inside its range or
jittered by ±20% on use, so nothing the cat does lands on a stopwatch:

- `.RestDelay(minMs, maxMs)` — overrides how long each resting pose holds its first frame. Zero for both restores the built-in 5–10s.
- `.SleepAfter(ms)` — resting time before it sleeps, jittered on use. `PixelAvatar.DefaultSleepAfterMs` is 60000; pass zero to keep it awake indefinitely.
- `.Wake()` — restarts the sleep countdown, and plays the wake-up performance if the cat was actually out.
- `.Speed(x)` — scales every frame duration and rest hold together.

## Custom palettes

A palette is eleven colors, one per palette index, plus the **background** an avatar-shaped
host such as `PixelAvatarBadge` sits the coat on. The indices are ordered by shading level, so
each shade is a contiguous run — `1..PixelAvatarSprites.LastHighlightIndex` (3) is the
highlight, up to `LastBaseIndex` (9) the base, and the rest the shadow.
`PixelAvatarSprites.ShadeOf(byte)` returns a `PixelAvatarShade` (`Highlight`, `Base`,
`Shadow`) for an index. That grouping is why the single-hue built-in designs are just three
colors repeated, and why three colors are enough to describe a whole coat.

Palettes are immutable. Every built-in design carries a hand-picked background; a custom one
either supplies its own or gets one derived from `DominantColor()`.

On the avatar:

- `.SetPalette(PixelAvatarPalette)` — apply a palette object.
- `.SetShades(Color highlight, Color baseColor, Color shadow, Color background = null, string name = "Custom")` — build a coat from the three shading levels.
- `.SetColor(byte index, Color color)` — recolor a single index. This only rewrites that index's CSS variable, so it is cheap enough to drive from a color picker's input event.

On `PixelAvatarPalette`:

- `PixelAvatarPalette.FromColors(string name, Color background, params Color[] colors)` — throws `ArgumentException` unless exactly `PixelAvatarSprites.PaletteSize` colors are given, so a short or long list fails loudly instead of rendering a broken cat. Pass a null background to derive one.
- `PixelAvatarPalette.FromShades(string name, Color background, Color highlight, Color baseColor, Color shadow)` — the three-color form.
- `.Colors` / `.ColorAt(byte)` — the colors, as `Color`. `.CssAt(byte)` gives the hex the renderer writes, or an empty string for the transparent index 0.
- `.Accent` / `.WithAccent(Color)` — an optional highlight painted on the ear tips. It is **not** a palette index: it is drawn as an extra pixel at half the avatar's `PixelSize` over each ear tip, so a design can carry a spot of color the shared artwork has no cell for. `Sudo` is the built-in that uses it. Null means no accent, which is the default.
- `.Background` — the avatar background color. `.BackgroundGradient()` turns it into CSS through `Avatar.GradientForHue`, the very method the regular `Avatar` uses for its initials, so a pixel-art badge and an initials avatar look like they came out of the same set. Only the hue is used; saturation and lightness are fixed by that formula.
- `.WithColor(byte, Color)` / `.WithBackground(Color)` / `.WithName(string)` — return modified copies. `WithBackground` is how a custom palette picks the background its badge sits on.
- `.DominantColor()` — the color covering most of the sprite, weighted by `PixelAvatarSprites.PixelCounts`. Used to derive a background when one is not given.
- `.ToString()` — the comma-separated color list. `.ToCode()` — C# source that reconstructs the palette.

On `PixelSprite`:

- `.InkLeft` / `.InkTop` / `.InkWidth` / `.InkHeight` — the bounds of a frame's non-transparent pixels. Frames share one 10x8 box so they stay aligned while animating, which means an individual pose sits wherever it sits inside it; anything centering or measuring a single frame wants these, not the box.
- `.HasEars` / `.EarY` / `.EarLeftX` / `.EarRightX` — where the ear tips are in this frame, which is what the accent follows as the animation plays. Every frame draws exactly one pixel of `PixelAvatarSprites.RightEarIndex` and it is always the right tip, with the left one `EarSpacing` cells to its left; the generator asserts that across all 43 frames, so this is a lookup rather than a silhouette guess (the topmost row will not do — in several poses the raised tail reaches it too).

```csharp
// Three colors and a background are enough for a whole coat.
var mint = PixelAvatar(SpriteKey.Value, PixelAvatarDesign.White).SetShades(
    Color.FromString("#D6F5E3"), Color.FromString("#8FD9B6"), Color.FromString("#3F8F6E"),
    background: Color.FromString("#B5762E"), name: "Mint");

// Recolor one index, or restyle the badge background, without touching the rest.
var warmer = mint.Palette.WithBackground(Color.FromString("#CC5533"));
```

There is deliberately no palette parser in the library: what a half-parsed palette should mean
is an application's decision. The sample page shows one, reading either eleven colors or three
and reporting anything else, built on `FromColors` / `FromShades`.

## As a chat / profile avatar

`PixelAvatarBadge` dresses a cat as a round profile picture, sized with the same `AvatarSize`
presets as `Avatar` so the two can sit side by side. It holds `SitIdle` on its first frame and
never animates — a badge is an identity, not an animation — and paints the palette's own
`Background` behind it, through the same gradient formula the regular `Avatar` uses.

`UI.PixelAvatarBadge(byte key, PixelAvatarDesign design = Black, AvatarSize size = Medium)`, or
`UI.PixelAvatarBadge(PixelAvatar avatar, AvatarSize size = Medium)` to wrap one you already
have.

- `.Size(AvatarSize)` — `XSmall` (24px) through `XLarge` (72px). The cat is scaled and positioned from the *ink* of the pose rather than from the 10x8 frame it sits in — `SitIdle` only fills a 6x6 corner of that frame, so centering the frame would leave it visibly off-center — and sized so the diagonal of that ink fits the circle, since the corners of the pose are drawn and fitting the width alone would clip an ear against the rim.
- `.SetDesign(...)` / `.SetPalette(...)` — recolor; the background follows the palette.
- `.Background(string)` — pin a CSS background instead; pass null to go back to the palette's.
- `.Avatar` — the wrapped `PixelAvatar`.

The badge's gradient sits at a fixed mid lightness, so it also picks the contrast halo from the
coat rather than from the page theme: a dark cat gets a light halo and a light cat a dark one.

`ChatMessage` takes a cat directly and wraps it for you:

```csharp
chat.Add(ChatMessage(TextBlock("They're on the shelf."), SpriteKey.Value, PixelAvatarDesign.SpottedOrange).MaxWidth());
chat.Add(ChatMessage(TextBlock("Classic."), PixelAvatar(SpriteKey.Value, PixelAvatarDesign.Grey)).RightAligned());
```

## As an OmniBox companion

Attaching an avatar to the top of an `OmniBox` gives it a life of its own. `AttachTo` notices
the target is an `OmniBox` and the anchor is one of the `Top*` ones, and wires up a
`PixelAvatarCompanion`, reachable through `PixelAvatarAttachment.Companion`:

- Resting *and sleeping* are left to `AutoIdle`, so between activities the cat drifts between standing, sitting and crouching on its own, and curls up after a long enough silence. The companion only supplies the activity in between.
- On top of that it plays a random animation every 5–14s, picked from `Move`, `Interact`, `JumpUp`, `Startle` and `Stretch`, and waits until the cat settles back into a resting pose before scheduling the next one.
- When `Move` comes up it picks a new spot along the top edge, turns to face the way it is going, and walks there.
- Typing settles it back to `Idle` — a one-shot animation is allowed to play out first, only the looping poses are cut short.
- About 10s after your **last** keystroke the cat walks over to the text caret and watches you type; each keystroke pushes that back, and it takes priority over the spontaneous animations. Arriving, it plays `Startle` or `Interact` — picked at random, so it reads as having noticed what you are typing rather than quietly sitting down — and then the ordinary random flow resumes.
- Focusing or typing wakes a sleeping cat with `Stretch` then `Startle` rather than snapping it back to `Idle`.

```csharp
var perched = PixelAvatar(SpriteKey.Value, PixelAvatarDesign.Orange).AttachTo(omniBox, PixelAvatarAnchor.TopLeft);

perched.Companion
   .IdleDelay(8000, 20000)   // gap between spontaneous animations; floors at 5s
   .RestDelay(4000, 9000)    // how long each resting pose holds; forwards to the avatar
   .CursorDelay(6000)        // quiet time before it walks to the caret; 0 to never follow it
   .SleepAfter(120000)       // resting time before it sleeps; 0 to keep it awake
   .WalkSpeed(40);           // CSS pixels per second
```

Every one of those is jittered or drawn from its range on use. `.WakeUp()`, `.Fidget()` and
`.FollowCursor()` drive it by hand, and `.IsAsleep` reads the state.

The caret position comes from `OmniBox.CaretClientX()`, which measures whichever of the two
inputs is focused and is public, so a different companion can use it too.

## On a modal

A `Modal` is the one target that is **not wrapped**. It centers itself inside its own
full-screen container and is put on screen by `Show()`, so a wrapper around it would simply be
dropped. It lends its own box to the avatar instead: the anchored side of the modal grows by the
avatar's size and the cat perches in that band, above the header and inside the modal's rounded
corners. `PixelAvatarAttachment.IsAdopted` reports it, and `Render()` returns the modal's own
element rather than a wrapper.

Because the modal stays the component you go on using, the extension returns the modal:

```csharp
Modal("Configure the endpoint")
   .Content(editor)
   .WithPixelAvatar(SpriteKey.Value, PixelAvatarDesign.Sudo, PixelAvatarAnchor.TopLeft)
   .Width(640.px())
   .Show();
```

A `Top*` anchor gets a `PixelAvatarCompanion` here too, so the cat roams the modal's top edge
the way it roams an `OmniBox`. There is nothing to type into, so it only wanders and plays the
odd animation — `CursorDelay` is ignored. Use `avatar.AttachTo(modal, anchor)` instead of the
extension when you need the attachment back to reach that companion.

Keep the reserved room on a modal: `.Overlap()` hangs the avatar outside the box, and a modal
scrolls its own content, so an overlapping cat is clipped.

## Attaching to another component

`.AttachTo(IComponent target, PixelAvatarAnchor anchor = PixelAvatarAnchor.TopLeft)` — or
the equivalent extension `target.WithPixelAvatar(avatar, anchor)` — returns a
`PixelAvatarAttachment` that renders the target with the avatar perched on one of its
edges. An `OmniBox` or `Modal` target with a `Top*` anchor also gets a companion — see above.

- `PixelAvatarAnchor` values: `TopLeft`, `TopCenter`, `TopRight`, `BottomLeft`, `BottomCenter`, `BottomRight`, `LeftCenter`, `RightCenter`.
- `.Anchor(PixelAvatarAnchor)` — move it to a different edge later.
- `.Offset(int x, int y)` — nudge it in CSS pixels; positive values move right and down.
- `.Overlap(bool = true)` — by default the wrapper reserves room for the avatar on the anchored side, so it can never be clipped by a scrolling ancestor. Overlap mode drops that room and lets the avatar hang outside the wrapper, keeping the target's footprint identical to the bare component — but any ancestor that scrolls or hides overflow will then clip it.
- `.Avatar` / `.Target` — the two wrapped components.
- `.IsAdopted` — whether the target lent its own box instead of being wrapped, which is what a `Modal` does.

The attachment implements `ISpecialCaseStyling`, so sizing helpers such as `.WS()` apply
to the wrapper and the avatar stays anchored.

## Example

```csharp
using static Tesserae.UI;

// A cat sitting on top of a button, cycling animations when clicked.
var cat    = PixelAvatar(SpriteKey.Value, PixelAvatarDesign.Tuxedo, PixelAvatarAnimation.SitIdle).PixelSize(5);
var button = Button("Feed the cat");

cat.OnAnimationStarted((_, animation) => button.SetText($"Feed the cat ({animation})"));
button.OnClick(() => cat.Play(PixelAvatarAnimation.Interact));

var perched = cat.AttachTo(button, PixelAvatarAnchor.TopCenter);

// A larger avatar on its own, walking to the left.
var walker = PixelAvatar(SpriteKey.Value, PixelAvatarDesign.SpottedOrange, PixelAvatarAnimation.Move)
   .PixelSize(8)
   .Facing(PixelAvatarFacing.Left)
   .Speed(1.5);
```

## Related

- User avatars and personas — `avatar.md`
- Corner-anchored overlays in general — `float.md`
- Full docs & API: `/tesserae/components/pixel-avatar`
