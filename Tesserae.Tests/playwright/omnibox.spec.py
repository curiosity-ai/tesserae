import re
import time
from playwright.sync_api import Playwright, sync_playwright, expect

def test_omnibox_inline_chips_and_right_text(playwright: Playwright) -> None:
    browser = playwright.chromium.launch(headless=True)
    context = browser.new_context()
    page = context.new_page()

    # Navigating via correct Hash Routing
    page.goto("http://localhost:8080/#/view/Omni%20Box")

    # Wait for rendering to complete
    page.wait_for_timeout(2000)

    # Locate the right text element
    right_text = page.locator(".tss-omnibox-right-text").first
    expect(right_text).to_be_visible()
    expect(right_text).to_have_text("124 results")

    # Locate the inline chips container
    chips_container = page.locator(".tss-omnibox-inline-chips").first
    expect(chips_container).to_be_visible()

    # We added two chips in the sample
    chips = page.locator(".tss-omnibox-inline-chips").first.locator(".tss-omnibox-inline-chip")
    expect(chips).to_have_count(2)

    expect(chips.nth(0)).to_contain_text("Tag: Red")
    expect(chips.nth(1)).to_contain_text("Author: Jules")

    # Click in the input to focus it
    input_box = page.locator(".tss-omnibox-search-input").first
    input_box.click()

    # Move cursor to start of text
    for i in range(50): # Ensure we're at index 0 (Left Arrow)
        page.keyboard.press("ArrowLeft")

    # Now we're at selectionStart=0, selectionEnd=0
    # Press Backspace. The last chip (Author: Jules) should be removed
    page.keyboard.press("Backspace")

    # Expect only 1 chip remaining
    expect(chips).to_have_count(1)
    expect(chips.nth(0)).to_contain_text("Tag: Red")

    # Press Backspace again. The remaining chip should be removed.
    page.keyboard.press("Backspace")
    expect(chips).to_have_count(0)

    context.close()
    browser.close()

if __name__ == "__main__":
    with sync_playwright() as p:
        test_omnibox_inline_chips_and_right_text(p)
