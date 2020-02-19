using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public sealed class BasicListItem : IComponent
    {
        private readonly HTMLElement _innerElement;

        public BasicListItem(string text)
        {
            _innerElement =
                Div(_(text: text, styles: s =>
                {
                    s.display   = "block";
                    s.textAlign = "center";
                    s.height    = "63px";
                }));
        }

        public HTMLElement Render() => _innerElement;
    }
}
