using System;
using System.Text.RegularExpressions;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Modal : Layer, ISpecialCaseStyling, IHasBackgroundColor
    {
        private readonly HTMLElement _closeButton;
        protected readonly HTMLElement _modalHeader;
        protected readonly HTMLElement _modalFooter;
        protected readonly HTMLElement _modalOverlay;
        protected readonly HTMLElement _modalContent;

        private readonly HTMLElement _modalHeaderCommands;
        private readonly HTMLElement _modalFooterCommands;
        private readonly HTMLElement _modalHeaderContents;
        private readonly HTMLElement _modalFooterContents;

        private bool _isDragged;
        private TranslationPoint _startPoint;

        internal readonly HTMLElement _modal;

        public HTMLElement StylingContainer => _modal;

        public bool PropagateToStackItemParent => false;

        public delegate void OnHideHandler(Modal sender);

        public event OnHideHandler onHide;

        public string Background { get => _modal.style.background; set => _modal.style.background = value; }


        public Modal(IComponent header = null)
        {
            _modalHeaderContents = Div(_("tss-modal-header-content"));
            _modalFooterContents = Div(_("tss-modal-footer-content"));

            _modalHeaderCommands = Div(_("tss-modal-header-commands"));
            _modalFooterCommands = Div(_("tss-modal-footer-commands"));

            _modalHeader = Div(_("tss-modal-header"), _modalHeaderContents, _modalHeaderCommands);
            _modalFooter = Div(_("tss-modal-footer"), _modalFooterContents, _modalFooterCommands);

            if (header != null)
            {
                _modalHeaderContents.appendChild(header.Render());
            }
            else
            {
                _modalHeader.style.display = "none";
            }

            _closeButton  = Button(_("tss-modal-button las la-times", el: el => el.onclick = (e) => Hide()));
            _modalHeaderCommands.appendChild(_closeButton);

            _modalContent = Div(_("tss-modal-content"));
            _modal = Div(_("tss-modal", styles: s => s.transform = "translate(0px,0px)"), _modalHeader, _modalContent, _modalFooter);
            _modalOverlay = Div(_("tss-modal-overlay"));
            _contentHtml = Div(_("tss-modal-container"), _modalOverlay, _modal);
            IsNonBlocking = false; //blocking by default
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
            if(footer is object)
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

        public override IComponent Content
        {
            get { return _content; }
            set
            {
                if (value != _content)
                {
                    ClearChildren(_modalContent); ;
                    _content = value;
                    if (_content != null)
                    {
                        _modalContent.appendChild(_content.Render());
                    }
                }
            }
        }

        public bool CanLightDismiss
        {
            get { return _modalOverlay.classList.contains("tss-modal-lightDismiss"); }
            set
            {
                if (value != CanLightDismiss)
                {
                    if (value)
                    {
                        _modalOverlay.classList.add("tss-modal-lightDismiss");
                        _modalOverlay.addEventListener("click", OnCloseClick);
                    }
                    else
                    {
                        _modalOverlay.classList.remove("tss-modal-lightDismiss");
                        _modalOverlay.removeEventListener("click", OnCloseClick);
                    }
                }
            }
        }

        public bool IsDark
        {
            get { return _contentHtml.classList.contains("dark"); }
            set
            {
                if (value != IsDark)
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

        public bool IsDraggable
        {
            get { return _modal.classList.contains("tss-modal-draggable"); }
            set
            {
                if (value != IsDraggable)
                {
                    if (value)
                    {
                        _modal.classList.add("tss-modal-draggable");
                        _modal.addEventListener("mousedown", OnDragMouseDown);

                    }
                    else
                    {
                        _modal.classList.remove("tss-modal-draggable");
                        _modal.removeEventListener("mousedown", OnDragMouseDown);
                    }
                }
            }
        }

        public bool IsNonBlocking
        {
            get { return _contentHtml.classList.contains("tss-modal-modeless"); }
            set
            {
                if (value != IsNonBlocking)
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

        public Modal CenterContent()
        {
            _modalContent.classList.add("tss-modal-centered-content");
            return this;
        }

        public Modal NoTopBorder()
        {
            _modalHeader.classList.add("noborder");
            return this;
        }

        public void ShowAt(UnitSize fromTop = null, UnitSize fromLeft = null, UnitSize fromRight = null, UnitSize fromBottom = null)
        {
            _modal.style.marginTop    = fromTop    != null ? fromTop.ToString()    : UnitSize.Auto().ToString();
            _modal.style.marginLeft   = fromLeft   != null ? fromLeft.ToString()   : UnitSize.Auto().ToString();
            _modal.style.marginRight  = fromRight  != null ? fromRight.ToString()  : UnitSize.Auto().ToString();
            _modal.style.marginBottom = fromBottom != null ? fromBottom.ToString() : UnitSize.Auto().ToString();
            if (!IsNonBlocking) document.body.style.overflowY = "hidden";
            _modal.style.transform = "translate(0px,0px)";
            base.Show();
        }

        public override void Show()
        {
            _modal.style.marginTop = "";
            _modal.style.marginLeft = "";
            _modal.style.marginRight = "";
            _modal.style.marginBottom = "";
            _modal.style.transform = "translate(0px,0px)";
            if (!IsNonBlocking) document.body.style.overflowY = "hidden";
            base.Show();
        }

        public Modal OnHide(OnHideHandler onHide)
        {
            this.onHide += onHide;
            return this;
        }

        public override void Hide(Action onHidden = null)
        {
            onHide?.Invoke(this);

            base.Hide(() =>
            {
                if (!IsNonBlocking) document.body.style.overflowY = "";
                onHidden?.Invoke();
            });
        }

        protected override HTMLElement BuildRenderedContent()
        {
            return _contentHtml;
        }

        private void OnCloseClick(object ev)
        {
            Hide();
        }

        private void OnDragMouseMove(object ev)
        {
            if (_isDragged)
            {
                var e = ev as MouseEvent;
                _startPoint.X += e.movementX;
                _startPoint.Y += e.movementY;
                _modal.style.transform = _startPoint.To();
            }
        }

        private void OnDragMouseUp(object ev)
        {
            var e = ev as MouseEvent;
            if (_isDragged && e.button == 0)
            {
                document.body.removeEventListener("mouseup", OnDragMouseUp);
                document.body.removeEventListener("mousemove", OnDragMouseMove);
                document.body.removeEventListener("mouseleave", OnDragMouseUp);
                document.body.style.userSelect = "";
                _isDragged = false;
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
                _modal.style.userSelect = "none";
                _startPoint = TranslationPoint.From(_modal.style.transform);
                _isDragged = true;
            }
        }

        class TranslationPoint
        {
            static Regex regex = new Regex(@"translate\(([-0-9.].*?)px,\s?([-0-9.].*?)px\)");
            static Regex regexShort = new Regex(@"translate\(([-0-9.].*?)px\)");

            public TranslationPoint(double x = 0, double y = 0)
            {
                X = x;
                Y = y;
            }

            public double X { get; set; }
            public double Y { get; set; }

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

        public static T LightDismiss<T>(this T modal) where T : Modal
        {
            modal.CanLightDismiss = true;
            return modal;
        }

        public static T NoLightDismiss<T>(this T modal) where T : Modal
        {
            modal.CanLightDismiss = false;
            return modal;
        }

        public static T Dark<T>(this T modal) where T : Modal
        {
            modal.IsDark = true;
            return modal;
        }

        public static T Draggable<T>(this T modal) where T : Modal
        {
            modal.IsDraggable = true;
            return modal;
        }

        public static T NonBlocking<T>(this T modal) where T : Modal
        {
            modal.IsNonBlocking = true;
            return modal;
        }

        public static T Blocking<T>(this T modal) where T : Modal
        {
            modal.IsNonBlocking = false;
            return modal;
        }
    }
}
