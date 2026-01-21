using static Tesserae.UI;
using static H5.Core.dom;
using System.Linq;
using System;

namespace Tesserae
{
    /// <summary>
    /// A spinner component used to indicate loading states.
    /// </summary>
    [H5.Name("tss.Spinner")]
    public class Spinner : ComponentBase<Spinner, HTMLDivElement>
    {
        private readonly HTMLElement _container;
        private readonly HTMLElement _label;

        /// <summary>
        /// Initializes a new instance of the <see cref="Spinner"/> class.
        /// </summary>
        /// <param name="text">The label text.</param>
        public Spinner(string text = string.Empty)
        {
            InnerElement = Div(_("tss-spinner"));
            _label       = Label(_("tss-spinner-label", text: text));
            _container   = Div(_("tss-spinner-container tss-spinner-position-right tss-spinner-size-small"), InnerElement, _label);
            AttachClick();
        }

        /// <summary>
        /// Sets the spinner to indicate success.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Spinner Success()
        {
            InnerElement.classList.add("tss-spinner-success");
            InnerElement.classList.remove("tss-spinner-danger");
            return this;
        }

        /// <summary>
        /// Sets the spinner to indicate danger/error.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Spinner Danger()
        {
            InnerElement.classList.add("tss-spinner-danger");
            InnerElement.classList.remove("tss-spinner-success");
            return this;
        }

        /// <summary>
        /// Sets the spinner to the primary color.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Spinner Primary()
        {
            InnerElement.classList.remove("tss-spinner-success");
            InnerElement.classList.remove("tss-spinner-danger");
            return this;
        }

        /// <summary>
        /// Gets or sets the position of the label.
        /// </summary>
        public LabelPosition Position
        {
            get
            {
                var s = _container.classList.FirstOrDefault(x => x.StartsWith("tss-spinner-position-"));
                if (s != null && Enum.TryParse(s, true, out LabelPosition result)) return result;
                return LabelPosition.Right;
            }
            set
            {
                var s = _container.classList.FirstOrDefault(x => x.StartsWith("tss-spinner-position-"));
                if (s != null) _container.classList.remove(s);
                _container.classList.add($"tss-spinner-position-{value.ToString().ToLower()}");
            }
        }

        /// <summary>
        /// Gets or sets the size of the spinner.
        /// </summary>
        public CircleSize Size
        {
            get
            {
                var s = _container.classList.FirstOrDefault(x => x.StartsWith("tss-spinner-size-"));
                if (s != null && Enum.TryParse(s, true, out CircleSize result)) return result;
                return CircleSize.Small;
            }
            set
            {
                var s = _container.classList.FirstOrDefault(x => x.StartsWith("tss-spinner-size-"));
                if (s != null) _container.classList.remove(s);
                _container.classList.add($"tss-spinner-size-{value.ToString().ToLower()}");
            }
        }

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        public string Text
        {
            get => _label.innerText;
            set => _label.innerText = value;
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <returns>The rendered HTML element.</returns>
        public override HTMLElement Render()
        {
            return _container;
        }

        /// <summary>Sets the label position to left.</summary>
        public Spinner Left()
        {
            Position = LabelPosition.Left;
            return this;
        }
        /// <summary>Sets the label position to right.</summary>
        public Spinner Right()
        {
            Position = LabelPosition.Right;
            return this;
        }
        /// <summary>Sets the label position to above.</summary>
        public Spinner Above()
        {
            Position = LabelPosition.Above;
            return this;
        }
        /// <summary>Sets the label position to below.</summary>
        public Spinner Below()
        {
            Position = LabelPosition.Below;
            return this;
        }

        /// <summary>Sets the size to extra small.</summary>
        public Spinner XSmall()
        {
            Size = CircleSize.XSmall;
            return this;
        }
        /// <summary>Sets the size to small.</summary>
        public Spinner Small()
        {
            Size = CircleSize.Small;
            return this;
        }
        /// <summary>Sets the size to medium.</summary>
        public Spinner Medium()
        {
            Size = CircleSize.Medium;
            return this;
        }
        /// <summary>Sets the size to large.</summary>
        public Spinner Large()
        {
            Size = CircleSize.Large;
            return this;
        }

        /// <summary>
        /// Sets the label text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The current instance.</returns>
        public Spinner SetText(string text)
        {
            Text = text;
            return this;
        }

        /// <summary>
        /// Represents the position of a spinner label.
        /// </summary>
        public enum LabelPosition
        {
            /// <summary>Above the spinner.</summary>
            Above,
            /// <summary>Below the spinner.</summary>
            Below,
            /// <summary>To the left of the spinner.</summary>
            Left,
            /// <summary>To the right of the spinner.</summary>
            Right
        }

        /// <summary>
        /// Represents the size of a spinner.
        /// </summary>
        public enum CircleSize
        {
            /// <summary>Extra small.</summary>
            XSmall,
            /// <summary>Small.</summary>
            Small,
            /// <summary>Medium.</summary>
            Medium,
            /// <summary>Large.</summary>
            Large
        }
    }
}