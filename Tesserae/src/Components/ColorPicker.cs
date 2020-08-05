namespace Tesserae.Components
{
    public class ColorPicker : Input<ColorPicker>, IBindableComponent<string>
    {
        public ColorPicker(Color color) : base("color", color?.ToHex() ?? "#000000") { }

        public Color Color => Color.FromString(Text);

        public ColorPicker SetColor(Color color) => SetText(color.ToHex());
    }
}