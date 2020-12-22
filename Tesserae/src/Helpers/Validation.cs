using System;
using System.Linq;
using TNT;
using static TNT.T;

namespace Tesserae
{
    public static class Validation
    {
        public static string NotEmpty(TextArea textArea) => string.IsNullOrWhiteSpace(textArea.Text) ? "must not be blank".t() : null;

        public static string NotEmpty(TextBox textBox) => string.IsNullOrWhiteSpace(textBox.Text) ? "must not be blank".t() : null;

        public static string NotNegativeInteger(TextBox textBox) => ((string.IsNullOrWhiteSpace(textBox.Text) || textBox.Text.Trim().Any(c => "0123456789".IndexOf(c) == -1) || !uint.TryParse(textBox.Text, out var numericValue))) ? "must be a positive whole number".t() : null;

        public static string NonZeroPositiveInteger(TextBox textBox) => ((string.IsNullOrWhiteSpace(textBox.Text) || textBox.Text.Trim().Any(c => "0123456789".IndexOf(c) == -1) || !uint.TryParse(textBox.Text, out var numericValue) || numericValue == 0)) ? "must be a positive whole number, except zero".t() : null;

        public static string LightColor(ColorPicker colorPicker) => colorPicker.Color.GetBrightness() >= 0.5f ? "must be a light color".t() : null;
        
        public static string DarkColor(ColorPicker colorPicker)  => colorPicker.Color.GetBrightness() <= 0.5f ? "must be a dark color".t() : null;

        public static string NotInThePast(DateTimePicker dateTimePicker) => dateTimePicker.DateTime < DateTime.Now ? "must not be in the past".t() : null;

        public static string NotInTheFuture(DateTimePicker dateTimePicker) => dateTimePicker.DateTime > DateTime.Now ? "must not be in the future".t() : null;

        public static string BetweenRange(DateTimePicker dateTimePicker, DateTime from, DateTime to)
        {
            return dateTimePicker.DateTime > from && dateTimePicker.DateTime < to ? t($"must be between {from} and {to}") : null;
        }
    }
}
