using H5;
using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A side-anchored slide-in panel (drawer) typically used for property inspectors and detail views.
    /// </summary>
    [H5.Name("tss.Panel")]
    public sealed class Panel : Layer<Panel>, IHasBackgroundColor, IBindableComponent<bool>
    {
        private event OnHideHandler HidePanel;
        public delegate void        OnHideHandler(Panel sender);

        private readonly SettableObservable<bool> _observable = new SettableObservable<bool>();

        private          IComponent  _footer;
        private readonly HTMLElement _panel;
        private readonly HTMLElement _panelOverlay;
        private readonly HTMLElement _panelContent;
        private readonly HTMLElement _panelFooter;
        private readonly HTMLElement _panelCommand;
        private readonly HTMLElement _closeButton;
        private readonly HTMLElement _panelTitle;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Panel(string title = null) : this(TextBlock(title).SemiBold()) { }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Panel(IComponent title)
        {
            _panelTitle = Div(_("tss-panel-title"));

            _closeButton  = Button(_($"tss-panel-command-button", el: el => el.onclick = (e) => Hide()), I(_("tss-fontsize-small " + UIcons.Cross.ToString())));
            _panelCommand = Div(_("tss-panel-command"), _panelTitle, _closeButton);
            _panelContent = Div(_("tss-panel-content"));
            _panelFooter  = Div(_("tss-panel-footer"));
            _panel        = Div(_("tss-panel tss-panelSize-small tss-panelSide-far"), _panelCommand, Div(_("tss-panel-inner"), _panelContent, _panelFooter));
            _panelOverlay = Div(_("tss-panel-overlay"));
            _contentHtml  = Div(_("tss-panel-container"), _panelOverlay, _panel);

            if (title is object)
            {
                _panelTitle.appendChild(title.Render());
            }
        }

        /// <summary>
        /// Sets the content rendered inside the surface.
        /// </summary>
        public override IComponent Content
        {
            get => _content;
            set
            {
                ClearChildren(_panelContent);
                ;
                _content = value;

                if (_content != null)
                {
                    _panelContent.appendChild(_content.Render());
                }
            }
        }

        /// <summary>
        /// Gets or sets the footer of the component.
        /// </summary>
        public IComponent Footer
        {
            get => _footer;
            set
            {
                ClearChildren(_panelFooter);
                ;
                _footer = value;

                if (_footer != null)
                {
                    _panelFooter.appendChild(_footer.Render());
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of the component.
        /// </summary>
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

        /// <summary>
        /// Gets or sets which side of the parent the component is anchored to.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the surface can be dismissed by clicking outside it (light dismiss).
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the component uses the dark colour theme.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the surface is non-blocking (allows interaction with the page beneath it).
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the close button is shown.
        /// </summary>
        public bool ShowCloseButton
        {
            get => _closeButton.style.display != "none";
            set
            {
                if (value) _closeButton.style.display = "";
                else _closeButton.style.display       = "none";
            }
        }

        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public string Background { get => _panel.style.background; set => _panel.style.background = value; }

        protected override HTMLElement BuildRenderedContent()
        {
            return new FocusTrap(_contentHtml).Render();
        }

        /// <summary>
        /// Shows the component.
        /// </summary>
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

            var result        = base.Show();
            _observable.Value = true;
            return result;
        }

        /// <summary>
        /// Registers a callback invoked when the hide event fires.
        /// </summary>
        public Panel OnHide(OnHideHandler onHide)
        {
            HidePanel += onHide;
            return this;
        }

        /// <summary>
        /// Hides the component.
        /// </summary>
        public override void Hide(Action onHidden = null)
        {
            HidePanel?.Invoke(this);
            _observable.Value = false;

            base.Hide(() =>
            {
                if (!IsNonBlocking) document.body.style.overflowY = "";
                onHidden?.Invoke();
            });
        }

        /// <summary>
        /// Returns an observable that tracks the visibility of the panel.
        /// </summary>
        public IObservable<bool> AsObservable() => _observable;

        /// <summary>
        /// Programmatically shows or hides the panel as part of a two-way binding.
        /// </summary>
        public void SetBoundValue(bool value)
        {
            if (value)
            {
                if (!IsVisible) Show();
            }
            else
            {
                if (IsVisible) Hide();
            }
        }
        private void OnCloseClick(object ev)
        {
            Hide();
        }


        [Enum(Emit.StringName)]
        public enum PanelSize
        {
            [Name("tss-panelSize-small")]      Small,
            [Name("tss-panelSize-medium")]     Medium,
            [Name("tss-panelSize-large")]      Large,
            [Name("tss-panelSize-largefixed")] LargeFixed,
            [Name("tss-panelSize-extralarge")] ExtraLarge,
            [Name("tss-panelSize-fullwidth")]  FullWidth,
        }

        [Enum(Emit.StringName)]
        public enum PanelSide
        {
            [Name("tss-panelSide-far")]  Far,
            [Name("tss-panelSide-near")]  Near
        }

        /// <summary>
        /// Hides the close button.
        /// </summary>
        public Panel HideCloseButton()
        {
            ShowCloseButton = false;
            return this;
        }

        /// <summary>
        /// Sets the footer of the component.
        /// </summary>
        public Panel SetFooter(IComponent footer)
        {
            Footer = footer;
            return this;
        }

        /// <summary>
        /// Renders the component at small size.
        /// </summary>
        public Panel Small()
        {
            Size = PanelSize.Small;
            return this;
        }
        /// <summary>
        /// Renders the component at medium size.
        /// </summary>
        public Panel Medium()
        {
            Size = PanelSize.Medium;
            return this;
        }
        /// <summary>
        /// Renders the component at large size.
        /// </summary>
        public Panel Large()
        {
            Size = PanelSize.Large;
            return this;
        }
        /// <summary>
        /// Renders the component at large size with a fixed width.
        /// </summary>
        public Panel LargeFixed()
        {
            Size = PanelSize.LargeFixed;
            return this;
        }
        /// <summary>
        /// Renders the component at extra-large size.
        /// </summary>
        public Panel ExtraLarge()
        {
            Size = PanelSize.ExtraLarge;
            return this;
        }
        /// <summary>
        /// Stretches the component to the full width of its parent.
        /// </summary>
        public Panel FullWidth()
        {
            Size = PanelSize.FullWidth;
            return this;
        }

        /// <summary>
        /// Anchors the component to the far side of its parent.
        /// </summary>
        public Panel Far()
        {
            Side = PanelSide.Far;
            return this;
        }

        /// <summary>
        /// Anchors the component to the near side of its parent.
        /// </summary>
        public Panel Near()
        {
            Side = PanelSide.Near;
            return this;
        }

        /// <summary>
        /// Enables light-dismiss behaviour (clicking outside the surface closes it).
        /// </summary>
        public Panel LightDismiss()
        {
            CanLightDismiss = true;
            return this;
        }

        /// <summary>
        /// Removes / disables the light dismiss on the component.
        /// </summary>
        public Panel NoLightDismiss()
        {
            CanLightDismiss = false;
            return this;
        }

        /// <summary>
        /// Applies the dark colour scheme to the component.
        /// </summary>
        public Panel Dark()
        {
            IsDark = true;
            return this;
        }

        /// <summary>
        /// Makes the surface non-blocking, allowing interaction with the page beneath it.
        /// </summary>
        public Panel NonBlocking()
        {
            IsNonBlocking = true;
            return this;
        }
        /// <summary>
        /// Makes the surface blocking (the default — interaction with the page beneath is prevented).
        /// </summary>
        public Panel Blocking()
        {
            IsNonBlocking = false;
            return this;
        }
    }
}
