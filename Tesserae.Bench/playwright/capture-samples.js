// Screenshots every sample in the gallery, so two builds can be compared with pixdiff.js /
// diffimg.js. Geometry comparison is the wrong gate whenever a change alters the DOM shape
// (see compare-samples.js) — pixels are what the user actually sees, and they stay comparable
// however the tree is rearranged underneath.
//
// Usage: node capture-samples.js --url http://127.0.0.1:5084/index.html --prefix B [--out out]
//        add --only "Banner,Omni Result" to shoot a few named samples instead of the whole gallery.

const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

function arg(name, def) {
    const i = process.argv.indexOf('--' + name);
    return i >= 0 ? process.argv[i + 1] : def;
}

const URL = arg('url');
const PREFIX = arg('prefix', 'S');
const OUT = path.join(__dirname, arg('out', 'out'));

(async () => {
    fs.mkdirSync(OUT, { recursive: true });
    const browser = await chromium.launch({ executablePath: '/opt/pw-browsers/chromium', args: ['--no-sandbox'] });
    const page = await (await browser.newContext({ viewport: { width: 1600, height: 1000 } })).newPage();
    await page.goto(URL, { waitUntil: 'networkidle' });
    await page.waitForTimeout(2000);

    const only = arg('only');
    const labels = only
        ? only.split(',').map(s => s.trim()).filter(Boolean)
        : [...new Set(await page.evaluate(() =>
            [...document.querySelectorAll('.tss-sidebar-btn')]
                .map(b => [...b.querySelectorAll('span')].map(s => s.textContent.trim()).find(Boolean))
                .filter(l => l && l !== 'Source Code')))];

    for (const label of labels) {
        await page.evaluate(l => {
            const btn = [...document.querySelectorAll('.tss-sidebar-btn')]
                .find(b => [...b.querySelectorAll('span')].some(s => s.textContent.trim() === l));
            if (btn) btn.click();
        }, label);
        // Charts, carousels and the chat surface animate in; sample too early and the shot catches
        // a mid-animation frame, which then reads as a difference between builds.
        await page.waitForTimeout(1200);
        await page.evaluate(() => document.querySelectorAll('*').forEach(e => { if (e.scrollTop) e.scrollTop = 0; }));
        await page.waitForTimeout(250);
        await page.screenshot({ path: path.join(OUT, `${PREFIX}-${label.replace(/[^A-Za-z0-9]/g, '')}.png`) });
    }

    console.log(`captured ${labels.length} samples as ${PREFIX}-*.png in ${OUT}`);
    await browser.close();
})().catch(e => { console.error(e); process.exit(1); });
