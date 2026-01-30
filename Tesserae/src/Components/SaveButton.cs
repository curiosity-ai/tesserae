using H5;
using static H5.Core.dom;
using static Tesserae.UI;
using System;
using System.Threading.Tasks;

namespace Tesserae
{
    [Name("tss.SaveButton")]
    public class SaveButton : IComponent
    {
        private Button _button;
        private string _textSave = "Save";
        private string _textVerifying = "Verifying...";
        private string _textSaving = "Saving...";
        private string _textSaved = "Saved";
        private string _textError = "Error";
        private State _state;

        public enum State
        {
            NothingToSave,
            PendingSave,
            Verifying,
            Saving,
            Saved,
            Error,
        }

        public SaveButton()
        {
            _button = Button().MinWidth(100.px());
            SetState(State.NothingToSave);
        }

        public SaveButton WithStateTexts(string save = null, string verifying = null, string saving = null, string saved = null, string error = null)
        {
            if (save != null) _textSave = save;
            if (verifying != null) _textVerifying = verifying;
            if (saving != null) _textSaving = saving;
            if (saved != null) _textSaved = saved;
            if (error != null) _textError = error;
            return this;
        }

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
                    _button.IsPrimary = true;
                    _button.SetIcon(UIcons.Disk).SetText(_textSave);
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

        
        public SaveButton NothingToSave(string message = null) => SetState(State.NothingToSave, message);
        public SaveButton Pending(string message = null) => SetState(State.PendingSave, message);
        public SaveButton Verifying(string message = null) => SetState(State.Verifying, message);
        public SaveButton Saving(string message = null) => SetState(State.Saving, message);
        public SaveButton Saved(string message = null) => SetState(State.Saved, message);
        public SaveButton Error(string message = null) => SetState(State.Error, message);


        public SaveButton OnClick(Action action)
        {
            if (_state != State.PendingSave) return this;
            _button.OnClick(action);
            return this;
        }

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

        public HTMLElement Render() => _button.Render();
    }
}
