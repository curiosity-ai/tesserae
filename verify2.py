from playwright.sync_api import sync_playwright

def main():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()
        page.goto('http://localhost:8000')
        page.wait_for_selector('text="Tesserae"')

        # Use the search box to filter the sidebar
        search_box = page.get_by_placeholder("Search...")
        search_box.fill("Force Directed Graph")

        # Wait a moment for filtering to happen
        page.wait_for_timeout(500)

        # Click the link
        page.locator('text="Force Directed Graph"').click()

        # Wait for the component to render
        page.wait_for_timeout(1000)

        page.screenshot(path='/home/jules/verification/force_directed_graph_final2.png')
        browser.close()

if __name__ == "__main__":
    main()
