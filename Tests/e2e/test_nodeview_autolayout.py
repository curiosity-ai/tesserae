import pytest
from playwright.sync_api import sync_playwright, expect
import time

def test_nodeview_autolayout():
    with sync_playwright() as p:
        browser = p.chromium.launch()
        page = browser.new_page()

        # Go to Node View sample
        page.goto("http://127.0.0.1:8080/#/view/Node%20View")

        # Wait for component to load
        page.wait_for_selector(".baklava-editor")

        # Ensure the custom Auto Layout button is present
        btn = page.locator(".custom-toolbar .baklava-toolbar-button")
        expect(btn).to_be_visible(timeout=10000)

        # Get position of nodes before auto layout
        time.sleep(2) # Wait for node view to fully render
        nodes_before = page.locator(".baklava-node").all()
        positions_before = [node.bounding_box() for node in nodes_before]

        # Click Auto Layout
        btn.click()

        # Wait for layout to update
        time.sleep(2)

        # Get position of nodes after auto layout
        nodes_after = page.locator(".baklava-node").all()
        positions_after = [node.bounding_box() for node in nodes_after]

        assert positions_before != positions_after, f"Nodes should have moved. Before: {positions_before}, After: {positions_after}"

        # Verify that nodes are actually present
        assert len(nodes_after) >= 2, "There should be at least two nodes rendered"

        browser.close()
