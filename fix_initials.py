with open('Tesserae/src/Components/Sidebar/SidebarSegmentedPivot.cs', 'r') as f:
    content = f.read()

replacement = """
            var textBlock = tab.CreateTitle();
            if(textBlock is TextBlock tb) {
                // To avoid "Tab" vs "T", let's use the first letter, but the image shows "Tab 1".
                // Wait, if tb.Text is "Tab 1", Substring(0, 1) is "T". Why did it show "Tab \n 1"?
                // Ah, maybe the CreateTitle() returns a Stack or something, or tb.Text is not what it seems.
                // In Sample: `() => TextBlock("Tab 1").Medium()`
                // Let's just append the created title and let CSS handle overflow with ellipsis, or use `Title(tb.Text.Substring(0, 1))` properly.
                // Let's use `TextBlock(tb.Text.Substring(0, 1)).Medium()` to match style, but wait, `tb` might not be easy to clone.
                // Let's just append it, and in CSS add `overflow: hidden; text-overflow: ellipsis; white-space: nowrap;`
                titleContainerClosed.appendChild(tab.CreateTitle().Render());
            } else {
                titleContainerClosed.appendChild(tab.CreateTitle().Render());
            }
"""

new_content = content.replace("""            var textBlock = tab.CreateTitle();
            if(textBlock is TextBlock tb) {
               titleContainerClosed.appendChild(TextBlock(tb.Text.Substring(0, 1)).Render()); // abbreviated title or similar
            } else {
                titleContainerClosed.appendChild(tab.CreateTitle().Render());
            }""", """            titleContainerClosed.appendChild(tab.CreateTitle().Render());""")

with open('Tesserae/src/Components/Sidebar/SidebarSegmentedPivot.cs', 'w') as f:
    f.write(new_content)
