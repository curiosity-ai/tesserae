import pytest
import time
from playwright.sync_api import sync_playwright, Page, expect

def test_mobile_mode_toggles():
    with sync_playwright() as p:
        browser = p.chromium.launch()
        page = browser.new_page()
        # Connect to 8080 as per run_tests.py server setup
        page.goto("http://localhost:8080/")

        # Wait for app to load
        page.wait_for_selector(".tss-sidebar")

        # Check that body does not have tss-mobile initially
        classes = page.locator("body").get_attribute("class") or ""
        assert "tss-mobile" not in classes

        # Wait for the mobile toggle button to be in the DOM
        clicked = page.evaluate('''() => {
            let btn = document.getElementById("mobile-toggle");
            if (btn) {
                btn.click();
                return true;
            }
            return false;
        }''')

        assert clicked, "Mobile toggle button should be found and clicked"

        time.sleep(1) # wait for transition

        # Check that body has tss-mobile class
        classes = page.locator("body").get_attribute("class") or ""
        assert "tss-mobile" in classes

        # Click again to toggle back to desktop
        clicked = page.evaluate('''() => {
            let btn = document.getElementById("mobile-toggle");
            if (btn) {
                btn.click();
                return true;
            }
            return false;
        }''')

        assert clicked, "Mobile toggle button should be found and clicked again"

        time.sleep(1) # wait for transition

        # Check that body no longer has tss-mobile class
        classes = page.locator("body").get_attribute("class") or ""
        assert "tss-mobile" not in classes

        browser.close()
