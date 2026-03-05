using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.OmniBox")]
    public class OmniBox : IComponent, IHasBackgroundColor, ITabIndex
    {
        public class Config
        {
            public Config(Mode mode, Mode? initialMode = null)
            {
                Mode = mode;
                InitialMode = initialMode ?? (mode == Mode.SearchAndChat ? Mode.Search : mode); 
            }

            public string PlaceholderSearch { get; set; }
            public string PlaceholderChat { get; set; }
            public Mode Mode { get; }
            public Mode InitialMode { get; }

            public IComponent[] ChatFooterLeftSide { get; set; }
            public IComponent[] ChatFooterRightSide { get; set;}
            public IComponent[] SearchFooterLeftSide { get; set; }
            public IComponent[] SearchFooterRightSide { get; set;}
        }

        public enum Mode
        {
            Search,
            Chat,
            SearchAndChat
        }


        private readonly Mode _mode;
        private readonly SettableObservable<Mode> _activeMode;
        private HTMLElement _activeInput;

        private readonly HTMLDivElement   _container;
        private readonly HTMLDivElement   _searchContainer;
        private readonly HTMLInputElement _searchInput;
        private readonly HTMLDivElement   _searchTokensContainer;
        private readonly HTMLDivElement   _searchInputContainer;
        private readonly Button      _searchHistoryBtn;
        private readonly Button      _searchClearBtn;
        private readonly Button      _searchTriggerBtn;

        private readonly HTMLTextAreaElement _chatInput;
        private readonly HTMLDivElement   _chatContainer;
        private readonly Button           _chatTriggerBtn;
        private readonly HTMLDivElement _footer;
        private Func<Task<SearchQuery[]>> _historyFetcher;
        private string _tokenTypeMap = string.Empty;
        private readonly IconToggle<Mode> _modeToggle;

        public delegate void SearchEventHandler(OmniBox sender, SearchQuery query);
        public delegate void ChatEventHandler(OmniBox sender, ChatMessage query);
        protected event SearchEventHandler Searched;
        protected event ChatEventHandler Chatted;

        public event ComponentEventHandler<OmniBox, Event> Input;

        public OmniBox(Config config)
        {
            _mode = config.Mode;

            _activeMode = new SettableObservable<Mode>(config.InitialMode);

            _footer = Div(_("tss-omnibox-footer"));

            var footerEnd = new List<IComponent>();

            if (_mode == Mode.SearchAndChat)
            {
                _modeToggle = IconToggle(IconToggleItem(UIcons.Beacon, "Chat", Mode.Chat), IconToggleItem(UIcons.Search, "Search", Mode.Search));
                _modeToggle.Select(_activeMode.Value);
                _modeToggle.AsObservable().ObserveFutureChanges(v => _activeMode.Value = v);
                _footer.appendChild(_modeToggle.Render());
            }


            if (_mode == Mode.Search || _mode == Mode.SearchAndChat)
            {
                _searchInput = TextBox(_("tss-omnibox-search-input", type: "text", placeholder: config.PlaceholderSearch ?? ""));
                _searchInput.autocomplete = "off";
                _searchInput.spellcheck = false;

                _searchTokensContainer = Div(_("tss-omnibox-search-tokens"));
                _searchInputContainer = Div(_("tss-omnibox-search-input-container"), _searchInput, _searchTokensContainer);

                _searchHistoryBtn = Button().SetIcon(UIcons.TimePast).Class("tss-omnibox-search-history-btn");
                _searchHistoryBtn.Collapse(); // Hidden by default unless WithHistory is called

                _searchClearBtn = Button().SetIcon(UIcons.CrossCircle).Class("tss-omnibox-search-clear-btn");
                _searchTriggerBtn = Button().SetIcon(UIcons.Search).Class("tss-omnibox-search-btn");

                if (_mode == Mode.Search)
                {
                    _searchContainer = Div(_("tss-omnibox-search-container"), _searchHistoryBtn.Render(), _searchInputContainer, _searchClearBtn.Render(), _searchTriggerBtn.Render());
                }
                else
                {
                    _searchContainer = Div(_("tss-omnibox-search-container"), _searchInputContainer);
                    _footer.appendChild(_searchHistoryBtn.Render());
                    footerEnd.Add(_searchClearBtn);
                    footerEnd.Add(_searchTriggerBtn);
                }

                // Set up event listeners
                _searchInput.addEventListener("input", (e) =>
                {
                    OnSearchInputChanged();
                    Input?.Invoke(this, e);
                });

                _searchInput.addEventListener("keydown", (e) =>
                {
                    var ke = e.As<KeyboardEvent>();
                    if (ke.key == "Enter")
                    {
                        TriggerSearch();
                    }
                    else if (ke.key == "Backspace")
                    {
                        var cursorPos = _searchInput.selectionStart;
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
                                e.preventDefault();
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
                                    e.preventDefault();
                                    var currentVal = _searchInput.value;
                                    var newVal = currentVal.Substring(0, (int)cursorPos) + currentVal.Substring((int)cursorPos + 2);
                                    _searchInput.value = newVal;
                                    _searchInput.setSelectionRange((uint)cursorPos, (uint)cursorPos);
                                    OnSearchInputChanged();
                                }
                                else
                                {
                                    // Edge case: space is at the very end of the string
                                    e.preventDefault();
                                    var currentVal = _searchInput.value;
                                    var newVal = currentVal.Substring(0, (int)cursorPos);
                                    _searchInput.value = newVal;
                                    _searchInput.setSelectionRange((uint)cursorPos, (uint)cursorPos);
                                    OnSearchInputChanged();
                                }
                            }
                        }
                    }
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
                        e.preventDefault();
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
                        e.preventDefault();
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
                _chatTriggerBtn = Button().SetIcon(UIcons.PaperPlane).Class("tss-omnibox-chat-btn");

                if (config.ChatFooterLeftSide is object)
                {
                    foreach (var i in config.ChatFooterLeftSide)
                    {
                        _footer.appendChild(i.Class("tss-omnibox-chat-footer-item").Render());
                    }
                }

                if (config.SearchFooterLeftSide is object)
                {
                    foreach (var i in config.SearchFooterLeftSide)
                    {
                        _footer.appendChild(i.Class("tss-omnibox-search-footer-item").Render());
                    }
                }


                _footer.appendChild(Div(_("tss-omnibox-footer-spacer")));
                footerEnd.Add(_chatTriggerBtn);
                _chatContainer = Div(_("tss-omnibox-chat-container"), _chatInput);
            }

            if (config.ChatFooterRightSide is object)
            {
                foreach (var i in config.ChatFooterRightSide)
                {
                    _footer.appendChild(i.Class("tss-omnibox-chat-footer-item").Render());
                }
            }

            if (config.SearchFooterRightSide is object)
            {
                foreach (var i in config.SearchFooterRightSide )
                {
                    _footer.appendChild(i.Class("tss-omnibox-search-footer-item").Render());
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
            var rawQuery = ParseQuery(val.Replace(specialWhitespaceString, " "));
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
            var query = ParseQuery(input);
            RenderTokens(query.Tokens);
            SyncScroll();
        }

        public static SearchQuery ParseQuery(string input)
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
                var upperWord = word.ToUpper();
                if (upperWord == "AND")
                {
                    tokens.Add(new SearchToken(SearchToken.TokenType.And, word));
                }
                else if (upperWord == "OR")
                {
                    tokens.Add(new SearchToken(SearchToken.TokenType.Or, word));
                }
                else if (upperWord == "NOT")
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
            var query = ParseQuery(val);
            Searched?.Invoke(this, query);
        }

        public OmniBox OnSearch(SearchEventHandler onSearch)
        {
            Searched += onSearch;
            return this;
        }

        public OmniBox OnSearch(Action<OmniBox, string> onSearch)
        {
            Searched += (s, q) => onSearch(s, q.RawQuery);
            return this;
        }

        public OmniBox OnChat(ChatEventHandler onChat)
        {
            Chatted += onChat;
            return this;
        }

        public OmniBox OnChat(Action<OmniBox, string> onChat)
        {
            Chatted += (s, q) => onChat(s, q.Text);
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
