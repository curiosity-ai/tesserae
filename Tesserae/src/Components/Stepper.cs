using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Stepper")]
    public class Stepper : ComponentBase<Stepper, HTMLDivElement>
    {
        private readonly List<string> _steps = new List<string>();
        private int _currentStep = 0;

        public Stepper()
        {
            InnerElement = Div(_("tss-stepper"));
        }

        public Stepper SetSteps(params string[] steps)
        {
            _steps.Clear();
            _steps.AddRange(steps);
            Rebuild();
            return this;
        }

        public int CurrentStep
        {
            get => _currentStep;
            set { _currentStep = value; Rebuild(); }
        }

        private void Rebuild()
        {
            ClearChildren(InnerElement);
            for (int i = 0; i < _steps.Count; i++)
            {
                var stepContainer = Div(_("tss-stepper-step"));
                if (i < _currentStep) stepContainer.classList.add("tss-completed");
                if (i == _currentStep) stepContainer.classList.add("tss-active");

                var circle = Div(_("tss-stepper-circle", text: (i + 1).ToString()));
                var label = Div(_("tss-stepper-label", text: _steps[i]));

                stepContainer.appendChild(circle);
                stepContainer.appendChild(label);
                InnerElement.appendChild(stepContainer);
            }
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
