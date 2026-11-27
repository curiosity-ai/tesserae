using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A text field with two named doors at its edge: <c>Search</c> and <c>Ask</c>. One box for the two things a
    /// home page offers, without a mode to switch: what to do with the text is chosen when it is sent. The text is
    /// plain - nothing is parsed or highlighted - and the field grows with it up to six lines.
    /// <para>Enter searches, Ctrl+Enter (Cmd+Enter) asks and Shift+Enter starts a new line. Both doors are disabled
    /// while the box is empty; <see cref="AskEnabled"/> can keep Ask disabled with a reason, for a workspace that has
    /// nothing to answer with.</para>
    /// <para>Typing <c>@</c> offers the box's <see cref="Mention"/>s - things to search in, assistants to ask - and
    /// <c>key:</c> offers a <see cref="Filter"/>'s values. Each pick becomes a chip in the field. A chip belongs to one
    /// door, and the door it does not belong to folds away, so the box only offers what can still be done with it
    /// and Enter does that.</para>
    /// </summary>
    [Transpose.Name("tss.SearchAskBox")]
    public sealed class SearchAskBox : ComponentBase<SearchAskBox, HTMLTextAreaElement>, ITabIndex
    {
        /// <summary>Where the two doors sit.</summary>
        public enum DoorLayout
        {
            /// <summary>Inside the frame at its right end, Search above Ask, each half the box's height.</summary>
            Stacked,
            /// <summary>Inside the frame at its right end, side by side, each the full height of the box.</summary>
            SideBySide,
            /// <summary>Outside the frame, stacked against its right edge and sharing it, like a cabinet bolted on.</summary>
            Outside
        }

        /// <summary>Which door is drawn filled.</summary>
        public enum PrimaryDoor
        {
            /// <summary>Ask is filled; Search is drawn as the plain door.</summary>
            Ask,
            /// <summary>Search is filled; Ask is drawn as the plain door.</summary>
            Search,
            /// <summary>Neither is filled: both are grey and take the primary colour under the pointer. The default.</summary>
            None
        }

        /// <summary>One of the two doors, and so one of the two things the text can be sent to.</summary>
        public enum Door
        {
            /// <summary>The Search door.</summary>
            Search,
            /// <summary>The Ask door.</summary>
            Ask
        }

        /// <summary>
        /// Something to <c>@</c>: a place to search in (<c>@Files</c>) or an assistant to ask (<c>@Legal</c>). It belongs
        /// to one door; choosing it folds the other one away.
        /// </summary>
        public sealed class Mention
        {
            /// <summary>Creates a mention.</summary>
            /// <param name="id">What the host recognises it by.</param>
            /// <param name="name">What the chip and the suggestion say.</param>
            /// <param name="door">The door it belongs to.</param>
            /// <param name="icon">The glyph on the chip and in the suggestion.</param>
            /// <param name="description">A line under the name in the suggestion.</param>
            /// <param name="group">The heading it is listed under; by default the door's (Search in, Ask).</param>
            /// <param name="tag">Anything the host wants back with it.</param>
            public Mention(string id, string name, Door door, UIcons icon = UIcons.At, string description = null, string group = null, object tag = null)
            {
                Id          = id;
                Name        = name;
                Door        = door;
                Icon        = icon;
                Description = description;
                Group       = group;
                Tag         = tag;
            }

            /// <summary>What the host recognises it by.</summary>
            public string Id { get; }
            /// <summary>What the chip and the suggestion say.</summary>
            public string Name { get; }
            /// <summary>The door it belongs to.</summary>
            public Door Door { get; }
            /// <summary>The glyph on the chip and in the suggestion.</summary>
            public UIcons Icon { get; }
            /// <summary>A line under the name in the suggestion.</summary>
            public string Description { get; }
            /// <summary>The heading it is listed under; null for the door's.</summary>
            public string Group { get; }
            /// <summary>Anything the host wants back with it.</summary>
            public object Tag { get; }
        }

        /// <summary>
        /// A <c>key:value</c> narrowing of a search (<c>ext:pdf</c>, <c>src:SharePoint</c>). Filters belong to the Search
        /// door. Typing one of its keys and a colon lists its values; the <c>@</c> list offers the filter itself.
        /// </summary>
        public sealed class Filter
        {
            private readonly Func<Task<string[]>> _load;
            private string[]                      _values;
            private Task<string[]>                _loading;

            /// <summary>A filter with a fixed list of values.</summary>
            public Filter(string id, string name, string[] keys, string[] values, UIcons icon = UIcons.Filter, string description = null)
                : this(id, name, keys, () => Task.FromResult(values ?? Array.Empty<string>()), icon, description) { }

            /// <summary>A filter whose values are loaded the first time they are needed.</summary>
            public Filter(string id, string name, string[] keys, Func<Task<string[]>> values, UIcons icon = UIcons.Filter, string description = null)
            {
                Id          = id;
                Name        = name;
                Keys        = keys is object && keys.Length > 0 ? keys : new[] { id };
                Icon        = icon;
                Description = description;
                _load       = values;
            }

            /// <summary>What the host recognises it by.</summary>
            public string Id { get; }
            /// <summary>What the chip and the suggestion say.</summary>
            public string Name { get; }
            /// <summary>The words that start it when typed with a colon; the first is the one offered.</summary>
            public string[] Keys { get; }
            /// <summary>The glyph on the chip and in the suggestion.</summary>
            public UIcons Icon { get; }
            /// <summary>A line under the name in the suggestion.</summary>
            public string Description { get; }

            internal async Task<string[]> ValuesAsync()
            {
                if (_values is object) return _values;
                if (_load is null) return _values = Array.Empty<string>();

                _loading ??= _load();
                return _values = (await _loading) ?? Array.Empty<string>();
            }
        }

        /// <summary>A chip in the field: a <see cref="Mention"/>, or a <see cref="Filter"/> with its value.</summary>
        public sealed class Chip
        {
            internal Chip(Mention mention)
            {
                Mention = mention;
            }

            internal Chip(Filter filter, string value)
            {
                Filter = filter;
                Value  = value;
            }

            /// <summary>The mention, when the chip is one.</summary>
            public Mention Mention { get; }
            /// <summary>The filter, when the chip is one.</summary>
            public Filter Filter { get; }
            /// <summary>The filter's value, when the chip is a filter.</summary>
            public string Value { get; }
            /// <summary>The door the chip belongs to.</summary>
            public Door Door => Mention is object ? Mention.Door : Door.Search;

            internal string Label => Mention is object ? Mention.Name : Filter.Name + ": " + Value;
            internal UIcons Icon => Mention is object ? Mention.Icon : Filter.Icon;
        }

        //One row in the suggestion list.
        private sealed class Suggestion
        {
            public int     Score;
            public string  Group;
            public string  Name;
            public string  Description;
            public UIcons  Icon;
            public Action  Pick;
        }

        //What the word at the caret is asking for, and where it starts.
        private sealed class Token
        {
            public int    Start;
            public bool   IsMention;
            public Filter Filter;
            public string Query;
        }

        private const int MaxLines       = 6;
        private const int MaxSuggestions = 30;

        private readonly HTMLDivElement    _host;
        private readonly HTMLDivElement    _container;
        private readonly HTMLDivElement    _field;
        private readonly HTMLDivElement    _chipsRow;
        private readonly HTMLDivElement    _doors;
        private readonly HTMLButtonElement _searchDoor;
        private readonly HTMLButtonElement _askDoor;
        private readonly HTMLSpanElement   _searchWord;
        private readonly HTMLSpanElement   _askWord;
        private readonly HTMLDivElement    _suggestionsPanel;
        private readonly string            _listId;

        private readonly List<Mention> _mentions = new List<Mention>();
        private readonly List<Filter>  _filters  = new List<Filter>();
        private readonly List<Chip>    _chips    = new List<Chip>();

        private Func<Task<IEnumerable<Mention>>> _loadMentions;
        private Task                             _mentionsLoading;

        private List<Suggestion> _shown = new List<Suggestion>();
        private int              _highlighted = -1;
        private Token            _token;
        private int              _suggestionsVersion;

        private readonly Action<Event> _reposition;
        private bool                   _following;

        private bool   _askAllowed = true;
        private string _askDisabledReason;
        private string _askShortcutHint;
        private string _searchGroupName = "Search in";
        private string _askGroupName    = "Ask";
        private string _filterGroupName = "Filters";

        private event Action<SearchAskBox, string> Searched;
        private event Action<SearchAskBox, string> Asked;
        private event Action<SearchAskBox>         ChipsChanged;

        private static int _instances;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="placeholder">The text shown while the box is empty; it is also the field's accessible name.</param>
        public SearchAskBox(string placeholder = null)
        {
            _listId = "tss-searchaskbox-list-" + (++_instances);

            InnerElement = UI.TextArea(Att("tss-searchaskbox-input", placeholder: placeholder ?? string.Empty, ariaLabel: placeholder));
            InnerElement.rows = 1;
            InnerElement.setAttribute("role", "combobox");
            InnerElement.setAttribute("aria-autocomplete", "list");
            InnerElement.setAttribute("aria-expanded", "false");
            InnerElement.setAttribute("aria-controls", _listId);

            var apple = IsApple();

            _searchWord = Span(Att("tss-searchaskbox-word", text: "Search"));
            _askWord    = Span(Att("tss-searchaskbox-word", text: "Ask"));

            //The key that opens each door, shown only with ShowKeys(): hidden from the accessibility tree, since the
            //door's name is its word and the shortcut is already in its tooltip.
            var searchKey = Span(Att("tss-searchaskbox-key", text: "↵"));
            var askKey    = Span(Att("tss-searchaskbox-key", text: apple ? "⌘ ↵" : "Ctrl ↵"));
            searchKey.setAttribute("aria-hidden", "true");
            askKey.setAttribute("aria-hidden", "true");

            _searchDoor = Button(Att("tss-searchaskbox-door tss-searchaskbox-search", type: "button"), _searchWord, searchKey);
            _askDoor    = Button(Att("tss-searchaskbox-door tss-searchaskbox-ask",    type: "button"), _askWord, askKey);

            _chipsRow = Div(Att("tss-searchaskbox-chips"));
            _field    = Div(Att("tss-searchaskbox-field"), _chipsRow, InnerElement);
            _doors    = Div(Att("tss-searchaskbox-doors"), _searchDoor, _askDoor);

            _container = Div(Att("tss-searchaskbox"), _field, _doors);

            //The list is mounted on the body while it is open, as the toolkit's other overlays are, so no container that
            //clips its content can cut it off; it is placed under the box (above it when there is no room below).
            _suggestionsPanel = Div(Att("tss-searchaskbox-suggestions", id: _listId, role: "listbox"));
            _suggestionsPanel.hidden = true;
            //A press in the list must not take the caret out of the field, or the list would close under it.
            _suggestionsPanel.addEventListener("mousedown", e => e.preventDefault());
            _reposition = _ => PlaceSuggestions();

            _host = Div(Att("tss-searchaskbox-host"), _container);

            DomObserver.WhenRemoved(_host, HideSuggestions);

            _askShortcutHint = apple ? "⌘ Enter" : "Ctrl Enter";

            AttachFocus();
            AttachBlur();

            InnerElement.addEventListener("input", _ =>
            {
                Refresh();
                UpdateSuggestions();
            });

            InnerElement.addEventListener("click", _ => UpdateSuggestions());
            InnerElement.addEventListener("blur",  _ => HideSuggestions());
            InnerElement.addEventListener("keydown", e => OnKeyDown(e.As<KeyboardEvent>()));

            //A press on the frame around the chips puts the caret in the field, as a press on any text box would.
            _field.addEventListener("mousedown", e =>
            {
                if (e.target == _field || e.target == _chipsRow)
                {
                    e.preventDefault();
                    InnerElement.focus();
                }
            });

            _searchDoor.addEventListener("click", _ => Search());
            _askDoor.addEventListener("click",    _ => Ask());

            Layout(DoorLayout.SideBySide);
            Primary(PrimaryDoor.None);
            Refresh();
        }

        /// <inheritdoc />
        public override HTMLElement Render() => _host;

        /// <summary>Gets or sets the text in the box.</summary>
        public string Text
        {
            get => InnerElement.value;
            set
            {
                InnerElement.value = value ?? string.Empty;
                Refresh();
            }
        }

        /// <summary>Sets the tab index of the field.</summary>
        public int TabIndex
        {
            set => InnerElement.tabIndex = value;
        }

        /// <summary>The chips in the field, in the order they were added.</summary>
        public IReadOnlyList<Chip> Chips => _chips;

        /// <summary>The mentions among <see cref="Chips"/>.</summary>
        public Mention[] SelectedMentions => _chips.Where(c => c.Mention is object).Select(c => c.Mention).ToArray();

        /// <summary>The filter chips among <see cref="Chips"/>.</summary>
        public Chip[] SelectedFilters => _chips.Where(c => c.Filter is object).ToArray();

        /// <summary>
        /// The doors still open: both with no chips, otherwise the one every chip belongs to.
        /// </summary>
        public Door[] AvailableDoors
        {
            get
            {
                var search = _chips.All(c => c.Door == Door.Search);
                var ask    = _chips.All(c => c.Door == Door.Ask);

                if (search && ask) return new[] { Door.Search, Door.Ask };
                if (search)        return new[] { Door.Search };
                return new[] { Door.Ask };
            }
        }

        /// <summary>Raised with the trimmed text when Search is chosen. <see cref="Chips"/> says what it was narrowed to.</summary>
        public SearchAskBox OnSearch(Action<SearchAskBox, string> onSearch)
        {
            Searched += onSearch;
            return this;
        }

        /// <summary>Raised with the trimmed text when Ask is chosen. <see cref="SelectedMentions"/> says who was asked.</summary>
        public SearchAskBox OnAsk(Action<SearchAskBox, string> onAsk)
        {
            Asked += onAsk;
            return this;
        }

        /// <summary>Raised whenever a chip is added or removed.</summary>
        public SearchAskBox OnChipsChanged(Action<SearchAskBox> onChipsChanged)
        {
            ChipsChanged += onChipsChanged;
            return this;
        }

        /// <summary>Adds things to <c>@</c>.</summary>
        public SearchAskBox Mentions(params Mention[] mentions)
        {
            if (mentions is object) _mentions.AddRange(mentions.Where(m => m is object));
            return this;
        }

        /// <summary>
        /// Adds things to <c>@</c> that have to be fetched - the assistants a user may talk to, say. They are fetched the
        /// first time <c>@</c> is typed, and listed beside the ones added directly.
        /// </summary>
        public SearchAskBox Mentions(Func<Task<IEnumerable<Mention>>> load)
        {
            _loadMentions    = load;
            _mentionsLoading = null;
            return this;
        }

        /// <summary>Adds <c>key:value</c> filters, which belong to the Search door.</summary>
        public SearchAskBox Filters(params Filter[] filters)
        {
            if (filters is object) _filters.AddRange(filters.Where(f => f is object));
            return this;
        }

        /// <summary>Renames the headings of the suggestion list - for a translation.</summary>
        public SearchAskBox SuggestionGroupNames(string searchIn, string ask, string filters)
        {
            _searchGroupName = searchIn ?? _searchGroupName;
            _askGroupName    = ask      ?? _askGroupName;
            _filterGroupName = filters  ?? _filterGroupName;
            return this;
        }

        /// <summary>Adds a mention chip as though it had been picked, folding away the door it does not belong to.</summary>
        public SearchAskBox AddChip(Mention mention)
        {
            if (mention is object && CanAdd(mention.Door) && !_chips.Any(c => c.Mention == mention)) AddChipAndRender(new Chip(mention));
            return this;
        }

        /// <summary>Adds a filter chip as though it had been picked.</summary>
        public SearchAskBox AddChip(Filter filter, string value)
        {
            if (filter is object && !string.IsNullOrWhiteSpace(value) && CanAdd(Door.Search)) AddChipAndRender(new Chip(filter, value.Trim()));
            return this;
        }

        /// <summary>Removes every chip, opening both doors again.</summary>
        public SearchAskBox ClearChips()
        {
            if (_chips.Count == 0) return this;
            _chips.Clear();
            RenderChips();
            Refresh();
            ChipsChanged?.Invoke(this);
            return this;
        }

        /// <summary>Renames the two doors - for a translation, or a host whose actions have other names.</summary>
        public SearchAskBox DoorTexts(string search, string ask)
        {
            _searchWord.textContent = search;
            _askWord.textContent    = ask;
            return this;
        }

        /// <summary>Prints each door's key on it: Enter on Search, Ctrl+Enter (Cmd+Enter on a Mac) on Ask.</summary>
        public SearchAskBox ShowKeys(bool show = true)
        {
            _container.classList.toggle("tss-searchaskbox-show-keys", show);
            return this;
        }

        /// <summary>Places the doors. <see cref="DoorLayout.SideBySide"/> by default.</summary>
        public SearchAskBox Layout(DoorLayout layout)
        {
            _container.classList.remove("tss-searchaskbox-stacked", "tss-searchaskbox-side-by-side", "tss-searchaskbox-outside");

            switch (layout)
            {
                case DoorLayout.Stacked:    _container.classList.add("tss-searchaskbox-stacked");      break;
                case DoorLayout.SideBySide: _container.classList.add("tss-searchaskbox-side-by-side"); break;
                case DoorLayout.Outside:    _container.classList.add("tss-searchaskbox-outside");      break;
            }

            return this;
        }

        /// <summary>Chooses a door drawn filled at rest. <see cref="PrimaryDoor.None"/> by default: both doors are grey and fill with the primary colour under the pointer.</summary>
        public SearchAskBox Primary(PrimaryDoor primary)
        {
            _searchDoor.classList.toggle("tss-searchaskbox-primary", primary == PrimaryDoor.Search);
            _askDoor.classList.toggle("tss-searchaskbox-primary",    primary == PrimaryDoor.Ask);
            return this;
        }

        /// <summary>
        /// Keeps Ask disabled whatever is typed, with <paramref name="reason"/> as its tooltip - for a host with no
        /// model to answer. Search and the Enter key are unaffected, and assistants are no longer offered to <c>@</c>.
        /// </summary>
        public SearchAskBox AskEnabled(bool enabled, string reason = null)
        {
            _askAllowed        = enabled;
            _askDisabledReason = reason;
            Refresh();
            return this;
        }

        /// <summary>Moves the caret into the box.</summary>
        public SearchAskBox Focus()
        {
            DomObserver.WhenMounted(InnerElement, () => InnerElement.focus());
            return this;
        }

        /// <summary>Empties the box: the text and the chips.</summary>
        public SearchAskBox Clear()
        {
            Text = string.Empty;
            ClearChips();
            return this;
        }

        private string Trimmed => (InnerElement.value ?? string.Empty).Trim();

        private bool IsOpen(Door door) => AvailableDoors.Contains(door) && (door != Door.Ask || _askAllowed);

        private bool CanAdd(Door door) => IsOpen(door);

        private void Search()
        {
            var text = Trimmed;
            if (!IsOpen(Door.Search)) return;
            if (text.Length == 0 && _chips.Count == 0) return;
            HideSuggestions();
            Searched?.Invoke(this, text);
        }

        private void Ask()
        {
            var text = Trimmed;
            if (text.Length == 0 || !IsOpen(Door.Ask)) return;
            HideSuggestions();
            Asked?.Invoke(this, text);
        }

        private void OnKeyDown(KeyboardEvent ke)
        {
            //Enter also confirms a word in an input method (Japanese, Chinese, Korean); that Enter is the IME's.
            //The DOM declarations have no isComposing, so it is read straight off the event.
            if (Script.Write<bool>("{0}.isComposing === true", ke)) return;

            if (!_suggestionsPanel.hidden && _shown.Count > 0)
            {
                switch (ke.key)
                {
                    case "ArrowDown": ke.preventDefault(); Highlight((_highlighted + 1) % _shown.Count);                 return;
                    case "ArrowUp":   ke.preventDefault(); Highlight((_highlighted - 1 + _shown.Count) % _shown.Count); return;
                    case "Escape":    ke.preventDefault(); HideSuggestions();                                            return;
                    case "Tab":
                    case "Enter":
                        if (ke.key == "Enter" && (ke.shiftKey || ke.ctrlKey || ke.metaKey)) break;
                        ke.preventDefault();
                        _shown[Math.Max(0, _highlighted)].Pick();
                        return;
                }
            }

            //Backspace at the very start of the field takes the last chip back, as in any field of tags.
            if (ke.key == "Backspace" && _chips.Count > 0 && CaretAtStart())
            {
                ke.preventDefault();
                RemoveChip(_chips[_chips.Count - 1]);
                return;
            }

            //A filter typed out in full (ext:pdf) becomes a chip at the space that ends it.
            if (ke.key == " " && TryCommitTypedFilter())
            {
                ke.preventDefault();
                return;
            }

            if (ke.key != "Enter" || ke.shiftKey) return;

            ke.preventDefault();
            TryCommitTypedFilter();

            //Enter does what the box can still do: Search while it is open, otherwise Ask. Ctrl+Enter asks while Ask is open.
            if ((ke.ctrlKey || ke.metaKey) && IsOpen(Door.Ask)) Ask();
            else if (IsOpen(Door.Search))                        Search();
            else                                                 Ask();
        }

        private bool CaretAtStart() => SelectionStart() == 0 && SelectionEnd() == 0;

        private int SelectionStart() => Script.Write<int>("{0}.selectionStart || 0", InnerElement);

        private int SelectionEnd() => Script.Write<int>("{0}.selectionEnd || 0", InnerElement);

        // ---------------------------------------------------------------------------------------------------------
        // Suggestions
        // ---------------------------------------------------------------------------------------------------------

        //The word that ends at the caret: "@leg" asks for mentions, "ext:p" for a filter's values.
        private Token ReadToken()
        {
            var text  = InnerElement.value ?? string.Empty;
            var caret = Math.Min(SelectionStart(), text.Length);
            if (SelectionEnd() != caret) return null;

            var start = caret;
            while (start > 0 && !char.IsWhiteSpace(text[start - 1])) start--;

            var word = text.Substring(start, caret - start);
            if (word.Length == 0) return null;

            if (word[0] == '@') return new Token { Start = start, IsMention = true, Query = word.Substring(1) };

            var colon = word.IndexOf(':');
            if (colon <= 0) return null;

            var key    = word.Substring(0, colon);
            var filter = _filters.FirstOrDefault(f => f.Keys.Any(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase)));
            if (filter is null) return null;

            return new Token { Start = start, Filter = filter, Query = word.Substring(colon + 1) };
        }

        private void UpdateSuggestions()
        {
            var token = ReadToken();
            _token = token;

            if (token is null)
            {
                HideSuggestions();
                return;
            }

            var version = ++_suggestionsVersion;

            if (token.IsMention) ShowMentionSuggestionsAsync(token, version).FireAndForget();
            else                 ShowFilterValueSuggestionsAsync(token, version).FireAndForget();
        }

        private async Task ShowMentionSuggestionsAsync(Token token, int version)
        {
            var mentions = await AllMentionsAsync();
            if (version != _suggestionsVersion) return;

            var query = token.Query ?? string.Empty;
            var rows  = new List<Suggestion>();

            foreach (var mention in mentions)
            {
                if (!CanAdd(mention.Door) || _chips.Any(c => c.Mention == mention)) continue;

                var score = Score(query, mention.Name, mention.Id);
                if (score < 0) continue;

                var picked = mention;

                rows.Add(new Suggestion
                {
                    Score       = score,
                    Group       = mention.Group ?? (mention.Door == Door.Search ? _searchGroupName : _askGroupName),
                    Name        = mention.Name,
                    Description = mention.Description,
                    Icon        = mention.Icon,
                    Pick        = () => PickMention(token, picked)
                });
            }

            //The filters are listed too, so they can be found without knowing their keys.
            if (CanAdd(Door.Search))
            {
                foreach (var filter in _filters)
                {
                    var score = Score(query, filter.Name, filter.Keys);
                    if (score < 0) continue;

                    var picked = filter;

                    rows.Add(new Suggestion
                    {
                        Score       = score,
                        Group       = _filterGroupName,
                        Name        = filter.Name,
                        Description = filter.Keys[0] + ":",
                        Icon        = filter.Icon,
                        Pick        = () => StartFilter(token, picked)
                    });
                }
            }

            ShowSuggestions(rows);
        }

        private async Task ShowFilterValueSuggestionsAsync(Token token, int version)
        {
            if (!CanAdd(Door.Search))
            {
                HideSuggestions();
                return;
            }

            var values = await token.Filter.ValuesAsync();
            if (version != _suggestionsVersion) return;

            var query = token.Query ?? string.Empty;

            ShowSuggestions(values.Select(v => new { Value = v, Score = Score(query, v) })
                                  .Where(v => v.Score >= 0)
                                  .Select(x => x.Value)
                                  .Select(v => new Suggestion
                                   {
                                       Score = Score(query, v),
                                       Group = token.Filter.Name,
                                       Name  = v,
                                       Icon  = token.Filter.Icon,
                                       Pick  = () => PickFilterValue(token, token.Filter, v)
                                   })
                                  .ToList());
        }

        private async Task<List<Mention>> AllMentionsAsync()
        {
            if (_loadMentions is object)
            {
                _mentionsLoading ??= LoadMentionsAsync();
                await _mentionsLoading;
            }

            return _mentions.ToList();
        }

        private async Task LoadMentionsAsync()
        {
            var loaded = await _loadMentions();
            if (loaded is object) _mentions.AddRange(loaded.Where(m => m is object));
        }

        //How well a suggestion answers what was typed, lower first: the name starting with it, a word of the name or one
        //of its other names starting with it, the name containing it. -1 when it does not match at all.
        private static int Score(string query, string name, params string[] others)
        {
            if (string.IsNullOrEmpty(query)) return 0;

            var q = query.ToLowerInvariant();
            var n = (name ?? string.Empty).ToLowerInvariant();

            if (n.StartsWith(q)) return 0;
            if (n.Split(' ').Any(w => w.StartsWith(q))) return 1;
            if (others is object && others.Any(o => (o ?? string.Empty).ToLowerInvariant().StartsWith(q))) return 1;
            if (n.Contains(q)) return 2;
            return -1;
        }

        private void ShowSuggestions(List<Suggestion> rows)
        {
            //Best matches first; OrderBy is stable, so equal ones keep the host's order. Groups then follow their best
            //row, so the first row - the one Enter picks - is the best match there is.
            rows = rows.OrderBy(r => r.Score).Take(MaxSuggestions).ToList();

            var groups = rows.Select(r => r.Group).Distinct().ToList();
            _shown = groups.SelectMany(g => rows.Where(r => r.Group == g)).ToList();

            _suggestionsPanel.innerHTML = "";

            if (_shown.Count == 0)
            {
                HideSuggestions();
                return;
            }

            var index = 0;

            foreach (var group in groups)
            {
                var heading = Div(Att("tss-searchaskbox-suggestions-group", text: group));
                heading.setAttribute("role", "presentation");
                _suggestionsPanel.appendChild(heading);

                foreach (var row in _shown.Where(r => r.Group == group))
                {
                    var i      = index++;
                    var option = Div(Att("tss-searchaskbox-suggestion", id: _listId + "-" + i, role: "option"),
                                     I(row.Icon, cssClass: "tss-searchaskbox-suggestion-icon"),
                                     Div(Att("tss-searchaskbox-suggestion-text"),
                                         Span(Att("tss-searchaskbox-suggestion-name", text: row.Name)),
                                         Span(Att("tss-searchaskbox-suggestion-description", text: row.Description ?? string.Empty))));

                    option.addEventListener("mousemove", _ => Highlight(i));
                    option.addEventListener("click",     _ => row.Pick());

                    _suggestionsPanel.appendChild(option);
                }
            }

            if (_suggestionsPanel.parentElement is null)
            {
                document.body.appendChild(_suggestionsPanel);
                _suggestionsPanel.style.zIndex = Layers.PushLayer(_suggestionsPanel);
            }

            if (!_following)
            {
                _following = true;
                window.addEventListener("resize", _reposition);
                document.addEventListener("scroll", _reposition, true);
            }

            _suggestionsPanel.hidden = false;
            PlaceSuggestions();
            InnerElement.setAttribute("aria-expanded", "true");
            Highlight(0);
        }

        private void Highlight(int index)
        {
            _highlighted = index;

            var options = _suggestionsPanel.querySelectorAll(".tss-searchaskbox-suggestion");

            for (int i = 0; i < options.length; i++)
            {
                var option = options[i].As<HTMLElement>();
                var on     = i == index;

                option.classList.toggle("tss-searchaskbox-suggestion-highlighted", on);
                option.setAttribute("aria-selected", on ? "true" : "false");

                if (on)
                {
                    InnerElement.setAttribute("aria-activedescendant", option.id);
                    Script.Write("{0}.scrollIntoView({ block: 'nearest' })", option);
                }
            }
        }

        //Under the box and as wide as it, or above it when the window has no room below.
        private void PlaceSuggestions()
        {
            if (_suggestionsPanel.hidden) return;

            var box        = (DOMRect)_container.getBoundingClientRect();
            var roomBelow  = window.innerHeight - box.bottom;
            var listHeight = ((DOMRect)_suggestionsPanel.getBoundingClientRect()).height;

            _suggestionsPanel.style.left  = box.left + "px";
            _suggestionsPanel.style.width = box.width + "px";

            if (roomBelow < listHeight + 8 && box.top > roomBelow)
            {
                _suggestionsPanel.style.top    = "";
                _suggestionsPanel.style.bottom = (window.innerHeight - box.top + 4) + "px";
            }
            else
            {
                _suggestionsPanel.style.bottom = "";
                _suggestionsPanel.style.top    = (box.bottom + 4) + "px";
            }
        }

        private void HideSuggestions()
        {
            _suggestionsVersion++;
            _shown       = new List<Suggestion>();
            _highlighted = -1;
            _suggestionsPanel.hidden    = true;
            _suggestionsPanel.innerHTML = "";
            _suggestionsPanel.remove();

            if (_following)
            {
                _following = false;
                window.removeEventListener("resize", _reposition);
                document.removeEventListener("scroll", _reposition, true);
            }

            InnerElement.setAttribute("aria-expanded", "false");
            InnerElement.removeAttribute("aria-activedescendant");
        }

        //Takes the word being typed out of the field, leaving the caret where it began.
        private void RemoveToken(Token token)
        {
            var text  = InnerElement.value ?? string.Empty;
            var caret = Math.Min(SelectionStart(), text.Length);
            var start = Math.Min(token.Start, caret);

            InnerElement.value = text.Substring(0, start) + text.Substring(caret);
            Script.Write("{0}.setSelectionRange({1}, {1})", InnerElement, start);
        }

        private void PickMention(Token token, Mention mention)
        {
            RemoveToken(token);
            HideSuggestions();
            AddChip(mention);
            InnerElement.focus();
        }

        private void StartFilter(Token token, Filter filter)
        {
            //The filter's key replaces the word, and its values are listed straight away.
            var text  = InnerElement.value ?? string.Empty;
            var caret = Math.Min(SelectionStart(), text.Length);
            var key   = filter.Keys[0] + ":";

            InnerElement.value = text.Substring(0, token.Start) + key + text.Substring(caret);
            Script.Write("{0}.setSelectionRange({1}, {1})", InnerElement, token.Start + key.Length);
            InnerElement.focus();
            Refresh();
            UpdateSuggestions();
        }

        private void PickFilterValue(Token token, Filter filter, string value)
        {
            RemoveToken(token);
            HideSuggestions();
            AddChip(filter, value);
            InnerElement.focus();
        }

        //"ext:pdf" typed in full, with the caret right after it, becomes a chip.
        private bool TryCommitTypedFilter()
        {
            var token = ReadToken();
            if (token is null || token.Filter is null || string.IsNullOrWhiteSpace(token.Query) || !CanAdd(Door.Search)) return false;

            PickFilterValue(token, token.Filter, token.Query);
            return true;
        }

        // ---------------------------------------------------------------------------------------------------------
        // Chips
        // ---------------------------------------------------------------------------------------------------------

        private void AddChipAndRender(Chip chip)
        {
            _chips.Add(chip);
            RenderChips();
            Refresh();
            ChipsChanged?.Invoke(this);
        }

        private void RemoveChip(Chip chip)
        {
            if (!_chips.Remove(chip)) return;
            RenderChips();
            Refresh();
            ChipsChanged?.Invoke(this);
        }

        private void RenderChips()
        {
            _chipsRow.innerHTML = "";

            foreach (var chip in _chips)
            {
                var c      = chip;
                var remove = Button(Att("tss-searchaskbox-chip-remove", type: "button", ariaLabel: "Remove " + chip.Label, title: "Remove"),
                                    I(UIcons.CrossSmall));
                remove.tabIndex = -1;
                remove.addEventListener("mousedown", e => e.preventDefault());
                remove.addEventListener("click", _ =>
                {
                    RemoveChip(c);
                    InnerElement.focus();
                });

                var element = Span(Att("tss-searchaskbox-chip " + (chip.Door == Door.Ask ? "tss-searchaskbox-chip-ask" : "tss-searchaskbox-chip-search")),
                                   I(chip.Icon, cssClass: "tss-searchaskbox-chip-icon"),
                                   Span(Att("tss-searchaskbox-chip-label", text: chip.Label)),
                                   remove);

                _chipsRow.appendChild(element);
            }

            _container.classList.toggle("tss-searchaskbox-has-chips", _chips.Count > 0);
        }

        private void Refresh()
        {
            var hasText    = Trimmed.Length > 0;
            var searchOpen = AvailableDoors.Contains(Door.Search);
            var askOpen    = AvailableDoors.Contains(Door.Ask);

            //A door the chips leave no use for folds away (the stylesheet animates it); the other stays and Enter opens it.
            _searchDoor.classList.toggle("tss-searchaskbox-door-collapsed", !searchOpen);
            _askDoor.classList.toggle("tss-searchaskbox-door-collapsed",    !askOpen);
            _container.classList.toggle("tss-searchaskbox-only-search", searchOpen && !askOpen);
            _container.classList.toggle("tss-searchaskbox-only-ask",    askOpen && !searchOpen);

            if (searchOpen) _searchDoor.removeAttribute("aria-hidden"); else _searchDoor.setAttribute("aria-hidden", "true");
            if (askOpen)    _askDoor.removeAttribute("aria-hidden");    else _askDoor.setAttribute("aria-hidden", "true");

            _searchDoor.tabIndex = searchOpen ? 0 : -1;
            _askDoor.tabIndex    = askOpen    ? 0 : -1;

            _searchDoor.disabled = !searchOpen || (!hasText && _chips.Count == 0);
            _askDoor.disabled    = !askOpen || !hasText || !_askAllowed;
            _container.classList.toggle("tss-searchaskbox-has-text", hasText);

            //A disabled button raises no pointer events, so the reason is said by the element round it: the door
            //list. Enabled, the door says how to reach it from the keyboard.
            _askDoor.title = _askAllowed ? _askShortcutHint : string.Empty;
            if (_askAllowed) _doors.removeAttribute("title");
            else if (!string.IsNullOrEmpty(_askDisabledReason)) _doors.setAttribute("title", _askDisabledReason);

            ContentHeight.FitWhenRendered(InnerElement, Fit);
        }

        //Grows with the text up to MaxLines, then scrolls. Measured from the line height the stylesheet gives it, so a
        //theme with another type size gets its own six lines.
        private void Fit()
        {
            InnerElement.style.height = "auto";

            var style      = window.getComputedStyle(InnerElement);
            var lineHeight = ParsePx(style.lineHeight, 22);
            var padding    = ParsePx(style.paddingTop, 0) + ParsePx(style.paddingBottom, 0);
            var max        = lineHeight * MaxLines + padding;
            var needed     = InnerElement.scrollHeight;

            InnerElement.style.height    = Math.Min(needed, max) + "px";
            InnerElement.style.overflowY = needed > max ? "auto" : "hidden";
        }

        private static double ParsePx(string value, double fallback)
        {
            if (string.IsNullOrEmpty(value)) return fallback;
            return double.TryParse(value.Replace("px", ""), out var v) ? v : fallback;
        }

        private static bool IsApple()
        {
            var platform = (navigator.platform ?? string.Empty).ToLowerInvariant();
            return platform.Contains("mac") || platform.Contains("iphone") || platform.Contains("ipad");
        }
    }
}
