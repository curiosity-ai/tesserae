using Tesserae;
using Tesserae.Tests;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using Panel = Tesserae.Panel;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 10, Icon = UIcons.WindowMinimize)]
    public class PanelSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public PanelSample()
        {
            var panel = Panel().LightDismiss();

            panel.Content(
                Stack().Children(
                    TextBlock("Sample panel").MediumPlus().SemiBold(),
                    ChoiceGroup("Side:").Choices(
                        Choice("Far").Selected().OnSelected(x => panel.Side = Panel.PanelSide.Far),
                        Choice("Near").OnSelected(x => panel.Side           = Panel.PanelSide.Near)
                    ),
                    Toggle("Light Dismiss").OnChange((s, e) => panel.CanLightDismiss = s.IsChecked).Checked(panel.CanLightDismiss),
                    ChoiceGroup("Size:").Choices(
                        Choice("Small").Selected().OnSelected(x => panel.Size = Panel.PanelSize.Small),
                        Choice("Medium").OnSelected(x => panel.Size           = Panel.PanelSize.Medium),
                        Choice("Large").OnSelected(x => panel.Size            = Panel.PanelSize.Large),
                        Choice("LargeFixed").OnSelected(x => panel.Size       = Panel.PanelSize.LargeFixed),
                        Choice("ExtraLarge").OnSelected(x => panel.Size       = Panel.PanelSize.ExtraLarge),
                        Choice("FullWidth").OnSelected(x => panel.Size        = Panel.PanelSize.FullWidth)
                    ),
                    Toggle("Is non-blocking").OnChange((s,   e) => panel.IsNonBlocking   = s.IsChecked).Checked(panel.IsNonBlocking),
                    Toggle("Is dark overlay").OnChange((s,   e) => panel.IsDark          = s.IsChecked).Checked(panel.IsDark),
                    Toggle("Hide close button").OnChange((s, e) => panel.ShowCloseButton = !s.IsChecked).Checked(!panel.ShowCloseButton)
                )).SetFooter(HStack().Children(Button("Footer Button 1").Primary(), Button("Footer Button 2")));

            _content = SectionStack()
               .Title(SampleHeader(nameof(PanelSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Panels are sliding overlays typically used for creation or management tasks, such as editing a user's profile or configuring settings. They provide a large, temporary surface that slides in from either the left or right side of the screen, keeping the user within the current context while providing space for complex forms or information.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use Panels for self-contained tasks that are too large for a Dialog or Modal. Choose the 'Far' side (right) for most common actions, and 'Near' (left) for navigation-related content. Provide clear 'Save' and 'Cancel' actions in the footer. Ensure that the Panel size is appropriate for its content, using wider variants for complex forms. Use 'LightDismiss' to allow users to quickly exit by clicking outside the panel.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    Button("Open panel").OnClick((s, e) => panel.Show())));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}