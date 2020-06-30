using System;

namespace Tesserae.Components
{
    public static class ValidationExtensions
    {
        public static T Validation<T>(this T component, Func<T, string> validate, Validator validator = null, Validation.Mode mode = Components.Validation.Mode.OnInput) where T : ICanValidate<T>
        {
            if (validate is null)
                throw new ArgumentNullException(nameof(validate));

            component.Attach(handler, mode);

            handler(component);

            validator?.Register(component, () => handler(component));
            validator?.RaiseOnValidation();

            return component;

            void handler(T sender)
            {
                var s = sender;
                var msg = validate(s) ?? "";
                var isInvalid = !string.IsNullOrWhiteSpace(msg);
                s.Error = msg;
                s.IsInvalid = isInvalid;
                validator?.RaiseOnValidation();
            }
        }
    }
}