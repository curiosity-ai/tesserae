using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using System.Collections.Generic;
using System.Linq;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 0, Icon = UIcons.Grid)]
    public class MasonrySample : IComponent, ISample
    {
        private readonly IComponent _content;

        public MasonrySample()
        {
            _content = SectionStack().S()
               .Title(SampleHeader(nameof(MasonrySample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Masonry layout (also known as a Pinterest-style layout) is a grid where items are placed in optimal positions based on available vertical space."),
                    TextBlock("Unlike a standard grid where rows have a uniform height, a Masonry layout allows items of varying heights to be packed tightly together.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use Masonry for visually-driven content like image galleries or dashboard widgets with varying heights. Ensure that the number of columns is appropriate for the screen size. Provide a consistent gap between items to maintain a clean appearance. Avoid using Masonry for content that needs to be read in a specific sequential order, as the placement can be non-linear.")))
               .Section(VStack().S().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic Masonry Grid (4 Columns)"),
                    Masonry(4).S().Children(GetCards(50).ToArray()).ScrollY()
                ), grow: true);
        }

        private IEnumerable<IComponent> GetCards(int count)
        {
            var rng = new Random();
            for (int i = 0; i < count; i++)
            {
                var height = 80 + (int)(rng.NextDouble() * 4) * 40;
                yield return Card(VStack().AlignCenter().JustifyContent(ItemJustify.Center).Children(TextBlock($"Card {i}"))).H(height.px()).W(100.percent());
            }
        }

        public HTMLElement Render() => _content.Render();
    }
}
