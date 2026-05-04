from playwright.sync_api import sync_playwright
import time

with sync_playwright() as p:
    browser = p.chromium.launch()
    page = browser.new_page()
    page.goto('http://127.0.0.1:8080/#/view/Sidebar')
    time.sleep(2)

    # Click Tab 2
    page.click('text=Tab 2')
    time.sleep(1)
    page.screenshot(path='sidebar_sample_tab2.png', full_page=True)

    # We can try toggling the main app sidebar collapse, which has an icon "ChevronLeft"
    page.click('.tss-sidebar-btn') # Let's just click the first sidebar button that might be a toggle, or find the ChevronLeft
    time.sleep(1)
    page.screenshot(path='sidebar_sample_closed.png', full_page=True)

    browser.close()
