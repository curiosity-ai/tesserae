import asyncio
from playwright.async_api import async_playwright

async def main():
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        page = await browser.new_page()
        await page.goto('http://localhost:8080/#/view/Teaching', wait_until='networkidle')
        await page.wait_for_timeout(2000)

        button = page.locator('button')
        count = await button.count()
        for i in range(count):
            text = await button.nth(i).text_content()
            print(f"Button {i}: {text}")

        await browser.close()

asyncio.run(main())
