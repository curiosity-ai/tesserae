// Runs the behavioural checks in src/Regressions.cs inside the bench app and fails on any failure
// or page error. Add a check there, not here: this script only reports what the page returns.
//
// Usage: node regressions.js [--url http://127.0.0.1:5099/index.html]

const { chromium } = require('playwright');

function arg(name, def) {
    const i = process.argv.indexOf('--' + name);
    return i >= 0 ? process.argv[i + 1] : def;
}

const URL = arg('url', 'http://127.0.0.1:5099/index.html');

(async () => {
    const browser = await chromium.launch({ executablePath: '/opt/pw-browsers/chromium', args: ['--no-sandbox'] });
    const page = await browser.newPage();
    const errors = [];
    page.on('pageerror', e => errors.push('pageerror: ' + e.message));

    await page.goto(URL, { waitUntil: 'networkidle' });
    await page.waitForFunction(() => window.__bench && window.__bench.regressions);

    const failures = Array.from(await page.evaluate(() => window.__bench.regressions()));

    console.log(JSON.stringify({ failures, errors }, null, 2));

    await browser.close();
    if (failures.length || errors.length) process.exitCode = 1;
})().catch(e => { console.error(e); process.exit(1); });
