using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A full-bleed background container used to host a centered card or hero section, typically as the application
    /// shell.
    /// </summary>
    [H5.Name("tss.BackgroundArea")]
    public class BackgroundArea : IComponent, IHasBackgroundColor
    {
        private readonly Raw         _raw;
        private readonly HTMLElement _container;

        public BackgroundArea(IComponent content)
        {
            _raw       = Raw(content.Render());
            _container = Div(_("tss-background-area"), _raw.Render());
        }

        public BackgroundArea Content(IComponent content)
        {
            _raw.Content(content);
            return this;
        }
        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public string Background { get => _container.style.backgroundColor; set => _container.style.backgroundColor = value; }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _container;
    }
}