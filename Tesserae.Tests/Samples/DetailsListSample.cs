using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae.Components;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Tests.Samples
{
    public class DetailsListSample : IComponent
    {
        private IComponent _content;

        public DetailsListSample()
        {
            var detailsList =
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
                    .WithListItems(GetDetailsListItems());

            _content = Stack().Children(detailsList);
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
