using H5;
using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Panel")]
    public sealed class Panel : Layer<Panel>, IHasBackgroundColor
    {
        private event OnHideHandler HidePanel;
        public delegate void OnHideHandler(Panel sender);

        private IComponent _footer;
        private readonly HTMLElement _panel;
        private readonly HTMLElement _panelOverlay;
        private readonly HTMLElement _panelContent;
        private readonly HTMLElement _panelFooter;
        private readonly HTMLElement _panelCommand;
        private readonly HTMLElement _closeButton;
        private readonly HTMLElement _panelTitle;

        public Panel(string title = null) : this(TextBlock(title).SemiBold()) { }

        public Panel(IComponent title)
        {
            _panelTitle = Div(_("tss-panel-title"));

            _closeButton = Button(_($"tss-panel-command-button", el: el => el.onclick = (e) => Hide()), I(_("tss-fontsize-small " + UIcons.Cross.ToString())));
            _panelCommand = Div(_("tss-panel-command"), _panelTitle, _closeButton);
            _panelContent = Div(_("tss-panel-content"));
            _panelFooter = Div(_("tss-panel-footer"));
            _panel = Div(_("tss-panel tss-panelSize-small tss-panelSide-far"), _panelCommand, Div(_("tss-panel-inner"), _panelContent, _panelFooter));
            _panelOverlay = Div(_("tss-panel-overlay"));
            _contentHtml = Div(_("tss-panel-container"), _panelOverlay, _panel);

            if(title is object)
            {
                _panelTitle.appendChild(title.Render());
            }
        }

        public override IComponent Content
        {
            get => _content;
            set
            {
                ClearChildren(_panelContent); ;
                _content = value;
                if (_content != null)
                {
                    _panelContent.appendChild(_content.Render());
                }
            }
        }

        public IComponent Footer
        {
            get => _footer;
            set
            {
                ClearChildren(_panelFooter); ;
                _footer = value;
                if (_footer != null)
                {
                    _panelFooter.appendChild(_footer.Render());
                }
            }
        }

        public PanelSize Size
        {
            get
            {
                return _panel.classList[1].As<PanelSize>(); //This works because the PanelSize Enum is emited as a string
                //switch (_panel.classList[1])
                //{
                //    case "tss-panelSize-small" : return PanelSize.Small;
                //    case "tss-panelSize-medium" : return PanelSize.Medium;
                //    case "tss-panelSize-large" : return PanelSize.Large;
                //    case "tss-panelSize-largefixed" : return PanelSize.LargeFixed;
                //    case "tss-panelSize-extralarge" : return PanelSize.ExtraLarge;
                //    case "tss-panelSize-fullwidth": return PanelSize.FullWidth;
                //}
            }
            set => _panel.classList.replace(_panel.classList[1], value.ToString());
        }

        public PanelSide Side
        {
            get
            {
                if (Enum.TryParse<PanelSide>(_panel.classList[2].Substring(_panel.classList[2].LastIndexOf('-') + 1), true, out var result))
                {
                    return result;
                }

                return PanelSide.Far;
            }
            set => _panel.classList.replace(_panel.classList[2], value.ToString());
        }

        public bool CanLightDismiss
        {
            get => _panelOverlay.classList.contains("tss-panel-lightDismiss");
            set
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

        public bool IsDark
        {
            get => _contentHtml.classList.contains("tss-dark");
            set
            {
                if (value)
                {
                    _contentHtml.classList.add("tss-dark");
                }
                else
                {
                    _contentHtml.classList.remove("tss-dark");
                }
            }
        }

        public bool IsNonBlocking
        {
            get => _contentHtml.classList.contains("tss-panel-modeless");
            set
            {
                if (value)
                {
                    _contentHtml.classList.add("tss-panel-modeless");
                }
                else
                {
                    _contentHtml.classList.remove("tss-panel-modeless");
                }
            }
        }

        public bool ShowCloseButton
        {
            get => _closeButton.style.display != "none";
            set
            {
                if (value) _closeButton.style.display = "";
                else _closeButton.style.display = "none";
            }
        }

        public string Background { get => _panel.style.background; set => _panel.style.background = value; }

        protected override HTMLElement BuildRenderedContent()
        {
            return _contentHtml;
        }

        public override Panel Show()
        {
            if (!IsNonBlocking) document.body.style.overflowY = "hidden";

            if (Side == PanelSide.Near)
            {
                _panel.classList.add("tss-panel-near-animate");
                _panel.classList.remove("tss-panel-far-animate");
            }
            else
            {
                _panel.classList.add("tss-panel-far-animate");
                _panel.classList.remove("tss-panel-near-animate");
            }

            return base.Show();
        }

        public Panel OnHide(OnHideHandler onHide)
        {
            HidePanel += onHide;
            return this;
        }

        public override void Hide(Action onHidden = null)
        {
            HidePanel?.Invoke(this);

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


        [Enum(Emit.StringName)]
        public enum PanelSize
        {
            [Name("tss-panelSize-small")] Small,
            [Name("tss-panelSize-medium")] Medium,
            [Name("tss-panelSize-large")] Large,
            [Name("tss-panelSize-largefixed")] LargeFixed,
            [Name("tss-panelSize-extralarge")] ExtraLarge,
            [Name("tss-panelSize-fullwidth")] FullWidth,
        }

        public enum PanelSide
        {
            Far,
            Near
        }

        public Panel HideCloseButton()
        {
            ShowCloseButton = false;
            return this;
        }

        public Panel SetFooter(IComponent footer)
        {
            Footer = footer;
            return this;
        }

        public Panel Small()
        {
            Size = PanelSize.Small;
            return this;
        }
        public Panel Medium()
        {
            Size = PanelSize.Medium;
            return this;
        }
        public Panel Large()
        {
            Size = PanelSize.Large;
            return this;
        }
        public Panel LargeFixed()
        {
            Size = PanelSize.LargeFixed;
            return this;
        }
        public Panel ExtraLarge()
        {
            Size = PanelSize.ExtraLarge;
            return this;
        }
        public Panel FullWidth()
        {
            Size = PanelSize.FullWidth;
            return this;
        }

        public Panel Far()
        {
            Side = PanelSide.Far;
            return this;
        }

        public Panel Near()
        {
            Side = PanelSide.Near;
            return this;
        }

        public Panel LightDismiss()
        {
            CanLightDismiss = true;
            return this;
        }

        public Panel NoLightDismiss()
        {
            CanLightDismiss = false;
            return this;
        }

        public Panel Dark()
        {
            IsDark = true;
            return this;
        }

        public Panel NonBlocking()
        {
            IsNonBlocking = true;
            return this;
        }
        public Panel Blocking()
        {
            IsNonBlocking = false;
            return this;
        }
    }
}