using System;
using System.Collections.Generic;
using H5.Core;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Showcase
{
    internal static class App
    {
        private static void Main()
        {
            // Sidebar
            var sidebar = Sidebar(sortable: false);

            // Custom header for Tesserae brand
            sidebar.AddHeader(new SidebarText("HEADER", "Tesserae", textSize: TextSize.Large, textWeight: TextWeight.SemiBold));

            sidebar.AddHeader(new SidebarSearchBox("search", "Search..."));

            sidebar.AddContent(new SidebarButton("HOME", UIcons.HouseBlank, "Home").Selected());
            sidebar.AddContent(new SidebarButton("INSTALLATION", UIcons.Download, "Installation"));
            sidebar.AddContent(new SidebarButton("CONTRIBUTING", UIcons.CodeBranch, "Contributing"));
            sidebar.AddContent(new SidebarButton("COLORS", UIcons.Palette, "Colors"));
            sidebar.AddContent(new SidebarButton("ACCESSIBILITY", UIcons.UniversalAccess, "Accessibility"));
            sidebar.AddContent(new SidebarButton("FIGMA", UIcons.BrandsFigma, "Figma Resources"));
            sidebar.AddContent(new SidebarButton("CLI", UIcons.Terminal, "CLI"));
            sidebar.AddContent(new SidebarButton("REGISTRY", UIcons.Archive, "Registry"));

            // Components Section
            sidebar.AddContent(new SidebarSeparator("SEP_COMPONENTS", "Components"));

            var components = new[] { "Badge", "Banner", "Breadcrumbs", "Button", "Checkbox", "Clipboard Text", "Cloudflare Logo", "Code" };
            foreach (var c in components)
            {
                 sidebar.AddContent(new SidebarButton(c.ToUpper(), UIcons.CircleSmall, c));
            }

            // Mobile Sidebar Logic
            var sidebarElement = sidebar.Render();
            var overlay = Div(_("mobile-overlay"));

            // Close sidebar on overlay click
            overlay.onclick = (e) => {
                sidebarElement.classList.remove("mobile-open");
                overlay.classList.remove("visible");
            };

            var hamburger = Button().SetIcon(UIcons.MenuBurger).Class("hamburger-menu").NoBackground().Style(s => s.fontSize = "20px").OnClick(() => {
                sidebarElement.classList.add("mobile-open");
                overlay.classList.add("visible");
            });


            // Button with spinner
            var spinnerButton = Button("Create Worker");
            spinnerButton.ToSpinner("Create Worker");

            // Invalid textbox
            var invalidTextBox = TextBox("Invalid!");
            invalidTextBox.Validation(v => "Invalid!");
            invalidTextBox.Error = "Invalid!";

            // Invalid email
            var invalidEmail = TextBox("name@example.com").SetPlaceholder("name@example.com");
            invalidEmail.Validation(v => "Invalid email");
            invalidEmail.Error = "Invalid email";

            // Responsive Grid Container
            var gridDiv = Div(_("responsive-grid"));

            // Helper to add to grid
            void AddToGrid(string title, IComponent content)
            {
                gridDiv.appendChild(ComponentCard(title, content).Render());
            }

            AddToGrid("Button",
               VStack().Style(s => s.gap = "16px").AlignItems(ItemAlign.Center).Children(
                   Button("Create Worker").NoBackground().Style(s => s.border = "1px solid #e0e0e0"),
                   Button("Create Worker").Primary(),
                   spinnerButton
               )
            );

            AddToGrid("Input",
               VStack().Style(s => s.gap = "16px").W(100.percent()).Children(
                   TextBox().SetPlaceholder("Type something..."),
                   invalidTextBox
               )
            );

            AddToGrid("Select",
               VStack().W(100.percent()).Children(
                   Dropdown().Items(new Dropdown.Item("Select a version..."))
               )
            );

            AddToGrid("Combobox",
               VStack().W(100.percent()).Children(
                   Dropdown().Items(new Dropdown.Item("Select an issue..."))
               )
            );

            AddToGrid("Switch",
                VStack().AlignItems(ItemAlign.Center).Children(
                    new Toggle(TextBlock(""), TextBlock("")).Checked()
                )
            );

            AddToGrid("Input (with validation)",
               VStack().Style(s => s.gap = "8px").W(100.percent()).Children(
                   Label("Email").SemiBold(),
                   invalidEmail
               )
            );

            // Extra placeholders
            AddToGrid("Dialog", TextBlock("Dialog Content"));
            AddToGrid("Tooltip", TextBlock("Tooltip Content"));
            AddToGrid("Dropdown", TextBlock("Dropdown Content"));


            // Main Content
            var mainContent = VStack().S().Children(
                // Top Bar
                HStack().W(100.percent()).H(64.px()).PL(32.px()).PR(32.px())
                    .Style(s => s.borderBottom = "1px solid #eee")
                    .AlignItems(ItemAlign.Center)
                    .JustifyContent(ItemJustify.Between)
                    .Children(
                        HStack().AlignItems(ItemAlign.Center).Children(
                            hamburger,
                            TextBlock("") // Spacer/Placeholder
                        ),
                        HStack().Style(s => s.gap = "16px").Children(
                            TextBlock("@tesserae/ui v1.0.0", textSize: TextSize.Small).Secondary(),
                            Icon(UIcons.Moon).Style(s => s.cursor = "pointer")
                        )
                    ),
                // Content Area
                VStack().W(100.percent()).Grow().ScrollY().Padding("32px").Background("#fafafa").Children(
                   Raw(gridDiv)
                )
            );

            // Layout
            var layout = HStack().S().Children(sidebar.HS(), mainContent.Grow());

            document.body.appendChild(overlay);
            MountToBody(layout);
        }

        private static IComponent ComponentCard(string title, IComponent content)
        {
            return VStack().Class("showcase-card").Children(
                TextBlock(title).SemiBold().MB(24),
                VStack().Grow().JustifyContent(ItemJustify.Center).Children(content)
            ).H(300.px());
        }
    }
}
