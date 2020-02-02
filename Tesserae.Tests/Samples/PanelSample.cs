using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;
using Panel = Tesserae.Components.Panel;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    public class PanelSample : IComponent
    {
        private IComponent _content;

        public PanelSample()
        {
            var panel = Panel();
            panel.CanLightDismiss = true;
            _content = SectionStack()
            .Title(TextBlock("Panel").XLarge().Bold())
            .Section(Stack().Children(
                SampleTitle("Overview"),
                TextBlock("Panels are modal UI overlays that provide contextual app information. They often request some kind of creation or management action from the user. Panels are paired with the Overlay component, also known as a Light Dismiss. The Overlay blocks interactions with the app view until dismissed either through clicking or tapping on the Overlay or by selecting a close or completion action within the Panel."),
                TextBlock("Examples of experiences that use Panels").MediumPlus(),
                TextBlock("Member or group list creation or management"),
                TextBlock("Document list creation or management"),
                TextBlock("Permissions creation or management"),
                TextBlock("Settings creation or management"),
                TextBlock("Multi-field forms")))
            .Section(Stack().Children(
                SampleTitle("Best Practices"),
                Stack().Horizontal().Children(
                Stack().Width(40, Unit.Percents).Children(
                    SampleSubTitle("Do"),
                    SampleDo("Use for self-contained experiences where the user does not need to interact with the app view to complete the task."),
                    SampleDo("Use for complex creation, edit or management experiences."),
                    SampleDo("Consider how the panel and its contained contents will scale across Fabric’s responsive web breakpoints.")
            ),
            Stack().Width(40, Unit.Percents).Children(
                SampleSubTitle("Don't"),
                SampleDont("Don't use for experiences where the user needs to interact with the app view.")))))
            .Section(Stack().Children(
                SampleTitle("Usage"),
                Button("Open panel").OnClick((s, e) => panel.Show()),
                panel.Content(
                Stack().Children(
                    TextBlock("Sample panel").MediumPlus().SemiBold(),
                    ChoiceGroup("Side:").Options(
                        Option("Far").Selected().OnSelected((x, e) => panel.Side = Panel.PanelSide.Far),
                        Option("Near").OnSelected((x, e) => panel.Side = Panel.PanelSide.Near)
                    ),
                    Toggle("Light Dismiss").OnChange((s, e) => panel.CanLightDismiss = s.IsChecked).Checked(panel.CanLightDismiss),
                    ChoiceGroup("Size:").Options(
                        Option("Small").Selected().OnSelected((x, e) => panel.Size = Panel.PanelSize.Small),
                        Option("Medium").OnSelected((x, e) => panel.Size = Panel.PanelSize.Medium),
                        Option("Large").OnSelected((x, e) => panel.Size = Panel.PanelSize.Large),
                        Option("LargeFixed").OnSelected((x, e) => panel.Size = Panel.PanelSize.LargeFixed),
                        Option("ExtraLarge").OnSelected((x, e) => panel.Size = Panel.PanelSize.ExtraLarge),
                        Option("FullWidth").OnSelected((x, e) => panel.Size = Panel.PanelSize.FullWidth)
                    ),
                    Toggle("Is non-blocking").OnChange((s, e) => panel.IsNonBlocking = s.IsChecked).Checked(panel.IsNonBlocking),
                    Toggle("Is dark overlay").OnChange((s, e) => panel.Dark = s.IsChecked).Checked(panel.Dark),
                    Toggle("Hide close button").OnChange((s, e) => panel.ShowCloseButton = !s.IsChecked).Checked(!panel.ShowCloseButton)
                    )).Footer(Stack().Horizontal().Children(Button("Footer Button 1").Primary(), Button("Footer Button 2")))));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
