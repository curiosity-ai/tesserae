using Transpose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;
using TNT;
using static TNT.T;

namespace Tesserae
{
    /// <summary>How a conversation's state is drawn in the dot at the start of its row.</summary>
    public enum ConversationStateTone
    {
        /// <summary>A hollow ring: nothing is happening.</summary>
        Idle,
        /// <summary>A filled dot in the primary colour that pulses: something is working on it right now.</summary>
        Running,
        /// <summary>A filled dot in the warning colour: it is waiting on the user.</summary>
        Attention,
        /// <summary>A filled dot in the success colour: it is done.</summary>
        Success,
        /// <summary>A faded ring: it is put away.</summary>
        Muted,
        /// <summary>A filled dot in the danger colour: it failed.</summary>
        Danger,
    }

    /// <summary>The time filter of a <see cref="ConversationList"/>, measured on <see cref="ConversationListItem.Time"/>.</summary>
    public enum ConversationTimeRange
    {
        /// <summary>No time filter.</summary>
        AnyTime,
        /// <summary>Since midnight, local time.</summary>
        Today,
        /// <summary>The last seven days.</summary>
        Past7Days,
        /// <summary>The last thirty days.</summary>
        Past30Days,
        /// <summary>Anything older than thirty days.</summary>
        Older,
    }

    /// <summary>One row of a <see cref="ConversationList"/>.</summary>
    public sealed class ConversationListItem
    {
        /// <summary>The stable key of the conversation. Rows are matched by it, so an update never rebuilds the list.</summary>
        public string          Id       { get; set; }
        /// <summary>The conversation's name.</summary>
        public string          Title    { get; set; }
        /// <summary>A second, quieter line under the name. None leaves the row on one line.</summary>
        public string          Subtitle { get; set; }
        /// <summary>When the conversation last changed. Drawn as a relative time ("now", "9h", "2d") and used by the time filter.</summary>
        public DateTimeOffset  Time     { get; set; }
        /// <summary>Whether the conversation sits in the pinned section.</summary>
        public bool            IsPinned { get; set; }
        /// <summary>The key of one of the states added with <see cref="ConversationList.AddState"/>. Null, or a key nobody added, is drawn as idle.</summary>
        public string          State    { get; set; }
        /// <summary>An icon drawn before the name. None draws none.</summary>
        public UIcons?         Icon     { get; set; }
        /// <summary>The icon's colour, as any CSS colour. None uses the row's text colour.</summary>
        public string          IconColor { get; set; }
        /// <summary>A small component drawn before the time - a warning, a badge. Rendered once per update of the item.</summary>
        public IComponent      Trailing { get; set; }
        /// <summary>Whatever the host wants to find again when the row is clicked.</summary>
        public object          Data     { get; set; }

        /// <summary>
        /// The work running inside this conversation - the agents it delegated to, say. A conversation with
        /// children can be expanded like a sidebar group, and its children are listed under it. None, or an
        /// empty list, leaves a plain row.
        /// </summary>
        public IReadOnlyList<ConversationListChild> Children { get; set; }

        internal string Signature()
        {
            var children = "";

            if (Children is object)
            {
                foreach (var child in Children) children += child.Signature() + "\u0002";
            }

            return Title + "\u0001" + Subtitle + "\u0001" + Time.ToUnixTimeMilliseconds() + "\u0001" + IsPinned + "\u0001" + State + "\u0001" + (Icon.HasValue ? Icon.Value.ToString() : "") + "\u0001" + IconColor + "\u0001" + children;
        }
    }

    /// <summary>
    /// One piece of work running inside a conversation - an agent the conversation delegated to - drawn as a
    /// compact row under it: a status glyph, an optional leading icon, its name, a quiet monospace detail
    /// (the model it runs on) and the time it last changed. See <see cref="ConversationListItem.Children"/>.
    /// </summary>
    public sealed class ConversationListChild
    {
        /// <summary>The stable key of the child. It shares the id space of the conversations, so it must not repeat any of theirs.</summary>
        public string                Id         { get; set; }
        /// <summary>What it is called.</summary>
        public string                Title      { get; set; }
        /// <summary>A short detail in a monospace face after the name - the model it runs on, say.</summary>
        public string                Meta       { get; set; }
        /// <summary>When it last changed. Drawn as a relative time, like the conversation's own.</summary>
        public DateTimeOffset        Time       { get; set; }
        /// <summary>What it is doing, drawn as the status glyph at the start of its row: a spinner while running, a check once done.</summary>
        public ConversationStateTone Tone       { get; set; }
        /// <summary>What the status glyph says, as its tooltip and accessible name ("Running", "Failed").</summary>
        public string                StateLabel { get; set; }
        /// <summary>An icon drawn between the status glyph and the name. None draws none.</summary>
        public UIcons?               Icon       { get; set; }
        /// <summary>The icon's colour, as any CSS colour. None uses the row's text colour.</summary>
        public string                IconColor  { get; set; }
        /// <summary>A small component drawn where the icon goes - a provider's logo. Wins over <see cref="Icon"/>. Rendered once per update of the child.</summary>
        public IComponent            Leading    { get; set; }
        /// <summary>Whatever the host wants to find again when the row is clicked.</summary>
        public object                Data       { get; set; }

        internal string Signature() => Id + "\u0003" + Title + "\u0003" + Meta + "\u0003" + Time.ToUnixTimeMilliseconds() + "\u0003" + Tone + "\u0003" + StateLabel + "\u0003" + (Icon.HasValue ? Icon.Value.ToString() : "") + "\u0003" + IconColor;
    }

    /// <summary>What a <see cref="ConversationList"/> is filtered by at the moment.</summary>
    public sealed class ConversationListFilter
    {
        /// <summary>The state key the list is narrowed to, or null for every state that is part of "All".</summary>
        public string                State     { get; set; }
        /// <summary>The time range the list is narrowed to.</summary>
        public ConversationTimeRange TimeRange { get; set; }
        /// <summary>What is typed in the search box, trimmed. Empty when the search is closed.</summary>
        public string                Search    { get; set; }

        /// <summary>True when nothing narrows the list: every state, any time, no search.</summary>
        public bool IsDefault => State is null && TimeRange == ConversationTimeRange.AnyTime && string.IsNullOrEmpty(Search);
    }

    /// <summary>
    /// The history of a chat surface: a list of conversations with a pinned section at the top, a search
    /// and a filter (by state and by time) in its header, a dot per row saying what each conversation is
    /// doing, and the relative time it last changed.
    /// <para>
    /// The states are the host's own (<see cref="AddState"/>): each has a label, an icon for the filter
    /// menu and a tone for the dot. A state added with <c>includedInAll: false</c> (an archive) is left out
    /// of "All" and only shown when it is picked.
    /// </para>
    /// <para>
    /// It is an <see cref="ISidebarItem"/>, so it can be put straight into a <see cref="Sidebar"/>. On the
    /// collapsed rail it leaves one glyph, and hovering that glyph opens the very same list in a flyout
    /// beside the rail - same filter, same search, same scroll - so a collapsed sidebar does not lose the
    /// history. <see cref="RenderClosed"/> is that glyph for a host that builds its own rail.
    /// </para>
    /// <para>
    /// Rows are matched by <see cref="ConversationListItem.Id"/>, so <see cref="SetItems"/>,
    /// <see cref="AddItems"/> and <see cref="UpdateItem"/> only touch the rows that changed. Filtering is
    /// done on the items the list holds; a host that pages from a server listens to
    /// <see cref="OnFilterChanged"/> and loads what the new filter asks for.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.ConversationList")]
    public sealed class ConversationList : IComponent, ISidebarItem, IHasMarginPadding
    {
        private const int FLYOUT_DELAY_HIDE = 200;

        private sealed class StateDefinition
        {
            public string                Key;
            public string                Label;
            public UIcons                Icon;
            public ConversationStateTone Tone;
            public bool                  IncludedInAll;
            public bool                  InFilterMenu;
        }

        private sealed class ChildRow
        {
            public ConversationListChild Child;
            public string                Signature;
            public HTMLElement           Element;
            public HTMLElement           Status;
            public HTMLElement           IconHost;
            public HTMLElement           Title;
            public HTMLElement           Meta;
            public HTMLElement           Time;
        }

        private sealed class Row
        {
            public ConversationListItem Item;
            public string               Signature;
            //What goes in the list: the row, and under it the children it can expand to.
            public HTMLElement          Group;
            public HTMLElement          ChildrenHost;
            public HTMLElement          Toggle;
            public HTMLElement          ToggleIcon;
            public HTMLElement          Count;
            public HTMLElement          CountDot;
            public HTMLElement          CountText;
            public Dictionary<string, ChildRow> Children = new Dictionary<string, ChildRow>();
            public HTMLElement          Element;
            public HTMLElement          Dot;
            public HTMLElement          IconHost;
            public HTMLElement          Title;
            public HTMLElement          Subtitle;
            public HTMLElement          TrailingHost;
            public HTMLElement          Time;
        }

        private readonly HTMLElement _root;
        private readonly HTMLElement _openHost;
        private readonly HTMLElement _flyoutHost;
        private readonly HTMLElement _titleElement;
        private readonly HTMLElement _searchRow;
        private readonly HTMLElement _filterSummary;
        private readonly HTMLElement _filterSummaryText;
        private readonly HTMLElement _body;
        private readonly HTMLElement _pinnedSection;
        private readonly HTMLElement _pinnedHeader;
        private readonly HTMLElement _pinnedCount;
        private readonly HTMLElement _pinnedList;
        private readonly HTMLElement _recentHeader;
        private readonly HTMLElement _recentList;
        private readonly HTMLElement _statusHost;
        private readonly HTMLElement _sensorHost;
        private readonly Button      _searchButton;
        private readonly Button      _filterButton;
        private readonly TextBox     _searchBox;
        private readonly Button      _closedButton;

        private readonly List<StateDefinition>      _states  = new List<StateDefinition>();
        private readonly List<ConversationListItem> _items   = new List<ConversationListItem>();
        private readonly Dictionary<string, Row>    _rows    = new Dictionary<string, Row>();
        private readonly HashSet<string>            _expanded = new HashSet<string>();

        private ConversationListItem[]                                   _searchResults;
        private Func<string, Task<IEnumerable<ConversationListItem>>>    _searchProvider;
        private int                                                      _searchGeneration;
        private Action<ConversationListItem>                             _onSelect;
        private Action<ConversationListChild, ConversationListItem>      _onChildSelect;
        private Func<ConversationListItem, ContextMenu.Item[]>           _itemMenu;
        private Action<ConversationListFilter>                           _onFilterChanged;
        private Action                                                   _onLoadMore;
        private Action                                                   _hideFlyout;

        private string                _selectedId;
        private string                _stateFilter;
        private ConversationTimeRange _timeRange = ConversationTimeRange.AnyTime;
        private string                _search    = "";
        private bool                  _loading;
        private bool                  _hasMore;
        private bool                  _pinnedCollapsed;
        private bool                  _flyoutOpen;
        private int                   _openPopups;
        private double                _clockTimer;
        private string                _emptyText         = "Conversations you start will show up here".t();
        private IComponent            _emptyComponent;
        private string                _closedTooltip     = "History".t();

        /// <summary>Initializes a new, empty conversation list.</summary>
        /// <param name="title">The header's title. None leaves the header with only its buttons.</param>
        public ConversationList(string title = null)
        {
            _titleElement = Div(Att("tss-convlist-title"));
            _titleElement.textContent = title ?? "";

            _searchButton = Button().SetIcon(UIcons.Search).NoBorder().NoBackground().LessPadding().Class("tss-convlist-header-button").Tooltip("Search".t());
            _searchButton.Render().setAttribute("aria-label", "Search conversations".t());
            _searchButton.OnClick(() => ToggleSearch());

            _filterButton = Button().SetIcon(UIcons.BarsFilter).NoBorder().NoBackground().LessPadding().Class("tss-convlist-header-button").Tooltip("Filter".t());
            _filterButton.Render().setAttribute("aria-label", "Filter conversations".t());
            _filterButton.Render().setAttribute("aria-haspopup", "menu");
            _filterButton.OnClick(() => ShowFilterMenu());

            var header = Div(Att("tss-convlist-header"), _titleElement, Div(Att("tss-convlist-header-buttons"), _searchButton.Render(), _filterButton.Render()));

            _searchBox = TextBox().SetPlaceholder("Search conversations".t()).Class("tss-convlist-search-box");
            _searchBox.OnInput((s, e) => SetSearch(_searchBox.Text));
            _searchBox.OnKeyDown((s, e) =>
            {
                if (e.key == "Escape")
                {
                    e.stopPropagation();
                    CloseSearch();
                }
            });

            _searchRow = Div(Att("tss-convlist-search"), I(UIcons.Search, cssClass: "tss-convlist-search-icon"), _searchBox.Render());
            _searchRow.style.display = "none";

            _filterSummaryText = Div(Att("tss-convlist-filter-summary-text"));
            var clearFilter = Button().SetIcon(UIcons.Cross).NoBorder().NoBackground().LessPadding().Class("tss-convlist-filter-clear").Tooltip("Clear filters".t());
            clearFilter.Render().setAttribute("aria-label", "Clear filters".t());
            clearFilter.OnClick(() => ClearFilters());

            _filterSummary = Div(Att("tss-convlist-filter-summary"), I(UIcons.BarsFilter, cssClass: "tss-convlist-filter-summary-icon"), _filterSummaryText, clearFilter.Render());
            _filterSummary.style.display = "none";

            _pinnedCount  = Div(Att("tss-convlist-section-count"));
            _pinnedHeader = Div(Att("tss-convlist-section-header tss-convlist-section-toggle"),
                                I(UIcons.AngleSmallDown, cssClass: "tss-convlist-section-chevron"),
                                TextDiv("tss-convlist-section-label", "Pinned".t()),
                                _pinnedCount);
            _pinnedHeader.setAttribute("role", "button");
            _pinnedHeader.setAttribute("tabindex", "0");
            _pinnedHeader.setAttribute("aria-expanded", "true");
            _pinnedHeader.addEventListener("click", (Event e) => TogglePinned());
            _pinnedHeader.addEventListener("keydown", (Event e) =>
            {
                var key = e.As<KeyboardEvent>().key;
                if (key == "Enter" || key == " ")
                {
                    e.preventDefault();
                    TogglePinned();
                }
            });

            _pinnedList    = Div(Att("tss-convlist-rows"));
            _pinnedList.setAttribute("role", "list");
            _pinnedSection = Div(Att("tss-convlist-section tss-convlist-pinned"), _pinnedHeader, _pinnedList);

            _recentHeader = Div(Att("tss-convlist-section-header"));
            _recentList   = Div(Att("tss-convlist-rows"));
            _recentList.setAttribute("role", "list");

            _statusHost = Div(Att("tss-convlist-status"));
            _sensorHost = Div(Att("tss-convlist-sensor"));

            _body = Div(Att("tss-convlist-body"), _pinnedSection, Div(Att("tss-convlist-section tss-convlist-recent"), _recentHeader, _recentList), _statusHost, _sensorHost);

            _root = Div(Att("tss-convlist"), header, _searchRow, _filterSummary, _body);

            _openHost   = Div(Att("tss-convlist-host"), _root);
            _flyoutHost = Div(Att("tss-convlist-flyout"));

            _closedButton = Button().SetIcon(UIcons.TimePast).NoBorder().NoBackground().Class("tss-convlist-rail-button");
            _closedButton.Render().setAttribute("aria-label", _closedTooltip);
            _closedButton.Render().addEventListener("mouseenter", (Event e) => ShowFlyout());
            _closedButton.Render().addEventListener("focus",      (Event e) => ShowFlyout());
            _closedButton.OnClick(() => ShowFlyout());

            //Relative times go stale while the list stays open - "now" is not "now" an hour later - so they are
            //redrawn once a minute, only while the list is on the page.
            DomObserver.WhenMounted(_root, () => StartClock());

            Refresh();
        }

        /// <inheritdoc />
        public string Margin  { get => _root.style.margin;  set => _root.style.margin  = value; }

        /// <inheritdoc />
        public string Padding { get => _root.style.padding; set => _root.style.padding = value; }

        /// <summary>The filter the list shows right now.</summary>
        public ConversationListFilter CurrentFilter => new ConversationListFilter { State = _stateFilter, TimeRange = _timeRange, Search = _search };

        /// <summary>The id of the selected conversation, or null.</summary>
        public string SelectedId => _selectedId;

        /// <summary>The items the list holds, in the order they were given, whatever the filter shows.</summary>
        public IReadOnlyList<ConversationListItem> Items => _items;

        /// <inheritdoc />
        public HTMLElement Render() => _openHost;

        // ------------------------------------------------------------------------------------------
        // Configuration
        // ------------------------------------------------------------------------------------------

        /// <summary>Sets the header's title.</summary>
        public ConversationList Title(string title)
        {
            _titleElement.textContent = title ?? "";
            return this;
        }

        /// <summary>
        /// Adds a state conversations can be in, and with it an entry in the filter menu.
        /// </summary>
        /// <param name="key">What <see cref="ConversationListItem.State"/> is set to for a conversation in this state.</param>
        /// <param name="label">The name of the state in the filter menu ("Working", "Archived").</param>
        /// <param name="icon">The icon beside the name in the filter menu.</param>
        /// <param name="tone">How the row's dot is drawn for a conversation in this state.</param>
        /// <param name="includedInAll">False leaves conversations in this state out of "All", for an archive that is only shown when asked for.</param>
        /// <param name="inFilterMenu">False keeps the state off the filter menu: its rows still get the dot and its label, but the list cannot be narrowed to it.</param>
        public ConversationList AddState(string key, string label, UIcons icon, ConversationStateTone tone = ConversationStateTone.Idle, bool includedInAll = true, bool inFilterMenu = true)
        {
            _states.RemoveAll(s => s.Key == key);
            _states.Add(new StateDefinition { Key = key, Label = label, Icon = icon, Tone = tone, IncludedInAll = includedInAll, InFilterMenu = inFilterMenu });
            Refresh(forceRows: true);
            return this;
        }

        /// <summary>The text shown when the list holds no conversations at all.</summary>
        public ConversationList EmptyMessage(string text)
        {
            _emptyText      = text;
            _emptyComponent = null;
            Refresh();
            return this;
        }

        /// <summary>What the list shows, in place of the default icon and text, when it holds no conversations at all.</summary>
        public ConversationList EmptyMessage(IComponent emptyMessage)
        {
            _emptyComponent = emptyMessage;
            Refresh();
            return this;
        }

        /// <summary>The name of the glyph the list leaves on a collapsed rail.</summary>
        public ConversationList ClosedLabel(string label, UIcons icon = UIcons.TimePast)
        {
            _closedTooltip = label;
            _closedButton.SetIcon(icon);
            _closedButton.Render().setAttribute("aria-label", label);
            return this;
        }

        /// <summary>Runs when a conversation is clicked (or picked with Enter).</summary>
        public ConversationList OnSelect(Action<ConversationListItem> onSelect)
        {
            _onSelect = onSelect;
            return this;
        }

        /// <summary>
        /// Runs when a child of a conversation (see <see cref="ConversationListItem.Children"/>) is clicked or picked
        /// with Enter, with the conversation it belongs to. Selecting one does not select the conversation.
        /// </summary>
        public ConversationList OnChildSelect(Action<ConversationListChild, ConversationListItem> onChildSelect)
        {
            _onChildSelect = onChildSelect;
            return this;
        }

        /// <summary>
        /// Gives every row a "more" button, and a right-click menu, listing the items this returns for it.
        /// Returning none leaves that row without a menu.
        /// </summary>
        public ConversationList ItemMenu(Func<ConversationListItem, ContextMenu.Item[]> itemMenu)
        {
            _itemMenu = itemMenu;
            Refresh(forceRows: true);
            return this;
        }

        /// <summary>
        /// Runs whenever the state filter, the time filter or the search changes - for a host that loads
        /// conversations from a server and has to ask for what the new filter covers. The list filters what
        /// it holds either way.
        /// </summary>
        public ConversationList OnFilterChanged(Action<ConversationListFilter> onFilterChanged)
        {
            _onFilterChanged = onFilterChanged;
            return this;
        }

        /// <summary>
        /// Answers the search from somewhere other than the items the list holds (a server). While something
        /// is typed the list shows what this returns instead of its items; a reply that arrives after the
        /// search moved on is dropped.
        /// </summary>
        public ConversationList SearchWith(Func<string, Task<IEnumerable<ConversationListItem>>> searchProvider)
        {
            _searchProvider = searchProvider;
            return this;
        }

        /// <summary>
        /// Runs when the end of the list scrolls into view while <see cref="HasMore"/> is true. Call
        /// <see cref="AddItems"/> with the next page and <see cref="HasMore"/> again to arm it for the one after.
        /// </summary>
        public ConversationList OnLoadMore(Action onLoadMore)
        {
            _onLoadMore = onLoadMore;
            return this;
        }

        // ------------------------------------------------------------------------------------------
        // Data
        // ------------------------------------------------------------------------------------------

        /// <summary>Replaces every item. Rows whose item did not change are kept as they are.</summary>
        public ConversationList SetItems(IEnumerable<ConversationListItem> items)
        {
            _items.Clear();
            _items.AddRange(Distinct(items));
            Refresh();
            return this;
        }

        /// <summary>Appends a page of items. An item the list already holds is updated in place rather than added twice.</summary>
        public ConversationList AddItems(IEnumerable<ConversationListItem> items)
        {
            foreach (var item in items ?? Enumerable.Empty<ConversationListItem>())
            {
                if (item?.Id is null) continue;

                var index = _items.FindIndex(i => i.Id == item.Id);

                if (index >= 0) _items[index] = item;
                else            _items.Add(item);
            }

            Refresh();
            return this;
        }

        /// <summary>Replaces the item with the same id, or adds it at the top when the list does not hold it.</summary>
        public ConversationList UpdateItem(ConversationListItem item)
        {
            if (item?.Id is null) return this;

            var index = _items.FindIndex(i => i.Id == item.Id);

            if (index >= 0) _items[index] = item;
            else            _items.Insert(0, item);

            if (_searchResults is object)
            {
                var searchIndex = Array.FindIndex(_searchResults, i => i.Id == item.Id);
                if (searchIndex >= 0) _searchResults[searchIndex] = item;
            }

            Refresh();
            return this;
        }

        /// <summary>Removes the item with this id. Removing the selected one clears the selection.</summary>
        public ConversationList RemoveItem(string id)
        {
            _items.RemoveAll(i => i.Id == id);
            if (_searchResults is object) _searchResults = _searchResults.Where(i => i.Id != id).ToArray();
            if (_selectedId == id) _selectedId = null;
            Refresh();
            return this;
        }

        /// <summary>Removes every item.</summary>
        public ConversationList Clear() => SetItems(Array.Empty<ConversationListItem>());

        /// <summary>
        /// Says the list is waiting for conversations. While it holds none it shows a spinner instead of the
        /// empty message, which would otherwise read as "you have no conversations".
        /// </summary>
        public ConversationList Loading(bool loading = true)
        {
            _loading = loading;
            Refresh();
            return this;
        }

        /// <summary>Says whether there is another page to load. See <see cref="OnLoadMore"/>.</summary>
        public ConversationList HasMore(bool hasMore)
        {
            _hasMore = hasMore;
            UpdateSensor();
            return this;
        }

        /// <summary>Opens a conversation's children. Does nothing for one that has none.</summary>
        public ConversationList ExpandItem(string id)
        {
            SetExpanded(id, true);
            return this;
        }

        /// <summary>Folds a conversation's children away.</summary>
        public ConversationList CollapseItem(string id)
        {
            SetExpanded(id, false);
            return this;
        }

        /// <summary>Whether the conversation with this id shows its children.</summary>
        public bool IsExpanded(string id) => id is object && _expanded.Contains(id);

        private static bool HasChildren(ConversationListItem item) => item.Children is object && item.Children.Count > 0;

        private void SetExpanded(string id, bool expanded)
        {
            if (id is null) return;

            var changed = expanded ? _expanded.Add(id) : _expanded.Remove(id);

            if (changed && _rows.TryGetValue(id, out var row)) ApplyExpanded(row);
        }

        private void ApplyExpanded(Row row)
        {
            var hasChildren = HasChildren(row.Item);
            var expanded    = hasChildren && (_expanded.Contains(row.Item.Id) || ForcedOpenBySearch(row.Item));

            row.Group.classList.toggle("tss-convlist-group-open", expanded);
            row.ChildrenHost.style.display = expanded ? "" : "none";

            if (hasChildren) row.Element.setAttribute("aria-expanded", expanded ? "true" : "false");
            else             row.Element.removeAttribute("aria-expanded");

            var label = expanded ? "Collapse agents".t() : "Expand agents".t();
            row.Toggle.setAttribute("aria-label", label);
            row.Toggle.setAttribute("aria-expanded", expanded ? "true" : "false");
            row.Count.setAttribute("aria-label", label);
        }

        /// <summary>Marks the conversation (or the child of one) with this id as the one open, scrolling it into view when it is off screen. A child opens its conversation.</summary>
        public ConversationList Select(string id)
        {
            _selectedId = id;

            if (id is object && !_rows.ContainsKey(id))
            {
                foreach (var candidate in _rows.Values)
                {
                    if (!candidate.Children.ContainsKey(id)) continue;

                    //The child is only on screen while its conversation is open.
                    SetExpanded(candidate.Item.Id, true);
                    break;
                }
            }

            foreach (var row in _rows.Values)
            {
                ApplySelected(row);
                foreach (var child in row.Children.Values) ApplyChildSelected(child);
            }

            HTMLElement target = null;

            if (id is object)
            {
                if (_rows.TryGetValue(id, out var selectedRow)) target = selectedRow.Element;
                else
                {
                    foreach (var candidate in _rows.Values)
                    {
                        if (candidate.Children.TryGetValue(id, out var selectedChild)) target = selectedChild.Element;
                    }
                }
            }

            if (target is object && target.IsMounted())
            {
                var rowRect  = target.getBoundingClientRect().As<DOMRect>();
                var bodyRect = _body.getBoundingClientRect().As<DOMRect>();

                if (rowRect.top < bodyRect.top || rowRect.bottom > bodyRect.bottom)
                {
                    target.scrollIntoView(new ScrollIntoViewOptions { block = ScrollLogicalPosition.nearest });
                }
            }

            return this;
        }

        // ------------------------------------------------------------------------------------------
        // Filters
        // ------------------------------------------------------------------------------------------

        /// <summary>Narrows the list to one state, or to every state in "All" with null.</summary>
        public ConversationList FilterByState(string state)
        {
            if (_stateFilter == state) return this;
            _stateFilter = state;
            FilterChanged();
            return this;
        }

        /// <summary>Narrows the list to a time range.</summary>
        public ConversationList FilterByTime(ConversationTimeRange range)
        {
            if (_timeRange == range) return this;
            _timeRange = range;
            FilterChanged();
            return this;
        }

        /// <summary>Clears the state and time filters and closes the search.</summary>
        public ConversationList ClearFilters()
        {
            var changed = _stateFilter is object || _timeRange != ConversationTimeRange.AnyTime || !string.IsNullOrEmpty(_search);

            _stateFilter = null;
            _timeRange   = ConversationTimeRange.AnyTime;

            if (!string.IsNullOrEmpty(_search) || _searchRow.style.display != "none")
            {
                _searchBox.Text = "";
                _search         = "";
                _searchResults  = null;
                _searchRow.style.display = "none";
                _searchButton.RemoveClass("tss-convlist-header-button-active");
            }

            if (changed) FilterChanged();
            return this;
        }

        /// <summary>Opens the search box and puts the cursor in it.</summary>
        public ConversationList OpenSearch()
        {
            _searchRow.style.display = "";
            _searchButton.Class("tss-convlist-header-button-active");
            _searchBox.Focus();
            return this;
        }

        /// <summary>Closes the search box, and with it the search.</summary>
        public ConversationList CloseSearch()
        {
            _searchRow.style.display = "none";
            _searchButton.RemoveClass("tss-convlist-header-button-active");

            if (!string.IsNullOrEmpty(_searchBox.Text) || !string.IsNullOrEmpty(_search))
            {
                _searchBox.Text = "";
                SetSearch("");
            }

            return this;
        }

        private void ToggleSearch()
        {
            if (_searchRow.style.display == "none") OpenSearch();
            else                                    CloseSearch();
        }

        private void SetSearch(string text)
        {
            text = (text ?? "").Trim();

            if (text == _search) return;

            _search = text;

            var generation = ++_searchGeneration;

            if (_searchProvider is object && text.Length > 0)
            {
                _searchResults = null;
                Searching(generation, text).FireAndForget();
            }
            else
            {
                _searchResults = null;
            }

            FilterChanged();
        }

        private async Task Searching(int generation, string text)
        {
            //A burst of keystrokes asks once, for the last one.
            await Task.Delay(250);
            if (generation != _searchGeneration) return;

            var results = await _searchProvider(text);

            if (generation != _searchGeneration) return;

            _searchResults = Distinct(results).ToArray();
            Refresh();
        }

        private void FilterChanged()
        {
            UpdateFilterChrome();
            Refresh();
            _onFilterChanged?.Invoke(CurrentFilter);
        }

        private void UpdateFilterChrome()
        {
            var parts = new List<string>();

            if (_stateFilter is object) parts.Add(StateFor(_stateFilter)?.Label ?? _stateFilter);
            if (_timeRange != ConversationTimeRange.AnyTime) parts.Add(LabelFor(_timeRange));

            if (parts.Count == 0)
            {
                _filterSummary.style.display = "none";
                _filterButton.RemoveClass("tss-convlist-header-button-active");
            }
            else
            {
                _filterSummaryText.textContent = string.Join(" · ", parts);
                _filterSummary.style.display   = "";
                _filterButton.Class("tss-convlist-header-button-active");
            }
        }

        private void ShowFilterMenu()
        {
            var items = new List<ContextMenu.Item>();

            items.Add(ContextMenuItem("Status".t()).Header());
            items.Add(FilterMenuItem("All".t(), UIcons.List, _stateFilter is null, () => FilterByState(null)));

            foreach (var state in _states)
            {
                if (!state.InFilterMenu) continue;

                var key = state.Key;
                items.Add(FilterMenuItem(state.Label, state.Icon, _stateFilter == key, () => FilterByState(key)));
            }

            items.Add(ContextMenuItem().Divider());
            items.Add(ContextMenuItem("Time".t()).Header());

            foreach (var range in new[] { ConversationTimeRange.AnyTime, ConversationTimeRange.Today, ConversationTimeRange.Past7Days, ConversationTimeRange.Past30Days, ConversationTimeRange.Older })
            {
                var r = range;
                items.Add(FilterMenuItem(LabelFor(r), IconFor(r), _timeRange == r, () => FilterByTime(r)));
            }

            ShowMenu(ContextMenu().Items(items.ToArray()), _filterButton.Render());
        }

        private static ContextMenu.Item FilterMenuItem(string label, UIcons icon, bool isSelected, Action onClick)
        {
            var check = I(UIcons.Check, cssClass: "tss-convlist-menu-check");
            if (!isSelected) check.style.visibility = "hidden";

            var content = Div(Att("tss-convlist-menu-item"), I(icon, cssClass: "tss-convlist-menu-icon"), TextDiv("tss-convlist-menu-label", label), check);

            var item = ContextMenuItem(Raw(content)).OnClick(onClick);

            if (isSelected) item.Render().setAttribute("aria-checked", "true");

            return item;
        }

        private void ShowMenu(ContextMenu menu, HTMLElement anchor)
        {
            //A menu opened from the flyout sits outside it, so moving the pointer onto the menu must not close the flyout under it.
            _openPopups++;

            menu.OnHide(() =>
            {
                _openPopups = Math.Max(0, _openPopups - 1);

                //The flyout refused to close while the menu was open, and the pointer may have left it since - nothing
                //will tell it so again, so it asks now, a moment later, once whatever the menu item did has settled.
                if (_flyoutOpen && _openPopups == 0)
                {
                    window.setTimeout(_ =>
                    {
                        if (_flyoutOpen && _openPopups == 0 && !IsHovered(_flyoutHost) && !IsHovered(_closedButton.Render())) _hideFlyout?.Invoke();
                    }, FLYOUT_DELAY_HIDE);
                }
            });

            menu.ShowFor(anchor);
        }

        private static bool IsHovered(HTMLElement element) => element.IsMounted() && element.matches(":hover");

        internal static string LabelFor(ConversationTimeRange range)
        {
            switch (range)
            {
                case ConversationTimeRange.Today:      return "Today".t();
                case ConversationTimeRange.Past7Days:  return "Past 7 days".t();
                case ConversationTimeRange.Past30Days: return "Past 30 days".t();
                case ConversationTimeRange.Older:      return "Older".t();
                default:                               return "Any time".t();
            }
        }

        private static UIcons IconFor(ConversationTimeRange range)
        {
            switch (range)
            {
                case ConversationTimeRange.Today:      return UIcons.CalendarDay;
                case ConversationTimeRange.Past7Days:  return UIcons.CalendarWeek;
                case ConversationTimeRange.Past30Days: return UIcons.CalendarDays;
                case ConversationTimeRange.Older:      return UIcons.Archive;
                default:                               return UIcons.Calendar;
            }
        }

        /// <summary>Whether <paramref name="time"/> falls in <paramref name="range"/>, measured from now.</summary>
        public static bool IsInRange(DateTimeOffset time, ConversationTimeRange range)
        {
            var now = DateTimeOffset.Now;

            switch (range)
            {
                case ConversationTimeRange.Today:      return time.LocalDateTime.Date == now.LocalDateTime.Date;
                case ConversationTimeRange.Past7Days:  return time >= now.AddDays(-7);
                case ConversationTimeRange.Past30Days: return time >= now.AddDays(-30);
                case ConversationTimeRange.Older:      return time <  now.AddDays(-30);
                default:                               return true;
            }
        }

        private StateDefinition StateFor(string key) => key is null ? null : _states.FirstOrDefault(s => s.Key == key);

        private bool Matches(ConversationListItem item, bool searchedOnServer)
        {
            var state = StateFor(item.State);

            if (_stateFilter is null)
            {
                if (state is object && !state.IncludedInAll) return false;
            }
            else if (item.State != _stateFilter)
            {
                return false;
            }

            if (!IsInRange(item.Time, _timeRange)) return false;

            if (!searchedOnServer && _search.Length > 0)
            {
                var needle = _search.ToLowerInvariant();
                var inTitle    = (item.Title    ?? "").ToLowerInvariant().Contains(needle);
                var inSubtitle = (item.Subtitle ?? "").ToLowerInvariant().Contains(needle);

                if (!inTitle && !inSubtitle && !AnyChildMatches(item, needle)) return false;
            }

            return true;
        }

        private static bool AnyChildMatches(ConversationListItem item, string lowerNeedle)
        {
            if (item.Children is null) return false;

            foreach (var child in item.Children)
            {
                if ((child.Title ?? "").ToLowerInvariant().Contains(lowerNeedle) || (child.Meta ?? "").ToLowerInvariant().Contains(lowerNeedle)) return true;
            }

            return false;
        }

        //A search that only found the conversation through one of its children has to show that child, or the hit reads as a mistake.
        private bool ForcedOpenBySearch(ConversationListItem item)
        {
            if (_search.Length == 0 || _searchProvider is object || !HasChildren(item)) return false;

            var needle = _search.ToLowerInvariant();

            return !(item.Title ?? "").ToLowerInvariant().Contains(needle) && !(item.Subtitle ?? "").ToLowerInvariant().Contains(needle) && AnyChildMatches(item, needle);
        }

        // ------------------------------------------------------------------------------------------
        // Rendering
        // ------------------------------------------------------------------------------------------

        private void TogglePinned()
        {
            _pinnedCollapsed = !_pinnedCollapsed;
            _pinnedSection.classList.toggle("tss-convlist-section-collapsed", _pinnedCollapsed);
            _pinnedHeader.setAttribute("aria-expanded", _pinnedCollapsed ? "false" : "true");
        }

        private void Refresh(bool forceRows = false)
        {
            var searching        = _search.Length > 0;
            var searchedOnServer = searching && _searchProvider is object;

            IEnumerable<ConversationListItem> source = searchedOnServer ? (IEnumerable<ConversationListItem>)(_searchResults ?? Array.Empty<ConversationListItem>()) : _items;

            var visible = source.Where(i => Matches(i, searchedOnServer)).ToArray();

            //Pinned conversations lead, as their own section - unless the user is searching, where the answer is one list.
            var pinned = searching ? Array.Empty<ConversationListItem>() : visible.Where(i => i.IsPinned).OrderByDescending(i => i.Time).ToArray();
            var rest   = searching ? visible : visible.Where(i => !i.IsPinned).ToArray();

            var keep = new HashSet<string>();

            Reconcile(_pinnedList, pinned, keep, forceRows);
            Reconcile(_recentList, rest,   keep, forceRows);

            foreach (var id in _rows.Keys.ToArray())
            {
                if (keep.Contains(id)) continue;

                _rows[id].Group.remove();
                _rows.Remove(id);
            }

            //What opens a conversation can be the search itself, so a search that moved on re-decides every row.
            foreach (var row in _rows.Values) ApplyExpanded(row);

            _pinnedSection.style.display = pinned.Length > 0 ? "" : "none";
            _pinnedCount.textContent     = pinned.Length.ToString();

            string recentLabel = null;

            if (searching)                      recentLabel = "Results".t();
            else if (_stateFilter is object)    recentLabel = StateFor(_stateFilter)?.Label ?? _stateFilter;
            else if (pinned.Length > 0)         recentLabel = "Recent".t();

            _recentHeader.textContent   = recentLabel ?? "";
            _recentHeader.style.display = recentLabel is object && rest.Length > 0 ? "" : "none";

            UpdateStatus(visible.Length, searching, searchedOnServer);
            UpdateSensor();
        }

        private void UpdateStatus(int visibleCount, bool searching, bool searchedOnServer)
        {
            ClearChildren(_statusHost);

            if (visibleCount > 0)
            {
                _statusHost.style.display = "none";
                return;
            }

            _statusHost.style.display = "";

            var waitingForSearch = searchedOnServer && _searchResults is null;

            if ((_loading && _items.Count == 0) || waitingForSearch || (_loading && !CurrentFilter.IsDefault))
            {
                _statusHost.appendChild(Spinner().Render());
                _statusHost.appendChild(TextDiv("tss-convlist-status-text", searching ? "Searching…".t() : "Loading…".t()));
                return;
            }

            if (CurrentFilter.IsDefault && _emptyComponent is object)
            {
                _statusHost.appendChild(_emptyComponent.Render());
                return;
            }

            if (CurrentFilter.IsDefault)
            {
                _statusHost.appendChild(I(UIcons.CommentAlt, cssClass: "tss-convlist-status-icon"));
                _statusHost.appendChild(TextDiv("tss-convlist-status-text", _emptyText));
                return;
            }

            _statusHost.appendChild(TextDiv("tss-convlist-status-text", searching ? "No conversations found".t() : "No conversations match these filters".t()));

            var clear = Button("Clear filters".t()).OnClick(() => ClearFilters()).LessPadding();
            clear.Link();
            clear.Class("tss-convlist-status-clear");
            _statusHost.appendChild(clear.Render());
        }

        private void UpdateSensor()
        {
            ClearChildren(_sensorHost);

            if (!_hasMore || _onLoadMore is null || _search.Length > 0) return;

            //One sensor per page: it fires once and is gone, and the next HasMore(true) arms another.
            var sensor = new VisibilitySensor(_ =>
            {
                ClearChildren(_sensorHost);
                _hasMore = false;
                _onLoadMore();
            }, singleCall: true, message: Spinner());

            _sensorHost.appendChild(sensor.Render());
        }

        private void Reconcile(HTMLElement container, ConversationListItem[] items, HashSet<string> keep, bool forceRows)
        {
            HTMLElement expected = container.firstElementChild.As<HTMLElement>();

            foreach (var item in items)
            {
                keep.Add(item.Id);

                if (!_rows.TryGetValue(item.Id, out var row))
                {
                    row = CreateRow(item);
                    _rows[item.Id] = row;
                }
                else if (forceRows || (!ReferenceEquals(row.Item, item) && row.Signature != item.Signature()) || !ReferenceEquals(row.Item.Trailing, item.Trailing))
                {
                    UpdateRow(row, item);
                }
                else
                {
                    row.Item = item;
                }

                if (expected != row.Group)
                {
                    container.insertBefore(row.Group, expected);
                }
                else
                {
                    expected = expected.nextElementSibling.As<HTMLElement>();
                }
            }

            //Whatever is left after the last expected row belongs to the other section (and is moved there next) or is gone (and Refresh removes it).
        }

        private Row CreateRow(ConversationListItem item)
        {
            var row = new Row
            {
                Dot          = Div(Att("tss-convlist-dot")),
                IconHost     = Div(Att("tss-convlist-row-icon")),
                Title        = Div(Att("tss-convlist-row-title")),
                Subtitle     = Div(Att("tss-convlist-row-subtitle")),
                TrailingHost = Div(Att("tss-convlist-row-trailing")),
                Time         = Div(Att("tss-convlist-row-time")),
            };

            var menuButton = Button().SetIcon(UIcons.MenuDots).NoBorder().NoBackground().LessPadding().Class("tss-convlist-row-menu");
            menuButton.Render().setAttribute("aria-label", "More options".t());
            menuButton.Render().setAttribute("tabindex", "-1");

            var main = Div(Att("tss-convlist-row-main"),
                           Div(Att("tss-convlist-row-line"), row.IconHost, row.Title, row.TrailingHost, row.Time),
                           row.Subtitle);

            //Both open the children, as a sidebar group's arrow does: the arrow takes the dot's place while the row
            //is hovered, and the count is there all the time, which is also what a touch screen has to tap.
            row.ToggleIcon = I(UIcons.AngleSmallRight, cssClass: "tss-convlist-toggle-icon");
            row.Toggle     = Div(Att("tss-convlist-toggle"), row.ToggleIcon);
            row.Toggle.setAttribute("role", "button");
            row.Toggle.setAttribute("tabindex", "-1");

            row.CountDot  = Div(Att("tss-convlist-count-dot"));
            row.CountText = Div(Att("tss-convlist-count-text"));
            row.Count     = Div(Att("tss-convlist-count"), row.CountDot, row.CountText);
            row.Count.setAttribute("role", "button");
            row.Count.setAttribute("tabindex", "-1");

            main.firstElementChild.insertBefore(row.Count, row.Time);

            row.Element = Div(Att("tss-convlist-row"), Div(Att("tss-convlist-dot-host"), row.Dot), row.Toggle, main, menuButton.Render());
            row.Element.setAttribute("role", "listitem");
            row.Element.setAttribute("tabindex", "0");

            row.ChildrenHost = Div(Att("tss-convlist-children"));
            row.ChildrenHost.setAttribute("role", "list");
            row.ChildrenHost.style.display = "none";

            row.Group = Div(Att("tss-convlist-group"), row.Element, row.ChildrenHost);

            row.Toggle.addEventListener("click", (Event e) =>
            {
                e.stopPropagation();
                SetExpanded(row.Item.Id, !_expanded.Contains(row.Item.Id));
            });

            row.Count.addEventListener("click", (Event e) =>
            {
                e.stopPropagation();
                SetExpanded(row.Item.Id, !_expanded.Contains(row.Item.Id));
            });

            row.Element.addEventListener("click", (Event e) =>
            {
                if (menuButton.Render().contains(e.target.As<Node>())) return;
                Activate(row.Item);
            });

            row.Element.addEventListener("keydown", (Event e) =>
            {
                var ke = e.As<KeyboardEvent>();

                if (ke.key == "Enter" || ke.key == " ")
                {
                    e.preventDefault();
                    Activate(row.Item);
                }
                else if (ke.key == "ArrowDown" || ke.key == "ArrowUp")
                {
                    e.preventDefault();
                    MoveFocus(row.Element, ke.key == "ArrowDown");
                }
                else if (HasChildren(row.Item) && ke.key == "ArrowRight")
                {
                    e.preventDefault();
                    SetExpanded(row.Item.Id, true);
                }
                else if (HasChildren(row.Item) && ke.key == "ArrowLeft")
                {
                    e.preventDefault();
                    SetExpanded(row.Item.Id, false);
                }
            });

            menuButton.OnClick((s, e) =>
            {
                e.stopPropagation();
                ShowItemMenu(row.Item, menuButton.Render());
            });

            row.Element.addEventListener("contextmenu", (Event e) =>
            {
                if (_itemMenu is null) return;
                e.preventDefault();
                ShowItemMenu(row.Item, menuButton.Render());
            });

            UpdateRow(row, item);

            return row;
        }

        private void UpdateRow(Row row, ConversationListItem item)
        {
            row.Item      = item;
            row.Signature = item.Signature();

            row.Element.dataset["convId"] = item.Id;
            row.Title.textContent         = item.Title ?? "";
            row.Element.title             = item.Title ?? "";

            row.Subtitle.textContent    = item.Subtitle ?? "";
            row.Subtitle.style.display  = string.IsNullOrEmpty(item.Subtitle) ? "none" : "";

            ClearChildren(row.IconHost);

            if (item.Icon.HasValue)
            {
                var icon = I(item.Icon.Value);
                if (!string.IsNullOrEmpty(item.IconColor)) icon.style.color = item.IconColor;
                row.IconHost.appendChild(icon);
                row.IconHost.style.display = "";
            }
            else
            {
                row.IconHost.style.display = "none";
            }

            ClearChildren(row.TrailingHost);

            if (item.Trailing is object)
            {
                row.TrailingHost.appendChild(item.Trailing.Render());
                row.TrailingHost.style.display = "";
            }
            else
            {
                row.TrailingHost.style.display = "none";
            }

            var state = StateFor(item.State);
            var tone  = state?.Tone ?? ConversationStateTone.Idle;

            row.Dot.className = "tss-convlist-dot tss-convlist-dot-" + tone.ToString().ToLowerInvariant();
            row.Dot.setAttribute("aria-label", state?.Label ?? "");
            row.Dot.title = state?.Label ?? "";

            row.Element.classList.toggle("tss-convlist-row-has-menu", _itemMenu is object);
            row.Group.classList.toggle("tss-convlist-group-has-children", HasChildren(item));

            UpdateChildren(row, item);

            UpdateTime(row);
            ApplySelected(row);
        }

        private void UpdateChildren(Row row, ConversationListItem item)
        {
            var children = item.Children ?? Array.Empty<ConversationListChild>();
            var keep     = new HashSet<string>();

            HTMLElement expected = row.ChildrenHost.firstElementChild.As<HTMLElement>();

            foreach (var child in children)
            {
                if (child?.Id is null || !keep.Add(child.Id)) continue;

                if (!row.Children.TryGetValue(child.Id, out var childRow))
                {
                    childRow = CreateChildRow(row, child);
                    row.Children[child.Id] = childRow;
                }
                else if (childRow.Signature != child.Signature() || !ReferenceEquals(childRow.Child.Leading, child.Leading))
                {
                    UpdateChildRow(childRow, child);
                }
                else
                {
                    childRow.Child = child;
                }

                if (expected != childRow.Element)
                {
                    row.ChildrenHost.insertBefore(childRow.Element, expected);
                }
                else
                {
                    expected = expected.nextElementSibling.As<HTMLElement>();
                }
            }

            foreach (var id in row.Children.Keys.ToArray())
            {
                if (keep.Contains(id)) continue;

                row.Children[id].Element.remove();
                row.Children.Remove(id);
            }

            //The count says how many are there, and how they are doing: a conversation folded shut still shows an agent at work.
            var running = 0;
            var failed  = 0;

            foreach (var id in keep)
            {
                var tone = row.Children[id].Child.Tone;
                if (tone == ConversationStateTone.Running) running++;
                else if (tone == ConversationStateTone.Danger) failed++;
            }

            row.CountText.textContent = keep.Count.ToString();
            row.Count.style.display   = keep.Count > 0 ? "" : "none";
            row.Count.className       = "tss-convlist-count" + (running > 0 ? " tss-convlist-count-running" : failed > 0 ? " tss-convlist-count-failed" : "");
            row.Count.title           = running > 0 ? string.Format("{0} running".t(), running) : keep.Count.ToString();

            ApplyExpanded(row);
        }

        private ChildRow CreateChildRow(Row parent, ConversationListChild child)
        {
            var row = new ChildRow
            {
                Status   = Div(Att("tss-convlist-child-status")),
                IconHost = Div(Att("tss-convlist-child-icon")),
                Title    = Div(Att("tss-convlist-child-title")),
                Meta     = Div(Att("tss-convlist-child-meta")),
                Time     = Div(Att("tss-convlist-child-time")),
            };

            row.Element = Div(Att("tss-convlist-child"), row.Status, row.IconHost, row.Title, row.Meta, row.Time);
            row.Element.setAttribute("role", "listitem");
            row.Element.setAttribute("tabindex", "0");

            row.Element.addEventListener("click", (Event e) => ActivateChild(row.Child, parent.Item));

            row.Element.addEventListener("keydown", (Event e) =>
            {
                var ke = e.As<KeyboardEvent>();

                if (ke.key == "Enter" || ke.key == " ")
                {
                    e.preventDefault();
                    ActivateChild(row.Child, parent.Item);
                }
                else if (ke.key == "ArrowDown" || ke.key == "ArrowUp")
                {
                    e.preventDefault();
                    MoveFocus(row.Element, ke.key == "ArrowDown");
                }
                else if (ke.key == "ArrowLeft")
                {
                    //Back to the conversation the child belongs to, folding it as a tree does.
                    e.preventDefault();
                    SetExpanded(parent.Item.Id, false);
                    parent.Element.focus();
                }
            });

            UpdateChildRow(row, child);

            return row;
        }

        private void UpdateChildRow(ChildRow row, ConversationListChild child)
        {
            row.Child     = child;
            row.Signature = child.Signature();

            row.Element.dataset["convChildId"] = child.Id;
            row.Element.title                  = child.Title ?? "";

            row.Title.textContent       = child.Title ?? "";
            row.Meta.textContent        = child.Meta ?? "";
            row.Meta.style.display      = string.IsNullOrEmpty(child.Meta) ? "none" : "";

            ClearChildren(row.Status);

            row.Status.className = "tss-convlist-child-status tss-convlist-child-status-" + child.Tone.ToString().ToLowerInvariant();
            row.Status.title     = child.StateLabel ?? "";
            row.Status.setAttribute("aria-label", child.StateLabel ?? "");

            switch (child.Tone)
            {
                case ConversationStateTone.Success:   row.Status.appendChild(I(UIcons.CheckCircle));   break;
                case ConversationStateTone.Attention: row.Status.appendChild(I(UIcons.TriangleWarning)); break;
                case ConversationStateTone.Danger:    row.Status.appendChild(I(UIcons.CrossCircle));   break;
            }

            ClearChildren(row.IconHost);

            if (child.Leading is object)
            {
                row.IconHost.appendChild(child.Leading.Render());
                row.IconHost.style.display = "";
            }
            else if (child.Icon.HasValue)
            {
                var icon = I(child.Icon.Value);
                if (!string.IsNullOrEmpty(child.IconColor)) icon.style.color = child.IconColor;
                row.IconHost.appendChild(icon);
                row.IconHost.style.display = "";
            }
            else
            {
                row.IconHost.style.display = "none";
            }

            row.Time.textContent = Relative(child.Time);
            row.Time.title       = child.Time.LocalDateTime.ToString("yyyy-MM-dd HH:mm");

            ApplyChildSelected(row);
        }

        private void ApplyChildSelected(ChildRow row)
        {
            var selected = _selectedId is object && row.Child.Id == _selectedId;

            row.Element.classList.toggle("tss-convlist-child-selected", selected);

            if (selected) row.Element.setAttribute("aria-current", "true");
            else          row.Element.removeAttribute("aria-current");
        }

        private void ActivateChild(ConversationListChild child, ConversationListItem parent)
        {
            Select(child.Id);
            _hideFlyout?.Invoke();
            _onChildSelect?.Invoke(child, parent);
        }

        private void UpdateTime(Row row)
        {
            row.Time.textContent = Relative(row.Item.Time);
            row.Time.title       = row.Item.Time.LocalDateTime.ToString("yyyy-MM-dd HH:mm");

            foreach (var child in row.Children.Values)
            {
                child.Time.textContent = Relative(child.Child.Time);
                child.Time.title       = child.Child.Time.LocalDateTime.ToString("yyyy-MM-dd HH:mm");
            }
        }

        private void ApplySelected(Row row)
        {
            var selected = _selectedId is object && row.Item.Id == _selectedId;

            row.Element.classList.toggle("tss-convlist-row-selected", selected);

            if (selected) row.Element.setAttribute("aria-current", "true");
            else          row.Element.removeAttribute("aria-current");
        }

        private void Activate(ConversationListItem item)
        {
            Select(item.Id);
            _hideFlyout?.Invoke();
            _onSelect?.Invoke(item);
        }

        private void ShowItemMenu(ConversationListItem item, HTMLElement anchor)
        {
            var items = _itemMenu?.Invoke(item);

            if (items is null || items.Length == 0) return;

            ShowMenu(ContextMenu().Items(items), anchor);
        }

        private void MoveFocus(HTMLElement from, bool down)
        {
            var rows  = _body.querySelectorAll(".tss-convlist-row, .tss-convlist-group-open .tss-convlist-child").As<NodeList>();
            var index = -1;

            for (uint i = 0; i < rows.length; i++)
            {
                if (rows[i] == from) index = (int)i;
            }

            var next = index + (down ? 1 : -1);

            if (next >= 0 && next < rows.length) rows[(uint)next].As<HTMLElement>().focus();
        }

        /// <summary>
        /// A time as the list draws it: "now" for the last minute, then minutes, hours, days and weeks
        /// ("5m", "9h", "2d", "3w"), and the date for anything older than a month.
        /// </summary>
        public static string Relative(DateTimeOffset time)
        {
            var elapsed = DateTimeOffset.Now - time;

            if (elapsed.TotalMinutes < 1)  return "now".t();
            if (elapsed.TotalHours   < 1)  return ((int)elapsed.TotalMinutes) + "m";
            if (elapsed.TotalDays    < 1)  return ((int)elapsed.TotalHours)   + "h";
            if (elapsed.TotalDays    < 7)  return ((int)elapsed.TotalDays)    + "d";
            if (elapsed.TotalDays    < 35) return ((int)(elapsed.TotalDays / 7)) + "w";

            var local = time.LocalDateTime;

            return local.Year == DateTime.Now.Year ? local.ToString("MMM d") : local.ToString("MMM yyyy");
        }

        private void StartClock()
        {
            if (_clockTimer != 0) return;

            _clockTimer = window.setInterval(_ =>
            {
                if (!_root.IsMounted())
                {
                    window.clearInterval(_clockTimer);
                    _clockTimer = 0;
                    DomObserver.WhenMounted(_root, () => StartClock());
                    return;
                }

                foreach (var row in _rows.Values) UpdateTime(row);
            }, 60_000);
        }

        private static IEnumerable<ConversationListItem> Distinct(IEnumerable<ConversationListItem> items)
        {
            var seen = new HashSet<string>();

            foreach (var item in items ?? Enumerable.Empty<ConversationListItem>())
            {
                if (item?.Id is null || !seen.Add(item.Id)) continue;
                yield return item;
            }
        }

        private static HTMLElement TextDiv(string className, string text)
        {
            var element = Div(Att(className));
            element.textContent = text ?? "";
            return element;
        }

        private static void ClearChildren(HTMLElement element)
        {
            while (element.firstChild is object) element.removeChild(element.firstChild);
        }

        // ------------------------------------------------------------------------------------------
        // The collapsed rail
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// The glyph the list leaves on a collapsed rail. Hovering, focusing or clicking it opens the list in
        /// a flyout beside it - the same list, moved into the flyout while it is open and back when it closes.
        /// </summary>
        public IComponent RenderClosed() => _closedButton;

        /// <summary>What the list shows while a sidebar holding it is open - the list itself.</summary>
        public IComponent RenderOpen() => this;

        private void ShowFlyout()
        {
            if (_flyoutOpen || !_closedButton.Render().IsMounted()) return;

            _flyoutOpen = true;
            _flyoutHost.appendChild(_root);
            _closedButton.Class("tss-convlist-rail-button-open");

            Tippy.ShowFor(_closedButton.Render(), _flyoutHost, out var hide,
                placement:        TooltipPlacement.RightStart,
                delayHide:        FLYOUT_DELAY_HIDE,
                maxWidth:         400,
                hideOnClick:      false,
                interactiveBorder: 24,
                theme:            "tss-convlist-flyout-theme",
                onHide:           () => _openPopups == 0,
                onClickOutside:   (instance, e) => { if (_openPopups == 0) _hideFlyout?.Invoke(); },
                onHiddenCallback: () => FlyoutHidden());

            _hideFlyout = hide;
        }

        private void FlyoutHidden()
        {
            _flyoutOpen = false;
            _hideFlyout = null;
            _closedButton.RemoveClass("tss-convlist-rail-button-open");

            //Back where the open sidebar shows it, so opening the sidebar finds the list as the flyout left it.
            _openHost.appendChild(_root);
        }

        // ------------------------------------------------------------------------------------------
        // ISidebarItem
        // ------------------------------------------------------------------------------------------

        /// <summary>Gets or sets whether the item is selected. The list owns its own selection, so this is only carried for the sidebar's sake.</summary>
        public bool IsSelected { get; set; }

        /// <summary>Gets the component that is currently rendered.</summary>
        public IComponent CurrentRendered => _closedButton.Render().IsMounted() ? (IComponent)_closedButton : this;

        /// <summary>Gets the full identifier of the item, including group identifiers.</summary>
        public string Identifier { get; private set; } = "tss-conversation-list";

        /// <summary>Gets the own identifier of the item, without group identifiers.</summary>
        public string OwnIdentifier => Sidebar.GetOwnIdentifier(Identifier);

        /// <summary>Adds a group identifier prefix to the item's identifier.</summary>
        public void AddGroupIdentifier(string groupIdentifier)
        {
            Identifier = Sidebar.WithGroupIdentifier(Identifier, groupIdentifier);
        }

        /// <summary>Shows the list (and its rail glyph).</summary>
        public void Show()
        {
            _openHost.style.display = "";
            _closedButton.Show();
        }

        /// <summary>Hides the list (and its rail glyph).</summary>
        public void Collapse()
        {
            _openHost.style.display = "none";
            _closedButton.Collapse();
        }
    }
}
