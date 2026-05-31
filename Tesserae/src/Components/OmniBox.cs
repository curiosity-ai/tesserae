using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A heavyweight omni-search / autocomplete component with inline filter chips, multiple search modes
    /// (search/chat) and async value providers.
    /// </summary>
    [H5.Name("tss.OmniBox")]
    public class OmniBox : IComponent, IHasBackgroundColor, ITabIndex
    {

        public class InlineFilterChip
        {
            /// <summary>
            /// Gets or sets the name of the component.
            /// </summary>
            public string Name { get; }
            /// <summary>
            /// Gets or sets the color of the component.
            /// </summary>
            public string Color { get; }
            /// <summary>
            /// Gets or sets the CSS background of the component.
            /// </summary>
            public string Background { get; }
            /// <summary>
            /// Gets or sets the removable.
            /// </summary>
            public bool Removable { get; }
            /// <summary>
            /// Registers a callback invoked when the click event fires.
            /// </summary>
            public Action<MouseEvent> OnClick { get; }
            internal IComponent Content { get; }

            private readonly SnapHandler _snap;
            internal SnapHandler Snap => _snap;

            private readonly FilterSnap _filterSnap;
            internal FilterSnap FilterSnap => _filterSnap;

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public InlineFilterChip(string name, string background = null, string color = null, Action<MouseEvent> onClick = null, bool removable = true)
            {
                Name = name;
                Background = background;
                Color = color;
                OnClick = onClick;
                Removable = removable;
            }

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public InlineFilterChip(IComponent content, Action<MouseEvent> onClick = null, bool removable = true)
            {
                Content = content;
                OnClick = onClick;
                Removable = removable;
            }

            internal InlineFilterChip(SnapHandler snap)
            {
                _snap = snap;
                Name = snap.DisplayName;
                Background = snap.Background;
                Color = snap.Color;
                Removable = true;
                if (snap.Icon != null)
                {
                    Content = HStack().AlignItemsCenter().Children(
                        snap.Icon,
                        TextBlock(snap.DisplayName).PaddingLeft(4.px()));
                }
            }

            internal InlineFilterChip(FilterSnap filterSnap)
            {
                _filterSnap = filterSnap;
                var handler = filterSnap.Handler;
                Name = filterSnap.Trigger + ":" + filterSnap.Value;
                Background = handler.Background;
                Color = handler.Color;
                Removable = true;
                if (handler.Icon != null)
                {
                    Content = HStack().AlignItemsCenter().Children(
                        handler.Icon,
                        TextBlock(Name).PaddingLeft(4.px()));
                }
            }
        }

        public class FilterSnapHandler
        {
            /// <summary>
            /// Gets or sets the filter id.
            /// </summary>
            public string FilterId { get; }
            /// <summary>
            /// Gets or sets the display name.
            /// </summary>
            public string DisplayName { get; }
            /// <summary>
            /// Gets or sets the description of the component.
            /// </summary>
            public string Description { get; }
            /// <summary>
            /// Gets or sets the trigger words.
            /// </summary>
            public string[] TriggerWords { get; }
            /// <summary>
            /// Gets or sets the icon shown by the component.
            /// </summary>
            public IComponent Icon { get; }
            /// <summary>
            /// Gets or sets the CSS background of the component.
            /// </summary>
            public string Background { get; }
            /// <summary>
            /// Gets or sets the color of the component.
            /// </summary>
            public string Color { get; }

            private readonly string[] _values;
            private readonly Func<string, Task<string[]>> _valuesProvider;
            private readonly bool _isTimeRange;

            /// <summary>
            /// Gets a value indicating whether this is a time/date-range filter snap. When <c>true</c>, the
            /// suggestion popup shows a date-range selector (plus shortcuts) instead of a list of values, and the
            /// committed <see cref="FilterSnap.Value"/> is a sortable <c>yyyy-MM-dd:yyyy-MM-dd</c> range string.
            /// </summary>
            public bool IsTimeRange => _isTimeRange;

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public FilterSnapHandler(string filterId, string displayName, string[] triggerWords, string[] values, IComponent icon = null, string description = null, string background = null, string color = null)
                : this(filterId, displayName, triggerWords, icon, description, background, color, false)
            {
                if (values == null) throw new ArgumentNullException(nameof(values));
                _values = values;
            }

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public FilterSnapHandler(string filterId, string displayName, string[] triggerWords, Func<string, Task<string[]>> valuesProvider, IComponent icon = null, string description = null, string background = null, string color = null)
                : this(filterId, displayName, triggerWords, icon, description, background, color, false)
            {
                if (valuesProvider == null) throw new ArgumentNullException(nameof(valuesProvider));
                _valuesProvider = valuesProvider;
            }

            private FilterSnapHandler(string filterId, string displayName, string[] triggerWords, IComponent icon, string description, string background, string color, bool isTimeRange)
            {
                if (string.IsNullOrEmpty(filterId)) throw new ArgumentException("filterId is required", nameof(filterId));
                if (string.IsNullOrEmpty(displayName)) throw new ArgumentException("displayName is required", nameof(displayName));
                if (triggerWords == null || triggerWords.Length == 0) throw new ArgumentException("At least one trigger word is required", nameof(triggerWords));
                FilterId = filterId;
                DisplayName = displayName;
                TriggerWords = triggerWords;
                Icon = icon;
                Description = description;
                Background = background;
                Color = color;
                _isTimeRange = isTimeRange;
            }

            /// <summary>
            /// Creates a time/date-range filter snap handler. The user types a range in a sortable date-time format
            /// separated by a colon (<c>yyyy-MM-dd:yyyy-MM-dd</c>), and the suggestion popup shows a date-range picker
            /// together with quick shortcuts (last week, last month, last 90 days, last year). Granularity is a single day.
            /// The committed <see cref="FilterSnap.Value"/> is the <c>yyyy-MM-dd:yyyy-MM-dd</c> range string, which can be
            /// parsed back via <see cref="FilterSnap.TryGetDateRange"/>.
            /// </summary>
            public static FilterSnapHandler TimeRange(string filterId, string displayName, string[] triggerWords, IComponent icon = null, string description = null, string background = null, string color = null)
            {
                return new FilterSnapHandler(filterId, displayName, triggerWords, icon, description, background, color, true);
            }

            internal async Task<string[]> GetValuesAsync(string input)
            {
                string[] raw;
                if (_values != null)
                {
                    raw = _values;
                }
                else
                {
                    raw = await _valuesProvider(input ?? string.Empty) ?? Array.Empty<string>();
                }

                var filtered = new List<string>(raw.Length);
                foreach (var v in raw)
                {
                    if (string.IsNullOrEmpty(v)) continue;
                    if (ContainsWhitespace(v)) continue;
                    if (_values != null && !string.IsNullOrEmpty(input) && v.IndexOf(input, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    filtered.Add(v);
                }
                return filtered.ToArray();
            }

            private static bool ContainsWhitespace(string s)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (char.IsWhiteSpace(s[i])) return true;
                }
                return false;
            }
        }

        public class FilterSnap
        {
            /// <summary>
            /// Gets or sets the handler.
            /// </summary>
            public FilterSnapHandler Handler { get; }
            /// <summary>
            /// Gets or sets the filter id.
            /// </summary>
            public string FilterId => Handler.FilterId;
            /// <summary>
            /// Gets or sets the trigger.
            /// </summary>
            public string Trigger { get; }
            /// <summary>
            /// Gets or sets the current value of the component.
            /// </summary>
            public string Value { get; }

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public FilterSnap(FilterSnapHandler handler, string trigger, string value)
            {
                Handler = handler;
                Trigger = trigger;
                Value = value;
            }

            /// <summary>
            /// Attempts to parse <see cref="Value"/> as a day-granularity date range in the
            /// <c>yyyy-MM-dd:yyyy-MM-dd</c> format produced by a time-range filter snap.
            /// </summary>
            /// <param name="from">The inclusive start of the range when parsing succeeds.</param>
            /// <param name="to">The inclusive end of the range when parsing succeeds.</param>
            /// <returns><c>true</c> when both bounds parse and <paramref name="from"/> is not after <paramref name="to"/>.</returns>
            public bool TryGetDateRange(out DateTime from, out DateTime to)
            {
                from = default;
                to = default;
                if (string.IsNullOrEmpty(Value)) return false;
                var parts = Value.Split(':');
                if (parts.Length != 2) return false;
                if (!DateTime.TryParseExact(parts[0], "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, out from)) return false;
                if (!DateTime.TryParseExact(parts[1], "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, out to)) return false;
                return from <= to;
            }
        }

        public class SnapHandler
        {
            /// <summary>
            /// Gets or sets the snap id.
            /// </summary>
            public string SnapId { get; }
            /// <summary>
            /// Gets or sets the display name.
            /// </summary>
            public string DisplayName { get; }
            /// <summary>
            /// Gets or sets the description of the component.
            /// </summary>
            public string Description { get; }
            /// <summary>
            /// Gets or sets the trigger words.
            /// </summary>
            public string[] TriggerWords { get; }
            /// <summary>
            /// Gets or sets the icon shown by the component.
            /// </summary>
            public IComponent Icon { get; }
            /// <summary>
            /// Gets or sets the CSS background of the component.
            /// </summary>
            public string Background { get; }
            /// <summary>
            /// Gets or sets the color of the component.
            /// </summary>
            public string Color { get; }
            /// <summary>
            /// Gets or sets the exclusive.
            /// </summary>
            public bool Exclusive { get; }

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public SnapHandler(string snapId, string displayName, string[] triggerWords, IComponent icon = null, string description = null, string background = null, string color = null, bool exclusive = false)
            {
                if (string.IsNullOrEmpty(snapId)) throw new ArgumentException("snapId is required", nameof(snapId));
                if (string.IsNullOrEmpty(displayName)) throw new ArgumentException("displayName is required", nameof(displayName));
                if (triggerWords == null || triggerWords.Length == 0) throw new ArgumentException("At least one trigger word is required", nameof(triggerWords));
                SnapId = snapId;
                DisplayName = displayName;
                TriggerWords = triggerWords;
                Icon = icon;
                Description = description;
                Background = background;
                Color = color;
                Exclusive = exclusive;
            }
        }

        public class OmniBoxSuggestionItem
        {
            /// <summary>
            /// Gets or sets the icon shown by the component.
            /// </summary>
            public IComponent Icon { get; }
            /// <summary>
            /// Gets or sets the text shown in the component.
            /// </summary>
            public IComponent Text { get; }
            /// <summary>
            /// Gets or sets the right component.
            /// </summary>
            public IComponent RightComponent { get; }
            /// <summary>
            /// Registers a callback invoked when the selected event fires.
            /// </summary>
            public Action<OmniBox> OnSelected { get; }
            /// <summary>
            /// Gets or sets the category.
            /// </summary>
            public string Category { get; }

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public OmniBoxSuggestionItem(IComponent text, IComponent icon = null, IComponent rightComponent = null, Action<OmniBox> onSelected = null, string category = null)
            {
                Text = text;
                Icon = icon;
                RightComponent = rightComponent;
                if (onSelected is null)
                {
                    OnSelected = (b) => b.SearchText = text.Render().textContent;
                }
                else
                {
                    OnSelected = onSelected;
                }
                Category = category;
            }

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public OmniBoxSuggestionItem(string text, IComponent icon = null, IComponent rightComponent = null, Action<OmniBox> onSelected = null, string category = null) : this(TextBlock(text), icon, rightComponent, onSelected, category)
            {
            }
        }

        public class Config
        {
            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public Config(Mode mode, Mode? initialMode = null)
            {
                Mode = mode;
                InitialMode = initialMode ?? (mode == Mode.SearchAndChat ? Mode.Search : mode);
                IconSearch = UIcons.Search;
                IconChat = UIcons.PaperPlane;
                IconStop = UIcons.StopCircle;
                IconModeToggleSearch = UIcons.Search;
                IconModeToggleChat = UIcons.Beacon;
                TooltipModeToggleSearch = "Search";
                TooltipModeToggleChat = "Chat";
                ExpandOnFocus = false;
                TokenIgnoreCase = false;
            }

            /// <summary>
            /// Gets or sets the placeholder search.
            /// </summary>
            public string PlaceholderSearch { get; set; }
            /// <summary>
            /// Gets or sets the placeholder chat.
            /// </summary>
            public string PlaceholderChat { get; set; }
            /// <summary>
            /// Gets or sets the mode.
            /// </summary>
            public Mode Mode { get; }
            /// <summary>
            /// Gets or sets the initial mode.
            /// </summary>
            public Mode InitialMode { get; }

            /// <summary>
            /// Gets or sets the icon search.
            /// </summary>
            public UIcons IconSearch { get; set; }
            /// <summary>
            /// Gets or sets the icon chat.
            /// </summary>
            public UIcons IconChat { get; set; }
            /// <summary>
            /// Gets or sets the icon stop.
            /// </summary>
            public UIcons IconStop { get; set; }
            /// <summary>
            /// Gets or sets the icon mode toggle search.
            /// </summary>
            public UIcons IconModeToggleSearch { get; set; }
            /// <summary>
            /// Gets or sets the icon mode toggle chat.
            /// </summary>
            public UIcons IconModeToggleChat { get; set; }
            /// <summary>
            /// Gets or sets the tooltip mode toggle search.
            /// </summary>
            public string TooltipModeToggleSearch { get; set; }
            /// <summary>
            /// Gets or sets the tooltip mode toggle chat.
            /// </summary>
            public string TooltipModeToggleChat { get; set; }
            /// <summary>
            /// Gets or sets the expand on focus.
            /// </summary>
            public bool ExpandOnFocus { get; set; }
            /// <summary>
            /// Gets or sets the token ignore case.
            /// </summary>
            public bool TokenIgnoreCase { get; set; }

            /// <summary>
            /// Gets or sets the chat footer.
            /// </summary>
            public FooterItems ChatFooter { get; set; }
            /// <summary>
            /// Gets or sets the search footer.
            /// </summary>
            public FooterItems SearchFooter { get; set; }

            /// <summary>
            /// Gets or sets the suggestions fetcher.
            /// </summary>
            public Func<string, Task<OmniBoxSuggestionItem[]>> SuggestionsFetcher { get; set; }
        }

        public class FooterItems
        {
            /// <summary>
            /// Gets or sets the left side.
            /// </summary>
            public IComponent[] LeftSide { get; set; }
            /// <summary>
            /// Gets or sets the right side.
            /// </summary>
            public IComponent[] RightSide { get; set; }
        }

        public enum Mode
        {
            Search,
            Chat,
            SearchAndChat
        }

        public enum ThinkingEffort
        {
            Disable,
            Low,
            Medium,
            High
        }

        public class ModelOption
        {
            /// <summary>
            /// Gets or sets the name of the component.
            /// </summary>
            public string Name { get; }
            /// <summary>
            /// Gets or sets the description of the component.
            /// </summary>
            public string Description { get; }
            /// <summary>
            /// Gets or sets the tag.
            /// </summary>
            public object Tag { get; }

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public ModelOption(string name, string description = null, object tag = null)
            {
                Name = name;
                Description = description;
                Tag = tag;
            }
        }


        private readonly Mode _mode;
        private readonly SettableObservable<Mode> _activeMode;
        private HTMLElement _activeInput;

        private readonly HTMLDivElement   _container;
        private readonly HTMLDivElement   _searchContainer;
        private readonly HTMLInputElement _searchInput;

        /// <summary>
        /// Gets or sets the inline filter chips.
        /// </summary>
        public ObservableList<InlineFilterChip> InlineFilterChips { get; }
        private readonly HTMLDivElement _searchInlineChipsContainer;
        private readonly HTMLDivElement _searchRightTextContainer;

        private readonly HTMLDivElement   _searchTokensContainer;
        private readonly HTMLDivElement   _searchInputContainer;
        private readonly HTMLDivElement   _searchShortcutContainer;
        private readonly Button      _searchHistoryBtn;
        private readonly Button      _searchClearBtn;
        private readonly Button      _searchTriggerBtn;

        private string[]      _shortcutKeys;
        private Action<Event> _globalShortcutHandler;

        private readonly HTMLTextAreaElement _chatInput;
        private readonly HTMLDivElement   _chatContainer;
        private readonly Button           _chatTriggerBtn;
        private Button                    _modelSelectorBtn;
        private List<ModelOption>         _models = new List<ModelOption>();
        private ModelOption               _selectedModel;
        private ThinkingEffort            _selectedEffort = ThinkingEffort.Medium;
        private bool                      _isModelLocked;
        private Action                    _hideModelPopover;
        private readonly HTMLDivElement _footer;
        private Func<Task<SearchQuery[]>> _historyFetcher;
        private Func<string, Task<OmniBoxSuggestionItem[]>> _suggestionsFetcher;
        private int _suggestionsDebounceTimeoutId = 0;
        private Action _hideSuggestions;
        private int _highlightedSuggestionIndex = -1;
        private List<Button> _currentSuggestionButtons = new List<Button>();

        private readonly List<SnapHandler> _snapHandlers = new List<SnapHandler>();
        private Action _hideSnapSuggestions;
        private int _highlightedSnapSuggestionIndex = -1;
        private List<Button> _currentSnapSuggestionButtons = new List<Button>();
        private SnapHandler[] _currentSnapMatches;
        private int _snapMentionStart = -1;
        private int _snapMentionEnd = -1;

        private readonly List<FilterSnapHandler> _filterSnapHandlers = new List<FilterSnapHandler>();
        private Action _hideFilterSnapSuggestions;
        private int _highlightedFilterSnapSuggestionIndex = -1;
        private List<Button> _currentFilterSnapSuggestionButtons = new List<Button>();
        private int _filterSnapStart = -1;
        private int _filterSnapEnd = -1;
        private int _filterSnapRequestId = 0;
        private bool _timeFilterSnapOpen = false;
        private string _tokenTypeMap = string.Empty;
        private readonly IconToggle<Mode> _modeToggle;
        private readonly bool _tokenIgnoreCase;
        private bool _isGenerating = false;
        private readonly UIcons _iconChat;
        private readonly UIcons _iconStop;

        public delegate void SearchEventHandler(OmniBox sender, SearchQuery query);
        public delegate void ChatEventHandler(OmniBox sender, ChatMessage query);
        public delegate void StopEventHandler(OmniBox sender);
        public delegate void ModelChangedEventHandler(OmniBox sender, ModelOption model, ThinkingEffort effort);
        protected event SearchEventHandler Searched;
        protected event ChatEventHandler Chatted;
        /// <summary>
        /// Raised when stopped occurs.
        /// </summary>
        public event StopEventHandler Stopped;
        protected event ModelChangedEventHandler ModelChanged;

        /// <summary>
        /// Raised when input occurs.
        /// </summary>
        public event ComponentEventHandler<OmniBox, Event> Input;
        /// <summary>
        /// Raised when key down occurs.
        /// </summary>
        public event ComponentEventHandler<OmniBox, KeyboardEvent> KeyDown;
        /// <summary>
        /// Raised when key up occurs.
        /// </summary>
        public event ComponentEventHandler<OmniBox, KeyboardEvent> KeyUp;
        /// <summary>
        /// Raised when key press occurs.
        /// </summary>
        public event ComponentEventHandler<OmniBox, KeyboardEvent> KeyPress;
        /// <summary>
        /// Raised when received focus occurs.
        /// </summary>
        public event ComponentEventHandler<OmniBox, Event> ReceivedFocus;
        /// <summary>
        /// Raised when lost focus occurs.
        /// </summary>
        public event ComponentEventHandler<OmniBox, Event> LostFocus;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public OmniBox(Config config)
        {
            _mode = config.Mode;
            _tokenIgnoreCase = config.TokenIgnoreCase;
            _iconChat = config.IconChat;
            _iconStop = config.IconStop;
            _suggestionsFetcher = config.SuggestionsFetcher;

            _activeMode = new SettableObservable<Mode>(config.InitialMode);

            _footer = Div(_("tss-omnibox-footer"));

            var footerEnd = new List<IComponent>();

            if (_mode == Mode.SearchAndChat)
            {
                _modeToggle = IconToggle(IconToggleItem(config.IconModeToggleChat, config.TooltipModeToggleChat, Mode.Chat), IconToggleItem(config.IconModeToggleSearch, config.TooltipModeToggleSearch, Mode.Search));
                _modeToggle.Select(_activeMode.Value);
                _modeToggle.AsObservable().ObserveFutureChanges(v => _activeMode.Value = v);
                _footer.appendChild(_modeToggle.Render());
            }


            if (_mode == Mode.Search || _mode == Mode.SearchAndChat)
            {

                InlineFilterChips = new ObservableList<InlineFilterChip>();
                _searchInlineChipsContainer = Div(_("tss-omnibox-inline-chips"));
                _searchRightTextContainer = Div(_("tss-omnibox-right-text"));
                _searchRightTextContainer.style.display = "none";

                InlineFilterChips.Observe(RenderInlineChips);

                _searchInput = TextBox(_("tss-omnibox-search-input", type: "text", placeholder: config.PlaceholderSearch ?? ""));
                _searchInput.autocomplete = "off";
                _searchInput.spellcheck = false;

                _searchTokensContainer = Div(_("tss-omnibox-search-tokens"));
                _searchInputContainer = Div(_("tss-omnibox-search-input-container"), _searchInput, _searchTokensContainer);

                _searchShortcutContainer = Div(_("tss-omnibox-shortcut"));

                _searchHistoryBtn = Button().SetIcon(UIcons.TimePast).Class("tss-omnibox-search-history-btn");
                _searchHistoryBtn.Collapse(); // Hidden by default unless WithHistory is called

                _searchClearBtn = Button().SetIcon(UIcons.CrossCircle).Class("tss-omnibox-search-clear-btn");
                _searchTriggerBtn = Button().SetIcon(config.IconSearch).Class("tss-omnibox-search-btn");

                if (_mode == Mode.Search)
                {
                    _searchContainer = Div(_("tss-omnibox-search-container"), _searchHistoryBtn.Render(), _searchInlineChipsContainer, _searchInputContainer, _searchRightTextContainer, _searchShortcutContainer, _searchClearBtn.Render(), _searchTriggerBtn.Render());
                }
                else
                {
                    _searchContainer = Div(_("tss-omnibox-search-container"), _searchInlineChipsContainer, _searchInputContainer, _searchRightTextContainer, _searchShortcutContainer);
                    _footer.appendChild(_searchHistoryBtn.Render());
                    footerEnd.Add(_searchClearBtn);
                    footerEnd.Add(_searchTriggerBtn);
                }

                // Set up event listeners
                _searchInput.addEventListener("input", (e) =>
                {
                    OnSearchInputChanged();
                    Input?.Invoke(this, e);
                    if (TryUpdateFilterSnapSuggestions())
                    {
                        HideSnapSuggestions();
                        HideRegularSuggestions();
                    }
                    else if (TryUpdateSnapSuggestions())
                    {
                        HideFilterSnapSuggestions();
                        HideRegularSuggestions();
                    }
                    else
                    {
                        HideFilterSnapSuggestions();
                        TriggerSuggestions();
                    }
                });

                _searchInput.addEventListener("keydown", (e) =>
                {
                    var ke = e.As<KeyboardEvent>();
                    if (ke.key == "Enter")
                    {
                        if (_hideFilterSnapSuggestions != null && _currentFilterSnapSuggestionButtons.Count > 0)
                        {
                            StopEvent(e);
                            var idx = _highlightedFilterSnapSuggestionIndex >= 0 ? _highlightedFilterSnapSuggestionIndex : 0;
                            if (idx < _currentFilterSnapSuggestionButtons.Count)
                            {
                                _currentFilterSnapSuggestionButtons[idx].RaiseOnClick(e.As<MouseEvent>());
                            }
                            return;
                        }
                        if (_hideSnapSuggestions != null && _currentSnapSuggestionButtons.Count > 0)
                        {
                            StopEvent(e);
                            var idx = _highlightedSnapSuggestionIndex >= 0 ? _highlightedSnapSuggestionIndex : 0;
                            if (idx < _currentSnapSuggestionButtons.Count)
                            {
                                _currentSnapSuggestionButtons[idx].RaiseOnClick(e.As<MouseEvent>());
                            }
                            return;
                        }
                        if (_hideSuggestions != null && _highlightedSuggestionIndex >= 0 && _highlightedSuggestionIndex < _currentSuggestionButtons.Count)
                        {
                            StopEvent(e);
                            _currentSuggestionButtons[_highlightedSuggestionIndex].RaiseOnClick(e.As<MouseEvent>());
                        }
                        else
                        {
                            StopEvent(e);
                            TriggerSearch();
                        }
                        return;
                    }
                    else if (ke.key == "Tab")
                    {
                        if (_hideFilterSnapSuggestions != null && _currentFilterSnapSuggestionButtons.Count > 0)
                        {
                            StopEvent(e);
                            var idx = _highlightedFilterSnapSuggestionIndex >= 0 ? _highlightedFilterSnapSuggestionIndex : 0;
                            if (idx < _currentFilterSnapSuggestionButtons.Count)
                            {
                                _currentFilterSnapSuggestionButtons[idx].RaiseOnClick(e.As<MouseEvent>());
                            }
                            return;
                        }
                        if (_hideSnapSuggestions != null && _currentSnapSuggestionButtons.Count > 0)
                        {
                            StopEvent(e);
                            var idx = _highlightedSnapSuggestionIndex >= 0 ? _highlightedSnapSuggestionIndex : 0;
                            if (idx < _currentSnapSuggestionButtons.Count)
                            {
                                _currentSnapSuggestionButtons[idx].RaiseOnClick(e.As<MouseEvent>());
                            }
                            return;
                        }
                    }
                    else if (ke.key == "Escape")
                    {
                        if (_hideFilterSnapSuggestions != null)
                        {
                            StopEvent(e);
                            HideFilterSnapSuggestions();
                            return;
                        }
                        if (_hideSnapSuggestions != null)
                        {
                            StopEvent(e);
                            HideSnapSuggestions();
                            return;
                        }
                    }
                    else if (ke.key == "Backspace")
                    {
                        var cursorPos = _searchInput.selectionStart;
                        if (cursorPos == 0 && _searchInput.selectionEnd == 0)
                        {
                            if (InlineFilterChips.Count > 0)
                            {
                                InlineFilterChips.RemoveAt(InlineFilterChips.Count - 1);
                                StopEvent(e);
                                return;
                            }
                        }

                        if (cursorPos > 0 && cursorPos == _searchInput.selectionEnd)
                        {
                            // Check if the character to be deleted is a non-breaking space
                            if (_searchInput.value[(int)cursorPos - 1] == specialWhitespace &&
                                (int)cursorPos >= 2 &&
                                _tokenTypeMap.Length > (int)cursorPos - 2 &&
                                _tokenTypeMap[(int)cursorPos - 2] == '1')
                            {
                                // If it is, and we're backspacing, we actually want to delete the character *before* it,
                                // which is the special token, because the user doesn't know the nb-space exists.
                                // However, we can just delete the character before the nb-space and the nb-space
                                StopEvent(e);
                                var currentVal = _searchInput.value;
                                var newVal = currentVal.Substring(0, (int)cursorPos - 2) + currentVal.Substring((int)cursorPos);
                                _searchInput.value = newVal;
                                _searchInput.setSelectionRange((uint)cursorPos - 2, (uint)cursorPos - 2);
                                OnSearchInputChanged();
                            }
                        }
                    }
                    else if (ke.key == "Delete")
                    {
                        var cursorPos = _searchInput.selectionStart;
                        if (cursorPos < _searchInput.value.Length && cursorPos == _searchInput.selectionEnd)
                        {
                            if (_searchInput.value[(int)cursorPos] == specialWhitespace)
                            {
                                // if we press delete on an nb-space, we probably want to delete the token *after* it
                                if (cursorPos + 2 <= _searchInput.value.Length)
                                {
                                    StopEvent(e);
                                    var currentVal = _searchInput.value;
                                    var newVal = currentVal.Substring(0, (int)cursorPos) + currentVal.Substring((int)cursorPos + 2);
                                    _searchInput.value = newVal;
                                    _searchInput.setSelectionRange((uint)cursorPos, (uint)cursorPos);
                                    OnSearchInputChanged();
                                }
                                else
                                {
                                    // Edge case: space is at the very end of the string
                                    StopEvent(e);
                                    var currentVal = _searchInput.value;
                                    var newVal = currentVal.Substring(0, (int)cursorPos);
                                    _searchInput.value = newVal;
                                    _searchInput.setSelectionRange((uint)cursorPos, (uint)cursorPos);
                                    OnSearchInputChanged();
                                }
                            }
                        }
                    }
                    else if (ke.key == "ArrowDown")
                    {
                        if (_hideFilterSnapSuggestions != null && _currentFilterSnapSuggestionButtons.Count > 0)
                        {
                            StopEvent(e);
                            UpdateHighlightedFilterSnapSuggestion(1);
                        }
                        else if (_hideSnapSuggestions != null && _currentSnapSuggestionButtons.Count > 0)
                        {
                            StopEvent(e);
                            UpdateHighlightedSnapSuggestion(1);
                        }
                        else if (_hideSuggestions != null && _currentSuggestionButtons.Count > 0)
                        {
                            StopEvent(e);
                            UpdateHighlightedSuggestion(1);
                        }
                    }
                    else if (ke.key == "ArrowUp")
                    {
                        if (_hideFilterSnapSuggestions != null && _currentFilterSnapSuggestionButtons.Count > 0)
                        {
                            StopEvent(e);
                            UpdateHighlightedFilterSnapSuggestion(-1);
                        }
                        else if (_hideSnapSuggestions != null && _currentSnapSuggestionButtons.Count > 0)
                        {
                            StopEvent(e);
                            UpdateHighlightedSnapSuggestion(-1);
                        }
                        else if (_hideSuggestions != null && _currentSuggestionButtons.Count > 0)
                        {
                            StopEvent(e);
                            UpdateHighlightedSuggestion(-1);
                        }
                    }
                });
                _searchInput.addEventListener("keyup", (e) => KeyUp?.Invoke(this, e.As<KeyboardEvent>()));
                _searchInput.addEventListener("keypress", (e) => KeyPress?.Invoke(this, e.As<KeyboardEvent>()));
                _searchInput.addEventListener("keydown", (e) => KeyDown?.Invoke(this, e.As<KeyboardEvent>()));
                _searchInput.addEventListener("focus", (e) => ReceivedFocus?.Invoke(this, e));
                _searchInput.addEventListener("blur", (e) => {
                    LostFocus?.Invoke(this, e);
                    if (_hideSuggestions != null) window.setTimeout(_ => { if (_hideSuggestions != null) _hideSuggestions(); _highlightedSuggestionIndex = -1; }, 200);
                    if (_hideSnapSuggestions != null) window.setTimeout(_ => { HideSnapSuggestions(); }, 200);
                    // Keep an open time-range popup alive when focus moves into its date inputs; that popup
                    // dismisses itself via Tippy's click-outside / commit instead of the blur timeout.
                    if (_hideFilterSnapSuggestions != null && !_timeFilterSnapOpen) window.setTimeout(_ => { HideFilterSnapSuggestions(); }, 200);
                });

                _searchInput.addEventListener("scroll", (e) => SyncScroll());

                _searchInput.addEventListener("copy", (e) =>
                {
                    var clipboardEvent = e.As<ClipboardEvent>();
                    var start = _searchInput.selectionStart;
                    var end = _searchInput.selectionEnd;
                    if (start != end)
                    {
                        var selectedText = _searchInput.value.Substring((int)start, (int)(end - start));
                        clipboardEvent.clipboardData.setData("text/plain", selectedText.Replace(specialWhitespaceString, " "));
                        StopEvent(e);
                    }
                });

                _searchInput.addEventListener("cut", (e) =>
                {
                    var clipboardEvent = e.As<ClipboardEvent>();
                    var start = _searchInput.selectionStart;
                    var end = _searchInput.selectionEnd;
                    if (start != end)
                    {
                        var selectedText = _searchInput.value.Substring((int)start, (int)(end - start));
                        clipboardEvent.clipboardData.setData("text/plain", selectedText.Replace(specialWhitespaceString, " "));

                        var currentVal = _searchInput.value;
                        _searchInput.value = currentVal.Substring(0, (int)start) + currentVal.Substring((int)end);
                        _searchInput.setSelectionRange((uint)start, (uint)start);
                        OnSearchInputChanged();
                        StopEvent(e);
                    }
                });

                _searchClearBtn.OnClick(() =>
                {
                    _searchInput.value = string.Empty;
                    OnSearchInputChanged();
                    _searchInput.focus();
                });

                _searchTriggerBtn.OnClick(TriggerSearch);

                _searchHistoryBtn.OnClickSpinWhile(async () => 
                {
                    if (_historyFetcher != null)
                    {
                        await ShowSearchHistory();
                    }
                });

                // Initial parse
                OnSearchInputChanged();
            }

            if (_mode == Mode.Chat || _mode == Mode.SearchAndChat)
            {
                _chatInput = TextArea(_("tss-omnibox-chat-input", type: "text", placeholder: config.PlaceholderChat ?? ""));
                _chatInput.spellcheck = true;

                _chatInput.addEventListener("keydown", (e) =>
                {
                    var ke = e.As<KeyboardEvent>();
                    KeyDown?.Invoke(this, ke);

                    if (ke.key == "Enter" && !ke.shiftKey)
                    {
                        TriggerChat();
                        StopEvent(e);
                    }
                });

                _chatInput.addEventListener("input", (e) =>
                {
                    UpdateChatTriggerActiveState();
                    Input?.Invoke(this, e);
                });
                _chatInput.addEventListener("keyup", (e) => KeyUp?.Invoke(this, e.As<KeyboardEvent>()));
                _chatInput.addEventListener("keypress", (e) => KeyPress?.Invoke(this, e.As<KeyboardEvent>()));
                _chatInput.addEventListener("focus", (e) => ReceivedFocus?.Invoke(this, e));
                _chatInput.addEventListener("blur", (e) => LostFocus?.Invoke(this, e));

                _chatContainer = Div(_("tss-omnibox-chat-container"), _chatInput);

                _chatTriggerBtn = Button().SetIcon(config.IconChat).Class("tss-omnibox-chat-btn").OnClick(TriggerChat);

                _modelSelectorBtn = Button().Class("tss-omnibox-model-selector-btn").NoBorder().NoBackground().OnClick(ShowModelPopover);
                _modelSelectorBtn.Collapse();

                if (config.ChatFooter?.LeftSide is object)
                {
                    foreach (var i in config.ChatFooter.LeftSide)
                    {
                        _footer.appendChild(i.Class("tss-omnibox-chat-footer-item").Class("tss-omnibox-footer-left").Render());
                    }
                }
            }

            if (config.SearchFooter?.LeftSide is object)
            {
                if (_mode == Mode.Search)
                {
                    var targetInsert = _searchHistoryBtn.Render();
                    foreach (var i in config.SearchFooter.LeftSide)
                    {
                        var el = i.Class("tss-omnibox-search-footer-item").Class("tss-omnibox-footer-left").Render();
                        targetInsert.insertAdjacentElement(InsertPosition.afterend, el);
                        targetInsert = el;
                    }
                }
                else
                {
                    foreach (var i in config.SearchFooter.LeftSide)
                    {
                        _footer.appendChild(i.Class("tss-omnibox-search-footer-item").Class("tss-omnibox-footer-left").Render());
                    }
                }
            }

            var generatingContainer = Div(_("tss-omnibox-generating-container"), Spinner().CustomColor(Theme.Colors.Purple500).Small().Render(), TextBlock("Generating...").Render());

            _footer.appendChild(generatingContainer);

            _footer.appendChild(Div(_("tss-omnibox-footer-spacer")));

            if (_mode == Mode.Chat || _mode == Mode.SearchAndChat)
            {
                footerEnd.Add(_modelSelectorBtn);
                footerEnd.Add(_chatTriggerBtn);
            }

            if (config.ChatFooter?.RightSide is object)
            {
                if (_mode == Mode.Chat)
                {
                    footerEnd.InsertRange(0, config.ChatFooter.RightSide.Select(i => i.Class("tss-omnibox-chat-footer-item")));
                }
                else
                {
                    foreach (var i in config.ChatFooter.RightSide)
                    {
                        _footer.appendChild(i.Class("tss-omnibox-chat-footer-item").Render());
                    }
                }
            }

            if (config.SearchFooter?.RightSide is object)
            {
                if (_mode == Mode.Search)
                {
                    HTMLElement targetInsert = _searchInputContainer;
                    foreach (var i in config.SearchFooter.RightSide)
                    {
                        var el = i.Class("tss-omnibox-search-footer-item").Render();
                        targetInsert.insertAdjacentElement(InsertPosition.afterend, el);
                        targetInsert = el;
                    }
                }
                else
                {
                    foreach (var i in config.SearchFooter.RightSide)
                    {
                        _footer.appendChild(i.Class("tss-omnibox-search-footer-item").Render());
                    }
                }
            }

            foreach (var i in footerEnd)
            {
                _footer.appendChild(i.Render());
            }

            switch (_mode)
            {
                case Mode.Search:
                {
                    _container = Div(_("tss-omnibox-container"), _searchContainer);
                    break;
                }
                case Mode.Chat:
                {
                    _container = Div(_("tss-omnibox-container"), _chatContainer, _footer);
                    break;
                }
                case Mode.SearchAndChat:
                {
                    _container = Div(_("tss-omnibox-container tss-omnibox-chat-and-search"), _searchContainer, _chatContainer, _footer);
                    break;
                }
            }

            _container.addEventListener("click", (_) =>
            {
                _activeInput.focus();
            });


            _activeMode.Observe(UpdateMode);

            if (config.ExpandOnFocus)
            {
                _container.addEventListener("focusin", (_) =>
                {
                    if (_mode == Mode.Chat || _mode == Mode.SearchAndChat)
                    {
                        _container.classList.add("tss-omnibox-expanded");
                    }
                });
                _container.addEventListener("focusout", (e) =>
                {
                    // Check if new focus target is inside the container
                    var focusEvent = e.As<FocusEvent>();
                    if (focusEvent.relatedTarget == null || !_container.contains(focusEvent.relatedTarget.As<HTMLElement>()))
                    {
                        _container.classList.remove("tss-omnibox-expanded");
                    }
                });
            }

        }

        private void UpdateMode(Mode mode)
        {
            switch (mode)
            {
                case Mode.Search:
                {
                    _activeInput = _searchInput;
                    _container.classList.add("tss-omnibox-mode-search");
                    _container.classList.remove("tss-omnibox-mode-chat");
                    break;
                }
                case Mode.Chat:
                {
                    _activeInput = _chatInput;
                    _container.classList.add("tss-omnibox-mode-chat");
                    _container.classList.remove("tss-omnibox-mode-search");
                    break;
                }
                case Mode.SearchAndChat:
                {
                    throw new InvalidOperationException("Can't set active mode to mixed value");
                }
            }
        }

        private void SyncScroll()
        {
            _searchTokensContainer.scrollLeft = _searchInput.scrollLeft;
        }

        private const char specialWhitespace = '\u2000'; // \u00A0
        private const string specialWhitespaceString = "\u2000"; // \u00A0
        private void OnSearchInputChanged()
        {
            var val = _searchInput.value;

            // Strip incoming zero-width spaces/non-breaking spaces to ensure clean parsing if user copies/pastes internal formatting
            var cursorPosition = _searchInput.selectionStart;

            bool formattingChanged = false;

            // Note: We use the non-breaking space specialWhitespace to pad special tokens
            var rawQuery = ParseQuery(val.Replace(specialWhitespaceString, " "), _tokenIgnoreCase);
            var formattedBuilder = new System.Text.StringBuilder();
            var mapBuilder = new System.Text.StringBuilder();

            foreach (var token in rawQuery.Tokens)
            {
                bool isSpecialToken = token.Type == SearchToken.TokenType.ParenthesisOpen ||
                                      token.Type == SearchToken.TokenType.ParenthesisClose ||
                                      token.Type == SearchToken.TokenType.And ||
                                      token.Type == SearchToken.TokenType.Or ||
                                      token.Type == SearchToken.TokenType.Not;

                if (isSpecialToken)
                {
                    // Ensure space + non-breaking space before
                    if (formattedBuilder.Length > 0 && formattedBuilder[formattedBuilder.Length - 1] != specialWhitespace && formattedBuilder[formattedBuilder.Length - 1] != ' ')
                    {
                        formattedBuilder.Append(specialWhitespace);
                        mapBuilder.Append(specialWhitespace);
                        formattingChanged = true;
                    }
                    else if (formattedBuilder.Length > 0 && formattedBuilder[formattedBuilder.Length - 1] == ' ')
                    {
                        // Replace standard space with nb-space for our token wrapper logic
                        formattedBuilder[formattedBuilder.Length - 1] = specialWhitespace;
                        mapBuilder[mapBuilder.Length - 1] = specialWhitespace;
                        //formattedBuilder.Append(specialWhitespace);
                        formattingChanged = true;
                    }

                    formattedBuilder.Append(token.Value);
                    mapBuilder.Append(new string('1', token.Value.Length));

                    // Space after will be handled by the next token, but we can proactively add it
                    formattedBuilder.Append(specialWhitespace);
                    mapBuilder.Append(specialWhitespace);
                    formattingChanged = true;
                }
                else
                {
                    // If previous was a special token that added a padding space, and this token starts with space, merge them
                    if (token.Type == SearchToken.TokenType.Whitespace && formattedBuilder.Length > 0 && formattedBuilder[formattedBuilder.Length - 1] == specialWhitespace)
                    {
                        // skip the leading spaces from this token since we already have the nb-space
                        var trimmed = token.Value.TrimStart();
                        if (!string.IsNullOrEmpty(trimmed))
                        {
                            formattedBuilder.Append(trimmed);
                            mapBuilder.Append(new string('0', trimmed.Length));
                        }
                    }
                    else
                    {
                        formattedBuilder.Append(token.Value);
                        mapBuilder.Append(new string('0', token.Value.Length));
                    }
                }
            }

            var finalFormattedStr = formattedBuilder.ToString();
            var finalMapStr = mapBuilder.ToString();

            int newCursorPos = (int)cursorPosition;
            if (finalFormattedStr != val)
            {
                int oldCharIndex = 0;
                int newCharIndex = 0;

                while (oldCharIndex < cursorPosition && newCharIndex < finalFormattedStr.Length)
                {
                    char oldChar = val[oldCharIndex];
                    char newChar = finalFormattedStr[newCharIndex];

                    if (oldChar == newChar)
                    {
                        oldCharIndex++;
                        newCharIndex++;
                    }
                    else if (char.IsWhiteSpace(oldChar) && char.IsWhiteSpace(newChar))
                    {
                        oldCharIndex++;
                        newCharIndex++;
                    }
                    else if (char.IsWhiteSpace(newChar))
                    {
                        // formatting added a space
                        newCharIndex++;
                    }
                    else if (char.IsWhiteSpace(oldChar))
                    {
                        // formatting removed a space
                        oldCharIndex++;
                    }
                    else
                    {
                        oldCharIndex++;
                        newCharIndex++;
                    }
                }
                newCursorPos = newCharIndex;
            }

            // Remove trailing nb-space if it's at the very end of the string and the user didn't type it
            if (finalFormattedStr.EndsWith(specialWhitespaceString) && val.Length < finalFormattedStr.Length)
            {
                finalFormattedStr = finalFormattedStr.Substring(0, finalFormattedStr.Length - 1);
                finalMapStr = finalMapStr.Substring(0, finalMapStr.Length - 1);
                if (newCursorPos > finalFormattedStr.Length) newCursorPos = finalFormattedStr.Length;
            }

            _tokenTypeMap = finalMapStr;

            if (finalFormattedStr != val)
            {
                _searchInput.value = finalFormattedStr;
                _searchInput.setSelectionRange((uint)newCursorPos, (uint)newCursorPos);
                val = finalFormattedStr;
            }

            if (string.IsNullOrEmpty(val))
            {
                _searchClearBtn.Collapse();
                ClearTokens();
            }
            else
            {
                _searchClearBtn.Show();
                ParseAndRenderTokens(val);
            }
        }

        private void ClearTokens()
        {
            _searchTokensContainer.innerHTML = "";
        }

        private void ParseAndRenderTokens(string input)
        {
            var query = ParseQuery(input, _tokenIgnoreCase);
            RenderTokens(query.Tokens);
            SyncScroll();
        }

        /// <summary>
        /// Parses the query.
        /// </summary>
        public static SearchQuery ParseQuery(string input, bool tokenIgnoreCase = false)
        {
            var tokens = new List<SearchToken>();
            if (string.IsNullOrEmpty(input)) return new SearchQuery(input, tokens);

            int i = 0;
            while (i < input.Length)
            {
                char c = input[i];

                if (char.IsWhiteSpace(c))
                {
                    int start = i;
                    while (i < input.Length && char.IsWhiteSpace(input[i])) i++;
                    tokens.Add(new SearchToken(SearchToken.TokenType.Whitespace, input.Substring(start, i - start)));
                    continue;
                }

                if (c == '(')
                {
                    tokens.Add(new SearchToken(SearchToken.TokenType.ParenthesisOpen, "("));
                    i++;
                    continue;
                }
                if (c == ')')
                {
                    tokens.Add(new SearchToken(SearchToken.TokenType.ParenthesisClose, ")"));
                    i++;
                    continue;
                }
                if (c == '"' || c == '\'')
                {
                    // Parse quoted string as a single token
                    int startQuote = i;
                    i++;
                    while (i < input.Length && input[i] != c)
                    {
                        i++;
                    }
                    if (i < input.Length) i++; // Include closing quote
                    tokens.Add(new SearchToken(SearchToken.TokenType.Quote, input.Substring(startQuote, i - startQuote)));
                    continue;
                }
                if (c == '&' || c == '|')
                {
                    var type = c == '&' ? SearchToken.TokenType.And : SearchToken.TokenType.Or;
                    // check if next char is same (e.g. && or ||)
                    if (i + 1 < input.Length && input[i + 1] == c)
                    {
                        tokens.Add(new SearchToken(type, new string(c, 2)));
                        i += 2;
                    }
                    else
                    {
                        tokens.Add(new SearchToken(type, c.ToString()));
                        i++;
                    }
                    continue;
                }
                if (c == '!')
                {
                    tokens.Add(new SearchToken(SearchToken.TokenType.Not, c.ToString()));
                    i++;
                    continue;
                }
                if (c == '-')
                {
                    // A '-' only acts as a negation operator when it sits at a token boundary,
                    // i.e. at the start of the input, after whitespace, or after an opening
                    // parenthesis. A '-' that is preceded by a non-whitespace character is part
                    // of the word so that ranges like "53-223" stay a single token.
                    bool isBoundary = i == 0 || char.IsWhiteSpace(input[i - 1]) || input[i - 1] == '(';
                    if (isBoundary)
                    {
                        tokens.Add(new SearchToken(SearchToken.TokenType.Not, c.ToString()));
                        i++;
                        continue;
                    }
                    // otherwise fall through and treat the '-' as part of a word
                }

                // Word token
                int wordStart = i;
                while (i < input.Length && !char.IsWhiteSpace(input[i]) && input[i] != '(' && input[i] != ')' && input[i] != '"' && input[i] != '\'' && input[i] != '&' && input[i] != '|' && input[i] != '!')
                {
                    i++;
                }
                var word = input.Substring(wordStart, i - wordStart);

                // Check for word-based operators
                var matchWord = tokenIgnoreCase ? word.ToUpper() : word;
                if (matchWord == "AND")
                {
                    tokens.Add(new SearchToken(SearchToken.TokenType.And, word));
                }
                else if (matchWord == "OR")
                {
                    tokens.Add(new SearchToken(SearchToken.TokenType.Or, word));
                }
                else if (matchWord == "NOT")
                {
                    tokens.Add(new SearchToken(SearchToken.TokenType.Not, word));
                }
                else
                {
                    tokens.Add(new SearchToken(SearchToken.TokenType.Word, word));
                }
            }

            return new SearchQuery(input, tokens);
        }

        private void RenderTokens(List<SearchToken> tokens)
        {
            ClearTokens();

            var consecutiveWordBuilder = new System.Text.StringBuilder();

            Action flushConsecutiveWords = () =>
            {
                if (consecutiveWordBuilder.Length > 0)
                {
                    // Trim trailing whitespace from the merged words to render it outside the span
                    var text = consecutiveWordBuilder.ToString();
                    var trimmed = text.TrimEnd();
                    var trailingWhitespace = text.Substring(trimmed.Length);

                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        var span = Span(_("tss-omnibox-search-token-word", text: trimmed));
                        _searchTokensContainer.appendChild(span);
                    }

                    if (!string.IsNullOrEmpty(trailingWhitespace))
                    {
                        var spanSpace = Span(_(text: trailingWhitespace));
                        _searchTokensContainer.appendChild(spanSpace);
                    }

                    consecutiveWordBuilder.Clear();
                }
            };

            foreach (var token in tokens)
            {
                if (token.Type == SearchToken.TokenType.Word || token.Type == SearchToken.TokenType.Whitespace)
                {
                    // If we have leading whitespace before any words, flush it directly
                    if (consecutiveWordBuilder.Length == 0 && token.Type == SearchToken.TokenType.Whitespace)
                    {
                        var spanSpace = Span(_(text: token.Value));
                        _searchTokensContainer.appendChild(spanSpace);
                    }
                    else
                    {
                        consecutiveWordBuilder.Append(token.Value);
                    }
                }
                else
                {
                    flushConsecutiveWords();

                    string className = "";
                    switch (token.Type)
                    {
                        case SearchToken.TokenType.And:
                            className = "tss-omnibox-search-token-operator-and";
                            break;
                        case SearchToken.TokenType.Or:
                            className = "tss-omnibox-search-token-operator-or";
                            break;
                        case SearchToken.TokenType.Not:
                            className = "tss-omnibox-search-token-operator-not";
                            break;
                        case SearchToken.TokenType.ParenthesisOpen:
                        case SearchToken.TokenType.ParenthesisClose:
                            className = "tss-omnibox-search-token-paren";
                            break;
                        case SearchToken.TokenType.Quote:
                            className = "tss-omnibox-search-token-quote";
                            break;
                    }

                    if (string.IsNullOrEmpty(className))
                    {
                        var span = Span(_(text: token.Value));
                        _searchTokensContainer.appendChild(span);
                    }
                    else
                    {
                        var span = Span(_(className, text: token.Value));
                        _searchTokensContainer.appendChild(span);
                    }
                }
            }

            flushConsecutiveWords();
        }

        private void TriggerSearch()
        {
            var val = _searchInput.value;
            var query = ParseQuery(val, _tokenIgnoreCase);
            query.Snaps = GetActiveSnaps();
            query.FilterSnaps = GetActiveFilterSnaps();
            Searched?.Invoke(this, query);
        }

        private SnapHandler[] GetActiveSnaps()
        {
            if (InlineFilterChips == null) return Array.Empty<SnapHandler>();
            var result = new List<SnapHandler>();
            for (int i = 0; i < InlineFilterChips.Count; i++)
            {
                var chip = InlineFilterChips[i];
                if (chip.Snap != null) result.Add(chip.Snap);
            }
            return result.ToArray();
        }

        private FilterSnap[] GetActiveFilterSnaps()
        {
            if (InlineFilterChips == null) return Array.Empty<FilterSnap>();
            var result = new List<FilterSnap>();
            for (int i = 0; i < InlineFilterChips.Count; i++)
            {
                var chip = InlineFilterChips[i];
                if (chip.FilterSnap != null) result.Add(chip.FilterSnap);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Registers a snap-handler that can convert recognized input into an inline filter chip.
        /// </summary>
        public OmniBox RegisterSnap(SnapHandler snap)
        {
            if (snap == null) return this;
            if (_mode != Mode.Search && _mode != Mode.SearchAndChat)
            {
                throw new InvalidOperationException("RegisterSnap can only be called when OmniBox is in Search or SearchAndChat mode.");
            }
            _snapHandlers.Add(snap);
            return this;
        }

        /// <summary>
        /// Registers multiple snap-handlers in one call.
        /// </summary>
        public OmniBox RegisterSnaps(params SnapHandler[] snaps)
        {
            if (snaps == null) return this;
            foreach (var s in snaps) RegisterSnap(s);
            return this;
        }

        /// <summary>
        /// Registers a filter snap handler that creates an inline filter chip.
        /// </summary>
        public OmniBox RegisterFilterSnap(FilterSnapHandler filter)
        {
            if (filter == null) return this;
            if (_mode != Mode.Search && _mode != Mode.SearchAndChat)
            {
                throw new InvalidOperationException("RegisterFilterSnap can only be called when OmniBox is in Search or SearchAndChat mode.");
            }
            _filterSnapHandlers.Add(filter);
            return this;
        }

        /// <summary>
        /// Registers multiple filter snap handlers in one call.
        /// </summary>
        public OmniBox RegisterFilterSnaps(params FilterSnapHandler[] filters)
        {
            if (filters == null) return this;
            foreach (var f in filters) RegisterFilterSnap(f);
            return this;
        }

        private bool TryUpdateFilterSnapSuggestions()
        {
            if (_filterSnapHandlers.Count == 0)
            {
                HideFilterSnapSuggestions();
                return false;
            }

            var val = _searchInput.value;
            var cursor = (int)_searchInput.selectionStart;

            int wordStart = 0;
            for (int i = cursor - 1; i >= 0; i--)
            {
                var ch = val[i];
                if (ch == ' ' || ch == specialWhitespace || char.IsWhiteSpace(ch))
                {
                    wordStart = i + 1;
                    break;
                }
            }

            var segment = val.Substring(wordStart, cursor - wordStart);
            int colonIdx = segment.IndexOf(':');
            if (colonIdx < 0)
            {
                HideFilterSnapSuggestions();
                return false;
            }

            var triggerText = segment.Substring(0, colonIdx);
            var valueText = segment.Substring(colonIdx + 1);

            FilterSnapHandler matched = null;
            string matchedTrigger = null;
            foreach (var h in _filterSnapHandlers)
            {
                foreach (var t in h.TriggerWords)
                {
                    if (string.Equals(t, triggerText, StringComparison.OrdinalIgnoreCase))
                    {
                        matched = h;
                        matchedTrigger = t;
                        break;
                    }
                }
                if (matched != null) break;
            }

            if (matched == null)
            {
                HideFilterSnapSuggestions();
                return false;
            }

            _filterSnapStart = wordStart;
            _filterSnapEnd = cursor;

            if (matched.IsTimeRange)
            {
                // Bump the request id so any in-flight async (value) suggestion load is ignored.
                _filterSnapRequestId++;
                ShowTimeFilterSnapSuggestions(matched, matchedTrigger, valueText);
                return true;
            }

            var requestId = ++_filterSnapRequestId;
            var captured = matched;
            var capturedTrigger = matchedTrigger;
            var capturedStart = wordStart;
            var capturedEnd = cursor;

            LoadFilterSnapSuggestionsAsync(captured, capturedTrigger, valueText, requestId, capturedStart, capturedEnd);
            return true;
        }

        private async void LoadFilterSnapSuggestionsAsync(FilterSnapHandler handler, string trigger, string valueText, int requestId, int start, int end)
        {
            string[] values;
            try
            {
                values = await handler.GetValuesAsync(valueText);
            }
            catch (Exception ex)
            {
                console.error("Filter snap values fetch failed: ", ex);
                values = Array.Empty<string>();
            }

            if (requestId != _filterSnapRequestId) return;
            if (_filterSnapStart != start || _filterSnapEnd != end)
            {
                return;
            }

            if (values == null || values.Length == 0)
            {
                HideFilterSnapSuggestions();
                return;
            }

            ShowFilterSnapSuggestions(handler, trigger, values);
        }

        private void ShowFilterSnapSuggestions(FilterSnapHandler handler, string trigger, string[] values)
        {
            _timeFilterSnapOpen = false;
            _currentFilterSnapSuggestionButtons = new List<Button>();
            _highlightedFilterSnapSuggestionIndex = 0;

            var content = VStack().NoDefaultMargin().W(360).MaxHeight(400.px()).ScrollY();
            var headerText = handler.DisplayName;
            content.Add(TextBlock(headerText).Small().SemiBold().PaddingBottom(4.px()).PaddingTop(8.px()).PaddingLeft(12.px()).Foreground(Theme.Secondary.Foreground));

            foreach (var v in values)
            {
                var capturedValue = v;
                var btn = Button().Class("tss-omnibox-snap-suggestion").Class("tss-omnibox-filter-snap-suggestion").WS().NoPadding().NoMinSize().H(32).MB(4).NoBorder().NoBackground().TextLeft();
                var row = HStack().WS().AlignItems(ItemAlign.Center).PaddingLeft(12.px()).PaddingRight(12.px());
                if (handler.Icon != null) row.Add(handler.Icon.MR(8));
                row.Add(TextBlock(capturedValue));
                row.Add(TextBlock(trigger + ":" + capturedValue).Small().Secondary().ML(UnitSize.Auto()));

                btn.ReplaceContent(row);
                btn.OnClick(() => CommitFilterSnap(handler, trigger, capturedValue));
                content.Add(btn);
                _currentFilterSnapSuggestionButtons.Add(btn);
            }

            if (_hideFilterSnapSuggestions != null)
            {
                _hideFilterSnapSuggestions();
                _hideFilterSnapSuggestions = null;
            }

            Tippy.ShowFor(_activeInput, content.Render(), out _hideFilterSnapSuggestions, placement: TooltipPlacement.BottomStart, maxWidth: 400, onHiddenCallback: () => _hideFilterSnapSuggestions = null);
            HighlightFilterSnapSuggestion(0);
        }

        private void ShowTimeFilterSnapSuggestions(FilterSnapHandler handler, string trigger, string valueText)
        {
            _currentFilterSnapSuggestionButtons = new List<Button>();
            _highlightedFilterSnapSuggestionIndex = 0;

            // Parse whatever the user has typed so far (yyyy-MM-dd:yyyy-MM-dd). Fall back to the last 7 days so
            // the picker and the "Apply" action always have a sensible day-granularity range to work with.
            var today = DateTime.Today;
            DateTime? typedFrom = null, typedTo = null;
            if (!string.IsNullOrEmpty(valueText))
            {
                var parts = valueText.Split(':');
                if (parts.Length >= 1 && TryParseDay(parts[0], out var pf)) typedFrom = pf;
                if (parts.Length >= 2 && TryParseDay(parts[1], out var pt)) typedTo = pt;
            }
            var initialFrom = typedFrom ?? today.AddDays(-7);
            var initialTo = typedTo ?? today;
            if (initialTo < initialFrom) initialTo = initialFrom;

            var picker = DateRangePicker(initialFrom, initialTo);

            var content = VStack().NoDefaultMargin().W(320).MaxHeight(440.px()).ScrollY();
            content.Add(TextBlock(handler.DisplayName).Small().SemiBold().PaddingTop(8.px()).PaddingLeft(12.px()).Foreground(Theme.Secondary.Foreground));
            content.Add(TextBlock("Pick a range or type yyyy-MM-dd:yyyy-MM-dd (day granularity)").Small().Secondary().PaddingBottom(8.px()).PaddingLeft(12.px()).PaddingRight(12.px()));
            content.Add(HStack().WS().AlignItems(ItemAlign.Center).PaddingLeft(12.px()).PaddingRight(12.px()).PaddingBottom(8.px()).Children(picker));

            var applyBtn = Button("Apply range").Primary().SetIcon(UIcons.Check).WS();
            applyBtn.OnClick(() =>
            {
                var from = picker.From ?? initialFrom;
                var to = picker.To ?? initialTo;
                if (to < from) { var swap = from; from = to; to = swap; }
                CommitTimeFilterSnap(handler, trigger, from, to);
            });
            content.Add(HStack().WS().PaddingLeft(12.px()).PaddingRight(12.px()).PaddingBottom(8.px()).Children(applyBtn));
            _currentFilterSnapSuggestionButtons.Add(applyBtn);

            content.Add(TextBlock("Shortcuts").Small().SemiBold().PaddingBottom(4.px()).PaddingLeft(12.px()).Foreground(Theme.Secondary.Foreground));
            AddTimeFilterSnapShortcut(content, handler, trigger, "Last week", today.AddDays(-7), today);
            AddTimeFilterSnapShortcut(content, handler, trigger, "Last month", today.AddMonths(-1), today);
            AddTimeFilterSnapShortcut(content, handler, trigger, "Last 90 days", today.AddDays(-90), today);
            AddTimeFilterSnapShortcut(content, handler, trigger, "Last year", today.AddYears(-1), today);

            if (_hideFilterSnapSuggestions != null)
            {
                _hideFilterSnapSuggestions();
                _hideFilterSnapSuggestions = null;
            }

            _timeFilterSnapOpen = true;
            Tippy.ShowFor(_activeInput, content.Render(), out _hideFilterSnapSuggestions, placement: TooltipPlacement.BottomStart, maxWidth: 360, onHiddenCallback: () => { _hideFilterSnapSuggestions = null; _timeFilterSnapOpen = false; });
            HighlightFilterSnapSuggestion(0);
        }

        private void AddTimeFilterSnapShortcut(Stack content, FilterSnapHandler handler, string trigger, string label, DateTime from, DateTime to)
        {
            var preview = from.ToString("yyyy-MM-dd") + ":" + to.ToString("yyyy-MM-dd");
            var btn = Button().Class("tss-omnibox-snap-suggestion").Class("tss-omnibox-filter-snap-suggestion").WS().NoPadding().NoMinSize().H(32).MB(4).NoBorder().NoBackground().TextLeft();
            var row = HStack().WS().AlignItems(ItemAlign.Center).PaddingLeft(12.px()).PaddingRight(12.px());
            row.Add(TextBlock(label));
            row.Add(TextBlock(preview).Small().Secondary().ML(UnitSize.Auto()));
            btn.ReplaceContent(row);
            btn.OnClick(() => CommitTimeFilterSnap(handler, trigger, from, to));
            content.Add(btn);
            _currentFilterSnapSuggestionButtons.Add(btn);
        }

        private void CommitTimeFilterSnap(FilterSnapHandler handler, string trigger, DateTime from, DateTime to)
        {
            var value = from.ToString("yyyy-MM-dd") + ":" + to.ToString("yyyy-MM-dd");
            CommitFilterSnap(handler, trigger, value);
        }

        private static bool TryParseDay(string s, out DateTime date)
        {
            date = default;
            if (string.IsNullOrWhiteSpace(s)) return false;
            return DateTime.TryParseExact(s.Trim(), "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, out date);
        }

        private void HideFilterSnapSuggestions()
        {
            if (_hideFilterSnapSuggestions != null)
            {
                _hideFilterSnapSuggestions();
                _hideFilterSnapSuggestions = null;
            }
            _currentFilterSnapSuggestionButtons.Clear();
            _highlightedFilterSnapSuggestionIndex = -1;
            _filterSnapStart = -1;
            _filterSnapEnd = -1;
            _timeFilterSnapOpen = false;
        }

        private void UpdateHighlightedFilterSnapSuggestion(int offset)
        {
            if (_currentFilterSnapSuggestionButtons.Count == 0) return;
            var next = (_highlightedFilterSnapSuggestionIndex + offset + _currentFilterSnapSuggestionButtons.Count) % _currentFilterSnapSuggestionButtons.Count;
            HighlightFilterSnapSuggestion(next);
        }

        private void HighlightFilterSnapSuggestion(int index)
        {
            if (_highlightedFilterSnapSuggestionIndex >= 0 && _highlightedFilterSnapSuggestionIndex < _currentFilterSnapSuggestionButtons.Count)
            {
                _currentFilterSnapSuggestionButtons[_highlightedFilterSnapSuggestionIndex].RemoveClass("tss-omnibox-suggestion-highlight");
            }
            _highlightedFilterSnapSuggestionIndex = index;
            if (_highlightedFilterSnapSuggestionIndex >= 0 && _highlightedFilterSnapSuggestionIndex < _currentFilterSnapSuggestionButtons.Count)
            {
                _currentFilterSnapSuggestionButtons[_highlightedFilterSnapSuggestionIndex].Class("tss-omnibox-suggestion-highlight");
            }
        }

        private void CommitFilterSnap(FilterSnapHandler handler, string trigger, string value)
        {
            if (handler == null) return;
            if (_filterSnapStart < 0 || _filterSnapEnd < 0) return;

            var val = _searchInput.value;
            var start = _filterSnapStart;
            var end = _filterSnapEnd;
            if (start < 0 || end > val.Length || start > end)
            {
                HideFilterSnapSuggestions();
                return;
            }

            var before = val.Substring(0, start);
            var after = val.Substring(end);

            if (before.Length > 0 && (before[before.Length - 1] == ' ' || before[before.Length - 1] == specialWhitespace) &&
                after.StartsWith(" "))
            {
                after = after.Substring(1);
            }

            var newVal = before + after;
            _searchInput.value = newVal;
            _searchInput.setSelectionRange((uint)before.Length, (uint)before.Length);

            InlineFilterChips.Add(new InlineFilterChip(new FilterSnap(handler, trigger, value)));

            HideFilterSnapSuggestions();
            OnSearchInputChanged();
            _searchInput.focus();
        }

        private bool TryUpdateSnapSuggestions()
        {
            if (_snapHandlers.Count == 0)
            {
                HideSnapSuggestions();
                return false;
            }

            var val = _searchInput.value;
            var cursor = (int)_searchInput.selectionStart;

            int atIdx = -1;
            for (int i = cursor - 1; i >= 0; i--)
            {
                var ch = val[i];
                if (ch == '@')
                {
                    if (i == 0 || val[i - 1] == ' ' || val[i - 1] == specialWhitespace || char.IsWhiteSpace(val[i - 1]))
                    {
                        atIdx = i;
                    }
                    break;
                }
                if (ch == ' ' || ch == specialWhitespace || char.IsWhiteSpace(ch))
                {
                    break;
                }
            }

            if (atIdx < 0)
            {
                HideSnapSuggestions();
                return false;
            }

            var typed = val.Substring(atIdx + 1, cursor - atIdx - 1);
            var active = GetActiveSnaps();
            var activeSnapIds = new HashSet<string>();
            bool anyActive = false;
            bool anyExclusiveActive = false;
            foreach (var s in active)
            {
                activeSnapIds.Add(s.SnapId);
                anyActive = true;
                if (s.Exclusive) anyExclusiveActive = true;
            }

            if (anyExclusiveActive)
            {
                HideSnapSuggestions();
                return false;
            }

            var matches = _snapHandlers
                .Where(s => !activeSnapIds.Contains(s.SnapId))
                .Where(s => !anyActive || !s.Exclusive)
                .Where(s => s.TriggerWords.Any(t => t.IndexOf(typed, StringComparison.OrdinalIgnoreCase) >= 0))
                .OrderBy(s => s.TriggerWords.Any(t => t.StartsWith(typed, StringComparison.OrdinalIgnoreCase)) ? 0 : 1)
                .ToArray();

            if (matches.Length == 0)
            {
                HideSnapSuggestions();
                return false;
            }

            _snapMentionStart = atIdx;
            _snapMentionEnd = cursor;
            ShowSnapSuggestions(matches);
            return true;
        }

        private void ShowSnapSuggestions(SnapHandler[] matches)
        {
            _currentSnapMatches = matches;
            _currentSnapSuggestionButtons = new List<Button>();
            _highlightedSnapSuggestionIndex = 0;

            var content = VStack().NoDefaultMargin().W(360).MaxHeight(400.px()).ScrollY();
            content.Add(TextBlock("Snaps").Small().SemiBold().PaddingBottom(4.px()).PaddingTop(8.px()).PaddingLeft(12.px()).Foreground(Theme.Secondary.Foreground));

            for (int i = 0; i < matches.Length; i++)
            {
                var captured = matches[i];
                var capturedIndex = i;
                var btn = Button().Class("tss-omnibox-snap-suggestion").WS().NoPadding().NoMinSize().H(40).MB(4).NoBorder().NoBackground().TextLeft();
                var row = HStack().WS().AlignItems(ItemAlign.Center).PaddingLeft(12.px()).PaddingRight(12.px());
                if (captured.Icon != null) row.Add(captured.Icon.MR(8));
                var labels = VStack().NoDefaultMargin();
                labels.Add(TextBlock(captured.DisplayName));
                if (!string.IsNullOrEmpty(captured.Description))
                {
                    labels.Add(TextBlock(captured.Description).Small().Secondary());
                }
                row.Add(labels);
                row.Add(TextBlock("@" + captured.TriggerWords[0]).Small().Secondary().ML(UnitSize.Auto()));
                btn.ReplaceContent(row);
                btn.OnClick(() => CommitSnap(captured));
                content.Add(btn);
                _currentSnapSuggestionButtons.Add(btn);
            }

            if (_hideSnapSuggestions != null)
            {
                _hideSnapSuggestions();
                _hideSnapSuggestions = null;
            }

            Tippy.ShowFor(_activeInput, content.Render(), out _hideSnapSuggestions, placement: TooltipPlacement.BottomStart, maxWidth: 400, onHiddenCallback: () => _hideSnapSuggestions = null);
            HighlightSnapSuggestion(0);
        }

        private void HideSnapSuggestions()
        {
            if (_hideSnapSuggestions != null)
            {
                _hideSnapSuggestions();
                _hideSnapSuggestions = null;
            }
            _currentSnapSuggestionButtons.Clear();
            _highlightedSnapSuggestionIndex = -1;
            _snapMentionStart = -1;
            _snapMentionEnd = -1;
        }

        private void HideRegularSuggestions()
        {
            window.clearTimeout(_suggestionsDebounceTimeoutId);
            if (_hideSuggestions != null)
            {
                _hideSuggestions();
                _hideSuggestions = null;
            }
            _highlightedSuggestionIndex = -1;
        }

        private void UpdateHighlightedSnapSuggestion(int offset)
        {
            if (_currentSnapSuggestionButtons.Count == 0) return;
            var next = (_highlightedSnapSuggestionIndex + offset + _currentSnapSuggestionButtons.Count) % _currentSnapSuggestionButtons.Count;
            HighlightSnapSuggestion(next);
        }

        private void HighlightSnapSuggestion(int index)
        {
            if (_highlightedSnapSuggestionIndex >= 0 && _highlightedSnapSuggestionIndex < _currentSnapSuggestionButtons.Count)
            {
                _currentSnapSuggestionButtons[_highlightedSnapSuggestionIndex].RemoveClass("tss-omnibox-suggestion-highlight");
            }
            _highlightedSnapSuggestionIndex = index;
            if (_highlightedSnapSuggestionIndex >= 0 && _highlightedSnapSuggestionIndex < _currentSnapSuggestionButtons.Count)
            {
                _currentSnapSuggestionButtons[_highlightedSnapSuggestionIndex].Class("tss-omnibox-suggestion-highlight");
            }
        }

        private void CommitSnap(SnapHandler snap)
        {
            if (snap == null) return;
            if (_snapMentionStart < 0 || _snapMentionEnd < 0) return;

            var val = _searchInput.value;
            var start = _snapMentionStart;
            var end = _snapMentionEnd;
            if (start < 0 || end > val.Length || start > end)
            {
                HideSnapSuggestions();
                return;
            }

            var before = val.Substring(0, start);
            var after = val.Substring(end);

            // Strip a single trailing space we'd otherwise leave dangling
            if (before.Length > 0 && (before[before.Length - 1] == ' ' || before[before.Length - 1] == specialWhitespace) &&
                after.StartsWith(" "))
            {
                after = after.Substring(1);
            }

            var newVal = before + after;
            _searchInput.value = newVal;
            _searchInput.setSelectionRange((uint)before.Length, (uint)before.Length);

            InlineFilterChips.Add(new InlineFilterChip(snap));

            HideSnapSuggestions();
            OnSearchInputChanged();
            _searchInput.focus();
        }

        private void TriggerChat()
        {
            if (_isGenerating)
            {
                TriggerStop();
                return;
            }
            var val = _chatInput.value;
            Chatted?.Invoke(this, new ChatMessage() { Text = val });
            _chatInput.value = "";
            UpdateChatTriggerActiveState();
        }

        private void UpdateChatTriggerActiveState()
        {
            if (_chatTriggerBtn == null || _chatInput == null) return;
            var hasText = !string.IsNullOrEmpty(_chatInput.value) && !string.IsNullOrWhiteSpace(_chatInput.value);
            var el = _chatTriggerBtn.Render();
            if (hasText) el.classList.add("tss-omnibox-chat-btn-active");
            else el.classList.remove("tss-omnibox-chat-btn-active");
        }

        private void TriggerStop()
        {
            Stopped?.Invoke(this);
        }

        /// <summary>
        /// Registers a callback invoked when the search event fires.
        /// </summary>
        public OmniBox OnSearch(SearchEventHandler onSearch)
        {
            Searched += onSearch;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the chat event fires.
        /// </summary>
        public OmniBox OnChat(ChatEventHandler onChat)
        {
            Chatted += onChat;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the stop event fires.
        /// </summary>
        public OmniBox OnStop(StopEventHandler onStop)
        {
            Stopped += onStop;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the model changed event fires.
        /// </summary>
        public OmniBox OnModelChanged(ModelChangedEventHandler onModelChanged)
        {
            ModelChanged += onModelChanged;
            return this;
        }

        /// <summary>
        /// Sets the models of the component.
        /// </summary>
        public OmniBox SetModels(params ModelOption[] models)
        {
            return SetModels((IEnumerable<ModelOption>)models);
        }

        /// <summary>
        /// Sets the models of the component.
        /// </summary>
        public OmniBox SetModels(IEnumerable<ModelOption> models)
        {
            if (_modelSelectorBtn == null)
            {
                throw new InvalidOperationException("SetModels can only be called when OmniBox is in Chat or SearchAndChat mode.");
            }

            _isModelLocked = false;
            _models = models?.ToList() ?? new List<ModelOption>();

            if (_selectedModel == null || !_models.Contains(_selectedModel))
            {
                _selectedModel = _models.FirstOrDefault();
            }

            UpdateModelSelectorButton();
            return this;
        }

        /// <summary>
        /// Locks the OmniBox to the given model option so users cannot pick another.
        /// </summary>
        public OmniBox LockModel(ModelOption model)
        {
            if (_modelSelectorBtn == null)
            {
                throw new InvalidOperationException("LockModel can only be called when OmniBox is in Chat or SearchAndChat mode.");
            }
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            _isModelLocked = true;
            _models = new List<ModelOption> { model };
            _selectedModel = model;

            if (_hideModelPopover != null)
            {
                _hideModelPopover();
                _hideModelPopover = null;
            }

            UpdateModelSelectorButton();
            return this;
        }

        /// <summary>
        /// Sets the thinking effort of the component.
        /// </summary>
        public OmniBox SetThinkingEffort(ThinkingEffort effort)
        {
            _selectedEffort = effort;
            UpdateModelSelectorButton();
            return this;
        }

        /// <summary>
        /// Gets or sets the selected model.
        /// </summary>
        public ModelOption SelectedModel => _selectedModel;
        /// <summary>
        /// Gets or sets the selected thinking effort.
        /// </summary>
        public ThinkingEffort SelectedThinkingEffort => _selectedEffort;
        /// <summary>
        /// Returns a value indicating whether the component is model locked.
        /// </summary>
        public bool IsModelLocked => _isModelLocked;

        private static string FormatEffort(ThinkingEffort effort)
        {
            switch (effort)
            {
                case ThinkingEffort.Disable: return "Disabled";
                case ThinkingEffort.Low:     return "Low";
                case ThinkingEffort.Medium:  return "Medium";
                case ThinkingEffort.High:    return "High";
            }
            return effort.ToString();
        }

        private void UpdateModelSelectorButton()
        {
            if (_modelSelectorBtn == null) return;

            if (_selectedModel == null)
            {
                _modelSelectorBtn.Collapse();
                return;
            }

            _modelSelectorBtn.Show();

            var label = _selectedModel.Name;
            if (_selectedEffort != ThinkingEffort.Disable)
            {
                label += " · " + FormatEffort(_selectedEffort);
            }
            _modelSelectorBtn.SetText(label);

            if (_isModelLocked)
            {
                _modelSelectorBtn.Render().classList.add("tss-omnibox-model-selector-locked");
                _modelSelectorBtn.Tooltip("Model locked");
            }
            else
            {
                _modelSelectorBtn.Render().classList.remove("tss-omnibox-model-selector-locked");
            }
        }

        private void ShowModelPopover()
        {
            if (_modelSelectorBtn == null) return;
            if (_isModelLocked) return;
            if (_models == null || _models.Count == 0) return;

            if (_hideModelPopover != null)
            {
                _hideModelPopover();
                _hideModelPopover = null;
                return;
            }

            var content = VStack().NoDefaultMargin().Class("tss-omnibox-model-selector-popover").MinWidth(220.px()).MaxHeight(500.px()).ScrollY();

            content.Add(TextBlock("Models").Small().SemiBold().Class("tss-omnibox-model-selector-header"));

            foreach (var model in _models)
            {
                var captured = model;
                var isSelected = ReferenceEquals(captured, _selectedModel);

                var row = HStack().WS().AlignItemsCenter().Class("tss-omnibox-model-selector-item");
                row.Add(TextBlock(captured.Name).Class("tss-omnibox-model-selector-item-name"));
                if (!string.IsNullOrEmpty(captured.Description))
                {
                    row.Add(TextBlock(captured.Description).Small().Secondary().ML(6));
                }
                row.Add(Raw(Div(_("tss-omnibox-model-selector-spacer"))));
                if (isSelected) row.Add(Icon(UIcons.Check).Class("tss-omnibox-model-selector-check"));

                var btn = Button().Class("tss-omnibox-model-selector-row").WS().NoPadding().NoMinSize().NoBorder().NoBackground().TextLeft();
                btn.ReplaceContent(row);
                btn.OnClick(() =>
                {
                    _selectedModel = captured;
                    UpdateModelSelectorButton();
                    ModelChanged?.Invoke(this, _selectedModel, _selectedEffort);
                    if (_hideModelPopover != null)
                    {
                        _hideModelPopover();
                        _hideModelPopover = null;
                    }
                });
                content.Add(btn);
            }

            content.Add(Raw(Div(_("tss-omnibox-model-selector-divider"))));
            content.Add(TextBlock("Thinking Effort").Small().SemiBold().Class("tss-omnibox-model-selector-header"));

            foreach (ThinkingEffort effort in new[] { ThinkingEffort.Disable, ThinkingEffort.Low, ThinkingEffort.Medium, ThinkingEffort.High })
            {
                var captured = effort;
                var isSelected = captured == _selectedEffort;

                var row = HStack().WS().AlignItemsCenter().Class("tss-omnibox-model-selector-item");
                row.Add(TextBlock(FormatEffort(captured)).Class("tss-omnibox-model-selector-item-name"));
                row.Add(Raw(Div(_("tss-omnibox-model-selector-spacer"))));
                if (isSelected) row.Add(Icon(UIcons.Check).Class("tss-omnibox-model-selector-check"));

                var btn = Button().Class("tss-omnibox-model-selector-row").WS().NoPadding().NoMinSize().NoBorder().NoBackground().TextLeft();
                btn.ReplaceContent(row);
                btn.OnClick(() =>
                {
                    _selectedEffort = captured;
                    UpdateModelSelectorButton();
                    ModelChanged?.Invoke(this, _selectedModel, _selectedEffort);
                    if (_hideModelPopover != null)
                    {
                        _hideModelPopover();
                        _hideModelPopover = null;
                    }
                });
                content.Add(btn);
            }

            Tippy.ShowFor(_modelSelectorBtn, content, out _hideModelPopover, placement: TooltipPlacement.TopEnd, maxWidth: 400, onHiddenCallback: () => _hideModelPopover = null);
        }

        /// <summary>
        /// Returns a value indicating whether the component is generating.
        /// </summary>
        public bool IsGenerating
        {
            get => _isGenerating;
            set
            {
                if (_isGenerating == value) return;
                _isGenerating = value;
                if (_chatTriggerBtn != null)
                {
                    if (value)
                    {
                        _chatTriggerBtn.SetIcon(_iconStop).Danger();
                        _container.classList.add("tss-omnibox-generating");
                    }
                    else
                    {
                        _chatTriggerBtn.SetIcon(_iconChat);
                        _chatTriggerBtn.IsDanger = false;
                        _container.classList.remove("tss-omnibox-generating");
                    }
                }
            }
        }

        /// <summary>
        /// Registers a callback invoked when the input event fires.
        /// </summary>
        public OmniBox OnInput(ComponentEventHandler<OmniBox, Event> onInput)
        {
            Input += onInput;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the key down event fires.
        /// </summary>
        public OmniBox OnKeyDown(ComponentEventHandler<OmniBox, KeyboardEvent> onKeyDown)
        {
            KeyDown += onKeyDown;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the key up event fires.
        /// </summary>
        public OmniBox OnKeyUp(ComponentEventHandler<OmniBox, KeyboardEvent> onKeyUp)
        {
            KeyUp += onKeyUp;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the key press event fires.
        /// </summary>
        public OmniBox OnKeyPress(ComponentEventHandler<OmniBox, KeyboardEvent> onKeyPress)
        {
            KeyPress += onKeyPress;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the focus event fires.
        /// </summary>
        public OmniBox OnFocus(ComponentEventHandler<OmniBox, Event> onFocus)
        {
            ReceivedFocus += onFocus;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the blur event fires.
        /// </summary>
        public OmniBox OnBlur(ComponentEventHandler<OmniBox, Event> onBlur)
        {
            LostFocus += onBlur;
            return this;
        }

        /// <summary>
        /// Returns the component configured with the given history.
        /// </summary>
        public OmniBox WithHistory(Func<Task<SearchQuery[]>> historyFetcher)
        {
            _historyFetcher = historyFetcher;
            _searchHistoryBtn.Show();
            return this;
        }

        private async Task ShowSearchHistory()
        {
            try
            {
                var query = await _historyFetcher();
                
                var content = VStack().NoDefaultMargin().W(450).MaxHeight(500.px()).ScrollY();
                Action hide = null;
                if (query.Length > 0)
                {
                    foreach(var q in query)
                    {
                        content.Add(Button(q.RawQuery).Class("tss-omnibox-search-history-entry").SetIcon(UIcons.TimePast).WS().NoPadding().NoMinSize().H(32).MB(4).NoBorder().NoBackground().TextLeft().OnClick(() =>
                        {
                            _searchInput.value = q.RawQuery;
                            OnSearchInputChanged();
                            TriggerSearch();
                            hide();
                        }));
                    }
                }
                else
                {
                    content.Children(Message("No recent searches", "Your recent searches will appear here once you start searching").Icon(UIcons.EmptySet).H(200));
                }

                window.setTimeout(_ =>
                {
                    Tippy.ShowFor(_searchHistoryBtn, content, out hide, placement: TooltipPlacement.BottomStart, maxWidth: 500, delayHide: 500);
                }, 1);
            }
            catch (Exception ex)
            {
                console.error("Failed to fetch history: ", ex);
            }
        }

        /// <summary>
        /// Sets the keyboard tab order of the component.
        /// </summary>
        public int TabIndex
        {
            set
            {
                if(_searchInput is object) _searchInput.tabIndex = value;
                if(_chatInput   is object) _chatInput.tabIndex = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the component is interactive (enabled).
        /// </summary>
        public bool IsEnabled
        {
            get => !_container.classList.contains("tss-disabled");
            set
            {
                if (value)
                {
                    _container.classList.remove("tss-disabled");
                    if(_searchInput is object) _searchInput.disabled = false;
                    if(_chatInput is object) _chatInput.disabled = false;
                }
                else
                {
                    _container.classList.add("tss-disabled");
                    if(_searchInput is object) _searchInput.disabled = true;
                    if(_chatInput is object) _chatInput.disabled = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        public string SearchText
        {
            get => _searchInput.value;
            set
            {
                _searchInput.value = value;
                OnSearchInputChanged();
                Input?.Invoke(this, null);
            }
        }

        /// <summary>
        /// Gets or sets the chat text.
        /// </summary>
        public string ChatText
        {
            get => _chatInput.value;
            set
            {
                _chatInput.value = value;
                UpdateChatTriggerActiveState();
                Input?.Invoke(this, null);
            }
        }

        /// <summary>
        /// Gets or sets the search placeholder.
        /// </summary>
        public string SearchPlaceholder
        {
            get => _searchInput.placeholder;
            set => _searchInput.placeholder = value;
        }

        /// <summary>
        /// Gets or sets the chat placeholder.
        /// </summary>
        public string ChatPlaceholder
        {
            get => _chatInput.placeholder;
            set => _chatInput.placeholder = value;
        }


        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public string Background { get => _container.style.background; set => _container.style.background = value; }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render()
        {
            return _container;
        }


        private void TriggerSuggestions()
        {
            if (_suggestionsFetcher == null) return;

            window.clearTimeout(_suggestionsDebounceTimeoutId);
            _suggestionsDebounceTimeoutId = (int)window.setTimeout(async _ =>
            {
                var val = _searchInput.value;
                var suggestions = await _suggestionsFetcher(val);
                if (suggestions == null || suggestions.Length == 0)
                {
                    if (_hideSuggestions != null)
                    {
                        _hideSuggestions();
                        _hideSuggestions = null;
                        _highlightedSuggestionIndex = -1;
                    }
                    return;
                }

                _currentSuggestionButtons.Clear();

                var content = VStack().NoDefaultMargin().W(450).MaxHeight(500.px()).ScrollY();

                var currentCategory = string.Empty;

                foreach(var s in suggestions)
                {
                    if (s.Category != currentCategory && !string.IsNullOrEmpty(s.Category))
                    {
                        content.Add(TextBlock(s.Category).Small().SemiBold().PaddingBottom(4.px()).PaddingTop(8.px()).PaddingLeft(12.px()).Foreground(Theme.Secondary.Foreground));
                        currentCategory = s.Category;
                    }

                    var btn = Button().Class("tss-omnibox-search-history-entry").WS().NoPadding().NoMinSize().H(32).MB(4).NoBorder().NoBackground().TextLeft();
                    var btnContent = HStack().WS().AlignItems(ItemAlign.Center).PaddingLeft(12.px()).PaddingRight(12.px());

                    if (s.Icon != null) btnContent.Add(s.Icon.MR(8));
                    if (s.Text != null) btnContent.Add(s.Text);
                    if (s.RightComponent != null) btnContent.Add(s.RightComponent.ML(UnitSize.Auto()));

                    btn.ReplaceContent(btnContent);
                    btn.OnClick(() =>
                    {
                        if (s.OnSelected != null) s.OnSelected(this);
                        if (_hideSuggestions != null) _hideSuggestions();
                        _highlightedSuggestionIndex = -1;
                    });
                    content.Add(btn);

                    _currentSuggestionButtons.Add(btn);
                }

                if (_hideSuggestions != null)
                {
                    _hideSuggestions();
                    _highlightedSuggestionIndex = -1;
                }

                Tippy.ShowFor(_activeInput, content.Render(), out _hideSuggestions, placement: TooltipPlacement.BottomStart, maxWidth: 500);

            }, 300);
        }

        private void UpdateHighlightedSuggestion(int offset)
        {
            if (_currentSuggestionButtons.Count == 0) return;

            if (_highlightedSuggestionIndex >= 0 && _highlightedSuggestionIndex < _currentSuggestionButtons.Count)
            {
                _currentSuggestionButtons[_highlightedSuggestionIndex].RemoveClass("tss-omnibox-suggestion-highlight");
            }

            _highlightedSuggestionIndex = (_highlightedSuggestionIndex + offset + _currentSuggestionButtons.Count) % _currentSuggestionButtons.Count;

            _currentSuggestionButtons[_highlightedSuggestionIndex].Class("tss-omnibox-suggestion-highlight");
        }

        /// <summary>
        /// Registers a global keyboard shortcut that focuses the OmniBox search input when pressed,
        /// and renders a visual chip showing the shortcut on the right side of the search box.
        /// In SearchAndChat mode, pressing the shortcut also switches the active mode to Search.
        /// Modifier names are case-insensitive ("Ctrl", "Cmd", "Meta", "Alt", "Shift").
        /// Example: <c>SetKeyboardShortcut("Ctrl", "K")</c>.
        /// </summary>
        public OmniBox SetKeyboardShortcut(params string[] keys)
        {
            if (_mode != Mode.Search && _mode != Mode.SearchAndChat)
            {
                throw new InvalidOperationException("SetKeyboardShortcut can only be called when OmniBox is in Search or SearchAndChat mode.");
            }

            if (_globalShortcutHandler is object)
            {
                window.removeEventListener("keydown", _globalShortcutHandler);
                _globalShortcutHandler = null;
            }

            while (_searchShortcutContainer.firstChild is object)
            {
                _searchShortcutContainer.removeChild(_searchShortcutContainer.firstChild);
            }

            if (keys is null || keys.Length == 0)
            {
                _shortcutKeys = null;
                _container.classList.remove("tss-omnibox-has-shortcut");
                return this;
            }

            _shortcutKeys = keys;
            _searchShortcutContainer.appendChild(KeyboardShortcut(keys).Render());
            _container.classList.add("tss-omnibox-has-shortcut");

            _globalShortcutHandler = ev =>
            {
                if (!_container.IsMounted()) return;
                if (!IsEnabled) return;

                var e = ev.As<KeyboardEvent>();
                if (!MatchesShortcut(e, _shortcutKeys)) return;

                StopEvent(e);
                if (_mode == Mode.SearchAndChat && _activeMode.Value != Mode.Search)
                {
                    _modeToggle.Select(Mode.Search);
                }
                _searchInput.focus();
                _searchInput.select();
            };

            window.addEventListener("keydown", _globalShortcutHandler);
            return this;
        }

        private static bool MatchesShortcut(KeyboardEvent e, string[] keys)
        {
            bool needCtrl  = false;
            bool needAlt   = false;
            bool needShift = false;
            bool needMeta  = false;
            string mainKey = null;

            foreach (var raw in keys)
            {
                if (raw is null) continue;
                var k = raw.Trim();
                switch (k)
                {
                    case "Ctrl":
                    case "ctrl":
                    case "Control":
                        needCtrl = true;
                        break;
                    case "Alt":
                    case "alt":
                        needAlt = true;
                        break;
                    case "Shift":
                    case "shift":
                        needShift = true;
                        break;
                    case "Meta":
                    case "meta":
                    case "Cmd":
                    case "cmd":
                        needMeta = true;
                        break;
                    default:
                        mainKey = k;
                        break;
                }
            }

            // On macOS, treat "Ctrl" in the shortcut as either Ctrl or Cmd so a single
            // declaration like ("Ctrl", "K") renders ⌘K and triggers on Cmd+K.
            bool isApple = navigator.userAgent.IndexOf("Mac") >= 0 || navigator.userAgent.IndexOf("iPhone") >= 0 || navigator.userAgent.IndexOf("iPad") >= 0;

            if (needCtrl && !needMeta && isApple)
            {
                if (!e.metaKey && !e.ctrlKey) return false;
            }
            else
            {
                if (needCtrl != e.ctrlKey) return false;
                if (needMeta != e.metaKey) return false;
            }

            if (needAlt   != e.altKey)   return false;
            if (needShift != e.shiftKey) return false;

            if (mainKey is null) return false;

            return string.Equals(e.key, mainKey, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sets the search right text of the component.
        /// </summary>
        public OmniBox SetSearchRightText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _searchRightTextContainer.textContent = "";
                _searchRightTextContainer.style.display = "none";
            }
            else
            {
                _searchRightTextContainer.textContent = text;
                _searchRightTextContainer.style.display = "flex";
            }
            return this;
        }

        private void RenderInlineChips(IReadOnlyList<InlineFilterChip> chips)
        {
            ClearChildren(_searchInlineChipsContainer);
            foreach (var chip in chips)
            {
                var chipEl = Div(_("tss-omnibox-inline-chip"));
                if (!string.IsNullOrEmpty(chip.Background)) chipEl.style.background = chip.Background;
                if (!string.IsNullOrEmpty(chip.Color)) chipEl.style.color = chip.Color;

                HTMLElement contentEl;
                if (chip.Content is object)
                {
                    contentEl = chip.Content.Render();
                }
                else
                {
                    contentEl = Span(_(text: chip.Name));
                }

                if (chip.OnClick is object)
                {
                    chipEl.onclick = (e) =>
                    {
                        StopEvent(e);
                        chip.OnClick(e);
                    };
                    chipEl.style.cursor = "pointer";
                }

                chipEl.appendChild(contentEl);

                if (chip.Removable)
                {
                    var closeBtn = Div(_("tss-omnibox-inline-chip-close"));
                    closeBtn.appendChild(UI.Icon(UIcons.Cross).Render());

                    closeBtn.onclick = (e) =>
                    {
                        StopEvent(e);
                        InlineFilterChips.Remove(chip);
                    };
                    chipEl.appendChild(closeBtn);
                }

                _searchInlineChipsContainer.appendChild(chipEl);
            }
        }

        /// <summary>
        /// Sets the search text of the component.
        /// </summary>
        public OmniBox SetSearchText(string text)
        {
            SearchText = text;
            return this;
        }

        /// <summary>
        /// Sets the search placeholder of the component.
        /// </summary>
        public OmniBox SetSearchPlaceholder(string text)
        {
            SearchPlaceholder = text;
            return this;
        }

        /// <summary>
        /// Sets the chat text of the component.
        /// </summary>
        public OmniBox SetChatText(string text)
        {
            ChatText = text;
            return this;
        }

        /// <summary>
        /// Sets the chat placeholder of the component.
        /// </summary>
        public OmniBox SetChatPlaceholder(string text)
        {
            ChatPlaceholder = text;
            return this;
        }

        /// <summary>
        /// Disables the component.
        /// </summary>
        public OmniBox Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        /// <summary>
        /// Moves keyboard focus to the component.
        /// </summary>
        public OmniBox Focus()
        {
            DomObserver.WhenMounted(_searchInput, () =>
            {
                _activeInput.focus();
            });
            return this;
        }

        /// <summary>
        /// Gets or sets the CSS height of the component.
        /// </summary>
        public OmniBox Height(UnitSize unitSize)
        {
            var h = unitSize.ToString();
            if (_mode == Mode.Search || _mode == Mode.SearchAndChat)
            {
                _searchInputContainer.style.height = h;
                _searchInput.style.lineHeight = h;
                _searchTokensContainer.style.lineHeight = h;
            }
            _container.style.height = h;
            return this;
        }

        /// <summary>
        /// Shortcut for setting the height in pixels.
        /// </summary>
        public OmniBox H(int unitSize) => Height(unitSize.px());

        public class SearchToken
        {
            public enum TokenType
            {
                Word,
                And,
                Or,
                Not,
                ParenthesisOpen,
                ParenthesisClose,
                Quote,
                Whitespace
            }

            /// <summary>
            /// Gets or sets the type.
            /// </summary>
            public TokenType Type { get; set; }
            /// <summary>
            /// Gets or sets the current value of the component.
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public SearchToken(TokenType type, string value)
            {
                Type = type;
                Value = value;
            }
        }

        public class SearchQuery
        {
            /// <summary>
            /// Gets or sets the raw query.
            /// </summary>
            public string RawQuery { get; set; }
            /// <summary>
            /// Gets or sets the tokens.
            /// </summary>
            public List<SearchToken> Tokens { get; set; }
            /// <summary>
            /// Gets or sets the snaps.
            /// </summary>
            public SnapHandler[] Snaps { get; set; }
            /// <summary>
            /// Gets or sets the filter snaps.
            /// </summary>
            public FilterSnap[] FilterSnaps { get; set; }

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public SearchQuery(string rawQuery, List<SearchToken> tokens)
            {
                RawQuery = rawQuery;
                Tokens = tokens;
                Snaps = Array.Empty<SnapHandler>();
                FilterSnaps = Array.Empty<FilterSnap>();
            }
        }

        public class ChatMessage
        {
            /// <summary>
            /// Gets or sets the text shown in the component.
            /// </summary>
            public string Text { get; set; }
        }
    }
}
