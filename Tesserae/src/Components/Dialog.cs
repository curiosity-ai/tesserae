using System;
using static Tesserae.UI;
using static Retyped.dom;
using System.Threading.Tasks;

namespace Tesserae.Components
{
    public class Dialog : Modal
    {
        private IComponent _footer;
        private readonly HTMLElement _modalFooter;

        public Dialog(IComponent header = null) : base(header)
        {
            _modal.classList.add("tss-dialog");
            _contentHtml.classList.add("tss-dialog-container");
            _modalFooter = Div(_("tss-modal-footer"));
            _modal.appendChild(_modalFooter);

            // As recommended
            CanLightDismiss = true;
        }

        public IComponent Footer
        {
            get { return _footer; }
            set
            {
                if (value != _footer)
                {
                    ClearChildren(_modalFooter); ;
                    _footer = value;
                    if (_footer != null)
                    {
                        _modalFooter.appendChild(_footer.Render());
                    }
                }
            }
        }

        public Dialog SetFooter(IComponent footer)
        {
            Footer = footer;
            return this;
        }

        public void Ok(Action onOk, Func<Button, Button> btnOk = null)
        {
            btnOk = btnOk ?? ((b) => b);
            this.HideCloseButton()
                  .SetFooter(Stack().HorizontalReverse()
                                 .Children(btnOk(Button("Ok").Primary()).AlignEnd().OnClick((s, e) => { Hide(); onOk?.Invoke(); })));
            Show();
        }

        public void YesNo(Action onYes, Action onNo, Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null)
        {
            btnYes = btnYes ?? ((b) => b);
            btnNo = btnNo ?? ((b) => b);
            this.HideCloseButton()
                  .SetFooter(Stack().HorizontalReverse()
                                 .Children(btnNo(Button("No")).AlignEnd().OnClick((s, e) => { Hide(); onNo?.Invoke(); }),
                                           btnYes(Button("Yes").Primary()).AlignEnd().OnClick((s, e) => { Hide(); onYes?.Invoke(); })));
            Show();
        }

        public void YesNoCancel(Action onYes, Action onNo, Action onCancel, Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null, Func<Button, Button> btnCancel = null)
        {
            btnYes = btnYes ?? ((b) => b);
            btnNo = btnNo ?? ((b) => b);
            btnCancel = btnCancel ?? ((b) => b);
            this.HideCloseButton()
                  .SetFooter(Stack().HorizontalReverse()
                                 .Children(btnCancel(Button("Cancel")).AlignEnd().OnClick((s, e) => { Hide(); onCancel?.Invoke(); }),
                                           btnNo(Button("No")).AlignEnd().OnClick((s, e) => { Hide(); onNo?.Invoke(); }),
                                           btnYes(Button("Yes").Primary()).AlignEnd().OnClick((s, e) => { Hide(); onYes?.Invoke(); })));
            Show();
        }

        public void RetryCancel(Action onRetry, Action onCancel, Func<Button, Button> btnRetry = null, Func<Button, Button> btnCancel = null)
        {
            btnRetry = btnRetry ?? ((b) => b);
            btnCancel = btnCancel ?? ((b) => b);
            this.HideCloseButton()
                  .SetFooter(Stack().HorizontalReverse()
                                 .Children(btnCancel(Button("Cancel")).AlignEnd().OnClick((s, e) => { Hide(); onCancel?.Invoke(); }),
                                           btnRetry(Button("Retry").Primary()).AlignEnd().OnClick((s, e) => { Hide(); onRetry?.Invoke(); })));
            Show();
        }

        public Task<Dialog.Response> OkAsync(Func<Button, Button> btnOk = null)
        {
            var tcs = new TaskCompletionSource<Dialog.Response>();
            Ok(() => tcs.SetResult(Dialog.Response.Ok), btnOk);
            return tcs.Task;
        }

        public Task<Dialog.Response> YesNoAsync(Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null)
        {
            var tcs = new TaskCompletionSource<Dialog.Response>();
            YesNo(() => tcs.SetResult(Dialog.Response.Yes), () => tcs.SetResult(Dialog.Response.No), btnYes, btnNo);
            return tcs.Task;
        }

        public Task<Dialog.Response> YesNoCancelAsync(Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null, Func<Button, Button> btnCancel = null)
        {
            var tcs = new TaskCompletionSource<Dialog.Response>();
            YesNoCancel(() => tcs.SetResult(Dialog.Response.Yes), () => tcs.SetResult(Dialog.Response.No), () => tcs.SetResult(Dialog.Response.Cancel), btnYes, btnNo, btnCancel);
            return tcs.Task;
        }

        public Task<Dialog.Response> RetryCancelAsync(Func<Button, Button> btnRetry = null, Func<Button, Button> btnCancel = null)
        {
            var tcs = new TaskCompletionSource<Dialog.Response>();
            RetryCancel(() => tcs.SetResult(Dialog.Response.Retry), () => tcs.SetResult(Dialog.Response.Cancel), btnRetry, btnCancel);
            return tcs.Task;
        }

        public enum Response
        {
            Yes,
            No,
            Cancel,
            Ok,
            Retry,
        }
    }
}
