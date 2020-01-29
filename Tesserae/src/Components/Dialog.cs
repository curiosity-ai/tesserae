using System;
using static Tesserae.UI;
using static Retyped.dom;
using System.Threading.Tasks;

namespace Tesserae.Components
{
    public class Dialog : Modal
    {
        private IComponent _Footer;
        private HTMLElement _ModalFooter;

        public IComponent Footer
        {
            get { return _Footer; }
            set
            {
                if (value != _Footer)
                {
                    ClearChildren(_ModalFooter); ;
                    _Footer = value;
                    if (_Footer != null)
                    {
                        _ModalFooter.appendChild(_Footer.Render());
                    }
                }
            }
        }

        public Dialog(string header = string.Empty) : base(header)
        {
            _Modal.classList.add("tss-dialog");
            _ContentHtml.classList.add("tss-dialog-container");
            _ModalFooter = Div(_("tss-modal-footer"));
            _Modal.appendChild(_ModalFooter);

            // As recommended
            CanLightDismiss = true;
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

    public static class DialogExtensions
    {
        public static Dialog Footer(this Dialog dialog, IComponent footer)
        {
            dialog.Footer = footer;
            return dialog;
        }

        public static void Ok(this Dialog dialog, Action onOk, Func<Button, Button> btnOk = null)
        {
            btnOk = btnOk ?? ((b) => b);
            dialog.HideCloseButton()
                  .Footer(Stack().HorizontalReverse()
                                 .Children(btnOk(Button("Ok").Primary()).AlignEnd().OnClick((s, e) => { dialog.Hide(); onOk?.Invoke(); })));
            dialog.Show();
        }

        public static void YesNo(this Dialog dialog, Action onYes, Action onNo, Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null)
        {
            btnYes = btnYes ?? ((b) => b);
            btnNo  = btnNo  ?? ((b) => b);
            dialog.HideCloseButton()
                  .Footer(Stack().HorizontalReverse()
                                 .Children(btnNo(Button("No")).AlignEnd().OnClick((s, e) => { dialog.Hide(); onNo?.Invoke(); }),
                                           btnYes(Button("Yes").Primary()).AlignEnd().OnClick((s, e) => { dialog.Hide(); onYes?.Invoke(); })));
            dialog.Show();
        }

        public static void YesNoCancel(this Dialog dialog, Action onYes, Action onNo, Action onCancel, Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null, Func<Button, Button> btnCancel = null)
        {
            btnYes     = btnYes ?? ((b) => b);
            btnNo      = btnNo  ?? ((b) => b);
            btnCancel  = btnCancel ?? ((b) => b);
            dialog.HideCloseButton()
                  .Footer(Stack().HorizontalReverse()
                                 .Children(btnCancel(Button("Cancel")).AlignEnd().OnClick((s, e) => { dialog.Hide(); onCancel?.Invoke(); }),
                                           btnNo(Button("No")).AlignEnd().OnClick((s, e) => { dialog.Hide(); onNo?.Invoke(); }),
                                           btnYes(Button("Yes").Primary()).AlignEnd().OnClick((s, e) => { dialog.Hide(); onYes?.Invoke(); })));
            dialog.Show();
        }

        public static void RetryCancel(this Dialog dialog, Action onRetry, Action onCancel, Func<Button, Button> btnRetry = null, Func<Button, Button> btnCancel = null)
        {
            btnRetry  = btnRetry  ?? ((b) => b);
            btnCancel = btnCancel ?? ((b) => b);
            dialog.HideCloseButton()
                  .Footer(Stack().HorizontalReverse() 
                                 .Children(btnCancel(Button("Cancel")).AlignEnd().OnClick((s, e) => { dialog.Hide(); onCancel?.Invoke(); }),
                                           btnRetry(Button("Retry").Primary()).AlignEnd().OnClick((s, e) => { dialog.Hide(); onRetry?.Invoke(); })));
            dialog.Show();
        }

        public static Task<Dialog.Response> OkAsync(this Dialog dialog, Func<Button, Button> btnOk = null)
        {
            var tcs = new TaskCompletionSource<Dialog.Response>();
            dialog.Ok(() => tcs.SetResult(Dialog.Response.Ok), btnOk);
            return tcs.Task;
        }

        public static Task<Dialog.Response> YesNoAsync(this Dialog dialog, Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null)
        {
            var tcs = new TaskCompletionSource<Dialog.Response>();
            dialog.YesNo(() => tcs.SetResult(Dialog.Response.Yes), () => tcs.SetResult(Dialog.Response.No), btnYes, btnNo);
            return tcs.Task;
        }

        public static Task<Dialog.Response> YesNoCancelAsync(this Dialog dialog, Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null, Func<Button, Button> btnCancel = null)
        {
            var tcs = new TaskCompletionSource<Dialog.Response>();
            dialog.YesNoCancel(() => tcs.SetResult(Dialog.Response.Yes), () => tcs.SetResult(Dialog.Response.No), () => tcs.SetResult(Dialog.Response.Cancel), btnYes, btnNo, btnCancel);
            return tcs.Task;
        }

        public static Task<Dialog.Response> RetryCancelAsync(this Dialog dialog, Func<Button, Button> btnRetry = null, Func<Button, Button> btnCancel = null)
        {
            var tcs = new TaskCompletionSource<Dialog.Response>();
            dialog.RetryCancel(() => tcs.SetResult(Dialog.Response.Retry), () => tcs.SetResult(Dialog.Response.Cancel), btnRetry, btnCancel);
            return tcs.Task;
        }
    }
}
