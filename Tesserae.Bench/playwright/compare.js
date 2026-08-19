// Renders every bench page in two builds and reports (a) full-page screenshots for a visual
// diff and (b) the geometry of every laid-out element, so a layout regression from the
// Stack/Grid style-transfer changes shows up as a coordinate mismatch rather than a vibe.
//
// Usage: node compare.js --a http://127.0.0.1:5097/index.html --b http://127.0.0.1:5099/index.html

const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

function arg(name, def) {
    const i = process.argv.indexOf('--' + name);
    return i >= 0 ? process.argv[i + 1] : def;
}

const A = arg('a', 'http://127.0.0.1:5097/index.html');
const B = arg('b', 'http://127.0.0.1:5099/index.html');
const OUT = path.join(__dirname, 'out');
const PAGES = ['dashboard', 'data', 'form', 'list', 'surfaces', 'search', 'tooltips', 'defer', 'chat', 'admin'];

async function capture(url, tag) {
    const browser = await chromium.launch({ executablePath: '/opt/pw-browsers/chromium', args: ['--no-sandbox'] });
    const page = await (await browser.newContext({ viewport: { width: 1600, height: 1000 } })).newPage();
    await page.goto(url, { waitUntil: 'networkidle' });
    await page.waitForSelector('#nav-dashboard');
    const geometry = {};
    for (const p of PAGES) {
        await page.click('#nav-' + p);
        // Chat animates its messages in and anchors scroll, so geometry needs a moment to settle;
        // sampling too early reads a mid-animation frame and reports the whole page as moved.
        await page.waitForTimeout(1600);
        // Chat auto-scrolls to its newest message, so where a capture lands depends on timing.
        // Pin every scroll container to the top first, otherwise the whole page reads as "moved".
        await page.evaluate(() => document.querySelectorAll('*').forEach(e => { if (e.scrollTop) e.scrollTop = 0; }));
        await page.waitForTimeout(250);
        await page.screenshot({ path: path.join(OUT, `${tag}-${p}.png`) });
        geometry[p] = await page.evaluate(() =>
            [...document.querySelectorAll('body *')].slice(0, 4000).map(e => {
                const r = e.getBoundingClientRect();
                return [e.tagName, Math.round(r.x), Math.round(r.y), Math.round(r.width), Math.round(r.height)].join(',');
            }));
    }
    await browser.close();
    return geometry;
}

(async () => {
    fs.mkdirSync(OUT, { recursive: true });
    const a = await capture(A, 'A');
    const b = await capture(B, 'B');

    const report = {};
    for (const p of PAGES) {
        const ga = a[p], gb = b[p];
        let mismatches = 0;
        const examples = [];
        const n = Math.max(ga.length, gb.length);
        for (let i = 0; i < n; i++) {
            if (ga[i] !== gb[i]) {
                mismatches++;
                if (examples.length < 5) examples.push({ i, a: ga[i], b: gb[i] });
            }
        }
        report[p] = { elementsA: ga.length, elementsB: gb.length, mismatches, examples };
    }
    fs.writeFileSync(path.join(OUT, 'layout-compare.json'), JSON.stringify(report, null, 2));
    console.log(JSON.stringify(report, null, 2));
})().catch(e => { console.error(e); process.exit(1); });
