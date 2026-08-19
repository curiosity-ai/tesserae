// Geometry diff of every sample in the gallery between two builds. This is the gate for any
// change to the layout containers: a sample whose elements land at different coordinates has
// regressed, whatever the console says.
//
// Usage: node compare-samples.js --a http://127.0.0.1:5095/index.html --b http://127.0.0.1:5088/index.html

const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

function arg(name, def) {
    const i = process.argv.indexOf('--' + name);
    return i >= 0 ? process.argv[i + 1] : def;
}

const A = arg('a');
const B = arg('b');
const OUT = path.join(__dirname, 'out');

async function capture(url) {
    const browser = await chromium.launch({ executablePath: '/opt/pw-browsers/chromium', args: ['--no-sandbox'] });
    const page = await (await browser.newContext({ viewport: { width: 1600, height: 1000 } })).newPage();
    await page.goto(url, { waitUntil: 'networkidle' });
    await page.waitForTimeout(2000);

    const labels = await page.evaluate(() =>
        [...document.querySelectorAll('.tss-sidebar-btn')]
            .map(b => [...b.querySelectorAll('span')].map(s => s.textContent.trim()).find(Boolean))
            .filter(l => l && l !== 'Source Code'));

    const geometry = {};
    for (const label of [...new Set(labels)]) {
        await page.evaluate(l => {
            const btn = [...document.querySelectorAll('.tss-sidebar-btn')]
                .find(b => [...b.querySelectorAll('span')].some(s => s.textContent.trim() === l));
            if (btn) btn.click();
        }, label);
        // Animated surfaces (chat, carousel, charts) need to settle before geometry is stable.
        await page.waitForTimeout(1100);
        await page.evaluate(() => document.querySelectorAll('*').forEach(e => { if (e.scrollTop) e.scrollTop = 0; }));
        await page.waitForTimeout(200);

        geometry[label] = await page.evaluate(() =>
            [...document.querySelectorAll('.tss-page-layout > *:last-child *')].slice(0, 2500).map(e => {
                const r = e.getBoundingClientRect();
                return [e.tagName, Math.round(r.x), Math.round(r.y), Math.round(r.width), Math.round(r.height)].join(',');
            }));
    }
    await browser.close();
    return geometry;
}

(async () => {
    fs.mkdirSync(OUT, { recursive: true });
    // Capture the baseline twice: a few samples (Masonry's JS relayout, animated surfaces) are not
    // deterministic within a single build, and comparing against a moving target proves nothing.
    // Anything that disagrees with itself is reported as unstable and excluded from pass/fail.
    const ga = await capture(A);
    const ga2 = await capture(A);
    const gb = await capture(B);

    const unstable = [];
    const report = [];
    for (const label of Object.keys(ga)) {
        const a = ga[label] || [], a2 = ga2[label] || [], b = gb[label] || [];

        if (a.join('|') !== a2.join('|')) { unstable.push(label); continue; }
        let mismatches = 0;
        const examples = [];
        for (let i = 0; i < Math.max(a.length, b.length); i++) {
            if (a[i] !== b[i]) {
                mismatches++;
                if (examples.length < 3) examples.push({ i, a: a[i], b: b[i] });
            }
        }
        report.push({ sample: label, countA: a.length, countB: b.length, mismatches, examples });
    }

    const bad = report.filter(r => r.mismatches > 0);
    fs.writeFileSync(path.join(OUT, 'compare-samples.json'), JSON.stringify(report, null, 2));
    console.log(JSON.stringify({
        samplesCompared: report.length,
        unstableExcluded: unstable,
        identical: report.length - bad.length,
        differing: bad.map(r => ({ sample: r.sample, mismatches: r.mismatches, of: r.countA, examples: r.examples })),
    }, null, 2));
    if (bad.length) process.exitCode = 1;
})().catch(e => { console.error(e); process.exit(1); });
