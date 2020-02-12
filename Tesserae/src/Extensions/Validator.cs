using System;
using System.Collections.Generic;
using System.Linq;

namespace Tesserae.Components
{
    public class Validator
    {
        private Dictionary<ICanValidate, ValidationHandler> RegisteredComponents = new Dictionary<ICanValidate, ValidationHandler>();

        public event OnValidationHandler onValidation;

        internal delegate void ValidationHandler(object e);

        public delegate void OnValidationHandler(bool isValid);

        private int CallsDepth = 0;

        internal void Register(ICanValidate component, ValidationHandler handler)
        {
            RegisteredComponents.Add(component, handler);
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
                return !RegisteredComponents.Keys.Any(c => c.IsInvalid);
            }
        }

        public void Revalidate()
        {
            if (CallsDepth > 2) { return; }
            CallsDepth++;
            foreach (var kv in RegisteredComponents)
            {
                kv.Value(this); //Force revalidation
            }
            CallsDepth--;
        }
    }
}