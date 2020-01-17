using System;
using System.Text.RegularExpressions;
using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    class TranslationPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        static Regex regex = new Regex("translate\\(([-0-9.].*?)px,([-0-9.].*?)px\\)");
        static Regex regexShort = new Regex("translate\\(([-0-9.].*?)px\\)");

        public TranslationPoint(double x = 0, double y = 0)
        {
            X = x;
            Y = y;
        }

        public static TranslationPoint From(string translation)
        {
            try
            {
                var m = regex.Match(translation);
                return new TranslationPoint(double.Parse(m.Groups[1].Value), double.Parse(m.Groups[2].Value));
            }
            catch
            {
                var m = regexShort.Match(translation);
                return new TranslationPoint(double.Parse(m.Groups[1].Value), double.Parse(m.Groups[1].Value));
            }
        }

        public string To()
        {
            return $"translate({X}px,{Y}px)";
        }
    }

    public class Modal : Layer
    {
        protected HTMLElement _Modal;
        private HTMLElement _CloseButton;
        private HTMLElement _ModalHeader;
        private HTMLElement _ModalOverlay;
        private HTMLElement _ModalContent;
        private HTMLElement _ModalCommand;

        private bool isDragged;
        private TranslationPoint startPoint;


        #region Properties

        public string Header
        {
            get { return _ModalHeader.innerText; }
            set { _ModalHeader.innerText = value; }
        }

        public override IComponent Content
        {
            get { return _Content; }
            set
            {
                if (value != _Content)
                {
                    ClearChildren(_ModalContent); ;
                    _Content = value;
                    if (_Content != null)
                    {
                        _ModalContent.appendChild(_Content.Render());
                    }
                }
            }
        }

        public bool CanLightDismiss
        {
            get { return _ModalOverlay.classList.contains("tss-modal-lightDismiss"); }
            set
            {
                if (value != CanLightDismiss)
                {
                    if (value)
                    {
                        _ModalOverlay.classList.add("tss-modal-lightDismiss");
                        _ModalOverlay.addEventListener("click", OnCloseClick);
                    }
                    else
                    {
                        _ModalOverlay.classList.remove("tss-modal-lightDismiss");
                        _ModalOverlay.removeEventListener("click", OnCloseClick);
                    }
                }
            }
        }

        public bool Dark
        {
            get { return _ContentHtml.classList.contains("dark"); }
            set
            {
                if (value != Dark)
                {
                    if (value)
                    {
                        _ContentHtml.classList.add("dark");
                    }
                    else
                    {
                        _ContentHtml.classList.remove("dark");
                    }
                }
            }
        }

        public bool IsDraggable
        {
            get { return _Modal.classList.contains("tss-modal-draggable"); }
            set
            {
                if (value != IsDraggable)
                {
                    if (value)
                    {
                        _Modal.classList.add("tss-modal-draggable");
                        _Modal.addEventListener("mousedown", OnDragMouseDown);

                    }
                    else
                    {
                        _Modal.classList.remove("tss-modal-draggable");
                        _Modal.removeEventListener("mousedown", OnDragMouseDown);
                    }
                }
            }
        }

        public bool IsNonBlocking
        {
            get { return _ContentHtml.classList.contains("tss-modal-modeless"); }
            set
            {
                if (value != IsNonBlocking)
                {
                    if (value)
                    {
                        _ContentHtml.classList.add("tss-modal-modeless");
                        if (IsVisible) document.body.style.overflowY = "";
                    }
                    else
                    {
                        _ContentHtml.classList.remove("tss-modal-modeless");
                        if (IsVisible) document.body.style.overflowY = "hidden";
                    }
                }
            }
        }

        public bool ShowCloseButton
        {
            get { return _CloseButton.style.display != "none"; }
            set
            {
                if (value) _CloseButton.style.display = "";
                else _CloseButton.style.display = "none";
            }

        }

        #endregion

        public Modal(string header = string.Empty)
        {
            _ModalHeader = Div(_("tss-modal-header", text: header));
            _CloseButton = Button(_("fal fa-times", el: el => el.onclick = (e) => Hide()));
            _ModalCommand = Div(_("tss-modal-command"), _ModalHeader, _CloseButton);
            _ModalContent = Div(_("tss-modal-content"));
            _Modal = Div(_("tss-modal", styles: s => s.transform = "translate(0px,0px)"), _ModalCommand, _ModalContent);
            _ModalOverlay = Div(_("tss-modal-overlay"));
            _ContentHtml = Div(_("tss-modal-container"), _ModalOverlay, _Modal);
        }
        protected override HTMLElement BuildRenderedContent()
        {
            return _ContentHtml;
        }

        public override void Show()
        {
            if (!IsNonBlocking) document.body.style.overflowY = "hidden";
            base.Show();
        }
        public override void Hide()
        {
            base.Hide();
            if (!IsNonBlocking) document.body.style.overflowY = "";
        }

        private void OnCloseClick(object ev)
        {
            Hide();
        }

        private void OnDragMouseMove(object ev)
        {
            if (isDragged)
            {
                var e = ev as MouseEvent;
                startPoint.X += e.movementX;
                startPoint.Y += e.movementY;
                _Modal.style.transform = startPoint.To();
            }
        }

        private void OnDragMouseUp(object ev)
        {
            var e = ev as MouseEvent;
            if (isDragged && e.button == 0)
            {
                document.body.removeEventListener("mouseup", OnDragMouseUp);
                document.body.removeEventListener("mousemove", OnDragMouseMove);
                document.body.removeEventListener("mouseleave", OnDragMouseUp);
                document.body.style.userSelect = "";
                isDragged = false;
            }
        }

        private void OnDragMouseDown(object ev)
        {
            var e = ev as MouseEvent;
            if (e.button == 0)
            {
                document.body.addEventListener("mouseup", OnDragMouseUp);
                document.body.addEventListener("mousemove", OnDragMouseMove);
                document.body.addEventListener("mouseleave", OnDragMouseUp);
                _Modal.style.userSelect = "none";
                isDragged = true;
                startPoint = TranslationPoint.From(_Modal.style.transform);
            }
        }
    }

    public static class ModalExtensions
    {
        public static T ShowCloseButton<T>(this T modal) where T : Modal
        {
            modal.ShowCloseButton = true;
            return modal;
        }

        public static T HideCloseButton<T>(this T modal) where T : Modal
        {
            modal.ShowCloseButton = false;
            return modal;
        }

        public static T Header<T>(this T modal, string header) where T : Modal
        {
            modal.Header = header;
            return modal;
        }

        public static T LightDismissr<T>(this T modal) where T : Modal
        {
            modal.CanLightDismiss = true;
            return modal;
        }

        public static T Draggabler<T>(this T modal) where T : Modal
        {
            modal.IsDraggable = true;
            return modal;
        }

        public static T NonBlockingr<T>(this T modal) where T : Modal
        {
            modal.IsNonBlocking = true;
            return modal;
        }
    }
}
