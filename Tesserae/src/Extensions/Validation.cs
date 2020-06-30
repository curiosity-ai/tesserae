using System.Linq;

namespace Tesserae.Components
{
    public static class Validation
    {
        public static string NotEmpty(TextArea textArea) => string.IsNullOrWhiteSpace(textArea.Text) ? "must not be blank" : null;
        public static string NotEmpty(TextBox textBox) => string.IsNullOrWhiteSpace(textBox.Text) ? "must not be blank" : null;
        public static string NotNegativeInteger(TextBox textBox) => ((string.IsNullOrWhiteSpace(textBox.Text) || textBox.Text.Trim().Any(c => "0123456789".IndexOf(c) == -1) || !uint.TryParse(textBox.Text, out var numericValue))) ? "must be a positive whole number" : null;
        public static string NonZeroPositiveInteger(TextBox textBox) => ((string.IsNullOrWhiteSpace(textBox.Text) || textBox.Text.Trim().Any(c => "0123456789".IndexOf(c) == -1) || !uint.TryParse(textBox.Text, out var numericValue) || numericValue == 0)) ? "must be a positive whole number, except zero" : null;

        public static string LightColor(ColorPicker colorPicker) => colorPicker.Color.GetBrightness() >= 0.5f ? "must be a light color" : null;
        public static string DarkColor(ColorPicker colorPicker)  => colorPicker.Color.GetBrightness() <= 0.5f ? "must be a dark color" : null;
    }
}
