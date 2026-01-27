using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = UIcons.SettingsSliders)]
    public class ToggleSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ToggleSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(ToggleSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("A Toggle represents a physical switch that allows users to choose between two mutually exclusive options, typically 'on' and 'off'."),
                    TextBlock("Unlike a Checkbox, a Toggle is intended for immediate actions where the change takes effect as soon as the switch is flipped.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use Toggles for binary settings that have an immediate effect (e.g., turning Wi-Fi on or off). Labels should be short and describe the setting clearly. Avoid using Toggles when a user needs to click a 'Submit' or 'Apply' button to save changes; use a Checkbox instead. Ensure that the 'on' and 'off' states are visually distinct and easy to understand at a glance.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic Toggles"),
                    VStack().Children(
                        Label("Default (Unchecked)").SetContent(Toggle()),
                        Label("Checked").SetContent(Toggle().Checked()),
                        Label("Disabled Checked").Disabled().SetContent(Toggle().Checked().Disabled()),
                        Label("Disabled Unchecked").Disabled().SetContent(Toggle().Disabled())
                    ),
                    SampleSubTitle("Custom Labels and Inline"),
                    VStack().Children(
                        Toggle().SetText("With Label"),
                        Toggle(onText: TextBlock("Visible"), offText: TextBlock("Hidden")),
                        Label("Inline Toggle").Inline().SetContent(Toggle())
                    ),
                    SampleSubTitle("Event Handling"),
                    Toggle().SetText("Feature X").OnChange((s, e) => Toast().Information($"Feature X is now {(s.IsChecked ? "Enabled" : "Disabled")}")),
                    SampleSubTitle("Formatting"),
                    VStack().Children(
                        Toggle("Tiny").Tiny(),
                        Toggle("Small (default)").Small(),
                        Toggle("Small Plus").SmallPlus(),
                        Toggle("Medium").Medium(),
                        Toggle("Large").Large(),
                        Toggle("XLarge").XLarge(),
                        Toggle("XXLarge").XXLarge(),
                        Toggle("Mega").Mega(),
                        Toggle("Bold text").Bold()),
                    SampleSubTitle("Rounded Toggles"),
                    VStack().Children(
                        Label("Small").SetContent(Toggle().Rounded(BorderRadius.Small)),
                        Label("Medium").SetContent(Toggle().Rounded(BorderRadius.Medium)),
                        Label("Full").SetContent(Toggle().Rounded(BorderRadius.Full))
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
