import asyncio
from playwright.async_api import async_playwright
import subprocess
import time

async def main():
    # Start the HTTP server in the background
    server_process = subprocess.Popen(["npx", "http-server", "Tesserae.Tests/bin/Debug/netstandard2.0/h5", "-p", "8080"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

    # Wait a moment for the server to start
    await asyncio.sleep(2)

    try:
        async with async_playwright() as p:
            browser = await p.chromium.launch(headless=True)
            page = await browser.new_page()

            # Wait for any errors
            page.on("pageerror", lambda err: print(f"Page error: {err}"))

            # --- Test Searchable List ---
            print("Navigating to SearchableListSample...")
            await page.goto("http://127.0.0.1:8080/#/view/Searchable%20List")

            # Wait for content to render
            await page.wait_for_selector("text=Paginated Searchable List")
            print("Page loaded.")

            await asyncio.sleep(1)

            # --- Test Searchable Grouped List ---
            print("Navigating to SearchableGroupedListSample...")
            await page.goto("http://127.0.0.1:8080/#/view/Searchable%20Grouped%20List")

            # Wait for content to render
            await page.wait_for_selector("text=Paginated Grouped List")
            print("Page loaded.")

            await asyncio.sleep(1)

            # --- Test Pagination Sample ---
            print("Navigating to PaginationSample...")
            await page.goto("http://127.0.0.1:8080/#/view/Pagination")

            await page.wait_for_selector("text=Basic Pagination")
            print("Page loaded.")

            await asyncio.sleep(1)

            await browser.close()
            print("Test complete.")

    finally:
        server_process.terminate()

if __name__ == "__main__":
    asyncio.run(main())
