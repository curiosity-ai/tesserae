using System;
using static Tesserae.UI;

namespace Tesserae
{

    /// <summary>
    /// A modal that shows a progress bar while a long-running operation completes, with optional cancel button.
    /// </summary>
    [Transpose.Name("tss.ProgressModal")]
    public class ProgressModal
    {
        private readonly Modal             _modalHost;
        private readonly Raw               _titleHost;
        private readonly Raw               _messageHost;
        private readonly Raw               _progressHost;
        private readonly Raw               _footerHost;
        private readonly ProgressIndicator _progressIndicator;
        private readonly Spinner           _spinner;
        private          bool              _isSpinner = true;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ProgressModal()
        {
            _titleHost    = Raw().WS();
            _messageHost  = Raw().WS();
            _footerHost   = Raw().WS();
            _progressHost = Raw();
            _spinner      = Spinner().Large().Margin(8.px());
            _progressHost.Content(_spinner);
            _progressIndicator = ProgressIndicator();
            _isSpinner         = true;

            _modalHost = Modal().Blocking().NoLightDismiss().HideCloseButton().CenterContent()
               .Content(Stack()
                   .AlignCenter()
                   .WS()
                   .Children(_titleHost, _progressHost, _messageHost, _footerHost));

        }

        /// <summary>
        /// Shows the component.
        /// </summary>
        public ProgressModal Show()
        {
            _modalHost.Show();
            return this;
        }


        /// <summary>
        /// Shows the embedded.
        /// </summary>
        public IComponent ShowEmbedded()
        {
            return _modalHost.ShowEmbedded();
        }

        /// <summary>
        /// Hides the component.
        /// </summary>
        public ProgressModal Hide()
        {
            _modalHost.Hide();
            return this;
        }

        /// <summary>
        /// Configures the component to message.
        /// </summary>
        public ProgressModal Message(string message)
        {
            _messageHost.Content(TextBlock(message));
            return this;
        }

        /// <summary>
        /// Configures the component to message.
        /// </summary>
        public ProgressModal Message(IComponent message)
        {
            _messageHost.Content(message);
            return this;
        }

        /// <summary>
        /// Gets or sets the title of the component.
        /// </summary>
        public ProgressModal Title(string title)
        {
            _titleHost.Content(TextBlock(title).SemiBold().Primary().PaddingTop(16.px()).PaddingBottom(8.px()));
            return this;
        }

        /// <summary>
        /// Gets or sets the title of the component.
        /// </summary>
        public ProgressModal Title(IComponent title)
        {
            _titleHost.Content(title);
            return this;
        }

        /// <summary>
        /// Configures the component to progress.
        /// </summary>
        public ProgressModal Progress(float percent)
        {
            if (_isSpinner)
            {
                _progressHost.Content(_progressIndicator);
                _isSpinner = false;
            }
            _progressIndicator.Progress(percent);
            return this;
        }

        /// <summary>
        /// Configures the component to progress.
        /// </summary>
        public ProgressModal Progress(int position, int total) => Progress(100f * position / total);

        /// <summary>
        /// Configures the progress indeterminated on the component.
        /// </summary>
        public ProgressModal ProgressIndeterminated()
        {
            if (_isSpinner)
            {
                _progressHost.Content(_progressIndicator);
                _isSpinner = false;
            }
            _progressIndicator.Indeterminated();
            return this;
        }

        /// <summary>
        /// Configures the progress spin on the component.
        /// </summary>
        public ProgressModal ProgressSpin()
        {
            if (!_isSpinner)
            {
                _progressHost.Content(_spinner);
                _isSpinner = true;
            }
            return this;
        }

        /// <summary>
        /// Returns the component configured with the given cancel.
        /// </summary>
        public ProgressModal WithCancel(Action<Button> onCancel, Action<Button> btnCancel = null)
        {
            var button = Button().SetText("Cancel").SetIcon(UIcons.Cross).Danger();
            btnCancel?.Invoke(button);
            button.OnClick((b, __) => onCancel(b));
            _footerHost.PaddingTop(16.px()).Content(button.AlignCenter());
            return this;
        }
    }
}