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
            var layer     = Layer();
            var layer2    = Layer();
            var layerHost = LayerHost();

            _content = SectionStack()
               .Title(SampleHeader(nameof(LayerSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Layer is a technical component used to render content outside of its parent's DOM tree, typically at the end of the document body. This allows content to escape boundaries like 'overflow: hidden' or complex z-index stacks, ensuring that elements like tooltips, context menus, and modals always appear on top of other content.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use Layers for UI elements that must appear above all other content regardless of their position in the component hierarchy. Utilize 'LayerHost' when you need to project layered content into a specific part of the DOM instead of the default body location. Be mindful of the lifecycle of layered components to ensure they are properly removed from the DOM when no longer needed.")))
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