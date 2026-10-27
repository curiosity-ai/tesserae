// The surface of window.flint: what Tesserae's Flint.cs calls, and nothing it does not.
// Built from the TypeScript sources the flint-chart package ships, so only the Vega-Lite path is
// bundled (build.mjs stubs the other backends out of mountChart and validateChartInput).

export { mountChart } from './node_modules/flint-chart/src/interactive/index.ts';
export { validateChartInput } from './node_modules/flint-chart/src/validate/index.ts';
export { THEME_PRESETS, listThemePresets, resolveThemeSpec } from './node_modules/flint-chart/src/core/theme/presets.ts';

export const version = '1.0.1';
