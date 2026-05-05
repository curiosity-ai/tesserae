import asyncio
from playwright.async_api import async_playwright

async def main():
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        page = await browser.new_page()
        await page.goto('http://localhost:8080/#/view/Teaching', wait_until='networkidle')
        await page.wait_for_timeout(2000)

        await page.evaluate('''
            const buttons = document.querySelectorAll('button');
            for (const b of buttons) {
                if (b.innerText.includes('Start Walkthrough')) {
                    b.click();
                    break;
                }
            }
        ''')
        await page.wait_for_timeout(1000)

        await page.evaluate('''
            const buttons = document.querySelectorAll('button');
            for (const b of buttons) {
                if (b.innerText.includes('Next')) {
                    b.click();
                    break;
                }
            }
        ''')
        await page.wait_for_timeout(6000)

        await page.screenshot(path='teaching-step3.png')
        await browser.close()

asyncio.run(main())
