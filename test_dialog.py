from playwright.sync_api import sync_playwright

def run():
    with sync_playwright() as p:
        browser = p.chromium.launch()
        page = browser.new_page()
        page.goto('http://localhost:8000') # Placeholder, we need to serve it first
        browser.close()

if __name__ == '__main__':
    pass
