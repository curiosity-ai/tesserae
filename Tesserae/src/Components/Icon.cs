using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Icon : IComponent, IHasForegroundColor
    {
        private readonly HTMLElement _icon;

        public Icon(string icon)    => _icon = I(_(icon));

        public string Foreground { get => _icon.style.color; set => _icon.style.color = value; }

        public HTMLElement Render() => _icon;
    }
}
