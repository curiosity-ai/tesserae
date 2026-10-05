using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using Transpose.Core;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A card offering actions about one object - a company, a contract, a dataset: what to do next with it,
    /// or what to ask about it.
    /// <para>
    /// It is split vertically: on the left the object's identity (an icon tile, a label, a second line,
    /// an optional detail line and a few key/value facts), on the right the actions, each one a row
    /// drawn like a <see cref="ToolCall"/> with a small icon saying what kind of action it is. Activating
    /// a row calls <see cref="OnAction(Action{ActionCard{TData}, Item})"/>, which is where the host acts
    /// on it - and the <see cref="Item.Data"/> it carries, typed as <c>TData</c>, is what it acts with.
    /// </para>
    /// <para>
    /// The card fills the width it is given. Below about 520px (a phone, a side panel) it stacks itself -
    /// the identity becomes a header strip and the actions wrap onto several lines - by a container
    /// query on its own width, so nothing has to tell it where it is. <see cref="Compact(bool)"/> turns
    /// it into one wrapping line of an identity chip followed by action pills, and
    /// <see cref="ActionCardGroup{TData}"/> stacks several cards into one.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.ActionCardT")]
    public sealed class ActionCard<TData> : ComponentBase<ActionCard<TData>, HTMLElement>
    {
        /// <summary>
        /// One action offered by a <see cref="ActionCard{TData}"/> card.
        /// </summary>
        [Transpose.Name("tss.ActionCardT.Item")]
        public sealed class Item
        {
            internal HTMLButtonElement Row;
            internal HTMLElement       IconContainer;
            internal HTMLElement       TextContainer;

            internal Item(string text, UIcons icon, UIconsWeight weight)
            {
                Text   = text ?? string.Empty;
                Icon   = icon;
                Weight = weight;
            }

            /// <summary>
            /// Gets the text of the action.
            /// </summary>
            public string Text { get; internal set; }

            /// <summary>
            /// Gets the icon saying what kind of action this is.
            /// </summary>
            public UIcons Icon { get; internal set; }

            /// <summary>
            /// Gets the weight the icon is drawn in.
            /// </summary>
            public UIconsWeight Weight { get; internal set; }

            /// <summary>
            /// Gets or sets the data behind the action - the prompt to send when it differs from the text
            /// shown, a query, an id - so the handler can act on it without a lookup or a cast.
            /// </summary>
            public TData Data { get; set; }
        }

        private const string Empty = "tss-actioncard-empty";

        private readonly HTMLElement    _card;
        private readonly HTMLElement    _identity;
        private readonly HTMLElement    _iconContainer;
        private readonly HTMLElement    _labelContainer;
        private readonly HTMLElement    _subLabelContainer;
        private readonly HTMLElement    _detailContainer;
        private readonly HTMLElement    _facts;
        private readonly HTMLElement    _body;
        private readonly HTMLElement    _title;
        private readonly HTMLElement    _list;
        private readonly HTMLElement    _skeleton;
        private readonly HTMLElement    _error;
        private readonly HTMLElement    _errorText;
        private readonly HTMLButtonElement _retry;
        private readonly HTMLButtonElement _more;
        private readonly List<Item> _actions = new List<Item>();

        private string _label;
        private string _subLabel;
        private string _moreFormat       = "Show {0} more";
        private int    _maxVisible       = int.MaxValue;
        private bool   _showAll;
        private bool   _isLoading;
        private Action _onRetry;

        private event Action<ActionCard<TData>, Item> ActionInvoked;

        /// <summary>
        /// Initializes a new instance of this class for the object with the given label and icon.
        /// </summary>
        public ActionCard(string label, UIcons icon = UIcons.Cube, UIconsWeight weight = UIconsWeight.Regular)
            : this()
        {
            SetLabel(label);
            SetIcon(icon, weight);
        }

        /// <summary>
        /// Initializes a new instance of this class for the object with the given label, showing an
        /// arbitrary component on the icon tile - an <see cref="Icon"/> with its own color, an emoji, an
        /// <see cref="Image"/>.
        /// </summary>
        public ActionCard(string label, IComponent iconOrImage)
            : this()
        {
            SetLabel(label);
            SetIcon(iconOrImage);
        }

        private ActionCard()
        {
            _iconContainer     = Div(Att("tss-actioncard-icon"));
            _labelContainer    = Div(Att("tss-actioncard-label"));
            _subLabelContainer = Div(Att("tss-actioncard-sublabel"));
            _detailContainer   = Div(Att("tss-actioncard-detail"));
            _facts             = Div(Att("tss-actioncard-facts"));

            var names = Div(Att("tss-actioncard-names"), _labelContainer, _subLabelContainer);

            _identity = Div(Att("tss-actioncard-identity"), _iconContainer, names, _facts, _detailContainer);

            _title    = Div(Att("tss-actioncard-title"));
            _list     = Div(Att("tss-actioncard-list"));
            _skeleton = Div(Att("tss-actioncard-skeleton"));

            _more = Button(Att("tss-actioncard-more", type: "button"));
            _more.addEventListener("click", _ => ShowAll());

            _errorText = Span(Att("tss-actioncard-error-text"));
            _retry     = Button(Att("tss-actioncard-more", type: "button", text: "Retry"));
            _retry.addEventListener("click", _ => _onRetry?.Invoke());
            _error     = Div(Att("tss-actioncard-error"), I(UIcons.TriangleWarning), _errorText, _retry);

            _body = Div(Att("tss-actioncard-body"), _title, _list, _more, _skeleton, _error);

            _card = Div(Att("tss-actioncard-card"), _identity, _body);

            // The root is only the size container: a container query styles descendants of the element
            // it measures, never that element itself, so the card the stacked layout restyles sits inside.
            InnerElement = Div(Att("tss-actioncard"), _card);

            SetLabel(null);
            SetSubLabel(null);
            SetDetail(null);
            SetTitle(null);
            UpdateFacts();
            SetError(null);
        }

        /// <summary>
        /// Gets the label naming the object.
        /// </summary>
        public string Label => _label;

        /// <summary>
        /// Gets the secondary line below the label, or null when there is none.
        /// </summary>
        public string SubLabel => _subLabel;

        /// <summary>
        /// Gets the actions the card offers, in the order they are shown.
        /// </summary>
        public IReadOnlyList<Item> Actions => _actions;

        /// <summary>
        /// Returns a value indicating whether the card is showing its loading placeholders.
        /// </summary>
        public bool IsLoading => _isLoading;

        /// <summary>
        /// Gets or sets an arbitrary payload for the card - the record the actions are about.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Sets the label naming the object. It wraps rather than being cut, since the identity column is
        /// narrow and the name is the one thing on it that has to be read in full.
        /// </summary>
        public ActionCard<TData> SetLabel(string label)
        {
            _label = label ?? string.Empty;
            _labelContainer.textContent = _label;
            return this;
        }

        /// <summary>
        /// Sets the line below the label - the kind of object and an id ("Company · ACC-20931"). A null or
        /// empty value hides it.
        /// </summary>
        public ActionCard<TData> SetSubLabel(string subLabel)
        {
            _subLabel = subLabel;
            _subLabelContainer.textContent = subLabel ?? string.Empty;
            _subLabelContainer.UpdateClassIf(string.IsNullOrEmpty(subLabel), Empty);
            return this;
        }

        /// <summary>
        /// Renders the line below the label in the monospace font, for an id, a path or a table name - the
        /// treatment <see cref="ContextCard.MonospaceSubLabel(bool)"/> gives it.
        /// </summary>
        public ActionCard<TData> MonospaceSubLabel(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-actioncard-mono");
            return this;
        }

        /// <summary>
        /// Sets a quiet line at the foot of the identity column ("Hamburg · Key account"). A null or empty
        /// value hides it.
        /// </summary>
        public ActionCard<TData> SetDetail(string detail)
        {
            _detailContainer.textContent = detail ?? string.Empty;
            _detailContainer.UpdateClassIf(string.IsNullOrEmpty(detail), Empty);
            return this;
        }

        /// <summary>
        /// Adds a key/value fact to the identity column ("Renews" / "2027-03-31"), for an object whose
        /// actions make more sense with a few of its numbers beside them.
        /// </summary>
        public ActionCard<TData> AddFact(string key, string value, bool monospace = false)
        {
            var valueElement = Span(Att("tss-actioncard-fact-value", text: value ?? string.Empty));
            valueElement.UpdateClassIf(monospace, "tss-actioncard-fact-mono");

            return AddFactElement(key, valueElement);
        }

        /// <summary>
        /// Adds a key/value fact whose value is an arbitrary component - a <see cref="Badge"/> tag, a
        /// <see cref="Link"/>, an <see cref="Icon"/> with text - in place of plain text.
        /// </summary>
        public ActionCard<TData> AddFact(string key, IComponent value)
        {
            var valueElement = Span(Att("tss-actioncard-fact-value"));
            if (value != null) valueElement.appendChild(value.Render());

            return AddFactElement(key, valueElement);
        }

        private ActionCard<TData> AddFactElement(string key, HTMLElement valueElement)
        {
            _facts.appendChild(Span(Att("tss-actioncard-fact-key", text: key ?? string.Empty)));
            _facts.appendChild(valueElement);

            UpdateFacts();
            return this;
        }

        /// <summary>
        /// Removes every fact added with <see cref="AddFact(string, string, bool)"/> or <see cref="AddFact(string, IComponent)"/>.
        /// </summary>
        public ActionCard<TData> ClearFacts()
        {
            ClearChildren(_facts);
            UpdateFacts();
            return this;
        }

        /// <summary>
        /// Sets the small heading above the actions ("Ask about this company"). A null or empty value
        /// hides it.
        /// </summary>
        public ActionCard<TData> SetTitle(string title)
        {
            _title.textContent = title ?? string.Empty;
            _title.UpdateClassIf(string.IsNullOrEmpty(title), Empty);
            return this;
        }

        /// <summary>
        /// Sets the icon shown on the tile.
        /// </summary>
        public ActionCard<TData> SetIcon(UIcons icon, UIconsWeight weight = UIconsWeight.Regular)
        {
            ClearChildren(_iconContainer);
            _iconContainer.classList.remove("tss-actioncard-icon-image");
            _iconContainer.appendChild(I(icon, weight));
            return this;
        }

        /// <summary>
        /// Sets an arbitrary component on the icon tile. A null value empties the tile.
        /// </summary>
        public ActionCard<TData> SetIcon(IComponent iconOrImage)
        {
            ClearChildren(_iconContainer);
            _iconContainer.classList.remove("tss-actioncard-icon-image");
            if (iconOrImage != null) _iconContainer.appendChild(iconOrImage.Render());
            return this;
        }

        /// <summary>
        /// Fills the tile with a thumbnail (cropped to cover it) - a logo, a photo, a favicon.
        /// </summary>
        public ActionCard<TData> SetImage(string url)
        {
            ClearChildren(_iconContainer);

            var hasImage = !string.IsNullOrEmpty(url);

            if (hasImage) _iconContainer.appendChild(Image(Att("tss-actioncard-image", src: url)));

            _iconContainer.UpdateClassIf(hasImage, "tss-actioncard-icon-image");
            return this;
        }

        /// <summary>
        /// Sets the background color of the icon tile (any CSS color).
        /// </summary>
        public ActionCard<TData> IconBackground(string color)
        {
            _iconContainer.style.background = color ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the color of the glyph on the icon tile.
        /// </summary>
        public ActionCard<TData> IconForeground(string color)
        {
            _iconContainer.style.color = color ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Tints the icon tile with the given color: a wash of it behind the glyph, the glyph in full
        /// strength. The quieter alternative to a saturated tile.
        /// </summary>
        public ActionCard<TData> IconTint(string color, int percent = 14)
        {
            if (string.IsNullOrEmpty(color)) return this;

            _iconContainer.style.background = $"color-mix(in srgb, {color} {percent}%, transparent)";
            _iconContainer.style.color      = color;
            return this;
        }

        /// <summary>
        /// Drops the colored square, letting the glyph or image sit on the card.
        /// </summary>
        public ActionCard<TData> NoIconBackground()
        {
            _iconContainer.classList.add("tss-actioncard-icon-nobackground");
            return this;
        }

        /// <summary>
        /// Adds an action. The icon says what kind of action it is - a search, a trend, the people
        /// involved, a document - the way a <see cref="ToolCall"/>'s icon names its tool.
        /// </summary>
        public ActionCard<TData> AddAction(string text, UIcons icon = UIcons.Bolt, UIconsWeight weight = UIconsWeight.Regular, TData data = default)
        {
            var action = new Item(text, icon, weight) { Data = data };

            action.IconContainer = Span(Att("tss-actioncard-action-icon"), I(icon, weight));
            action.TextContainer  = Span(Att("tss-actioncard-action-text",  text: action.Text));

            // The row is the button itself, so it is a tab stop and Enter or Space activates it with no help.
            action.Row = Button(Att("tss-actioncard-action", type: "button", title: action.Text),
                action.IconContainer,
                action.TextContainer,
                I(UIcons.ArrowUpRight, cssClass: "tss-actioncard-action-go"));

            action.Row.addEventListener("click", _ => Act(action));

            _actions.Add(action);
            _list.appendChild(action.Row);

            UpdateState();
            return this;
        }

        /// <summary>
        /// Adds several actions with the default icon.
        /// </summary>
        public ActionCard<TData> AddActions(params string[] actions)
        {
            foreach (var a in actions) AddAction(a);
            return this;
        }

        /// <summary>
        /// Removes a action.
        /// </summary>
        public ActionCard<TData> RemoveAction(Item action)
        {
            if (action == null || !_actions.Remove(action)) return this;

            _list.removeChild(action.Row);

            UpdateState();
            return this;
        }

        /// <summary>
        /// Removes every action, ready for a fresh set.
        /// </summary>
        public ActionCard<TData> ClearActions()
        {
            _actions.Clear();
            ClearChildren(_list);
            _showAll = false;

            UpdateState();
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when an action is clicked (or activated from the keyboard). This
        /// is where the host acts on it, usually from <c>action.Data</c>.
        /// </summary>
        public ActionCard<TData> OnAction(Action<ActionCard<TData>, Item> onAction)
        {
            ActionInvoked += onAction;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked with the text of an action when it is clicked.
        /// </summary>
        public ActionCard<TData> OnAction(Action<string> onAction) => OnAction((_, a) => onAction?.Invoke(a.Text));

        /// <summary>
        /// Shows only the first <paramref name="count"/> actions, with a "Show N more" button for the rest.
        /// </summary>
        public ActionCard<TData> MaxVisible(int count)
        {
            _maxVisible = Math.Max(1, count);
            UpdateState();
            return this;
        }

        /// <summary>
        /// Shows every action, as clicking "Show N more" does.
        /// </summary>
        public ActionCard<TData> ShowAll()
        {
            _showAll = true;
            UpdateState();
            return this;
        }

        /// <summary>
        /// Sets the texts of the "Show N more" button (with <c>{0}</c> for the count) and of the retry
        /// button, for localisation.
        /// </summary>
        public ActionCard<TData> SetTexts(string moreFormat = null, string retryText = null)
        {
            if (moreFormat != null) _moreFormat = moreFormat;
            if (retryText != null) _retry.textContent = retryText;

            UpdateState();
            return this;
        }

        /// <summary>
        /// Shows placeholder rows while the actions are still being generated, in place of the list.
        /// The identity column is drawn as usual, since the object is already known.
        /// </summary>
        public ActionCard<TData> Loading(bool value = true, int placeholders = 3)
        {
            _isLoading = value;

            ClearChildren(_skeleton);

            if (value)
            {
                // Uneven widths read as lines of text still to come rather than as empty boxes.
                var widths = new[] { 82, 64, 73, 58, 70 };

                for (var i = 0; i < Math.Max(1, placeholders); i++)
                {
                    var line = Div(Att("tss-actioncard-skeleton-line"));
                    line.style.width = widths[i % widths.Length] + "%";
                    _skeleton.appendChild(line);
                }

                _card.setAttribute("aria-busy", "true");
            }
            else
            {
                _card.removeAttribute("aria-busy");
            }

            UpdateState();
            return this;
        }

        /// <summary>
        /// Shows an error in place of the actions - generating them failed - with a Retry button when a
        /// handler is given. A null or empty message clears it.
        /// </summary>
        public ActionCard<TData> SetError(string message, Action onRetry = null)
        {
            _errorText.textContent = message ?? string.Empty;
            _onRetry               = onRetry;

            _error.UpdateClassIf(string.IsNullOrEmpty(message), Empty);
            _retry.UpdateClassIf(onRetry == null, Empty);

            if (!string.IsNullOrEmpty(message)) _isLoading = false;

            UpdateState();
            return this;
        }

        /// <summary>
        /// Clears the error set with <see cref="SetError(string, Action)"/>.
        /// </summary>
        public ActionCard<TData> ClearError() => SetError(null);

        /// <summary>
        /// Draws the card as one wrapping line: the object as a chip, then the actions as pills. For a
        /// transcript where a full card under every answer would be too much.
        /// </summary>
        public ActionCard<TData> Compact(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-actioncard-compact");
            return this;
        }

        /// <summary>
        /// Forces the stacked layout - identity on top, actions below - whatever the width. The card
        /// already stacks itself below about 520px; this is for a host that wants it everywhere.
        /// </summary>
        public ActionCard<TData> Stacked(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-actioncard-stacked");
            return this;
        }

        private void Act(Item action)
        {
            ActionInvoked?.Invoke(this, action);
        }

        private void UpdateFacts()
        {
            _facts.UpdateClassIf(_facts.childElementCount == 0, Empty);
        }

        private void UpdateState()
        {
            var hasError = !_error.classList.contains(Empty);

            _skeleton.UpdateClassIf(!_isLoading || hasError, Empty);
            _list.UpdateClassIf(_isLoading || hasError, Empty);

            var visible = _showAll ? int.MaxValue : _maxVisible;
            var hidden  = 0;

            for (var i = 0; i < _actions.Count; i++)
            {
                var isHidden = i >= visible;
                _actions[i].Row.UpdateClassIf(isHidden, Empty);
                if (isHidden) hidden++;
            }

            _more.textContent = string.Format(_moreFormat, hidden);
            _more.UpdateClassIf(hidden == 0 || _isLoading || hasError, Empty);
        }

        /// <inheritdoc />
        public override HTMLElement Render() => InnerElement;
    }
}
