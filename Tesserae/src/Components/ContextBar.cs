using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A row of small bubbles naming the context something is scoped to — the documents a chat can
    /// read from, the records a search is restricted to, the sources an answer cites.
    /// <para>
    /// A bubble carries an icon, a name ellipsized to a narrow fixed width, and an optional remove
    /// button. A trailing file extension is kept out of the ellipsis, so a bubble reads
    /// "Quarterly repo….pdf" rather than "Quarterly repor…".
    /// </para>
    /// <para>
    /// Only the first <see cref="MaxVisible"/> bubbles are rendered; the remainder collapse into a
    /// "+N more" button that hands over to <see cref="OnShowAll"/>, where a host typically opens the
    /// full list of what the context is. Bubbles behind that button are never added to the DOM, so a
    /// host can hand over everything it has without paying to render it.
    /// </para>
    /// <para>
    /// Meant to sit above a chat composer (see <see cref="OmniBox"/>) or under a reply
    /// (<see cref="ChatMessage.WithReferences(IEnumerable{IComponent})"/>), but it is a plain row and
    /// works anywhere.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.ContextBar")]
    public sealed class ContextBar : ComponentBase<ContextBar, HTMLElement>
    {
        private readonly List<Item>  _items = new List<Item>();
        private readonly HTMLElement _more;

        private int    _maxVisible = 3;
        private string _moreFormat = "+{0} more";
        private Action _onShowAll;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ContextBar(params Item[] items)
        {
            _more = Div(Att("tss-contextbar-more", role: "button"), Span(Att("tss-contextbar-more-text")));
            _more.tabIndex = 0;

            _more.addEventListener("click", e =>
            {
                StopEvent(e);
                _onShowAll?.Invoke();
            });

            _more.addEventListener("keydown", e =>
            {
                var ke = e.As<KeyboardEvent>();

                if (ke.key == "Enter" || ke.key == " ")
                {
                    StopEvent(e);
                    _onShowAll?.Invoke();
                }
            });

            InnerElement = Div(Att("tss-contextbar"));

            Items(items);
        }

        /// <summary>
        /// Gets the number of bubbles in the bar, including the ones collapsed behind "+N more".
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// Returns a value indicating whether the bar has no bubbles, in which case it renders nothing
        /// and takes up no space.
        /// </summary>
        public bool IsEmpty => _items.Count == 0;

        /// <summary>
        /// Sets the bubbles the bar shows, replacing any it already had.
        /// </summary>
        public ContextBar Items(params Item[] items)
        {
            _items.Clear();

            if (items is object) _items.AddRange(items.Where(i => i is object));

            Rebuild();
            return this;
        }

        /// <summary>
        /// Appends a bubble to the bar.
        /// </summary>
        public ContextBar Add(Item item)
        {
            if (item is null) return this;

            _items.Add(item);
            Rebuild();
            return this;
        }

        /// <summary>
        /// Drops a bubble from the bar. Removing a bubble the bar doesn't have does nothing.
        /// </summary>
        public ContextBar Remove(Item item)
        {
            if (item is null || !_items.Remove(item)) return this;

            Rebuild();
            return this;
        }

        /// <summary>
        /// Drops every bubble, leaving the bar empty (and invisible) until the next one is added.
        /// </summary>
        public ContextBar Clear()
        {
            _items.Clear();
            Rebuild();
            return this;
        }

        /// <summary>
        /// Configures how many bubbles are rendered before the rest collapse behind "+N more".
        /// Three by default.
        /// </summary>
        public ContextBar MaxVisible(int count)
        {
            _maxVisible = count < 0 ? 0 : count;
            Rebuild();
            return this;
        }

        /// <summary>
        /// Configures the text of the button standing in for the collapsed bubbles, as a format string
        /// whose <c>{0}</c> is the number of bubbles it hides. <c>"+{0} more"</c> by default.
        /// </summary>
        public ContextBar MoreText(string format)
        {
            _moreFormat = string.IsNullOrEmpty(format) ? "+{0} more" : format;
            Rebuild();
            return this;
        }

        /// <summary>
        /// Configures what happens when the "+N more" button is activated — typically opening the full
        /// list of what the context is. Without a handler the button still reports how many bubbles are
        /// collapsed, but does not respond to clicks.
        /// </summary>
        public ContextBar OnShowAll(Action handler)
        {
            _onShowAll = handler;
            Rebuild();
            return this;
        }

        private void Rebuild()
        {
            InnerElement.innerHTML = string.Empty;
            InnerElement.UpdateClassIf(_items.Count == 0, "tss-contextbar-empty");

            var visible = Math.Min(_items.Count, _maxVisible);

            for (int i = 0; i < visible; i++)
            {
                InnerElement.appendChild(_items[i].Render());
            }

            var hidden = _items.Count - visible;

            if (hidden <= 0) return;

            _more.firstElementChild.textContent = string.Format(_moreFormat, hidden);
            _more.UpdateClassIf(_onShowAll is null, "tss-contextbar-more-static");
            InnerElement.appendChild(_more);
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;

        /// <summary>
        /// One context bubble: an icon, a name, and an optional remove button.
        /// </summary>
        public sealed class Item : IComponent
        {
            // A longer tail than this isn't a file extension worth keeping out of the ellipsis.
            private const int maxExtensionLength = 7;

            private readonly HTMLElement _root;
            private readonly HTMLElement _icon;
            private readonly HTMLElement _name;
            private readonly HTMLElement _extension;

            private readonly bool _keepExtensionVisible;

            private string       _stem;
            private bool         _waitingForMount;
            private HTMLElement  _remove;
            private Action<Item> _onClick;
            private Action<Item> _onRemove;

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            /// <param name="name">Text shown on the bubble, e.g. a file name or a record's title.</param>
            /// <param name="icon">Icon shown in front of the name.</param>
            /// <param name="keepExtensionVisible">
            /// Whether a trailing file extension is held out of the ellipsis so it stays readable when
            /// the name is too long for the bubble. On by default.
            /// </param>
            public Item(string name, UIcons icon = UIcons.File, bool keepExtensionVisible = true)
            {
                _keepExtensionVisible = keepExtensionVisible;

                _icon      = Span(Att("tss-contextbar-item-icon"), I(icon));
                _name      = Span(Att("tss-contextbar-item-name"));
                _extension = Span(Att("tss-contextbar-item-extension"));

                _root = Div(Att("tss-contextbar-item"), _icon, _name, _extension);

                SetName(name);
            }

            /// <summary>
            /// Gets the name currently shown on the bubble.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Gets or sets an arbitrary payload associated with this bubble.
            /// </summary>
            public object Tag { get; set; }

            /// <summary>
            /// Writes a new name into the bubble already on screen, re-splitting the file extension.
            /// </summary>
            public Item SetName(string name)
            {
                Name = name ?? string.Empty;

                var (stem, extension) = SplitExtension(Name, _keepExtensionVisible);

                _stem                    = stem;
                _name.textContent        = stem;
                _extension.textContent   = extension ?? string.Empty;
                _extension.style.display = string.IsNullOrEmpty(extension) ? "none" : "";

                FitNameWhenMeasurable();

                return this;
            }

            /// <summary>
            /// Replaces the bubble's icon.
            /// </summary>
            public Item SetIcon(UIcons icon)
            {
                _icon.innerHTML = string.Empty;
                _icon.appendChild(I(icon));
                return this;
            }

            /// <summary>
            /// Colors the bubble's icon, e.g. to follow a file type.
            /// </summary>
            public Item IconColor(string color)
            {
                _icon.style.color = color ?? "";
                return this;
            }

            /// <summary>
            /// Configures the width the name is ellipsized at (80px by default, set in CSS). The
            /// extension, when kept visible, sits outside this width.
            /// </summary>
            public Item MaxNameWidth(UnitSize size)
            {
                _name.style.maxWidth = size is null ? "" : size.ToString();
                FitNameWhenMeasurable();
                return this;
            }

            /// <summary>
            /// Configures what happens when the bubble is activated — typically opening whatever the
            /// bubble names. A bubble without a handler is not interactive.
            /// </summary>
            public Item OnClick(Action<Item> handler)
            {
                if (_onClick is null && handler is object)
                {
                    _root.tabIndex = 0;
                    _root.setAttribute("role", "button");

                    _root.addEventListener("click", _ => _onClick?.Invoke(this));

                    _root.addEventListener("keydown", e =>
                    {
                        var ke = e.As<KeyboardEvent>();

                        if (ke.key == "Enter" || ke.key == " ")
                        {
                            StopEvent(e);
                            _onClick?.Invoke(this);
                        }
                    });
                }

                _onClick = handler;
                _root.UpdateClassIf(handler is object, "tss-contextbar-item-clickable");

                return this;
            }

            /// <summary>
            /// Adds a remove button to the bubble and configures what happens when it is activated.
            /// Activating it never also activates <see cref="OnClick"/>.
            /// </summary>
            public Item OnRemove(Action<Item> handler, string tooltip = "Remove")
            {
                _onRemove = handler;

                if (handler is null)
                {
                    if (_remove is object) _remove.style.display = "none";
                    return this;
                }

                if (_remove is null)
                {
                    _remove          = Div(Att("tss-contextbar-item-remove", role: "button"), I(UIcons.CrossSmall));
                    _remove.tabIndex = 0;

                    _remove.addEventListener("click", e =>
                    {
                        StopEvent(e);
                        _onRemove?.Invoke(this);
                    });

                    _remove.addEventListener("keydown", e =>
                    {
                        var ke = e.As<KeyboardEvent>();

                        if (ke.key == "Enter" || ke.key == " ")
                        {
                            StopEvent(e);
                            _onRemove?.Invoke(this);
                        }
                    });

                    _root.appendChild(_remove);
                }

                _remove.style.display = "";
                _remove.setAttribute("title", tooltip ?? string.Empty);
                _remove.setAttribute("aria-label", tooltip ?? string.Empty);

                return this;
            }

            /// <summary>
            /// Renders the component's root HTML element.
            /// </summary>
            public HTMLElement Render() => _root;

            // CSS alone leaves the tail of the (fixed-width) name box unused, which shows up as a gap
            // between the ellipsis and the extension - "Q3 rev... .xlsx". Truncating the text ourselves
            // puts the ellipsis right where the name stops, so the two read as one file name and the
            // bubble hugs its content. `text-overflow: ellipsis` still covers us until this can measure.
            private void FitNameWhenMeasurable()
            {
                if (_name.isConnected)
                {
                    FitName();
                    return;
                }

                if (_waitingForMount) return;

                _waitingForMount = true;

                DomObserver.WhenMounted(_root, () =>
                {
                    _waitingForMount = false;
                    FitName();
                });
            }

            private void FitName()
            {
                _name.textContent = _stem ?? string.Empty;

                var limit = _name.clientWidth;

                if (limit <= 0 || _name.scrollWidth <= limit) return;

                // Longest prefix that still fits once the ellipsis is appended.
                var low  = 0;
                var high = _stem.Length;

                while (low < high)
                {
                    var middle = (low + high + 1) / 2;

                    _name.textContent = _stem.Substring(0, middle) + "\u2026";

                    if (_name.scrollWidth <= limit) low = middle;
                    else                            high = middle - 1;
                }

                // Trimmed so a cut landing on a space doesn't read as "Q3 revenue ….xlsx".
                var kept = low <= 0 ? string.Empty : _stem.Substring(0, low).TrimEnd();

                _name.textContent = kept + "\u2026";
            }

            private static (string name, string extension) SplitExtension(string label, bool keepExtensionVisible)
            {
                if (!keepExtensionVisible || string.IsNullOrEmpty(label)) return (label, null);

                var dot = label.LastIndexOf('.');

                if (dot <= 0 || dot == label.Length - 1) return (label, null);

                var extension = label.Substring(dot);

                if (extension.Length > maxExtensionLength || extension.Contains(" ")) return (label, null);

                return (label.Substring(0, dot), extension);
            }
        }
    }
}
