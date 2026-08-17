// Opens every sample in the sidebar, one at a time, and records any page error, console
// error, or failure to render. Used to check a toolkit-wide change (e.g. turning off
// reflection metadata) against the whole gallery rather than a hand-picked subset.
//
// Usage: node all-samples.js --url http://127.0.0.1:5092/index.html [--label after]

const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

function arg(name, def) {
    const i = process.argv.indexOf('--' + name);
    return i >= 0 ? process.argv[i + 1] : def;
}

const URL = arg('url', 'http://127.0.0.1:5092/index.html');
const LABEL = arg('label', 'after');
const OUT = path.join(__dirname, 'out');

(async () => {
    fs.mkdirSync(OUT, { recursive: true });
    const browser = await chromium.launch({ executablePath: '/opt/pw-browsers/chromium', args: ['--no-sandbox'] });
    const page = await (await browser.newContext({ viewport: { width: 1600, height: 1000 } })).newPage();

    let bucket = [];
    page.on('pageerror', e => bucket.push('pageerror: ' + e.message));
    page.on('console', m => {
        if (m.type() !== 'error') return;
        const t = m.text();
        if (/404|favicon|net::ERR_/.test(t)) return;   // asset noise, not toolkit behaviour
        bucket.push('console: ' + t);
    });

    await page.goto(URL, { waitUntil: 'networkidle' });
    await page.waitForTimeout(2000);

    const labels = await page.evaluate(() =>
        [...document.querySelectorAll('.tss-sidebar-btn')]
            .map(b => [...b.querySelectorAll('span')].map(s => s.textContent.trim()).find(Boolean))
            .filter(l => l && l !== 'Source Code'));
    const unique = [...new Set(labels)];

    const results = [];
    for (const label of unique) {
        bucket = [];
        const clicked = await page.evaluate(l => {
            const btn = [...document.querySelectorAll('.tss-sidebar-btn')]
                .find(b => [...b.querySelectorAll('span')].some(s => s.textContent.trim() === l));
            if (!btn) return false;
            btn.click();
            return true;
        }, label);
        await page.waitForTimeout(700);

        // A rendered sample puts a section stack and a meaningful amount of text on the page.
        const rendered = await page.evaluate(() => {
            const content = document.querySelectorAll('.tss-sectionstack').length > 0;
            return { content, textLen: document.body.innerText.length };
        });

        results.push({ label, clicked, ...rendered, errors: bucket.slice(0, 4) });
    }

    const broken = results.filter(r => !r.clicked || !r.content || r.errors.length);
    fs.writeFileSync(path.join(OUT, `all-samples-${LABEL}.json`), JSON.stringify(results, null, 2));

    console.log(JSON.stringify({
        label: LABEL,
        total: results.length,
        ok: results.length - broken.length,
        broken: broken.map(b => ({ label: b.label, clicked: b.clicked, content: b.content, errors: b.errors })),
    }, null, 2));

    await browser.close();
    if (broken.length) process.exitCode = 1;
})().catch(e => { console.error(e); process.exit(1); });
