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
        private Dictionary<ICanValidate, ValidationHandler> RegisteredComponents = new Dictionary<ICanValidate, ValidationHandler>();

        public event EventHandler<bool> OnValidation;

        internal delegate void ValidationHandler(object e);

        private int CallsDepth = 0;

        internal void Register(ICanValidate component, ValidationHandler handler)
        {
            RegisteredComponents.Add(component, handler);
        }

        internal void RaiseOnValidation()
        {
            OnValidation?.Invoke(this, IsValid);
            Revalidate();
        }

        public bool IsValid
        {
            get
            {
                Revalidate();
                return !RegisteredComponents.Keys.Any(c => c.IsInvalid);
            }
        }
        public void Revalidate()
        {
            if(CallsDepth > 2) { return; }
            CallsDepth++;
            foreach (var kv in RegisteredComponents)
            {
                kv.Value(this); //Force revalidation
            }
            CallsDepth--;
        }
    }

    public static class ValidationExtensions
    {
        public static T Validation<T>(this T textBox, Func<T, string> validate, Validator validator = null, Validation.Mode mode = Components.Validation.Mode.OnInput) where T : ICanValidate<T>
        {
            if (validate is null)
            {
                throw new ArgumentNullException(nameof(validate));
            }

            void handler(object sender, T e)
            {
                var msg = validate(e) ?? "";
                var isInvalid = !string.IsNullOrWhiteSpace(msg);
                bool shouldRaise = isInvalid != e.IsInvalid || e.Error != msg;
                e.Error = msg;
                e.IsInvalid = isInvalid;
                if (shouldRaise)
                {
                    validator?.RaiseOnValidation();
                }
            }

            textBox.Attach(handler, mode);

            handler(null, textBox);

            validator?.Register(textBox, (s) => handler(s,textBox));
            validator?.RaiseOnValidation();

            return textBox;
        }
    }
}
