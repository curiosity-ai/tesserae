using System;

namespace Tesserae.Components
{
    public static class ValidationExtensions
    {
        public static T Validation<T>(this T component, Func<T, string> validate, Validator validator = null, Validation.Mode mode = Components.Validation.Mode.OnInput) where T : ICanValidate<T>
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

            component.Attach(handler, mode);

            handler(null, component);

            validator?.Register(component, (s) => handler(s,component));
            validator?.RaiseOnValidation();

            return component;
        }
    }
}