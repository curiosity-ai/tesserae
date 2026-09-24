using System;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A sheet that slides up from the bottom of the screen over a dimmed page - the phone's answer to a
    /// <see cref="Panel"/> or a <see cref="Modal"/>. It has a grab handle, an optional title with a close
    /// button, a scrolling content area and an optional footer, and it can be dragged down to dismiss.
    /// </summary>
    /// <remarks>
    /// A <see cref="Modal"/> that should become one of these on a phone does not need rebuilding:
    /// <see cref="Modal.DrawerOnMobile"/> renders the same modal inside a drawer while the page is in mobile mode.
    /// </remarks>
    [Transpose.Name("tss.Drawer")]
    public sealed class Drawer : Layer<Drawer>, IHasBackgroundColor, IBindableComponent<bool>
    {
        /// <summary>How long the sheet takes to slide in or out, in milliseconds. Kept in step with tss.drawer.css.</summary>
        public const int DRAWER_TRANSITION_TIME = 250;

        /// <summary>How far the sheet has to be dragged down, in pixels, before letting go dismisses it.</summary>
        private const int DRAG_TO_DISMISS_DISTANCE = 80;

        private event OnHideHandler Hidden;
        public delegate void        OnHideHandler(Drawer sender);

        private readonly SettableObservable<bool> _observable = new SettableObservable<bool>();

        private          IComponent  _footer;
        private readonly HTMLElement _drawer;
        private readonly HTMLElement _overlay;
        private readonly HTMLElement _header;
        private readonly HTMLElement _title;
        private readonly HTMLElement _closeButton;
        private readonly HTMLElement _drawerContent;
        private readonly HTMLElement _drawerFooter;

        private readonly Action<Event> _onOverlayClick;

        private bool   _dragToDismiss = true;
        private double _dragStartY    = -1;
        private double _dragOffset;
        private double _hideTimeout;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Drawer(string title = null) : this(string.IsNullOrEmpty(title) ? null : TextBlock(title).SemiBold()) { }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Drawer(IComponent title)
        {
            _onOverlayClick = _ => Hide();

            var handle = Div(Att("tss-drawer-handle"), Div(Att("tss-drawer-handle-bar")));

            _title         = Div(Att("tss-drawer-title"));
            _closeButton   = Button(Att("tss-drawer-close", type: "button", ariaLabel: "Close", el: el => el.onclick = _ => Hide()), I(Att("tss-fontsize-small " + UIcons.Cross.ToCssClass())));
            _header        = Div(Att("tss-drawer-header"), _title, _closeButton);
            _drawerContent = Div(Att("tss-drawer-content"));
            _drawerFooter  = Div(Att("tss-drawer-footer tss-drawer-footer-empty"));
            _drawer        = Div(Att("tss-drawer", role: "dialog"), handle, _header, _drawerContent, _drawerFooter);
            _overlay       = Div(Att("tss-drawer-overlay"));
            _contentHtml   = Div(Att("tss-drawer-container"), _overlay, _drawer);

            _drawer.setAttribute("aria-modal", "true");

            SetTitle(title);
            CanLightDismiss = true;

            HookDragToDismiss(handle);
            HookDragToDismiss(_header);
        }

        /// <summary>
        /// Sets the content rendered inside the sheet.
        /// </summary>
        public override IComponent Content
        {
            get => _content;
            set
            {
                ClearChildren(_drawerContent);
                _content = value;

                if (_content != null)
                {
                    _drawerContent.appendChild(_content.Render());
                }
            }
        }

        /// <summary>
        /// Gets or sets the footer of the sheet, drawn below the scrolling content.
        /// </summary>
        public IComponent Footer
        {
            get => _footer;
            set
            {
                ClearChildren(_drawerFooter);
                _footer = value;

                if (_footer != null)
                {
                    _drawerFooter.appendChild(_footer.Render());
                }

                _drawerFooter.UpdateClassIf(_footer is null, "tss-drawer-footer-empty");
            }
        }

        /// <summary>
        /// Gets or sets whether tapping the dimmed page around the sheet closes it. True by default: a sheet is
        /// something the reader looks at and puts away.
        /// </summary>
        public bool CanLightDismiss
        {
            get => _overlay.classList.contains("tss-drawer-lightDismiss");
            set
            {
                _overlay.removeEventListener("click", _onOverlayClick);
                _overlay.UpdateClassIf(value, "tss-drawer-lightDismiss");

                if (value) _overlay.addEventListener("click", _onOverlayClick);
            }
        }

        /// <summary>
        /// Gets or sets whether the close button is shown.
        /// </summary>
        public bool ShowCloseButton
        {
            get => _closeButton.style.display != "none";
            set => _closeButton.style.display = value ? "" : "none";
        }

        /// <summary>
        /// Gets or sets the CSS background of the sheet.
        /// </summary>
        public string Background { get => _drawer.style.background; set => _drawer.style.background = value; }

        /// <summary>
        /// A drawer blocks the page behind it.
        /// </summary>
        protected override bool LocksPageScroll => true;

        protected override HTMLElement BuildRenderedContent()
        {
            var trap = new FocusTrap(_contentHtml);
            trap.TrapEscape(() => Hide());
            return trap.Render();
        }

        /// <summary>
        /// Sets the title shown beside the close button. Null leaves the header with only the button.
        /// </summary>
        public Drawer SetTitle(IComponent title)
        {
            ClearChildren(_title);

            if (title is object)
            {
                _title.appendChild(title.Render());
            }

            return this;
        }

        /// <summary>
        /// Sets the title shown beside the close button.
        /// </summary>
        public Drawer SetTitle(string title) => SetTitle(string.IsNullOrEmpty(title) ? null : TextBlock(title).SemiBold());

        /// <summary>
        /// Sets the footer of the sheet.
        /// </summary>
        public Drawer SetFooter(IComponent footer)
        {
            Footer = footer;
            return this;
        }

        /// <summary>
        /// Caps the sheet's height, as a CSS size. It is 85% of the screen by default, so the page it covers is
        /// always still in sight above it; the sheet is otherwise as tall as its content.
        /// </summary>
        public Drawer MaxHeight(UnitSize height)
        {
            _drawer.style.maxHeight = height.ToString();
            return this;
        }

        /// <summary>
        /// Makes the sheet as tall as <see cref="MaxHeight"/> allows, whatever its content - for content that
        /// grows while it is open (a list still loading), which would otherwise make the sheet jump.
        /// </summary>
        public Drawer FullHeight()
        {
            _drawer.classList.add("tss-drawer-full-height");
            return this;
        }

        /// <summary>
        /// Leaves out the title row, for content that brings its own header - the sheet is then held by its
        /// handle alone.
        /// </summary>
        public Drawer NoHeader()
        {
            _header.style.display = "none";
            return this;
        }

        /// <summary>
        /// Removes the padding around the content.
        /// </summary>
        public Drawer NoContentPadding()
        {
            _drawerContent.classList.add("tss-drawer-content-no-padding");
            return this;
        }

        /// <summary>
        /// Hides the close button. The sheet can still be dragged down, and dismissed by tapping outside it.
        /// </summary>
        public Drawer HideCloseButton()
        {
            ShowCloseButton = false;
            return this;
        }

        /// <summary>
        /// Enables light-dismiss behaviour (tapping outside the sheet closes it). On by default.
        /// </summary>
        public Drawer LightDismiss()
        {
            CanLightDismiss = true;
            return this;
        }

        /// <summary>
        /// Disables light-dismiss behaviour, for a sheet that asks for a decision.
        /// </summary>
        public Drawer NoLightDismiss()
        {
            CanLightDismiss = false;
            return this;
        }

        /// <summary>
        /// Gets or sets whether pulling the sheet down by its handle dismisses it. True by default.
        /// </summary>
        public bool CanDragToDismiss
        {
            get => _dragToDismiss;
            set
            {
                _dragToDismiss = value;
                _contentHtml.UpdateClassIf(!value, "tss-drawer-no-drag");
            }
        }

        /// <summary>
        /// Stops the sheet being dismissed by pulling it down, for one that asks for a decision.
        /// </summary>
        public Drawer NoDragToDismiss()
        {
            CanDragToDismiss = false;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the sheet is hidden.
        /// </summary>
        public Drawer OnHide(OnHideHandler onHide)
        {
            Hidden += onHide;
            return this;
        }

        /// <summary>
        /// Shows the sheet, sliding it up from the bottom.
        /// </summary>
        public override Drawer Show()
        {
            window.clearTimeout(_hideTimeout);

            ResetDrag();
            _contentHtml.classList.remove("tss-drawer-open");

            var result = base.Show();

            // The sheet has to be laid out below the screen before it is moved up, or there is nothing to animate
            window.requestAnimationFrame(_ => window.requestAnimationFrame(__ => _contentHtml.classList.add("tss-drawer-open")));

            _observable.Value = true;
            return result;
        }

        /// <summary>
        /// Hides the sheet, sliding it back down.
        /// </summary>
        public override void Hide(Action onHidden = null)
        {
            if (!IsVisible) return;

            _contentHtml.classList.remove("tss-drawer-open");
            _drawer.style.transform = "";

            Hidden?.Invoke(this);
            _observable.Value = false;

            // The layer's own fade would take the sheet away before it has slid out of sight
            window.clearTimeout(_hideTimeout);
            _hideTimeout = window.setTimeout(_ => base.Hide(onHidden), DRAWER_TRANSITION_TIME);
        }

        /// <summary>
        /// Returns an observable that tracks the visibility of the sheet.
        /// </summary>
        public IObservable<bool> AsObservable() => _observable;

        /// <summary>
        /// Programmatically shows or hides the sheet as part of a two-way binding.
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

        // The handle and the header are what a thumb pulls the sheet down by; the content scrolls instead.
        private void HookDragToDismiss(HTMLElement grip)
        {
            //A vertical drag on the grip is the sheet's, not a scroll of the page under it
            grip.style.touchAction = "none";

            grip.addEventListener("pointerdown", (Action<Event>)(e =>
            {
                if (!_dragToDismiss || e.target.As<HTMLElement>().closest(".tss-drawer-close") is object) return;

                _dragStartY = e.As<MouseEvent>().clientY;
                _dragOffset = 0;
                _drawer.classList.add("tss-drawer-dragging");
                Script.Write("try { {0}.setPointerCapture({1}.pointerId); } catch (x) { }", grip, e);
            }));

            grip.addEventListener("pointermove", (Action<Event>)(e =>
            {
                if (_dragStartY < 0) return;

                _dragOffset             = Math.Max(0, e.As<MouseEvent>().clientY - _dragStartY);
                _drawer.style.transform = $"translateY({_dragOffset}px)";
            }));

            Action<Event> release = e =>
            {
                if (_dragStartY < 0) return;

                var dismiss = _dragOffset > DRAG_TO_DISMISS_DISTANCE;

                ResetDrag();

                if (dismiss) Hide();
            };

            grip.addEventListener("pointerup",     release);
            grip.addEventListener("pointercancel", release);
        }

        private void ResetDrag()
        {
            _dragStartY = -1;
            _dragOffset = 0;
            _drawer.classList.remove("tss-drawer-dragging");
            _drawer.style.transform = "";
        }
    }
}
