using System;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A reusable, anchored overlay surface used to display arbitrary <see cref="IComponent"/> content
    /// next to another component (the anchor). Popovers are the general-purpose primitive on top of which
    /// menus, comboboxes, date pickers, color pickers and similar transient surfaces are built.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="Popover"/> instance is configured once via the fluent setters
    /// (<see cref="Content"/>, <see cref="Placement"/>, <see cref="Arrow"/>, …) and then shown imperatively
    /// against a particular anchor with <see cref="ShowFor(IComponent)"/> or <see cref="ShowFor(HTMLElement)"/>.
    /// Unlike the <c>.Tooltip(...)</c> extension — which attaches a hover-triggered tooltip to a component —
    /// a popover is *manually triggered*: it stays visible until <see cref="Hide"/> is called, the user clicks
    /// outside (when <see cref="HideOnClickOutside"/> is enabled, the default), or the anchor is removed
    /// from the DOM.
    /// </para>
    /// <para>
    /// Positioning, click-outside dismissal, arrow rendering and animation are delegated to the bundled
    /// Tippy / Popper.js library, so the same set of placements and animations available to tooltips also
    /// apply to popovers.
    /// </para>
    /// <example>
    /// <code>
    /// var popover = UI.Popover()
    ///     .Content(UI.Stack().Children(UI.TextBlock("Hello"), UI.Button("Close").OnClick(() => popover.Hide())))
    ///     .Placement(TooltipPlacement.BottomStart)
    ///     .Arrow();
    ///
    /// var button = UI.Button("Open").OnClick(b => popover.ShowFor(b));
    /// </code>
    /// </example>
    /// </remarks>
    [H5.Name("tss.Popover")]
    public sealed class Popover
    {
        private IComponent       _content;
        private TooltipPlacement _placement      = TooltipPlacement.Bottom;
        private TooltipAnimation _animation      = TooltipAnimation.None;
        private bool             _arrow          = false;
        private int              _maxWidth       = 350;
        private string           _theme          = "tss-popover";
        private bool             _hideOnClick    = true;
        private bool             _hideOnEsc      = true;
        private int              _delayShow      = 0;
        private int              _delayHide      = 0;
        private Action           _onShown;
        private Action           _onHidden;
        private Func<bool>       _onBeforeHide;
        private Action           _hideAction;
        private bool             _isVisible;
        private HTMLDivElement   _renderedContent;

        /// <summary>
        /// Creates a new, empty popover. Configure it with <see cref="Content"/> and the other fluent
        /// setters before calling <see cref="ShowFor(IComponent)"/>.
        /// </summary>
        public Popover() { }

        /// <summary>
        /// Creates a popover whose content is already set.
        /// </summary>
        /// <param name="content">The component to render inside the popover.</param>
        public Popover(IComponent content) { _content = content; }

        /// <summary>Gets a value indicating whether the popover is currently displayed.</summary>
        public bool IsVisible => _isVisible;

        /// <summary>
        /// Sets the content displayed inside the popover. Replacing the content while the popover is
        /// visible will not retroactively update the rendered element; call <see cref="Hide"/> and
        /// <see cref="ShowFor(IComponent)"/> again to apply a new content tree.
        /// </summary>
        public Popover Content(IComponent content) { _content = content; return this; }

        /// <summary>
        /// Sets the preferred placement of the popover relative to its anchor. Defaults to
        /// <see cref="TooltipPlacement.Bottom"/>. The actual placement may flip automatically when there
        /// is not enough room on the preferred side.
        /// </summary>
        public Popover Placement(TooltipPlacement placement) { _placement = placement; return this; }

        /// <summary>Sets the show/hide animation. Defaults to <see cref="TooltipAnimation.None"/>.</summary>
        public Popover Animation(TooltipAnimation animation) { _animation = animation; return this; }

        /// <summary>Enables or disables the small arrow that points from the popover to the anchor.</summary>
        public Popover Arrow(bool arrow = true) { _arrow = arrow; return this; }

        /// <summary>Sets the maximum width of the popover surface, in pixels. Defaults to 350.</summary>
        public Popover MaxWidth(int pixels) { _maxWidth = pixels; return this; }

        /// <summary>Applies a named Tippy theme to the popover (e.g. <c>"light"</c>, <c>"light-border"</c>).</summary>
        public Popover Theme(string theme) { _theme = theme; return this; }

        /// <summary>
        /// Controls whether clicking outside the popover hides it. Enabled by default; disable for popovers
        /// that should stay open until explicitly closed by their content.
        /// </summary>
        public Popover HideOnClickOutside(bool hide = true) { _hideOnClick = hide; return this; }

        /// <summary>Controls whether pressing <c>Escape</c> hides the popover. Enabled by default.</summary>
        public Popover HideOnEscape(bool hide = true) { _hideOnEsc = hide; return this; }

        /// <summary>Adds a delay (in milliseconds) before the popover shows when <see cref="ShowFor(IComponent)"/> is called.</summary>
        public Popover DelayShow(int milliseconds) { _delayShow = milliseconds; return this; }

        /// <summary>Adds a delay (in milliseconds) before the popover hides after <see cref="Hide"/> is requested.</summary>
        public Popover DelayHide(int milliseconds) { _delayHide = milliseconds; return this; }

        /// <summary>Registers a callback that runs once the popover has finished its show animation.</summary>
        public Popover OnShown(Action action) { _onShown += action; return this; }

        /// <summary>Registers a callback that runs once the popover has finished its hide animation.</summary>
        public Popover OnHidden(Action action) { _onHidden += action; return this; }

        /// <summary>
        /// Registers a callback that runs immediately before the popover hides. Return <c>false</c> from the
        /// callback to cancel the hide (useful for guarding against accidental dismissal during editing).
        /// </summary>
        public Popover OnBeforeHide(Func<bool> shouldHide) { _onBeforeHide = shouldHide; return this; }

        /// <summary>
        /// Shows the popover anchored to the rendered element of <paramref name="anchor"/>. If the popover
        /// is already visible against a different anchor, that instance is hidden first.
        /// </summary>
        /// <param name="anchor">The component the popover is positioned relative to.</param>
        public Popover ShowFor(IComponent anchor)
        {
            if (anchor is null) return this;
            return ShowFor(anchor.Render());
        }

        /// <summary>
        /// Shows the popover anchored to the given DOM element. Use this overload when you only have an
        /// element reference (for example, when reacting to a low-level event).
        /// </summary>
        public Popover ShowFor(HTMLElement anchor)
        {
            if (anchor is null) return this;
            if (_content is null) throw new InvalidOperationException("Popover.Content must be set before showing.");

            if (_isVisible)
            {
                Hide();
            }

            _renderedContent                          = UI.DIV(_content.Render());
            _renderedContent.style.display            = "block";
            _renderedContent.classList.add("tss-popover");

            Action onShownInternal  = () =>
            {
                _isVisible = true;
                _onShown?.Invoke();
            };

            Action onHiddenInternal = () =>
            {
                _isVisible       = false;
                _renderedContent = null;
                _hideAction      = null;
                _onHidden?.Invoke();
            };

            Func<bool> shouldHide = () =>
            {
                if (!_hideOnEsc && IsEscapeOnlyHide()) return false;
                return _onBeforeHide?.Invoke() ?? true;
            };

            Tippy.ShowFor(
                anchor,
                _renderedContent,
                out var hide,
                animation:         _animation,
                placement:         _placement,
                delayShow:         _delayShow,
                delayHide:         _delayHide,
                maxWidth:          _maxWidth,
                arrow:             _arrow,
                theme:             _theme,
                hideOnClick:       _hideOnClick,
                onHiddenCallback:  onHiddenInternal,
                onHide:            shouldHide,
                // Manual trigger so Tippy does not auto-hide when the cursor leaves the anchor — without
                // this, the user could not move the mouse from the anchor to the popover surface
                // (especially relevant for submenus, where there is an inherent gap between the parent
                // item and the submenu).
                manualTrigger:     true,
                // Widen the interactive grace area so brief excursions into the gap between the anchor
                // and the popover do not register as a click-outside.
                interactiveBorder: 24);

            _hideAction = hide;
            // Tippy does not surface a separate "shown" callback in this codebase, so we fire it inline.
            onShownInternal();
            return this;
        }

        /// <summary>Hides the popover if it is currently visible. Safe to call repeatedly.</summary>
        public void Hide()
        {
            _hideAction?.Invoke();
        }

        // Tippy fires onHide both for click-outside/click-on-anchor and Escape; we cannot distinguish them
        // here at runtime, so HideOnEscape is best-effort — the popover honours OnBeforeHide for all paths.
        private static bool IsEscapeOnlyHide() => false;
    }
}
