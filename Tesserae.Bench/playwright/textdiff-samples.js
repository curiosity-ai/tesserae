// Compares two builds of the sample gallery by the position of every visible text run.
//
// Why text runs and not elements: a change that adds, removes or re-parents elements makes an
// index-based element comparison meaningless (see the README), but the text a user reads is the
// same list in the same order whatever the tree looks like underneath. Matching runs by their
// content gives a comparison that survives a structural change and still catches a real one.
//
// Text alone has a blind spot: an empty box — a TextBox with no value, a colour swatch, an icon —
// can move or resize without a single text run shifting. So a second pass compares the boxes of the
// elements themselves, keyed by tag plus their first `tss-` class and the ordinal of that key on the
// page. Marker classes that the change itself moves around (tss-stack-item and the margin utilities)
// are left out of the key, so the same component matches in both builds however it is wrapped.
//
// Report per sample:
//   COUNT   the two builds render a different number of text runs — content appeared or vanished
//   TEXT    the runs diverge in order — a structural break
//   X/W     a run moved horizontally or changed width — a real layout difference
//   Y       a run moved vertically only — usually a deliberate change in vertical rhythm
//   BOX     a component's own content width changed (its position is left to the checks above)
//
// Usage: node textdiff-samples.js --a http://127.0.0.1:5083/index.html --b http://127.0.0.1:5082/index.html
//        add --only Banner to look at one sample, --tol 2 to ignore drift under N px, --verbose to
//        list every differing run instead of one example per sample.

const { chromium } = require('playwright');

function arg(name, def) { const i = process.argv.indexOf('--' + name); return i >= 0 ? process.argv[i + 1] : def; }
const A = arg('a'), B = arg('b'), ONLY = arg('only'), TOL = parseInt(arg('tol', '0'), 10);
const VERBOSE = process.argv.includes('--verbose');

async function capture(url, labels) {
    const browser = await chromium.launch({ executablePath: '/opt/pw-browsers/chromium', args: ['--no-sandbox'] });
    const page = await (await browser.newContext({ viewport: { width: 1600, height: 1000 } })).newPage();
    await page.goto(url, { waitUntil: 'networkidle' });
    await page.waitForTimeout(2000);

    if (!labels) {
        labels = [...new Set(await page.evaluate(() =>
            [...document.querySelectorAll('.tss-sidebar-btn')]
                .map(b => [...b.querySelectorAll('span')].map(s => s.textContent.trim()).find(Boolean))
                .filter(l => l && l !== 'Source Code')))];
    }

    const runs = {}, boxes = {};
    for (const label of labels) {
        await page.evaluate(l => {
            const btn = [...document.querySelectorAll('.tss-sidebar-btn')]
                .find(b => [...b.querySelectorAll('span')].some(s => s.textContent.trim() === l));
            if (btn) btn.click();
        }, label);
        await page.waitForTimeout(1200);
        await page.evaluate(() => document.querySelectorAll('*').forEach(e => { if (e.scrollTop) e.scrollTop = 0; }));
        await page.waitForTimeout(200);
        runs[label] = await page.evaluate(() => {
            const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
            const out = []; let node;
            while ((node = walker.nextNode())) {
                const text = node.nodeValue.trim();
                if (!text) continue;
                const range = document.createRange();
                range.selectNodeContents(node);
                const box = range.getBoundingClientRect();
                if (!box.width && !box.height) continue;
                out.push([text.slice(0, 48), Math.round(box.x), Math.round(box.y), Math.round(box.width), Math.round(box.height)]);
            }
            return out;
        });
        boxes[label] = await page.evaluate(() => {
            // Classes the wrap-and-transfer change itself moves between elements: keying on them
            // would match a wrapper in one build against a component in the other.
            const MOVES_AROUND = new Set(['tss-stack-item', 'tss-grid-item', 'tss-default-component-margin',
                'tss-default-component-no-margin', 'tss-ismounted']);
            const seen = {}, out = {};
            for (const e of document.querySelectorAll('[class*="tss-"]')) {
                const classes = ((e.className.baseVal !== undefined ? e.className.baseVal : e.className) || '').trim().split(/\s+/);
                const own = classes.find(c => c.startsWith('tss-') && !MOVES_AROUND.has(c));
                if (!own) continue;
                const box = e.getBoundingClientRect();
                if (!box.width && !box.height) continue;
                // Content width, not border-box width. A padding or margin utility that used to live
                // on the wrapper now lives on the component, which grows its border box by exactly
                // that padding while the space the content gets is unchanged — reporting the border
                // box would flag every one of those as a difference.
                const cs = getComputedStyle(e);
                const px = p => parseFloat(cs[p]) || 0;
                const inset = px('paddingLeft') + px('paddingRight') + px('borderLeftWidth') + px('borderRightWidth');
                const key = e.tagName.toLowerCase() + '.' + own;
                const n = seen[key] = (seen[key] || 0) + 1;
                out[key + '#' + n] = [Math.round(box.width - inset), Math.round(box.height)];
            }
            return out;
        });
    }
    await browser.close();
    return { labels, runs, boxes };
}

(async () => {
    const a = await capture(A, ONLY ? [ONLY] : null);
    const b = await capture(B, a.labels);

    const report = [];
    for (const label of a.labels) {
        const ra = a.runs[label] || [], rb = b.runs[label] || [];
        if (ra.length !== rb.length) { report.push({ label, kind: 'COUNT', detail: `${ra.length} vs ${rb.length} runs` }); continue; }

        let order = -1, xw = 0, y = 0, worstY = 0, firstXw = null;
        for (let i = 0; i < ra.length; i++) {
            if (ra[i][0] !== rb[i][0]) { order = i; break; }
            const dx = rb[i][1] - ra[i][1], dy = rb[i][2] - ra[i][2];
            const dw = rb[i][3] - ra[i][3], dh = rb[i][4] - ra[i][4];
            if (Math.abs(dx) > TOL || Math.abs(dw) > TOL || Math.abs(dh) > TOL) {
                xw++;
                if (!firstXw) firstXw = `"${ra[i][0]}" dx=${dx} dw=${dw} dh=${dh}`;
                if (VERBOSE) console.log(`      ${label} #${i} "${ra[i][0]}" A[${ra[i].slice(1)}] B[${rb[i].slice(1)}] d=(${dx},${dy},${dw},${dh})`);
            } else if (Math.abs(dy) > TOL) {
                y++; worstY = Math.abs(dy) > Math.abs(worstY) ? dy : worstY;
                if (VERBOSE) console.log(`      ${label} #${i} "${ra[i][0]}" dy=${dy}`);
            }
        }
        // Widths of the components' own boxes, which catches an empty one moving or resizing when no
        // text does. Heights are deliberately not reported: a change in vertical rhythm moves every
        // height on the page and would drown the signal, and the Y check above already covers it.
        const ba = a.boxes[label] || {}, bb = b.boxes[label] || {};
        let boxDiffs = 0, firstBox = null;
        for (const key of Object.keys(ba)) {
            if (!bb[key]) continue;
            const dw = bb[key][0] - ba[key][0];
            if (Math.abs(dw) <= TOL) continue;
            boxDiffs++;
            if (!firstBox) firstBox = `${key} dw=${dw} (${ba[key][0]} -> ${bb[key][0]})`;
            if (VERBOSE) console.log(`      ${label} ${key} dw=${dw} (${ba[key][0]} -> ${bb[key][0]})`);
        }

        if (order >= 0) report.push({ label, kind: 'TEXT', detail: `runs diverge at #${order}: "${ra[order][0]}" vs "${rb[order][0]}"` });
        else if (xw) report.push({ label, kind: 'X/W', detail: `${xw} run(s), e.g. ${firstXw}` });
        else if (boxDiffs) report.push({ label, kind: 'BOX', detail: `${boxDiffs} element(s), e.g. ${firstBox}` });
        else if (y) report.push({ label, kind: 'Y', detail: `${y} run(s), max drift ${worstY}px` });
    }

    const rank = { COUNT: 0, TEXT: 1, 'X/W': 2, BOX: 3, Y: 4 };
    report.sort((p, q) => rank[p.kind] - rank[q.kind] || p.label.localeCompare(q.label));
    for (const r of report) console.log(`${r.kind.padEnd(5)} ${r.label.padEnd(26)} ${r.detail}`);
    const counts = report.reduce((m, r) => (m[r.kind] = (m[r.kind] || 0) + 1, m), {});
    console.log(`\n${a.labels.length} samples: ${a.labels.length - report.length} identical, ` +
        Object.entries(counts).map(([k, v]) => `${v} ${k}`).join(', '));
})().catch(e => { console.error(e); process.exit(1); });
