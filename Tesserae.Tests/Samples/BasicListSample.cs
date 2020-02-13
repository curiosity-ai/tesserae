using System.Linq;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class BasicListSample : IComponent
    {
        private IComponent _content;

        public BasicListSample()
        {
            var basicList =
                BasicList(
                        Enumerable
                            .Range(1, 5000)
                            .Select(number => new BasicListItem($"Lorem Ipsum {number}")));

            _content = Stack().Children(basicList);
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}