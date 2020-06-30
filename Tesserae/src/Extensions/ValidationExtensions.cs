using System;

namespace Tesserae.Components
{
    public static class ValidationExtensions
    {
        public static T Validation<T>(this T component, Func<T, string> validate, Validator validator = null) where T : ICanValidate<T>
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));
            if (validate == null)
                throw new ArgumentNullException(nameof(validate));

            // 2020-06-30 DWR: When we attach validation logic to a component - we ONLY perform the attaching work now, we do NOT execute that logic immediately otherwise we could present the User with a form of validatable fields that all
            // scream red validation error messages at them before they've even had a chance to start filling it in and that hardly seems polite
            // - So validation should only occur when a component's content is changed (which the component.Atttach call handles) or when the entire form is submitted, at which point there should be a validator instance whose "IsValid"
            //   property is checked and that will FORCE validation of all fields. If the User has left half of the mandatory fields blank at that point, THEN they can be shouted at about it.
            component.Attach(_ => ApplyValidation());
            validator?.Register(component, () => ApplyValidation());

            return component;

            void ApplyValidation() => ApplyValidationToComponent(component, validate);
        }

        private static void ApplyValidationToComponent<T>(T component, Func<T, string> validate) where T : ICanValidate
        {
            var msg = validate(component) ?? "";
            var isInvalid = !string.IsNullOrWhiteSpace(msg);
            component.Error = msg;
            component.IsInvalid = isInvalid;
        }
    }
}