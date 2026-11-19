// Builds Tesserae/tps/assets/js/yjs.min.js: Yjs, y-protocols (sync and awareness) and the lib0 encoders they write
// through, as one classic script that defines window.Y and window.YProtocols. Tesserae ships it as the unlinked
// tss-yjs.min.js resource and fetches it the first time something calls Yjs.LoadAsync (Yjs.cs).
//
//   cd Build.Yjs && npm ci && npm run build
//
// Also writes yjs.LICENSE.txt beside it, with the licence of every package that ended up in the bundle.

import { build } from 'esbuild';
import { readFileSync, readdirSync, statSync, writeFileSync } from 'node:fs';
import { dirname, join, relative, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const here    = dirname(fileURLToPath(import.meta.url));
const outDir  = resolve(here, '../Tesserae/tps/assets/js');
const outFile = join(outDir, 'yjs.min.js');
const version = (name) => JSON.parse(readFileSync(resolve(here, 'node_modules', name, 'package.json'))).version;

const result = await build({
    entryPoints: [resolve(here, 'entry.js')],
    outfile: outFile,
    bundle: true,
    format: 'iife',
    platform: 'browser',
    target: 'es2020',
    minify: true,
    legalComments: 'none',
    metafile: true,
    banner: { js: `/* Yjs ${version('yjs')}, y-protocols ${version('y-protocols')}, lib0 ${version('lib0')} (https://github.com/yjs), bundled for Tesserae. Licences: yjs.LICENSE.txt */` },
});

// Yjs refuses to work with a second instance of itself on the page, and a second lib0 would give the protocols
// encoders Yjs does not share. Each package must have come from exactly one folder, and yjs from exactly one file.
const packages = new Map();
const yjsFiles = [];

for (const input of Object.keys(result.metafile.inputs)) {
    const parts = input.split(/[\\/]/);
    const at    = parts.lastIndexOf('node_modules');
    if (at < 0) continue;

    const name = parts[at + 1].startsWith('@') ? parts[at + 1] + '/' + parts[at + 2] : parts[at + 1];
    const root = resolve(here, parts.slice(0, at + 1 + name.split('/').length).join(sep));

    if (packages.has(name) && packages.get(name) !== root) throw new Error(`Two copies of ${name} in the bundle: ${packages.get(name)} and ${root}.`);
    packages.set(name, root);
    if (name === 'yjs') yjsFiles.push(input);
}

if (yjsFiles.length !== 1) throw new Error(`Expected exactly one yjs module in the bundle, found ${yjsFiles.length}: ${yjsFiles.join(', ')}.`);

const order = ['yjs', 'y-protocols', 'lib0'];
const names = [...packages.keys()].sort((a, b) => (order.indexOf(a) + 1 || 99) - (order.indexOf(b) + 1 || 99) || a.localeCompare(b));
let licences = 'Third-party software bundled into yjs.min.js.\n';

for (const name of names) {
    const root = packages.get(name);
    const pkg  = JSON.parse(readFileSync(join(root, 'package.json')));
    const file = readdirSync(root).find((f) => /^(licen[cs]e|copying)(\.|$)/i.test(f) && statSync(join(root, f)).isFile());
    const text = file ? readFileSync(join(root, file), 'utf8').trim() : 'License: ' + pkg.license;

    licences += '\n' + '='.repeat(80) + '\n' + name + ' ' + pkg.version + ' (' + pkg.license + ')\n' + '='.repeat(80) + '\n\n' + text + '\n';
}

writeFileSync(join(outDir, 'yjs.LICENSE.txt'), licences);

const size = statSync(outFile).size;
console.log(relative(process.cwd(), outFile) + ': ' + (size / 1024).toFixed(0) + ' KB from ' + names.join(', ') + ' (' + yjsFiles[0] + ')');
