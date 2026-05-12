using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.SectionTitle")]
    public class SectionTitle : IComponent, IHasMarginPadding
    {
        private readonly Stack     _stack;
        private readonly TextBlock _titleBlock;

        public SectionTitle(UIcons icon, string title, string subtitle, params IComponent[] commands)
        {
            var iconComponent = Icon(icon, size: TextSize.Large).Class("tss-sectiontitle-icon");
            var titleBlock    = TextBlock(title).MediumPlus().Bold().Class("tss-sectiontitle-title");
            _titleBlock       = titleBlock;

            var subtitleBlock = TextBlock(subtitle).WS().SmallPlus().Foreground(Theme.Secondary.Foreground).Class("tss-sectiontitle-subtitle");

            var topStack = HStack().WS().AlignItemsCenter().Children(iconComponent, titleBlock).Class("tss-sectiontitle-top");

            _stack = VStack().WS().Class("tss-sectiontitle").Children(topStack);

            if(!string.IsNullOrWhiteSpace(subtitle))
            {
                _stack.Add(subtitleBlock);
            }

            if (commands != null && commands.Length > 0)
            {
                var commandsStack = HStack().AlignItemsCenter().Children(commands).Class("tss-sectiontitle-commands");
                topStack.Add(Raw().Grow(1));
                topStack.Add(commandsStack);
            }
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

        public HTMLElement Render() => _stack.Render();

        /// <summary>Disables wrapping on the title TextBlock.</summary>
        public SectionTitle NoWrap()
        {
            _titleBlock.NoWrap();
            return this;
        }
    }
}
