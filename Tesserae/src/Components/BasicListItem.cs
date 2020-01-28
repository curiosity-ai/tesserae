using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class BasicListItem : IComponent
    {
        private readonly HTMLElement _innerElement;

        public BasicListItem()
        {
            _innerElement =
                Div(_(text: "Lorem Ipsum", styles: s =>
                {
                    s.display   = "block";
                    s.textAlign = "center";
                }));
        }

        public HTMLElement Render() => _innerElement;
    }
}