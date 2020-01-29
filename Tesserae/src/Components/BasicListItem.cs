using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class BasicListItem : IComponent
    {
        private readonly HTMLElement _innerElement;

        public BasicListItem(string text)
        {
            _innerElement =
                Div(_(text: text, styles: s =>
                {
                    s.display   = "block";
                    s.textAlign = "center";
                }));
        }

        public HTMLElement Render() => _innerElement;
    }
}