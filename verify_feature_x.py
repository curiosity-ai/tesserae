from playwright.sync_api import sync_playwright

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    page = browser.new_page()
    page.goto("http://localhost:8000")

    # Wait for the sidebar search and filter to Tippy
    search_box = page.get_by_placeholder("Search...")
    search_box.fill("Tippy")
    page.get_by_text("Tippy", exact=True).click()

    page.wait_for_timeout(2000)
    page.screenshot(path="/home/jules/verification/debug_error.png")

    browser.close()
