using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.SectionTitle")]
    public class SectionTitle : IComponent, IHasMarginPadding
    {
        private readonly HTMLElement _innerElement;
        private readonly Stack _stack;

        public SectionTitle(UIcons icon, string title, string subtitle, params IComponent[] commands)
        {
            var iconComponent = Icon(icon, size: TextSize.Large).Class("tss-sectiontitle-icon");
            var titleBlock = TextBlock(title).MediumPlus().SemiBold().Class("tss-sectiontitle-title");
            var subtitleBlock = TextBlock(subtitle).Small().Foreground(Theme.Secondary.Foreground).Class("tss-sectiontitle-subtitle");

            var textStack = Stack().Vertical().Children(titleBlock, subtitleBlock).Class("tss-sectiontitle-textstack");

            var leftSideStack = Stack().Horizontal().AlignItems(ItemAlign.Center).Children(iconComponent, textStack).Class("tss-sectiontitle-left");

            _stack = Stack().Horizontal()
                .AlignItems(ItemAlign.Center)
                .WidthStretch()
                .Class("tss-sectiontitle");

            if (commands != null && commands.Length > 0)
            {
                var commandsStack = Stack().Horizontal().AlignItems(ItemAlign.Center).Children(commands).Class("tss-sectiontitle-commands");
                _stack.Children(leftSideStack, Raw().Grow(1), commandsStack);
            }
            else
            {
                _stack.Children(leftSideStack, Raw().Grow(1));
            }

            _innerElement = _stack.Render();
        }

        public string Margin
        {
            get => _stack.Margin;
            set => _stack.Margin = value;
        }

        public string Padding
        {
            get => _stack.Padding;
            set => _stack.Padding = value;
        }

        public HTMLElement Render()
        {
            return _innerElement;
        }
    }
}
