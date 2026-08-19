// Profiles the off-screen build+render of a single page in isolation, so the CPU
// profile contains nothing but Tesserae's construction/render work.
//
// Usage: node profile-build.js <page> [--label baseline] [--iterations 10]

const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

function arg(name, def) {
    const i = process.argv.indexOf('--' + name);
    return i >= 0 ? process.argv[i + 1] : def;
}

const PAGE = process.argv[2] || 'list';
const LABEL = arg('label', 'baseline');
const ITER = parseInt(arg('iterations', '10'), 10);
const URL = arg('url', 'http://127.0.0.1:5099/index.html');
const OUT = path.join(__dirname, 'out');
fs.mkdirSync(OUT, { recursive: true });

(async () => {
    const browser = await chromium.launch({ executablePath: '/opt/pw-browsers/chromium', args: ['--no-sandbox', '--js-flags=--expose-gc'] });
    const context = await browser.newContext({ viewport: { width: 1600, height: 1000 } });
    const page = await context.newPage();
    const client = await context.newCDPSession(page);
    await page.goto(URL, { waitUntil: 'networkidle' });
    await page.waitForSelector('#nav-dashboard');

    // warm up
    await page.evaluate(p => window.__bench.build(p, 2), PAGE);

    await client.send('Profiler.enable');
    await client.send('Profiler.setSamplingInterval', { interval: 50 });
    await page.evaluate(() => { if (window.gc) window.gc(); });
    const before = (await client.send('Runtime.getHeapUsage')).usedSize;
    await client.send('Profiler.start');
    const ms = await page.evaluate(({ p, n }) => window.__bench.build(p, n), { p: PAGE, n: ITER });
    const profile = await client.send('Profiler.stop');
    const after = (await client.send('Runtime.getHeapUsage')).usedSize;

    const f = path.join(OUT, `${LABEL}-build-${PAGE}.cpuprofile`);
    fs.writeFileSync(f, JSON.stringify(profile.profile));
    console.log(JSON.stringify({
        label: LABEL, page: PAGE, iterations: ITER,
        totalMs: +ms.toFixed(1), perIterationMs: +(ms / ITER).toFixed(2),
        heapGrowthMB: +((after - before) / 1048576).toFixed(2),
        profile: f,
    }));
    await browser.close();
})().catch(e => { console.error(e); process.exit(1); });
