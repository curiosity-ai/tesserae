// Drives the bench app through a fixed set of navigations/interactions while a CDP
// CPU profile + heap sampling runs, then writes the raw profile and an aggregated
// self-time report to ./out.
//
// Usage: node bench.js [--url http://127.0.0.1:5099] [--label baseline] [--repeat 3]

const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

function arg(name, def) {
    const i = process.argv.indexOf('--' + name);
    return i >= 0 ? process.argv[i + 1] : def;
}

const URL = arg('url', 'http://127.0.0.1:5099/index.html');
const LABEL = arg('label', 'baseline');
const REPEAT = parseInt(arg('repeat', '3'), 10);
const OUT = path.join(__dirname, 'out');
fs.mkdirSync(OUT, { recursive: true });

const PAGES = ['dashboard', 'data', 'form', 'list', 'surfaces', 'search', 'tooltips', 'defer', 'chat', 'admin'];

// Closes any visible modal/panel layer and waits for it to actually go away, so a
// stale overlay never intercepts the next click.
async function closeSurfaces(page) {
    for (let i = 0; i < 10; i++) {
        const open = await page.locator('.tss-layer.tss-show').count();
        if (open === 0) return;
        const close = page.locator('.tss-panel-command-button, .tss-modal-command-button, [class*="command-button"]').first();
        if (await close.count()) await close.click({ force: true }).catch(() => {});
        else await page.keyboard.press('Escape');
        await page.waitForTimeout(150);
    }
}

async function main() {
    const browser = await chromium.launch({
        executablePath: '/opt/pw-browsers/chromium',
        args: ['--no-sandbox', '--js-flags=--expose-gc', '--disable-dev-shm-usage'],
    });
    const context = await browser.newContext({ viewport: { width: 1600, height: 1000 } });
    const page = await context.newPage();
    const client = await context.newCDPSession(page);

    const results = { label: LABEL, url: URL, repeat: REPEAT, runs: [], build: {}, memory: {} };

    // ---------------------------------------------------------------- cold load
    await client.send('Profiler.enable');
    await client.send('Profiler.setSamplingInterval', { interval: 100 });
    await client.send('Profiler.start');
    const t0 = Date.now();
    await page.goto(URL, { waitUntil: 'networkidle' });
    await page.waitForSelector('#nav-dashboard', { timeout: 30000 });
    const coldMs = Date.now() - t0;
    const coldProfile = await client.send('Profiler.stop');
    fs.writeFileSync(path.join(OUT, `${LABEL}-cold.cpuprofile`), JSON.stringify(coldProfile.profile));
    results.coldLoadMs = coldMs;
    results.coldNav = await page.evaluate(() => {
        const n = performance.getEntriesByType('navigation')[0] || {};
        return {
            domContentLoaded: Math.round(n.domContentLoadedEventEnd || 0),
            loadEvent: Math.round(n.loadEventEnd || 0),
        };
    });

    // ---------------------------------------------------------------- in-page build cost
    // Times the pure Tesserae build+render of each page, off-screen, N times.
    for (const p of PAGES) {
        const times = [];
        for (let i = 0; i < REPEAT; i++) {
            times.push(await page.evaluate(({ p }) => window.__bench.build(p, 1), { p }));
        }
        times.sort((a, b) => a - b);
        results.build[p] = { median: +times[Math.floor(times.length / 2)].toFixed(2), min: +times[0].toFixed(2), all: times.map(t => +t.toFixed(2)) };
    }

    // ---------------------------------------------------------------- interaction profile
    await client.send('Profiler.start');
    const interactionStart = Date.now();

    for (let r = 0; r < REPEAT; r++) {
        for (const p of PAGES) {
            const t = Date.now();
            await page.click('#nav-' + p);
            await page.waitForTimeout(120);
            results.runs.push({ round: r, action: 'nav:' + p, ms: Date.now() - t });
        }

        // Data page: sort columns + filter
        await page.click('#nav-data');
        await page.waitForTimeout(150);
        for (const col of ['Owner', 'Size', 'Name']) {
            const t = Date.now();
            const h = page.locator('.tss-detailslist-header .tss-detailslist-cell', { hasText: col }).first();
            if (await h.count()) { await h.click(); await page.waitForTimeout(80); }
            results.runs.push({ round: r, action: 'sort:' + col, ms: Date.now() - t });
        }
        const tf = Date.now();
        await page.fill('#data-search input', 'alice');
        await page.waitForTimeout(400);
        await page.fill('#data-search input', '');
        await page.waitForTimeout(400);
        results.runs.push({ round: r, action: 'filter', ms: Date.now() - tf });

        // Scroll the list page
        await page.click('#nav-list');
        await page.waitForTimeout(150);
        const ts = Date.now();
        for (let s = 0; s < 10; s++) {
            await page.mouse.wheel(0, 800);
            await page.waitForTimeout(30);
        }
        results.runs.push({ round: r, action: 'scroll:list', ms: Date.now() - ts });

        // Surfaces: modal + panel + pivot
        await page.click('#nav-surfaces');
        await page.waitForTimeout(200);
        const tm = Date.now();
        await page.click('#open-modal');
        await page.waitForTimeout(200);
        await closeSurfaces(page);
        await page.click('#open-panel');
        await page.waitForTimeout(200);
        await closeSurfaces(page);
        results.runs.push({ round: r, action: 'modal+panel', ms: Date.now() - tm });

        // ---- app-shaped interactions -------------------------------------
        // Hovering a tooltip grid: each first hover builds a tippy instance.
        await page.click('#nav-tooltips');
        await page.waitForTimeout(200);
        const th = Date.now();
        const tips = await page.locator('.tss-btn').all();
        for (const t of tips.slice(0, 25)) { await t.hover({ force: true }).catch(() => {}); await page.waitForTimeout(15); }
        results.runs.push({ round: r, action: 'hover:tooltips', ms: Date.now() - th });

        // Defer churn: 20 observable flips, each rebuilding 60 deferred panels.
        await page.click('#nav-defer');
        await page.waitForTimeout(250);
        const td = Date.now();
        await page.evaluate(() => window.__bench.churnDefer(20));
        await page.waitForTimeout(400);
        results.runs.push({ round: r, action: 'churn:defer', ms: Date.now() - td });

        // Search: type into the box and page through results.
        await page.click('#nav-search');
        await page.waitForTimeout(250);
        const ts2 = Date.now();
        await page.fill('#search-box input', 'quarterly revenue');
        await page.waitForTimeout(250);
        for (let s2 = 0; s2 < 5; s2++) { await page.mouse.wheel(0, 900); await page.waitForTimeout(40); }
        results.runs.push({ round: r, action: 'search', ms: Date.now() - ts2 });

        // Toast burst — 571 Toast call sites in the real app.
        const tt = Date.now();
        await page.evaluate(() => window.__bench.burstToasts(12));
        await page.waitForTimeout(300);
        results.runs.push({ round: r, action: 'toasts', ms: Date.now() - tt });

        // Pivot tab switching (rebuilds a whole page per tab)
        const tp = Date.now();
        for (const tab of ['Tab 2', 'Tab 3', 'Tab 1']) {
            const b = page.locator('.tss-pivot-titlebar button', { hasText: tab }).first();
            if (await b.count()) { await b.click(); await page.waitForTimeout(150); }
        }
        results.runs.push({ round: r, action: 'pivot', ms: Date.now() - tp });
    }

    results.interactionMs = Date.now() - interactionStart;
    const profile = await client.send('Profiler.stop');
    fs.writeFileSync(path.join(OUT, `${LABEL}-interaction.cpuprofile`), JSON.stringify(profile.profile));

    // ---------------------------------------------------------------- memory
    await page.evaluate(() => { if (window.gc) { window.gc(); window.gc(); } });
    await page.waitForTimeout(500);
    const m = await client.send('Runtime.getHeapUsage');
    results.memory = { usedMB: +(m.usedSize / 1048576).toFixed(2), totalMB: +(m.totalSize / 1048576).toFixed(2) };
    results.domNodes = await page.evaluate(() => document.getElementsByTagName('*').length);
    const counters = await client.send('Memory.getDOMCounters');
    results.domCounters = counters;

    await browser.close();

    fs.writeFileSync(path.join(OUT, `${LABEL}-results.json`), JSON.stringify(results, null, 2));
    console.log(JSON.stringify({
        label: LABEL,
        coldLoadMs: results.coldLoadMs,
        interactionMs: results.interactionMs,
        build: results.build,
        memory: results.memory,
        domNodes: results.domNodes,
    }, null, 2));
}

main().catch(e => { console.error(e); process.exit(1); });
