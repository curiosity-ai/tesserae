using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A group of <see cref="ContextCard"/>s behind one summary pill ("Added 5 items to context") that
    /// expands to list them and collapses back, styled like <see cref="ToolCall"/> / <see cref="ToolsUsed"/>
    /// so a transcript reads as one family of disclosures.
    /// <para>
    /// Expanded, the cards render as rows of a bordered list - full width, one divider between each, the
    /// remove button in the row rather than hovering over a corner. <see cref="Compact()"/> switches the
    /// whole group to a wrapping row of pills instead, truncated to <see cref="MaxVisible"/> with a
    /// "+N more" pill that reveals the rest.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.ContextCards")]
    public sealed class ContextCards : ComponentBase<ContextCards, HTMLElement>
    {
        private readonly HTMLElement       _header;
        private readonly HTMLElement       _iconContainer;
        private readonly HTMLElement       _textContainer;
        private readonly HTMLElement       _chevron;
        private readonly HTMLElement       _list;
        private readonly HTMLElement       _more;
        private readonly List<ContextCard> _cards = new List<ContextCard>();

        private string _summary;
        private string _moreFormat = "+{0} more";
        private string _lessText   = "Show less";
        private bool   _isExpanded;
        private bool   _isCompact;
        private int    _maxVisible = 5;
        private Action _onShowAll;

        private event Action<ContextCards> Toggled;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ContextCards(params ContextCard[] cards)
        {
            _iconContainer = Div(Att("tss-contextcards-icon"), I(UIcons.Layers));
            _textContainer = Div(Att("tss-contextcards-text"));
            _chevron       = I(UIcons.AngleDown, cssClass: "tss-contextcards-chevron");

            _header = Div(Att("tss-contextcards-header", role: "button", ariaLabel: "Toggle context"),
                          _iconContainer, _textContainer, _chevron);

            // The "+N more" pill only exists for the compact row; the header chevron is the affordance
            // everywhere else.
            _more = Button(Att("tss-contextcards-more", type: "button"));
            _more.addEventListener("click", ev =>
            {
                StopEvent(ev);
                ShowAll();
            });

            _list = Div(Att("tss-contextcards-list"), _more);

            InnerElement = Div(Att("tss-contextcards"), _header, _list);

            // A tap rather than a click, for the same reason ToolsUsed uses one: in a live transcript the
            // content around this pill re-renders and scrolls under the pointer, so the browser can drop
            // the click between press and release.
            Raw(_header).OnTapped(() => Toggle());

            _header.tabIndex = 0;

            _header.addEventListener("keydown", ev =>
            {
                var ke = ev.As<KeyboardEvent>();

                if (ke.key == "Enter" || ke.key == " ")
                {
                    StopEvent(ev);
                    Toggle();
                }
            });

            AddRange(cards);

            UpdateExpandedState();
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ContextCards(IEnumerable<ContextCard> cards) : this(ToArray(cards))
        {
        }

        /// <summary>
        /// Gets the cards in this group, in the order they were added.
        /// </summary>
        public IReadOnlyList<ContextCard> Cards => _cards;

        /// <summary>
        /// Gets the number of cards in this group.
        /// </summary>
        public int Count => _cards.Count;

        /// <summary>
        /// Returns a value indicating whether the group is expanded (in compact mode: whether the cards
        /// beyond <see cref="MaxVisible"/> are shown).
        /// </summary>
        public bool IsExpanded => _isExpanded;

        /// <summary>
        /// Adds a card to the group. Its remove button is wired to the group, so the (x) takes the card
        /// out of the list - a handler the caller registered on the card still runs.
        /// </summary>
        public ContextCards Add(ContextCard card)
        {
            if (card == null || _cards.Contains(card)) return this;

            _cards.Add(card);

            // Always before the "+N more" pill, which stays last in the row.
            _list.insertBefore(card.Render(), _more);

            card.OnRemove(c => Remove(c));

            UpdateSummary();
            UpdateVisibleCards();

            return this;
        }

        /// <summary>
        /// Adds the given cards to the group.
        /// </summary>
        public ContextCards AddRange(IEnumerable<ContextCard> cards)
        {
            if (cards == null) return this;

            foreach (var card in cards)
            {
                Add(card);
            }

            return this;
        }

        /// <summary>
        /// Removes a card from the group. Does nothing if the card isn't in it.
        /// </summary>
        public ContextCards Remove(ContextCard card)
        {
            if (card == null) return this;

            if (_cards.Remove(card))
            {
                TryRemoveChild(_list, card.Render());
                UpdateSummary();
                UpdateVisibleCards();
            }

            return this;
        }

        /// <summary>
        /// Removes every card from the group.
        /// </summary>
        public ContextCards Clear()
        {
            foreach (var card in _cards)
            {
                TryRemoveChild(_list, card.Render());
            }

            _cards.Clear();

            UpdateSummary();
            UpdateVisibleCards();

            return this;
        }

        /// <summary>
        /// Sets the summary shown on the header pill. Without one, the group summarises itself as
        /// "Added N items to context", updated as cards come and go.
        /// </summary>
        public ContextCards SetSummary(string summary)
        {
            _summary = summary;
            UpdateSummary();
            return this;
        }

        /// <summary>
        /// Sets the icon shown on the header pill (a stack of layers by default).
        /// </summary>
        public ContextCards SetIcon(UIcons icon)
        {
            ClearChildren(_iconContainer);
            _iconContainer.appendChild(I(icon));
            return this;
        }

        /// <summary>
        /// Sets the background color of the header's icon tile (any CSS color).
        /// </summary>
        public ContextCards IconBackground(string color)
        {
            _iconContainer.style.background = color ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the color of the glyph on the header's icon tile.
        /// </summary>
        public ContextCards IconForeground(string color)
        {
            _iconContainer.style.color = color ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Renders the group as a wrapping row of pills with no header, showing the first
        /// <see cref="MaxVisible"/> cards and a "+N more" pill for the rest. For a dense line of
        /// attachments where the list view would be too much.
        /// </summary>
        public ContextCards Compact(bool value = true)
        {
            _isCompact = value;
            InnerElement.UpdateClassIf(value, "tss-contextcards-compact");
            UpdateExpandedState();
            UpdateVisibleCards();
            return this;
        }

        /// <summary>
        /// Sets how many pills the compact row shows before the "+N more" pill takes over (5 by default).
        /// Ignored while the group isn't compact, where the list shows every card.
        /// </summary>
        public ContextCards MaxVisible(int count)
        {
            _maxVisible = count < 0 ? 0 : count;
            UpdateVisibleCards();
            return this;
        }

        /// <summary>
        /// Sets the wording of the pill standing in for the cards the compact row is hiding, as a format
        /// string whose <c>{0}</c> is how many that is ("+{0} more" by default), and the wording it takes
        /// once they are all shown ("Show less").
        /// </summary>
        public ContextCards MoreText(string moreFormat, string lessText = null)
        {
            _moreFormat = string.IsNullOrEmpty(moreFormat) ? "+{0} more" : moreFormat;
            _lessText   = string.IsNullOrEmpty(lessText) ? "Show less" : lessText;

            UpdateVisibleCards();

            return this;
        }

        /// <summary>
        /// Hands the "+N more" pill over to the given handler instead of revealing the hidden cards in
        /// place - for a host that would rather open the full list somewhere else (a panel, a search
        /// scoped to the context). Passing <c>null</c> restores the reveal-in-place behaviour.
        /// </summary>
        public ContextCards OnShowAll(Action handler)
        {
            _onShowAll = handler;
            return this;
        }

        /// <summary>
        /// Expands or collapses the group.
        /// </summary>
        public ContextCards Expanded(bool value = true) => value ? Expand() : Collapse();

        /// <summary>
        /// Expands the group.
        /// </summary>
        public ContextCards Expand()
        {
            if (_isExpanded) return this;

            _isExpanded = true;
            UpdateExpandedState();
            UpdateVisibleCards();
            Toggled?.Invoke(this);

            return this;
        }

        /// <summary>
        /// Collapses the group.
        /// </summary>
        public ContextCards Collapse()
        {
            if (!_isExpanded) return this;

            _isExpanded = false;
            UpdateExpandedState();
            UpdateVisibleCards();
            Toggled?.Invoke(this);

            return this;
        }

        /// <summary>
        /// Toggles the group between expanded and collapsed.
        /// </summary>
        public ContextCards Toggle() => _isExpanded ? Collapse() : Expand();

        /// <summary>
        /// Registers a callback invoked whenever the group is expanded or collapsed.
        /// </summary>
        public ContextCards OnToggle(Action<ContextCards> onToggle)
        {
            Toggled += onToggle;
            return this;
        }

        // What the "+N more" pill does: reveal the rest here, or hand over to a host that shows the full
        // context its own way.
        private void ShowAll()
        {
            if (_onShowAll != null)
            {
                _onShowAll();
                return;
            }

            Toggle();
        }

        private void UpdateSummary()
        {
            _textContainer.innerText = _summary ?? (_cards.Count == 1
                                                       ? "Added 1 item to context"
                                                       : $"Added {_cards.Count} items to context");

            // An empty group takes up no space at all, so it can sit permanently in a layout.
            InnerElement.UpdateClassIf(_cards.Count == 0, "tss-contextcards-empty");
        }

        private void UpdateExpandedState()
        {
            InnerElement.UpdateClassIf(_isExpanded, "tss-expanded");
            _header.setAttribute("aria-expanded", _isExpanded ? "true" : "false");

            // Compact mode keeps its row on screen at all times - what expanding changes there is how many
            // pills of it are shown, not whether the row exists.
            _list.style.display = _isCompact || _isExpanded ? "" : "none";
        }

        private void UpdateVisibleCards()
        {
            var hidden = 0;

            for (int i = 0; i < _cards.Count; i++)
            {
                // Only the compact row truncates: the list shows every card it holds.
                var isHidden = _isCompact && !_isExpanded && i >= _maxVisible;

                _cards[i].Render().UpdateClassIf(isHidden, "tss-contextcards-hidden");

                if (isHidden) hidden++;
            }

            var showMore = _isCompact && (hidden > 0 || (_isExpanded && _cards.Count > _maxVisible));

            // With an OnShowAll handler the pill hands over rather than revealing anything, so it never
            // turns into "Show less".
            _more.innerText = hidden > 0 ? string.Format(_moreFormat, hidden) : _lessText;
            _more.UpdateClassIf(!showMore || (_onShowAll != null && hidden == 0), "tss-contextcards-hidden");
        }

        private static ContextCard[] ToArray(IEnumerable<ContextCard> cards)
        {
            if (cards == null) return new ContextCard[0];

            var list = new List<ContextCard>(cards);
            return list.ToArray();
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
