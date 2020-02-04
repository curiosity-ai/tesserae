using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class Icon : IComponent
    {
        private HTMLElement _icon;
        public Icon(string icon) => _icon = I(_(icon));

        public HTMLElement Render() => _icon;
    }
}