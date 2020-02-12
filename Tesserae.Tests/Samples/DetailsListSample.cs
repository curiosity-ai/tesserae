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
                            name: "File Type",
                            minWidth: 16.ToPixelWidth(),
                            maxWidth: 16.ToPixelWidth(),
                            icon: "far fa-file"))
                    .WithColumn(
                        new DetailsListColumn(
                            name: "File Name",
                            minWidth: 210.ToPixelWidth(),
                            maxWidth: 350.ToPixelWidth()))
                    .WithColumn(
                        new DetailsListColumn(
                                name: "Date Modified",
                                minWidth: 70.ToPixelWidth(),
                                maxWidth: 350.ToPixelWidth()))
                    .WithColumn(
                        new DetailsListColumn(
                                name: "Modified By",
                                minWidth: 70.ToPixelWidth(),
                                maxWidth: 90.ToPixelWidth()))
                    .WithColumn(
                        new DetailsListColumn(
                                name: "File Size",
                                minWidth: 70.ToPixelWidth(),
                                maxWidth: 90.ToPixelWidth()))
                    .WithListItems(null);

            _content = Stack().Children(detailsList);
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
