---
name: tesserae-benchmarking
description: >-
  Measure Tesserae's rendering cost and prove a change did not alter what the user sees, using the
  Tesserae.Bench harness (a ten-page app shaped like a real product, driven by Playwright under a CDP
  CPU profile). Use this WHENEVER the task is to make the toolkit faster or leaner, to find where
  build time or memory goes, to A/B two builds, or to verify that a change to a layout container,
  a component or a stylesheet did not change rendering. Triggers on "is this faster", "profile
  Tesserae", "why is this page slow", "did this break the layout", "compare before and after",
  "benchmark", or any change to Stack/Grid, the sizing extensions, or tps/assets/css.
---

# Benchmarking and visual regression

Everything lives in `Tesserae.Bench/` (see its README for what the app is and how to serve it).
Scripts run from `Tesserae.Bench/playwright/`.

## Pick the right comparison — this is the part people get wrong

| The change… | Compare with | Why |
|---|---|---|
| leaves the DOM shape alone (a style value, an algorithm, a cache) | `compare.js` / `compare-samples.js` — geometry | Exact, cheap, names the element |
| adds, removes or re-parents elements | `textdiff-samples.js` — text-run positions | The text a user reads is the same list in the same order however the tree is rearranged underneath, so it stays comparable, and it classifies what moved |
| repaints without moving anything (colour, border, shadow) | `pixdiff.js` + `diffimg.js` — pixels | The only one that sees paint |

A worked example of the first trap: removing the `tss-stack-item` wrapper made every element's own
box change (the child absorbs the wrapper's size and position) while the page rendered identically.
Geometry called it a total regression. And the trap in the other direction: on that same change
`pixdiff.js` reported all 131 samples differing, but most of that was a deliberate 2–3px change in
vertical rhythm re-rendering every glyph on the page — plus two samples that generate random colours
and three that print a live clock. A percentage cannot tell those apart from a broken layout.

`textdiff-samples.js` is the gate that can. It matches runs by their content and reports, per sample:

- `COUNT` — the two builds render a different number of text runs; content appeared or vanished
- `TEXT` — the runs diverge in order; a structural break (or a live clock)
- `X/W` — a run moved horizontally or changed size: **this is where real regressions show up**
- `Y` — a run moved vertically only, which a change to spacing or line boxes is expected to do

Work the `X/W` list to zero (or to differences you can each name and defend), read the `Y` list for
anything absurd, then use `diffimg.js` on the two or three worst pages to confirm by eye.

**Never trust one run.** Establish the noise floor first by comparing a build against *itself* —
`textdiff-samples.js --a <url> --b <same url>` captures twice in separate browsers and shows exactly
which samples disagree with themselves (Masonry, and the clock pages). `compare-samples.js` does the
same for geometry. For `pixdiff.js`, capture the same build twice and diff those.

## Measuring

```bash
# one build
node bench.js --url http://127.0.0.1:5099/index.html --label mine
node analyze.js out/mine-interaction.cpuprofile 30      # self + inclusive time, by function and file
node profile-build.js admin --url ... --label mine      # one page's build in isolation, off-screen

# A/B: interleave the runs, never all of A then all of B — the machine drifts
for i in 1 2; do
  node bench.js --url http://127.0.0.1:5086/index.html --label before$i
  node bench.js --url http://127.0.0.1:5090/index.html --label after$i
done
```

Take **best-of**, not mean: you are measuring a floor, and the noise is one-sided.

Read the numbers honestly. `bench.js`'s session wall-clock is dominated by fixed waits between
interactions, so a real build-cost win of 30% can leave it flat. Say "build cost", not "the app got
30% faster", unless the session number moved too.

To attribute time inside the toolkit, walk the profile's call tree up from a hot runtime frame to
the first `tss.js` frame — that names the component or helper responsible. `analyze.js` gives the
totals; the tree is what tells you whose fault it is.

## Verifying rendering

```bash
# the whole gallery — 131 samples, classified. This is the gate for a layout change.
node textdiff-samples.js --a http://127.0.0.1:5083/index.html --b http://127.0.0.1:5090/index.html
node textdiff-samples.js --a ... --b ... --only Banner --verbose   # every differing run in one sample

# bench pages: capture both builds (writes A-*.png / B-*.png), then diff
node compare.js --a http://127.0.0.1:5086/index.html --b http://127.0.0.1:5090/index.html
node pixdiff.js out A B                       # % differing, and the bounding box of the change
node diffimg.js out/A-form.png out/B-form.png diff-form.png   # look at it

# gallery screenshots, for diffimg / pixdiff
node capture-samples.js --url http://127.0.0.1:5083/index.html --prefix SA
node capture-samples.js --url http://127.0.0.1:5090/index.html --prefix SB --only "Banner,Label"
node pixdiff.js out SA SB

node all-samples.js --url ...   # opens every sample, reports console/page errors
node smoke.js --url ...         # clicks, toggles, types, opens a dropdown
```

`all-samples.js` and `smoke.js` run against the **samples gallery**, not the bench app (they reach
components by sidebar label). Both are **error gates, not rendering gates** — a sample that renders
wrong but throws nothing passes both. The gallery is where the toolkit's real surface is, and it
exercises components (SectionStack on every page, InlineLabel, PropertyGrid) the bench app does not,
so a layout change has to be checked there as well as on the bench pages.

`diffimg.js` is the fastest way to understand a non-zero pixel diff: the greyed page with changes in
red shows *what* moved, which a percentage never does. An accumulating drift down the page means a
per-row size change; a uniform offset means something above shifted.

## Attributing a difference to one cause

`inspect.js` is the step after the diff — it prints an element's box and every layout property that
decides it, up its ancestors. Run it against both builds and read the two side by side; the property
that differs is the cause:

```bash
node inspect.js --url http://127.0.0.1:5083/index.html --sample Banner --text "Review now"
node inspect.js --url ... --sample Binding --text "volume = " --siblings   # who is pushing it sideways
node inspect.js --url ... --sample "Pages Stack" --selector .tss-pagesstack-holder --limit 40
```

Measure before reading CSS. Every wrong guess in this area was a plausible stylesheet reading —
`.tss-label + .tss-label` looked like the obvious cause of a label drift and was not; the actual
cause (`.tss-label { margin: 0 }` beating the standard margin utility) was two `inspect.js` runs away.

When measurement is ambiguous, neutralise one suspect at a time, rebuild, re-diff. If the number does
not move, it was not that — put the suspect back rather than leaving a speculative edit in the tree.

Scratch probes belong in `playwright/_something.js`, which is gitignored, so a one-question script
never ends up committed next to the harness.

## Watch for

- **Animated surfaces need to settle.** Chat animates messages in and anchors scroll; charts and
  carousels animate. Capture too early and you diff a mid-animation frame. The scripts wait ~1.2–1.6s
  and pin scroll containers to the top; keep that if you add pages.
- **Rebuild what you measure.** `dotnet build` on the bench project does not rebuild the toolkit's
  CSS into an already-built site — wipe `bin`/`obj` when a stylesheet changed. `tps` also needs to be
  on `PATH` (`export PATH="$PATH:$HOME/.dotnet/tools"`) or the build fails with exit code 127.
- **Re-serve after a rebuild.** The build replaces the output directory, so an `http-server` started
  in the old one keeps serving the deleted inode and you diff a stale site. Restart it, then confirm
  with `curl -s <url>/assets/css/tss.css | grep -c <something you just added>` — the stylesheets are
  bundled into one `tss.css`, so asking for `tss.common.css` by name always 404s.
- **Some samples disagree with themselves.** Two generate random colours per run (Keyed Observable
  Stack), three print a live clock (Date Time Picker, Pivot, Time Histogram Picker) and Masonry lays
  out asynchronously. They will always show a diff; the self-comparison is what proves it is them.
- **The harness is per-page.** Adding a page means adding it to `PAGES` in `bench.js` *and*
  `compare.js`, and adding a nav button with a stable `#nav-<id>` in `src/App.cs`.
