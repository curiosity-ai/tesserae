using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Raw")]
    public class Raw : IComponent, IHasMarginPadding, IHasBackgroundColor
    {
        private HTMLElement InnerElement;
        private byte _flag;
        private byte _flag2;
        public Raw(HTMLElement content = null) => InnerElement = content ?? DIV();
        public Raw(IComponent component) : this(component?.Render()) { }

        public Raw Content(IComponent component) => Content(component.Render());

        public Raw Content(HTMLElement element)
        {
            if (_flag > 0 || _flag2 > 0)
            {
                CopyPropertiesTo(element);
            }

            if ((InnerElement is object) && (InnerElement.parentElement is object))
            {
                InnerElement.parentElement.replaceChild(element, InnerElement);
            }

            InnerElement = element;
            return this;
        }

        private void CopyPropertiesTo(HTMLElement element)
        {
            if ((_flag & 0b00000001) == 0b00000001) element.style.background = InnerElement.style.background;
            if ((_flag & 0b00000010) == 0b00000010) element.style.margin = InnerElement.style.margin;
            if ((_flag & 0b00000100) == 0b00000100) element.style.padding = InnerElement.style.padding;
            if ((_flag & 0b00001000) == 0b00001000) element.style.width = InnerElement.style.width;
            if ((_flag & 0b00010000) == 0b00010000) element.style.height = InnerElement.style.height;
            if ((_flag & 0b00100000) == 0b00100000) element.style.maxWidth = InnerElement.style.maxWidth;
            if ((_flag & 0b01000000) == 0b01000000) element.style.maxHeight = InnerElement.style.maxHeight;
            if ((_flag & 0b10000000) == 0b10000000) element.style.flexGrow = InnerElement.style.flexGrow;

            if ((_flag2 & 0b00000001) == 0b00000001) element.style.minHeight = InnerElement.style.minHeight;
            if ((_flag2 & 0b00000010) == 0b00000010) element.style.minWidth = InnerElement.style.minWidth;

        }

        public string Background
        {
            get => InnerElement.style.background;
            set
            {
                _flag |= 0b00000001;
                InnerElement.style.background = value;
            }
        }
        public string Margin
        {
            get => InnerElement.style.margin;
            set
            {
                _flag |= 0b00000010;
                InnerElement.style.margin = value;
            }
        }
        public string Padding
        {
            get => InnerElement.style.padding;
            set
            {
                _flag |= 0b00000100;
                InnerElement.style.padding = value;
            }
        }
        public string Width
        {
            get => InnerElement.style.width;
            set
            {
                _flag |= 0b00001000;
                InnerElement.style.width = value;
            }
        }
        public string Height
        {
            get => InnerElement.style.height;
            set
            {
                _flag |= 0b00010000;
                InnerElement.style.height = value;
            }
        }
        public string MaxWidth
        {
            get => InnerElement.style.maxWidth;
            set
            {
                _flag |= 0b00100000;
                InnerElement.style.maxWidth = value;
            }
        }

        public string MinWidth
        {
            get => InnerElement.style.minWidth;
            set
            {
                _flag2 |= 0b00000010;
                InnerElement.style.minWidth = value;
            }
        }

        public string MaxHeight
        {
            get => InnerElement.style.maxHeight;
            set
            {
                _flag |= 0b01000000;
                InnerElement.style.maxHeight = value;
            }
        }

        public string MinHeight
        {
            get => InnerElement.style.minHeight;
            set
            {
                _flag2 |= 0b00000001;
                InnerElement.style.minHeight = value;
            }
        }
        public string FlexGrow
        {
            get => InnerElement.style.flexGrow;
            set
            {
                _flag |= 0b10000000;
                InnerElement.style.flexGrow = value;
            }
        }
        public string FlexShrink
        {
            get => InnerElement.style.flexShrink;
            set
            {
                _flag |= 0b10000000;
                InnerElement.style.flexShrink = value;
            }
        }

        public HTMLElement Render() => InnerElement;
    }
}