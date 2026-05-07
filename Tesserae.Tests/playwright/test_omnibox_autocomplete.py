import pytest
from playwright.sync_api import Page, expect
import time

def test_omnibox_autocomplete_light(page: Page):
    _run_test(page, "light")

def test_omnibox_autocomplete_dark(page: Page):
    _run_test(page, "dark")

def _run_test(page: Page, theme: str):
    # Navigate to sample app with theme and specific routing to OmniBoxSample
    page.goto(f"http://localhost:8080/?theme={theme}#/view/Omni%20Box")

    # Wait for app to render
    page.wait_for_selector(".tss-omnibox-search-input")

    # Type into the search input
    search_input = page.locator(".tss-omnibox-search-input").first
    search_input.fill("dat")
    search_input.press("a") # To trigger keydown/input events natively

    # Wait for the suggestions popup
    popup = page.locator(".tss-omnibox-search-history-entry")
    popup.first.wait_for(state="visible", timeout=10000)

    # Verify categories are visible using a more robust selector
    page.wait_for_selector("text=DATASETS", timeout=10000)
    expect(page.locator("text=DATASETS").first).to_be_visible()
    expect(page.locator("text=SCHEMAS").first).to_be_visible()

    # Verify specific items
    expect(page.locator("text=dataset / curiosity-prod").first).to_be_visible()

    # Click an item and verify the popup hides
    page.locator("text=dataset / curiosity-prod").first.click()
    popup.first.wait_for(state="hidden", timeout=2000)
