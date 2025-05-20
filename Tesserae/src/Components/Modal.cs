﻿using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Modal")]
    public sealed class Modal : Layer<Modal>, ISpecialCaseStyling, IHasBackgroundColor
    {
        private event OnShowHandler Shown;
        public delegate void        OnShowHandler(Modal sender);

        private event OnHideHandler Hidden;
        public delegate void        OnHideHandler(Modal sender);

        private readonly HTMLElement    _closeButton;
        private readonly HTMLElement    _modalHeader;
        private readonly HTMLElement    _modalFooter;
        private readonly HTMLElement    _modalOverlay;
        private readonly HTMLDivElement _modalContent;

        private readonly HTMLElement    _modalLeftHeaderCommands;
        private readonly HTMLElement    _modalHeaderCommands;
        private readonly HTMLElement    _modalLeftFooterCommands;
        private readonly HTMLElement    _modalFooterCommands;
        private readonly HTMLElement    _modalHeaderContents;
        private readonly HTMLElement    _modalFooterContents;
        private readonly HTMLDivElement _modal;

        private bool             _isDragged;
        private TranslationPoint _startPoint;

        public bool AnimateOnShow { get { return base.AnimateOnShow; } set { base.AnimateOnShow = value; } }

        public HTMLElement StylingContainer => _modal;

        public bool PropagateToStackItemParent => false;

        public string Background
        {
            get => _modal.style.background;
            set => _modal.style.background = value;
        }

        public Modal(IComponent header = null)
        {
            _modalHeaderContents = Div(_("tss-modal-header-content"));
            _modalFooterContents = Div(_("tss-modal-footer-content"));

            _modalHeaderCommands     = Div(_("tss-modal-header-commands tss-default-component-no-margin"));
            _modalLeftHeaderCommands = Div(_("tss-modal-left-header-commands tss-default-component-no-margin"));
            _modalFooterCommands     = Div(_("tss-modal-footer-commands tss-default-component-no-margin"));
            _modalLeftFooterCommands = Div(_("tss-modal-left-footer-commands tss-default-component-no-margin"));

            _modalHeader = Div(_("tss-modal-header"), _modalLeftHeaderCommands, _modalHeaderContents, _modalHeaderCommands);
            _modalFooter = Div(_("tss-modal-footer"), _modalLeftFooterCommands, _modalFooterContents, _modalFooterCommands);

            if (header != null)
            {
                _modalHeaderContents.appendChild(header.Render());
            }
            else
            {
                _modalHeader.style.display = "none";
            }

            _closeButton = Button(_($"tss-modal-button", el: el => el.onclick = e => Hide()), I(_("tss-fontsize-small " + UIcons.Cross.ToString())));
            _modalHeaderCommands.appendChild(_closeButton);

            _modalContent = Div(_("tss-modal-content"));
            _modal        = Div(_("tss-modal", styles: s => s.transform = "translate(0px,0px)"), _modalHeader, _modalContent, _modalFooter);
            _modalOverlay = Div(_("tss-modal-overlay"));
            _contentHtml  = Div(_("tss-modal-container"), _modalOverlay, _modal);
            IsNonBlocking = false; //blocking by default

            // 2020-05-01 DWR: In order to pick up key press events, we need to set the InnerElement on the base class before calling AttachKeys AND we need to give the container a tabindex value, otherwise it's not focusable and can't pick up key
            InnerElement    = _modal;
            _modal.tabIndex = 0;
            AttachKeys();

            // Look for [Esc] presses and hide the modal if one occurs - note that we don't know at this point whether this modal will have a close button (or be considered "light dismiss"-applicable, which is where clicking outside the modal can close the modal
            // it - which is logic which we use here to decide whether the User must explicitly click the close button or if they can easily discard it via clicking away or hitting [Esc] <- 2020-05-01 DWR: Checking for light dismiss was Rafa's idea, we MIGHT also
            // want to allow [Esc] support for modals with a close button that DON'T allow light dismiss in the future). So we'll hook up the key press now and then check the component's configuration if the event occurs. Also note that there is key PRESS event
            // for [Esc] (unlike other buttons), only key DOWN and key UP and so we'll have to settle for using onKeyUp.
            KeyUp += (_, e) =>
            {
                if ((e.keyCode == 27) && CanLightDismiss && WillShowCloseButton)
                {
                    Hide();
                }
            };
        }

        public Modal SetHeader(IComponent header)
        {
            _modalHeader.style.display = "";
            ClearChildren(_modalHeaderContents);

            if (header is object)
            {
                _modalHeaderContents.appendChild(header.Render());
            }
            return this;
        }

        public Modal SetFooter(IComponent footer)
        {
            _modalFooter.style.display = "";
            ClearChildren(_modalFooterContents);

            if (footer is object)
            {
                _modalFooterContents.appendChild(footer.Render());
            }
            return this;
        }

        public Modal SetHeaderCommands(params IComponent[] commands)
        {
            _modalHeader.style.display = "";
            ClearChildren(_modalHeaderCommands);

            if (commands is object)
            {
                foreach (var command in commands)
                {
                    _modalHeaderCommands.appendChild(command.Render());
                }
            }

            return this;
        }

        public Modal SetFooterCommands(params IComponent[] commands)
        {
            _modalFooter.style.display = "";
            ClearChildren(_modalFooterCommands);

            if (commands is object)
            {
                foreach (var command in commands)
                {
                    _modalFooterCommands.appendChild(command.Render());
                }
            }

            return this;
        }

        public Modal SetLeftHeaderCommands(params IComponent[] commands)
        {
            _modalHeader.style.display = "";
            ClearChildren(_modalLeftHeaderCommands);

            if (commands is object)
            {
                foreach (var command in commands)
                {
                    _modalLeftHeaderCommands.appendChild(command.Render());
                }
            }

            return this;
        }

        public Modal SetLeftFooterCommands(params IComponent[] commands)
        {
            _modalFooter.style.display = "";
            ClearChildren(_modalLeftFooterCommands);

            if (commands is object)
            {
                foreach (var command in commands)
                {
                    _modalLeftFooterCommands.appendChild(command.Render());
                }
            }

            return this;
        }

        public Modal ContentHeight(UnitSize height)
        {
            _modalContent.style.height = height.ToString();
            return this;
        }

        public Modal NoHeader()
        {
            ClearChildren(_modalHeader);
            _modalHeader.style.display = "none";
            return this;
        }

        public Modal NoFooter()
        {
            ClearChildren(_modalFooter);
            _modalFooter.style.display = "none";
            return this;
        }

        public Modal NoAnimation()
        {
            AnimateOnShow = false;
            return this;
        }

        public override IComponent Content
        {
            get => _content;
            set
            {
                ClearChildren(_modalContent);
                ;
                _content = value;

                if (_content != null)
                {
                    _modalContent.appendChild(_content.Render());
                }
            }
        }

        public bool CanLightDismiss
        {
            get => _modalOverlay.classList.contains("tss-modal-lightDismiss");
            set
            {
                if (value)
                {
                    _modalOverlay.classList.add("tss-modal-lightDismiss");
                    _modalOverlay.addEventListener("click",       OnCloseClick);
                    _modalOverlay.addEventListener("dblclick",    OnCloseClick);
                    _modalOverlay.addEventListener("contextmenu", OnCloseClick);
                }
                else
                {
                    _modalOverlay.classList.remove("tss-modal-lightDismiss");
                    _modalOverlay.removeEventListener("click",       OnCloseClick);
                    _modalOverlay.removeEventListener("dblclick",    OnCloseClick);
                    _modalOverlay.removeEventListener("contextmenu", OnCloseClick);
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

        public bool IsDraggable
        {
            get => _modalHeaderContents.classList.contains("tss-modal-draggable");
            set
            {
                if (value)
                {
                    _modalHeaderContents.classList.add("tss-modal-draggable");
                    _modalHeaderContents.addEventListener("mousedown", OnDragMouseDown);

                }
                else
                {
                    _modalHeaderContents.classList.remove("tss-modal-draggable");
                    _modalHeaderContents.removeEventListener("mousedown", OnDragMouseDown);
                }
            }
        }

        public bool IsNonBlocking
        {
            get => _contentHtml.classList.contains("tss-modal-modeless");
            set
            {
                if (value)
                {
                    _contentHtml.classList.add("tss-modal-modeless");
                    if (IsVisible) document.body.style.overflowY = "";
                }
                else
                {
                    _contentHtml.classList.remove("tss-modal-modeless");
                    if (IsVisible) document.body.style.overflowY = "hidden";
                }
            }
        }

        public IComponent ShowEmbedded()
        {
            WillShowCloseButton = false;
            _modal.classList.add("tss-embedded");
            return Raw(_modal);
        }

        public bool WillShowCloseButton
        {
            get => _closeButton.style.display != "none";
            set
            {
                if (value)
                {
                    _closeButton.style.display = "";
                    _modalHeader.style.display = "";
                }
                else _closeButton.style.display = "none";
            }

        }

        public Modal CenterContent()
        {
            _modalContent.classList.add("tss-modal-centered-content");
            return this;
        }

        public Modal NoPadding()
        {
            _modalContent.style.padding = _modalHeader.style.padding = _modalFooter.style.padding = "unset";
            return this;
        }

        public Modal NoContentPadding()
        {
            _modalContent.style.padding = "unset";
            return this;
        }

        public Modal ShowCloseButton()
        {
            WillShowCloseButton = true;
            return this;
        }

        public Modal HideCloseButton()
        {
            WillShowCloseButton = false;
            return this;
        }

        public Modal LightDismiss()
        {
            CanLightDismiss = true;
            return this;
        }

        public Modal NoLightDismiss()
        {
            CanLightDismiss = false;
            return this;
        }

        public Modal Dark()
        {
            IsDark = true;
            return this;
        }

        public Modal Draggable()
        {
            IsDraggable = true;
            return this;
        }

        public Modal NonBlocking()
        {
            IsNonBlocking = true;
            return this;
        }

        public Modal Blocking()
        {
            IsNonBlocking = false;
            return this;
        }

        public void ShowAt(UnitSize fromTop = null, UnitSize fromLeft = null, UnitSize fromRight = null, UnitSize fromBottom = null)
        {
            _modal.style.marginTop    = fromTop is object ? fromTop.ToString() : UnitSize.Auto().ToString();
            _modal.style.marginLeft   = fromLeft is object ? fromLeft.ToString() : UnitSize.Auto().ToString();
            _modal.style.marginRight  = fromRight is object ? fromRight.ToString() : UnitSize.Auto().ToString();
            _modal.style.marginBottom = fromBottom is object ? fromBottom.ToString() : UnitSize.Auto().ToString();
            DoShow();
        }

        public override Modal Show()
        {
            _modal.style.marginTop    = "";
            _modal.style.marginLeft   = "";
            _modal.style.marginRight  = "";
            _modal.style.marginBottom = "";
            DoShow();
            return this;
        }

        public Task ShowAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            OnHide((_) => tcs.SetResult(true));
            Show();

            return tcs.Task;
        }

        private void DoShow()
        {
            _modal.style.transform = "translate(0px,0px)";
            if (AnimateOnShow) _modal.classList.add("tss-modal-animate");
            if (!IsNonBlocking) document.body.style.overflowY = "hidden";
            base.Show();
            _modal.focus(); // 2020-05-01 DWR: We need to put focus into the modal container in order to pick up keypresses
            RaiseOnShow();
        }

        public Modal OnHide(OnHideHandler onHide)
        {
            Hidden += onHide;
            return this;
        }

        public Modal OnShow(OnShowHandler onShow)
        {
            Shown += onShow;
            return this;
        }


        public void RaiseOnHide()
        {
            Hidden?.Invoke(this);
        }

        public void RaiseOnShow()
        {
            Shown?.Invoke(this);
        }

        public override void Hide(Action onHidden = null)
        {
            RaiseOnHide();

            base.Hide(() =>
            {
                if (!IsNonBlocking) document.body.style.overflowY = "";
                onHidden?.Invoke();
            });
        }

        protected override HTMLElement BuildRenderedContent() => _contentHtml;

        private void OnCloseClick(object ev) => Hide();

        private void OnDragMouseMove(object ev)
        {
            if (_isDragged)
            {
                var e = ev as MouseEvent;
                _startPoint.X          += e.movementX;
                _startPoint.Y          += e.movementY;
                _modal.style.transform =  _startPoint.To(_modal.getBoundingClientRect().As<DOMRect>());
            }
        }

        private void OnDragMouseUp(object ev)
        {
            var e = ev as MouseEvent;

            if (_isDragged && e.button == 0)
            {
                document.body.removeEventListener("mouseup",    OnDragMouseUp);
                document.body.removeEventListener("mousemove",  OnDragMouseMove);
                document.body.removeEventListener("mouseleave", OnDragMouseUp);
                document.body.classList.remove("tss-modal-dragging");
                document.body.style.userSelect = "";
                _isDragged                     = false;
            }
        }

        private void OnDragMouseDown(object ev)
        {
            var e = ev as MouseEvent;

            if (e.button == 0)
            {
                document.body.addEventListener("mouseup",    OnDragMouseUp);
                document.body.addEventListener("mousemove",  OnDragMouseMove);
                document.body.addEventListener("mouseleave", OnDragMouseUp);
                document.body.classList.add("tss-modal-dragging");
                _modal.style.userSelect = "none";
                _startPoint             = TranslationPoint.From(_modal.style.transform);
                _isDragged              = true;
            }
        }

        class TranslationPoint
        {
            static readonly Regex regex      = new Regex(@"translate\(([-0-9.].*?)px,\s?([-0-9.].*?)px\)");
            static readonly Regex regexShort = new Regex(@"translate\(([-0-9.].*?)px\)");

            public TranslationPoint(double x = 0, double y = 0)
            {
                X = x;
                Y = y;
            }

            public double X;
            public double Y;

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

            public string To(DOMRect rect)
            {
                var x     = X;
                var y     = Y;
                var halfW = (window.innerWidth  - rect.width)  / 2;
                var halfH = (window.innerHeight - rect.height) / 2;

                if (x < -halfW + 5)
                {
                    x = -halfW + 5;
                }

                if (y < -halfH + 5)
                {
                    y = -halfH + 5;
                }

                if (y > halfH - 5)
                {
                    y = halfH - 5;
                }

                if (x > halfW - 5)
                {
                    x = halfW - 5;
                }

                return $"translate({x}px,{y}px)";
            }
        }
    }
}