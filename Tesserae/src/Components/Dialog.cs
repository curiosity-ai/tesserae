using System;
using System.Text.RegularExpressions;
using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

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
            _Modal.classList.add("mss-dialog");
            _ContentHtml.classList.add("mss-dialog-container");
            _ModalFooter = Div(_("mss-modal-footer"));
            _Modal.appendChild(_ModalFooter);

            // As recommended
            CanLightDismiss = true;
        }
    }

    public static class DialogExtensions
    {
        public static Dialog Footer(this Dialog dialog, IComponent footer)
        {
            dialog.Footer = footer;
            return dialog;
        }
    }
}
