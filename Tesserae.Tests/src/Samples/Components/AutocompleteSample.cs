using System.Collections.Generic;
using System.Linq;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 20, Icon = UIcons.Search)]
    public class AutocompleteSample : IComponent, ISample
    {
        private readonly IComponent _content;
        private readonly string[] _data = { "Apple", "Banana", "Cherry", "Date", "Elderberry", "Fig", "Grape" };

        public AutocompleteSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(AutocompleteSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Autocomplete provides suggestions as the user types.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    Autocomplete()
                        .Suggestions(text => _data.Where(d => d.ToLower().Contains(text.ToLower())))
                        .Do(a => a.OnItemSelected += (item) => Toast().Information($"Selected: {item}"))
                ));
        }

        public H5.Core.dom.HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
