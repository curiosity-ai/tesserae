using System;
using System.Collections.Generic;
using System.Linq;

namespace Tesserae.Components
{
    public class Validator
    {
        private Dictionary<ICanValidate, ValidationHandler> _registeredComponents = new Dictionary<ICanValidate, ValidationHandler>();

        public event OnValidationHandler onValidation;

        internal delegate void ValidationHandler(Validator e);

        public delegate void OnValidationHandler(bool isValid);

        private int CallsDepth = 0;

        internal void Register(ICanValidate component, ValidationHandler handler)
        {
            _registeredComponents.Add(component, handler);
        }

        public void RegisterFromCallback(Func<bool> isInvalid, Action<Validator> onRevalidation)
        {
            _registeredComponents.Add(new Dummy(isInvalid), (v) => onRevalidation(v));
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

        internal void RaiseOnValidation()
        {
            Revalidate();
            onValidation?.Invoke(IsValid);
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
                kv.Value(this); //Force revalidation
            }
            CallsDepth--;
        }
    }
}