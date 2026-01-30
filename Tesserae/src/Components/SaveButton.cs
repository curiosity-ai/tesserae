using H5;
using static H5.Core.dom;
using static Tesserae.UI;
using System;

namespace Tesserae
{
    [Name("tss.SaveButton")]
    public class SaveButton : IComponent
    {
        private Button _button;
        private string _textSavePending = "Save";
        private string _textVerifying = "Verifying...";
        private string _textSaving = "Saving...";
        private string _textSaved = "Saved";
        private string _textError = "Error";

        public enum SaveState
        {
            SavePending,
            Verifying,
            Saving,
            Saved,
            Error
        }

        public SaveButton()
        {
            _button = Button();
            SetState(SaveState.SavePending);
        }

        public SaveButton WithStateTexts(string savePending = null, string verifying = null, string saving = null, string saved = null, string error = null)
        {
            if (savePending != null) _textSavePending = savePending;
            if (verifying != null) _textVerifying = verifying;
            if (saving != null) _textSaving = saving;
            if (saved != null) _textSaved = saved;
            if (error != null) _textError = error;
            return this;
        }

        public void SetState(SaveState state, string message = null)
        {
            _button.UndoSpinner();

            // Reset base styles
            _button.IsPrimary = false;
            _button.IsSuccess = false;
            _button.IsDanger = false;
            _button.IsEnabled = true; // Default to enabled
            _button.RemoveTooltip();

            switch (state)
            {
                case SaveState.SavePending:
                    _button.SetIcon(UIcons.Disk).SetText(_textSavePending);
                    _button.IsPrimary = true;
                    break;
                case SaveState.Verifying:
                    _button.ToSpinner(message ?? _textVerifying);
                    _button.IsEnabled = false;
                    break;
                case SaveState.Saving:
                    _button.SetIcon(UIcons.Refresh).SetText(message ?? _textSaving);
                    _button.IsEnabled = false;
                    break;
                case SaveState.Saved:
                    _button.SetIcon(UIcons.Check).SetText(_textSaved);
                    _button.IsSuccess = true;
                    break;
                case SaveState.Error:
                    _button.SetIcon(UIcons.Exclamation).SetText(_textError);
                    _button.IsDanger = true;
                    if (!string.IsNullOrEmpty(message))
                    {
                        _button.Tooltip(message);
                    }
                    break;
            }
        }

        public SaveButton OnClick(Action action)
        {
            _button.OnClick(action);
            return this;
        }

        public HTMLElement Render() => _button.Render();
    }
}
