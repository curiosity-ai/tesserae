using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae.Components;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Tests.Samples
{
    public class DetailsListSample : IComponent
    {
        private IComponent _content;

        public DetailsListSample()
        {
            _content =
                SectionStack()
                    .Title(
                        TextBlock("DetailsList")
                            .XLarge()
                            .Bold())
                    .Section(
                        Stack()
                            .Children(
                                SampleTitle("Overview"),
                                TextBlock("DetailsList is a derivative of the List component. It is a robust way to " +
                                          "display an information rich collection of items. It can support powerful " +
                                          "ways to aid a user in finding content with sorting, grouping and " +
                                          "filtering.  Lists are a great way to handle large amounts of content, " +
                                          "but poorly designed Lists can be difficult to parse.")
                                    .PaddingBottom(Unit.Pixels, 16),
                                TextBlock("Use a DetailsList when density of information is critical. Lists can " +
                                          "support single and multiple selection, as well as drag and drop and " +
                                          "marquee selection. They are composed of a column header, which " +
                                          "contains the metadata fields which are attached to the list items, " +
                                          "and provide the ability to sort, filter and even group the list. " +
                                          "List items are composed of selection, icon, and name columns at " +
                                          "minimum. One can also include other columns such as Date Modified, or " +
                                          "any other metadata field associated with the collection. Place the most " +
                                          "important columns from left to right for ease of recall and comparison.")
                                    .PaddingBottom(Unit.Pixels, 16),
                                TextBlock("DetailsList is classically used to display files, but is also used to " +
                                          "render custom lists that can be purely metadata. Avoid using file type " +
                                          "icon overlays to denote status of a file as it can make the entire icon " +
                                          "unclear. Be sure to leave ample width for each column’s data. " +
                                          "If there are multiple lines of text in a column, " +
                                          "consider the variable row height variant.")))
                    .Section(
                        Stack()
                            .Children(
                                SampleTitle("Best Practices"),
                                Stack()
                                    .Horizontal()
                                    .Children(
                                        Stack()
                                            .Width(40, Unit.Percent)
                                            .Children(
                                                SampleSubTitle("Do"),
                                                SampleDo("Use them to display content."),
                                                SampleDo("Provide useful columns of metadata."),
                                                SampleDo("Display columns in order of importance left to right or " +
                                                         "right to left depending on the standards of the culture."),
                                                SampleDo("Give columns ample default width to display information.")),
                                        Stack()
                                            .Width(40, Unit.Percent)
                                            .Children(
                                                SampleSubTitle("Don't"),
                                                SampleDont("Use them to display commands or settings."),
                                                SampleDont("Overload the view with too many columns that require " +
                                                         "excessive horizontal scrolling."),
                                                SampleDont("Make columns so narrow that it truncates the information " +
                                                         "in typical cases.")))))
                    .Section(
                        Stack()
                            .Children(
                                SampleTitle("Usage"),
                                TextBlock("Details List With Textual Rows")
                                    .Medium()
                                    .PaddingBottom(Unit.Pixels, 16),
                                DetailsList<DetailsListItem>()
                                    .WithColumn(
                                        DetailsListLineAwesomeIconColumn(
                                            sortingKey: "FileIcon",
                                            lineAwesomeIcon: LineAwesome.File,
                                            lineAwesomeSize: LineAwesomeSize.Regular,
                                            width: 32.px()))
                                    .WithColumn(
                                        DetailsListColumn(
                                            sortingKey: "FileName",
                                            title: "File Name",
                                            width: 350.px(),
                                            isRowHeader: true))
                                    .WithColumn(
                                        DetailsListColumn(
                                            sortingKey: "DateModified",
                                            title: "Date Modified",
                                            width: 170.px()))
                                    .WithColumn(
                                        DetailsListColumn(
                                            sortingKey: "ModifiedBy",
                                            title: "Modified By",
                                            width: 150.px()))
                                    .WithColumn(
                                        DetailsListColumn(
                                            sortingKey: "FileSize",
                                            title: "File Size",
                                            width: 120.px()))
                                    .WithListItems(
                                        GetDetailsListItems())
                                    .SortedBy("FileName"),
                                DetailsList<ComponentDetailsListItem>()
                                    .WithColumn(
                                        DetailsListLineAwesomeIconColumn(
                                            sortingKey: "Icon",
                                            lineAwesomeIcon: LineAwesome.Microsoft,
                                            lineAwesomeSize: LineAwesomeSize.Regular,
                                            width: 32.px()))
                                    .WithColumn(
                                        DetailsListColumn(
                                            sortingKey: "CheckBox",
                                            title: "CheckBox",
                                            width: 350.px()))
                                    .WithColumn(
                                        DetailsListColumn(
                                            sortingKey: "Name",
                                            title: "Name",
                                            width: 350.px(),
                                            isRowHeader: true))
                                    .WithColumn(
                                        DetailsListColumn(
                                            sortingKey: "Button",
                                            title: "Button",
                                            width: 350.px()))
                                    .WithColumn(
                                        DetailsListColumn(
                                            sortingKey: "ChoiceGroup",
                                            title: "ChoiceGroup",
                                            width: 350.px()))
                                    .WithColumn(
                                        DetailsListColumn(
                                            sortingKey: "Dropdown",
                                            title: "Dropdown",
                                            width: 350.px()))
                                    .WithColumn(
                                        DetailsListColumn(
                                            sortingKey: "Toggle",
                                            title: "Toggle",
                                            width: 350.px()))
                                    .WithListItems(GetComponentDetailsListItems())
                                    .SortedBy("Name")));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }

        private DetailsListItem[] GetDetailsListItems()
        {
            return Enumerable
                .Range(1, 100)
                .SelectMany(number => new List<DetailsListItem>
                {
                    new DetailsListItem(
                        fileIcon: LineAwesome.FileWord,
                        lineAwesomeSize: LineAwesomeSize.Regular,
                        fileName: "Interesting File Name, quite long as you can see. In fact, let's make it " +
                                  "longer to see how the padding looks.",
                        dateModified: DateTime.Today.AddDays(-10),
                        modifiedBy: "Dale Cooper",
                        fileSize: 10),
                    new DetailsListItem(
                        fileIcon: LineAwesome.FileExcel,
                        lineAwesomeSize: LineAwesomeSize.Regular,
                        fileName: "File Name 2",
                        dateModified: DateTime.Today.AddDays(-20),
                        modifiedBy: "Rusty",
                        fileSize: 12),
                    new DetailsListItem(
                        fileIcon: LineAwesome.FilePowerpoint,
                        lineAwesomeSize: LineAwesomeSize.Regular,
                        fileName: "File Name 3",
                        dateModified: DateTime.Today.AddDays(-30),
                        modifiedBy: "Cole",
                        fileSize: 15)
            }).ToArray();
        }

        private ComponentDetailsListItem[] GetComponentDetailsListItems()
        {
            return Enumerable
                .Range(1, 100)
                .SelectMany(number => new List<ComponentDetailsListItem>
                {
                    new ComponentDetailsListItem()
                        .WithIcon(LineAwesome.Code)
                        .WithCheckBox(
                            CheckBox("CheckBox"))
                        .WithName("Component Details List Item")
                        .WithButton(
                            Button()
                                .SetText("Primary")
                                .Primary()
                                .OnClick(
                                    (s, e) => alert("Clicked!")))
                        .WithChoiceGroup(
                            ChoiceGroup()
                                .Horizontal()
                                .Options(
                                     Option("Option A"),
                                     Option("Option B"),
                                     Option("Option C").Disabled(),
                                     Option("Option D")))
                        .WithDropdown(
                            Dropdown()
                                .Multi()
                                .Items(
                                    DropdownItem("Header 1").Header(),
                                    DropdownItem("1-1"),
                                    DropdownItem("1-2").Selected(),
                                    DropdownItem("1-3"),
                                    DropdownItem("1-4").Disabled(),
                                    DropdownItem("1-5"),
                                    DropdownItem().Divider(),
                                    DropdownItem("Header 2").Header(),
                                    DropdownItem("2-1"),
                                    DropdownItem("2-2"),
                                    DropdownItem("2-3"),
                                    DropdownItem("2-4").Selected(),
                                    DropdownItem("2-5")))
                        .WithToggle(Toggle())
            }).ToArray();
        }
    }
}
