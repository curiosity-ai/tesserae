using System;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A two-step button: the first click arms it (the label morphs into a confirmation prompt, the button turns to
    /// the danger tone and a bar drains along its foot), a second click within the time limit confirms it and
    /// <see cref="OnConfirmed"/> fires. Letting the time run out, pressing Escape or moving focus away disarms it.
    /// It confirms a small destructive action in place, where a dialog would be out of proportion.
    /// </summary>
    [Transpose.Name("tss.ConfirmButton")]
    public sealed class ConfirmButton : ComponentBase<ConfirmButton, HTMLButtonElement>
    {
        private readonly HTMLSpanElement _textSpan;
        private readonly HTMLSpanElement _countdown;
        private readonly HTMLSpanElement _live;

        private event Action _confirmed;

        private string _text;
        private string _confirmText;
        private string _doneText;
        private int    _timeoutMs    = 3000;
        private int    _resetAfterMs = 1500;
        private double _timer;
        private int    _generation;
        private State  _state = State.Idle;
        private string _restingTone;

        private enum State { Idle, Armed, Done }

        /// <summary>
        /// Creates a two-step confirmation button.
        /// </summary>
        /// <param name="text">The resting label, e.g. "Remove item".</param>
        /// <param name="confirmText">The label once armed, e.g. "Click again to confirm".</param>
        /// <param name="doneText">The label once confirmed, e.g. "Removed"; null keeps the resting label.</param>
        public ConfirmButton(string text, string confirmText = "Click again to confirm", string doneText = null)
        {
            _text        = text ?? "";
            _confirmText = confirmText ?? _text;
            _doneText    = doneText;

            _textSpan  = Span(Att("tss-confirmbutton-text", text: _text));
            _countdown = Span(Att("tss-confirmbutton-countdown"));
            _live      = Span(Att("tss-holdbutton-live"));
            _live.setAttribute("aria-live", "polite");

            InnerElement = Button(Att("tss-btn tss-btn-default tss-confirmbutton tss-fontsize-small tss-fontweight-regular tss-default-component-margin"), _textSpan, _countdown, _live);
            InnerElement.type = "button";
            InnerElement.style.setProperty("--tss-confirm-timeout", _timeoutMs + "ms");

            InnerElement.addEventListener("click", _ =>
            {
                switch (_state)
                {
                    case State.Idle:  Arm();     break;
                    case State.Armed: Confirm(); break;
                }
            });

            InnerElement.addEventListener("keydown", e =>
            {
                if (_state == State.Armed && e.As<KeyboardEvent>().key == "Escape")
                {
                    e.stopPropagation();
                    Reset();
                }
            });

            InnerElement.addEventListener("blur", _ =>
            {
                if (_state == State.Armed) Reset();
            });
        }

        /// <summary>Whether the button is armed, waiting for the confirming click.</summary>
        public bool IsArmed => _state == State.Armed;

        /// <summary>Registers the action to run on the confirming click.</summary>
        public ConfirmButton OnConfirmed(Action onConfirmed)
        {
            _confirmed += onConfirmed;
            return this;
        }

        /// <summary>How long the button stays armed before it disarms itself, in milliseconds.</summary>
        public ConfirmButton Timeout(int milliseconds)
        {
            _timeoutMs = Math.Max(500, milliseconds);
            InnerElement.style.setProperty("--tss-confirm-timeout", _timeoutMs + "ms");
            return this;
        }

        /// <summary>
        /// How long the confirmed label is shown before the button goes back to rest, in milliseconds. Zero or less
        /// keeps it confirmed (and disabled) until <see cref="Reset"/> is called.
        /// </summary>
        public ConfirmButton ResetAfter(int milliseconds)
        {
            _resetAfterMs = milliseconds;
            return this;
        }

        /// <summary>Puts an icon before the label.</summary>
        public ConfirmButton SetIcon(UIcons icon)
        {
            var existing = (HTMLElement)InnerElement.querySelector(".tss-confirmbutton-icon");
            if (existing is object) existing.remove();
            InnerElement.insertBefore(I(icon, cssClass: "tss-confirmbutton-icon"), _textSpan);
            return this;
        }

        /// <summary>Enables or disables the button.</summary>
        public ConfirmButton Disabled(bool value = true)
        {
            InnerElement.disabled = value;
            InnerElement.UpdateClassIf(value, "tss-disabled");
            if (value && _state == State.Armed) Reset();
            return this;
        }

        /// <summary>Returns the button to its resting label and tone.</summary>
        public ConfirmButton Reset()
        {
            _generation++;
            window.clearTimeout(_timer);
            var wasIdle = _state == State.Idle;
            _state = State.Idle;
            InnerElement.disabled = InnerElement.classList.contains("tss-disabled");
            _live.textContent = "";

            if (wasIdle) return this;

            SetLabel(_text, () =>
            {
                InnerElement.classList.remove("tss-confirmbutton-armed", "tss-confirmbutton-done");
                SetDangerTone(false);
            });
            return this;
        }

        private void Arm()
        {
            _state = State.Armed;
            var generation = ++_generation;

            SetLabel(_confirmText, () =>
            {
                InnerElement.classList.add("tss-confirmbutton-armed");
                SetDangerTone(true);
            });
            _live.textContent = _confirmText;

            window.clearTimeout(_timer);
            _timer = window.setTimeout(_ =>
            {
                if (generation == _generation && _state == State.Armed) Reset();
            }, _timeoutMs);
        }

        private void Confirm()
        {
            _state = State.Done;
            var generation = ++_generation;
            window.clearTimeout(_timer);

            var label = string.IsNullOrEmpty(_doneText) ? _text : _doneText;
            SetLabel(label, () =>
            {
                InnerElement.classList.remove("tss-confirmbutton-armed");
                InnerElement.classList.add("tss-confirmbutton-done");
                SetDangerTone(false);
            });
            _live.textContent = label;

            _confirmed?.Invoke();

            if (_resetAfterMs > 0)
            {
                window.setTimeout(_ =>
                {
                    if (generation == _generation) Reset();
                }, _resetAfterMs);
            }
        }

        // Armed, the button borrows the theme's own danger button (tss-btn-danger), so every theme draws it the way
        // it draws any destructive button. The tone it had before is put back when it disarms or confirms.
        private void SetDangerTone(bool armed)
        {
            if (armed)
            {
                _restingTone = InnerElement.classList.contains("tss-btn-primary") ? "tss-btn-primary" : "tss-btn-default";
                InnerElement.classList.remove("tss-btn-default", "tss-btn-primary");
                InnerElement.classList.add("tss-btn-danger");
            }
            else if (_restingTone is object)
            {
                InnerElement.classList.remove("tss-btn-danger");
                InnerElement.classList.add(_restingTone);
                _restingTone = null;
            }
        }

        private void SetLabel(string text, Action alsoChange)
        {
            MicroInteractionExtensions.MorphWidth(InnerElement, () =>
            {
                alsoChange();
                _textSpan.textContent = text;
            });
            MicroInteractionExtensions.Play(_textSpan, "tss-mi-text-in");
        }

        /// <summary>Renders the component's root HTML element.</summary>
        public override HTMLElement Render() => InnerElement;
    }
}
