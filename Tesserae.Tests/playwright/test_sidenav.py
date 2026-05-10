import os
import time

from playwright.sync_api import sync_playwright


CHROME_PATH = os.environ.get(
    "PLAYWRIGHT_CHROME_PATH",
    "/opt/pw-browsers/chromium-1194/chrome-linux/chrome",
)


def test_sidenav():
    with sync_playwright() as p:
        launch_kwargs = {}
        if CHROME_PATH and os.path.exists(CHROME_PATH):
            launch_kwargs["executable_path"] = CHROME_PATH

        browser = p.chromium.launch(**launch_kwargs)
        page = browser.new_page(viewport={"width": 1400, "height": 900})

        page.goto("http://127.0.0.1:8080/#/view/Sidenav")

        # Wait for the Sidenav to render
        page.wait_for_selector(".tss-sidenav", timeout=15000)
        time.sleep(0.5)

        # Take a screenshot of the initial 'Build' selected state for visual review
        initial_screenshot_path = os.path.join(
            os.path.dirname(__file__), "sidenav-initial-screenshot.png"
        )
        page.screenshot(path=initial_screenshot_path, full_page=True)
        print(f"Initial screenshot saved to {initial_screenshot_path}")

        # The rail should contain header + middle + footer sections
        assert page.locator(".tss-sidenav-header").count() >= 1
        assert page.locator(".tss-sidenav-middle").count() >= 1
        assert page.locator(".tss-sidenav-footer").count() >= 1

        # The sample page has 2 Sidenavs (combined + standalone) and 2 Sidebars
        # (the app's outer Sidebar + the demo Sidebar). Index 0 of .tss-sidenav is
        # the demo combined with a Sidebar; the demo Sidebar is index 1 of
        # .tss-sidebar.
        demo_sidenav = page.locator(".tss-sidenav").nth(0)
        demo_sidebar = page.locator(".tss-sidebar").nth(1)

        demo_middle = demo_sidenav.locator(".tss-sidenav-middle")
        items = demo_middle.locator(".tss-sidenav-btn-wrap")
        assert items.count() == 5, f"expected 5 items in middle, got {items.count()}"

        # Build should be selected initially
        selected = demo_sidenav.locator(".tss-sidenav-btn-wrap.tss-sidenav-selected")
        assert selected.count() == 1
        selected_label = selected.locator(".tss-sidenav-btn-label").inner_text()
        assert selected_label == "Build", f"expected 'Build', got '{selected_label}'"

        # Operate should show a dot indicator
        operate_dot = demo_middle.locator(
            ".tss-sidenav-btn-wrap:has(.tss-sidenav-btn-label:text('Operate')) "
            ".tss-sidenav-btn-dot.tss-sidenav-btn-dot-visible"
        )
        assert operate_dot.count() == 1, "Operate item should have a visible dot"

        # Click 'Govern' and verify selection moves
        govern = demo_middle.locator(
            ".tss-sidenav-btn-wrap:has(.tss-sidenav-btn-label:text('Govern')) .tss-sidenav-btn"
        )
        govern.first.click()
        time.sleep(0.5)

        selected = demo_sidenav.locator(".tss-sidenav-btn-wrap.tss-sidenav-selected")
        assert selected.count() == 1
        selected_label = selected.locator(".tss-sidenav-btn-label").inner_text()
        assert selected_label == "Govern", f"expected 'Govern', got '{selected_label}'"

        # Demo Sidebar (to the right of the rail) should now show "Govern" content
        assert demo_sidebar.locator("text=Govern").count() >= 1, (
            "demo sidebar should reflect 'Govern' selection"
        )

        # Click 'Home' and verify
        home = demo_middle.locator(
            ".tss-sidenav-btn-wrap:has(.tss-sidenav-btn-label:text('Home')) .tss-sidenav-btn"
        )
        home.first.click()
        time.sleep(0.5)

        selected_label = (
            demo_sidenav.locator(".tss-sidenav-btn-wrap.tss-sidenav-selected")
            .locator(".tss-sidenav-btn-label").inner_text()
        )
        assert selected_label == "Home", f"expected 'Home', got '{selected_label}'"

        # The standalone Sidenav should also be present
        standalone = page.locator(".tss-sidenav").nth(1)
        standalone_items = standalone.locator(".tss-sidenav-middle .tss-sidenav-btn-wrap")
        assert standalone_items.count() == 4, (
            f"expected 4 items in standalone, got {standalone_items.count()}"
        )

        # Take a screenshot for visual inspection
        screenshot_path = os.path.join(
            os.path.dirname(__file__), "sidenav-screenshot.png"
        )
        page.screenshot(path=screenshot_path, full_page=True)
        print(f"Screenshot saved to {screenshot_path}")

        browser.close()
        print("Sidenav playwright test completed successfully.")


if __name__ == "__main__":
    test_sidenav()
