from playwright.sync_api import sync_playwright

def verify(page):
    page.goto("http://localhost:8000/index.html")
    page.get_by_placeholder("Search...").fill("Task")
    page.get_by_text("Task Board", exact=True).click()
    page.wait_for_timeout(1000)
    page.screenshot(path="/tmp/taskboard_col.png", full_page=True)

    page.locator(".tss-icon-toggle-item").nth(1).click()
    page.wait_for_timeout(1000)
    page.screenshot(path="/tmp/taskboard_row.png", full_page=True)

with sync_playwright() as p:
    browser = p.chromium.launch()
    page = browser.new_page()
    verify(page)
    browser.close()
