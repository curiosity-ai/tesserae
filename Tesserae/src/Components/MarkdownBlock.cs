using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A component that renders Markdown text as sanitized HTML.
    /// Uses the bundled <c>marked</c> and <c>DOMPurify</c> libraries via <see cref="Markdown"/>.
    /// </summary>
    [H5.Name("tss.mdb")]
    public class MarkdownBlock : ComponentBase<MarkdownBlock, HTMLElement>, ICanWrap
    {
        private string _text;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownBlock"/> class.
        /// </summary>
        /// <param name="text">The Markdown source text to render.</param>
        public MarkdownBlock(string text = "")
        {
            InnerElement                  = Div(_("tss-markdown"));
            InnerElement.style.whiteSpace = "break-spaces";
            Text                          = text ?? string.Empty;
        }

        /// <summary>Gets or sets the Markdown source text. Setting this re-renders the sanitized HTML.</summary>
        public string Text
        {
            get => _text;
            set
            {
                _text                  = value ?? string.Empty;
                InnerElement.innerHTML = Tesserae.Markdown.ConvertMarkdownSanitized(_text);
            }
        }

        /// <summary>Gets the rendered sanitized HTML (derived from <see cref="Text"/>).</summary>
        public string HTML => InnerElement.innerHTML;

        /// <summary>Gets or sets whether the rendered Markdown can wrap.</summary>
        public bool CanWrap
        {
            get => !InnerElement.classList.contains("tss-text-nowrap");
            set => InnerElement.UpdateClassIfNot(value, "tss-text-nowrap");
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
