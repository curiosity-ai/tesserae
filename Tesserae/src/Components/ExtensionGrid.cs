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
        private const int STACK_MARGINS = 12; // getBoundingClientRect does not return the width including the margins

        private ObservableList<IComponent> _items;
        private Stack                      _stack;
        private bool                       _isSmall;

        public ExtensionGrid(List<IComponent> items, Action<Button> onExtension, string extensionCardText = "See more", bool isSmall = false)
        {
            _isSmall = isSmall;
            _stack = HStack().Wrap().WS();
            _items = new ObservableList<IComponent>(items.ToArray());
            Init(b => b.ReplaceContent(TextBlock(extensionCardText).AlignCenter().TextCenter().Secondary().SemiBold()).OnClick((btn, e) => onExtension(btn)));
        }

        public ExtensionGrid(Action<Button> onExtension, string extensionCardText = "See more", bool isSmall = false)
        {
            _isSmall = isSmall;
            _stack = HStack().Wrap().WS();
            _items = new ObservableList<IComponent>();
            Init(b => b.ReplaceContent(TextBlock(extensionCardText).AlignCenter().TextCenter().Secondary().SemiBold()).OnClick((btn, e) => onExtension(btn)));
        }

        public ExtensionGrid(List<IComponent> items, Func<Button, Button> modifyExtension = null, bool isSmall = false)
        {
            _isSmall = isSmall;
            _stack = HStack().Wrap().WS();
            _items = new ObservableList<IComponent>(items.ToArray());
            Init(modifyExtension);
        }

        public ExtensionGrid(Func<Button, Button> modifyExtension = null, bool isSmall = false)
        {
            _isSmall = isSmall;
            _stack = HStack().Wrap().WS();
            _items = new ObservableList<IComponent>();
            Init(modifyExtension);
        }

        private void Init(Func<Button, Button> modifyExtension)
        {
            double initCardWidth = 200;
            double initCardHeight = _isSmall ? 60 : 80;

            bool hasExtensionBtn = modifyExtension is object;
            _items.Observe(items =>
            {
                _stack.Clear();

                double cardWidth = -1, cardHeight = -1;

                foreach (var item in items)
                {
                    item.Class("extension-grid-stack-card" + (_isSmall ? "-small" : ""));
                    _stack.Add(item);
                    if (cardWidth < 0)
                    {
                        var rectCard = item.Render().getBoundingClientRect().As<dom.DOMRect>();
                        cardWidth = es5.Math.max(rectCard.width + STACK_MARGINS, cardWidth);
                        cardHeight = es5.Math.max(rectCard.height,               cardHeight);
                        initCardWidth = cardWidth;
                        initCardHeight = cardHeight;
                    }
                }

                if (hasExtensionBtn)
                {
                    var btn = Button().Class("extension-grid-stack-extension" + (_isSmall ? "-small" : ""));
                    btn = modifyExtension(btn);
                    _stack.Add(btn);
                }

                var r = new ResizeObserver
                {
                    OnResizeElement = (e) => Resized(cardWidth, cardHeight, hasExtensionBtn)
                };
                r.Observe(_stack.Render());
            });
            Resized(initCardWidth, initCardHeight, hasExtensionBtn);
        }

        private void Resized(double cardWidth, double cardHeight, bool hasExtensionBtn)
        {
            var rectParent = _stack.Render().getBoundingClientRect().As<dom.DOMRect>();

            var parentWidth = rectParent.width;
            var parentHeight = rectParent.height;

            var perRow = Math.Floor(parentWidth / cardWidth);
            var rows = Math.Floor(parentHeight / cardHeight);

            var toKeep = (int) (rows * perRow) - (hasExtensionBtn ? 1 : 0);

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

        public void Add(IExtensionGridItem component)
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
            if (_isSmall) throw new ArgumentException("set isSmall to true to have cards without content");
            _items.Add(Button().TextLeft()
               .ReplaceContent(
                    HStack().S()
                       .Children(image,
                            VStack().HS().W(10).Grow().Padding(4.px()).PL(8).Children(
                                TextBlock(title).SemiBold().Ellipsis().PB(8).ML(5).WS(),
                                HStack().Children(content).JustifyContent(ItemJustify.End).ML(12))
                        )).OnClick((c, e) => onClick?.Invoke()));
        }

        public void Add(IComponent image, string title, Action onClick)
        {
            if (!_isSmall) throw new ArgumentException("set isSmall to true to have cards without content");
            _items.Add(Button().TextLeft()
               .ReplaceContent(
                    HStack().S()
                       .Children(image,
                            VStack().HS().W(10).Grow().Padding(4.px()).PL(8).Children(
                                TextBlock(title).SemiBold().Ellipsis().PB(8).ML(5).WS())
                        )).OnClick((c, e) => onClick?.Invoke()));
        }

        public void Clear()
        {
            _items.Clear();
        }
    }

    public interface IExtensionGridItem
    {
        IComponent Image   { get; set; }
        IComponent Title   { get; set; }
        IComponent Content { get; set; }
        Action     OnClick { get; set; }
    }


}