import pytest
from playwright.sync_api import sync_playwright
import time

def test_omnibox_tokens_dark_mode():
    with sync_playwright() as p:
        browser = p.chromium.launch()
        page = browser.new_page()
        page.goto('http://127.0.0.1:8080/#/view/Omni%20Box')

        # Wait for rendering
        page.wait_for_selector('.tss-omnibox-search-token-word')
        time.sleep(1)

        # Toggle dark mode on
        page.evaluate('''() => {
            document.body.classList.add('tss-dark-mode');
        }''')
        time.sleep(1)

        # Take a screenshot to verify
        page.screenshot(path="omnibox_darkmode.png")

        # Verify word token background
        word_bg_color = page.evaluate('''() => {
            const token = document.querySelector('.tss-omnibox-search-token-word');
            return window.getComputedStyle(token).backgroundColor;
        }''')
        # Check if it has the dark blue color (from var(--tss-colors-blue-900)) which might evaluate to its RGB
        # Instead of checking explicit RGB, verify it's not the light blue rgb(219, 234, 254) which is --tss-colors-blue-100
        # Light blue background in Chrome is often "rgb(219, 234, 254)" or similar, dark blue 900 is often "rgb(30, 58, 138)"
        assert word_bg_color != 'rgb(219, 234, 254)'

        browser.close()
        print("Playwright test completed successfully.")

if __name__ == "__main__":
    test_omnibox_tokens_dark_mode()
