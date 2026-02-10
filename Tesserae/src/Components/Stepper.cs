using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public sealed class StepperStep
    {
        public StepperStep(string title, IComponent content, string description = null)
        {
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
            Content = content;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public IComponent Content { get; set; }
    }

    [H5.Name("tss.Stepper")]
    public sealed class Stepper : ComponentBase<Stepper, HTMLElement>
    {
        private readonly List<StepperStep> _steps;
        private readonly HTMLElement       _header;
        private readonly HTMLElement       _content;
        private readonly HTMLElement       _footer;
        private readonly HTMLButtonElement _nextButton;
        private readonly HTMLButtonElement _prevButton;
        private          int               _currentIndex;
        private          Action<Stepper>   _onStepChanged;

        public Stepper(params StepperStep[] steps)
        {
            _steps   = new List<StepperStep>();
            _header  = Div(_("tss-stepper-header"));
            _content = Div(_("tss-stepper-content"));
            _footer  = Div(_("tss-stepper-footer"));

            _prevButton = Button(_("tss-stepper-nav", text: "Back", type: "button"));
            _nextButton = Button(_("tss-stepper-nav", text: "Next", type: "button"));

            _prevButton.addEventListener("click", _ => Previous());
            _nextButton.addEventListener("click", _ => Next());

            _footer.appendChild(_prevButton);
            _footer.appendChild(_nextButton);

            InnerElement = Div(_("tss-stepper"), _header, _content, _footer);

            AddSteps(steps);
            SetStep(0, false);
        }

        public int CurrentStepIndex
        {
            get => _currentIndex;
            set => SetStep(value);
        }

        public StepperStep CurrentStep => _steps.Count == 0 ? null : _steps[_currentIndex];

        public Stepper AddStep(string title, IComponent content, string description = null)
        {
            _steps.Add(new StepperStep(title, content, description));
            UpdateHeader();
            UpdateContent();
            return this;
        }

        public Stepper AddSteps(params StepperStep[] steps)
        {
            if (steps == null)
            {
                return this;
            }

            foreach (var step in steps)
            {
                if (step != null)
                {
                    _steps.Add(step);
                }
            }

            UpdateHeader();
            UpdateContent();
            return this;
        }

        public Stepper OnStepChange(Action<Stepper> onStepChange)
        {
            _onStepChanged += onStepChange;
            return this;
        }

        public Stepper SetStep(int index, bool raiseEvent = true)
        {
            if (_steps.Count == 0)
            {
                _currentIndex = 0;
                UpdateHeader();
                UpdateContent();
                return this;
            }

            var clamped = Math.Max(0, Math.Min(index, _steps.Count - 1));

            if (_currentIndex == clamped)
            {
                return this;
            }

            _currentIndex = clamped;
            UpdateHeader();
            UpdateContent();

            if (raiseEvent)
            {
                _onStepChanged?.Invoke(this);
            }

            return this;
        }

        public Stepper Next()
        {
            return SetStep(_currentIndex + 1);
        }

        public Stepper Previous()
        {
            return SetStep(_currentIndex - 1);
        }

        private void UpdateHeader()
        {
            ClearChildren(_header);

            for (var i = 0; i < _steps.Count; i++)
            {
                var step     = _steps[i];
                var stepItem = Div(_("tss-stepper-step"));
                var circle   = Span(_("tss-stepper-circle", text: (i + 1).ToString()));
                var label    = Span(_("tss-stepper-label", text: step.Title));
                var desc     = Span(_("tss-stepper-description", text: step.Description));

                stepItem.appendChild(circle);
                stepItem.appendChild(label);

                if (!string.IsNullOrEmpty(step.Description))
                {
                    stepItem.appendChild(desc);
                }

                if (i < _currentIndex)
                {
                    stepItem.classList.add("tss-stepper-complete");
                }

                if (i == _currentIndex)
                {
                    stepItem.classList.add("tss-stepper-active");
                }

                var stepIndex = i;
                stepItem.addEventListener("click", _ => SetStep(stepIndex));

                _header.appendChild(stepItem);
            }
        }

        private void UpdateContent()
        {
            ClearChildren(_content);

            if (_steps.Count == 0)
            {
                return;
            }

            var step = _steps[_currentIndex];

            if (step?.Content is object)
            {
                _content.appendChild(step.Content.Render());
            }

            _prevButton.disabled = _currentIndex == 0;
            _nextButton.disabled = _currentIndex >= _steps.Count - 1;
            _prevButton.UpdateClassIf(_prevButton.disabled, "tss-disabled");
            _nextButton.UpdateClassIf(_nextButton.disabled, "tss-disabled");
        }

        public override HTMLElement Render() => InnerElement;
    }
}
