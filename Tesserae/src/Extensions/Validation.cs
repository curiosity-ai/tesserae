using System;
using System.Linq;

namespace Tesserae.Components
{
    public static class Validation
    {
        public enum Mode
        {
            OnInput,
            OnBlur
        }

        public static string NotEmpty(TextArea textArea) => string.IsNullOrWhiteSpace(textArea.Text) ? "must not be blank" : null;

        public static string NotEmpty(TextBox textBox) => string.IsNullOrWhiteSpace(textBox.Text) ? "must not be blank" : null;

        public static string NotNegativeInteger(TextBox textBox) => ((string.IsNullOrWhiteSpace(textBox.Text) || textBox.Text.Trim().Any(c => "0123456789".IndexOf(c) == -1) || !uint.TryParse(textBox.Text, out var numericValue))) ? "must be a positive whole number" : null;

        public static string NonZeroPositiveInteger(TextBox textBox) => ((string.IsNullOrWhiteSpace(textBox.Text) || textBox.Text.Trim().Any(c => "0123456789".IndexOf(c) == -1) || !uint.TryParse(textBox.Text, out var numericValue) || numericValue == 0)) ? "must be a positive whole number, except zero" : null;

        public static string NonWhite(ColorPicker colorPicker) => colorPicker.Base10 == 0xffffff ? "must not be white" : null;

        public static string NonBlack(ColorPicker colorPicker) => colorPicker.Base10 == 0 ? "must not be black" : null;

        public static string NotInThePast(DateTimePicker dateTimePicker) => dateTimePicker.DateTime < DateTime.Now ? "must not be in the past" : null;

        public static string NotInTheFuture(DateTimePicker dateTimePicker) => dateTimePicker.DateTime > DateTime.Now ? "must not be in the future" : null;

        public static string BetweenRange(DateTimePicker dateTimePicker, DateTime from, DateTime to)
        {
            return dateTimePicker.DateTime > from && dateTimePicker.DateTime < to ? $"must be between {from} and {to}" : null;
        }
    }
}
