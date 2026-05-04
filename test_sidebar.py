from playwright.sync_api import sync_playwright
import time

with sync_playwright() as p:
    browser = p.chromium.launch()
    page = browser.new_page()
    page.goto('http://127.0.0.1:8080/#/view/Sidebar')
    time.sleep(3) # Wait for rendering
    page.screenshot(path='sidebar_sample.png', full_page=True)
    browser.close()
