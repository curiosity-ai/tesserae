namespace Tesserae
{
    /// <summary>
    /// A form input for picking a color, backed by the browser's native color input.
    /// </summary>
    [H5.Name("tss.ColorPicker")]
    public class ColorPicker : Input<ColorPicker>
    {
        public ColorPicker(Color color) : base("color", color?.ToHex() ?? "#000000")
        {
        }

        public Color Color
        {
            get
            {
                return Color.FromString(Text);
            }
            set
            {
                SetColor(value);
            }
        }

        /// <summary>
        /// Sets the color of the component.
        /// </summary>
        public ColorPicker SetColor(Color color) => SetText(color.ToHex());
    }
}