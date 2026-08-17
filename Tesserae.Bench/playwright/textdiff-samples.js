// Compares two builds of the sample gallery by the position of every visible text run.
//
// Why text runs and not elements: a change that adds, removes or re-parents elements makes an
// index-based element comparison meaningless (see the README), but the text a user reads is the
// same list in the same order whatever the tree looks like underneath. Matching runs by their
// content gives a comparison that survives a structural change and still catches a real one.
//
// Report per sample:
//   COUNT   the two builds render a different number of text runs — content appeared or vanished
//   TEXT    the runs diverge in order — a structural break
//   X/W     a run moved horizontally or changed width — a real layout difference
//   Y       a run moved vertically only — usually a deliberate change in vertical rhythm
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

    const runs = {};
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
    }
    await browser.close();
    return { labels, runs };
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
        if (order >= 0) report.push({ label, kind: 'TEXT', detail: `runs diverge at #${order}: "${ra[order][0]}" vs "${rb[order][0]}"` });
        else if (xw) report.push({ label, kind: 'X/W', detail: `${xw} run(s), e.g. ${firstXw}` });
        else if (y) report.push({ label, kind: 'Y', detail: `${y} run(s), max drift ${worstY}px` });
    }

    const rank = { COUNT: 0, TEXT: 1, 'X/W': 2, Y: 3 };
    report.sort((p, q) => rank[p.kind] - rank[q.kind] || p.label.localeCompare(q.label));
    for (const r of report) console.log(`${r.kind.padEnd(5)} ${r.label.padEnd(26)} ${r.detail}`);
    const counts = report.reduce((m, r) => (m[r.kind] = (m[r.kind] || 0) + 1, m), {});
    console.log(`\n${a.labels.length} samples: ${a.labels.length - report.length} identical, ` +
        Object.entries(counts).map(([k, v]) => `${v} ${k}`).join(', '));
})().catch(e => { console.error(e); process.exit(1); });
