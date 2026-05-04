import pytest
import time
from playwright.sync_api import sync_playwright, expect

def test_mobile_mode_iphone():
    with sync_playwright() as p:
        iphone_13 = p.devices['iPhone 13']
        browser = p.chromium.launch()
        context = browser.new_context(**iphone_13)
        page = context.new_page()
        page.goto("http://localhost:8080/")

        # Wait for rendering
        page.wait_for_selector(".tss-navbar")

        # The auto-detection in Theme() should add tss-mobile to the body automatically
        expect(page.locator("body")).to_have_class("tss-mobile")

        browser.close()
