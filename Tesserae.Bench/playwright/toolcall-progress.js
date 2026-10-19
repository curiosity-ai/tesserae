// Regression test for a tool call whose progress line streams: the chip may grow, but must never
// shrink while the line shows, so its chevron and the action beside it (the "watch this agent" eye)
// never slide back under the pointer - and the action never ends up past the container's edge.
//
// Drives the ToolCall sample of the samples gallery, whose "Stream progress" demo rebuilds two
// bubbles on every update and diffs them onto the page, the way a chat refreshes a streaming reply:
// a plain call, and an agent group (#tss-toolcall-sample-diffed-agent) carrying an action. The demo's
// lines grow and shrink, and a diff writes the rebuilt chip's style attribute over the one on screen,
// so this is the path where a chip that only remembers its width in a component would snap back.
//
// Every animation frame is sampled. Runs in each theme, light and dark, and in a narrow viewport.
//
// Usage: node toolcall-progress.js [--url http://127.0.0.1:5098/index.html]

const { chromium } = require('playwright');

function arg(name, def) {
    const i = process.argv.indexOf('--' + name);
    return i >= 0 ? process.argv[i + 1] : def;
}

const URL = arg('url', 'http://127.0.0.1:5098/index.html');

const RUNS = [
    { theme: 'default',   dark: false, width: 1280 },
    { theme: 'default',   dark: true,  width: 1280 },
    { theme: 'curiosity', dark: false, width: 1280 },
    { theme: 'curiosity', dark: true,  width: 1280 },
    { theme: 'stratus',   dark: false, width: 1280 },
    { theme: 'stratus',   dark: true,  width: 1280 },
    { theme: 'curiosity', dark: false, width: 480 },
];

// Sub-pixel layout differences between frames are not movement.
const TOLERANCE = 0.5;

async function runOnce(browser, { theme, dark, width }) {
    const context = await browser.newContext({ viewport: { width, height: 1000 } });
    const page = await context.newPage();
    const errors = [];
    page.on('pageerror', e => errors.push('pageerror: ' + e.message));
    page.on('console', m => { if (m.type() === 'error' && !/404|favicon/.test(m.text())) errors.push('console: ' + m.text()); });

    await page.addInitScript(t => { try { localStorage.setItem('tss-sample-theme', t); } catch (e) { } }, theme);
    await page.goto(URL.replace(/#.*$/, '') + '#/view/ToolCallSample', { waitUntil: 'networkidle' });
    await page.waitForSelector('#tss-toolcall-sample-diffed-agent .tss-toolcall-action', { timeout: 30000 });

    if (dark) await page.evaluate(() => document.body.classList.add('tss-dark-mode'));

    await page.locator('#tss-toolcall-sample-diffed-agent').scrollIntoViewIfNeeded();

    // Window error events too: a ResizeObserver loop is reported there, not as a page error.
    await page.evaluate(() => {
        window.__toolcallErrors = [];
        window.addEventListener('error', e => window.__toolcallErrors.push(String(e.message)));
    });

    // Sampled from the page every frame, so nothing between two frames can hide from it.
    await page.evaluate(() => {
        const frames = window.__toolcallFrames = [];
        const live   = el => !!el && !el.classList.contains('tss-liveprogress-empty') && el.textContent.length > 0;

        const sample = () => {
            const agent    = document.querySelector('#tss-toolcall-sample-diffed-agent .tss-toolsused');
            const header   = agent && agent.querySelector(':scope > .tss-toolsused-header');
            const eye      = header && header.querySelector('.tss-toolcall-action');
            const progress = header && header.querySelector(':scope > .tss-toolsused-progress');

            // The plain call, rebuilt in its own bubble just above the agent group.
            const calls = [...document.querySelectorAll('.tss-toolcall')].filter(c => c.querySelector(':scope > .tss-toolcall-header > .tss-toolcall-progress') && !c.closest('#tss-toolcall-sample-diffed-agent'));
            const call  = calls[calls.length - 1];
            const callProgress = call && call.querySelector(':scope > .tss-toolcall-header > .tss-toolcall-progress');

            if (agent && eye) {
                const e = eye.getBoundingClientRect();
                frames.push({
                    text:           progress ? progress.textContent : '',
                    live:           live(progress),
                    eyeLeft:        e.left,
                    eyeRight:       e.right,
                    containerRight: agent.getBoundingClientRect().right,
                    headerWidth:    header.getBoundingClientRect().width,
                    callLive:       live(callProgress),
                    callWidth:      call ? call.getBoundingClientRect().width : 0,
                });
            }

            if (!window.__toolcallStop) requestAnimationFrame(sample);
        };

        requestAnimationFrame(sample);
    });

    await page.getByRole('button', { name: 'Stream progress' }).click();

    // The demo runs a little over three seconds and ends with an empty line.
    await page.waitForFunction(() => {
        const f = window.__toolcallFrames;
        return f.some(x => x.live) && f.length > 0 && !f[f.length - 1].live;
    }, null, { timeout: 15000 });

    await page.waitForTimeout(200);
    await page.evaluate(() => { window.__toolcallStop = true; });

    const frames = await page.evaluate(() => window.__toolcallFrames);
    errors.push(...(await page.evaluate(() => window.__toolcallErrors)).map(m => 'window error: ' + m));

    await context.close();

    return check(frames, errors);
}

function check(frames, errors) {
    const failures = [];
    const live     = frames.filter(f => f.live);

    let eyeMovedRight = 0, lineShrank = 0, widest = 0;

    for (let i = 1; i < frames.length; i++) {
        const prev = frames[i - 1], cur = frames[i];
        if (!prev.live || !cur.live) continue;

        if (cur.eyeLeft < prev.eyeLeft - TOLERANCE) {
            failures.push(`eye moved left ${(prev.eyeLeft - cur.eyeLeft).toFixed(1)}px when the line went from "${prev.text}" to "${cur.text}"`);
        }

        if (cur.eyeLeft > prev.eyeLeft + TOLERANCE) eyeMovedRight++;
        if (cur.text.length < prev.text.length) lineShrank++;
    }

    for (const f of live) {
        widest = Math.max(widest, f.headerWidth);

        if (f.eyeRight > f.containerRight + TOLERANCE) {
            failures.push(`eye past the container's edge by ${(f.eyeRight - f.containerRight).toFixed(1)}px on "${f.text}"`);
        }
    }

    const callLive = frames.filter(f => f.callLive);

    for (let i = 1; i < callLive.length; i++) {
        if (callLive[i].callWidth < callLive[i - 1].callWidth - TOLERANCE) {
            failures.push(`plain call shrank ${(callLive[i - 1].callWidth - callLive[i].callWidth).toFixed(1)}px while its line showed`);
        }
    }

    // A run that never shrinks its line, or never moves the eye, has not tested anything.
    if (live.length === 0)  failures.push('the progress line never showed');
    if (lineShrank === 0)    failures.push('the demo never shortened its line, so a shrink was never attempted');
    if (eyeMovedRight === 0) failures.push('the eye never moved right, so the line never grew the chip');

    // Once the line is gone the floor goes with it.
    const last = frames[frames.length - 1];
    if (last && widest > 0 && last.headerWidth >= widest - TOLERANCE) failures.push('the chip kept its widest width after the line was cleared');

    failures.push(...errors);

    // One failure of a kind per run is enough to read; the count says how often it happened.
    const counts = {};
    for (const f of failures) {
        const key = f.replace(/[\d.]+px.*$/, '').trim();
        counts[key] = counts[key] || { first: f, times: 0 };
        counts[key].times++;
    }

    return {
        frames:        frames.length,
        liveFrames:    live.length,
        eyeMovedRight,
        lineShrank,
        failures:      Object.values(counts).map(c => c.times > 1 ? `${c.first} (${c.times} times)` : c.first),
    };
}

(async () => {
    const browser = await chromium.launch({ executablePath: '/opt/pw-browsers/chromium', args: ['--no-sandbox'] });
    let failed = false;

    for (const run of RUNS) {
        const result = await runOnce(browser, run);
        const label  = `${run.theme} ${run.dark ? 'dark' : 'light'} ${run.width}px`;

        failed = failed || result.failures.length > 0;

        console.log(`${result.failures.length ? 'FAIL' : 'ok  '} ${label}: ${result.liveFrames} live frames, eye moved right ${result.eyeMovedRight}x, line shortened ${result.lineShrank}x`);
        for (const f of result.failures) console.log('       ' + f);
    }

    await browser.close();
    if (failed) process.exitCode = 1;
})().catch(e => { console.error(e); process.exit(1); });
