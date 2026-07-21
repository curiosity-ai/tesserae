namespace Tesserae
{
    /// <summary>
    /// Fluent extension methods for <see cref="Layer{T}"/> and its derived components.
    /// </summary>
    [Transpose.Name("tss.LayerExtensions")]
    public static class LayerExtensions
    {
        /// <summary>
        /// Sets the content rendered inside the layer.
        /// </summary>
        public static T Content<T>(this T layer, IComponent content) where T : Layer<T>
        {
            //Fix for a strange bug with Bridge, where layer.Content is not the overloaded property from the Modal class
            if (layer is Modal modal)
            {
                modal.Content = content;
            }
            else
            {
                layer.Content = content;
            }
            return layer;
        }

        /// <summary>
        /// Sets the layer's visibility.
        /// </summary>
        public static T Visible<T>(this T layer, bool visible) where T : Layer<T>
        {
            layer.IsVisible = visible;
            return layer;
        }

        /// <summary>
        /// Specifies the layer host this layer renders into. When unset, the layer is hosted at the document body.
        /// </summary>
        public static T Host<T>(this T layer, LayerHost host) where T : Layer<T>
        {
            layer.Host = host;
            return layer;
        }
    }
}