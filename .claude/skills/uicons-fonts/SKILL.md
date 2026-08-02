---
name: uicons-fonts
description: Update the bundled UIcons webfonts and the UIcons enum, and re-bake the optical centering into the glyph outlines. Use when bumping the icon set, when an icon looks off centre, when icons that should overlap have drifted apart, or when touching Build.UpdateInterfaceIcons.
---

# Updating the UIcons fonts

`Build.UpdateInterfaceIcons` owns the whole icon-font lifecycle. Nothing under
`Tesserae/tps/assets/fonts/`, nothing in `Tesserae/src/Icons/UIcons.cs`, and none of the
`Tesserae/tps/assets/css/uicons-*.css` files is hand-written — they are all output.

```bash
dotnet run --project Build.UpdateInterfaceIcons        # download, regenerate, re-centre
dotnet run --project Build.UpdateInterfaceIcons -- --help
```

The only prerequisite is a Chromium for Playwright. The font surgery is C# — `Woff2File.cs`,
`TransformedGlyf.cs` and `CmapLookup.cs` — so nothing has to be installed for it.

The run does four things in order, and the order matters:

1. Downloads the woff2 and css for all nine weights from Flaticon.
2. Rewrites the `uicons-*.css` files and regenerates the `UIcons` enum.
3. Measures every glyph of every weight in headless Chromium and works out how far each one is
   from being optically centred in the box the browser lays it out in.
4. Shifts those glyph outlines inside the woff2 files, then re-measures the patched fonts to prove
   the correction landed.

It exits non-zero if any check fails, and only writes the `uicons-source.txt` marker on success, so a
failed run leaves the next one to start over rather than treating half-finished fonts as done.

## It only runs when the icon set changed

Bumping icons is rare and the run takes minutes, so it is gated. After downloading, the version
plus a hash of every woff2 is compared against `Build.UpdateInterfaceIcons/uicons-source.txt`; if they
match, the run stops without touching anything. `--force` overrides it.

The fonts in the tree no longer match the vendor bytes — their outlines have been shifted — which is
exactly why the marker records what was *downloaded* rather than hashing what is in the tree.

## Why the centering is in the font and not in css

It used to be a generated stylesheet, `tss.uicons.adjustments.css`, that nudged icons with
`position: relative` and an em offset. Measurement killed that approach:

- The browser rounds a paint-time offset to a whole CSS pixel. Tesserae's default icon size is
  13&nbsp;px (`TextSize.Small`), where a 0.02–0.035&nbsp;em nudge is under half a pixel and so did
  **nothing at all** — only 6% of the offsets moved anything at that size.
- It rounds the *accumulated* position, not the offset, so the same icon moved a full pixel in one
  container and not at all in another, depending on where its box landed on the pixel grid.

An offset baked into the outline is part of the shape the rasterizer draws, so it survives at any
size and does not depend on the container. Measured on `baby-carriage` at 13/16/20/24&nbsp;px:
wanted −0.52/−0.64/−0.80/−0.96&nbsp;px, the stylesheet gave 0/0/−1/−1, the baked font gives
−0.52/−0.63/−0.79/−0.96.

## How a glyph gets moved

`TransformedGlyf` edits the fonts in the woff2 glyph encoding rather than round-tripping them
through plain `glyf`. In that encoding coordinates are deltas from the previous point, so moving a
glyph means rewriting its *first* point and nothing else — every other glyph, the declared bounding
boxes, the side bearings, the instructions and every other table stay as the vendor shipped them,
byte for byte. `Woff2File` only re-compresses the Brotli block around the one table that changed.

## Two traps in these fonts

Both were found the hard way, and both are enforced in the code:

- **The declared metrics disagree with the outlines.** A glyph whose ink starts at x=75 is declared
  with `xMin=0` and `hmtx` lsb 0, and the rasterizer places the glyph from the *declared* box —
  `rendered_x = outline_xMin - declared_xMin + lsb`. Any tool that recomputes those boxes (fontTools
  does by default on save) moves every glyph by tens of units.
- **The em square is 300 units**, so an offset lands on a whole unit: 1/300&nbsp;em, shifting the
  intended value by at most 0.0017&nbsp;em. Fine, but it is why offsets are not infinitely precise.

`measureText().actualBoundingBox*` reads from the declared boxes, so it is useless for checking this
font. Measure pixels instead.

## Three container rules that will silently produce a broken font

The woff2 container has consistency rules that a font library reads straight past and a browser does
not, and a font Chromium rejects renders as nothing at all. All three cost a diagnosis:

- **A glyph with no bounding box of its own has one computed from its points.** Move the points and
  the computed box moves with them, the two cancel, and the glyph renders exactly where it did
  before — the edit does nothing. So a glyph being moved that has no explicit box is given one
  first, computed from where its points were *before* the move (`PinDownBoundingBoxes`).
- **The un-transformed length of `glyf` has to be recomputed.** The directory declares how big the
  table is once the decoder rebuilds it, and a moved glyph's first delta can change how many bytes
  it needs. `ReconstructedLength()` models that rebuild — a fixed header, contour ends,
  instructions, run-length compressed flags whose bits depend on the deltas, the deltas, each glyph
  padded to four — and the run refuses to continue unless it reproduces what the vendor declared for
  the untouched font.
- **The file must end on a four byte boundary.** The decoder rounds the end of the compressed block
  up to four to find what follows, so an unpadded file looks like it runs off its own end. This one
  hits three fonts in four, which reads as a mysterious per-font bug rather than a missing pad byte.

## Icons that have to stay registered with each other

The point of `AlignmentGroups.cs` and the rules around it: a checkbox is drawn on the same square as
`square`, and the toolkit swaps one for the other in place, so they must overlap exactly. Three
rules handle the general case, in increasing priority:

- **Lookalikes** — matching ink boxes *and* already agreeing on where their centre is — share one
  offset, so rounding cannot separate them. Agreement is what identifies a lookalike: `circle` and
  `square` have identical ink boxes, as do thousands of unrelated icons drawn edge to edge.
- **State variants** — `X-slash`, `X-crossed`, `X-off`, `X-mute`, `X-muted`, `X-disabled` — take the
  offset of the `X` they are a state of. 464 such pairs exist.
- **Frame families** — icons sharing an ink box *and* a shape word (`square`, `circle`, `rectangle`,
  `hexagon`, `octagon`, `diamond`, `triangle`) — are drawn on the same frame, so if they cannot agree
  on one offset, none of them is moved.

`AlignmentGroups.All` then names only what no rule can derive, because the names have nothing in
common: `square`/`checkbox`/`square-a`, `toggle-on`/`toggle-off`, `lock`/`unlock`/`lock-open-alt`,
the mirrored pairs, and `slash`, which is composited over other icons and so is never moved at all.

**If you add an icon to a group, or add a group, re-run the tool** — the groups are inputs to the
measurement, not annotations on it.

## What the pass deliberately does not fix

Icons further than the cap (0.04&nbsp;em) off centre are left exactly as drawn, because at that
distance the asymmetry is usually the drawing, not a mistake: `circle-half`, `heart-half`,
`tally-1`, `signal-bars-weak`, `crate-empty`, `window-minimize`. Half-correcting those looks worse
than leaving them. Around 2,200 glyphs fall in this bucket and the run lists the extremes.

## Checks that fail the run

- No glyph may end up further off centre than rounding explains. The exceptions are the icons that
  deliberately give up their own centering to stay registered with another one.
- Icons that must overlap may not drift apart, measured on their ink boxes.
- A set of icons pinned to one offset must actually all have it.
- The rebuilt size of every `glyf` table must match what the font declares, before anything is moved.
- The browser must decode all nine patched fonts. Checked on its own, before re-measuring, because a
  font it rejects would otherwise show up as every glyph being wildly off centre.
- After patching, every adjusted glyph must measure as centred.

## In CI

It runs on every build, in the `update uicons` step of
[`.azure-devops/build-nuget-h5.yml`](../../../.azure-devops/build-nuget-h5.yml), and that is safe
because the gate and the commit-back make it self-limiting: the run that sees a new icon set does the
work and commits the patched woff2 files, the enum, the stylesheets and `uicons-source.txt` back to
master with `[skip ci]`; every build after that finds the marker matching and exits in seconds.

The one thing the step depends on, installed by the step before it, is a Chromium for Playwright, via
the `playwright.ps1` that the Microsoft.Playwright package drops in the build output — so the project
has to be built before the browser can be installed.

The commit is conditional on `git diff --cached --quiet`, because on the common run nothing changes
and `git commit` would otherwise fail the step. A `uicons-source.txt` change in the diff is the
signal that the fonts were actually rebuilt.
