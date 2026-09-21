using System;
using System.Collections.Generic;
using Transpose.Core;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// What a <see cref="SidebarBrand"/> and a <see cref="SidebarProfile"/> have in common: a row twice the
    /// height of an ordinary one, carrying a picture, a name, an optional second line under it, and the
    /// commands that belong to whatever the row names.
    /// <para>
    /// The two exist as components rather than as a <see cref="SidebarButton"/> dressed up because the shape
    /// is different - two lines of text against a picture, taller than the rows around it, with the commands
    /// drawn at rest rather than under the pointer. Composing that out of a button and a stack got every
    /// application as far as a stylesheet of its own overriding the rail's heights, paddings and icon sizes,
    /// which is what this replaces.
    /// </para>
    /// <para>
    /// On the collapsed rail both are the picture alone, with the name (and the second line) in the tooltip.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The concrete row type, so the fluent methods return it.</typeparam>
    public abstract class SidebarIdentityRow<T> : ISidebarItem where T : SidebarIdentityRow<T>
    {
        private readonly HTMLSpanElement          _titleSpan;
        private readonly HTMLSpanElement          _subtitleSpan;
        private readonly Button                   _openButton;
        private readonly Button                   _closedButton;
        private readonly HTMLElement              _openRoot;
        private readonly HTMLElement              _closedRoot;
        private readonly HTMLElement              _openOuter;
        private readonly HTMLElement              _closedOuter;
        private readonly HTMLElement              _commandsContainer;
        private readonly IComponent               _open;
        private readonly IComponent               _closed;
        private readonly SettableObservable<bool> _selected;

        private SidebarCommand[] _extraCommands = new SidebarCommand[0];
        private SidebarCommand   _primaryCommand;
        private SidebarCommand   _secondaryCommand;
        private Action<Button>   _tooltipClosed;
        private string           _title;
        private string           _subtitle;

        private event Action<HTMLElement> _onRendered;

        /// <param name="identifier">The identifier for the item.</param>
        /// <param name="rowClass">The class naming the concrete row, e.g. <c>tss-sidebar-profile</c>.</param>
        /// <param name="openLeading">The picture shown while the sidebar is open.</param>
        /// <param name="closedLeading">The picture shown on the collapsed rail. A second instance, since one element cannot be in two places.</param>
        /// <param name="title">The name on the first line.</param>
        /// <param name="subtitle">The second line, or null for a single-line row.</param>
        protected SidebarIdentityRow(string identifier, string rowClass, IComponent openLeading, IComponent closedLeading, string title, string subtitle)
        {
            Identifier = identifier;
            _title     = title    ?? string.Empty;
            _subtitle  = subtitle ?? string.Empty;
            _selected  = new SettableObservable<bool>(false);

            _titleSpan    = Span(Att("tss-sidebar-identity-title",    text: _title));
            _subtitleSpan = Span(Att("tss-sidebar-identity-subtitle", text: _subtitle));

            var lines   = Div(Att("tss-sidebar-identity-lines"), _titleSpan, _subtitleSpan);
            var content = Div(Att("tss-sidebar-identity-content"), openLeading.Render(), lines);

            _openButton   = Button().ReplaceContent(Raw(content)).Class("tss-sidebar-btn").Class("tss-sidebar-identity-button").Id(identifier);
            _closedButton = Button().ReplaceContent(closedLeading).Class("tss-sidebar-btn").Class("tss-sidebar-identity-button-closed").Id(identifier);

            _openRoot          = Div(Att($"tss-sidebar-btn-open tss-sidebar-identity {rowClass}"), _openButton.Render());
            _commandsContainer = Div(Att("tss-sidebar-commands"));
            _openRoot.appendChild(_commandsContainer);

            _closedRoot = Div(Att($"tss-sidebar-identity-closed {rowClass}-closed"), _closedButton.Render());

            //The row is wrapped, and everything a caller adds around it - the divider and the margins
            //.Separated() draws, a class of the caller's own - goes on the wrapper rather than on the row.
            //The command strip is positioned against the row, so padding on the same element would push
            //the strip off the button's centre and past its right edge: a divider under the brand moved
            //the gear down by half the padding and out over the rail's gutter.
            _openOuter   = Div(Att($"tss-sidebar-identity-row {rowClass}-row"),                                 _openRoot);
            _closedOuter = Div(Att($"tss-sidebar-identity-row tss-sidebar-identity-row-closed {rowClass}-row"), _closedRoot);

            _open   = Raw(_openOuter);
            _closed = Raw(_closedOuter);

            //Chrome rather than something the pointer brings in: a command that only appears on hover is one
            //nobody finds, and the row is tall enough to lay the name out beside the strip.
            _openRoot.classList.add("tss-sidebar-commands-always-open");

            UpdateSubtitleVisibility();
            RefreshDefaultTooltip();

            _selected.Observe(isSelected =>
            {
                _closedRoot.UpdateClassIf(isSelected, "tss-sidebar-selected");
                _openRoot.UpdateClassIf(isSelected,   "tss-sidebar-selected");
            });
        }

        /// <summary>The concrete row, so that a fluent method defined here returns the type it was called on.</summary>
        protected abstract T Self { get; }

        /// <summary>The button the open row draws, for a subclass that has to reach it.</summary>
        protected Button OpenButton => _openButton;

        /// <summary>The button the collapsed rail draws.</summary>
        protected Button ClosedButton => _closedButton;

        /// <summary>
        /// The element the collapsed rail draws, for a row that puts something else beside the picture there
        /// - see <see cref="SidebarBrand.WithSidebarControl"/>, which lays a control over it for the hover.
        /// </summary>
        protected HTMLElement ClosedRoot => _closedRoot;

        /// <summary>The name on the first line.</summary>
        protected string TitleText => _title;

        /// <summary>The second line, or an empty string when there is none.</summary>
        protected string SubtitleText => _subtitle;

        /// <summary>Gets or sets whether the item is currently selected.</summary>
        public bool IsSelected
        {
            get { return _selected.Value; }
            set
            {
                _selected.Value = value;

                if (value)
                {
                    CurrentRendered?.ScrollIntoView();
                }
            }
        }

        /// <summary>Gets an observable for the selected status.</summary>
        public IObservable<bool> SelectedStatus => _selected;

        /// <summary>Gets the component that is currently rendered.</summary>
        public IComponent CurrentRendered => _closedButton.IsMounted() ? _closed : _open;

        /// <summary>Gets the full identifier of the item, including group identifiers.</summary>
        public string Identifier { get; private set; }

        /// <summary>Gets the own identifier of the item, without group identifiers.</summary>
        public string OwnIdentifier => Sidebar.GetOwnIdentifier(Identifier);

        /// <summary>Adds a group identifier prefix to the item's identifier.</summary>
        public void AddGroupIdentifier(string groupIdentifier)
        {
            Identifier = Sidebar.WithGroupIdentifier(Identifier, groupIdentifier);
        }

        /// <summary>Shows the item.</summary>
        public void Show()
        {
            _closed.Show();
            _open.Show();
        }

        /// <summary>Collapses the item.</summary>
        public void Collapse()
        {
            _closed.Collapse();
            _open.Collapse();
        }

        /// <summary>
        /// Sets the second line - an e-mail address, a company, an environment. Null or empty leaves the row
        /// a single line rather than an empty one.
        /// </summary>
        /// <param name="subtitle">The text for the second line.</param>
        /// <returns>The current instance of the type.</returns>
        public T SetSubtitle(string subtitle)
        {
            _subtitle               = subtitle ?? string.Empty;
            _subtitleSpan.innerText = _subtitle;

            UpdateSubtitleVisibility();
            RefreshDefaultTooltip();
            return Self;
        }

        /// <summary>
        /// The commands drawn before the row's own - a search, a back arrow, the control that collapses the
        /// rail. Replaces whatever was set before.
        /// </summary>
        /// <param name="commands">The commands.</param>
        /// <returns>The current instance of the type.</returns>
        public T Commands(params SidebarCommand[] commands)
        {
            _extraCommands = commands ?? new SidebarCommand[0];
            UpdateCommands();
            return Self;
        }

        /// <summary>
        /// Keeps the commands out of sight until the pointer is on the row, the way an ordinary row's
        /// commands behave. The default is the other way round, because the row's commands are usually the
        /// only way to what they open.
        /// </summary>
        /// <param name="onHover">Whether the commands wait for a hover.</param>
        /// <returns>The current instance of the type.</returns>
        public T CommandsOnHover(bool onHover = true)
        {
            _openRoot.UpdateClassIf(!onHover, "tss-sidebar-commands-always-open");
            return Self;
        }

        /// <summary>
        /// Draws a divider on the row's outer edge - above a row at the bottom of the rail, below one at the
        /// top - running the full width of the sidebar rather than stopping at its padding.
        /// </summary>
        /// <param name="separated">Whether to draw the divider.</param>
        /// <returns>The current instance of the type.</returns>
        public T Separated(bool separated = true)
        {
            _openOuter.UpdateClassIf(separated,   "tss-sidebar-identity-separated");
            _closedOuter.UpdateClassIf(separated, "tss-sidebar-identity-separated");
            return Self;
        }

        /// <summary>
        /// Sets whether the row is selected - what marks the page it opens as the one being shown.
        /// </summary>
        /// <param name="isSelected">Whether the row is selected.</param>
        /// <returns>The current instance of the type.</returns>
        public T Selected(bool isSelected = true)
        {
            _selected.Value = isSelected;
            return Self;
        }

        /// <summary>
        /// Marks the item as not sortable, which is what a row of chrome wants.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public T NotSortable()
        {
            _openOuter.classList.add("tss-sortable-disable");
            _closedOuter.classList.add("tss-sortable-disable");
            return Self;
        }

        /// <summary>
        /// Adds a class to both renderings of the item.
        /// </summary>
        /// <param name="className">The class name.</param>
        /// <returns>The current instance of the type.</returns>
        public T Class(string className)
        {
            _open.Class(className);
            _closed.Class(className);
            return Self;
        }

        /// <summary>
        /// Sets the tooltip shown on the collapsed rail, where the row is the picture alone. None - the
        /// default - shows the name and the second line.
        /// </summary>
        /// <param name="text">The tooltip text.</param>
        /// <returns>The current instance of the type.</returns>
        public T Tooltip(string text)
        {
            _tooltipClosed = (b) => b.Tooltip(text, placement: TooltipPlacement.Right);
            _tooltipClosed(_closedButton);
            return Self;
        }

        /// <summary>
        /// Sets the tooltip component shown on the collapsed rail.
        /// </summary>
        /// <param name="tooltip">The tooltip component.</param>
        /// <returns>The current instance of the type.</returns>
        public T Tooltip(IComponent tooltip)
        {
            _tooltipClosed = (b) => b.Tooltip(tooltip, placement: TooltipPlacement.Right);
            _tooltipClosed(_closedButton);
            return Self;
        }

        /// <summary>
        /// Adds a click event handler to the row itself. The commands are separate buttons, so pressing one
        /// is not pressing the row.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public T OnClick(Action action)
        {
            _openButton.OnClick(action);
            _closedButton.OnClick(action);
            return Self;
        }

        /// <summary>
        /// Adds a click event handler with button and mouse event arguments.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public T OnClick(Action<Button, MouseEvent> action)
        {
            _openButton.OnClick((b,   e) => action(b, e));
            _closedButton.OnClick((b, e) => action(b, e));
            return Self;
        }

        /// <summary>
        /// Adds a context menu event handler.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public T OnContextMenu(Action<Button, MouseEvent> action)
        {
            _openButton.OnContextMenu((b,   e) => action(b, e));
            _closedButton.OnContextMenu((b, e) => action(b, e));
            return Self;
        }

        /// <summary>
        /// Adds a rendered event handler.
        /// </summary>
        /// <param name="onRendered">The rendered event handler.</param>
        /// <returns>The current instance of the type.</returns>
        public ISidebarItem OnRendered(Action<HTMLElement> onRendered)
        {
            _onRendered += onRendered;
            return this;
        }

        /// <summary>Renders the item for the closed state of the sidebar.</summary>
        public IComponent RenderClosed()
        {
            _onRendered?.Invoke(_closed.Render());
            _closedButton.RemoveTooltip();

            DomObserver.WhenMounted(_closedButton.Render(), () =>
            {
                window.setTimeout(_ => { ApplyClosedTooltip(); }, Sidebar.SIDEBAR_TRANSITION_TIME);
            });
            return _closed;
        }

        /// <summary>Renders the item for the open state of the sidebar.</summary>
        public IComponent RenderOpen()
        {
            foreach (var command in RenderedCommands()) command.RefreshTooltip();

            _onRendered?.Invoke(_open.Render());
            return _open;
        }

        /// <summary>
        /// Replaces the name on the first line. Kept protected so each row can name it for what it holds -
        /// the account's name, the product's name - and keep whatever else follows from it in step.
        /// </summary>
        /// <param name="title">The name.</param>
        protected void SetTitleText(string title)
        {
            _title               = title ?? string.Empty;
            _titleSpan.innerText = _title;

            RefreshDefaultTooltip();
        }

        /// <summary>The row's own command, drawn last - settings on a profile, configuration on a brand.</summary>
        /// <param name="command">The command, or null to remove it.</param>
        protected void SetPrimaryCommand(SidebarCommand command)
        {
            _primaryCommand = command;
            UpdateCommands();
        }

        /// <summary>The row's second command, drawn after the first - logging out, on a profile.</summary>
        /// <param name="command">The command, or null to remove it.</param>
        protected void SetSecondaryCommand(SidebarCommand command)
        {
            _secondaryCommand = command;
            UpdateCommands();
        }

        private IEnumerable<SidebarCommand> RenderedCommands()
        {
            foreach (var command in _extraCommands)
            {
                if (command is object) yield return command;
            }

            if (_primaryCommand is object) yield return _primaryCommand;
            if (_secondaryCommand is object) yield return _secondaryCommand;
        }

        private void UpdateCommands()
        {
            ClearChildren(_commandsContainer);

            var count = 0;

            foreach (var command in RenderedCommands())
            {
                _commandsContainer.appendChild(command.Render());
                count++;
            }

            //How wide the strip is decides how much room the name gives up for it, and here a command is a
            //square of the row's own height rather than the 22px chip an ordinary row's is - so the count
            //is written and the stylesheet computes the width from it and from the height it is drawing
            //(see --tss-sidebar-commands-width on .tss-sidebar-identity in tss.sidebar.css). Writing the
            //pixels here instead would hard-code a height a skin is free to change.
            _openRoot.UpdateClassIf(count > 0, "tss-sidebar-has-commands");
            _openRoot.style.setProperty("--tss-sidebar-identity-command-count", count.ToString());
        }

        private void UpdateSubtitleVisibility()
        {
            var hasSubtitle = !string.IsNullOrWhiteSpace(_subtitle);

            _subtitleSpan.style.display = hasSubtitle ? "block" : "none";
            _openRoot.UpdateClassIf(hasSubtitle, "tss-sidebar-identity-two-lines");
        }

        private void ApplyClosedTooltip()
        {
            if (_tooltipClosed is object)
            {
                _tooltipClosed(_closedButton);
                return;
            }

            _closedButton.Tooltip(DefaultClosedTooltip(), placement: TooltipPlacement.Right);
        }

        /// <summary>
        /// The tooltip the collapsed rail gets when the caller has not set one: the name, and the second line
        /// after it where there is one. Re-applied whenever either changes, and left alone once a tooltip has
        /// been set by hand.
        /// </summary>
        private void RefreshDefaultTooltip()
        {
            if (_tooltipClosed is object) return;

            _closedButton.Tooltip(DefaultClosedTooltip(), placement: TooltipPlacement.Right);
        }

        private string DefaultClosedTooltip() => string.IsNullOrWhiteSpace(_subtitle) ? _title : _title + " - " + _subtitle;
    }
}
