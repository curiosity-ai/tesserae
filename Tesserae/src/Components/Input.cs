using System;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Abstract base class for input components that wrap a single <see cref="HTMLInputElement"/>.
    /// Provides shared behaviour for text-like inputs (value, validation, focus, observable binding).
    /// </summary>
    /// <typeparam name="TInput">The concrete input type (CRTP self-reference for fluent return types).</typeparam>
    [Transpose.Name("tss.Input")]
    public abstract class Input<TInput> : ComponentBase<TInput, HTMLInputElement>, ITabIndex, ICanValidate<TInput>, IBindableComponent<string> where TInput : Input<TInput>
    {
        private readonly HTMLDivElement             _container;
        private readonly HTMLSpanElement            _errorSpan;
        private readonly SettableObservable<string> _observable;

        private Func<string, Task<InputCheck>> _check;
        private int                            _checkDebounceMs = 400;
        private double                         _checkTimeout;
        private int                            _checkRequest;
        private HTMLElement                    _status, _statusIcon, _statusText;
        private IComponent                     _statusComponent;

        /// <summary>
        /// Initializes a new instance of the <see cref="Input{TInput}"/> class.
        /// </summary>
        /// <param name="type">The HTML <c>input</c> <c>type</c> attribute (e.g. <c>text</c>, <c>date</c>, <c>number</c>).</param>
        /// <param name="defaultText">Optional initial value for the input.</param>
        protected Input(string type, string defaultText = null)
        {
            // The initial text is the input's default value - its value attribute - rather than its live value,
            // so it is in the markup: a form reset restores it, and DeltaComponent, which patches a re-render
            // into the node already on screen through its attributes, shows the new text. The live value
            // follows the attribute until the user types or Text is set.
            InnerElement = UI.TextBox(Att("tss-textbox", type: type, defaultValue: defaultText));
            _errorSpan = Span(Att("tss-textbox-error"));
            _container = Div(Att("tss-textbox-container tss-default-component-margin"), InnerElement, _errorSpan);

            _observable = new SettableObservable<string>(defaultText);

            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
            AttachKeys();
            AttachClick();

            // Subscribe to the underlying events directly rather than calling the virtual
            // OnChange / OnInput methods, which would dispatch to derived overrides before
            // the derived constructor has finished running (a classic virtual-in-constructor
            // antipattern). This achieves the same observable-update behaviour safely.
            SubscribeChanged((_,      __) => _observable.Value = Text);
            SubscribeInputUpdated((_, __) =>
            {
                _observable.Value = Text;
                ScheduleCheck();
            });
        }

        /// <summary>
        /// This will reset the input to a blank state but it will NOT trigger the InputUpdated event because this should be used when a form is being programmatically reset, as opposed to when the User has set the field to blank - the important difference is
        /// that if a form is reset then it should not immediately be covered in validation warnings until the User starts to interact with the reset form (and firing InputUpdated will cause any validator that this component is registered with to revalidate)
        /// </summary>
        public void Reset()
        {
            InnerElement.value = "";
            _observable.Value  = "";
        }

        /// <summary>
        /// Gets or sets the text shown in the component.
        /// </summary>
        public string Text
        {
            get => InnerElement.value;
            set
            {
                InnerElement.value = value;
                _observable.Value  = value;
                RaiseOnInput(null);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the component is interactive (enabled).
        /// </summary>
        public bool IsEnabled
        {
            get => !InnerElement.classList.contains("tss-disabled");
            set
            {
                if (value)
                {
                    InnerElement.classList.remove("tss-disabled");
                    InnerElement.disabled = false;
                }
                else
                {
                    InnerElement.classList.add("tss-disabled");
                    InnerElement.disabled = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the validation error message displayed beneath the component.
        /// </summary>
        public string Error
        {
            get => _errorSpan.innerText;
            set
            {
                _errorSpan.innerText = value;
                _errorSpan.title     = value ?? ""; // A validated field keeps its message to one line and ellipsizes it, so the full text stays reachable
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the component is currently in an invalid state.
        /// </summary>
        public bool IsInvalid
        {
            get => _container.classList.contains("tss-invalid");
            set
            {
                if (value)
                {
                    _container.classList.add("tss-invalid");
                }
                else
                {
                    _container.classList.remove("tss-invalid");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the component is required for form submission.
        /// </summary>
        public bool IsRequired
        {
            get => _container.classList.contains("tss-required");
            set
            {
                if (value)
                {
                    _container.classList.add("tss-required");
                }
                else
                {
                    _container.classList.remove("tss-required");
                }
            }
        }

        /// <summary>
        /// Sets the keyboard tab order of the component.
        /// </summary>
        public int TabIndex
        {
            set
            {
                InnerElement.tabIndex = value;
            }
        }

        /// <summary>
        /// Attaches a handler to the component's value-changed event.
        /// </summary>
        public void Attach(ComponentEventHandler<TInput> handler)
        {
            SubscribeInputUpdated((s, _) => handler(s));
        }

        /// <summary>
        /// Sets the text of the component.
        /// </summary>
        public TInput SetText(string text)
        {
            Text = text;
            return this.As<TInput>();
        }

        /// <summary>
        /// Clears the text.
        /// </summary>
        public TInput ClearText()
        {
            SetText(string.Empty);
            return this.As<TInput>();
        }

        /// <summary>
        /// Disables the component.
        /// </summary>
        public TInput Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this.As<TInput>();
        }

        /// <summary>
        /// Removes / disables the spell check on the component.
        /// </summary>
        public TInput NoSpellCheck()
        {
            InnerElement.spellcheck = false;
            return this.As<TInput>();
        }

        /// <summary>
        /// Marks the component as required.
        /// </summary>
        public TInput Required()
        {
            IsRequired = true;
            return this.As<TInput>();
        }

        /// <summary>
        /// Moves keyboard focus to the component.
        /// </summary>
        public TInput Focus()
        {
            // 2020-12-29 DWR: Seems like this setTimeout is required then the element is rendered within a container that uses "simplebar" scrolling - without the delay, if the element getting focus is out of view then it will not be
            // scrolled into view (even though it has successfully received focus)
            DomObserver.WhenMounted(InnerElement, () =>
            {
                try
                {
                    InnerElement.scrollIntoViewIfNeeded();
                }
                catch
                {
                    InnerElement.scrollIntoView();
                }
                InnerElement.focus();
            });
            return this.As<TInput>();
        }

        /// <summary>
        /// Returns the component's state as a(n) observable.
        /// </summary>
        public IObservable<string> AsObservable() => _observable;

        /// <summary>
        /// Programmatically updates the input as part of a two-way binding (DOM input event is not raised).
        /// </summary>
        public void SetBoundValue(string value)
        {
            InnerElement.value = value;
            _observable.Value  = value;
        }

        /// <summary>
        /// Checks the text asynchronously after each edit, debounced by <paramref name="debounceMs"/>,
        /// and shows the answer at the end of the box as an icon and its message, with the answer's
        /// tooltip (if any) over them: <see cref="InputCheck.Valid"/>, <see cref="InputCheck.Warning"/>,
        /// <see cref="InputCheck.Invalid"/> (which also draws the border red), or
        /// <see cref="InputCheck.None"/> for nothing. An empty box is not checked; the initial text is
        /// checked straight away. Only the answer for the text currently in the box is shown.
        /// </summary>
        /// <remarks>
        /// Apart from <see cref="Error"/> and <see cref="IsInvalid"/>, so a synchronous
        /// <c>.Validation(...)</c> on the same box keeps working; <see cref="LastCheck"/> holds the verdict.
        /// </remarks>
        public TInput CheckAsync(Func<string, Task<InputCheck>> check, int debounceMs = 400)
        {
            if (_status is null) BuildCheckIndicator();

            _check           = check;
            _checkDebounceMs = Math.Max(0, debounceMs);
            return Recheck();
        }

        /// <summary>Runs the check now for the current text, without waiting for the debounce.</summary>
        public TInput Recheck()
        {
            window.clearTimeout(_checkTimeout);
            RunCheckAsync().FireAndForget();
            return this.As<TInput>();
        }

        /// <summary>The answer of the last check that completed for the current text, or <c>null</c>.</summary>
        public InputCheck LastCheck { get; private set; }

        // Built on the first CheckAsync, so an input without a check carries none of it.
        private void BuildCheckIndicator()
        {
            // An icon and a short label inside the box at its end, which is also what a screen reader
            // announces; the check's explanation, if it has one, is its tooltip.
            _statusIcon      = I(Att("tss-input-check-icon"));
            _statusText      = Span(Att("tss-input-check-text"));
            _status          = Div(Att("tss-input-check"), _statusIcon, _statusText);
            _statusComponent = Raw(_status);
            _status.setAttribute("aria-live", "polite");

            var field = Div(Att("tss-input-check-field"));
            _container.insertBefore(field, InnerElement);
            field.appendChild(InnerElement);
            field.appendChild(_status);
            InnerElement.classList.add("tss-input-has-check");

            // The label's length depends on the answer, so the input's end padding follows the
            // indicator's measured width and typed text never runs under it.
            new ResizeObserver((entries, obs) =>
            {
                if (!_status.isConnected) return; // Detached, it measures as nothing
                field.style.setProperty("--tss-input-check-width", _status.offsetWidth + "px");
            }).observe(_status);
        }

        private void ScheduleCheck()
        {
            if (_check is null) return;

            window.clearTimeout(_checkTimeout);
            _checkRequest++;

            if (string.IsNullOrWhiteSpace(Text))
            {
                ShowCheck(null);
                return;
            }

            _checkTimeout = window.setTimeout(_ => RunCheckAsync().FireAndForget(), _checkDebounceMs);
        }

        private async Task RunCheckAsync()
        {
            if (_check is null) return;

            var request = ++_checkRequest;
            var text    = Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                ShowCheck(null);
                return;
            }

            ShowChecking();

            var result = await _check(text);

            // A later edit started its own check; its answer is the one the box should show.
            if (request != _checkRequest) return;

            ShowCheck(result);
        }

        private void ShowChecking()
        {
            _status.className     = "tss-input-check tss-input-check-checking";
            _statusIcon.className = "tss-input-check-icon " + Icon.Transform(UIcons.Spinner, UIconsWeight.Regular);
            _statusText.innerText = "Checking…";
            _status.title         = "";
            SetCheckTooltip(null);
        }

        private void ShowCheck(InputCheck result)
        {
            LastCheck = result;

            _container.classList.toggle("tss-input-check-invalid", result?.State == InputCheckState.Invalid);

            if (result is null || result.State == InputCheckState.None)
            {
                _status.className     = "tss-input-check";
                _statusIcon.className = "tss-input-check-icon";
                _statusText.innerText = "";
                _status.title         = "";
                SetCheckTooltip(null);
                return;
            }

            UIcons icon;
            string state;

            switch (result.State)
            {
                case InputCheckState.Valid:   icon = UIcons.CheckCircle;     state = "valid";   break;
                case InputCheckState.Warning: icon = UIcons.TriangleWarning; state = "warning"; break;
                default:                      icon = UIcons.CrossCircle;     state = "invalid"; break;
            }

            _status.className     = "tss-input-check tss-input-check-visible tss-input-check-state-" + state;
            _statusIcon.className = "tss-input-check-icon " + Icon.Transform(icon, UIconsWeight.Regular);
            _statusText.innerText = result.Message ?? "";

            // A long label is cut short, so without a tooltip the full message stays reachable as the title.
            _status.title = string.IsNullOrWhiteSpace(result.Tooltip) ? result.Message ?? "" : "";
            SetCheckTooltip(result.Tooltip);
        }

        private void SetCheckTooltip(string tooltip)
        {
            _statusComponent.RemoveTooltip();

            if (string.IsNullOrWhiteSpace(tooltip))
            {
                _status.classList.remove("tss-input-check-has-tooltip");
                return;
            }

            _status.classList.add("tss-input-check-has-tooltip");
            _statusComponent.Tooltip(TextBlock(tooltip));
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => _container;
    }
}