using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A thin vertical divider used to separate the items of a horizontally laid out row: the counterpart of
    /// <see cref="HorizontalSeparator"/>, which divides a column. It stretches to the height of the row it is in and can carry a short
    /// label or any component in the middle (the "or" between two halves of a form).
    /// </summary>
    [Transpose.Name("tss.VerticalSeparator")]
    public class VerticalSeparator : IComponent, IHasBackgroundColor
    {
        private readonly HTMLElement _container;
        private readonly HTMLElement _separator;

        /// <summary>The label's position along the line.</summary>
        public enum Align
        {
            /// <summary>The label sits at the top, with the line below it.</summary>
            Top,
            /// <summary>The label sits in the middle, with a line above and below.</summary>
            Center,
            /// <summary>The label sits at the bottom, with the line above it.</summary>
            Bottom
        }

        /// <summary>Gets or sets where the label sits on the line.</summary>
        public Align Alignment
        {
            get
            {
                if (_container.classList.contains("tss-top")) return Align.Top;
                if (_container.classList.contains("tss-bottom")) return Align.Bottom;
                return Align.Center;
            }
            set
            {
                _container.classList.remove("tss-top");
                _container.classList.remove("tss-bottom");
                if (value == Align.Top) _container.classList.add("tss-top");
                if (value == Align.Bottom) _container.classList.add("tss-bottom");
            }
        }

        /// <summary>Gets or sets the label.</summary>
        public string Text
        {
            get => _separator.textContent;
            set => _separator.textContent = value ?? "";
        }

        /// <summary>Gets or sets the CSS background of the label.</summary>
        public string Background { get => _separator.style.background; set => _separator.style.background = value; }

        /// <summary>Initializes a new vertical separator, optionally with a label.</summary>
        public VerticalSeparator(string text = string.Empty)
        {
            _separator = Div(Att("tss-verticalseparator"));
            _container = Div(Att("tss-verticalseparator-container"), _separator);
            Text       = text;
        }

        /// <summary>Initializes a new vertical separator carrying a component.</summary>
        public VerticalSeparator(IComponent component)
        {
            _separator = Div(Att("tss-verticalseparator"));
            _separator.appendChild(component.Render());
            _container = Div(Att("tss-verticalseparator-container"), _separator);
        }

        /// <summary>Replaces the content in the middle of the line.</summary>
        public VerticalSeparator SetContent(IComponent component)
        {
            ClearChildren(_separator);
            _separator.appendChild(component.Render());
            return this;
        }

        /// <summary>Sets the label.</summary>
        public VerticalSeparator SetText(string text)
        {
            Text = text;
            return this;
        }

        /// <summary>Colours the label with the primary tone.</summary>
        public VerticalSeparator Primary()
        {
            _separator.classList.add("tss-primary");
            return this;
        }

        /// <summary>Puts the label at the top of the line.</summary>
        public VerticalSeparator Top()
        {
            Alignment = Align.Top;
            return this;
        }

        /// <summary>Puts the label in the middle of the line.</summary>
        public VerticalSeparator Center()
        {
            Alignment = Align.Center;
            return this;
        }

        /// <summary>Puts the label at the bottom of the line.</summary>
        public VerticalSeparator Bottom()
        {
            Alignment = Align.Bottom;
            return this;
        }

        /// <summary>Renders the component's root HTML element.</summary>
        public HTMLElement Render() => _container;
    }
}
