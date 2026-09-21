using System;
using TNT;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The row at the top of a rail that says what the application is: a logo, its name, and an optional
    /// second line for whatever qualifies that name - the workspace, the tenant, the environment. It is the
    /// same shape and height as <see cref="SidebarProfile"/>, so a rail bracketed by the two reads as one
    /// piece of chrome.
    /// <para>
    /// It can also be the one control that collapses and expands the rail - see
    /// <see cref="WithSidebarControl"/>. On the collapsed rail the row is the logo alone, with the name (and
    /// the second line) in its tooltip.
    /// </para>
    /// </summary>
    public class SidebarBrand : SidebarIdentityRow<SidebarBrand>
    {
        private Button _closedControl;

        /// <summary>
        /// Initializes a new instance of the SidebarBrand class, with a logo loaded from a URL.
        /// </summary>
        /// <param name="identifier">The identifier for the item.</param>
        /// <param name="title">The application's name.</param>
        /// <param name="subtitle">What qualifies the name - the workspace, the tenant, the environment. None leaves the row a single line.</param>
        /// <param name="logoUrl">The logo's URL. None leaves the row without one.</param>
        public SidebarBrand(string identifier, string title, string subtitle = null, string logoUrl = null)
            : this(identifier, LogosFor(logoUrl), title, subtitle) { }

        /// <summary>
        /// Initializes a new instance of the SidebarBrand class, with a glyph in place of a logo.
        /// </summary>
        /// <param name="identifier">The identifier for the item.</param>
        /// <param name="icon">The glyph.</param>
        /// <param name="title">The application's name.</param>
        /// <param name="subtitle">What qualifies the name. None leaves the row a single line.</param>
        /// <param name="weight">The glyph's weight.</param>
        public SidebarBrand(string identifier, UIcons icon, string title, string subtitle = null, UIconsWeight weight = UIconsWeight.Regular)
            : this(identifier, new IComponent[] { Icon(icon, weight, TextSize.Medium), Icon(icon, weight, TextSize.Medium) }, title, subtitle) { }

        /// <summary>
        /// Initializes a new instance of the SidebarBrand class, with an emoji in place of a logo.
        /// </summary>
        /// <param name="identifier">The identifier for the item.</param>
        /// <param name="icon">The emoji.</param>
        /// <param name="title">The application's name.</param>
        /// <param name="subtitle">What qualifies the name. None leaves the row a single line.</param>
        public SidebarBrand(string identifier, Emoji icon, string title, string subtitle = null)
            : this(identifier, new IComponent[] { Icon(icon, TextSize.Medium), Icon(icon, TextSize.Medium) }, title, subtitle) { }

        /// <summary>
        /// Initializes a new instance of the SidebarBrand class, with a logo of the caller's own.
        /// </summary>
        /// <param name="identifier">The identifier for the item.</param>
        /// <param name="logo">The logo. It is cloned for the collapsed rail, since one element cannot be in two places.</param>
        /// <param name="title">The application's name.</param>
        /// <param name="subtitle">What qualifies the name. None leaves the row a single line.</param>
        public SidebarBrand(string identifier, ISidebarIcon logo, string title, string subtitle = null)
            : this(identifier, new IComponent[] { logo, logo.Clone() }, title, subtitle) { }

        private SidebarBrand(string identifier, IComponent[] logos, string title, string subtitle)
            : base(identifier, "tss-sidebar-brand", WrapLogo(logos[0]), WrapLogo(logos[1]), title, subtitle) { }

        /// <summary>
        /// A box of its own around whatever the logo is, so that an image, a glyph and an emoji all take the
        /// same square beside the name - the stylesheet sizes the box rather than each of the three.
        /// </summary>
        private static IComponent WrapLogo(IComponent logo)
        {
            return Raw(Div(Att("tss-sidebar-brand-logo"), logo.Render()));
        }

        private static IComponent[] LogosFor(string logoUrl)
        {
            if (string.IsNullOrWhiteSpace(logoUrl)) return new IComponent[] { Empty(), Empty() };

            return new IComponent[] { new ImageIcon(logoUrl), new ImageIcon(logoUrl) };
        }

        /// <inheritdoc/>
        protected override SidebarBrand Self => this;

        /// <summary>
        /// Sets the application's name.
        /// </summary>
        /// <param name="title">The name.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarBrand SetTitle(string title)
        {
            SetTitleText(title);
            return this;
        }

        /// <summary>
        /// Adds the command that opens the application's configuration.
        /// </summary>
        /// <param name="onClick">What pressing it does.</param>
        /// <param name="tooltip">The tooltip. None uses "Settings".</param>
        /// <param name="icon">The icon. Defaults to a gear.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarBrand Configure(Action onClick, string tooltip = null, UIcons icon = UIcons.Settings)
        {
            return Configure(new SidebarCommand(icon).Tooltip(string.IsNullOrWhiteSpace(tooltip) ? "Settings".t() : tooltip).OnClick(onClick));
        }

        /// <summary>
        /// Sets the configuration command, built by the caller - for one that opens a menu
        /// (<see cref="SidebarCommand.OnClickMenu"/>) rather than a page. Passing null removes it.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarBrand Configure(SidebarCommand command)
        {
            SetPrimaryCommand(command);
            return this;
        }

        /// <summary>
        /// Makes the brand the rail's own open/close control, which is where an application that has one
        /// usually wants it: at the top, on the row that is there in both states.
        /// <para>
        /// While the sidebar is open it is the last command on the row, so closing the rail is the rightmost
        /// thing on it. While the sidebar is closed there is no room for a command beside the logo, so the
        /// logo <em>is</em> the control: it stands as the brand at rest and turns into the open button under
        /// the pointer.
        /// </para>
        /// <para>
        /// The row reports the two intentions rather than driving a sidebar itself, because the rail a brand
        /// sits on is not always the one it opens - an application shifts between rails, and rebuilds them -
        /// so the caller is the one that knows which <see cref="Sidebar"/> to toggle and what else follows
        /// from it.
        /// </para>
        /// </summary>
        /// <param name="onOpen">What opening the rail does. Called from the control the collapsed rail shows.</param>
        /// <param name="onClose">What closing it does. Called from the last command on the open row.</param>
        /// <param name="openTooltip">The tooltip on the open control. None uses "Open Sidebar".</param>
        /// <param name="closeTooltip">The tooltip on the close command. None uses "Close Sidebar".</param>
        /// <param name="openIcon">The icon of the open control. The rail's own glyph, mirrored.</param>
        /// <param name="closeIcon">The icon of the close command. The rail's own glyph.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarBrand WithSidebarControl(
            Action onOpen,
            Action onClose,
            string openTooltip  = null,
            string closeTooltip = null,
            UIcons openIcon     = UIcons.SidebarFlip,
            UIcons closeIcon    = UIcons.Sidebar)
        {
            SetSecondaryCommand(new SidebarCommand(closeIcon)
               .Tooltip(string.IsNullOrWhiteSpace(closeTooltip) ? "Close Sidebar".t() : closeTooltip)
               .OnClick(onClose));

            _closedControl?.Render().remove();

            _closedControl = Button()
               .SetIcon(openIcon)
               .Class("tss-sidebar-btn")
               .Class("tss-sidebar-identity-closed-control")
               .Tooltip(string.IsNullOrWhiteSpace(openTooltip) ? "Open Sidebar".t() : openTooltip, placement: TooltipPlacement.Right)
               .OnClick(onOpen);

            ClosedRoot.appendChild(_closedControl.Render());
            ClosedRoot.classList.add("tss-sidebar-identity-has-closed-control");

            return this;
        }
    }
}
