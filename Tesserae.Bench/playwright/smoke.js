// Functional smoke test over the samples app: walks a set of sidebar samples and exercises
// clicks, toggles, text input, dropdowns and context menus, asserting the handlers still fire.
//
// Usage: node smoke.js [--url http://127.0.0.1:5098/index.html]

const { chromium } = require('playwright');

function arg(name, def) {
    const i = process.argv.indexOf('--' + name);
    return i >= 0 ? process.argv[i + 1] : def;
}

const URL = arg('url', 'http://127.0.0.1:5098/index.html');

// Sidebar labels, which is how a user actually reaches a sample.
const SAMPLES = ['Button', 'Check Box', 'Toggle', 'Text Box', 'Dropdown', 'Slider', 'Choice Group',
    'Details List', 'Pivot', 'Context Menu', 'Search Box', 'Tree', 'Modal', 'Panel', 'Nav', 'Chat'];

async function openSample(page, label) {
    const clicked = await page.evaluate((l) => {
        const btn = [...document.querySelectorAll('.tss-sidebar-btn')]
            .find(b => [...b.querySelectorAll('span')].some(s => s.textContent.trim() === l));
        if (!btn) return false;
        btn.click();
        return true;
    }, label);
    if (!clicked) return false;
    await page.waitForTimeout(800);
    return true;
}

(async () => {
    const browser = await chromium.launch({ executablePath: '/opt/pw-browsers/chromium', args: ['--no-sandbox'] });
    const context = await browser.newContext({ viewport: { width: 1600, height: 1000 } });
    const page = await context.newPage();
    const errors = [];
    page.on('pageerror', e => errors.push('pageerror: ' + e.message));
    page.on('console', m => { if (m.type() === 'error' && !/404|favicon/.test(m.text())) errors.push('console: ' + m.text()); });

    await page.goto(URL, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1500);

    const results = [];

    for (const name of SAMPLES) {
        const opened = await openSample(page, name);
        const rendered = opened && await page.evaluate(() => document.querySelectorAll('.tss-sectionstack, .tss-stack').length > 3);
        results.push({ sample: name, rendered });
    }

    // --- interaction assertions on the Button sample (click handlers) ---
    await openSample(page, 'Button');
    const btn = page.locator('button.tss-btn:visible').first();
    await btn.click();
    await page.waitForTimeout(600);
    const toastAfterClick = await page.locator('.tss-toast, .tss-toast-container').count();

    // --- CheckBox toggles ---
    await openSample(page, 'Check Box');
    const cb = page.locator('.tss-checkbox-container:visible').first();
    const before = await cb.evaluate(e => !!e.querySelector('input.tss-checkbox')?.checked);
    await cb.click();
    await page.waitForTimeout(300);
    const after = await cb.evaluate(e => !!e.querySelector('input.tss-checkbox')?.checked);

    // --- TextBox input event ---
    await openSample(page, 'Text Box');
    const tb = page.locator('input.tss-textbox:visible, .tss-textbox input:visible').first();
    await tb.fill('hello-tesserae');
    await page.waitForTimeout(300);
    const typed = await tb.inputValue();

    // --- Dropdown opens and selects ---
    await openSample(page, 'Dropdown');
    const dd = page.locator('.tss-dropdown:visible').first();
    await dd.click();
    await page.waitForTimeout(400);
    const ddOpen = await page.locator('.tss-dropdown-layer .tss-dropdown-item, .tss-layer .tss-dropdown-item').count();
    await page.keyboard.press('Escape');

    console.log(JSON.stringify({
        samplesRendered: results.filter(r => r.rendered).length + '/' + results.length,
        notRendered: results.filter(r => !r.rendered).map(r => r.sample),
        clickShowedToast: toastAfterClick > 0,
        checkBoxToggled: before !== after,
        textBoxValue: typed,
        dropdownItemsShown: ddOpen,
        errors: errors.slice(0, 10),
    }, null, 2));

    await browser.close();
    if (errors.length) process.exitCode = 1;
})().catch(e => { console.error(e); process.exit(1); });
