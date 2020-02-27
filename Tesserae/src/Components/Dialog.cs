﻿using System;
using static Tesserae.UI;
using static Retyped.dom;
using System.Threading.Tasks;

namespace Tesserae.Components
{
    public class Dialog
    {
        private Modal _modal;

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

        public Dialog(IComponent content = null, IComponent title = null)
        {
            _modal = Modal().CenterContent().HideCloseButton().NoLightDismiss().Blocking();
            _modal.SetHeader(title);
            _modal.Content = content;
            _modal._modal.classList.add("tss-dialog");
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

        public void Ok(Action onOk, Func<Button, Button> btnOk = null)
        {
            btnOk = btnOk ?? ((b) => b);
            _modal.LightDismiss();
            _modal.SetFooter(Stack().HorizontalReverse()
                                 .Children(btnOk(Button("Ok").Primary()).AlignEnd().OnClick((s, e) => { _modal.Hide(); onOk?.Invoke(); })));
            _modal.Show();
        }
        public void OkCancel(Action onOk = null, Action OnCancel = null, Func<Button, Button> btnOk = null, Func<Button, Button> btnCancel = null)
        {
            btnOk = btnOk ?? ((b) => b);
            btnCancel = btnCancel ?? ((b) => b);
            _modal.SetFooter(Stack().HorizontalReverse()
                                 .Children(btnCancel(Button("Cancel")).AlignEnd().OnClick((s, e) => { _modal.Hide(); OnCancel?.Invoke(); }),
                                           btnOk(Button("Ok").Primary()).AlignEnd().OnClick((s, e) => { _modal.Hide(); onOk?.Invoke(); })));
            _modal.Show();
        }

        public void YesNo(Action onYes = null, Action onNo = null, Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null)
        {
            btnYes = btnYes ?? ((b) => b);
            btnNo = btnNo ?? ((b) => b);
            _modal.SetFooter(Stack().HorizontalReverse()
                                 .Children(btnNo(Button("No")).AlignEnd().OnClick((s, e) => { _modal.Hide(); onNo?.Invoke(); }),
                                           btnYes(Button("Yes").Primary()).AlignEnd().OnClick((s, e) => { _modal.Hide(); onYes?.Invoke(); })));
            _modal.Show();
        }

        public void YesNoCancel(Action onYes = null, Action onNo = null, Action onCancel = null, Func<Button, Button> btnYes = null, Func<Button, Button> btnNo = null, Func<Button, Button> btnCancel = null)
        {
            btnYes = btnYes ?? ((b) => b);
            btnNo = btnNo ?? ((b) => b);
            btnCancel = btnCancel ?? ((b) => b);
            _modal.SetFooter(Stack().HorizontalReverse()
                                 .Children(btnCancel(Button("Cancel")).AlignEnd().OnClick((s, e) => { _modal.Hide(); onCancel?.Invoke(); }),
                                           btnNo(Button("No")).AlignEnd().OnClick((s, e) => { _modal.Hide(); onNo?.Invoke(); }),
                                           btnYes(Button("Yes").Primary()).AlignEnd().OnClick((s, e) => { _modal.Hide(); onYes?.Invoke(); })));
            _modal.Show();
        }

        public void RetryCancel(Action onRetry = null, Action onCancel = null, Func<Button, Button> btnRetry = null, Func<Button, Button> btnCancel = null)
        {
            btnRetry = btnRetry ?? ((b) => b);
            btnCancel = btnCancel ?? ((b) => b);
            _modal.SetFooter(Stack().HorizontalReverse()
                                 .Children(btnCancel(Button("Cancel")).AlignEnd().OnClick((s, e) => { _modal.Hide(); onCancel?.Invoke(); }),
                                           btnRetry(Button("Retry").Primary()).AlignEnd().OnClick((s, e) => { _modal.Hide(); onRetry?.Invoke(); })));
            _modal.Show();
        }

        public void Show()
        {
            _modal.Show();
        }

        public void Hide(Action onHidden = null)
        {
            _modal.Hide(onHidden);
        }

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
            Retry,
        }
    }
}
