import sys
from playwright.sync_api import sync_playwright

def verify():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()
        page.goto("http://localhost:5000/#/view/Card")
        page.wait_for_timeout(5000)

        # Toggle dark mode
        page.evaluate("document.body.classList.add('tss-dark-mode')")
        page.wait_for_timeout(2000)

        # Take screenshot of dark theme full page
        page.set_viewport_size({"width": 1200, "height": 1000})
        page.screenshot(path="dark_theme_full.png", full_page=True)

        browser.close()

if __name__ == '__main__':
    verify()
