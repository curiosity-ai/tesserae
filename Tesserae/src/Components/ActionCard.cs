using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using Transpose.Core;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A card offering follow-up questions about one object - a company, a contract, a dataset - meant to
    /// be embedded in a chat transcript under the answer that mentioned it.
    /// <para>
    /// It is split vertically: on the left the object's identity (an icon tile, a label, a second line,
    /// an optional detail line and a few key/value facts), on the right the questions, each one a row
    /// drawn like a <see cref="ToolCall"/> with a small icon saying what kind of question it is. Clicking
    /// a question calls <see cref="OnAsk(Action{ActionCard{TData}, Item})"/>, which is where the host
    /// sends it as the next message, and marks it as asked.
    /// </para>
    /// <para>
    /// The card fills the width it is given. Below about 520px (a phone, a side panel) it stacks itself -
    /// the identity becomes a header strip and the questions wrap onto several lines - by a container
    /// query on its own width, so nothing has to tell it where it is. <see cref="Compact(bool)"/> turns
    /// it into one wrapping line of an identity chip followed by question pills, and
    /// <see cref="ActionCardGroup"/> stacks several cards into one.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.ActionCardT")]
    public sealed class ActionCard<TData> : ComponentBase<ActionCard<TData>, HTMLElement>
    {
        /// <summary>
        /// One question offered by a <see cref="ActionCard{TData}"/> card.
        /// </summary>
        [Transpose.Name("tss.ActionCardT.Item")]
        public sealed class Item
        {
            internal HTMLButtonElement Row;
            internal HTMLElement       IconContainer;
            internal HTMLElement       TextContainer;
            internal HTMLElement       AskedContainer;

            internal Item(string text, UIcons icon, UIconsWeight weight)
            {
                Text   = text ?? string.Empty;
                Icon   = icon;
                Weight = weight;
            }

            /// <summary>
            /// Gets the text of the question, which is also what the host usually sends.
            /// </summary>
            public string Text { get; internal set; }

            /// <summary>
            /// Gets the icon saying what kind of question this is.
            /// </summary>
            public UIcons Icon { get; internal set; }

            /// <summary>
            /// Gets the weight the icon is drawn in.
            /// </summary>
            public UIconsWeight Weight { get; internal set; }

            /// <summary>
            /// Returns a value indicating whether the question has been asked.
            /// </summary>
            public bool IsAsked { get; internal set; }

            /// <summary>
            /// Gets or sets the data behind the action - the prompt to send when it differs from the text
            /// shown, a query, an id - so an ask handler can act on it without a lookup or a cast.
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
        private readonly List<Item> _questions = new List<Item>();

        private string _label;
        private string _subLabel;
        private string _moreFormat       = "Show {0} more";
        private string _askedText        = "Asked";
        private int    _maxVisible       = int.MaxValue;
        private bool   _showAll;
        private bool   _markAskedOnClick = true;
        private bool   _isLoading;
        private Action _onRetry;

        private event Action<ActionCard<TData>, Item> Asked;

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
        /// Gets the questions the card offers, in the order they are shown.
        /// </summary>
        public IReadOnlyList<Item> Actions => _questions;

        /// <summary>
        /// Returns a value indicating whether the card is showing its loading placeholders.
        /// </summary>
        public bool IsLoading => _isLoading;

        /// <summary>
        /// Gets or sets an arbitrary payload for the card - the record the questions are about.
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
        /// questions make more sense with a few of its numbers beside them.
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
        /// Sets the small heading above the questions ("Ask about this company"). A null or empty value
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
        /// Adds a question. The icon says what kind of question it is - a search, a trend, the people
        /// involved, a document - the way a <see cref="ToolCall"/>'s icon names its tool.
        /// </summary>
        public ActionCard<TData> AddAction(string text, UIcons icon = UIcons.CommentQuestion, UIconsWeight weight = UIconsWeight.Regular, TData data = default)
        {
            var question = new Item(text, icon, weight) { Data = data };

            question.IconContainer = Span(Att("tss-actioncard-question-icon"), I(icon, weight));
            question.TextContainer  = Span(Att("tss-actioncard-question-text",  text: question.Text));
            question.AskedContainer = Span(Att("tss-actioncard-question-asked", text: _askedText));

            // The row is the button itself, so it is a tab stop and Enter or Space asks it with no help.
            question.Row = Button(Att("tss-actioncard-question", type: "button", title: question.Text),
                question.IconContainer,
                question.TextContainer,
                question.AskedContainer,
                I(UIcons.ArrowUpRight, cssClass: "tss-actioncard-question-go"));

            question.Row.addEventListener("click", _ => Ask(question));

            _questions.Add(question);
            _list.appendChild(question.Row);

            UpdateState();
            return this;
        }

        /// <summary>
        /// Adds several questions with the default icon.
        /// </summary>
        public ActionCard<TData> AddActions(params string[] questions)
        {
            foreach (var q in questions) AddAction(q);
            return this;
        }

        /// <summary>
        /// Removes a question.
        /// </summary>
        public ActionCard<TData> RemoveAction(Item question)
        {
            if (question == null || !_questions.Remove(question)) return this;

            _list.removeChild(question.Row);

            UpdateState();
            return this;
        }

        /// <summary>
        /// Removes every question, ready for a fresh set.
        /// </summary>
        public ActionCard<TData> ClearActions()
        {
            _questions.Clear();
            ClearChildren(_list);
            _showAll = false;

            UpdateState();
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when a question is clicked (or activated from the keyboard). This
        /// is where the host sends it - usually <c>q.Text</c>, or whatever it kept in <c>q.Data</c>.
        /// </summary>
        public ActionCard<TData> OnAsk(Action<ActionCard<TData>, Item> onAsk)
        {
            Asked += onAsk;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked with the text of a question when it is clicked.
        /// </summary>
        public ActionCard<TData> OnAsk(Action<string> onAsk) => OnAsk((_, q) => onAsk?.Invoke(q.Text));

        /// <summary>
        /// Configures whether clicking a question marks it as asked. On by default; turn it off when the
        /// host decides that itself (only once the message was actually sent, say) and calls
        /// <see cref="MarkAsked(Item, bool)"/>.
        /// </summary>
        public ActionCard<TData> MarkAskedOnClick(bool value = true)
        {
            _markAskedOnClick = value;
            return this;
        }

        /// <summary>
        /// Marks a question as asked - its icon becomes a check and an "Asked" tag appears - or clears the
        /// mark. An asked question can still be clicked again.
        /// </summary>
        public ActionCard<TData> MarkAsked(Item question, bool value = true)
        {
            if (question == null) return this;

            question.IsAsked = value;
            question.Row.UpdateClassIf(value, "tss-actioncard-question-is-asked");

            ClearChildren(question.IconContainer);
            question.IconContainer.appendChild(value ? I(UIcons.Check) : I(question.Icon, question.Weight));

            return this;
        }

        /// <summary>
        /// Marks the question with the given text as asked, for a host that only kept the text.
        /// </summary>
        public ActionCard<TData> MarkAsked(string text, bool value = true) => MarkAsked(_questions.FirstOrDefault(q => q.Text == text), value);

        /// <summary>
        /// Shows only the first <paramref name="count"/> questions, with a "Show N more" button for the rest.
        /// </summary>
        public ActionCard<TData> MaxVisible(int count)
        {
            _maxVisible = Math.Max(1, count);
            UpdateState();
            return this;
        }

        /// <summary>
        /// Shows every question, as clicking "Show N more" does.
        /// </summary>
        public ActionCard<TData> ShowAll()
        {
            _showAll = true;
            UpdateState();
            return this;
        }

        /// <summary>
        /// Sets the text of the "Show N more" button, with <c>{0}</c> for the count and the "Asked" tag,
        /// for localisation.
        /// </summary>
        public ActionCard<TData> SetTexts(string moreFormat = null, string askedText = null, string retryText = null)
        {
            if (moreFormat != null) _moreFormat = moreFormat;

            if (askedText != null)
            {
                _askedText = askedText;

                foreach (var q in _questions) q.AskedContainer.textContent = askedText;
            }

            if (retryText != null) _retry.textContent = retryText;

            UpdateState();
            return this;
        }

        /// <summary>
        /// Shows placeholder rows while the questions are still being generated, in place of the list.
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
        /// Shows an error in place of the questions - generating them failed - with a Retry button when a
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
        /// Draws the card as one wrapping line: the object as a chip, then the questions as pills. For a
        /// transcript where a full card under every answer would be too much.
        /// </summary>
        public ActionCard<TData> Compact(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-actioncard-compact");
            return this;
        }

        /// <summary>
        /// Forces the stacked layout - identity on top, questions below - whatever the width. The card
        /// already stacks itself below about 520px; this is for a host that wants it everywhere.
        /// </summary>
        public ActionCard<TData> Stacked(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-actioncard-stacked");
            return this;
        }

        private void Ask(Item question)
        {
            if (_markAskedOnClick) MarkAsked(question);

            Asked?.Invoke(this, question);
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

            for (var i = 0; i < _questions.Count; i++)
            {
                var isHidden = i >= visible;
                _questions[i].Row.UpdateClassIf(isHidden, Empty);
                if (isHidden) hidden++;
            }

            _more.textContent = string.Format(_moreFormat, hidden);
            _more.UpdateClassIf(hidden == 0 || _isLoading || hasError, Empty);
        }

        /// <inheritdoc />
        public override HTMLElement Render() => InnerElement;
    }
}
