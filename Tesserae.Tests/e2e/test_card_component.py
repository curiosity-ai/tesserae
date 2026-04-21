from playwright.sync_api import sync_playwright

def test_card_with_header():
    with sync_playwright() as p:
        browser = p.chromium.launch()
        page = browser.new_page()
        page.goto('http://localhost:5000/#/view/Metric')

        # Wait for the sample page to load and check if the card is displayed
        page.wait_for_selector('.tss-card.tss-has-header')

        # Verify the header structure exists
        assert page.query_selector('.tss-card-header') is not None
        assert page.query_selector('.tss-card-title') is not None
        assert page.query_selector('.tss-card-tag') is not None

        # Get specific text to verify
        title_element = page.query_selector('.tss-card-title')
        tag_element = page.query_selector('.tss-card-tag')

        assert title_element.inner_text() == 'Metrics'
        assert tag_element.inner_text() == 'Last 24 hours'

        browser.close()

if __name__ == "__main__":
    test_card_with_header()
