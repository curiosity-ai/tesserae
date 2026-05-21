using H5;
using Tesserae;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A thin horizontal divider used to separate sections of a vertically stacked layout.
    /// </summary>
    [H5.Name("tss.HorizontalSeparator")]
    public class HorizontalSeparator : IComponent, IHasBackgroundColor
    {
        private readonly HTMLElement _container;
        private readonly HTMLElement _separator;

        public Align Alignment
        {
            get
            {
                if (_container.classList.contains("tss-left")) return Align.Left;
                if (_container.classList.contains("tss-right")) return Align.Right;
                return Align.Center;
            }
            set
            {
                _container.classList.remove("tss-left");
                _container.classList.remove("tss-right");
                if (value == Align.Left) _container.classList.add("tss-left");
                if (value == Align.Right) _container.classList.add("tss-right");
                //Center is the default, no need for class
            }
        }

        public string Text
        {
            get => _separator.textContent;
            set => _separator.textContent = value ?? "";
        }

        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public string Background { get => _separator.style.background; set => _separator.style.background = value; }

        public HorizontalSeparator(string text = string.Empty)
        {
            _separator = Div(_("tss-horizontalseparator"));
            _container = Div(_("tss-horizontalseparator-container"), _separator);
            Text       = text;
        }

        public HorizontalSeparator(IComponent component)
        {
            _separator = Div(_("tss-horizontalseparator"));
            _separator.appendChild(component.Render());
            _container = Div(_("tss-horizontalseparator-container"), _separator);
        }

        /// <summary>
        /// Sets the content of the component.
        /// </summary>
        public HorizontalSeparator SetContent(IComponent component)
        {
            ClearChildren(_separator);
            _separator.appendChild(component.Render());
            return this;
        }

        public HorizontalSeparator Primary()
        {
            _separator.classList.add("tss-primary");
            return this;
        }

        /// <summary>
        /// Sets the text of the component.
        /// </summary>
        public HorizontalSeparator SetText(string text)
        {
            Text = text;
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render()
        {
            return _container;
        }

        public enum Align
        {
            Left,
            Center,
            Right
        }

        public HorizontalSeparator Left()
        {
            Alignment = Align.Left;
            return this;
        }

        public HorizontalSeparator Center()
        {
            Alignment = Align.Center;
            return this;
        }

        public HorizontalSeparator Right()
        {
            Alignment = Align.Right;
            return this;
        }
    }
}