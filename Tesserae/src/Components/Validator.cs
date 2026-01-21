using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A Validator class that coordinates validation for multiple components.
    /// </summary>
    [H5.Name("tss.Validator")]
    public sealed class Validator
    {
        private event OnValidationHandler ValidationOccured;

        /// <summary>
        /// The haveEncounteredInvalidValue value indicates whether any invalid values have been encountered SO FAR - components will only be validated as a User edits them OR when a Revalidate call is made (or the IsValid property is checked), which
        /// indicates an action such a form submission is about to occur and that EVERYTHING should be checked (unless such an action has occurred, we want to give Users a chance to fill things in BEFORE we shout at them about it)
        /// </summary>
        public delegate void OnValidationHandler(ValidationState validity);

        private readonly Dictionary<ICanValidate, (Func<bool> WouldBeValid, Action Validate)> _registeredComponents;
        private readonly HashSet<ICanValidate>                                                _registeredComponentsThatUserHasInteractedWith;
        private          DebouncerWithMaxDelay                                                _debouncer;

        private int _callsDepth = 0;

        /// <summary>
        /// Initializes a new instance of the Validator class.
        /// </summary>
        public Validator()
        {
            _registeredComponents                          = new Dictionary<ICanValidate, (Func<bool>, Action)>();
            _registeredComponentsThatUserHasInteractedWith = new HashSet<ICanValidate>();

            _debouncer = new DebouncerWithMaxDelay(() => RaiseOnValidationInternal(), delayInMs: 100, maxDelayInMs: 3000);
        }

        /// <summary>
        /// Registers a component with the validator.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="component">The component to register.</param>
        /// <param name="wouldBeValid">A function that returns whether the component would be valid without updating its visual state.</param>
        /// <param name="validate">An action that performs validation and updates the component's visual state.</param>
        public void Register<T>(ICanValidate<T> component, Func<bool> wouldBeValid, Action validate) where T : ICanValidate<T>
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            if (wouldBeValid is null)
                throw new ArgumentNullException(nameof(wouldBeValid));

            if (validate is null)
                throw new ArgumentNullException(nameof(validate));

            // Record each component that's in the form but ALSO use its Attach method to record each component that the User has interacted with - we want to only show validation messages for components that the User has edited and put into a
            // bad state OR show them for ALL components if the User has tried to submit a form (it's not nice to present them with a form littered with validation messages before they've had a chance to enter anything)
            _registeredComponents.Add(component, (wouldBeValid, validate));

            component.Attach(_ =>
            {
                _registeredComponentsThatUserHasInteractedWith.Add(component);
                RaiseOnValidation();
            });
        }

        /// <summary>
        /// Resets the validation state of all registered components.
        /// </summary>
        public void ResetState()
        {
            _registeredComponentsThatUserHasInteractedWith.Clear();

            foreach (var comp in _registeredComponents)
            {
                comp.Key.IsInvalid = false;
                comp.Key.Error     = "";
            }
        }

        /// <summary>
        /// Registers a custom validation logic not tied to a specific component.
        /// </summary>
        /// <param name="isInvalid">A function that returns whether the state is invalid.</param>
        /// <param name="onRevalidation">An action to perform on revalidation.</param>
        public void RegisterFromCallback(Func<bool> isInvalid, Action onRevalidation)
        {
            var dummy = new DummyComponentToUseForCustomValidationLogicNotTiedToOneComponent(isInvalid);
            _registeredComponents.Add(dummy, (isInvalid, onRevalidation));
        }

        /// <summary>
        /// Adds a validation event handler.
        /// </summary>
        /// <param name="onValidation">The validation event handler.</param>
        /// <returns>The current instance of the type.</returns>
        public Validator OnValidation(OnValidationHandler onValidation)
        {
            ValidationOccured += onValidation;
            return this;
        }

        private void RaiseOnValidation()
        {
            _debouncer.RaiseOnValueChanged();
        }

        private void RaiseOnValidationInternal()
        {
            // Do NOT force a full revalidation just because one thing has changed, only validate components that the User has edited so far (call Revalidate() or check IsValid to force a FULL revalidation)
            var validity = GetValidity(validateOnlyUserEditedComponents: true, updateComponentAppearances: true);
            ValidationOccured?.Invoke(validity);
        }

        /// <summary>
        /// Sets the debounce delay for validation.
        /// The milliseconds must be a value of at least one, trying to disable Debounce by passing a zero (or negative) value is not supported.
        /// </summary>
        /// <param name="delayInMs">The delay in milliseconds.</param>
        /// <returns>The current instance of the type.</returns>
        public Validator Debounce(int delayInMs)
        {
            _debouncer = new DebouncerWithMaxDelay(() => RaiseOnValidationInternal(), delayInMs: delayInMs);
            return this;
        }

        /// <summary>
        /// Sets the debounce delay and maximum delay for validation.
        /// </summary>
        /// <param name="delayInMs">The delay in milliseconds.</param>
        /// <param name="maxDelayInMs">The maximum delay in milliseconds.</param>
        /// <returns>The current instance of the type.</returns>
        public Validator Debounce(int delayInMs, int maxDelayInMs)
        {
            _debouncer = new DebouncerWithMaxDelay(() => RaiseOnValidationInternal(), delayInMs: delayInMs, maxDelayInMs: maxDelayInMs);
            return this;
        }

        /// <summary>
        /// This will check whether the form's values would currently be considered valid but without updating any the visual states relating to validity - this may be used when a form is being displayed to the User where the fields MIGHT all have been
        /// pre-populated and so the form may be valid already (but if it's not valid yet then we don't want the fields that the User hasn't edited yet to be shown as invalid until they've had a chance to interact with them).
        /// 
        /// This would be used if the submit on the form should be set to disabled initially if the form is invalid (or enabled if IS valid) and subsequently updated on each ValidationOccured event.
        /// </summary>
        public bool AreCurrentValuesAllValid()
        {
            // Check EVERY component but don't update their visual states if they're invalid
            return GetValidity(validateOnlyUserEditedComponents: false, updateComponentAppearances: false) != ValidationState.Invalid;
        }

        /// <summary>
        /// This will trigger the validation logic for EVERY registered component and return false if any of them are not in a valid state (and, by doing so, their display state will be updated accordingly)
        /// </summary>
        public bool IsValid
        {
            get
            {
                // If we want to know if the ENTIRE form that this validator is related to then we need to check ALL components and NOT just the ones that the User has edited so far - and so we won't call Revalidate and specify Revalidate as true - and
                // we ALSO want the ValidationOccured event to fire - which is another reason to call Revalidate() and not Revalidate(bool validateOnlyUserEditedComponents)
                return Revalidate();
            }
        }

        /// <summary>
        /// This will trigger the validation logic for EVERY registered component and return false if any of them are not in a valid state (and, by doing so, their display state will be updated accordingly)
        /// </summary>
        /// <summary>
        /// Triggers validation for all registered components and returns whether they are all valid.
        /// </summary>
        /// <returns>True if all components are valid, false otherwise.</returns>
        public bool Revalidate()
        {
            var validity = GetValidity(validateOnlyUserEditedComponents: false, updateComponentAppearances: true);
            ValidationOccured?.Invoke(validity);
            return validity != ValidationState.Invalid; // Since we've forced a full re-validate here, we know we can translate the enum into a bool safely because it's either ALL valid or at least one component is NOT valid (we didn't skip ANY not-yet-interacted-with components)
        }

        /// <summary>
        /// This will return false if any of the components that were checked were found to be in an invalid state (the components checked depends upon validateOnlyUserEditedComponents and which registered components that the User has interacted with)
        /// </summary>
        private ValidationState GetValidity(bool validateOnlyUserEditedComponents, bool updateComponentAppearances)
        {
            if (_callsDepth > 2)
                return ValidationState.EveryComponentIsValid;

            var atLeastOneComponentNotChecked = false;
            var looksValidSoFar               = true;
            _callsDepth++;

            foreach (var kv in _registeredComponents)
            {
                if (validateOnlyUserEditedComponents && !_registeredComponentsThatUserHasInteractedWith.Contains(kv.Key))
                {
                    atLeastOneComponentNotChecked = true;
                    continue;
                }

                // 2020-09-16 DWR: This method is called in a few different ways -
                //  1. Force a full validation of the form (this is done when the User clicks submit - ie. when Revalidate is called or the IsValid property is checked, which forces a full re-validation)
                //      > This updates the visual states of all components so that the User can easily see what is wrong (if anything)
                //  2. When a validation is required for all fields that the User has interacted with so far (this occurs when the User edits any of the fields)
                //      > This updates the visual states of all components that the User has interacted with so far, so that they User can easily see what they have entered wrong (but without shouting at them about fields that they haven't touched yet)
                //  3. When we want to determine whether the form, as initially rendered, is in a valid state (this is done after the form is rendered if the submit button should be set to a disabled state if the form is not valid)
                //      > This does NOT update the visual state of components because we don't want to shout at the User about fields that they haven't interact with yet but we do want to know if the form as it currently stands is ok to submit
                bool componentIsInvalid;

                if (updateComponentAppearances)
                {
                    kv.Value.Validate?.Invoke(); // Force revalidation
                    componentIsInvalid = kv.Key.IsInvalid;
                }
                else
                {
                    componentIsInvalid = !kv.Value.WouldBeValid();
                }

                if (componentIsInvalid)
                {
                    looksValidSoFar = false;
                }
            }
            _callsDepth--;

            // If we encountered an invalid component then it doesn't matter whether we check all of them or just a subset - the resulting state is Invalid
            if (!looksValidSoFar)
                return ValidationState.Invalid;

            // If we DIDN'T encounter an invalid component then it could be that we haven't checked the entire form yet OR it could be that we have and every single one of them is perfect
            return atLeastOneComponentNotChecked
                ? ValidationState.NoInvalidComponentFoundSoFar
                : ValidationState.EveryComponentIsValid;
        }

        private sealed class DummyComponentToUseForCustomValidationLogicNotTiedToOneComponent : ICanValidate
        {
            private readonly Func<bool>  _isInvalid;
            private readonly HTMLElement _innerElement;
            public DummyComponentToUseForCustomValidationLogicNotTiedToOneComponent(Func<bool> isInvalid)
            {
                _isInvalid    = isInvalid;
                _innerElement = Span(_(text: "This is a dummy element to illustrate validation"));
            }

            public HTMLElement Render() => _innerElement;

            public string Error     { get;                 set; }
            public bool   IsInvalid { get => _isInvalid(); set => throw new NotSupportedException(); }
        }
    }
}