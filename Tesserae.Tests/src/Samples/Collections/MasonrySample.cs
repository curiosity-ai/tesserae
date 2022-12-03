using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using System.Collections.Generic;
using System.Linq;

namespace Tesserae.Tests.src.Samples.Collections
{
    [SampleDetails(Group = "Collections", Order = 0, Icon = LineAwesome.Table)]
    public class MasonrySample : IComponent, ISample
    {
        private readonly IComponent _content;

        public MasonrySample()
        {
            _content = SectionStack().S()
               .Title(SampleHeader(nameof(MasonrySample)))
               .Section(VStack().S().Children(Masonry(4).S().Children(GetCards(100).ToArray())).ScrollY(), grow: true);
        }

        private IEnumerable<IComponent> GetCards(int v)
        {
            var rng = new Random();

            for (int i = 0; i < v; i++)
            {
                yield return Card(VStack().AlignItemsCenter().JustifyContent(ItemJustify.Center).Children(TextBlock($"Card {i}").NoWrap())).H(100 + (int)(rng.NextDouble() * 6) * 50);
            }
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}