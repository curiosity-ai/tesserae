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

        // One face per state, all stacked in the same grid cell (see tss.button.css): the button is as wide
        // as the widest of them, so it keeps one width across every state.
        private readonly Face _faceSave      = Face.WithIcon();
        private readonly Face _faceSaveHover = Face.WithIcon();
        private readonly Face _faceVerifying = Face.WithSpinner(success: false);
        private readonly Face _faceSaving    = Face.WithSpinner(success: true);
        private readonly Face _faceSaved     = Face.WithIcon();
        private readonly Face _faceError     = Face.WithIcon();

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
            element.classList.add("tss-savebtn");
            element.appendChild(Span(Att("tss-savebtn-faces"),
                _faceSave.Element, _faceSaveHover.Element, _faceVerifying.Element, _faceSaving.Element, _faceSaved.Element, _faceError.Element));
            _faceSaved.SetIcon(UIcons.Check);
            _faceError.SetIcon(UIcons.OctagonXmark);
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
            _button.UndoSpinner();
            _state = state;
            _stateVersion++;
            // Reset base styles
            _button.IsPrimary = false;
            _button.IsSuccess = false;
            _button.IsDanger = false;
            _button.IsEnabled = state != State.NothingToSave; // Default to enabled
            _button.RemoveTooltip();

            _faceSave.SetIcon(_iconSave).SetText(_textSave);
            _faceSaveHover.SetIcon(_iconSaveHover).SetText(_textSaveHover);
            _faceSaveHover.Element.style.display = string.IsNullOrEmpty(_textSaveHover) ? "none" : "";
            _faceVerifying.SetText(state == State.Verifying ? message ?? _textVerifying : _textVerifying);
            _faceSaving.SetText(state == State.Saving ? message ?? _textSaving : _textSaving);
            _faceSaved.SetText(_textSaved);
            _faceError.SetText(_textError);

            Face active = null;

            switch (state)
            {
                case State.NothingToSave:
                case State.PendingSave:
                    _button.IsPrimary = _pendingPrimary;
                    active = _hovering ? _faceSaveHover : _faceSave;
                    break;
                case State.Verifying:
                    _button.IsPrimary = true;
                    _button.IsEnabled = false;
                    active = _faceVerifying;
                    break;
                case State.Saving:
                    _button.IsSuccess = true;
                    _button.IsEnabled = false;
                    active = _faceSaving;
                    break;
                case State.Saved:
                    _button.IsSuccess = true;
                    active = _faceSaved;
                    break;
                case State.Error:
                    _button.IsDanger = true;
                    active = _faceError;
                    if (!string.IsNullOrEmpty(message))
                    {
                        _button.Tooltip(message);
                    }
                    break;
            }

            foreach (var face in new[] { _faceSave, _faceSaveHover, _faceVerifying, _faceSaving, _faceSaved, _faceError })
            {
                face.Element.UpdateClassIf(face == active, "tss-savebtn-face-active");
            }

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
            // Spins with the button's own Saving face rather than Button.ToSpinner, which swaps in a clone of the
            // button drawn differently (a larger spinner, no label) from the Verifying and Saving states.
            _button.OnClick(() =>
            {
                if (_state != State.PendingSave) return;

                Task.Run(async () =>
                {
                    SetState(State.Saving, text);
                    var version = _stateVersion;

                    try
                    {
                        await action();
                    }
                    catch (Exception e)
                    {
                        if (onError is object)
                        {
                            onError(this, e);
                        }
                        else
                        {
                            SetState(State.Error);
                            Toast().Error(e.Message);
                        }
                        return;
                    }

                    // The action moved the button on itself (Verifying, Saved, ...): leave it there. Otherwise
                    // go back to where the click found it, which is what restoring the spun-out button used to do.
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

        private sealed class Face
        {
            private readonly HTMLElement _label;
            private readonly HTMLElement _icon;

            private Face(HTMLElement leading)
            {
                _icon   = leading.tagName == "I" ? leading : null;
                _label  = Span(Att());
                Element = Span(Att("tss-savebtn-face"), leading, _label);
            }

            public HTMLElement Element { get; }

            public static Face WithIcon() => new Face(I(Att()));

            public static Face WithSpinner(bool success) => new Face(Span(Att("tss-spinner-size-small"), Div(Att(success ? "tss-spinner tss-spinner-success" : "tss-spinner"))));

            public Face SetIcon(UIcons icon)
            {
                _icon.className = $"{Tesserae.Icon.Transform(icon, UIconsWeight.Regular)} {TextSize.Small}";
                return this;
            }

            public Face SetText(string text)
            {
                _label.innerText = text ?? "";
                return this;
            }
        }
    }
}
