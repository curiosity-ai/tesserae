using System;
using System.Collections.Generic;
using System.Linq;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public enum PanelSize
    {
        Small,
        Medium,
        Large,
        LargeFixed,
        ExtraLarge,
        FullWidth
    }

    public enum PanelSide
    {
        Far,
        Near
    }

    public class Panel : Layer
    {
        private IComponent _Footer;
        private HTMLElement _Panel;
        private HTMLElement _PanelOverlay;
        private HTMLElement _PanelContent;
        private HTMLElement _PanelFooter;
        private HTMLElement _PanelCommand;
        private HTMLElement _CloseButton;

        #region Properties

        public override IComponent Content
        {
            get { return _Content; }
            set
            {
                if (value != _Content)
                {
                    ClearChildren(_PanelContent); ;
                    _Content = value;
                    if (_Content != null)
                    {
                        _PanelContent.appendChild(_Content.Render());
                    }
                }
            }
        }

        public IComponent Footer
        {
            get { return _Footer; }
            set
            {
                if (value != _Footer)
                {
                    ClearChildren(_PanelFooter); ;
                    _Footer = value;
                    if (_Footer != null)
                    {
                        _PanelFooter.appendChild(_Footer.Render());
                    }
                }
            }
        }

        public PanelSize Size
        {
            get { if (Enum.TryParse<PanelSize>(_Panel.classList[1].Substring(_Panel.classList[1].LastIndexOf('-') + 1), true, out var result)) return result; return PanelSize.Small; }
            set
            {
                _Panel.classList.replace(_Panel.classList[1], $"tss-panelSize-{value.ToString().ToLower()}");
            }
        }

        public PanelSide Side
        {
            get { if (Enum.TryParse<PanelSide>(_Panel.classList[2].Substring(_Panel.classList[2].LastIndexOf('-') + 1), true, out var result)) return result; return PanelSide.Far; }
            set
            {
                _Panel.classList.replace(_Panel.classList[2], $"tss-panelSide-{value.ToString().ToLower()}");
            }
        }

        public bool CanLightDismiss
        {
            get { return _PanelOverlay.classList.contains("tss-panel-lightDismiss"); }
            set
            {
                if (value != CanLightDismiss)
                {
                    if (value)
                    {
                        _PanelOverlay.classList.add("tss-panel-lightDismiss");
                        _PanelOverlay.addEventListener("click", OnCloseClick);
                    }
                    else
                    {
                        _PanelOverlay.classList.remove("tss-panel-lightDismiss");
                        _PanelOverlay.removeEventListener("click", OnCloseClick);
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

        public bool IsNonBlocking
        {
            get { return _ContentHtml.classList.contains("tss-panel-modeless"); }
            set
            {
                if (value != IsNonBlocking)
                {
                    if (value)
                    {
                        _ContentHtml.classList.add("tss-panel-modeless");
                        //if (IsVisible) document.body.style.overflowY = "";
                    }
                    else
                    {
                        _ContentHtml.classList.remove("tss-panel-modeless");
                        //if (IsVisible) document.body.style.overflowY = "hidden";
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

        public Panel() : base()
        {
            _CloseButton = Button(_("fal fa-times", el: el => el.onclick = (e) => Hide()));
            _PanelCommand = Div(_("tss-panel-command"), _CloseButton);
            _PanelContent = Div(_("tss-panel-content"));
            _PanelFooter = Div(_("tss-panel-footer"));
            _Panel = Div(_("tss-panel tss-panelSize-small tss-panelSide-far"), _PanelCommand, Div(_("tss-panel-inner"), _PanelContent, _PanelFooter));
            _PanelOverlay = Div(_("tss-panel-overlay"));
            _ContentHtml = Div(_("tss-panel-container"), _PanelOverlay, _Panel);
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
        public override void Hide(Action onHidden = null)
        {
            base.Hide(() =>
            {
                if (!IsNonBlocking) document.body.style.overflowY = "";
                onHidden?.Invoke();
            });
        }
        private void OnCloseClick(object ev)
        {
            Hide();
        }
    }

    public static class PanelExtensions
    {
        public static Panel ShowCloseButton(this Panel panel)
        {
            panel.ShowCloseButton = true;
            return panel;
        }

        public static Panel HideCloseButton(this Panel panel)
        {
            panel.ShowCloseButton = false;
            return panel;
        }

        public static Panel Footer(this Panel panel, IComponent footer)
        {
            panel.Footer = footer;
            return panel;
        }

        public static Panel Small(this Panel panel)
        {
            panel.Size = PanelSize.Small;
            return panel;
        }
        public static Panel Medium(this Panel panel)
        {
            panel.Size = PanelSize.Medium;
            return panel;
        }
        public static Panel Large(this Panel panel)
        {
            panel.Size = PanelSize.Large;
            return panel;
        }
        public static Panel LargeFixed(this Panel panel)
        {
            panel.Size = PanelSize.LargeFixed;
            return panel;
        }
        public static Panel ExtraLarge(this Panel panel)
        {
            panel.Size = PanelSize.ExtraLarge;
            return panel;
        }
        public static Panel FullWidth(this Panel panel)
        {
            panel.Size = PanelSize.FullWidth;
            return panel;
        }

        public static Panel Far(this Panel panel)
        {
            panel.Side = PanelSide.Far;
            return panel;
        }

        public static Panel Near(this Panel panel)
        {
            panel.Side = PanelSide.Near;
            return panel;
        }

        public static Panel LightDismiss(this Panel panel)
        {
            panel.CanLightDismiss = true;
            return panel;
        }

        public static Panel NonBlocking(this Panel panel)
        {
            panel.IsNonBlocking = true;
            return panel;
        }
    }
}
