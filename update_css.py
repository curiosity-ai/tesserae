with open('Tesserae/h5/assets/css/tss.sidebar.css', 'r') as f:
    css = f.read()

# Let's fix the closed tab to render initials properly if needed
# The closed tab "Tab 1" is using TextBlock with text "Tab 1".
# But wait, in `SidebarSegmentedPivot.cs` I actually wrote:
# if(textBlock is TextBlock tb) { titleContainerClosed.appendChild(TextBlock(tb.Text.Substring(0, 1)).Render()); }
# Let's check `SidebarSegmentedPivot.cs`
