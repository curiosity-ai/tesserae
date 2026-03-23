from playwright.sync_api import sync_playwright, expect
import time

def test_sidebar_href():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()
        page.set_viewport_size({"width": 1280, "height": 800})

        # Navigate to the local server
        page.goto("http://localhost:8000")

        # Wait for the app to load
        page.wait_for_selector(".tss-sidebar")

        # Open the sidebar sample
        search_box = page.get_by_placeholder("Search...")
        search_box.wait_for(state="visible", timeout=10000)
        search_box.fill("Sidebar")

        # Click the sidebar sample item
        sidebar_item = page.get_by_text("Sidebar", exact=True).first
        sidebar_item.click()

        # Wait for the sidebar sample content to render
        page.wait_for_selector("text='A fully featured Sidebar with Search, Navigation, Buttons, and Separators.'")

        # Hover the CURIOSITY_REF button so its commands appear
        curiosity_btn = page.locator("#CURIOSITY_REF").first
        curiosity_btn.hover()

        # Wait for the commands container to be visible
        commands_container = curiosity_btn.locator("xpath=ancestor::div[contains(@class, 'tss-sidebar-btn-open')]//div[contains(@class, 'tss-sidebar-commands')]")
        commands_container.wait_for(state="visible")

        # Check that the CURIOSITY_REF button has an href
        button_link = curiosity_btn.locator("xpath=ancestor::a").first
        expect(button_link).to_have_attribute("href", "https://curiosity.ai")
        print("Verified SidebarButton href.")

        # Check that the command button inside has an href
        command_link = commands_container.locator("a").first
        expect(command_link).to_have_attribute("href", "https://github.com/curiosity-ai/tesserae")
        print("Verified SidebarCommand href.")

        # Take a screenshot showing the hovered state
        time.sleep(1) # Allow tooltip/hover effects to settle
        page.screenshot(path="/tmp/sidebar_href_verification.png")

        browser.close()

if __name__ == "__main__":
    test_sidebar_href()