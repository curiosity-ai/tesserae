using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class PickerSample : IComponent
    {
        private readonly IComponent _content;

        public PickerSample()
        {
            _content =
                SectionStack()
                    .Title(
                        TextBlock("Picker")
                            .XLarge()
                            .Bold())
                    .Section(
                        Stack()
                            .Children(
                                SamplesHelper.SampleTitle("Overview"),
                                TextBlock("DetailsList is a derivative of the List component. It is a robust way to " +
                                             "display an information rich collection of items. It can support powerful " +
                                             "ways to aid a user in finding content with sorting, grouping and " +
                                             "filtering.  Lists are a great way to handle large amounts of content, " +
                                             "but poorly designed Lists can be difficult to parse.")
                                    .PaddingBottom(16.px()),
                                TextBlock("Use a DetailsList when density of information is critical. Lists can " +
                                             "support single and multiple selection, as well as drag and drop and " +
                                             "marquee selection. They are composed of a column header, which " +
                                             "contains the metadata fields which are attached to the list items, " +
                                             "and provide the ability to sort, filter and even group the list. " +
                                             "List items are composed of selection, icon, and name columns at " +
                                             "minimum. One can also include other columns such as Date Modified, or " +
                                             "any other metadata field associated with the collection. Place the most " +
                                             "important columns from left to right for ease of recall and comparison.")
                                    .PaddingBottom(16.px()),
                                TextBlock("DetailsList is classically used to display files, but is also used to " +
                                             "render custom lists that can be purely metadata. Avoid using file type " +
                                             "icon overlays to denote status of a file as it can make the entire icon " +
                                             "unclear. Be sure to leave ample width for each column’s data. " +
                                             "If there are multiple lines of text in a column, " +
                                             "consider the variable row height variant.")))
                    .Section(
                        Stack()
                            .Children(
                                SamplesHelper.SampleTitle("Best Practices"),
                                Stack()
                                    .Horizontal()
                                    .Children(
                                        Stack()
                                            .Width(40.percent())
                                            .Children(
                                                SamplesHelper.SampleSubTitle("Do"),
                                                SamplesHelper.SampleDo("Use them to display content."),
                                                SamplesHelper.SampleDo("Provide useful columns of metadata."),
                                                SamplesHelper.SampleDo("Display columns in order of importance left to right or " +
                                                                       "right to left depending on the standards of the culture."),
                                                SamplesHelper.SampleDo("Give columns ample default width to display information.")),
                                        Stack()
                                            .Width(40.percent())
                                            .Children(
                                                SamplesHelper.SampleSubTitle("Don't"),
                                                SamplesHelper.SampleDont("Use them to display commands or settings."),
                                                SamplesHelper.SampleDont("Overload the view with too many columns that require " +
                                                                         "excessive horizontal scrolling."),
                                                SamplesHelper.SampleDont("Make columns so narrow that it truncates the information " +
                                                                         "in typical cases.")))))
                    .Section(
                        Stack()
                            .Width(40.percent())
                            .Children(
                                SamplesHelper.SampleTitle("Usage"),
                                TextBlock("Picker with text suggestions and tag-like selections")
                                    .Medium()
                                    .PaddingBottom(16.px()),
                                Picker<PickerSampleItem>(suggestionsTitleText: "Suggested Tags").WithItems(GetPickerItems())
                                    .PaddingBottom(32.px()),
                                TextBlock("Picker with icon and text suggestions and component based selections")
                                    .Medium()
                                    .PaddingBottom(16.px()),
                                Picker<PickerSampleItemWithComponents>(suggestionsTitleText: "Suggested Items", renderSuggestionsInline: false).WithItems(GetComponentPickerItems())));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }

        private PickerSampleItem[] GetPickerItems()
        {
            return new []
            {
                new PickerSampleItem("Bob"),
                new PickerSampleItem("BOB"),
                new PickerSampleItem("Donuts by J Dilla"),
                new PickerSampleItem("Donuts"),
                new PickerSampleItem("Coffee"),
                new PickerSampleItem("Chicken Coop"),
                new PickerSampleItem("Cherry Pie"),
                new PickerSampleItem("Chess"),
                new PickerSampleItem("Cooper")
            };
        }

        private PickerSampleItemWithComponents[] GetComponentPickerItems()
        {
            return new []
            {
                new PickerSampleItemWithComponents("Bob", LineAwesome.Bomb),
                new PickerSampleItemWithComponents("BOB", LineAwesome.Blender),
                new PickerSampleItemWithComponents("Donuts by J Dilla", LineAwesome.Carrot),
                new PickerSampleItemWithComponents("Donuts", LineAwesome.CarBattery),
                new PickerSampleItemWithComponents("Coffee", LineAwesome.Coffee),
                new PickerSampleItemWithComponents("Chicken Coop", LineAwesome.Hamburger),
                new PickerSampleItemWithComponents("Cherry Pie", LineAwesome.ChartPie),
                new PickerSampleItemWithComponents("Chess", LineAwesome.Chess),
                new PickerSampleItemWithComponents("Cooper", LineAwesome.QuestionCircle)
            };
        }
    }
}
