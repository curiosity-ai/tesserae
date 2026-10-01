using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A card at the top of a sidebar that says what the section it heads is doing right now: a glyph and
    /// the section's name, a status line ("Index healthy · 2.4M nodes") with a dot in the status's tone, and
    /// optionally a progress bar with a caption under it for a job that is running ("Re-index 72% · 4 min
    /// left"). Every part can be changed after it is on screen, so it is meant to be kept up to date from
    /// whatever reports the state.
    /// <para>
    /// On the collapsed rail the card is the glyph alone, with the progress drawn as a short bar under it
    /// and the whole text in its tooltip.
    /// </para>
    /// </summary>
    public class SidebarStatusCard : ISidebarItem
    {
        private const string ToneSuccess = "tss-sidebar-status-success";
        private const string ToneWarning = "tss-sidebar-status-warning";
        private const string ToneDanger  = "tss-sidebar-status-danger";

        private readonly Stack             _open;
        private readonly Stack             _closed;
        private readonly TextBlock         _title;
        private readonly TextBlock         _status;
        private readonly Stack             _statusLine;
        private readonly Stack             _progressArea;
        private readonly ProgressIndicator _progress;
        private readonly TextBlock         _caption;
        private readonly ProgressIndicator _closedProgress;

        private string _titleText;
        private string _statusText;
        private string _captionText;

        /// <summary>
        /// Initializes a new instance of the SidebarStatusCard class.
        /// </summary>
        /// <param name="identifier">The identifier for the item.</param>
        /// <param name="icon">The section's glyph.</param>
        /// <param name="title">The section's name.</param>
        /// <param name="status">The status line. None leaves the card without one.</param>
        /// <param name="weight">The glyph's weight.</param>
        public SidebarStatusCard(string identifier, UIcons icon, string title, string status = null, UIconsWeight weight = UIconsWeight.Regular)
        {
            Identifier = identifier;

            _title  = TextBlock(title).Class("tss-sidebar-status-title");
            _status = TextBlock(status ?? "").Class("tss-sidebar-status-text");

            _statusLine = HStack().AlignItemsCenter().Class("tss-sidebar-status-line").Children(
                Raw(Div(Att("tss-sidebar-status-dot"))),
                _status);

            _progress     = ProgressIndicator().WS();
            _caption      = TextBlock("").Class("tss-sidebar-status-caption");
            _progressArea = VStack().WS().Class("tss-sidebar-status-progress").Children(_progress, _caption);

            _open = VStack().WS().Class("tss-sidebar-status-card").Id(identifier).Children(
                HStack().WS().AlignItemsCenter().Class("tss-sidebar-status-head").Children(
                    Raw(Div(Att("tss-sidebar-status-icon"), Icon(icon, weight, TextSize.Large).Render())),
                    VStack().Class("tss-sidebar-status-lines").Children(_title, _statusLine)),
                _progressArea);

            _closedProgress = ProgressIndicator().WS();
            _closed = VStack().AlignItemsCenter().Class("tss-sidebar-status-card-closed").Id(identifier).Children(
                Icon(icon, weight, TextSize.Medium),
                _closedProgress);

            _titleText  = title;
            _statusText = status;

            ApplyStatus();
            ClearProgress();
        }

        /// <summary>Gets or sets whether the item is currently selected. A status card is not a destination, so this is only carried for the sidebar's sake.</summary>
        public bool IsSelected { get; set; }

        /// <summary>Gets the component that is currently rendered.</summary>
        public IComponent CurrentRendered => _closed.IsMounted() ? (IComponent)_closed : _open;

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
            _open.Show();
            _closed.Show();
        }

        /// <summary>Collapses the item.</summary>
        public void Collapse()
        {
            _open.Collapse();
            _closed.Collapse();
        }

        /// <summary>
        /// Sets the section's name.
        /// </summary>
        /// <param name="title">The name.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarStatusCard SetTitle(string title)
        {
            _titleText  = title;
            _title.Text = title;
            UpdateTooltip();
            return this;
        }

        /// <summary>
        /// Sets the status line. Null or empty hides it.
        /// </summary>
        /// <param name="status">The status, e.g. "Index healthy · 2.4M nodes".</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarStatusCard SetStatus(string status)
        {
            _statusText = status;
            ApplyStatus();
            return this;
        }

        /// <summary>Draws the status dot in the success colour.</summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarStatusCard Success() => Tone(ToneSuccess);

        /// <summary>Draws the status dot in the warning colour.</summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarStatusCard Warning() => Tone(ToneWarning);

        /// <summary>Draws the status dot in the danger colour.</summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarStatusCard Danger() => Tone(ToneDanger);

        /// <summary>Draws the status dot in the neutral colour, which is the default.</summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarStatusCard Neutral() => Tone(null);

        /// <summary>
        /// Shows a progress bar for a running job, with an optional caption under it.
        /// </summary>
        /// <param name="percent">How far along the job is, from 0 to 100.</param>
        /// <param name="caption">The caption, e.g. "Re-index 72% · 4 min left". None leaves the bar on its own.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarStatusCard Progress(float percent, string caption = null)
        {
            _progress.Progress(percent);
            _closedProgress.Progress(percent);
            return ShowProgress(caption);
        }

        /// <summary>
        /// Shows an indeterminate progress bar, for a job that cannot say how far along it is.
        /// </summary>
        /// <param name="caption">The caption. None leaves the bar on its own.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarStatusCard Indeterminate(string caption = null)
        {
            _progress.Indeterminated();
            _closedProgress.Indeterminated();
            return ShowProgress(caption);
        }

        /// <summary>Removes the progress bar and its caption.</summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarStatusCard ClearProgress()
        {
            _captionText = null;
            _progressArea.Collapse();
            _closedProgress.Collapse();
            UpdateTooltip();
            return this;
        }

        /// <summary>
        /// Runs an action when the card is clicked, typically to open the page with the full status.
        /// </summary>
        /// <param name="onClick">What clicking the card does.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarStatusCard OnClick(Action onClick)
        {
            foreach (var card in new IComponent[] { _open, _closed })
            {
                card.Class("tss-sidebar-status-clickable").Render().addEventListener("click", (Event e) => onClick());
            }
            return this;
        }

        /// <summary>
        /// Marks the item as not draggable in a sortable sidebar.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarStatusCard NotSortable()
        {
            _open.Class("tss-sortable-disable");
            _closed.Class("tss-sortable-disable");
            return this;
        }

        /// <summary>Renders the item for the closed state of the sidebar.</summary>
        public IComponent RenderClosed() => _closed;

        /// <summary>Renders the item for the open state of the sidebar.</summary>
        public IComponent RenderOpen() => _open;

        private SidebarStatusCard ShowProgress(string caption)
        {
            _captionText  = caption;
            _caption.Text = caption ?? "";

            if (string.IsNullOrWhiteSpace(caption)) _caption.Collapse();
            else                                    _caption.Show();

            _progressArea.Show();
            _closedProgress.Show();
            UpdateTooltip();
            return this;
        }

        private SidebarStatusCard Tone(string toneClass)
        {
            foreach (var card in new IComponent[] { _open, _closed })
            {
                var element = card.Render();
                element.classList.remove(ToneSuccess);
                element.classList.remove(ToneWarning);
                element.classList.remove(ToneDanger);
                if (toneClass is object) element.classList.add(toneClass);
            }
            return this;
        }

        private void ApplyStatus()
        {
            _status.Text = _statusText ?? "";

            if (string.IsNullOrWhiteSpace(_statusText)) _statusLine.Collapse();
            else                                        _statusLine.Show();

            UpdateTooltip();
        }

        //The collapsed rail has room for the glyph only, so everything the open card says goes in its tooltip
        private void UpdateTooltip()
        {
            var text = _titleText ?? "";
            if (!string.IsNullOrWhiteSpace(_statusText)) text += " · " + _statusText;

            var tooltip = VStack().Children(TextBlock(text));
            if (!string.IsNullOrWhiteSpace(_captionText)) tooltip.Add(TextBlock(_captionText));

            _closed.Tooltip(tooltip, placement: TooltipPlacement.Right);
        }
    }
}
