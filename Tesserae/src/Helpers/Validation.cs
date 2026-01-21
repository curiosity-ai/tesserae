using System;
using System.Linq;
using TNT;
using static TNT.T;

namespace Tesserae
{
    /// <summary>
    /// Provides a set of standard validation rules for various components.
    /// </summary>
    [H5.Name("tss.Validation")]
    public static class Validation
    {
        /// <summary>Validates that the TextArea is not empty or whitespace.</summary>
        public static string NotEmpty(TextArea textArea) => string.IsNullOrWhiteSpace(textArea.Text) ? "must not be blank".t() : null;

        /// <summary>Validates that the TextBox is not empty or whitespace.</summary>
        public static string NotEmpty(TextBox textBox) => string.IsNullOrWhiteSpace(textBox.Text) ? "must not be blank".t() : null;

        /// <summary>Validates that the TextBox contains a non-negative integer.</summary>
        public static string NotNegativeInteger(TextBox textBox) => ((string.IsNullOrWhiteSpace(textBox.Text) || textBox.Text.Trim().Any(c => "0123456789".IndexOf(c) == -1) || !uint.TryParse(textBox.Text, out var numericValue))) ? "must be a positive whole number".t() : null;

        /// <summary>Validates that the TextBox contains a positive integer greater than zero.</summary>
        public static string NonZeroPositiveInteger(TextBox textBox) => ((string.IsNullOrWhiteSpace(textBox.Text) || textBox.Text.Trim().Any(c => "0123456789".IndexOf(c) == -1) || !uint.TryParse(textBox.Text, out var numericValue) || numericValue == 0)) ? "must be a positive whole number, except zero".t() : null;

        /// <summary>Validates that the ColorPicker has a light color selected.</summary>
        public static string LightColor(ColorPicker colorPicker) => colorPicker.Color.GetBrightness() < 0.5f ? "must be a light color".t() : null;

        /// <summary>Validates that the ColorPicker has a dark color selected.</summary>
        public static string DarkColor(ColorPicker colorPicker) => colorPicker.Color.GetBrightness() > 0.5f ? "must be a dark color".t() : null;

        /// <summary>Validates that the selected date and time is not in the past.</summary>
        public static string NotInThePast(DateTimePicker dateTimePicker) => dateTimePicker.DateTime < DateTime.Now ? "must not be in the past".t() : null;

        /// <summary>Validates that the selected date and time is not in the future.</summary>
        public static string NotInTheFuture(DateTimePicker dateTimePicker) => dateTimePicker.DateTime > DateTime.Now ? "must not be in the future".t() : null;

        /// <summary>Validates that the selected date and time is within the specified range.</summary>
        public static string BetweenRange(DateTimePicker dateTimePicker, DateTime from, DateTime to)
        {
            return dateTimePicker.DateTime > from && dateTimePicker.DateTime < to ? t($"must be between {from} and {to}") : null;
        }

        /// <summary>Validates that the selected date is not in the past.</summary>
        public static string NotInThePast(DatePicker datePicker) => datePicker.Date < DateTime.Now ? "must not be in the past".t() : null;

        /// <summary>Validates that the selected date is not in the future.</summary>
        public static string NotInTheFuture(DatePicker datePicker) => datePicker.Date > DateTime.Now ? "must not be in the future".t() : null;

        /// <summary>Validates that the selected date is within the specified range.</summary>
        public static string BetweenRange(DatePicker datePicker, DateTime from, DateTime to)
        {
            return datePicker.Date > from && datePicker.Date < to ? t($"must be between {from} and {to}") : null;
        }
    }
}