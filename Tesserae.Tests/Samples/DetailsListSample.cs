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
                        new DetailsListColumn(
                            name: "File Type",
                            minWidth: 16.ToPixelWidth(),
                            maxWidth: 16.ToPixelWidth(),
                            isIconOnly: true,
                            onItemRender: detailsListItem =>
                                new Image(
                                    _(
                                            src: $"assets/img/{detailsListItem.IconName}",
                                            className: detailsListItem.ClassName)
                                    .WithAlt("File Icon"))))
                    .WithColumn(
                        new DetailsListColumn(
                            name: "File Name",
                            minWidth: 210.ToPixelWidth(),
                            maxWidth: 350.ToPercentageWidth()));

            _content = Stack().Children(detailsList);
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
