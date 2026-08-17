// Renders the difference between two screenshots as an image: the page greyed out, with every
// changed pixel painted red. pixdiff.js tells you *how much* changed and where the bounding box
// is; this tells you *what* changed, which is usually the thing you actually need.
//
// It is what identified the accumulating per-row drift on the form page during the stack-item
// removal — a number could not have shown that, a picture did immediately.
//
// Usage: node diffimg.js out/A-form.png out/B-form.png diff-form.png

const { chromium } = require('playwright');
const fs = require('fs');

(async () => {
    const [fa, fb, out] = [process.argv[2], process.argv[3], process.argv[4]];
    if (!fa || !fb || !out) {
        console.error('usage: node diffimg.js <beforePng> <afterPng> <outPng>');
        process.exit(2);
    }

    const browser = await chromium.launch({ executablePath: '/opt/pw-browsers/chromium', args: ['--no-sandbox'] });
    const page = await (await browser.newContext()).newPage();

    const a64 = fs.readFileSync(fa).toString('base64');
    const b64 = fs.readFileSync(fb).toString('base64');

    // Decoding in the browser avoids depending on an image library.
    const png = await page.evaluate(async ([a, b]) => {
        const load = async d => createImageBitmap(await (await fetch('data:image/png;base64,' + d)).blob());
        const [ia, ib] = [await load(a), await load(b)];
        const c = new OffscreenCanvas(ia.width, ia.height);
        const x = c.getContext('2d');

        x.drawImage(ia, 0, 0);
        const da = x.getImageData(0, 0, ia.width, ia.height);
        x.drawImage(ib, 0, 0);
        const db = x.getImageData(0, 0, ib.width, ib.height);

        const o = x.createImageData(ia.width, ia.height);
        for (let i = 0; i < da.data.length; i += 4) {
            const changed = Math.abs(da.data[i] - db.data[i]) > 8
                || Math.abs(da.data[i + 1] - db.data[i + 1]) > 8
                || Math.abs(da.data[i + 2] - db.data[i + 2]) > 8;
            // Unchanged pixels wash out to a pale ghost of the "before" image, so the red reads
            // against the layout it belongs to instead of against a blank page.
            const grey = (da.data[i] + da.data[i + 1] + da.data[i + 2]) / 3;
            o.data[i]     = changed ? 255 : 220 + grey * 0.13;
            o.data[i + 1] = changed ? 0   : 220 + grey * 0.13;
            o.data[i + 2] = changed ? 0   : 220 + grey * 0.13;
            o.data[i + 3] = 255;
        }
        x.putImageData(o, 0, 0);

        const blob = await c.convertToBlob({ type: 'image/png' });
        return new Promise(r => {
            const fr = new FileReader();
            fr.onload = () => r(fr.result.split(',')[1]);
            fr.readAsDataURL(blob);
        });
    }, [a64, b64]);

    fs.writeFileSync(out, Buffer.from(png, 'base64'));
    console.log('wrote', out);
    await browser.close();
})().catch(e => { console.error(e); process.exit(1); });
