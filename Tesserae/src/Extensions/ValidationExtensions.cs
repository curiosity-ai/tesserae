using System;

namespace Tesserae.Components
{
    public static class ValidationExtensions
    {
        /// <summary>
        /// This applies validation logic to a component that implements ICanValidate-of-itself and will register the component with a Validator instance if one is provided
        /// </summary>
        public static TComponent Validation<TComponent>(this TComponent component, Func<TComponent, string> validate, Validator validator = null) where TComponent : ICanValidate<TComponent>
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));
            if (validate == null)
                throw new ArgumentNullException(nameof(validate));

            // 2020-06-30 DWR: When we attach validation logic to a component - we ONLY perform the attaching work now, we do NOT execute that logic immediately otherwise we could present the User with a form of validatable fields that all
            // scream red validation error messages at them before they've even had a chance to start filling it in and that hardly seems polite
            // - So validation should only occur when a component's content is changed (which the component.Atttach call handles) or when the entire form is submitted, at which point there should be a validator instance whose "IsValid"
            //   property is checked and that will FORCE validation of all fields. If the User has left half of the mandatory fields blank at that point, THEN they can be shouted at about it.

            // 2020-09-16 DWR: We now register TWO callbacks with the Validator - one to just check whether the component WOULD be considered valid and a second one to check that and then update the visual state accordingly. The second
            // callback used to be the only one that we had and the first one has been added so that the Validator can implement an method "AreCurrentValuesAllValid" that peeks at the current state to see if the form has been pre-populated
            // at all and would be considered valid but without displaying all the validation messages on inputs that the User hasn't touched yet for things that are NOT valid.

            component.Attach(_ => ApplyValidation());
            validator?.Register(component, WouldBeValid, () => ApplyValidation());;

            return component;

            bool WouldBeValid() => string.IsNullOrWhiteSpace(validate(component));

            void ApplyValidation()
            {
                var validationWarningIfAny = validate(component);
                var isInvalid = !string.IsNullOrWhiteSpace(validationWarningIfAny);
                component.Error = isInvalid ? validationWarningIfAny : "";
                component.IsInvalid = isInvalid;
            }
        }
    }
}