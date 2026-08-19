// Pixel-compares the screenshot pairs written by compare.js / capture-samples.js.
//
// Removing the stack-item wrapper changes the DOM on purpose, so an index-based geometry diff
// misaligns and proves nothing. What must not change is what the user sees, so compare the
// rendered pixels instead. Decoding happens in Chromium (createImageBitmap + canvas), which
// avoids pulling in an image library.
//
// Usage: node pixdiff.js <dirWithPngs> <prefixA> <prefixB>

const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const DIR = process.argv[2] || path.join(__dirname, 'out');
const PA = process.argv[3] || 'A';
const PB = process.argv[4] || 'B';

(async () => {
    const names = fs.readdirSync(DIR)
        .filter(f => f.startsWith(PA + '-') && f.endsWith('.png'))
        .map(f => f.slice(PA.length + 1, -4));

    const browser = await chromium.launch({ executablePath: '/opt/pw-browsers/chromium', args: ['--no-sandbox'] });
    const page = await (await browser.newContext()).newPage();

    const results = [];
    for (const n of names) {
        const fa = path.join(DIR, `${PA}-${n}.png`);
        const fb = path.join(DIR, `${PB}-${n}.png`);
        if (!fs.existsSync(fb)) { results.push({ name: n, error: 'missing counterpart' }); continue; }

        const a = fs.readFileSync(fa).toString('base64');
        const b = fs.readFileSync(fb).toString('base64');

        const r = await page.evaluate(async ([a, b]) => {
            const load = async (d) => {
                const blob = await (await fetch('data:image/png;base64,' + d)).blob();
                return createImageBitmap(blob);
            };
            const [ia, ib] = [await load(a), await load(b)];
            if (ia.width !== ib.width || ia.height !== ib.height) {
                return { sizeMismatch: `${ia.width}x${ia.height} vs ${ib.width}x${ib.height}` };
            }
            const draw = (img) => {
                const c = new OffscreenCanvas(img.width, img.height);
                const x = c.getContext('2d');
                x.drawImage(img, 0, 0);
                return x.getImageData(0, 0, img.width, img.height).data;
            };
            const da = draw(ia), db = draw(ib);
            let differing = 0;
            let x0 = 1e9, y0 = 1e9, x1 = -1, y1 = -1;
            const W = ia.width;
            // A tolerance of 8/255 per channel ignores antialiasing jitter but not a moved element.
            for (let i = 0; i < da.length; i += 4) {
                if (Math.abs(da[i] - db[i]) > 8 || Math.abs(da[i + 1] - db[i + 1]) > 8 ||
                    Math.abs(da[i + 2] - db[i + 2]) > 8 || Math.abs(da[i + 3] - db[i + 3]) > 8) {
                    differing++;
                    const px = (i / 4) % W, py = Math.floor((i / 4) / W);
                    if (px < x0) x0 = px; if (px > x1) x1 = px;
                    if (py < y0) y0 = py; if (py > y1) y1 = py;
                }
            }
            return { pixels: da.length / 4, differing, bbox: x1 < 0 ? null : `${x0},${y0} -> ${x1},${y1}` };
        }, [a, b]);

        results.push({ name: n, ...r, pct: r.pixels ? +(r.differing / r.pixels * 100).toFixed(3) : null });
    }
    await browser.close();

    results.sort((x, y) => (y.pct || 0) - (x.pct || 0));
    for (const r of results) {
        console.log(String(r.name).padEnd(14), r.sizeMismatch ? 'SIZE ' + r.sizeMismatch
            : r.error ? r.error
            : `${String(r.differing).padStart(8)} px differ  (${r.pct}%)  bbox ${r.bbox}`);
    }
    const worst = Math.max(...results.map(r => r.pct || 0));
    console.log('\nworst page:', worst + '%');
})().catch(e => { console.error(e); process.exit(1); });
