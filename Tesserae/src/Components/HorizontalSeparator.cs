using Transpose;
using Tesserae;
using static Tesserae.UI;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A thin horizontal divider used to separate sections of a vertically stacked layout.
    /// </summary>
    [Transpose.Name("tss.HorizontalSeparator")]
    public class HorizontalSeparator : IComponent, IHasBackgroundColor
    {
        private readonly HTMLElement _container;
        private readonly HTMLElement _separator;

        /// <summary>
        /// Gets or sets the alignment.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the text shown in the component.
        /// </summary>
        public string Text
        {
            get => _separator.textContent;
            set => _separator.textContent = value ?? "";
        }

        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public string Background { get => _separator.style.background; set => _separator.style.background = value; }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public HorizontalSeparator(string text = string.Empty)
        {
            _separator = Div(Att("tss-horizontalseparator"));
            _container = Div(Att("tss-horizontalseparator-container"), _separator);
            Text       = text;
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public HorizontalSeparator(IComponent component)
        {
            _separator = Div(Att("tss-horizontalseparator"));
            _separator.appendChild(component.Render());
            _container = Div(Att("tss-horizontalseparator-container"), _separator);
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

        /// <summary>
        /// Styles the component using the primary tone.
        /// </summary>
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

        /// <summary>
        /// Aligns the content of the component to the left.
        /// </summary>
        public HorizontalSeparator Left()
        {
            Alignment = Align.Left;
            return this;
        }

        /// <summary>
        /// Centers the content of the component.
        /// </summary>
        public HorizontalSeparator Center()
        {
            Alignment = Align.Center;
            return this;
        }

        /// <summary>
        /// Aligns the content of the component to the right.
        /// </summary>
        public HorizontalSeparator Right()
        {
            Alignment = Align.Right;
            return this;
        }
    }
}