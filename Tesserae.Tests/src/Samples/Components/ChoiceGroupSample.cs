using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = UIcons.List)]
    public class ChoiceGroupSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ChoiceGroupSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(ChoiceGroupSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("ChoiceGroups, also known as radio button groups, allow users to select exactly one option from a set of mutually exclusive choices."),
                    TextBlock("They emphasize all options equally, which can be useful when you want to ensure the user considers all available alternatives before making a selection.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use ChoiceGroups when there are between 2 and 7 options and screen space is available. For more than 7 options, a Dropdown is typically more efficient. List options in a logical order (e.g., most likely to least likely). Align options vertically whenever possible for better readability and localization support. Always provide a default selection if one is significantly more likely than the others, and ensure the safest option is the default if applicable.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic ChoiceGroup"),
                    ChoiceGroup().Choices(
                        Choice("Option A"),
                        Choice("Option B"),
                        Choice("Option C").Disabled(),
                        Choice("Option D")),
                    SampleSubTitle("Required with Label"),
                    ChoiceGroup("Select an environment").Required().Choices(
                        Choice("Development"),
                        Choice("Staging"),
                        Choice("Production")),
                    SampleSubTitle("Horizontal Layout"),
                    ChoiceGroup("Sizes").Horizontal().Choices(
                        Choice("Small"),
                        Choice("Medium").Medium(),
                        Choice("Large").Large()),
                    SampleSubTitle("Event Handling"),
                    ChoiceGroup("Language").Choices(
                        Choice("English"),
                        Choice("Spanish"),
                        Choice("French")
                    ).OnChange((s, e) => Toast().Information($"Selected: {s.SelectedOption.Text}")),
                    SampleSubTitle("Formatting"),
                    ChoiceGroup("Pick a style").Choices(
                        Choice("Tiny").Tiny(),
                        Choice("Small (default)").Small(),
                        Choice("Small Plus").SmallPlus(),
                        Choice("Medium").Medium(),
                        Choice("Large").Large(),
                        Choice("XLarge").XLarge(),
                        Choice("XXLarge").XXLarge(),
                        Choice("Mega").Mega(),
                        Choice("Bold").Bold()
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
