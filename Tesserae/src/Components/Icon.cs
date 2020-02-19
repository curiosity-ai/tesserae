using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Icon : IComponent
    {
        private readonly HTMLElement _icon;

        public Icon(string icon)    => _icon = I(_(icon));

        public HTMLElement Render() => _icon;
    }
}
