using Tesserae;
using Tesserae.Tests;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 0, Icon = UIcons.TablePivot)]
    public class LayerSample : IComponent, ISample
    {
        private readonly IComponent _content;
        public LayerSample()
        {
            var layer = Layer();
            var layer2 = Layer();
            var layerHost = LayerHost();

            _content = SectionStack()
               .Title(SampleHeader(nameof(LayerSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("A Layer is a technical component that does not have specific Design guidance."),
                    TextBlock("Layers are used to render content outside of a DOM tree, at the end of the document. This allows content to escape traditional boundaries caused by \"overflow: hidden\" css rules and keeps it on the top without using z-index rules. This is useful for example in ContextualMenu and Tooltip scenarios, where the content should always overlay everything else.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    TextBlock("Basic layered content").Medium(),
                    layer.Content(HStack().Children(TextBlock("This is example layer content."),
                        Button("Show second Layer").SetIcon(UIcons.Add).Primary().OnClick((s, e) => layer2.IsVisible = true),
                        layer2.Content(HStack().Children(TextBlock("This is the second example layer content."),
                            Button("Hide second Layer").SetIcon(UIcons.CrossCircle).Primary().OnClick((s, e) => layer2.IsVisible = false)
                        )),
                        Toggle("Toggle Component Layer").OnChange((s, e) => layer.IsVisible = s.IsChecked), Toggle())),
                    Toggle("Toggle Component Layer").OnChange((s, e) => layer.IsVisible = s.IsChecked),
                    TextBlock("Using LayerHost to control projection").Medium(),
                    Toggle("Show on Host").OnChange((s, e) => layer.Host = s.IsChecked ? layerHost : null),
                    layerHost));
        }

        public HTMLElement Render() => _content.Render();
    }
}