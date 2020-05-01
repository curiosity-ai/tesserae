namespace Tesserae.Components
{
    public static class LayerExtensions
    {
        public static T Content<T>(this T layer, IComponent content) where T : Layer
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

        public static T Visible<T>(this T layer) where T : Layer
        {
            layer.IsVisible = true;
            return layer;
        }

        public static T Host<T>(this T layer, LayerHost host) where T : Layer
        {
            layer.Host = host;
            return layer;
        }
    }
}