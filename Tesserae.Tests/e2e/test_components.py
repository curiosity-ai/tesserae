import re
from playwright.sync_api import Page, expect
import time

def test_avatar_gradient_hash(page: Page):
    """
    Tests that the Avatar component correctly applies a linear-gradient background
    when no image is provided, acting as a fallback for initials.
    """
    page.goto("http://localhost:8000")

    # Wait for the app to load
    page.wait_for_load_state("networkidle")

    # Navigate to Avatar Sample
    search = page.get_by_placeholder("Search...")
    search.fill("Avatar")
    page.get_by_text("Avatar", exact=True).click()

    # Find the 'Initials Fallback' section and its avatars
    fallback_heading = page.get_by_text("Initials Fallback", exact=True)
    expect(fallback_heading).to_be_visible()

    # Need a small delay for render sometimes
    time.sleep(1)

    # Check that at least one avatar has a linear-gradient background applied to its style
    # Wait until we see JD in the fallback block
    jd_avatar_fallback = page.locator(".tss-avatar", has_text="MW").first
    expect(jd_avatar_fallback).to_be_visible()

    # Retrieve the inline style background to verify it contains linear-gradient
    background_style = jd_avatar_fallback.evaluate("el => el.style.background")
    assert "linear-gradient" in background_style, f"Expected linear-gradient in background, got: '{background_style}'"


def test_task_board_modes(page: Page):
    """
    Tests that the TaskBoard component can switch between Column Mode and Card Mode.
    """
    page.goto("http://localhost:8000")
    page.wait_for_load_state("networkidle")

    # Navigate to Task Board Sample
    search = page.get_by_placeholder("Search...")
    search.fill("Task Board")
    page.get_by_text("Task Board", exact=True).click()

    # Verify we are on the TaskBoard page
    expect(page.get_by_text("TaskBoard provides a Trello-like interface")).to_be_visible()

    # The columns should be visible
    expect(page.get_by_text("To Do")).to_be_visible()
    expect(page.get_by_text("In Progress")).to_be_visible()
    expect(page.get_by_text("Done")).to_be_visible()

    # And some cards
    expect(page.get_by_text("Research user needs")).to_be_visible()
    expect(page.get_by_text("Implement TaskBoard component")).to_be_visible()

    # Get the inner TaskBoard stack, which wraps in Card mode
    board_stack = page.locator(".tss-taskboard")

    # By default, Column mode (no wrap)
    expect(board_stack).not_to_have_class(re.compile(r".*tss-taskboard-card-mode.*"))

    # Toggle to Card Mode
    card_mode_toggle = page.get_by_text("Card Mode")
    card_mode_toggle.click()

    # Verify that the card mode class is applied
    expect(board_stack).to_have_class(re.compile(r".*tss-taskboard-card-mode.*"))