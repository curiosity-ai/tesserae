using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class LayerSample : IComponent
    {
        private IComponent content;

        public LayerSample()
        {
            var layer = Layer();
            var layerHost = LayerHost();
            content = Stack().Children(
                TextBlock("Layer").XLarge(),
                TextBlock("Overview").MediumPlus(),
                TextBlock("A Layer is a technical component that does not have specific Design guidance."),
                TextBlock("Layers are used to render content outside of a DOM tree, at the end of the document. This allows content to escape traditional boundaries caused by \"overflow: hidden\" css rules and keeps it on the top without using z-index rules. This is useful for example in ContextualMenu and Tooltip scenarios, where the content should always overlay everything else."),
                TextBlock("Usage").MediumPlus(),
                TextBlock("Basic layered content").Medium(),
                layer.Content(Stack().Horizontal().Children(TextBlock("This is example layer content."),Toggle(), Toggle(), Toggle())),
                Toggle("Toggle Component Layer").OnChanged((s, e) => layer.IsVisible = s.IsChecked),
                TextBlock("Using LayerHost to control projection").Medium(),
                Toggle("Show on Host").OnChanged((s, e) => layer.Host = s.IsChecked ? layerHost : null),
                layerHost
            );
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
