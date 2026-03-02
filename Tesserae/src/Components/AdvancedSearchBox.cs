using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
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

    [H5.Name("tss.AdvancedSearchBox")]
    public class AdvancedSearchBox : ComponentBase<AdvancedSearchBox, HTMLDivElement>, ITextFormating, IHasBackgroundColor, ITabIndex, IRoundedStyle
    {
        private readonly HTMLDivElement _container;
        private readonly HTMLInputElement _input;
        private readonly HTMLDivElement _tokensContainer;
        private readonly HTMLDivElement _inputContainer;

        private readonly HTMLElement _historyBtn;
        private readonly HTMLElement _clearBtn;
        private readonly HTMLElement _searchBtn;

        private Func<Task<SearchQuery>> _historyFetcher;

        public delegate void SearchEventHandler(AdvancedSearchBox sender, SearchQuery query);
        protected event SearchEventHandler Searched;

        public AdvancedSearchBox(string placeholder = string.Empty)
        {
            _input = TextBox(_("tss-advancedsearchbox-input", type: "text", placeholder: placeholder));
            _input.autocomplete = "off";
            _input.spellcheck = false;

            _tokensContainer = Div(_("tss-advancedsearchbox-tokens"));
            _inputContainer = Div(_("tss-advancedsearchbox-input-container"), _input, _tokensContainer);

            _historyBtn = Div(_("tss-advancedsearchbox-history-btn"), Icon(UIcons.Clock).Render());
            _historyBtn.style.display = "none"; // Hidden by default unless WithHistory is called

            _clearBtn = Div(_("tss-advancedsearchbox-clear-btn tss-hidden"), Icon(UIcons.CrossCircle).Render());
            _searchBtn = Div(_("tss-advancedsearchbox-search-btn"), Icon(UIcons.Search).Render());

            _container = Div(_("tss-advancedsearchbox-container"), _historyBtn, _inputContainer, _clearBtn, _searchBtn);

            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
            AttachKeys();

            // Set up event listeners
            _input.addEventListener("input", (e) => OnInputChanged());
            _input.addEventListener("keydown", (e) =>
            {
                var ke = e.As<KeyboardEvent>();
                if (ke.key == "Enter")
                {
                    TriggerSearch();
                }
            });
            _input.addEventListener("scroll", (e) => SyncScroll());

            _clearBtn.addEventListener("click", (e) =>
            {
                _input.value = string.Empty;
                OnInputChanged();
                _input.focus();
            });

            _searchBtn.addEventListener("click", (e) => TriggerSearch());

            _historyBtn.addEventListener("click", async (e) =>
            {
                if (_historyFetcher != null)
                {
                    await ShowHistoryModal();
                }
            });

            // Initial parse
            OnInputChanged();
        }

        private void SyncScroll()
        {
            _tokensContainer.scrollLeft = _input.scrollLeft;
        }

        private void OnInputChanged()
        {
            var val = _input.value;
            if (string.IsNullOrEmpty(val))
            {
                _clearBtn.classList.add("tss-hidden");
                ClearTokens();
            }
            else
            {
                _clearBtn.classList.remove("tss-hidden");
                ParseAndRenderTokens(val);
            }
        }

        private void ClearTokens()
        {
            _tokensContainer.innerHTML = "";
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
            foreach (var token in tokens)
            {
                string className = "";
                switch (token.Type)
                {
                    case SearchToken.TokenType.And:
                        className = "tss-adv-token-operator-and";
                        break;
                    case SearchToken.TokenType.Or:
                        className = "tss-adv-token-operator-or";
                        break;
                    case SearchToken.TokenType.Not:
                        className = "tss-adv-token-operator-not";
                        break;
                    case SearchToken.TokenType.ParenthesisOpen:
                    case SearchToken.TokenType.ParenthesisClose:
                        className = "tss-adv-token-paren";
                        break;
                    case SearchToken.TokenType.Quote:
                        className = "tss-adv-token-quote";
                        break;
                    case SearchToken.TokenType.Word:
                        className = "tss-adv-token-word";
                        break;
                }

                if (string.IsNullOrEmpty(className))
                {
                    var span = Span(_(text: token.Value));
                    _tokensContainer.appendChild(span);
                }
                else
                {
                    var span = Span(_(className, text: token.Value));
                    _tokensContainer.appendChild(span);
                }
            }
        }

        private void TriggerSearch()
        {
            var val = _input.value;
            var query = ParseQuery(val);
            Searched?.Invoke(this, query);
        }

        public AdvancedSearchBox OnSearch(SearchEventHandler onSearch)
        {
            Searched += onSearch;
            return this;
        }

        public AdvancedSearchBox OnSearch(Action<AdvancedSearchBox, string> onSearch)
        {
            Searched += (s, q) => onSearch(s, q.RawQuery);
            return this;
        }

        public AdvancedSearchBox WithHistory(Func<Task<SearchQuery>> historyFetcher)
        {
            _historyFetcher = historyFetcher;
            _historyBtn.style.display = "flex";
            return this;
        }

        private async Task ShowHistoryModal()
        {
            try
            {
                var query = await _historyFetcher();
                if (query != null && !string.IsNullOrEmpty(query.RawQuery))
                {
                    var content = VStack().Padding(8.px()).Children(
                        TextBlock("Recent searches").SemiBold().PaddingBottom(4.px()),
                        Button(query.RawQuery).NoBorder().NoBackground().Do(b => b.Render().style.textAlign = "left").OnClick(() =>
                        {
                            _input.value = query.RawQuery;
                            OnInputChanged();
                            TriggerSearch();
                            Tippy.HideAll();
                        })
                    );

                    Tippy.ShowFor(Raw(_historyBtn), content, out var tippyInstance, placement: TooltipPlacement.BottomStart, maxWidth: 500);
                }
            }
            catch (Exception ex)
            {
                console.error("Failed to fetch history: ", ex);
            }
        }

        public int TabIndex
        {
            set => _input.tabIndex = value;
        }

        public bool IsEnabled
        {
            get => !_container.classList.contains("tss-disabled");
            set
            {
                if (value)
                {
                    _container.classList.remove("tss-disabled");
                    _input.disabled = false;
                }
                else
                {
                    _container.classList.add("tss-disabled");
                    _input.disabled = true;
                }
            }
        }

        public string Text
        {
            get => _input.value;
            set
            {
                _input.value = value;
                OnInputChanged();
                RaiseOnInput(null);
            }
        }

        public string Placeholder
        {
            get => _input.placeholder;
            set => _input.placeholder = value;
        }

        public TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(_input, TextSize.Small);
            set
            {
                _input.classList.remove(Size.ToString());
                _input.classList.add(value.ToString());
                _tokensContainer.classList.remove(Size.ToString());
                _tokensContainer.classList.add(value.ToString());
            }
        }

        public TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(_input, TextWeight.Regular);
            set
            {
                _input.classList.remove(Weight.ToString());
                _input.classList.add(value.ToString());
                _tokensContainer.classList.remove(Weight.ToString());
                _tokensContainer.classList.add(value.ToString());
            }
        }

        public TextAlign TextAlign
        {
            get => ITextFormatingExtensions.FromClassList(_input, TextAlign.Left);
            set
            {
                _input.classList.remove(TextAlign.ToString());
                _input.classList.add(value.ToString());
                _tokensContainer.classList.remove(TextAlign.ToString());
                _tokensContainer.classList.add(value.ToString());
            }
        }

        public string Background { get => _container.style.background; set => _container.style.background = value; }

        public override HTMLDivElement Render()
        {
            return _container;
        }

        public AdvancedSearchBox SetText(string text)
        {
            Text = text;
            return this;
        }

        public AdvancedSearchBox SetPlaceholder(string text)
        {
            Placeholder = text;
            return this;
        }

        public AdvancedSearchBox Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        public AdvancedSearchBox Focus()
        {
            DomObserver.WhenMounted(_input, () => window.setTimeout((_) => _input.focus(), 500));
            return this;
        }

        public AdvancedSearchBox Height(UnitSize unitSize)
        {
            var h = unitSize.ToString();
            _inputContainer.style.height = h;
            _input.style.lineHeight = h;
            _tokensContainer.style.lineHeight = h;
            _container.style.height = h;
            return this;
        }

        public AdvancedSearchBox H(int unitSize) => Height(unitSize.px());
    }
}
