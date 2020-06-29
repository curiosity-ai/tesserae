namespace Tesserae.Components
{
    public class ColorPicker : Input<ColorPicker>
    {
        public ColorPicker(int? color = null)
            : base("color", FormatColor(color))
        {
        }

        public string Color => Text;

        public int Base10   => int.Parse(Text.Replace("#", string.Empty), 16);

        public ColorPicker SetColor(int color) => SetText(FormatColor(color));

        private static string FormatColor(int? color)
        {
            if (!color.HasValue)
            {
                return string.Empty;
            }

            return $"#{color.Value:X6}";
        }
    }
}
