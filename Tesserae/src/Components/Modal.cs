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
        private HTMLElement _Modal;
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
            get { return _ModalOverlay.classList.contains("mss-modal-lightDismiss"); }
            set
            {
                if (value != CanLightDismiss)
                {
                    if (value)
                    {
                        _ModalOverlay.classList.add("mss-modal-lightDismiss");
                        _ModalOverlay.addEventListener("click", OnCloseClick);
                    }
                    else
                    {
                        _ModalOverlay.classList.remove("mss-modal-lightDismiss");
                        _ModalOverlay.removeEventListener("click", OnCloseClick);
                    }
                }
            }
        }

        public bool IsDraggable
        {
            get { return _Modal.classList.contains("mss-modal-draggable"); }
            set
            {
                if (value != IsDraggable)
                {
                    if (value)
                    {
                        _Modal.classList.add("mss-modal-draggable");
                        _Modal.addEventListener("mousedown", OnDragMouseDown);

                    }
                    else
                    {
                        _Modal.classList.remove("mss-modal-draggable");
                        _Modal.removeEventListener("mousedown", OnDragMouseDown);
                    }
                }
            }
        }

        public bool IsNonBlocking
        {
            get { return _ContentHtml.classList.contains("mss-modal-modeless"); }
            set
            {
                if (value != IsNonBlocking)
                {
                    if (value)
                    {
                        _ContentHtml.classList.add("mss-modal-modeless");
                        if (IsVisible) document.body.style.overflowY = "";
                    }
                    else
                    {
                        _ContentHtml.classList.remove("mss-modal-modeless");
                        if (IsVisible) document.body.style.overflowY = "hidden";
                    }
                }
            }
        }

        #endregion

        public Modal(string header = string.Empty)
        {
            _ModalHeader = Div(_("mss-modal-header", text: header));
            _ModalCommand = Div(_("mss-modal-command"), _ModalHeader, Button(_("fal fa-times", el: el => el.onclick = (e) => Hide())));
            _ModalContent = Div(_("mss-modal-content"));
            _Modal = Div(_("mss-modal", styles: s => s.transform = "translate(0px,0px)"), _ModalCommand, _ModalContent);
            _ModalOverlay = Div(_("mss-modal-overlay"));
            _ContentHtml = Div(_("mss-modal-container"), _ModalOverlay, _Modal);
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
                _Modal.removeEventListener("mouseup", OnDragMouseUp);
                _Modal.removeEventListener("mousemove", OnDragMouseMove);
                _Modal.removeEventListener("mouseleave", OnDragMouseUp);
                _Modal.style.userSelect = "";
                isDragged = false;
            }
        }

        private void OnDragMouseDown(object ev)
        {
            var e = ev as MouseEvent;
            if (e.button == 0)
            {
                _Modal.addEventListener("mouseup", OnDragMouseUp);
                _Modal.addEventListener("mousemove", OnDragMouseMove);
                _Modal.addEventListener("mouseleave", OnDragMouseUp);
                _Modal.style.userSelect = "none";
                isDragged = true;
                startPoint = TranslationPoint.From(_Modal.style.transform);
            }
        }
    }

    public static class ModalExtensions
    {
        public static Modal Header(this Modal modal, string header)
        {
            modal.Header = header;
            return modal;
        }

        public static Modal LightDismiss(this Modal modal)
        {
            modal.CanLightDismiss = true;
            return modal;
        }

        public static Modal Draggable(this Modal modal)
        {
            modal.IsDraggable = true;
            return modal;
        }

        public static Modal NonBlocking(this Modal modal)
        {
            modal.IsNonBlocking = true;
            return modal;
        }
    }
}
