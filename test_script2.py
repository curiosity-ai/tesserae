import asyncio
from playwright.async_api import async_playwright

async def main():
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        page = await browser.new_page()
        await page.goto('http://localhost:8080/#/view/Slider')
        await page.wait_for_timeout(2000)
        await page.screenshot(path='slider.png')
        await browser.close()

asyncio.run(main())
