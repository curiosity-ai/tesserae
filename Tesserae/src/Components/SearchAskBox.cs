using System;
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

        private const int MaxLines = 6;

        private readonly HTMLDivElement    _container;
        private readonly HTMLButtonElement _searchDoor;
        private readonly HTMLButtonElement _askDoor;
        private readonly HTMLSpanElement   _searchWord;
        private readonly HTMLSpanElement   _askWord;

        private bool        _askAllowed = true;
        private string      _askDisabledReason;
        private string      _askShortcutHint;

        private event Action<SearchAskBox, string> Searched;
        private event Action<SearchAskBox, string> Asked;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="placeholder">The text shown while the box is empty; it is also the field's accessible name.</param>
        public SearchAskBox(string placeholder = null)
        {
            InnerElement = UI.TextArea(Att("tss-searchaskbox-input", placeholder: placeholder ?? string.Empty, ariaLabel: placeholder));
            InnerElement.rows = 1;

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

            _container = Div(Att("tss-searchaskbox"),
                             Div(Att("tss-searchaskbox-field"), InnerElement),
                             Div(Att("tss-searchaskbox-doors"), _searchDoor, _askDoor));

            _askShortcutHint = apple ? "⌘ Enter" : "Ctrl Enter";

            AttachFocus();
            AttachBlur();

            InnerElement.addEventListener("input", _ => Refresh());
            InnerElement.addEventListener("keydown", e =>
            {
                var ke = e.As<KeyboardEvent>();

                //Enter also confirms a word in an input method (Japanese, Chinese, Korean); that Enter is the IME's.
                //The DOM declarations have no isComposing, so it is read straight off the event.
                var isComposing = Script.Write<bool>("{0}.isComposing === true", ke);

                if (ke.key != "Enter" || ke.shiftKey || isComposing) return;

                ke.preventDefault();

                if (ke.ctrlKey || ke.metaKey) Ask();
                else                           Search();
            });

            _searchDoor.addEventListener("click", _ => Search());
            _askDoor.addEventListener("click",    _ => Ask());

            Layout(DoorLayout.SideBySide);
            Primary(PrimaryDoor.None);
            Refresh();
        }

        /// <inheritdoc />
        public override HTMLElement Render() => _container;

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

        /// <summary>Raised with the trimmed text when Search is chosen.</summary>
        public SearchAskBox OnSearch(Action<SearchAskBox, string> onSearch)
        {
            Searched += onSearch;
            return this;
        }

        /// <summary>Raised with the trimmed text when Ask is chosen.</summary>
        public SearchAskBox OnAsk(Action<SearchAskBox, string> onAsk)
        {
            Asked += onAsk;
            return this;
        }

        /// <summary>Sets the placeholder, which is also the field's accessible name.</summary>
        public SearchAskBox Placeholder(string placeholder)
        {
            InnerElement.placeholder = placeholder ?? string.Empty;
            InnerElement.setAttribute("aria-label", placeholder ?? string.Empty);
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
        /// model to answer. Search and the Enter key are unaffected.
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

        /// <summary>Empties the box.</summary>
        public SearchAskBox Clear()
        {
            Text = string.Empty;
            return this;
        }

        private string Trimmed => (InnerElement.value ?? string.Empty).Trim();

        private void Search()
        {
            var text = Trimmed;
            if (text.Length == 0) return;
            Searched?.Invoke(this, text);
        }

        private void Ask()
        {
            var text = Trimmed;
            if (text.Length == 0 || !_askAllowed) return;
            Asked?.Invoke(this, text);
        }

        private void Refresh()
        {
            var hasText = Trimmed.Length > 0;

            _searchDoor.disabled = !hasText;
            _askDoor.disabled    = !hasText || !_askAllowed;
            _container.classList.toggle("tss-searchaskbox-has-text", hasText);

            //A disabled button raises no pointer events, so the reason is said by the element round it: the door
            //list. Enabled, the door says how to reach it from the keyboard.
            _askDoor.title = _askAllowed ? _askShortcutHint : string.Empty;
            if (_askAllowed) _askDoor.parentElement?.removeAttribute("title");
            else if (!string.IsNullOrEmpty(_askDisabledReason)) _askDoor.parentElement?.setAttribute("title", _askDisabledReason);

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
