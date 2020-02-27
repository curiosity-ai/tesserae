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
                                SampleTitle("Best Practices"),
                                Stack()
                                    .Horizontal()
                                    .Children(
                                        Stack()
                                            .Width(40.percent())
                                            .Children(
                                                SampleSubTitle("Do"),
                                                SampleDo("Use them to display content."),
                                                SampleDo("Provide useful columns of metadata."),
                                                SampleDo("Display columns in order of importance left to right or " +
                                                         "right to left depending on the standards of the culture."),
                                                SampleDo("Give columns ample default width to display information.")),
                                        Stack()
                                            .Width(40.percent())
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
                                    .PaddingBottom(16.px()),
                                DetailsList<DetailsListSampleFileItem>()
                                    .Height(500.px())
                                    .WithColumn(IconColumn(Icon(LineAwesome.File), width: 32.px(),  enableColumnSorting: true, sortingKey: "FileIcon"))
                                    .WithColumn(DetailsListColumn(title: "File Name",         width: 350.px(), enableColumnSorting: true, sortingKey: "FileName", isRowHeader: true))
                                    .WithColumn(DetailsListColumn(title: "Date Modified",     width: 170.px(), enableColumnSorting: true, sortingKey: "DateModified"))
                                    .WithColumn(DetailsListColumn(title: "Modified By",       width: 150.px(), enableColumnSorting: true, sortingKey: "ModifiedBy"))
                                    .WithColumn(DetailsListColumn(title: "File Size",         width: 120.px(), enableColumnSorting: true, sortingKey: "FileSize"))
                                    .WithListItems(GetDetailsListItems())
                                    .SortedBy("FileName")
                            .PaddingBottom(32.px()),
                                TextBlock("Details List With Component Rows")
                                    .Medium()
                                    .PaddingBottom(16.px()),
                                DetailsList<DetailsListSampleItemWithComponents>(small:true)
                                    .Height(500.px())
                                    .WithColumn(IconColumn(Icon(LineAwesome.Apple), width: 32.px(), enableColumnSorting: true, sortingKey: "Icon"))
                                    .WithColumn(DetailsListColumn(title: "CheckBox",   width: 120.px()))
                                    .WithColumn(DetailsListColumn(title: "Name",       width: 250.px(), isRowHeader: true))
                                    .WithColumn(DetailsListColumn(title: "Button",     width: 150.px()))
                                    .WithColumn(DetailsListColumn(title: "ChoiceGroup",width: 400.px()))
                                    .WithColumn(DetailsListColumn(title: "Dropdown",   width: 250.px()))
                                    .WithColumn(DetailsListColumn(title: "Toggle",     width: 100.px()))
                                    .WithListItems(GetComponentDetailsListItems())
                                    .SortedBy("Name")));
        }

        public HTMLElement Render()
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
                        lineAwesomeSize: LineAwesomeWeight.Regular,
                        fileName: "Interesting File Name, quite long as you can see. In fact, let's make it " +
                                  "longer to see how the padding looks.",
                        dateModified: DateTime.Today.AddDays(-10),
                        modifiedBy: "Dale Cooper",
                        fileSize: 10),
                    new DetailsListSampleFileItem(
                        fileIcon: LineAwesome.FileExcel,
                        lineAwesomeSize: LineAwesomeWeight.Regular,
                        fileName: "File Name 2",
                        dateModified: DateTime.Today.AddDays(-20),
                        modifiedBy: "Rusty",
                        fileSize: 12),
                    new DetailsListSampleFileItem(
                        fileIcon: LineAwesome.FilePowerpoint,
                        lineAwesomeSize: LineAwesomeWeight.Regular,
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
                                     Option("Option B").Disabled(),
                                     Option("Option C")))
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
