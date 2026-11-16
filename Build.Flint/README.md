# Build.Flint

Builds `Tesserae/tps/assets/js/flint.min.js`, the script Markdown fetches the first time a
` ```flint ` fence is drawn (`Tesserae/src/Helpers/Flint.cs`, `MarkdownCharts.cs`), and
`flint.LICENSE.txt` beside it, with the licence of every package bundled into it.

```bash
cd Build.Flint
npm ci
npm run build
```

What goes in, pinned in `package.json`:

- [`flint-chart`](https://www.npmjs.com/package/flint-chart), built from the TypeScript
  sources its package ships (`src/`), not from its `dist/`: the published `dist` keeps
  extensionless dynamic imports esbuild cannot follow, and building from source is what
  lets `build.mjs` leave the other backends out.
- `vega`, `vega-lite` and `vega-tooltip`, the Vega-Lite renderer Flint draws with.

`entry.js` is the whole of `window.flint`: `mountChart`, `validateChartInput`, the
theme presets, and `clickReport`, the interaction `FlintChart.OnElementClick` mounts so the
surface reports clicks (it changes nothing on the chart, and leaves the key entries and axis
labels a spec's own interaction clicks to that interaction). `build.mjs` replaces the ECharts, Chart.js and Plotly modules
`mountChart` and `validateChartInput` reach for with stubs that throw, which keeps those
backends (and their compilers) out of the bundle. Vega-Lite is the only backend Flint's
themes and interactions apply to.

`bind.js` is Vega-Lite's [legend and scale binding](https://vega.github.io/vega-lite/docs/bind.html)
for a spec's top-level `bind` (a Tesserae key Flint does not read): it adds `tss_legend` /
`tss_scales` selection params to the Vega-Lite spec Flint builds, the opacity condition that
fades the series not chosen, clipping, and free ticks on a bound axis. Flint has no hook
between building its Vega-Lite spec and compiling it to Vega, so `build.mjs` rewrites the one
`compile(shapedSpec)` call in Flint's `vegalite/interactive.ts` to pass through it (and fails
the build if that call moves). `window.flint.validateBindings` checks a `bind` up front.

The output is one classic script (an IIFE defining `window.flint`), shipped by
`Tesserae/tps.json` as the unlinked `tss-flint.min.js` resource (`"load": false`).

To update Flint: bump the versions in `package.json`, `npm install`, `npm run build`,
check the gallery's MarkdownBlock, DeltaComponent and FlintChart samples (the "Legend and
scale binding" section exercises `bind.js`), and commit the two
generated files with the lock file.
