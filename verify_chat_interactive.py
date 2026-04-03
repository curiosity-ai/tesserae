from playwright.sync_api import sync_playwright
import time

def verify_chat_interactive():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        # Record video to capture the animation
        context = browser.new_context(record_video_dir="videos/")
        page = context.new_page()

        # Open the index page
        page.goto('http://localhost:5000')
        page.wait_for_load_state('networkidle')

        # Search and navigate to Chat
        search_box = page.get_by_placeholder("Search...")
        search_box.fill("Chat")
        time.sleep(1)
        page.click("text='Chat'")

        # Wait for chat area
        page.wait_for_selector('.tss-chatarea', timeout=5000)

        # Type a message in the OmniBox and send
        # The OmniBox has a placeholder "Type a message..."
        chat_input = page.get_by_placeholder("Type a message...")
        chat_input.fill("What is the meaning of life?")

        # Click the send button (it has text "Send")
        page.click("text='Send'")

        # Wait a few seconds to let the AI response stream in
        time.sleep(4)

        # Output final screenshot
        page.screenshot(path='chat_interactive_final.png', full_page=True)

        context.close()
        browser.close()

verify_chat_interactive()
