using System;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class GridPickerSample : IComponent
    {
        private readonly IComponent _content;
        public GridPickerSample()
        {
            var picker = new GridPicker(
                columnNames: new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"},
                rowNames: new[] { "Morning", "Afternoon", "Night"},
                states: 3,
                initialStates: new[] { new []{ 0, 0, 0, 0, 0, 0, 0 },
                        new []{ 0, 0, 0, 0, 0, 0, 0 },
                        new []{ 0, 0, 0, 0, 0, 0, 0 }
                },
                formatState: (btn, state, previousState) =>
                {
                    string text = "";

                    switch (state)
                    {
                        case 0: text = "☠"; break;
                        case 1: text = "🐢"; break;
                        case 2: text = "🐇"; break;
                    }

                    if (previousState >= 0 && previousState != state)
                    {
                        switch (previousState)
                        {
                            case 0: text = $"☠ -> {text}"; break;
                            case 1: text = $"🐢 -> {text}"; break;
                            case 2: text = $"🐇 -> {text}"; break;
                        }
                    }

                    btn.SetText(text);
                });

            _content = SectionStack()
               .Title(SampleHeader(nameof(GridPickerSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("This component let you select states on a grid")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    picker
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}