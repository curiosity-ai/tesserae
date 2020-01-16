using System;
using System.Collections.Generic;
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

        public static string NotEmpty(TextBox textBox) => string.IsNullOrWhiteSpace(textBox.Text) ? "must not be blank" : null;
        public static string NotNegativeInteger(TextBox textBox) => ((string.IsNullOrWhiteSpace(textBox.Text) || textBox.Text.Trim().Any(c => "0123456789".IndexOf(c) == -1) || !uint.TryParse(textBox.Text, out var numericValue))) ? "must be a positive whole number" : null;
        public static string NonZeroPositiveInteger(TextBox textBox) => ((string.IsNullOrWhiteSpace(textBox.Text) || textBox.Text.Trim().Any(c => "0123456789".IndexOf(c) == -1) || !uint.TryParse(textBox.Text, out var numericValue) || numericValue == 0)) ? "must be a positive whole number, except zero" : null;
    }

    public class Validator
    {
        private HashSet<ICanValidate> RegisteredComponents = new HashSet<ICanValidate>();
        
        public event EventHandler<bool> OnValidation;

        internal void Register(ICanValidate component)
        {
            RegisteredComponents.Add(component);
        }

        internal void RaiseOnValidation()
        {
            OnValidation?.Invoke(this, IsValid);
        }

        public bool IsValid => !RegisteredComponents.Any(c => c.IsInvalid);
    }

    public static class TextBoxValidationExtensions
    {
        public static TextBox Validation(this TextBox textBox, Func<TextBox, string> validate, Validator validator = null, Validation.Mode mode = Components.Validation.Mode.OnInput)
        {
            if (validate is null)
            {
                throw new ArgumentNullException(nameof(validate));
            }

            void handler(object sender, TextBox e)
            {
                var msg = validate(e);
                if (string.IsNullOrWhiteSpace(msg))
                {
                    e.Error = "";
                    e.IsInvalid = false;
                }
                else
                {
                    e.Error = msg;
                    e.IsInvalid = true;
                }
                validator?.RaiseOnValidation();
            }

            if (mode == Components.Validation.Mode.OnBlur)
            {
                textBox.OnChange += handler;
            }
            else
            {
                textBox.OnInput += handler;
            }

            handler(null, textBox);

            validator?.Register(textBox);
            validator?.RaiseOnValidation();

            return textBox;
        }
    }
}
