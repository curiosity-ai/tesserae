using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A heading used to label a section of a form or page, with optional subtitle and trailing actions.
    /// </summary>
    [H5.Name("tss.SectionTitle")]
    public class SectionTitle : IComponent, IHasMarginPadding
    {
        private readonly Stack _stack;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public SectionTitle(UIcons icon, string title, string subtitle, params IComponent[] commands)
        {
            var iconComponent = Icon(icon, size: TextSize.Large).Class("tss-sectiontitle-icon");
            var titleBlock    = TextBlock(title).MediumPlus().Bold().NoWrap().Class("tss-sectiontitle-title");

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

        /// <summary>
        /// Gets or sets the CSS margin of the component.
        /// </summary>
        public string Margin
        {
            get => _stack.Margin;
            set => _stack.Margin = value;
        }

        /// <summary>
        /// Gets or sets the CSS padding of the component.
        /// </summary>
        public string Padding
        {
            get => _stack.Padding;
            set => _stack.Padding = value;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _stack.Render();
    }
}
