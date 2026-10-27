// The surface of window.flint: what Tesserae's Flint.cs calls, and nothing it does not.
// Built from the TypeScript sources the flint-chart package ships, so only the Vega-Lite path is
// bundled (build.mjs stubs the other backends out of mountChart and validateChartInput).

export { mountChart } from './node_modules/flint-chart/src/interactive/index.ts';
export { validateChartInput } from './node_modules/flint-chart/src/validate/index.ts';
export { THEME_PRESETS, listThemePresets, resolveThemeSpec } from './node_modules/flint-chart/src/core/theme/presets.ts';

import { assistedElementTrigger, clickTrigger } from './node_modules/flint-chart/src/interactive/triggers.ts';
import { resolveInteractionSpec } from './node_modules/flint-chart/src/interactive/spec/resolve.ts';

// A click on a mark, a key entry or a category's axis label that changes nothing on the chart: the surface reports it through
// onInteraction as a 'click-element' / 'click-legend' / 'click-axis' event, and the host decides what it means.
// Same trigger, assist distance and hover cue as Flint's click-highlight preset, without its emphasis.
//
// A key entry or an axis label has one owner among a chart's interactions, and Flint gives it to a code definition such as
// this one over an interaction of the spec. So this leaves to the spec's interactions (click-highlight, legend-toggle, ...)
// the ones they click, and the host reads those clicks from their events instead. A mark click is shared by everyone.
export function clickReport(id, input) {
    const taken = new Set();
    try {
        for (const def of resolveInteractionSpec(input && input.interaction_spec).interactions) {
            if (!def.eventSource || def.eventSource.gesture !== 'click' || !def.affordances) continue;
            for (const key of ['legend-item', 'axis-label']) if (key in def.affordances) taken.add(key);
        }
    } catch {
        // An interaction_spec Flint cannot read is reported by mountChart, with its reason.
    }

    const affordances = { 'mark': { cursor: 'activate', hover: 'target' } };
    if (!taken.has('legend-item')) affordances['legend-item'] = { cursor: 'activate', hover: 'cohort' };
    if (!taken.has('axis-label'))  affordances['axis-label']  = { cursor: 'activate', hover: 'cohort' };

    return {
        id: id || 'tss-click',
        eventSource: assistedElementTrigger(clickTrigger, 8),
        affordances,
        handle: () => null,
    };
}

export const version = '1.0.1';
