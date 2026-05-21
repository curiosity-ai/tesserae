using H5;
using static H5.Core.dom;
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
        private bool _hovering;
        private bool _pendingPrimary = true;

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
            // Reset base styles
            _button.IsPrimary = false;
            _button.IsSuccess = false;
            _button.IsDanger = false;
            _button.IsEnabled = state != State.NothingToSave; // Default to enabled
            _button.RemoveTooltip();

            switch (state)
            {
                case State.NothingToSave:
                case State.PendingSave:
                    _button.IsPrimary = _pendingPrimary;
                    if (_hovering)
                    {
                        _button.SetText(_textSaveHover).SetIcon(_iconSaveHover);
                    }
                    else
                    {
                        _button.SetIcon(_iconSave).SetText(_textSave);
                    }
                    break;
                case State.Verifying:
                    _button.IsPrimary = true;
                    _button.IsEnabled = false;
                    _button.ToSpinner(message ?? _textVerifying);
                    break;
                case State.Saving:
                    _button.IsSuccess = true;
                    _button.IsEnabled = false;
                    _button.ToSpinner(message ?? _textSaving);
                    break;
                case State.Saved:
                    _button.IsSuccess = true;
                    _button.SetIcon(UIcons.Check).SetText(_textSaved);
                    break;
                case State.Error:
                    _button.IsDanger = true;
                    _button.SetIcon(UIcons.OctagonXmark).SetText(_textError);
                    if (!string.IsNullOrEmpty(message))
                    {
                        _button.Tooltip(message);
                    }
                    break;
            }

            _button.MinWidth(100.px());
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
        public SaveButton OnClickSpinWhile(Func<Task> actionAsync)
        {
            _button.OnClickSpinWhile(async () =>
            {
                if (_state != State.PendingSave) return;
                await actionAsync();
            });
            return this;
        }

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
            Action<Button, Exception> onErrorInner;
            if (onError is object)
            {
                onErrorInner = (Button b, Exception e) => onError(this, e);
            }
            else
            {
                onErrorInner = (Button b, Exception e) =>
                {
                    this.SetState(State.Error);
                    Toast().Error(e.Message);
                    throw e;
                };
            }
            _button.OnClickSpinWhile(async () =>
            {
                if (_state != State.PendingSave) return;

                await action();
            }, text, onErrorInner);
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _button.Render();
    }
}
