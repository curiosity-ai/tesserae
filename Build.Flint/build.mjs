// Builds Tesserae/tps/assets/js/flint.min.js: Flint with its Vega-Lite renderer (vega, vega-lite and
// vega-tooltip bundled in), as one classic script that defines window.flint. Tesserae ships it as the
// unlinked tss-flint.min.js resource and fetches it the first time a flint fence is drawn (Flint.cs).
//
//   cd Build.Flint && npm ci && npm run build
//
// Also writes flint.LICENSE.txt beside it, with the licence of every package that ended up in the bundle.

import { build } from 'esbuild';
import { existsSync, readFileSync, readdirSync, statSync, writeFileSync } from 'node:fs';
import { dirname, join, relative, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const here    = dirname(fileURLToPath(import.meta.url));
const outDir  = resolve(here, '../Tesserae/tps/assets/js');
const outFile = join(outDir, 'flint.min.js');

// mountChart imports each backend's renderer on demand, and validateChartInput takes every backend's assembler and
// templates. Only Vega-Lite carries Flint's themes and its interactions, so it is the one Tesserae ships: the other
// backends' modules become stubs that say so, which keeps ECharts, Chart.js and Plotly (and their compilers) out.
const stubs = {
    'echarts/interactive': ['createEChartsInteractiveRenderer'],
    'chartjs/interactive': ['createChartjsInteractiveRenderer'],
    'plotly/interactive':  ['createPlotlyInteractiveRenderer'],
    'echarts/assemble':    ['assembleECharts'],
    'chartjs/assemble':    ['assembleChartjs'],
    'plotly/assemble':     ['assemblePlotly'],
    'echarts/templates':   ['ecGetTemplateDef'],
    'chartjs/templates':   ['cjsGetTemplateDef'],
    'plotly/templates':    ['plGetTemplateDef'],
};

const onlyVegaLite = {
    name: 'only-vega-lite',
    setup(b) {
        b.onResolve({ filter: /^\.\.\/(echarts|chartjs|plotly)\/(interactive|assemble|templates)$/ }, (args) => ({ path: args.path.slice(3), namespace: 'flint-unbundled' }));
        b.onLoad({ filter: /.*/, namespace: 'flint-unbundled' }, (args) => {
            const backend = args.path.split('/')[0];
            const fail    = `() => { throw new Error("Tesserae bundles only the Vega-Lite backend of Flint, not ${backend}."); }`;
            return { contents: stubs[args.path].map((name) => `export const ${name} = ${fail};`).join('\n'), loader: 'js' };
        });
    },
};

const result = await build({
    entryPoints: [resolve(here, 'entry.js')],
    outfile: outFile,
    bundle: true,
    format: 'iife',
    globalName: 'flint',
    platform: 'browser',
    target: 'es2020',
    minify: true,
    legalComments: 'none',
    metafile: true,
    plugins: [onlyVegaLite],
    banner: { js: '/* Flint ' + JSON.parse(readFileSync(resolve(here, 'node_modules/flint-chart/package.json'))).version + ' (https://github.com/microsoft/flint-chart) with Vega, Vega-Lite and vega-tooltip, bundled for Tesserae. Licences: flint.LICENSE.txt */' },
});

// One licence per package that contributed code, in the order a reader looks for them: Flint first.
const packages = new Map();

for (const input of Object.keys(result.metafile.inputs)) {
    const parts = input.split(/[\\/]/);
    const at    = parts.lastIndexOf('node_modules');
    if (at < 0) continue;

    const name = parts[at + 1].startsWith('@') ? parts[at + 1] + '/' + parts[at + 2] : parts[at + 1];
    const root = resolve(here, parts.slice(0, at + 1 + name.split('/').length).join(sep));
    packages.set(name, root);
}

const names = [...packages.keys()].sort((a, b) => (a === 'flint-chart' ? -1 : b === 'flint-chart' ? 1 : a.localeCompare(b)));
let licences = 'Third-party software bundled into flint.min.js.\n';

for (const name of names) {
    const root    = packages.get(name);
    const pkg     = JSON.parse(readFileSync(join(root, 'package.json')));
    const file    = readdirSync(root).find((f) => /^(licen[cs]e|copying)(\.|$)/i.test(f) && statSync(join(root, f)).isFile());
    const text    = file ? readFileSync(join(root, file), 'utf8').trim() : 'License: ' + pkg.license;

    licences += '\n' + '='.repeat(80) + '\n' + name + ' ' + pkg.version + ' (' + pkg.license + ')\n' + '='.repeat(80) + '\n\n' + text + '\n';
}

writeFileSync(join(outDir, 'flint.LICENSE.txt'), licences);

const size = statSync(outFile).size;
console.log(relative(process.cwd(), outFile) + ': ' + (size / 1024).toFixed(0) + ' KB from ' + names.length + ' packages');
if (!existsSync(outFile)) process.exit(1);
