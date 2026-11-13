// Vega-Lite's selection bindings (https://vega.github.io/vega-lite/docs/bind.html) for a Flint chart: legend binding
// and scale binding, asked for by the spec's top-level `bind` (a Tesserae key; Flint itself does not read it):
//
//   "bind": {
//     "legend": true | { "channel": "color", "events": "click", "marks": false, "opacity": 0.2 },
//     "scales": true | "x" | "y" | { "encodings": ["x", "y"], "zoom": true, "translate": true }
//   }
//
// Flint compiles its chart to a Vega-Lite spec and that to Vega; build.mjs calls applyBindings on the Vega-Lite spec
// just before that second step, every time the chart is shaped (a mount, a re-plan for filtered rows). The selections
// are written as top-level params, which Vega-Lite pushes down to every unit view; what a unit cannot bind (a channel
// it does not encode, a band scale) is left alone by Vega-Lite.

export const LEGEND_PARAM = 'tss_legend';
export const SCALES_PARAM = 'tss_scales';

// The channels a key is drawn for with symbols, which is the only kind of key Vega-Lite binds a selection to.
const LEGEND_CHANNELS = ['color', 'fill', 'stroke', 'shape', 'strokeDash', 'size', 'opacity'];
const DISCRETE        = ['nominal', 'ordinal'];
const CONTINUOUS      = ['quantitative', 'temporal'];
const POSITION        = ['x', 'y'];

/** Reads `input.bind` into the bindings to apply; throws, with the reason, for one that is not well-formed. */
export function readBindings(input) {
    const bind = input && input.bind;
    if (bind === undefined || bind === null || bind === false) return {};
    if (typeof bind !== 'object' || Array.isArray(bind)) throw new Error('"bind" must be an object with "legend" and/or "scales".');

    for (const key of Object.keys(bind)) {
        if (key !== 'legend' && key !== 'scales') throw new Error(`"bind" has no "${key}": it binds "legend" and "scales".`);
    }

    return { legend: readLegend(bind.legend), scales: readScales(bind.scales) };
}

function readLegend(value) {
    if (value === undefined || value === null || value === false) return undefined;
    const legend = { channel: undefined, events: undefined, marks: false, opacity: 0.2 };
    if (value === true) return legend;
    if (typeof value !== 'object' || Array.isArray(value)) throw new Error('"bind.legend" must be true or an object.');

    if (value.channel !== undefined) {
        if (!LEGEND_CHANNELS.includes(value.channel)) throw new Error(`"bind.legend.channel" must be one of ${LEGEND_CHANNELS.join(', ')}.`);
        legend.channel = value.channel;
    }
    if (value.events !== undefined) {
        if (typeof value.events !== 'string' || !value.events.trim()) throw new Error('"bind.legend.events" must be a Vega event stream, such as "click" or "dblclick".');
        legend.events = value.events;
    }
    if (value.marks !== undefined) {
        if (typeof value.marks !== 'boolean') throw new Error('"bind.legend.marks" must be true or false.');
        legend.marks = value.marks;
    }
    if (value.opacity !== undefined) {
        if (typeof value.opacity !== 'number' || !(value.opacity >= 0 && value.opacity <= 1)) throw new Error('"bind.legend.opacity" must be a number from 0 to 1.');
        legend.opacity = value.opacity;
    }
    return legend;
}

function readScales(value) {
    if (value === undefined || value === null || value === false) return undefined;
    const scales = { encodings: POSITION.slice(), zoom: true, translate: true };
    if (value === true) return scales;
    if (value === 'x' || value === 'y') return { ...scales, encodings: [value] };
    if (typeof value !== 'object' || Array.isArray(value)) throw new Error('"bind.scales" must be true, "x", "y" or an object.');

    if (value.encodings !== undefined) {
        const list = Array.isArray(value.encodings) ? value.encodings : [value.encodings];
        if (list.length === 0 || list.some((e) => !POSITION.includes(e))) throw new Error('"bind.scales.encodings" must list "x", "y" or both.');
        scales.encodings = [...new Set(list)];
    }
    for (const key of ['zoom', 'translate']) {
        if (value[key] === undefined) continue;
        if (typeof value[key] !== 'boolean' && (typeof value[key] !== 'string' || !value[key].trim())) {
            throw new Error(`"bind.scales.${key}" must be true, false or a Vega event stream.`);
        }
        scales[key] = value[key];
    }
    return scales;
}

/**
 * Writes the bindings `input.bind` asks for into `spec`, the Vega-Lite spec Flint is about to compile, and returns it.
 * A binding the chart has nothing for (no key drawn with symbols, no continuous x or y) is left out.
 */
export function applyBindings(spec, input) {
    const { legend, scales } = readBindings(input);
    if (!spec || typeof spec !== 'object' || (!legend && !scales)) return spec;

    // Shaping the same spec twice must not add the params twice.
    const params = Array.isArray(spec.params) ? spec.params : [];
    if (params.some((p) => p && (p.name === LEGEND_PARAM || p.name === SCALES_PARAM))) return spec;

    const units = unitsOf(spec);
    if (legend) bindLegend(spec, units, legend);
    if (scales) bindScales(spec, units, scales);
    return spec;
}

function bindLegend(spec, units, options) {
    const found = legendField(units, options.channel);
    if (!found) return;

    // Vega-Lite turns direct manipulation off for a legend-bound selection, clearing included: on its own, the choice
    // goes only with a click on nothing at all (not even the plot's background). A double click anywhere clears it, as
    // it resets a scale binding. Vega-Lite reads `clear` only beside an `on`, so without mark clicks `on` is a stream
    // that never fires; asked for, a click on a mark chooses its series too.
    const select = { type: 'point', fields: [found.field], on: options.marks ? 'click' : 'click[false]', clear: 'dblclick' };

    addParam(spec, {
        name: LEGEND_PARAM,
        select,
        bind: options.events ? { legend: options.events } : 'legend',
    });

    // Each unit drawing that field fades while something else is chosen: the marks, the labels Flint writes on them.
    const muted = { test: { not: { param: LEGEND_PARAM } }, value: options.opacity };
    for (const unit of units) {
        if (!Object.values(unit.encoding).some((def) => fieldOf(def) === found.field)) continue;
        const own = unit.spec.encoding = unit.spec.encoding ?? {};
        own.opacity = withMutedCondition(unit.encoding.opacity, muted);
    }
}

// The first key drawn with symbols, on `channel` when one is asked for: the field it names.
function legendField(units, channel) {
    for (const ch of channel ? [channel] : LEGEND_CHANNELS) {
        for (const unit of units) {
            const def = unit.encoding[ch];
            const field = fieldOf(def);
            if (!field || def.legend === null) continue;
            // A colour or opacity key for a measure is a gradient; a size key is symbols either way.
            if (ch !== 'size' && !DISCRETE.includes(def.type)) continue;
            return { field, channel: ch };
        }
    }
    return undefined;
}

function withMutedCondition(opacity, muted) {
    if (!opacity || typeof opacity !== 'object') return { condition: muted };
    const conditions = opacity.condition === undefined ? [] : Array.isArray(opacity.condition) ? opacity.condition : [opacity.condition];
    return { ...opacity, condition: [muted, ...conditions] };
}

function bindScales(spec, units, options) {
    // Only a continuous scale can be panned and zoomed; a band of categories cannot.
    const encodings = options.encodings.filter((ch) => units.some((unit) => CONTINUOUS.includes((unit.encoding[ch] || {}).type) && fieldOf(unit.encoding[ch])));
    if (encodings.length === 0) return;

    const select = { type: 'interval', encodings };
    for (const key of ['zoom', 'translate']) {
        if (options[key] === false) select[key] = false;
        else if (typeof options[key] === 'string') select[key] = options[key];
    }
    addParam(spec, { name: SCALES_PARAM, select, bind: 'scales' });

    for (const unit of units) {
        // Marks scrolled past the plot's edge are cut off at it, not drawn over the axes.
        unit.spec.mark = typeof unit.spec.mark === 'string' ? { type: unit.spec.mark, clip: true } : { ...unit.spec.mark, clip: true };
    }

    // Flint plans the ticks of an axis for the domain it laid the chart out for, and prints labels only at those. A
    // panned or zoomed axis needs ticks for the domain in view, so a bound axis keeps Flint's format and drops its ticks.
    for (const holder of encodingHolders(spec)) {
        for (const ch of encodings) {
            const axis = holder.encoding[ch] && holder.encoding[ch].axis;
            if (axis && typeof axis === 'object') freeAxisTicks(axis);
        }
    }
}

// The labelExpr Flint writes for planned temporal ticks: `(<planned ticks>)[toString(toNumber(datum.value))] ? <format> : ''`.
const PLANNED_LABELS = /^\(\{[^}]*\}\)\[toString\(toNumber\(datum\.value\)\)\] \? ([\s\S]*) : ''$/;

function freeAxisTicks(axis) {
    delete axis.values;
    if (typeof axis.labelExpr === 'string') {
        const planned = PLANNED_LABELS.exec(axis.labelExpr);
        if (planned) axis.labelExpr = planned[1];
    }
    // Flint turns overlap removal off because it chose ticks that fit; ticks Vega chooses can collide.
    delete axis.labelOverlap;
    delete axis.labelFlush;
}

function addParam(spec, param) {
    spec.params = [...(Array.isArray(spec.params) ? spec.params : []), param];
}

function fieldOf(def) {
    return def && typeof def === 'object' && typeof def.field === 'string' ? def.field : undefined;
}

// Every unit view of the spec, with the encoding it draws with: its own over the ones of the layers it is in.
function unitsOf(spec) {
    const units = [];
    const visit = (node, inherited) => {
        if (!node || typeof node !== 'object') return;
        const encoding = { ...inherited, ...(node.encoding ?? {}) };
        if (node.mark !== undefined) {
            units.push({ spec: node, encoding });
            return;
        }
        if (Array.isArray(node.layer)) node.layer.forEach((child) => visit(child, encoding));
        if (node.spec) visit(node.spec, {});
        for (const key of ['concat', 'hconcat', 'vconcat']) if (Array.isArray(node[key])) node[key].forEach((child) => visit(child, {}));
    };
    visit(spec, {});
    return units;
}

// Every node of the spec that has an encoding: units, and layers that give theirs to the units in them.
function encodingHolders(spec) {
    const holders = [];
    const visit = (node) => {
        if (!node || typeof node !== 'object') return;
        if (node.encoding && typeof node.encoding === 'object') holders.push(node);
        if (Array.isArray(node.layer)) node.layer.forEach(visit);
        if (node.spec) visit(node.spec);
        for (const key of ['concat', 'hconcat', 'vconcat']) if (Array.isArray(node[key])) node[key].forEach(visit);
    };
    visit(spec);
    return holders;
}
