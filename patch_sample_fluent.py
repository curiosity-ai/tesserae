import re

content = open("Tesserae.Tests/src/Samples/Utilities/TippySample.cs").read()

pattern = r"var tooltipConfig = new TippyConfig\s*\{\s*Header = TextBlock\(\"Header\"\)\.Medium\(\)\,\s*Content = TextBlock\(\"This is a tooltip with header and footer\"\)\,\s*Footer = TextBlock\(\"Footer\"\)\.Small\(\)\.Secondary\(\)\s*\}\;\s*var button2 = Button\(\"Tooltip with Header\/Footer\"\)\.Tooltip\(tooltipConfig\);"

new_code = """var button2 = Button("Tooltip with Header/Footer").Tooltip(config => config
                .SetHeader(TextBlock("Header").Medium())
                .SetContent(TextBlock("This is a tooltip with header and footer"))
                .SetFooter(TextBlock("Footer").Small().Secondary())
            );"""

content = re.sub(pattern, new_code, content, flags=re.DOTALL)
open("Tesserae.Tests/src/Samples/Utilities/TippySample.cs", "w").write(content)
