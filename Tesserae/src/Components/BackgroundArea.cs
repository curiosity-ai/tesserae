using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae
{
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
        public string Background { get => _container.style.backgroundColor; set => _container.style.backgroundColor = value; }

        public HTMLElement Render() => _container;
    }
}