with open('Tesserae/h5/assets/css/tss.sidebar.css', 'r') as f:
    css = f.read()

replacement = """
.tss-sidebar-segmentedpivot-wrapper-closed .tss-segmentedpivot-titlebar {
    flex-direction: column !important;
    padding: 2px;
}
"""

with open('Tesserae/h5/assets/css/tss.sidebar.css', 'w') as f:
    f.write(css.replace('.tss-sidebar-segmentedpivot-wrapper-closed .tss-segmentedpivot-titlebar {\n    flex-direction: column;\n    padding: 2px;\n}', replacement))
