from playwright.sync_api import sync_playwright
import time

def test_sidebar_segmented_pivot():
    with sync_playwright() as p:
        browser = p.chromium.launch()
        page = browser.new_page()
        page.goto('http://127.0.0.1:8080/#/view/Sidebar')

        # Wait for rendering
        page.wait_for_selector('.tss-sidebar-segmentedpivot-open .tss-segmentedpivot-tab')
        time.sleep(1)

        # Click Tab 2
        page.evaluate('''() => {
            let tabs = document.querySelectorAll('.tss-sidebar-segmentedpivot-open .tss-segmentedpivot-tab');
            if (tabs.length > 1) {
                tabs[1].click();
            }
        }''')
        time.sleep(1)

        # Force sidebar closed
        page.evaluate('''() => {
            let sidebars = document.querySelectorAll('.tss-sidebar');
            sidebars.forEach(s => s.classList.add('tss-sidebar-closed'));
        }''')
        time.sleep(1)

        # Click Tab 1 using JS in closed state
        page.evaluate('''() => {
            let tabs = document.querySelectorAll('.tss-sidebar-segmentedpivot-closed .tss-segmentedpivot-tab');
            if (tabs.length > 0) {
                tabs[0].click();
            }
        }''')
        time.sleep(1)

        browser.close()
        print("Playwright test completed successfully.")

if __name__ == "__main__":
    test_sidebar_segmented_pivot()
