using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;
using System;
using System.Threading.Tasks;

namespace Tesserae
{
    /// <summary>
    /// A button variant that shows a "saving…" spinner and a confirmation tick while an async save operation is in
    /// progress.
    /// </summary>
    [Name("tss.SaveButton")]
    public class SaveButton : IComponent
    {
        private Button _button;
        private string _textSave = "Save";
        private string _textSaveHover = null;
        private string _textVerifying = "Verifying...";
        private string _textSaving = "Saving...";
        private string _textSaved = "Saved";
        private string _textError = "Error";
        private UIcons _iconSave = UIcons.Disk;
        private UIcons _iconSaveHover = UIcons.Disk;
        private State _state;
        private int _stateVersion;
        private bool _hovering;
        private bool _pendingPrimary = true;

        // The visible face (icon or spinner, then the label) shares a grid cell with a hidden sizer that always
        // shows the Verifying label. Only the sizer counts towards the width (see tss.button.css), so the button
        // keeps one width across every state and any label wider than "Verifying..." is ellipsized.
        private readonly HTMLElement _icon       = I(Att());
        private readonly HTMLElement _spinner    = Div(Att("tss-spinner"));
        private readonly HTMLElement _spinnerBox;
        private readonly HTMLElement _label      = Span(Att());
        private readonly HTMLElement _sizerLabel = Span(Att());

        public enum State
        {
            NothingToSave,
            PendingSave,
            Verifying,
            Saving,
            Saved,
            Error,
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public SaveButton()
        {
            _button = Button().MinWidth(100.px());
            var element = _button.Render();
            _spinnerBox = SmallSpinner(_spinner);
            var sizer   = Span(Att("tss-savebtn-face tss-savebtn-sizer"), SmallSpinner(Div(Att("tss-spinner"))), _sizerLabel);
            sizer.setAttribute("aria-hidden", "true");
            element.appendChild(Span(Att("tss-savebtn-faces"), sizer, Span(Att("tss-savebtn-face"), _icon, _spinnerBox, _label)));
            element.addEventListener("mouseenter", (e) =>
            {
                if (_state == State.PendingSave && !string.IsNullOrEmpty(_textSaveHover))
                {
                    _hovering = true;
                    SetState(_state);
                }
            });

            element.addEventListener("mouseleave", (e) =>
            {
                if (_state == State.PendingSave)
                {
                    _hovering = false;
                    SetState(_state);
                }
            });
            SetState(State.NothingToSave);
        }

        /// <summary>
        /// Configures the component to configure.
        /// </summary>
        public SaveButton Configure(string save = null, string verifying = null, string saving = null, string saved = null, string error = null, string saveHover = null, UIcons saveIcon = UIcons.Disk, UIcons saveHoverIcon = UIcons.Disk, bool pendingPrimary = true)
        {
            if (save != null) _textSave = save;
            if (verifying != null) _textVerifying = verifying;
            if (saving != null) _textSaving = saving;
            if (saved != null) _textSaved = saved;
            if (error != null) _textError = error;
            if (saveHover != null) _textSaveHover = saveHover;

            if(string.IsNullOrEmpty(_textSaveHover))  _textSaveHover = save;
            
            _pendingPrimary = pendingPrimary;
            _iconSave = saveIcon;
            _iconSaveHover = saveHoverIcon;

            SetState(_state);

            return this;
        }

        /// <summary>
        /// Sets the state of the component.
        /// </summary>
        public SaveButton SetState(State state, string message = null)
        {
            _state = state;
            _stateVersion++;
            _sizerLabel.innerText = _textVerifying ?? "";

            // Reset base styles
            _button.IsPrimary = false;
            _button.IsSuccess = false;
            _button.IsDanger = false;
            _button.IsEnabled = state != State.NothingToSave; // Default to enabled
            _button.RemoveTooltip();

            UIcons? icon = null;
            string  text;

            switch (state)
            {
                case State.NothingToSave:
                case State.PendingSave:
                    _button.IsPrimary = _pendingPrimary;
                    icon = _hovering ? _iconSaveHover : _iconSave;
                    text = _hovering ? _textSaveHover : _textSave;
                    break;
                case State.Verifying:
                    _button.IsPrimary = true;
                    _button.IsEnabled = false;
                    text = message ?? _textVerifying;
                    break;
                case State.Saving:
                    _button.IsSuccess = true;
                    _button.IsEnabled = false;
                    text = message ?? _textSaving;
                    break;
                case State.Saved:
                    _button.IsSuccess = true;
                    icon = UIcons.Check;
                    text = _textSaved;
                    break;
                default:
                    _button.IsDanger = true;
                    icon = UIcons.OctagonXmark;
                    text = _textError;
                    if (!string.IsNullOrEmpty(message))
                    {
                        _button.Tooltip(message);
                    }
                    break;
            }

            if (icon.HasValue)
            {
                _icon.className = $"{Tesserae.Icon.Transform(icon.Value, UIconsWeight.Regular)} {TextSize.Small}";
            }

            _icon.style.display       = icon.HasValue ? "" : "none";
            _spinnerBox.style.display = icon.HasValue ? "none" : "";
            _spinner.UpdateClassIf(state == State.Saving, "tss-spinner-success");
            _label.innerText          = text ?? "";

            return this;
        }

        
        /// <summary>
        /// Configures the nothing to save on the component.
        /// </summary>
        public SaveButton NothingToSave(string message = null) => SetState(State.NothingToSave, message);
        /// <summary>
        /// Configures the component to pending.
        /// </summary>
        public SaveButton Pending(string message = null) => SetState(State.PendingSave, message);
        /// <summary>
        /// Configures the component to verifying.
        /// </summary>
        public SaveButton Verifying(string message = null) => SetState(State.Verifying, message);
        /// <summary>
        /// Configures the component to saving.
        /// </summary>
        public SaveButton Saving(string message = null) => SetState(State.Saving, message);
        /// <summary>
        /// Configures the component to saved.
        /// </summary>
        public SaveButton Saved(string message = null) => SetState(State.Saved, message);
        /// <summary>
        /// Gets or sets the validation error message displayed beneath the component.
        /// </summary>
        public SaveButton Error(string message = null) => SetState(State.Error, message);


        /// <summary>
        /// Registers a callback invoked when the click event fires.
        /// </summary>
        public SaveButton OnClick(Action action)
        {
            _button.OnClick(() =>
            {
                if (_state != State.PendingSave) return;
                action();
            });
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the click spin while event fires.
        /// </summary>
        public SaveButton OnClickSpinWhile(Func<Task> actionAsync) => OnClickSpinWhile(actionAsync, null, null);

        /// <summary>
        /// Configures the verifying while on the component.
        /// </summary>
        public async Task<State> VerifyingWhile(Func<Task<State>> action, string text = null, Action<SaveButton, Exception> onError = null)
        {
            SetState(State.Verifying, text);
            try
            {
                var result = await action();
                SetState(result);
                return result;
            }
            catch (Exception e)
            {
                if (onError != null)
                {
                    onError(this, e);
                }
                else
                {
                    SetState(State.Error);
                    Toast().Error(e.Message);
                }
                return State.Error;
            }
        }

        /// <summary>
        /// Registers a callback invoked when the click spin while event fires.
        /// </summary>
        public SaveButton OnClickSpinWhile(Func<Task> action, string text = null, Action<SaveButton, Exception> onError = null)
        {
            // Not Button.OnClickSpinWhile: its ToSpinner swaps the button for a clone that does not keep this width.
            _button.OnClick((_, e) =>
            {
                StopEvent(e);

                if (_state != State.PendingSave) return;

                Task.Run(async () =>
                {
                    SetState(State.Saving, text);
                    var version = _stateVersion;

                    try
                    {
                        await action();
                    }
                    catch (Exception ex)
                    {
                        if (onError is object)
                        {
                            onError(this, ex);
                        }
                        else
                        {
                            SetState(State.Error);
                            Toast().Error(ex.Message);
                        }
                        throw;
                    }

                    // The action set no state of its own: return to PendingSave, as UndoSpinner used to.
                    if (_stateVersion == version)
                    {
                        SetState(State.PendingSave);
                    }
                }).FireAndForget();
            });
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _button.Render();

        private static HTMLElement SmallSpinner(HTMLElement spinner) => Span(Att("tss-spinner-size-small"), spinner);
    }
}
