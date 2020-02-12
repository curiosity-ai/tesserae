using System;
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
                        new DetailsListIconColumn(
                            title: "File Type",
                            minWidth: 16.ToPixelWidth(),
                            maxWidth: 16.ToPixelWidth(),
                            icon: "far fa-file"))
                    .WithColumn(
                        new DetailsListColumn(
                            title: "File Name",
                            minWidth: 210.ToPixelWidth(),
                            maxWidth: 350.ToPixelWidth(),
                            isRowHeader: true))
                    .WithColumn(
                        new DetailsListColumn(
                                title: "Date Modified",
                                minWidth: 70.ToPixelWidth(),
                                maxWidth: 350.ToPixelWidth()))
                    .WithColumn(
                        new DetailsListColumn(
                                title: "Modified By",
                                minWidth: 70.ToPixelWidth(),
                                maxWidth: 90.ToPixelWidth()))
                    .WithColumn(
                        new DetailsListColumn(
                                title: "File Size",
                                minWidth: 70.ToPixelWidth(),
                                maxWidth: 90.ToPixelWidth()))
                    .WithListItems(
                        new DetailsListItem(
                            "",
                            "File Name 1",
                            DateTime.Today.AddDays(-10),
                            "Dale Cooper",
                            10),
                        new DetailsListItem(
                            "",
                            "File Name 2",
                            DateTime.Today.AddDays(-20),
                            "Rusty",
                            10),
                        new DetailsListItem(
                             "",
                             "File Name 3",
                            DateTime.Today.AddDays(-30),
                             "Cole",
                             15));

            _content = Stack().Children(detailsList);
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
