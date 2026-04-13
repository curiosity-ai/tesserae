const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch();
  const page = await browser.newPage();

  // Go to the sample page
  await page.goto('http://127.0.0.1:8080/index.html#/view/Cron%20Editor');
  await page.waitForTimeout(2000);

  const descContainers = await page.$$('.tss-cron-desc');

  // Find the last one which is Observable
  const observableContainer = descContainers[descContainers.length - 1];
  await observableContainer.click();
  await page.waitForTimeout(500);

  // Switch to custom on this one
  // Get the last open editor container
  const openEditors = await page.$$('.tss-cron-open');
  const openEditor = openEditors[openEditors.length - 1];

  const switchCustomBtn = await openEditor.$('.tss-cron-btn-icon');
  if (switchCustomBtn) {
    await switchCustomBtn.click();
    await page.waitForTimeout(500);

    const customInput = await openEditor.$('.tss-cron-custom-input input');
    if (customInput) {
      await customInput.fill('*/10 * * * *');
      await page.waitForTimeout(500);
      await page.screenshot({ path: 'verify_custom.png' });

      const switchSimpleBtn = await openEditor.$('text="Back to simple schedule editor"');
      if (switchSimpleBtn) {
        await switchSimpleBtn.click();
        await page.waitForTimeout(500);
        await page.screenshot({ path: 'verify_simple.png' });
      }
    }
  }

  await browser.close();
})();
