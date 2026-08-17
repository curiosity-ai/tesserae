// Aggregates a .cpuprofile into self-time and total-time per function, and per file.
// Usage: node analyze.js out/baseline-interaction.cpuprofile [topN]

const fs = require('fs');

const file = process.argv[2];
const topN = parseInt(process.argv[3] || '40', 10);
const p = JSON.parse(fs.readFileSync(file, 'utf8'));

const byId = new Map();
for (const n of p.nodes) byId.set(n.id, n);

// self time from samples/timeDeltas
const self = new Map();
let total = 0;
for (let i = 0; i < p.samples.length; i++) {
    const dt = p.timeDeltas[i] || 0;
    if (dt < 0) continue;
    total += dt;
    self.set(p.samples[i], (self.get(p.samples[i]) || 0) + dt);
}

const parent = new Map();
for (const n of p.nodes) for (const c of n.children || []) parent.set(c, n.id);

function key(n) {
    const f = n.callFrame;
    const name = f.functionName || '(anonymous)';
    const url = (f.url || '').split('/').pop();
    return `${name} @ ${url}:${f.lineNumber + 1}`;
}

// aggregate self by function key
const agg = new Map();
for (const [id, t] of self) {
    const n = byId.get(id);
    if (!n) continue;
    const k = key(n);
    agg.set(k, (agg.get(k) || 0) + t);
}

// total (inclusive) time: propagate self up the tree
const incl = new Map();
for (const [id, t] of self) {
    let cur = id;
    const seen = new Set();
    while (cur !== undefined) {
        if (seen.has(cur)) break;
        seen.add(cur);
        const n = byId.get(cur);
        if (!n) break;
        const k = key(n);
        incl.set(k, (incl.get(k) || 0) + t);
        cur = parent.get(cur);
    }
}

const byFile = new Map();
for (const [id, t] of self) {
    const n = byId.get(id);
    if (!n) continue;
    const url = (n.callFrame.url || '(native)').split('/').pop() || '(native)';
    byFile.set(url, (byFile.get(url) || 0) + t);
}

const fmt = (t) => (t / 1000).toFixed(1) + 'ms';
const pct = (t) => ((t / total) * 100).toFixed(1) + '%';

console.log(`\n== ${file} — total sampled ${fmt(total)} ==`);
console.log('\n-- by file (self) --');
[...byFile].sort((a, b) => b[1] - a[1]).slice(0, 15).forEach(([k, v]) => console.log(`${fmt(v).padStart(9)} ${pct(v).padStart(6)}  ${k}`));

console.log('\n-- top self time --');
[...agg].sort((a, b) => b[1] - a[1]).slice(0, topN).forEach(([k, v]) => console.log(`${fmt(v).padStart(9)} ${pct(v).padStart(6)}  ${k}`));

console.log('\n-- top inclusive time --');
[...incl].sort((a, b) => b[1] - a[1]).slice(0, topN).forEach(([k, v]) => console.log(`${fmt(v).padStart(9)} ${pct(v).padStart(6)}  ${k}`));
