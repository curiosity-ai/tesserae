using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The tone a <see cref="Banner"/> is drawn in - the same set of tones a <see cref="Button"/> has, so a
    /// banner and the button that answers it can be told to mean the same thing.
    /// </summary>
    [Transpose.Name("tss.BannerStyle")]
    public enum BannerStyle
    {
        /// <summary>The neutral tone: something the user should know, drawn in the page's own colors.</summary>
        Secondary,

        /// <summary>The accent tone: something worth pointing at, drawn in the theme's primary color.</summary>
        Primary,

        /// <summary>Something that went right.</summary>
        Success,

        /// <summary>Something that needs care but hasn't failed.</summary>
        Warning,

        /// <summary>Something that failed, or that will if it is left alone.</summary>
        Danger
    }

    /// <summary>
    /// A one-line-or-two notice strip: an <see cref="IconTile"/>, a title with an optional badge, a message
    /// under it, an action at the far end and a dismiss button after that.
    /// <para>
    /// A banner is a plain <see cref="IComponent"/>, so it renders inline anywhere - at the top of a page, in
    /// a card, above a list. The same banner is also what <see cref="Toast"/> shows: pass one to
    /// <see cref="Toast.Show(Banner)"/> and it is floated over the page instead, with its dismiss button
    /// hooked to the toast's own hiding.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// Banner("3 items need your review", "They were flagged as high priority and are waiting in your queue.")
    ///    .Danger()
    ///    .SetIcon(UIcons.Fire)
    ///    .SetBadge("Priority")
    ///    .Action("Review now", () =&gt; OpenQueue())
    ///    .OnDismiss(() =&gt; Remember("queue-banner-dismissed"));
    /// </code>
    /// </example>
    [Transpose.Name("tss.Banner")]
    public sealed class Banner : ComponentBase<Banner, HTMLElement>
    {
        private readonly HTMLElement _iconContainer;
        private readonly HTMLElement _contentContainer;
        private readonly HTMLElement _headerContainer;
        private readonly HTMLElement _titleContainer;
        private readonly HTMLElement _badgeContainer;
        private readonly HTMLElement _textContainer;
        private readonly HTMLElement _actionContainer;
        private readonly HTMLElement _dismissContainer;

        private IconTile    _iconTile;
        private BannerStyle _style        = BannerStyle.Secondary;
        private bool        _hostSetIcon  = false;
        private bool        _iconRemoved  = false;

        /// <summary>
        /// Initializes a new instance of this class, with the given title and message - either of which may
        /// be left out.
        /// </summary>
        public Banner(string title = null, string message = null)
        {
            _iconContainer    = Div(Att("tss-banner-icon"));
            _titleContainer   = Div(Att("tss-banner-title"));
            _badgeContainer   = Div(Att("tss-banner-badge"));
            _headerContainer  = Div(Att("tss-banner-header"), _titleContainer, _badgeContainer);
            _textContainer    = Div(Att("tss-banner-text"));
            _contentContainer = Div(Att("tss-banner-content"), _headerContainer, _textContainer);
            _actionContainer  = Div(Att("tss-banner-action"));
            _dismissContainer = Div(Att("tss-banner-dismiss"));

            InnerElement = Div(Att("tss-banner"), _iconContainer, _contentContainer, _actionContainer, _dismissContainer);

            SetTitle(title);
            SetText(message);
            SetBadge((string)null);

            Style(BannerStyle.Secondary);
        }

        /// <summary>
        /// Gets the handler the dismiss button runs, if one was set - so a host that wraps a banner (as
        /// <see cref="Toast"/> does) can chain its own hiding onto whatever the caller already asked for.
        /// </summary>
        internal Action DismissHandler { get; private set; }

        /// <summary>
        /// Gets whether dismissing takes the banner out of the DOM itself, which it does unless
        /// <see cref="OnDismiss(Action, bool)"/> said otherwise.
        /// </summary>
        internal bool HidesOnDismiss { get; private set; } = true;

        /// <summary>
        /// Gets the tone the banner is drawn in.
        /// </summary>
        public BannerStyle CurrentStyle => _style;

        /// <summary>
        /// Draws the banner in the given tone, and - unless the host has put an icon on the tile itself -
        /// puts that tone's own icon on it.
        /// </summary>
        public Banner Style(BannerStyle style)
        {
            _style = style;

            InnerElement.classList.remove("tss-banner-secondary", "tss-banner-primary", "tss-banner-success", "tss-banner-warning", "tss-banner-danger");
            InnerElement.classList.add(ClassFor(style));

            if (!_hostSetIcon && !_iconRemoved) ApplyDefaultIcon();

            return this;
        }

        /// <summary>Draws the banner in the neutral tone.</summary>
        public Banner Secondary() => Style(BannerStyle.Secondary);

        /// <summary>Draws the banner in the theme's primary color.</summary>
        public Banner Primary() => Style(BannerStyle.Primary);

        /// <summary>Draws the banner in the success tone.</summary>
        public Banner Success() => Style(BannerStyle.Success);

        /// <summary>Draws the banner in the warning tone.</summary>
        public Banner Warning() => Style(BannerStyle.Warning);

        /// <summary>Draws the banner in the danger tone.</summary>
        public Banner Danger() => Style(BannerStyle.Danger);

        /// <summary>
        /// Sets the bold first line. A null or empty title drops the line entirely, leaving the message to
        /// stand on its own.
        /// </summary>
        public Banner SetTitle(string title)
        {
            var isEmpty = string.IsNullOrEmpty(title);

            ClearChildren(_titleContainer);

            if (!isEmpty) _titleContainer.appendChild(Span(Att("tss-banner-title-text", text: title)));

            _titleContainer.style.display = isEmpty ? "none" : "";

            return UpdateHeaderVisibility();
        }

        /// <summary>
        /// Puts the given component on the first line in place of plain text.
        /// </summary>
        public Banner SetTitle(IComponent title)
        {
            ClearChildren(_titleContainer);

            if (title != null) _titleContainer.appendChild(title.Render());

            _titleContainer.style.display = title is null ? "none" : "";

            return UpdateHeaderVisibility();
        }

        /// <summary>
        /// Sets the message under the title. A null or empty message drops the line.
        /// </summary>
        public Banner SetText(string text)
        {
            var isEmpty = string.IsNullOrEmpty(text);

            ClearChildren(_textContainer);

            if (!isEmpty) _textContainer.appendChild(Span(Att("tss-banner-text-content", text: text)));

            _textContainer.style.display = isEmpty ? "none" : "";

            return this;
        }

        /// <summary>
        /// Puts the given component under the title in place of plain text.
        /// </summary>
        public Banner SetText(IComponent text)
        {
            ClearChildren(_textContainer);

            if (text != null) _textContainer.appendChild(text.Render());

            _textContainer.style.display = text is null ? "none" : "";

            return this;
        }

        /// <summary>
        /// Puts a badge beside the title - the reference the notice is about, what raised it. A null or
        /// empty value hides it.
        /// </summary>
        public Banner SetBadge(string text)
        {
            var isEmpty = string.IsNullOrEmpty(text);

            ClearChildren(_badgeContainer);

            if (!isEmpty) _badgeContainer.appendChild(Badge(text).Pill().Class("tss-banner-badge-pill").Render());

            _badgeContainer.style.display = isEmpty ? "none" : "";

            return UpdateHeaderVisibility();
        }

        /// <summary>
        /// Puts the given component beside the title in place of the plain badge.
        /// </summary>
        public Banner SetBadge(IComponent badge)
        {
            ClearChildren(_badgeContainer);

            if (badge != null) _badgeContainer.appendChild(badge.Render());

            _badgeContainer.style.display = badge is null ? "none" : "";

            return UpdateHeaderVisibility();
        }

        /// <summary>
        /// Puts the given icon on the leading tile. Without a color the tile takes the banner's own tone;
        /// pass one to say something the tone doesn't.
        /// </summary>
        public Banner SetIcon(UIcons icon, string color = null, UIconsWeight weight = UIconsWeight.Regular)
        {
            _hostSetIcon = true;
            _iconRemoved = false;

            EnsureIconTile().SetIcon(icon, color, weight);

            return TintTileWithStyle(color);
        }

        /// <summary>
        /// Puts a few letters on the leading tile in place of a glyph - a code, a count, a file type.
        /// </summary>
        public Banner SetIcon(string text, string color = null, TextSize? size = null)
        {
            _hostSetIcon = true;
            _iconRemoved = false;

            EnsureIconTile().SetIcon(text, color, size);

            return TintTileWithStyle(color);
        }

        /// <summary>
        /// Puts the given component on the leading tile - an <see cref="Image"/>, an <see cref="Avatar"/>,
        /// a <see cref="Spinner"/>. A null value takes the tile away, same as <see cref="NoIcon"/>.
        /// </summary>
        public Banner SetIcon(IComponent iconOrImage, string color = null)
        {
            if (iconOrImage is null) return NoIcon();

            _hostSetIcon = true;
            _iconRemoved = false;

            EnsureIconTile().SetIcon(iconOrImage, color);

            return TintTileWithStyle(color);
        }

        /// <summary>
        /// Drops the leading tile, for a banner whose tone already says everything the icon would.
        /// </summary>
        public Banner NoIcon()
        {
            _iconRemoved = true;
            _hostSetIcon = false;

            ClearChildren(_iconContainer);

            _iconTile                     = null;
            _iconContainer.style.display  = "none";

            return this;
        }

        /// <summary>
        /// Puts a button at the far end of the banner running the given handler - drawn in the banner's own
        /// tone, so the action reads as part of the notice rather than beside it.
        /// </summary>
        public Banner Action(string text, Action onClick)
        {
            var button = Button(text).OnClick(() => onClick?.Invoke()).Class("tss-banner-action-button");

            switch (_style)
            {
                case BannerStyle.Primary: button.Primary(); break;
                case BannerStyle.Success: button.Success(); break;
                case BannerStyle.Danger:  button.Danger();  break;
                case BannerStyle.Warning: button.Class("tss-banner-action-warning"); break;
            }

            return Action(button);
        }

        /// <summary>
        /// Puts the given component at the far end of the banner - a button of the host's own, a link, a
        /// pair of them. A null value empties the slot.
        /// </summary>
        public Banner Action(IComponent action)
        {
            ClearChildren(_actionContainer);

            if (action != null) _actionContainer.appendChild(action.Render());

            _actionContainer.style.display = action is null ? "none" : "";

            return this;
        }

        /// <summary>
        /// Shows a dismiss button after the action and runs the given handler when it is pressed. The banner
        /// takes itself out of the page as well unless <paramref name="hide"/> says not to - which is what a
        /// host that removes the banner some other way (<see cref="Toast"/>) passes.
        /// </summary>
        public Banner OnDismiss(Action onDismiss, bool hide = true)
        {
            DismissHandler = onDismiss;
            HidesOnDismiss = hide;

            ClearChildren(_dismissContainer);

            if (onDismiss is null && !hide)
            {
                _dismissContainer.style.display = "none";
                return this;
            }

            _dismissContainer.style.display = "";

            _dismissContainer.appendChild(
                Button()
                   .SetIcon(UIcons.Cross)
                   .NoBorder().NoMinSize().NoPadding()
                   .Tooltip("Dismiss")
                   .Class("tss-banner-dismiss-button")
                   .OnClick((_, e) =>
                    {
                        StopEvent(e);
                        Dismiss();
                    })
                   .Render());

            return this;
        }

        /// <summary>
        /// Dismisses the banner as though its dismiss button had been pressed: the handler set by
        /// <see cref="OnDismiss(Action, bool)"/> runs, and the banner takes itself out of the page.
        /// </summary>
        public Banner Dismiss()
        {
            DismissHandler?.Invoke();

            if (HidesOnDismiss && InnerElement.parentElement is object)
            {
                InnerElement.parentElement.removeChild(InnerElement);
            }

            return this;
        }

        /// <summary>
        /// Tightens the banner, for one that sits inside something small.
        /// </summary>
        public Banner Compact(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-banner-compact");

            return this;
        }

        /// <summary>
        /// Drops the rounded corners and the border, for a banner pinned edge to edge across a page.
        /// </summary>
        public Banner Flat(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-banner-flat");

            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;

        private static string ClassFor(BannerStyle style)
        {
            switch (style)
            {
                case BannerStyle.Primary: return "tss-banner-primary";
                case BannerStyle.Success: return "tss-banner-success";
                case BannerStyle.Warning: return "tss-banner-warning";
                case BannerStyle.Danger:  return "tss-banner-danger";
                default:                  return "tss-banner-secondary";
            }
        }

        private static UIcons IconFor(BannerStyle style)
        {
            switch (style)
            {
                case BannerStyle.Success: return UIcons.CheckCircle;
                case BannerStyle.Warning: return UIcons.TriangleWarning;
                case BannerStyle.Danger:  return UIcons.CircleXmark;
                default:                  return UIcons.Info;
            }
        }

        private void ApplyDefaultIcon()
        {
            EnsureIconTile().SetIcon(IconFor(_style));

            TintTileWithStyle(null);
        }

        private IconTile EnsureIconTile()
        {
            if (_iconTile is null)
            {
                _iconTile = new IconTile();

                _iconContainer.appendChild(_iconTile.Render());
            }

            _iconContainer.style.display = "";

            return _iconTile;
        }

        // Without an explicit color the tile is painted from the banner's tone in CSS, which keeps it right
        // through a theme switch; with one, the tile's own inline tint takes over.
        private Banner TintTileWithStyle(string color)
        {
            _iconTile.Render().UpdateClassIf(string.IsNullOrEmpty(color), "tss-banner-icon-tile");

            return this;
        }

        private Banner UpdateHeaderVisibility()
        {
            var isEmpty = _titleContainer.style.display == "none" && _badgeContainer.style.display == "none";

            _headerContainer.style.display = isEmpty ? "none" : "";

            return this;
        }
    }
}
