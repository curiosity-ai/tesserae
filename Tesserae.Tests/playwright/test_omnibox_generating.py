import re
from playwright.sync_api import Page, expect

def test_omnibox_generating(page: Page):
    page.goto("http://127.0.0.1:8080/#/view/Omni%20Box")

    omniboxes = page.locator(".tss-omnibox-container")
    expect(omniboxes).to_have_count(3)

    chat_omnibox = omniboxes.nth(1)

    attachment_button = chat_omnibox.locator(".tss-omnibox-footer-left")
    expect(attachment_button).to_be_visible()

    chat_input = chat_omnibox.locator(".tss-omnibox-chat-input")
    chat_input.fill("Hello world")

    chat_input.press("Enter")

    expect(chat_omnibox).to_have_class(re.compile(r"tss-omnibox-generating"))

    generating_container = chat_omnibox.locator(".tss-omnibox-generating-container")
    expect(generating_container).to_be_visible()

    expect(attachment_button).not_to_be_visible()

    stop_button = chat_omnibox.locator(".tss-omnibox-chat-btn")
    expect(stop_button).to_have_class(re.compile(r"tss-btn-danger"))

    stop_button.click()

    expect(chat_omnibox).not_to_have_class(re.compile(r"tss-omnibox-generating"))

    expect(attachment_button).to_be_visible()

    expect(generating_container).not_to_be_visible()
