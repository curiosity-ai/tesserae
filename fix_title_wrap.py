with open('Tesserae/h5/assets/css/tss.sidebar.css', 'r') as f:
    css = f.read()

# Make the closed wrapper overflow hidden, and fix text rendering
replacement = """
.tss-sidebar-segmentedpivot-wrapper-closed .tss-segmentedpivot-tab {
    padding: 4px 4px;
    margin-bottom: 2px;
    flex-grow: 1;
    text-align: center;
    min-width: 20px;
    overflow: hidden;
    text-overflow: clip;
    white-space: nowrap;
    word-break: keep-all;
}

.tss-sidebar-segmentedpivot-wrapper-closed .tss-segmentedpivot-tab:last-child {
    margin-bottom: 0px;
}
"""

# Let's just override it all at the bottom
with open('Tesserae/h5/assets/css/tss.sidebar.css', 'a') as f:
    f.write(replacement)
