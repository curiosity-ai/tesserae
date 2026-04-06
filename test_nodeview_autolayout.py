import asyncio
from playwright.async_api import async_playwright
import time
import os

async def main():
    # Kill existing servers
    os.system("kill $(lsof -t -i :8080) 2>/dev/null || true")

    # Run the live server in background from the correct directory
    os.system("npx http-server Tesserae.Tests/bin/Debug/netstandard2.0/h5 -p 8080 &")

    # Wait for server to start
    time.sleep(3)

    async with async_playwright() as p:
        browser = await p.chromium.launch(headless=True)
        page = await browser.new_page()

        await page.goto("http://localhost:8080/#/view/Node%20View")
        print("Navigated to Node View")

        # Wait for the sample page to render
        await page.wait_for_timeout(5000)

        search = await page.query_selector("input[placeholder='Search...']")
        if search:
            await search.fill("NodeView")
            await page.wait_for_timeout(1000)

            nav_link = await page.query_selector("text=NodeView")
            if nav_link:
                await nav_link.click()
                await page.wait_for_timeout(3000)
                print("Clicked nav link")


        # Take a screenshot before
        await page.screenshot(path="before_autolayout.png")
        print("Took before screenshot")

        # Check if baklava-toolbar exists
        toolbar = await page.query_selector('.baklava-toolbar')
        if not toolbar:
            print("baklava-toolbar not found")
        else:
            print("baklava-toolbar found")

        # Click Auto Layout button
        button = await page.query_selector('.tss-autolayout-btn')
        if button:
            await button.click()
            print("Clicked Auto Layout button")
        else:
            print("Auto Layout button not found! Searching by tooltip...")
            button = await page.query_selector('[title="Auto Layout"]')
            if button:
                await button.click()
                print("Clicked Auto Layout button by tooltip")
            else:
                print("Could not find Auto Layout button")

        await page.wait_for_timeout(2000)

        # Take a screenshot after
        await page.screenshot(path="after_autolayout.png")
        print("Took after screenshot")

        await browser.close()

    # Kill the server
    os.system("pkill -f http-server")

if __name__ == "__main__":
    asyncio.run(main())
