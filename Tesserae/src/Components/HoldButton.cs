using System;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A button that has to be pressed and held to act: a fill sweeps across it while it is held, letting go early
    /// drains the fill back, and holding to the end confirms (a check replaces the icon and <see cref="OnConfirmed"/>
    /// fires). It guards a destructive or irreversible action without a confirmation dialog: the hold is the
    /// confirmation, and it cannot happen by accident.
    /// <para>
    /// Works with the pointer (mouse, touch, pen) and the keyboard (hold Space or Enter). The fill is a CSS
    /// transition, so a release mid-way reverses from wherever the fill had reached.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.HoldButton")]
    public sealed class HoldButton : ComponentBase<HoldButton, HTMLButtonElement>
    {
        private readonly HTMLSpanElement _fill;
        private readonly HTMLSpanElement _textSpan;
        private readonly HTMLElement     _icon;
        private readonly HTMLSpanElement _live;

        private event Action _confirmed;

        private string _text;
        private string _confirmedText;
        private int    _durationMs;
        private int    _resetAfterMs = 1500;
        private double _timer;
        private bool   _holding;
        private bool   _confirmedState;
        private int    _generation;

        /// <summary>
        /// Creates a hold-to-confirm button.
        /// </summary>
        /// <param name="text">The label, e.g. "Hold to delete".</param>
        /// <param name="durationMs">How long the button must be held, in milliseconds.</param>
        public HoldButton(string text, int durationMs = 1200)
        {
            _text       = text ?? "";
            _durationMs = Math.Max(200, durationMs);

            _fill     = Span(Att("tss-holdbutton-fill"));
            _icon     = I(UIcons.Check, cssClass: "tss-holdbutton-check");
            _textSpan = Span(Att("tss-holdbutton-text", text: _text));
            _live     = Span(Att("tss-holdbutton-live"));
            _live.setAttribute("aria-live", "polite");

            InnerElement = Button(Att("tss-btn tss-btn-default tss-holdbutton tss-fontsize-small tss-fontweight-regular tss-default-component-margin"), _fill, _icon, _textSpan, _live);
            InnerElement.type = "button";
            InnerElement.style.setProperty("--tss-hold-duration", _durationMs + "ms");
            InnerElement.setAttribute("aria-description", "Press and hold to confirm");

            InnerElement.addEventListener("pointerdown", e =>
            {
                var pe = e.As<MouseEvent>();
                if (pe.button != 0) return;
                Script.Write("try { {0}.setPointerCapture({1}.pointerId); } catch (x) { }", InnerElement, e);
                StartHold();
            });

            InnerElement.addEventListener("pointerup",          _ => Release());
            InnerElement.addEventListener("pointercancel",      _ => Release());
            InnerElement.addEventListener("lostpointercapture", _ => Release());
            InnerElement.addEventListener("contextmenu",        e => { if (_holding) e.preventDefault(); });

            InnerElement.addEventListener("keydown", e =>
            {
                var ke = e.As<KeyboardEvent>();
                if (ke.key != " " && ke.key != "Enter") return;
                e.preventDefault();
                if (!ke.repeat) StartHold();
            });

            InnerElement.addEventListener("keyup", e =>
            {
                var ke = e.As<KeyboardEvent>();
                if (ke.key != " " && ke.key != "Enter") return;
                e.preventDefault();
                Release();
            });

            InnerElement.addEventListener("blur", _ => Release());
        }

        /// <summary>Whether the hold has completed and the button shows its confirmed state.</summary>
        public bool IsConfirmed => _confirmedState;

        /// <summary>Gets or sets the label.</summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value ?? "";
                if (!_confirmedState) _textSpan.textContent = _text;
            }
        }

        /// <summary>Registers the action to run when a hold completes.</summary>
        public HoldButton OnConfirmed(Action onConfirmed)
        {
            _confirmed += onConfirmed;
            return this;
        }

        /// <summary>Sets how long the button must be held, in milliseconds.</summary>
        public HoldButton Duration(int milliseconds)
        {
            _durationMs = Math.Max(200, milliseconds);
            InnerElement.style.setProperty("--tss-hold-duration", _durationMs + "ms");
            return this;
        }

        /// <summary>The label shown once the hold completes, e.g. "Deleted". Defaults to the button's own label.</summary>
        public HoldButton ConfirmedText(string text)
        {
            _confirmedText = text;
            return this;
        }

        /// <summary>
        /// How long the confirmed state is held before the button goes back to its resting state, in milliseconds.
        /// Zero or less keeps it confirmed (and disabled) until <see cref="Reset"/> is called.
        /// </summary>
        public HoldButton ResetAfter(int milliseconds)
        {
            _resetAfterMs = milliseconds;
            return this;
        }

        /// <summary>Uses the primary tone.</summary>
        public HoldButton Primary()
        {
            InnerElement.classList.remove("tss-btn-default", "tss-btn-danger");
            InnerElement.classList.add("tss-btn-primary");
            return this;
        }

        /// <summary>Uses the danger tone, for a destructive action.</summary>
        public HoldButton Danger()
        {
            InnerElement.classList.remove("tss-btn-default", "tss-btn-primary");
            InnerElement.classList.add("tss-btn-danger");
            return this;
        }

        /// <summary>Puts an icon before the label; the check that confirms the hold takes its place.</summary>
        public HoldButton SetIcon(UIcons icon)
        {
            var existing = (HTMLElement)InnerElement.querySelector(".tss-holdbutton-icon");
            if (existing is object) existing.remove();
            InnerElement.insertBefore(I(icon, cssClass: "tss-holdbutton-icon"), _icon);
            return this;
        }

        /// <summary>Enables or disables the button.</summary>
        public HoldButton Disabled(bool value = true)
        {
            InnerElement.disabled = value;
            InnerElement.UpdateClassIf(value, "tss-disabled");
            if (value) Release();
            return this;
        }

        /// <summary>Returns the button to its resting state.</summary>
        public HoldButton Reset()
        {
            _generation++;
            window.clearTimeout(_timer);
            _holding        = false;
            _confirmedState = false;
            InnerElement.classList.remove("tss-holdbutton-holding", "tss-holdbutton-confirmed");
            InnerElement.disabled = InnerElement.classList.contains("tss-disabled");
            _textSpan.textContent = _text;
            _live.textContent     = "";
            return this;
        }

        private void StartHold()
        {
            if (_holding || _confirmedState || InnerElement.disabled) return;

            _holding = true;
            var generation = ++_generation;

            InnerElement.classList.add("tss-holdbutton-holding");

            window.clearTimeout(_timer);
            _timer = window.setTimeout(_ =>
            {
                if (generation != _generation || !_holding) return;
                Confirm();
            }, _durationMs);
        }

        private void Release()
        {
            if (!_holding) return;
            _holding = false;
            window.clearTimeout(_timer);
            InnerElement.classList.remove("tss-holdbutton-holding");
        }

        private void Confirm()
        {
            _holding        = false;
            _confirmedState = true;
            var generation  = ++_generation;

            InnerElement.classList.remove("tss-holdbutton-holding");
            InnerElement.classList.add("tss-holdbutton-confirmed");

            var label = string.IsNullOrEmpty(_confirmedText) ? _text : _confirmedText;
            _textSpan.textContent = label;
            _live.textContent     = label;
            MicroInteractionExtensions.Play(_textSpan, "tss-mi-text-in");

            _confirmed?.Invoke();

            if (_resetAfterMs > 0)
            {
                window.setTimeout(_ =>
                {
                    if (generation == _generation) Reset();
                }, _resetAfterMs);
            }
        }

        /// <summary>Renders the component's root HTML element.</summary>
        public override HTMLElement Render() => InnerElement;
    }
}
