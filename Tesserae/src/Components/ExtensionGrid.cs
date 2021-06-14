using System.Collections.Generic;
using System.Threading.Tasks;
using H5.Core;
using Tesserae.HTML;
using static Tesserae.UI;
using static H5.Core.dom;
using TNT;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tesserae;
using Tesserae.HTML;
using TNT;
using static Tesserae.UI;
using static H5.Core.dom;
using System;

namespace Tesserae
{
    public class ExtensionGrid : IComponent
    {
        public const int STACK_MARGINS = 12; // getBoundingClientRect does not return the width including the margins

        private ObservableList<IComponent> _items;
        private Stack                      _stack;
        public ExtensionGrid(List<IComponent> items, Action<Button> onSeeMore, string seeMoreCardText = "See more")
        {
            _stack = HStack().Wrap().WS();
            _items = new ObservableList<IComponent>(items.ToArray());
            Init(onSeeMore, seeMoreCardText);
        }

        public ExtensionGrid(Action<Button> onSeeMore, string seeMoreCardText = "See more")
        {
            _stack = HStack().Wrap().WS();
            _items = new ObservableList<IComponent>();
            Init(onSeeMore, seeMoreCardText);
        }

        private void Init(Action<Button> onSeeMore, string seeMoreCardText)
        {
            double initCardWidth = 200;
            double initCardHeight = 80;
            _items.Observe(items =>
            {
                _stack.Clear();

                double cardWidth = 0, cardHeight = 0;

                foreach (var item in items)
                {
                    item.Class("extension-grid-stack-card");
                    _stack.Add(item);
                    if (cardWidth < 1)
                    {
                        var rectCard = item.Render().getBoundingClientRect().As<dom.DOMRect>();
                        cardWidth = es5.Math.max(rectCard.width + STACK_MARGINS, cardWidth);
                        cardHeight = es5.Math.max(rectCard.height,               cardHeight);
                        if (initCardWidth == 0) initCardWidth = cardWidth;
                        if (initCardHeight == 0) initCardHeight = cardHeight;
                    }
                }

                _stack.Add(Button().Var(out var seeMoreBtn).ReplaceContent(TextBlock(seeMoreCardText).AlignCenter().TextCenter().Secondary().SemiBold())
                   .Class("extension-grid-stack-see-more")
                   .OnClick(() => onSeeMore?.Invoke(seeMoreBtn)));

                var r = new ResizeObserver
                {
                    OnResizeElement = (e) => Resized(cardWidth, cardHeight)
                };
                r.Observe(_stack.Render());
            });
            Resized(initCardWidth, initCardHeight);
        }

        private void Resized(double cardWidth, double cardHeight)
        {
            var rectParent = _stack.Render().getBoundingClientRect().As<dom.DOMRect>();

            var parentWidth = rectParent.width;
            var parentHeight = rectParent.height;

            var perRow = Math.Floor((parentWidth) / cardWidth);
            var rows = Math.Floor(parentHeight / cardHeight);

            var toKeep = (int) (rows * perRow) - 1;

            foreach (var item in _items)
            {
                if (toKeep > 0)
                {
                    item.Show();
                }
                else
                {
                    item.Collapse();
                }
                toKeep--;
            }

        }

        public dom.HTMLElement Render()
        {
            return _stack.Render();
        }

        public void Add(IExtensionGridComponent component)
        {
            _items.Add(Button().TextLeft()
               .ReplaceContent(
                    HStack().S()
                       .Children(component.Image,
                            VStack().HS().W(10).Grow().Padding(4.px()).PL(8).Children(
                                component.Title.WS(),
                                HStack().Children(component.Content).JustifyContent(ItemJustify.End).ML(12))
                        ))
               .OnClick((c, e) => component.OnClick?.Invoke()));
        }

        public void Add(IComponent image, string title, IComponent content, Action onClick)
        {
            _items.Add(Button().TextLeft()
               .ReplaceContent(
                    HStack().S()
                       .Children(image,
                            VStack().HS().W(10).Grow().Padding(4.px()).PL(8).Children(
                                TextBlock(title).SemiBold().Ellipsis().PB(8).ML(5).WS(),
                                HStack().Children(content).JustifyContent(ItemJustify.End).ML(12))
                        )).OnClick((c, e) => onClick?.Invoke()));
        }

        public void Clear()
        {
            _items.Clear();
        }
    }

    public interface IExtensionGridComponent
    {
        IComponent Image   { get; set; }
        IComponent Title   { get; set; }
        IComponent Content { get; set; }
        Action     OnClick { get; set; }
    }


}