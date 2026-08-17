# Tesserae.Bench

A benchmark and visual-regression harness for the toolkit. It exists to answer two questions that
the sample gallery cannot:

1. **What does Tesserae cost?** — how long a realistic screen takes to build, how much memory and
   how many DOM nodes it leaves behind, and which functions inside the toolkit the time goes to.
2. **Did a change alter what the user sees?** — compared against another build of the same app,
   down to the pixel.

It is a benchmark, not a demo. `Tesserae.Tests` is the gallery that shows every component; this is a
small app shaped like a *product*, driven by a script, measured under a CPU profiler.

## Why it is shaped the way it is

The ten pages are not arbitrary. Five are generic stress shapes (dense table, long card list, big
form, chart dashboard, overlays). The other five are modelled on the Mosaik front end, after taking
a census of which components that codebase actually uses:

```
TextBlock 2257   Button 1312   HStack 1017   VStack 964   Tooltip 892
Toast 571        Label 547     Icon 390      Defer/DeferSync 503   Raw 298
TextBox 213      Modal 200     Dropdown 136  Dialog 115    Pivot 109
```

The first five pages exercised almost none of the tooltip, defer or toast surface, which is where a
real app spends much of its build time — so `search`, `tooltips`, `defer`, `chat` and `admin` were
added to cover it. If you are adding a page, do it the same way: find the shape in a real app first.

The app exposes `window.__bench` (`go`, `build`, `churnDefer`, `burstToasts`) so a page can time its
own work without Playwright round-trips in the measurement, and every nav button has a stable id.

## Layout

```
src/App.cs             the benchmark app — ten pages, plus the window.__bench hooks
tps.json               Transpose config (reflection off; this is not a reflection test)
playwright/            the harness, see the skill for what each script is for
playwright/out/        screenshots, .cpuprofile traces, result JSON (gitignored, tens of MB)
playwright/_*.js       scratch probes: one question, then thrown away (gitignored)
```

The scripts split into three jobs:

| | |
|---|---|
| `bench.js` `analyze.js` `profile-build.js` | measure — wall clock, heap, DOM nodes, CPU profile |
| `textdiff-samples.js` `compare.js` `compare-samples.js` `pixdiff.js` `capture-samples.js` `diffimg.js` | compare two builds |
| `inspect.js` | explain one difference: an element's box and the properties behind it |
| `all-samples.js` `smoke.js` | error gates over the samples gallery |

## Running it

The project is deliberately **not** in `Tesserae.sln`: it is built on demand, so a normal
`dotnet build` at the repo root stays fast.

```bash
dotnet build Tesserae.Bench/Tesserae.Bench.csproj
cd Tesserae.Bench/bin/Debug/netstandard2.0/tps && npx http-server -p 5099 -s
```

The Playwright scripts need a `playwright` module resolvable from `playwright/`. On a machine with a
global install, symlink it once (it is gitignored):

```bash
ln -s "$(npm root -g)" Tesserae.Bench/playwright/node_modules
```

They expect Chromium at `/opt/pw-browsers/chromium`; change `executablePath` if yours lives
elsewhere.

Then, from `Tesserae.Bench/playwright/`:

```bash
node bench.js --url http://127.0.0.1:5099/index.html --label mine   # measure
node analyze.js out/mine-interaction.cpuprofile                     # where did the time go
```

The `tesserae-benchmarking` skill covers the full workflow, including how to A/B two builds and
which comparison to trust for which kind of change.

## The one rule worth remembering

**Every comparison here answers a different question, and picking the wrong one wastes a day.**
Geometry (`compare.js`) is exact but compares elements by index, so a change that adds, removes or
re-parents anything misaligns it and it reports differences that are not real. Pixels (`pixdiff.js`)
see everything, including a deliberate 2px change in spacing repainting every glyph on the page, and
a percentage cannot tell that apart from a broken layout. Text-run positions
(`textdiff-samples.js`) survive a structural change and say *how* something moved — sideways, which
is almost always a bug, or downwards, which a spacing change is supposed to do.

Start with the noise floor: compare a build against itself. Then pick the comparison that matches
your change, and use `inspect.js` to find the cause rather than reading the stylesheet and guessing.
