using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Inline questionnaire that shows a question and a list of choice
    /// buttons. Once the user picks an answer (or it is set programmatically
    /// via <see cref="SetAnswer"/>) the component switches to a
    /// "response selected" mode that shows the question with the chosen
    /// answer highlighted and disables further input.
    /// </summary>
    [Transpose.Name("tss.Questionnaire")]
    public sealed class Questionnaire : ComponentBase<Questionnaire, HTMLElement>
    {
        private readonly HTMLElement                _questionContainer;
        private readonly HTMLElement                _optionsContainer;
        private readonly HTMLElement                _answeredBadge;
        private readonly List<(string Value, HTMLButtonElement Button)> _optionButtons;
        private          string                     _question;
        private          string                     _answer;
        private          bool                       _isAnswered;
        private          Action<Questionnaire>      _onAnswered;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Questionnaire(string question, IEnumerable<string> options)
        {
            _question          = question ?? string.Empty;
            _optionButtons     = new List<(string, HTMLButtonElement)>();

            _questionContainer = Div(Att("tss-questionnaire-question", text: _question));
            _optionsContainer  = Div(Att("tss-questionnaire-options"));
            _answeredBadge     = Div(Att("tss-questionnaire-answered-badge"));
            _answeredBadge.style.display = "none";

            InnerElement = Div(Att("tss-questionnaire"),
                               _questionContainer,
                               _optionsContainer,
                               _answeredBadge);

            if (options != null)
            {
                foreach (var option in options)
                {
                    AddOption(option);
                }
            }
        }

        /// <summary>
        /// Gets the question shown by the component.
        /// </summary>
        public string Question   => _question;
        /// <summary>
        /// Gets the answer currently selected, or <c>null</c> when no answer has been set.
        /// </summary>
        public string Answer     => _answer;
        /// <summary>
        /// Returns a value indicating whether the component is in the "response selected" mode.
        /// </summary>
        public bool   IsAnswered => _isAnswered;

        /// <summary>
        /// Sets the question of the component.
        /// </summary>
        public Questionnaire SetQuestion(string question)
        {
            _question = question ?? string.Empty;
            _questionContainer.innerText = _question;
            return this;
        }

        /// <summary>
        /// Adds the given option to the component.
        /// </summary>
        public Questionnaire AddOption(string option)
        {
            if (option is null) return this;

            var btn = UI.Button(Att("tss-questionnaire-option", type: "button"));
            btn.innerText = option;

            var capturedValue = option;
            btn.addEventListener("click", _ =>
            {
                if (_isAnswered) return;
                SetAnswer(capturedValue);
                _onAnswered?.Invoke(this);
            });

            _optionButtons.Add((capturedValue, btn));
            _optionsContainer.appendChild(btn);
            return this;
        }

        /// <summary>
        /// Adds the given range of options to the component.
        /// </summary>
        public Questionnaire AddOptions(IEnumerable<string> options)
        {
            if (options == null) return this;
            foreach (var o in options) AddOption(o);
            return this;
        }

        /// <summary>
        /// Sets the selected answer and switches the component into the
        /// "response selected" mode. Calling this does not invoke the
        /// <see cref="OnAnswered"/> callback — use this to reflect an
        /// already-known answer (e.g. one fetched from the server).
        /// </summary>
        public Questionnaire SetAnswer(string answer)
        {
            _answer     = answer;
            _isAnswered = true;
            InnerElement.classList.add("tss-questionnaire-answered");

            foreach (var (value, btn) in _optionButtons)
            {
                btn.disabled = true;
                ((HTMLElement)btn).UpdateClassIf(string.Equals(value, answer, StringComparison.Ordinal), "tss-questionnaire-option-selected");
            }

            if (!string.IsNullOrEmpty(answer) && !_optionButtons.Any(o => string.Equals(o.Value, answer, StringComparison.Ordinal)))
            {
                _answeredBadge.innerText      = answer;
                _answeredBadge.style.display  = "block";
            }
            else
            {
                _answeredBadge.style.display = "none";
            }

            return this;
        }

        /// <summary>
        /// Clears the selected answer and re-enables the option buttons.
        /// </summary>
        public Questionnaire ClearAnswer()
        {
            _answer     = null;
            _isAnswered = false;
            InnerElement.classList.remove("tss-questionnaire-answered");
            _answeredBadge.style.display = "none";

            foreach (var (_, btn) in _optionButtons)
            {
                btn.disabled = false;
                btn.classList.remove("tss-questionnaire-option-selected");
            }

            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the user picks an option.
        /// The callback is not invoked when the answer is set
        /// programmatically via <see cref="SetAnswer"/>.
        /// </summary>
        public Questionnaire OnAnswered(Action<Questionnaire> onAnswered)
        {
            _onAnswered += onAnswered;
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
