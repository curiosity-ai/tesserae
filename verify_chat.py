from playwright.sync_api import sync_playwright
import time

def verify_chat():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()

        # Open the index page
        page.goto('http://localhost:5000')
        page.wait_for_load_state('networkidle')

        # Take an initial screenshot
        page.screenshot(path='chat_debug3_init.png', full_page=True)

        # The memory says:
        # "When using Playwright to verify components in the Tesserae.Tests application, if the target component is deep in the sidebar list, use `page.get_by_placeholder("Search...").fill("ComponentName")` to filter the sidebar before clicking, avoiding scroll and visibility timeouts."
        search_box = page.get_by_placeholder("Search...")
        search_box.fill("Chat")

        # Wait for a bit for the filter to apply
        time.sleep(1)
        page.screenshot(path='chat_debug3_filter.png', full_page=True)

        # Click the "Chat" item
        page.click("text='Chat'")

        # Take a screenshot after click
        time.sleep(2)
        page.screenshot(path='chat_debug3_click.png', full_page=True)

        # Wait for the chat area to appear
        try:
            page.wait_for_selector('.tss-chatarea', timeout=5000)
            print("Chat area rendered successfully.")
        except Exception as e:
            print("Failed to find chat area:", e)

        # Output final screenshot
        page.screenshot(path='chat_final.png', full_page=True)

        browser.close()

verify_chat()
