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

`entry.js` is the whole of `window.flint`: `mountChart`, `validateChartInput`, and the
theme presets. `build.mjs` replaces the ECharts, Chart.js and Plotly modules
`mountChart` and `validateChartInput` reach for with stubs that throw, which keeps those
backends (and their compilers) out of the bundle. Vega-Lite is the only backend Flint's
themes and interactions apply to.

The output is one classic script (an IIFE defining `window.flint`), shipped by
`Tesserae/tps.json` as the unlinked `tss-flint.min.js` resource (`"load": false`).

To update Flint: bump the versions in `package.json`, `npm install`, `npm run build`,
check the gallery's MarkdownBlock and DeltaComponent samples, and commit the two
generated files with the lock file.
