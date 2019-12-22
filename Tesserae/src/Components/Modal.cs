using System;
using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Modal : Layer
    {
        private HTMLElement _Modal;
        private HTMLElement _ModalHeader;
        private HTMLElement _ModalOverlay;
        private HTMLElement _ModalContent;
        private HTMLElement _ModalCommand;

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

        #endregion

        public Modal(string header = string.Empty)
        {
            _ModalHeader = Div(_("mss-modal-header", text: header));
            _ModalCommand = Div(_("mss-modal-command"), _ModalHeader, Button(_("fal fa-times", el: el => el.onclick = (e) => Hide())));
            _ModalContent = Div(_("mss-modal-content"));
            _Modal = Div(_("mss-modal", styles: s => s.transform = "translate(0px,0px)"), _ModalCommand, _ModalContent);
            _ModalOverlay = Div(_("mss-modal-overlay mss-modal-lightDismiss"));
            _ContentHtml = Div(_("mss-modal-container"), _ModalOverlay, _Modal);
        }
        protected override HTMLElement BuildRenderedContent()
        {
            return _ContentHtml;
        }

        public override void Show()
        {
            document.body.style.overflowY = "hidden";
            base.Show();
        }
        public override void Hide()
        {
            base.Hide();
            document.body.style.overflowY = "";
        }

        private void OnCloseClick(object ev)
        {
            Hide();
        }
    }
}
