// Measures one element in one sample, in one build, and prints its box and the layout properties
// that decide it, walking up its ancestors.
//
// This is the tool for the step after a diff: `textdiff-samples.js` / `pixdiff.js` tell you *that*
// something moved, and this tells you *why* — which box changed size, which property it came from,
// and which ancestor it is a flex item of. Run it against both builds and read the two side by side;
// the property that differs is the cause. Guessing from the stylesheet is slower and usually wrong.
//
// Pick the element one of three ways:
//   --selector ".tss-banner"        every match (first --limit, 12 by default), with its ancestor chain
//   --text "Review now"             the element containing that text
//   --point 293,37                  whatever is painted at those viewport coordinates
//
// Usage: node inspect.js --url http://127.0.0.1:5090/index.html --sample Banner --text "Review now"
//        add --depth 6 to walk further up, --siblings to print the element's flex siblings instead
//        of its ancestors (which is what you want when something is being pushed sideways).

const { chromium } = require('playwright');

function arg(name, def) { const i = process.argv.indexOf('--' + name); return i >= 0 ? process.argv[i + 1] : def; }
const URL = arg('url'), SAMPLE = arg('sample'), DEPTH = parseInt(arg('depth', '4'), 10);
const SELECTOR = arg('selector'), TEXT = arg('text'), POINT = arg('point');
const LIMIT = parseInt(arg('limit', '12'), 10);
const SIBLINGS = process.argv.includes('--siblings');

if (!URL || !SAMPLE || !(SELECTOR || TEXT || POINT)) {
    console.error('need --url, --sample and one of --selector / --text / --point. See the header.');
    process.exit(2);
}

(async () => {
    const browser = await chromium.launch({ executablePath: '/opt/pw-browsers/chromium', args: ['--no-sandbox'] });
    const page = await (await browser.newContext({ viewport: { width: 1600, height: 1000 } })).newPage();
    await page.goto(URL, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1500);
    await page.evaluate(label => {
        const btn = [...document.querySelectorAll('.tss-sidebar-btn')]
            .find(b => [...b.querySelectorAll('span')].some(s => s.textContent.trim() === label));
        if (btn) btn.click();
    }, SAMPLE);
    await page.waitForTimeout(1400);
    await page.evaluate(() => document.querySelectorAll('*').forEach(e => { if (e.scrollTop) e.scrollTop = 0; }));

    const groups = await page.evaluate(([selector, text, point, depth, siblings, limit]) => {
        const className = e => (e.className && e.className.baseVal !== undefined ? e.className.baseVal : e.className) || '';

        function describe(e) {
            const cs = getComputedStyle(e), r = e.getBoundingClientRect();
            return {
                el: e.tagName.toLowerCase() + (className(e) ? '.' + className(e).trim().split(/\s+/).join('.') : ''),
                box: `x=${r.x.toFixed(1)} y=${r.y.toFixed(1)} w=${r.width.toFixed(1)} h=${r.height.toFixed(1)}`,
                display: cs.display,
                size: `width:${cs.width} height:${cs.height}`,
                bounds: `min-width:${cs.minWidth} min-height:${cs.minHeight} max-width:${cs.maxWidth} max-height:${cs.maxHeight}`,
                box_model: `margin:${cs.margin} padding:${cs.padding} box-sizing:${cs.boxSizing}`,
                flex: `direction:${cs.flexDirection} grow:${cs.flexGrow} shrink:${cs.flexShrink} basis:${cs.flexBasis} wrap:${cs.flexWrap} gap:${cs.gap}`,
                align: `align-items:${cs.alignItems} align-self:${cs.alignSelf} justify-content:${cs.justifyContent} text-align:${cs.textAlign}`,
                inline_style: e.getAttribute('style') || '',
            };
        }

        let roots = [];
        if (selector) {
            roots = [...document.querySelectorAll(selector)].slice(0, limit);
        } else if (text) {
            const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
            let node;
            while ((node = walker.nextNode())) if (node.nodeValue.includes(text)) { roots = [node.parentElement]; break; }
        } else {
            const [x, y] = point.split(',').map(Number);
            const hit = document.elementFromPoint(x, y);
            if (hit) roots = [hit];
        }

        return roots.map(root => {
            if (siblings) {
                const parent = root.parentElement;
                return { header: `siblings of ${describe(root).el}`, rows: parent ? [...parent.children].map(describe) : [] };
            }
            const rows = []; let e = root;
            for (let i = 0; i < depth && e; i++, e = e.parentElement) rows.push(describe(e));
            return { header: null, rows };
        });
    }, [SELECTOR, TEXT, POINT, DEPTH, SIBLINGS, LIMIT]);

    if (!groups.length) { console.log('nothing matched'); await browser.close(); return; }

    for (const g of groups) {
        if (g.header) console.log(`\n${g.header}`);
        for (const [i, r] of g.rows.entries()) {
            console.log(`${i ? '  ^ ' : '  > '}${r.el}`);
            for (const k of ['box', 'display', 'size', 'bounds', 'box_model', 'flex', 'align'])
                console.log(`      ${k.padEnd(12)} ${r[k]}`);
            if (r.inline_style) console.log(`      ${'inline'.padEnd(12)} ${r.inline_style}`);
        }
        console.log('');
    }
    await browser.close();
})().catch(e => { console.error(e); process.exit(1); });
