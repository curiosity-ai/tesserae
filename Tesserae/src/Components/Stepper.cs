using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A single step in a <see cref="Stepper"/> wizard, with title, description and completion state.
    /// </summary>
    public sealed class StepperStep
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public StepperStep(string title, IComponent content, string description = null)
        {
            Title       = title ?? string.Empty;
            Description = description ?? string.Empty;
            Content     = content;
        }

        /// <summary>
        /// Gets or sets the title of the component.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets the description of the component.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the content of the component.
        /// </summary>
        public IComponent Content { get; set; }
    }

    [H5.Name("tss.Stepper")]
    public sealed class Stepper : ComponentBase<Stepper, HTMLElement>, IBindableComponent<int>
    {
        private readonly List<StepperStep>       _steps;
        private readonly HTMLElement             _header;
        private readonly HTMLElement             _content;
        private readonly HTMLElement             _footer;
        private readonly HTMLButtonElement       _nextButton;
        private readonly HTMLButtonElement       _prevButton;
        private readonly SettableObservable<int> _observable;
        private          int                     _currentIndex;
        private          Action<Stepper>         _onStepChanged;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Stepper(params StepperStep[] steps)
        {
            _steps      = new List<StepperStep>();
            _header     = Div(_("tss-stepper-header"));
            _content    = Div(_("tss-stepper-content"));
            _footer     = Div(_("tss-stepper-footer"));
            _observable = new SettableObservable<int>(0);

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

        /// <summary>
        /// Gets or sets the current step index.
        /// </summary>
        public int CurrentStepIndex
        {
            get => _currentIndex;
            set => SetStep(value);
        }

        /// <summary>
        /// Gets or sets the current step.
        /// </summary>
        public StepperStep CurrentStep => _steps.Count == 0 ? null : _steps[_currentIndex];

        /// <summary>
        /// Adds the given step to the component.
        /// </summary>
        public Stepper AddStep(string title, IComponent content, string description = null)
        {
            _steps.Add(new StepperStep(title, content, description));
            UpdateHeader();
            UpdateContent();
            return this;
        }

        /// <summary>
        /// Adds the given steps to the component.
        /// </summary>
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

        /// <summary>
        /// Registers a callback invoked when the step change event fires.
        /// </summary>
        public Stepper OnStepChange(Action<Stepper> onStepChange)
        {
            _onStepChanged += onStepChange;
            return this;
        }

        /// <summary>
        /// Sets the step of the component.
        /// </summary>
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
            _observable.Value = _currentIndex;

            if (raiseEvent)
            {
                _onStepChanged?.Invoke(this);
            }

            return this;
        }

        /// <summary>
        /// Returns an observable that tracks the current step index.
        /// </summary>
        public IObservable<int> AsObservable() => _observable;

        /// <summary>
        /// Programmatically updates the current step index as part of a two-way binding.
        /// </summary>
        public void SetBoundValue(int value) => SetStep(value);

        /// <summary>
        /// Configures the component to next.
        /// </summary>
        public Stepper Next()
        {
            return SetStep(_currentIndex + 1);
        }

        /// <summary>
        /// Configures the component to previous.
        /// </summary>
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
                var circle   = Span(_("tss-stepper-circle",      text: (i + 1).ToString()));
                var label    = Span(_("tss-stepper-label",       text: step.Title));
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

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}