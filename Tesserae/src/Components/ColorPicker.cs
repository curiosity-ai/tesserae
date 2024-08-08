namespace Tesserae
{
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

        public ColorPicker SetColor(Color color) => SetText(color.ToHex());
    }
}