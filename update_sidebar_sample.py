import re

with open('Tesserae.Tests/src/Samples/Components/SidebarSample.cs', 'r') as f:
    content = f.read()

replacement = """
            var tabs = new SidebarSegmentedPivot("tabs")
                .Add("tab1", () => TextBlock("1").Medium(),
                    new SidebarButton("t1_btn1", UIcons.Rocket, "Launch"),
                    new SidebarButton("t1_btn2", UIcons.Rocket, "Launch 2"))
                .Add("tab2", () => TextBlock("2").Medium(),
                    new SidebarButton("t2_btn1", UIcons.Globe, "World"),
                    new SidebarButton("t2_btn2", UIcons.Globe, "World 2"));

            sidebar.AddContent(tabs);

            var settingsNav = new SidebarNav("settings", UIcons.Settings, "Settings", true);
"""

new_content = re.sub(r'            var tabs = new SidebarSegmentedPivot\("tabs"\).*?            var settingsNav = new SidebarNav\("settings", UIcons\.Settings, "Settings", true\);', replacement, content, flags=re.DOTALL)

with open('Tesserae.Tests/src/Samples/Components/SidebarSample.cs', 'w') as f:
    f.write(new_content)
