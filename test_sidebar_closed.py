from playwright.sync_api import sync_playwright
import time

with sync_playwright() as p:
    browser = p.chromium.launch()
    page = browser.new_page()
    page.goto('http://127.0.0.1:8080/#/view/Sidebar')
    time.sleep(2)

    # We want to force the sample's sidebar to close so we can test the closed rendering of SidebarSegmentedPivot
    page.evaluate('''() => {
        const sidebars = document.querySelectorAll('.tss-sidebar');
        if(sidebars.length > 1) {
            sidebars[1].classList.add('tss-sidebar-closed');
        }
    }''')
    time.sleep(1)

    # When it's closed, the text inside is truncated and might not be visible to Playwright click?
    # Let's just click using JS
    page.evaluate('''() => {
        const tabs = document.querySelectorAll('.tss-sidebar-segmentedpivot-wrapper-closed .tss-segmentedpivot-tab');
        if(tabs.length > 1) {
            tabs[1].click();
        }
    }''')
    time.sleep(1)

    page.screenshot(path='sidebar_sample_closed_tab2.png', full_page=True)
    browser.close()
