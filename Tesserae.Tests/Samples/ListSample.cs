using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class ListSample : IComponent
    {
        private IComponent _content;

        public ListSample()
        {
            _content = Stack().Children(
                List());
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}