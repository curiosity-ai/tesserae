using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;

namespace Tesserae.Components
{
    public class Validator
    {
        private Dictionary<ICanValidate, Action> _registeredComponents;

        public event OnValidationHandler onValidation;

        public delegate void OnValidationHandler(bool isValid);

        private int CallsDepth = 0;

        public Validator()
        {
            _registeredComponents = new Dictionary<ICanValidate, Action>();
        }

        internal void Register(ICanValidate component, Action onRevalidation)
        {
            _registeredComponents.Add(component, onRevalidation);
        }

        public void RegisterFromCallback(Func<bool> isInvalid, Action onRevalidation)
        {
            _registeredComponents.Add(new Dummy(isInvalid), onRevalidation);
        }

        private class Dummy : ICanValidate
        {
            private Func<bool> _isInvalid;

            public Dummy(Func<bool> isInvalid) => _isInvalid = isInvalid;

            public string Error { get ; set ; }
            public bool IsInvalid { get => _isInvalid(); set => throw new NotSupportedException(); }
        }

        public Validator OnValidation(OnValidationHandler onValidation)
        {
            this.onValidation += onValidation;
            return this;
        }

        private double _timeout = 0;
        internal void RaiseOnValidation()
        {
            //Debounce validation, as this can become expensive when creating a large number of components using the same validator
            window.clearTimeout(_timeout);
            _timeout = window.setTimeout(_ => {
                Revalidate();
                onValidation?.Invoke(IsValid);
            }, 100);
        }

        public bool IsValid
        {
            get
            {
                Revalidate();
                return !_registeredComponents.Keys.Any(c => c.IsInvalid);
            }
        }

        public void Revalidate()
        {
            if (CallsDepth > 2) { return; }
            CallsDepth++;
            foreach (var kv in _registeredComponents)
            {
                kv.Value?.Invoke(); //Force revalidation
            }
            CallsDepth--;
        }
    }
}