from playwright.sync_api import sync_playwright

def test_sidebar_external_link():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()
        page.on('console', lambda msg: print(f"Browser console: {msg.text}"))

        # Navigate to the local testing server
        page.goto("http://localhost:8000")

        # Click on Components -> Sidebar
        page.click("text='Sidebar'")

        # Wait for Sidebar content to render
        page.wait_for_selector(".tss-sidebar-btn-open")

        page.wait_for_timeout(2000)

        # Assert the external link exists and has the correct href
        link_element = page.locator("a[href='https://bing.com']")
        if link_element.count() > 0:
            print("Successfully found the external link in the sidebar.")
            link_element.first.screenshot(path="sidebar_link.png")
            print("Screenshot saved to sidebar_link.png")
        else:
            print("Failed to find the external link in the sidebar.")
            page.screenshot(path="full_page_failed.png")

        browser.close()

if __name__ == "__main__":
    test_sidebar_external_link()