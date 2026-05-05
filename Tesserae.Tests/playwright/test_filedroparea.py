import asyncio
from playwright.async_api import async_playwright
import os

async def test_filedroparea():
    async with async_playwright() as p:
        browser = await p.chromium.launch(headless=True)
        page = await browser.new_page()

        await page.goto("http://localhost:8080/#/view/Omni%20Box")

        # Wait for the sample to load
        await page.wait_for_selector(".tss-filedroparea-wrapper")

        wrapper = page.locator(".tss-filedroparea-wrapper")

        # 1. Test drag & drop overlay
        await wrapper.evaluate("el => el.dispatchEvent(new Event('dragenter'))")

        # Overlay should be visible
        # Check if overlay is visible or class tss-dropping is present
        await page.wait_for_selector(".tss-filedroparea-wrapper.tss-dropping")

        # 2. Add attachment manual trigger
        # We need a file to upload. Create a dummy one.
        with open("dummy.txt", "w") as f:
            f.write("Hello world!")

        async with page.expect_file_chooser() as fc_info:
            await page.click("text=Add attachment")

        file_chooser = await fc_info.value
        await file_chooser.set_files("dummy.txt")

        # Wait for toast
        await page.wait_for_selector("text=Dropped files: dummy.txt")

        print("Playwright test passed!")
        await browser.close()

        # cleanup
        os.remove("dummy.txt")

if __name__ == "__main__":
    asyncio.run(test_filedroparea())
