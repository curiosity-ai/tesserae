using System;
using System.Threading.Tasks;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public sealed class Dialog
    {
        private static readonly Random RNG = new Random();

        private readonly Modal _modal;
        private readonly string _scope;
        public Dialog(IComponent content = null, IComponent title = null, bool centerContent = true)
        {
            _modal = Modal().HideCloseButton().NoLightDismiss().Blocking();

            if (centerContent)
            {
                _modal.CenterContent();
                if (title is TextBlock tb)
                    tb.TextCenter();
            }

            _modal.SetHeader(title);
            _modal.Content = content;
            _modal.StylingContainer.classList.add("tss-dialog");

            _scope = $"dialog-{RNG.Next()}";

            _modal.OnShow(_ => Hotkeys.SetScope(_scope));
            _modal.OnHide(_ => Hotkeys.DeleteScope(_scope));
        }

        public bool IsDraggable
        {
            get => _modal.IsDraggable;
            set => _modal.IsDraggable = value;
        }

        public bool IsDark
        {
            get => _modal.IsDark;
            set => _modal.IsDark = value;
        }

        public Dialog Title(IComponent title)
        {
            _modal.SetHeader(title);
            return this;
        }

        public Dialog Content(IComponent content)
        {
            _modal.Content(content);
            return this;
        }

        public Dialog Commands(params IComponent[] content)
        {
            content.Reverse();
            _modal.SetFooter(Stack().HorizontalReverse().Children(content));
            return this;
        }

        public Dialog Dark()
        {
            IsDark = true;
            return this;
        }

        public Dialog MinHeight(UnitSize unitSize) // 2020-06-30 DWR: This class does not implement IComponent and so the extension method for this functionality that operates against IComponent is not available here
        {
            _modal.MinHeight(unitSize);
            return this;
        }

        public Dialog Height(UnitSize unitSize) // 2020-06-30 DWR: This class does not implement IComponent and so the extension method for this functionality that operates against IComponent is not available here
        {
            _modal.Height(unitSize);
            return this;
        }

        public void Ok(Action onOk, Func<Button, Button> btnOk = null)
        {
            _modal
                .LightDismiss()
                .SetFooter(Stack().HorizontalReverse().Children(
                    CreateButton("Ok", onOk, "Esc, Escape, Enter", modifier: btnOk, isPrimary: true)
                ))
                .Show();
        }

        public void OkCancel(Action onOk = null, Action onCancel = null, Func<Button, Button> btnOk = null, Func<Button, Button> btnCancel = null)
        {
            _modal
                .SetFooter(Stack().HorizontalReverse().Children(
                    CreateButton("Cancel", onCancel, "Esc, Escape", modifier: btnCancel, isPrimary: false),
                    CreateButton("Ok", onOk, "Enter", modifier: btnOk, isPrimary: true)
                ))
                .Show();
        }

        public void YesNo(Action onYes = null, Action onNo = null, Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null)
        {
            _modal
                .SetFooter(Stack().HorizontalReverse().Children(
                    CreateButton("No", onNo, "Esc, Escape", modifier: btnNo, isPrimary: false),
                    CreateButton("Yes", onYes, "Enter", modifier: btnYes, isPrimary: true)
                ))
                .Show();
        }

        public void YesNoCancel(Action onYes = null, Action onNo = null, Action onCancel = null, Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null, Func<Button, Button> btnCancel = null)
        {
            _modal
                .SetFooter(Stack().HorizontalReverse().Children(
                    CreateButton("Cancel", onCancel, "Esc, Escape", modifier: btnCancel, isPrimary: false),
                    CreateButton("No", onNo, bindToKeys: null, modifier: btnNo, isPrimary: false),
                    CreateButton("Yes", onYes, "Enter", modifier: btnYes, isPrimary: true)
                ))
                .Show();
        }

        public void RetryCancel(Action onRetry = null, Action onCancel = null, Func<Button, Button> btnRetry = null, Func<Button, Button> btnCancel = null)
        {
            _modal
                .SetFooter(Stack().HorizontalReverse().Children(
                    CreateButton("Cancel", onCancel, "Esc, Escape", modifier: btnCancel, isPrimary: false),
                    CreateButton("Retry", onRetry, "Enter", modifier: btnRetry, isPrimary: true)
                ))
                .Show();
        }

        public void Show() => _modal.Show();

        public void Hide(Action onHidden = null) => _modal.Hide(onHidden);

        public Dialog Draggable()
        {
            IsDraggable = true;
            return this;
        }

        public Task<Response> OkAsync(Func<Button, Button> btnOk = null)
        {
            var tcs = new TaskCompletionSource<Response>();
            Ok(() => tcs.SetResult(Response.Ok), btnOk);
            return tcs.Task;
        }

        public Task<Response> OkCancelAsync(Func<Button, Button> btnOk = null, Func<Button, Button> btnCancel = null)
        {
            var tcs = new TaskCompletionSource<Response>();
            OkCancel(() => tcs.SetResult(Response.Ok), () => tcs.SetResult(Response.Cancel), btnOk, btnCancel);
            return tcs.Task;
        }

        public Task<Response> YesNoAsync(Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null)
        {
            var tcs = new TaskCompletionSource<Response>();
            YesNo(() => tcs.SetResult(Response.Yes), () => tcs.SetResult(Response.No), btnYes, btnNo);
            return tcs.Task;
        }

        public Task<Response> YesNoCancelAsync(Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null, Func<Button, Button> btnCancel = null)
        {
            var tcs = new TaskCompletionSource<Response>();
            YesNoCancel(() => tcs.SetResult(Response.Yes), () => tcs.SetResult(Response.No), () => tcs.SetResult(Response.Cancel), btnYes, btnNo, btnCancel);
            return tcs.Task;
        }

        public Task<Response> RetryCancelAsync(Func<Button, Button> btnRetry = null, Func<Button, Button> btnCancel = null)
        {
            var tcs = new TaskCompletionSource<Response>();
            RetryCancel(() => tcs.SetResult(Response.Retry), () => tcs.SetResult(Response.Cancel), btnRetry, btnCancel);
            return tcs.Task;
        }

        public enum Response
        {
            Yes,
            No,
            Cancel,
            Ok,
            Retry
        }

        private Button CreateButton(string text, Action onClick, string bindToKeys, Func<Button, Button> modifier, bool isPrimary)
        {
            var button = Button(text)
                .AlignEnd()
                .OnClick((_, __) =>
                {
                    _modal.Hide();
                    onClick?.Invoke();
                });

            if (isPrimary)
                button.Primary();

            if (modifier is object)
                button = modifier(button);

            if (!string.IsNullOrWhiteSpace(bindToKeys))
            {
                Hotkeys.Bind(bindToKeys, new Hotkeys.Option() { scope = _scope }, (e, _) =>
                {
                    StopEvent(e);
                    _modal.Hide();
                    onClick?.Invoke();
                });
            }
            
            return button;
        }
   }
}