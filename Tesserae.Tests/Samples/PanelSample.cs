using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.Components.UI;

namespace Tesserae.Tests.Samples
{
    public class PanelSample : IComponent
    {
        private IComponent content;

        public PanelSample()
        {
            var panel = Panel();
            content = Stack().Children(
                TextBlock("Panel").XLarge(),
                TextBlock("Overview").MediumPlus(),
                TextBlock("Panels are modal UI overlays that provide contextual app information. They often request some kind of creation or management action from the user. Panels are paired with the Overlay component, also known as a Light Dismiss. The Overlay blocks interactions with the app view until dismissed either through clicking or tapping on the Overlay or by selecting a close or completion action within the Panel."),
                TextBlock("Examples of experiences that use Panels").MediumPlus(),
                TextBlock("Member or group list creation or management"),
                TextBlock("Document list creation or management"),
                TextBlock("Permissions creation or management"),
                TextBlock("Settings creation or management"),
                TextBlock("Multi-field forms"),
                TextBlock("Best Practices").MediumPlus(),
                Stack().Horizontal().Children(
                    Stack().WidthPercents(40).Children(
                        TextBlock("Do").Medium(),
                        TextBlock("Use for self-contained experiences where the user does not need to interact with the app view to complete the task."),
                        TextBlock("Use for complex creation, edit or management experiences."),
                        TextBlock("Consider how the panel and its contained contents will scale across Fabric’s responsive web breakpoints.")
                    ),
                    Stack().WidthPercents(40).Children(
                        TextBlock("Don't").Medium(),
                        TextBlock("Don't use for experiences where the user needs to interact with the app view.")
                    )
                ),
                TextBlock("Usage").MediumPlus(),
                Button("Open panel").OnClicked((s, e) => panel.Show()),
                panel.Content(
                    Stack().Children(
                        TextBlock("Sample panel").MediumPlus().SemiBold(),
                        ChoiceGroup("Side:").Options(
                            Option("Far").Selected().OnSelected((x, e) => panel.Side = PanelSide.Far),
                            Option("Near").OnSelected((x, e) => panel.Side = PanelSide.Near)
                        ),
                        Toggle("Light Dismiss").OnChanged((s, e) => panel.CanLightDismiss = e.IsChecked),
                        ChoiceGroup("Size:").Options(
                            Option("Small").Selected().OnSelected((x, e) => panel.Size = PanelSize.Small),
                            Option("Medium").OnSelected((x, e) => panel.Size = PanelSize.Medium),
                            Option("Large").OnSelected((x, e) => panel.Size = PanelSize.Large),
                            Option("LargeFixed").OnSelected((x, e) => panel.Size = PanelSize.LargeFixed),
                            Option("ExtraLarge").OnSelected((x, e) => panel.Size = PanelSize.ExtraLarge),
                            Option("FullWidth").OnSelected((x, e) => panel.Size = PanelSize.FullWidth)
                        ),
                        Toggle("Is non-blocking").OnChanged((s, e) => panel.IsNonBlocking = e.IsChecked)
                    )
                ).Footer(Stack().Horizontal().Children(Button("Footer Button 1").Primary(), Button("Footer Button 2")))
            );
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
