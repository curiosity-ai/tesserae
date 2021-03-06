﻿namespace Tesserae
{
    public class ColorPicker : Input<ColorPicker>
    {
        public ColorPicker(Color color) : base("color", color?.ToHex() ?? "#000000")
        {
        }

        public Color Color => Color.FromString(Text);

        public ColorPicker SetColor(Color color) => SetText(color.ToHex());
    }
}
