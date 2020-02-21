using System;
using System.Collections.Generic;
using System.Linq;
using Retyped;
using Tesserae.Components;

namespace Tesserae.Tests.Samples
{
    public class PickerSample : IComponent
    {
        private readonly IComponent _content;

        public PickerSample()
        {
            _content =
                UI.SectionStack()
                    .Title(
                        UI.TextBlock("Picker")
                            .XLarge()
                            .Bold())
                    .Section(
                        UI.Stack()
                            .Children(
                                SamplesHelper.SampleTitle("Overview"),
                                UI.TextBlock("DetailsList is a derivative of the List component. It is a robust way to " +
                                             "display an information rich collection of items. It can support powerful " +
                                             "ways to aid a user in finding content with sorting, grouping and " +
                                             "filtering.  Lists are a great way to handle large amounts of content, " +
                                             "but poorly designed Lists can be difficult to parse.")
                                    .PaddingBottom(16.px()),
                                UI.TextBlock("Use a DetailsList when density of information is critical. Lists can " +
                                             "support single and multiple selection, as well as drag and drop and " +
                                             "marquee selection. They are composed of a column header, which " +
                                             "contains the metadata fields which are attached to the list items, " +
                                             "and provide the ability to sort, filter and even group the list. " +
                                             "List items are composed of selection, icon, and name columns at " +
                                             "minimum. One can also include other columns such as Date Modified, or " +
                                             "any other metadata field associated with the collection. Place the most " +
                                             "important columns from left to right for ease of recall and comparison.")
                                    .PaddingBottom(16.px()),
                                UI.TextBlock("DetailsList is classically used to display files, but is also used to " +
                                             "render custom lists that can be purely metadata. Avoid using file type " +
                                             "icon overlays to denote status of a file as it can make the entire icon " +
                                             "unclear. Be sure to leave ample width for each column’s data. " +
                                             "If there are multiple lines of text in a column, " +
                                             "consider the variable row height variant.")))
                    .Section(
                        UI.Stack()
                            .Children(
                                SamplesHelper.SampleTitle("Best Practices"),
                                UI.Stack()
                                    .Horizontal()
                                    .Children(
                                        UI.Stack()
                                            .Width(40.percent())
                                            .Children(
                                                SamplesHelper.SampleSubTitle("Do"),
                                                SamplesHelper.SampleDo("Use them to display content."),
                                                SamplesHelper.SampleDo("Provide useful columns of metadata."),
                                                SamplesHelper.SampleDo("Display columns in order of importance left to right or " +
                                                                       "right to left depending on the standards of the culture."),
                                                SamplesHelper.SampleDo("Give columns ample default width to display information.")),
                                        UI.Stack()
                                            .Width(40.percent())
                                            .Children(
                                                SamplesHelper.SampleSubTitle("Don't"),
                                                SamplesHelper.SampleDont("Use them to display commands or settings."),
                                                SamplesHelper.SampleDont("Overload the view with too many columns that require " +
                                                                         "excessive horizontal scrolling."),
                                                SamplesHelper.SampleDont("Make columns so narrow that it truncates the information " +
                                                                         "in typical cases.")))))
                    .Section(
                        UI.Stack()
                            .Children(
                                SamplesHelper.SampleTitle("Usage"),
                                UI.TextBlock("Picker")
                                    .Medium()
                                    .PaddingBottom(16.px()),
                                UI.Picker<PickerSampleItem>().WithItems(new PickerSampleItem("Donuts"), new PickerSampleItem("Coffee"), new PickerSampleItem("Chicken Coop"), new PickerSampleItem("Cherry Pie"), new PickerSampleItem("Chess"), new PickerSampleItem("Cooper"))));
        }

        public dom.HTMLElement Render()
        {
            return _content.Render();
        }

        private DetailsListSampleFileItem[] GetDetailsListItems()
        {
            return Enumerable
                .Range(1, 100)
                .SelectMany(number => new List<DetailsListSampleFileItem>
                {
                    new DetailsListSampleFileItem(
                        fileIcon: LineAwesome.FileWord,
                        lineAwesomeSize: LineAwesomeSize.Regular,
                        fileName: "Interesting File Name, quite long as you can see. In fact, let's make it " +
                                  "longer to see how the padding looks.",
                        dateModified: DateTime.Today.AddDays(-10),
                        modifiedBy: "Dale Cooper",
                        fileSize: 10),
                    new DetailsListSampleFileItem(
                        fileIcon: LineAwesome.FileExcel,
                        lineAwesomeSize: LineAwesomeSize.Regular,
                        fileName: "File Name 2",
                        dateModified: DateTime.Today.AddDays(-20),
                        modifiedBy: "Rusty",
                        fileSize: 12),
                    new DetailsListSampleFileItem(
                        fileIcon: LineAwesome.FilePowerpoint,
                        lineAwesomeSize: LineAwesomeSize.Regular,
                        fileName: "File Name 3",
                        dateModified: DateTime.Today.AddDays(-30),
                        modifiedBy: "Cole",
                        fileSize: 15)
                }).ToArray();
        }

        private DetailsListSampleItemWithComponents[] GetComponentDetailsListItems()
        {
            return Enumerable
                .Range(1, 100)
                .SelectMany(number => new List<DetailsListSampleItemWithComponents>
                {
                    new DetailsListSampleItemWithComponents()
                        .WithIcon(LineAwesome.Code)
                        .WithCheckBox(
                            UI.CheckBox("CheckBox"))
                        .WithName("Component Details List Item")
                        .WithButton(
                            UI.Button()
                                .SetText("Primary")
                                .Primary()
                                .OnClick(
                                    (s, e) => dom.alert("Clicked!")))
                        .WithChoiceGroup(
                            UI.ChoiceGroup()
                                .Horizontal()
                                .Options(
                                    UI.Option("Option A"),
                                    UI.Option("Option B").Disabled(),
                                    UI.Option("Option C")))
                        .WithDropdown(
                            UI.Dropdown()
                                .Multi()
                                .Items(
                                    UI.DropdownItem("Header 1").Header(),
                                    UI.DropdownItem("1-1"),
                                    UI.DropdownItem("1-2").Selected(),
                                    UI.DropdownItem("1-3"),
                                    UI.DropdownItem("1-4").Disabled(),
                                    UI.DropdownItem("1-5"),
                                    UI.DropdownItem("2-1"),
                                    UI.DropdownItem("2-2"),
                                    UI.DropdownItem("2-3"),
                                    UI.DropdownItem("2-4").Selected(),
                                    UI.DropdownItem("2-5")))
                        .WithToggle(UI.Toggle())
                }).ToArray();
        }

    }
}
