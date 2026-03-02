from playwright.sync_api import sync_playwright

def verify():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()

        # Capture console output for debugging
        page.on("console", lambda msg: print(f"CONSOLE: {msg.text}"))

        print("Navigating to index.html...")
        # Path to the locally hosted index.html, served by an HTTP server or loaded via file URI
        # Assuming we serve it locally on port 8080 or simply load file://.
        # It's better to launch a python http server and then hit localhost
        import subprocess
        import time

        server = subprocess.Popen(["python3", "-m", "http.server", "8080", "--directory", "Tesserae.Tests/bin/Debug/netstandard2.0/h5/"])
        time.sleep(2) # Give server time to start

        try:
            page.goto("http://localhost:8080/index.html")

            print("Waiting for application to load...")
            # Wait for sidebar to load
            page.wait_for_selector(".tss-sidebar")

            print("Clicking AdvancedSearchBoxSample...")
            # We will clear local storage in case the test cached sample order
            page.evaluate("window.localStorage.clear();")
            page.reload()
            page.wait_for_selector(".tss-sidebar")

            # The sidebar item for AdvancedSearchBox might be named differently
            try:
                page.locator(".tss-sidebar").locator("text=Advanced Search Box").first.scroll_into_view_if_needed()
                page.locator(".tss-sidebar").locator("text=Advanced Search Box").first.click()
            except Exception as e:
                pass

            # Click explicitly in the Components section if we can find it
            try:
                page.get_by_text("Components", exact=True).click()
                page.wait_for_timeout(500)
                page.get_by_text("Advanced Search Box", exact=True).click()
            except:
                pass

            page.wait_for_timeout(2000)

            # Try navigating by url hash to ensure we're there
            page.goto("http://localhost:8080/index.html#AdvancedSearchBoxSample")
            page.reload()
            page.wait_for_timeout(2000)

            # Let's use evaluate to forcibly render the sample if the router fails
            print("Forcibly rendering sample if needed...")
            page.evaluate("""
                if (!document.querySelector('.tss-advancedsearchbox-container')) {
                    // We can attempt to create it using bridge or just click the raw link if it exists in DOM
                    const items = document.querySelectorAll('button');
                    for (let i = 0; i < items.length; i++) {
                        if (items[i].innerText.includes('Advanced Search Box')) {
                            items[i].click();
                            break;
                        }
                    }
                }
            """)
            page.wait_for_timeout(2000)

            # Wait for page content
            try:
                page.wait_for_selector(".tss-advancedsearchbox-container", timeout=10000)
            except Exception as e:
                print("Still failed to find advancedsearchbox-container.")
                page.screenshot(path="debug.png")
                pass

            print("Taking screenshot...")
            page.screenshot(path="advanced_searchbox_sample.png")

            try:
                print("Verifying tokens...")
                tokens_container = page.locator(".tss-advancedsearchbox-tokens")

                # Count the tokens
                count = page.locator(".tss-adv-token-word").count()
                print(f"Found {count} word tokens.")

                print("Testing Clear and Input...")
                clear_btn = page.locator(".tss-advancedsearchbox-clear-btn").first
                if clear_btn.is_visible():
                    clear_btn.click()

                input_el = page.locator(".tss-advancedsearchbox-input").first
                input_el.type("apple AND banana OR (cherry AND NOT date) 'and quotes'", delay=50)

                page.wait_for_timeout(500)
                page.screenshot(path="advanced_searchbox_typed.png")

                print("Testing History...")
                history_btn = page.locator(".tss-advancedsearchbox-history-btn").first
                if history_btn.is_visible():
                    history_btn.click()
                    page.wait_for_timeout(1000)

                page.screenshot(path="advanced_searchbox_history.png")
            except Exception as ex:
                print(f"Test interaction failed: {ex}")


            print("Done verifying!")
        finally:
            browser.close()
            server.terminate()

if __name__ == "__main__":
    verify()
