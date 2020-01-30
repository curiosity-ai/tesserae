using static Tesserae.UI;

namespace Tesserae.Components
{

    public class ProgressModal
    {
        private Modal _modalHost;
        private Raw _titleHost;
        private Raw _messageHost;
        private Raw _progressHost;
        private ProgressIndicator _progressIndicator;
        private Spinner _spinner;
        private bool _isSpinner = true;

        public ProgressModal()
        {
            _titleHost   = Raw();
            _messageHost = Raw();
            _progressHost = Raw();
            _spinner = Spinner().Large().Margin(8);
            _progressHost.Content(_spinner);
            _progressIndicator = ProgressIndicator();
            _isSpinner = true;
            _modalHost = Modal().Blocking().NoLightDismiss().HideCloseButton().CenterContent()
                                .Content(Stack().Vertical()
                                               .AlignCenter()
                                               .WidthStretch()
                                               .Children(_titleHost, _progressHost, _messageHost));
            
        }

        public void Show()
        {
            _modalHost.Show();
        }

        public void Hide()
        {
            _modalHost.Hide();
        }

        public ProgressModal Message(string message)
        {
            _messageHost.Content(TextBlock(message));
            return this;
        }

        public ProgressModal Message(IComponent message)
        {
            _messageHost.Content(message);
            return this;
        }

        public ProgressModal Title(string title)
        {
            _titleHost.Content(TextBlock(title));
            return this;
        }

        public ProgressModal Title(IComponent title)
        {
            _titleHost.Content(title);
            return this;
        }

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

        public ProgressModal ProgressSpin()
        {
            if (!_isSpinner)
            {
                _progressHost.Content(_spinner);
                _isSpinner = true;
            }
            return this;
        }
    }
}
