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

        public void SetState(SaveState state, string message = null)
        {
            // Reset base styles
            _button.IsPrimary = false;
            _button.IsSuccess = false;
            _button.IsDanger = false;
            _button.IsEnabled = true; // Default to enabled
            _button.RemoveTooltip();

            switch (state)
            {
                case SaveState.SavePending:
                    _button.SetIcon(UIcons.Disk).SetText("Save");
                    _button.IsPrimary = true;
                    break;
                case SaveState.Verifying:
                    _button.SetIcon(UIcons.Eye).SetText("Verifying...");
                    _button.IsEnabled = false;
                    break;
                case SaveState.Saving:
                    _button.SetIcon(UIcons.Refresh).SetText("Saving...");
                    _button.IsEnabled = false;
                    break;
                case SaveState.Saved:
                    _button.SetIcon(UIcons.Check).SetText("Saved");
                    _button.IsSuccess = true;
                    break;
                case SaveState.Error:
                    _button.SetIcon(UIcons.Exclamation).SetText("Error");
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
