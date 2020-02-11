using System;
using System.Collections.Generic;
using System.Linq;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Panel : Layer
    {
        private IComponent _footer;
        private readonly HTMLElement _panel;
        private readonly HTMLElement _panelOverlay;
        private readonly HTMLElement _panelContent;
        private readonly HTMLElement _panelFooter;
        private readonly HTMLElement _panelCommand;
        private readonly HTMLElement _closeButton;

        public Panel() : base()
        {
            _closeButton = Button(_("lal la-times", el: el => el.onclick = (e) => Hide()));
            _panelCommand = Div(_("tss-panel-command"), _closeButton);
            _panelContent = Div(_("tss-panel-content"));
            _panelFooter = Div(_("tss-panel-footer"));
            _panel = Div(_("tss-panel tss-panelSize-small tss-panelSide-far"), _panelCommand, Div(_("tss-panel-inner"), _panelContent, _panelFooter));
            _panelOverlay = Div(_("tss-panel-overlay"));
            _contentHtml = Div(_("tss-panel-container"), _panelOverlay, _panel);
        }

        public override IComponent Content
        {
            get { return _content; }
            set
            {
                if (value != _content)
                {
                    ClearChildren(_panelContent); ;
                    _content = value;
                    if (_content != null)
                    {
                        _panelContent.appendChild(_content.Render());
                    }
                }
            }
        }

        public IComponent Footer
        {
            get { return _footer; }
            set
            {
                if (value != _footer)
                {
                    ClearChildren(_panelFooter); ;
                    _footer = value;
                    if (_footer != null)
                    {
                        _panelFooter.appendChild(_footer.Render());
                    }
                }
            }
        }

        public PanelSize Size
        {
            get { if (Enum.TryParse<PanelSize>(_panel.classList[1].Substring(_panel.classList[1].LastIndexOf('-') + 1), true, out var result)) return result; return PanelSize.Small; }
            set
            {
                _panel.classList.replace(_panel.classList[1], $"tss-panelSize-{value.ToString().ToLower()}");
            }
        }

        public PanelSide Side
        {
            get { if (Enum.TryParse<PanelSide>(_panel.classList[2].Substring(_panel.classList[2].LastIndexOf('-') + 1), true, out var result)) return result; return PanelSide.Far; }
            set
            {
                _panel.classList.replace(_panel.classList[2], $"tss-panelSide-{value.ToString().ToLower()}");
            }
        }

        public bool CanLightDismiss
        {
            get { return _panelOverlay.classList.contains("tss-panel-lightDismiss"); }
            set
            {
                if (value != CanLightDismiss)
                {
                    if (value)
                    {
                        _panelOverlay.classList.add("tss-panel-lightDismiss");
                        _panelOverlay.addEventListener("click", OnCloseClick);
                    }
                    else
                    {
                        _panelOverlay.classList.remove("tss-panel-lightDismiss");
                        _panelOverlay.removeEventListener("click", OnCloseClick);
                    }
                }
            }
        }

        public bool Dark
        {
            get { return _contentHtml.classList.contains("dark"); }
            set
            {
                if (value != Dark)
                {
                    if (value)
                    {
                        _contentHtml.classList.add("dark");
                    }
                    else
                    {
                        _contentHtml.classList.remove("dark");
                    }
                }
            }
        }

        public bool IsNonBlocking
        {
            get { return _contentHtml.classList.contains("tss-panel-modeless"); }
            set
            {
                if (value != IsNonBlocking)
                {
                    if (value)
                    {
                        _contentHtml.classList.add("tss-panel-modeless");
                        //if (IsVisible) document.body.style.overflowY = "";
                    }
                    else
                    {
                        _contentHtml.classList.remove("tss-panel-modeless");
                        //if (IsVisible) document.body.style.overflowY = "hidden";
                    }
                }
            }
        }

        public bool ShowCloseButton
        {
            get { return _closeButton.style.display != "none"; }
            set
            {
                if (value) _closeButton.style.display = "";
                else _closeButton.style.display = "none";
            }

        }

        protected override HTMLElement BuildRenderedContent()
        {
            return _contentHtml;
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
            panel.Size = Panel.PanelSize.Small;
            return panel;
        }
        public static Panel Medium(this Panel panel)
        {
            panel.Size = Panel.PanelSize.Medium;
            return panel;
        }
        public static Panel Large(this Panel panel)
        {
            panel.Size = Panel.PanelSize.Large;
            return panel;
        }
        public static Panel LargeFixed(this Panel panel)
        {
            panel.Size = Panel.PanelSize.LargeFixed;
            return panel;
        }
        public static Panel ExtraLarge(this Panel panel)
        {
            panel.Size = Panel.PanelSize.ExtraLarge;
            return panel;
        }
        public static Panel FullWidth(this Panel panel)
        {
            panel.Size = Panel.PanelSize.FullWidth;
            return panel;
        }

        public static Panel Far(this Panel panel)
        {
            panel.Side = Panel.PanelSide.Far;
            return panel;
        }

        public static Panel Near(this Panel panel)
        {
            panel.Side = Panel.PanelSide.Near;
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
