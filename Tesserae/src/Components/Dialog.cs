using System;
using System.Threading.Tasks;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A modal dialog with a title, body and configurable action buttons (OK / Cancel / custom).
    /// </summary>
    [Transpose.Name("tss.Dialog")]
    public sealed class Dialog
    {
        private static readonly Random RNG = new Random();

        private readonly Modal _modal;
        private readonly string _scope;
        private readonly bool _centerContent;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Dialog(IComponent content = null, IComponent title = null, bool centerContent = true)
        {
            _modal = Modal().HideCloseButton().NoLightDismiss().Blocking();

            if (centerContent)
            {
                _modal.CenterContent();

                if (title is TextBlock tb)
                    tb.TextCenter();
            }

            _centerContent = centerContent;

            _modal.SetHeader(title);
            _modal.Content = content;
            _modal.StylingContainer.classList.add("tss-dialog");

            _scope = $"dialog-{RNG.Next()}";

            _modal.OnShow(_ => Hotkeys.SetScope(_scope));
            _modal.OnHide(_ => Hotkeys.DeleteScope(_scope));
        }

        /// <summary>
        /// Gets or sets a value indicating whether the component can be dragged by the user.
        /// </summary>
        public bool IsDraggable
        {
            get => _modal.IsDraggable;
            set => _modal.IsDraggable = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the component uses the dark colour theme.
        /// </summary>
        public bool IsDark
        {
            get => _modal.IsDark;
            set => _modal.IsDark = value;
        }

        /// <summary>
        /// Gets or sets the title of the component.
        /// </summary>
        public Dialog Title(IComponent title)
        {
            _modal.SetHeader(title);
            return this;
        }

        /// <summary>
        /// Sets the content rendered inside the surface.
        /// </summary>
        public Dialog Content(IComponent content)
        {
            _modal.Content(content);
            return this;
        }

        /// <summary>
        /// Adds the given components to the dialog's command row.
        /// </summary>
        public Dialog Commands(params IComponent[] content)
        {
            content.Reverse();
            _modal.SetFooter(Stack().HorizontalReverse().Children(content));
            return this;
        }

        /// <summary>
        /// Applies the dark colour scheme to the component.
        /// </summary>
        public Dialog Dark()
        {
            IsDark = true;
            return this;
        }

        /// <summary>
        /// Gets or sets the CSS min-height of the component.
        /// </summary>
        public Dialog MinHeight(UnitSize unitSize)
        {
            _modal.MinHeight(unitSize);
            return this;
        }

        /// <summary>
        /// Gets or sets the CSS height of the component.
        /// </summary>
        public Dialog Height(UnitSize unitSize)
        {
            _modal.Height(unitSize);
            return this;
        }

        private static Stack GetButtonsStack()
        {
            return Stack().NoDefaultMargin().HorizontalReverse().JustifyContent(ItemJustify.Evenly).WS();
        }

        /// <summary>
        /// Shows OK button actions on the dialog and wires up the callback.
        /// </summary>
        public void Ok(Action onOk, Func<Button, Button> btnOk = null)
        {
            bool acted = false;

            _modal
               .LightDismiss()
               .SetFooter(GetButtonsStack().Children(
                    CreateButton("Ok", onOk, "Esc, Escape, Enter", modifier: btnOk, isPrimary: true, onActed: () => acted = true)
                ))
               .OnHide((_) =>
               {
                   if (!acted) { onOk(); }
               })
               .Show();
        }

        /// <summary>
        /// Shows OK / Cancel button actions on the dialog and wires up the callbacks.
        /// </summary>
        public void OkCancel(Action onOk = null, Action onCancel = null, Func<Button, Button> btnOk = null, Func<Button, Button> btnCancel = null)
        {
            bool acted = false;

            _modal
               .SetFooter(GetButtonsStack().Children(
                    CreateButton("Cancel", onCancel, "Esc, Escape", modifier: btnCancel, isPrimary: false, onActed: () => acted = true),
                    CreateButton("Ok", onOk, "Enter", modifier: btnOk, isPrimary: true, onActed: () => acted = true)
                ))
               .OnHide((_) =>
                {
                    if (!acted) { onCancel(); }
                })
               .Show();
        }

        /// <summary>
        /// Shows Yes / No button actions on the dialog and wires up the callbacks.
        /// </summary>
        public void YesNo(Action onYes = null, Action onNo = null, Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null)
        {
            bool acted = false;

            _modal
               .SetFooter(GetButtonsStack().Children(
                    CreateButton("No", onNo, "Esc, Escape", modifier: btnNo, isPrimary: false, onActed: () => acted = true),
                    CreateButton("Yes", onYes, "Enter", modifier: btnYes, isPrimary: true, onActed: () => acted = true)
                ))
               .OnHide((_) =>
                {
                    if (!acted) { onNo(); }
                })
               .Show();
        }

        /// <summary>
        /// Shows Yes / No / Cancel button actions on the dialog and wires up the callbacks.
        /// </summary>
        public void YesNoCancel(Action onYes = null, Action onNo = null, Action onCancel = null, Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null, Func<Button, Button> btnCancel = null)
        {
            bool acted = false;

            _modal
               .SetFooter(GetButtonsStack().Children(
                    CreateButton("Cancel", onCancel, "Esc, Escape", modifier: btnCancel, isPrimary: false, onActed: () => acted = true),
                    CreateButton("No", onNo, bindToKeys: null, modifier: btnNo, isPrimary: false, onActed: () => acted = true),
                    CreateButton("Yes", onYes, "Enter", modifier: btnYes, isPrimary: true, onActed: () => acted = true)
                ))
               .OnHide((_) =>
                {
                    if (!acted) { onCancel(); }
                })
               .Show();
        }

        /// <summary>
        /// Shows Retry / Cancel button actions on the dialog and wires up the callbacks.
        /// </summary>
        public void RetryCancel(Action onRetry = null, Action onCancel = null, Func<Button, Button> btnRetry = null, Func<Button, Button> btnCancel = null)
        {
            bool acted = false;

            _modal
               .SetFooter(GetButtonsStack().Children(
                    CreateButton("Cancel", onCancel, "Esc, Escape", modifier: btnCancel, isPrimary: false, onActed: () => acted = true),
                    CreateButton("Retry", onRetry, "Enter", modifier: btnRetry, isPrimary: true, onActed: () => acted = true)
                ))
               .OnHide((_) =>
                {
                    if (!acted) { onCancel(); }
                })
               .Show();
        }

        /// <summary>
        /// Shows the component.
        /// </summary>
        public void Show() => _modal.Show();

        /// <summary>
        /// Hides the component.
        /// </summary>
        public void Hide(Action onHidden = null) => _modal.Hide(onHidden);

        /// <summary>
        /// Makes the surface draggable.
        /// </summary>
        public Dialog Draggable()
        {
            IsDraggable = true;
            return this;
        }

        /// <summary>
        /// Awaitable variant of <see cref="Ok"/> that resolves with the user's response.
        /// </summary>
        public Task<Response> OkAsync(Func<Button, Button> btnOk = null)
        {
            var tcs = new TaskCompletionSource<Response>();
            Ok(() => tcs.SetResult(Response.Ok), btnOk);
            return tcs.Task;
        }

        /// <summary>
        /// Awaitable variant of <see cref="OkCancel"/> that resolves with the user's response.
        /// </summary>
        public Task<Response> OkCancelAsync(Func<Button, Button> btnOk = null, Func<Button, Button> btnCancel = null)
        {
            var tcs = new TaskCompletionSource<Response>();
            OkCancel(() => tcs.SetResult(Response.Ok), () => tcs.SetResult(Response.Cancel), btnOk, btnCancel);
            return tcs.Task;
        }

        /// <summary>
        /// Awaitable variant of <see cref="YesNo"/> that resolves with the user's response.
        /// </summary>
        public Task<Response> YesNoAsync(Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null)
        {
            var tcs = new TaskCompletionSource<Response>();
            YesNo(() => tcs.SetResult(Response.Yes), () => tcs.SetResult(Response.No), btnYes, btnNo);
            return tcs.Task;
        }

        /// <summary>
        /// Awaitable variant of <see cref="YesNoCancel"/> that resolves with the user's response.
        /// </summary>
        public Task<Response> YesNoCancelAsync(Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null, Func<Button, Button> btnCancel = null)
        {
            var tcs = new TaskCompletionSource<Response>();
            YesNoCancel(() => tcs.SetResult(Response.Yes), () => tcs.SetResult(Response.No), () => tcs.SetResult(Response.Cancel), btnYes, btnNo, btnCancel);
            return tcs.Task;
        }

        /// <summary>
        /// Awaitable variant of <see cref="RetryCancel"/> that resolves with the user's response.
        /// </summary>
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

        private Button CreateButton(string text, Action onClick, string bindToKeys, Func<Button, Button> modifier, bool isPrimary, Action onActed)
        {
            var button = Button(text)
               .AlignEnd()
               .OnClick((_, __) =>
                {
                    onActed();
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
                    onActed();
                    _modal.Hide();
                    onClick?.Invoke();
                });
            }

            return button;
        }
    }
}