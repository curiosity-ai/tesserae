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
                        new DetailsListIconColumn(
                            title: "File Type",
                            width: 16.px(),
                            icon: "far fa-file"))
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
                        "",
                        "Interesting File Name, quite long as you can see. In fact, let's make it longer to see " +
                        "how the padding looks.",
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
                        15)
            }).ToArray();
        }
    }
}
