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
                                TextBlock("List provides a base component for rendering large sets of items. " +
                                          "It is agnostic of the tile component used, and selection " +
                                          "management. These concerns can be layered separately.")))
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
                                                SampleDo("Use them to display commands or settings."),
                                                SampleDo("Overload the view with too many columns that require " +
                                                         "excessive horizontal scrolling."),
                                                SampleDo("Make columns so narrow that it truncates the information " +
                                                         "in typical cases.")))))
                    .Section(
                        Stack()
                            .Children(
                                SampleTitle("Usage"),
                                TextBlock("Details List")
                                    .Medium()
                                    .PaddingBottom(Unit.Pixels, 16),
                                DetailList<DetailsListItem>()
                                    .WithColumn(
                                        new DetailsListLineAwesomeIconColumn(
                                            lineAwesomeIcon: LineAwesome.File,
                                            lineAwesomeSize: LineAwesomeSize.Regular,
                                            title: "File Type",
                                            width: 32.px()))
                                    .WithColumn(
                                        new DetailsListColumn(
                                            title: "File Name",
                                            width: 350.px(),
                                            isRowHeader: true))
                                    .WithColumn(
                                        new DetailsListColumn(
                                                title: "Date Modified",
                                                width: 150.px()))
                                    .WithColumn(
                                        new DetailsListColumn(
                                                title: "Modified By",
                                                width: 150.px()))
                                    .WithColumn(
                                        new DetailsListColumn(
                                                title: "File Size",
                                                width: 100.px()))
                                    .WithListItems(GetDetailsListItems())));
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
                        lineAwesomeIcon: LineAwesome.FileWord,
                        lineAwesomeSize: LineAwesomeSize.Regular,
                        fileName: "Interesting File Name, quite long as you can see. In fact, let's make it " +
                                  "longer to see how the padding looks.",
                        dateModified: DateTime.Today.AddDays(-10),
                        modifiedBy: "Dale Cooper",
                        fileSize: 10),
                    new DetailsListItem(
                        lineAwesomeIcon: LineAwesome.FileExcel,
                        lineAwesomeSize: LineAwesomeSize.Regular,
                        fileName: "File Name 2",
                        dateModified: DateTime.Today.AddDays(-20),
                        modifiedBy: "Rusty",
                        fileSize: 10),
                    new DetailsListItem(
                        lineAwesomeIcon: LineAwesome.FilePowerpoint,
                        lineAwesomeSize: LineAwesomeSize.Regular,
                        fileName: "File Name 3",
                        dateModified: DateTime.Today.AddDays(-30),
                        modifiedBy: "Cole",
                        fileSize: 15)
            }).ToArray();
        }
    }
}
