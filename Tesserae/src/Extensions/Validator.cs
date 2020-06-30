using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public sealed class Validator
    {
        private event OnValidationHandler OnValidationOccured;

        /// <summary>
        /// The haveEncounteredInvalidValue value indicates whether any invalid values have been encountered SO FAR - components will only be validated as a User edits them OR when a Revalidate call is made (or the IsValid property is checked), which
        /// indicates an action such a form submission is about to occur and that EVERYTHING should be checked (unless such an action has occurred, we want to give Users a chance to fill things in BEFORE we shout at them about it)
        /// </summary>
        public delegate void OnValidationHandler(bool haveEncounteredInvalidValue);

        private readonly Dictionary<ICanValidate, Action> _registeredComponents;
        private readonly HashSet<ICanValidate> _registeredComponentsThatUserHasInteractedWith;
        private double _timeout = 0;
        private int _callsDepth = 0;
        public Validator()
        {
            _registeredComponents = new Dictionary<ICanValidate, Action>();
            _registeredComponentsThatUserHasInteractedWith = new HashSet<ICanValidate>();
        }

        public void Register<T>(ICanValidate<T> component, Action onRevalidation) where T : ICanValidate<T>
        {
            // Record each component that's in the form but ALSO use its Attach method to record each component that the User has interacted with - we want to only show validation messages for components that the User has edited and put into a
            // bad state OR show them for ALL components if the User has tried to submit a form (it's not nice to present them with a form littered with validation messages before they've had a chance to enter anything)
            _registeredComponents.Add(component, onRevalidation);
            component.Attach(_ =>
            {
                _registeredComponentsThatUserHasInteractedWith.Add(component);
                RaiseOnValidation();
            });
        }

        public void RegisterFromCallback(Func<bool> isInvalid, Action onRevalidation) => _registeredComponents.Add(new DummyComponentToUseForCustomValidationLogicNotTiedToOneComponent(isInvalid), onRevalidation);

        public Validator OnValidation(OnValidationHandler onValidation)
        {
            OnValidationOccured += onValidation;
            return this;
        }

        internal void RaiseOnValidation()
        {
            // Debounce validation, as this can become expensive when creating a large number of components using the same validator
            window.clearTimeout(_timeout);
            _timeout = window.setTimeout(
                _ =>
                {
                    // Do NOT force a full revalidation just because one thing has changed, only validate components that the User has edited so far (call Revalidate() or check IsValid to force a FULL revalidation)
                    var looksValidsForFar = Revalidate(validateOnlyUserEditedComponents: true);
                    OnValidationOccured?.Invoke(haveEncounteredInvalidValue: !looksValidsForFar);
                },
                100
            );
        }

        /// <summary>
        /// This will trigger the validation logic for EVERY registered component and return false if any of them are not in a valid state (and, by doing so, their display state will be updated accordingly)
        /// </summary>
        public bool IsValid
        {
            get
            {
                return !Revalidate(); // If we want to know if the ENTIRE form that this validator is related to then we need to check ALL components and NOT just the ones that the User has edited so far
            }
        }

        /// <summary>
        /// This will trigger the validation logic for EVERY registered component and return false if any of them are not in a valid state (and, by doing so, their display state will be updated accordingly)
        /// </summary>
        public bool Revalidate()
        {
            var isValid = Revalidate(validateOnlyUserEditedComponents: false);
            OnValidationOccured?.Invoke(haveEncounteredInvalidValue: !isValid);
            return isValid;
        }

        /// <summary>
        /// This will return false if any of the components that were checked were found to be in an invalid state (the components checked depends upon validateOnlyUserEditedComponents and which registered components that the User has interacted with)
        /// </summary>
        private bool Revalidate(bool validateOnlyUserEditedComponents)
        {
            if (_callsDepth > 2)
                return true;

            var looksValidSoFar = true;
            _callsDepth++;
            foreach (var kv in _registeredComponents)
            {
                if (!validateOnlyUserEditedComponents || _registeredComponentsThatUserHasInteractedWith.Contains(kv.Key))
                {
                    kv.Value?.Invoke(); // Force revalidation
                    if (kv.Key.IsInvalid)
                    {
                        looksValidSoFar = false;
                    }
                }
            }
            _callsDepth--;
            return looksValidSoFar;
        }

        private sealed class DummyComponentToUseForCustomValidationLogicNotTiedToOneComponent : ICanValidate
        {
            private readonly Func<bool> _isInvalid;
            private readonly HTMLElement _innerElement;
            public DummyComponentToUseForCustomValidationLogicNotTiedToOneComponent(Func<bool> isInvalid)
            {
                _isInvalid = isInvalid;
                _innerElement = Span(_(text: "This is a dummy element to illustrate validation"));
            }

            public HTMLElement Render() => _innerElement;

            public string Error { get; set; }
            public bool IsInvalid { get => _isInvalid(); set => throw new NotSupportedException(); }
        }
    }
}