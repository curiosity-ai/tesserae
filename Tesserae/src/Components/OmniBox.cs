using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.OmniBox")]
    public class OmniBox : IComponent, IHasBackgroundColor, ITabIndex
    {

        public class InlineFilterChip
        {
            public string Name { get; }
            public string Color { get; }
            public string Background { get; }
            public bool Removable { get; }
            public Action<MouseEvent> OnClick { get; }
            internal IComponent Content { get; }

            public InlineFilterChip(string name, string background = null, string color = null, Action<MouseEvent> onClick = null, bool removable = true)
            {
                Name = name;
                Background = background;
                Color = color;
                OnClick = onClick;
                Removable = removable;
            }

            public InlineFilterChip(IComponent content, Action<MouseEvent> onClick = null, bool removable = true)
            {
                Content = content;
                OnClick = onClick;
                Removable = removable;
            }
        }

        public class OmniBoxSuggestionItem
        {
            public IComponent Icon { get; }
            public IComponent Text { get; }
            public IComponent RightComponent { get; }
            public Action<OmniBox> OnSelected { get; }
            public string Category { get; }

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

            public OmniBoxSuggestionItem(string text, IComponent icon = null, IComponent rightComponent = null, Action<OmniBox> onSelected = null, string category = null) : this(TextBlock(text), icon, rightComponent, onSelected, category)
            {
            }
        }

        public class Config
        {
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

            public string PlaceholderSearch { get; set; }
            public string PlaceholderChat { get; set; }
            public Mode Mode { get; }
            public Mode InitialMode { get; }

            public UIcons IconSearch { get; set; }
            public UIcons IconChat { get; set; }
            public UIcons IconStop { get; set; }
            public UIcons IconModeToggleSearch { get; set; }
            public UIcons IconModeToggleChat { get; set; }
            public string TooltipModeToggleSearch { get; set; }
            public string TooltipModeToggleChat { get; set; }
            public bool ExpandOnFocus { get; set; }
            public bool TokenIgnoreCase { get; set; }

            public FooterItems ChatFooter { get; set; }
            public FooterItems SearchFooter { get; set; }

            public Func<string, Task<OmniBoxSuggestionItem[]>> SuggestionsFetcher { get; set; }
        }

        public class FooterItems
        {
            public IComponent[] LeftSide { get; set; }
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
            public string Name { get; }
            public string Description { get; }
            public object Tag { get; }

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
        public event StopEventHandler Stopped;
        protected event ModelChangedEventHandler ModelChanged;

        public event ComponentEventHandler<OmniBox, Event> Input;
        public event ComponentEventHandler<OmniBox, KeyboardEvent> KeyDown;
        public event ComponentEventHandler<OmniBox, KeyboardEvent> KeyUp;
        public event ComponentEventHandler<OmniBox, KeyboardEvent> KeyPress;
        public event ComponentEventHandler<OmniBox, Event> ReceivedFocus;
        public event ComponentEventHandler<OmniBox, Event> LostFocus;

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
                    TriggerSuggestions();
                });

                _searchInput.addEventListener("keydown", (e) =>
                {
                    var ke = e.As<KeyboardEvent>();
                    if (ke.key == "Enter")
                    {
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
                        if (_hideSuggestions != null && _currentSuggestionButtons.Count > 0)
                        {
                            StopEvent(e);
                            UpdateHighlightedSuggestion(1);
                        }
                    }
                    else if (ke.key == "ArrowUp")
                    {
                        if (_hideSuggestions != null && _currentSuggestionButtons.Count > 0)
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

                _chatInput.addEventListener("input", (e) => Input?.Invoke(this, e));
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
                    //if (_mode == Mode.Chat)
                    //{
                    //    foreach (var i in config.ChatFooter.LeftSide)
                    //    {
                    //        var el = i.Class("tss-omnibox-chat-footer-item").Render();
                    //        _chatContainer.insertBefore(el, _chatInput);
                    //    }
                    //}
                    //else
                    //{
                        foreach (var i in config.ChatFooter.LeftSide)
                        {
                            _footer.appendChild(i.Class("tss-omnibox-chat-footer-item").Class("tss-omnibox-footer-left").Render());
                        }
                    //}
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
                    if ((config.ChatFooter?.LeftSide?.Length + config.ChatFooter?.RightSide?.Length) > 0)
                    {
                        _container.classList.add("tss-omnibox-chat-with-footer");
                    }
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
                if (c == '!' || c == '-')
                {
                    tokens.Add(new SearchToken(SearchToken.TokenType.Not, c.ToString()));
                    i++;
                    continue;
                }

                // Word token
                int wordStart = i;
                while (i < input.Length && !char.IsWhiteSpace(input[i]) && input[i] != '(' && input[i] != ')' && input[i] != '"' && input[i] != '\'' && input[i] != '&' && input[i] != '|' && input[i] != '!' && input[i] != '-')
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
            Searched?.Invoke(this, query);
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
        }

        private void TriggerStop()
        {
            Stopped?.Invoke(this);
        }

        public OmniBox OnSearch(SearchEventHandler onSearch)
        {
            Searched += onSearch;
            return this;
        }

        public OmniBox OnChat(ChatEventHandler onChat)
        {
            Chatted += onChat;
            return this;
        }

        public OmniBox OnStop(StopEventHandler onStop)
        {
            Stopped += onStop;
            return this;
        }

        public OmniBox OnModelChanged(ModelChangedEventHandler onModelChanged)
        {
            ModelChanged += onModelChanged;
            return this;
        }

        public OmniBox SetModels(params ModelOption[] models)
        {
            return SetModels((IEnumerable<ModelOption>)models);
        }

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

        public OmniBox SetThinkingEffort(ThinkingEffort effort)
        {
            _selectedEffort = effort;
            UpdateModelSelectorButton();
            return this;
        }

        public ModelOption SelectedModel => _selectedModel;
        public ThinkingEffort SelectedThinkingEffort => _selectedEffort;
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

        public OmniBox OnInput(ComponentEventHandler<OmniBox, Event> onInput)
        {
            Input += onInput;
            return this;
        }

        public OmniBox OnKeyDown(ComponentEventHandler<OmniBox, KeyboardEvent> onKeyDown)
        {
            KeyDown += onKeyDown;
            return this;
        }

        public OmniBox OnKeyUp(ComponentEventHandler<OmniBox, KeyboardEvent> onKeyUp)
        {
            KeyUp += onKeyUp;
            return this;
        }

        public OmniBox OnKeyPress(ComponentEventHandler<OmniBox, KeyboardEvent> onKeyPress)
        {
            KeyPress += onKeyPress;
            return this;
        }

        public OmniBox OnFocus(ComponentEventHandler<OmniBox, Event> onFocus)
        {
            ReceivedFocus += onFocus;
            return this;
        }

        public OmniBox OnBlur(ComponentEventHandler<OmniBox, Event> onBlur)
        {
            LostFocus += onBlur;
            return this;
        }

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

        public int TabIndex
        {
            set
            {
                if(_searchInput is object) _searchInput.tabIndex = value;
                if(_chatInput   is object) _chatInput.tabIndex = value;
            }
        }

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

        public string ChatText
        {
            get => _chatInput.value;
            set
            {
                _chatInput.value = value;
                Input?.Invoke(this, null);
            }
        }

        public string SearchPlaceholder
        {
            get => _searchInput.placeholder;
            set => _searchInput.placeholder = value;
        }

        public string ChatPlaceholder
        {
            get => _chatInput.placeholder;
            set => _chatInput.placeholder = value;
        }


        public string Background { get => _container.style.background; set => _container.style.background = value; }

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

        public OmniBox SetSearchText(string text)
        {
            SearchText = text;
            return this;
        }

        public OmniBox SetSearchPlaceholder(string text)
        {
            SearchPlaceholder = text;
            return this;
        }

        public OmniBox SetChatText(string text)
        {
            ChatText = text;
            return this;
        }

        public OmniBox SetChatPlaceholder(string text)
        {
            ChatPlaceholder = text;
            return this;
        }

        public OmniBox Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        public OmniBox Focus()
        {
            DomObserver.WhenMounted(_searchInput, () =>
            {
                _activeInput.focus();
            });
            return this;
        }

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

            public TokenType Type { get; set; }
            public string Value { get; set; }

            public SearchToken(TokenType type, string value)
            {
                Type = type;
                Value = value;
            }
        }

        public class SearchQuery
        {
            public string RawQuery { get; set; }
            public List<SearchToken> Tokens { get; set; }

            public SearchQuery(string rawQuery, List<SearchToken> tokens)
            {
                RawQuery = rawQuery;
                Tokens = tokens;
            }
        }

        public class ChatMessage
        {
            public string Text { get; set; }
        }
    }
}
