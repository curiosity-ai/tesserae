// Regression test for a tool call whose progress line streams: the chip may grow, but must never
// shrink while the line shows, so its chevron and the action beside it (the "watch this agent" eye)
// never slide back under the pointer - and the action never ends up past the container's edge.
//
// Drives the ToolCall sample of the samples gallery, whose "Stream progress" demo rebuilds its bubbles
// on every update and diffs them onto the page, the way a chat refreshes a streaming reply: a plain
// call, and three chips carrying an action (TARGETS) - an agent group, and an agent group and a single
// call each given a MaxWidth. The demo's lines grow and shrink, and a diff writes the rebuilt chip's
// style attribute over the one on screen, so this is the path where a chip that only remembers its
// width in a component would snap back.
//
// Every animation frame is sampled. Runs in each theme, light and dark, in a narrow viewport (narrower
// than the MaxWidth), and with a max-width on the columns the chips sit in.
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
    { theme: 'curiosity', dark: false, width: 1280, columnMaxWidth: 320 },
];

// The bubbles whose chip carries an action. maxWidth is the MaxWidth the sample gives the chip.
const TARGETS = [
    { id: 'tss-toolcall-sample-diffed-agent',           kind: 'group' },
    { id: 'tss-toolcall-sample-diffed-agent-max-width', kind: 'group', maxWidth: 420 },
    { id: 'tss-toolcall-sample-diffed-call-max-width',  kind: 'call',  maxWidth: 420 },
];

// Sub-pixel layout differences between frames are not movement.
const TOLERANCE = 0.5;

async function runOnce(browser, { theme, dark, width, columnMaxWidth }) {
    const context = await browser.newContext({ viewport: { width, height: 1000 } });
    const page = await context.newPage();
    const errors = [];
    page.on('pageerror', e => errors.push('pageerror: ' + e.message));
    page.on('console', m => { if (m.type() === 'error' && !/404|favicon/.test(m.text())) errors.push('console: ' + m.text()); });

    await page.addInitScript(t => { try { localStorage.setItem('tss-sample-theme', t); } catch (e) { } }, theme);
    await page.goto(URL.replace(/#.*$/, '') + '#/view/ToolCallSample', { waitUntil: 'networkidle' });
    await page.waitForSelector('#tss-toolcall-sample-diffed-agent .tss-toolcall-action', { timeout: 30000 });

    if (dark) await page.evaluate(() => document.body.classList.add('tss-dark-mode'));

    // A stylesheet rule rather than an inline style: the diff rewrites the bubbles' attributes.
    if (columnMaxWidth) await page.addStyleTag({ content: `[id^="tss-toolcall-sample-diffed"] { max-width: ${columnMaxWidth}px; }` });

    await page.locator('#tss-toolcall-sample-diffed-agent').scrollIntoViewIfNeeded();

    // Window error events too: a ResizeObserver loop is reported there, not as a page error.
    await page.evaluate(() => {
        window.__toolcallErrors = [];
        window.addEventListener('error', e => window.__toolcallErrors.push(String(e.message)));
    });

    // Sampled from the page every frame, so nothing between two frames can hide from it.
    await page.evaluate((targets) => {
        const frames = window.__toolcallFrames = [];
        const live   = el => !!el && !el.classList.contains('tss-liveprogress-empty') && el.textContent.length > 0;

        const sample = () => {
            const frame = { chips: {} };

            for (const t of targets) {
                const bubble = document.getElementById(t.id);
                const root   = bubble && bubble.querySelector(t.kind === 'group' ? '.tss-toolsused' : '.tss-toolcall');
                const chip   = root && (t.kind === 'group' ? root.querySelector(':scope > .tss-toolsused-header') : root);
                const eye    = chip && chip.querySelector('.tss-toolcall-action');
                const line   = chip && chip.querySelector(t.kind === 'group' ? ':scope > .tss-toolsused-progress' : ':scope > .tss-toolcall-header > .tss-toolcall-progress');

                if (!eye) continue;

                const e = eye.getBoundingClientRect();

                frame.chips[t.id] = {
                    text:           line ? line.textContent : '',
                    live:           live(line),
                    eyeLeft:        e.left,
                    eyeRight:       e.right,
                    containerLeft:  bubble.getBoundingClientRect().left,
                    containerRight: bubble.getBoundingClientRect().right,
                    chipWidth:      chip.getBoundingClientRect().width,
                };
            }

            // The plain call, rebuilt in its own bubble above the others.
            const calls = [...document.querySelectorAll('.tss-toolcall')].filter(c => c.querySelector(':scope > .tss-toolcall-header > .tss-toolcall-progress') && !c.closest('[id^="tss-toolcall-sample-diffed"]'));
            const call  = calls[calls.length - 1];

            frame.callLive  = live(call && call.querySelector(':scope > .tss-toolcall-header > .tss-toolcall-progress'));
            frame.callWidth = call ? call.getBoundingClientRect().width : 0;

            frames.push(frame);

            if (!window.__toolcallStop) requestAnimationFrame(sample);
        };

        requestAnimationFrame(sample);
    }, TARGETS);

    await page.getByRole('button', { name: 'Stream progress' }).click();

    // The demo runs a little over three seconds and ends with an empty line.
    await page.waitForFunction(() => {
        const f = window.__toolcallFrames;
        return f.some(x => x.callLive) && f.length > 0 && !f[f.length - 1].callLive;
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
    const chips    = {};

    for (const t of TARGETS) {
        const label  = t.id.replace('tss-toolcall-sample-diffed-', '');
        const series = frames.map(f => f.chips[t.id]).filter(Boolean);
        const live   = series.filter(c => c.live);

        let eyeMovedRight = 0, lineShrank = 0, widest = 0;

        for (let i = 1; i < series.length; i++) {
            const prev = series[i - 1], cur = series[i];
            if (!prev.live || !cur.live) continue;

            if (cur.eyeLeft < prev.eyeLeft - TOLERANCE) {
                failures.push(`${label}: eye moved left ${(prev.eyeLeft - cur.eyeLeft).toFixed(1)}px when the line went from "${prev.text}" to "${cur.text}"`);
            }

            if (cur.eyeLeft > prev.eyeLeft + TOLERANCE) eyeMovedRight++;
            if (cur.text.length < prev.text.length) lineShrank++;
        }

        for (const c of live) {
            widest = Math.max(widest, c.chipWidth);

            if (c.eyeRight > c.containerRight + TOLERANCE) {
                failures.push(`${label}: eye past the container's edge by ${(c.eyeRight - c.containerRight).toFixed(1)}px on "${c.text}"`);
            }

            // A MaxWidth holds the chip and its actions both, for a call as for a group.
            if (t.maxWidth && c.eyeRight - c.containerLeft > t.maxWidth + TOLERANCE) {
                failures.push(`${label}: eye ${(c.eyeRight - c.containerLeft - t.maxWidth).toFixed(1)}px past the MaxWidth on "${c.text}"`);
            }
        }

        // A run that never shrinks its line has not tested anything. (A chip at its limit from the
        // first line on may never move right, which is fine.)
        if (live.length === 0) failures.push(`${label}: the progress line never showed`);
        if (lineShrank === 0)  failures.push(`${label}: the demo never shortened its line, so a shrink was never attempted`);

        // Once the line is gone the floor goes with it.
        const last = series[series.length - 1];
        if (last && eyeMovedRight > 0 && last.chipWidth >= widest - TOLERANCE) failures.push(`${label}: the chip kept its widest width after the line was cleared`);

        chips[label] = { liveFrames: live.length, eyeMovedRight, widest: Math.round(widest) };
    }

    const callLive = frames.filter(f => f.callLive);

    for (let i = 1; i < callLive.length; i++) {
        if (callLive[i].callWidth < callLive[i - 1].callWidth - TOLERANCE) {
            failures.push(`plain call shrank ${(callLive[i - 1].callWidth - callLive[i].callWidth).toFixed(1)}px while its line showed`);
        }
    }

    failures.push(...errors);

    // One failure of a kind per run is enough to read; the count says how often it happened.
    const counts = {};
    for (const f of failures) {
        const key = f.replace(/[\d.]+px.*$/, '').trim();
        counts[key] = counts[key] || { first: f, times: 0 };
        counts[key].times++;
    }

    return {
        chips,
        failures: Object.values(counts).map(c => c.times > 1 ? `${c.first} (${c.times} times)` : c.first),
    };
}

(async () => {
    const browser = await chromium.launch({ executablePath: '/opt/pw-browsers/chromium', args: ['--no-sandbox'] });
    let failed = false;

    for (const run of RUNS) {
        const result = await runOnce(browser, run);
        const label  = `${run.theme} ${run.dark ? 'dark' : 'light'} ${run.width}px` + (run.columnMaxWidth ? `, columns max-width ${run.columnMaxWidth}px` : '');
        const chips  = Object.entries(result.chips).map(([k, c]) => `${k} ${c.liveFrames} live frames, widest ${c.widest}px, eye moved right ${c.eyeMovedRight}x`).join('; ');

        failed = failed || result.failures.length > 0;

        console.log(`${result.failures.length ? 'FAIL' : 'ok  '} ${label}: ${chips}`);
        for (const f of result.failures) console.log('       ' + f);
    }

    await browser.close();
    if (failed) process.exitCode = 1;
})().catch(e => { console.error(e); process.exit(1); });
