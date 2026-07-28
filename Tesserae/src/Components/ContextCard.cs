using System;
using static Transpose.Core.dom;
using Transpose.Core;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A compact card describing one piece of context attached to a conversation — a file, a page, a
    /// dataset — meant to sit above (or inside) a <see cref="ChatArea"/> composer.
    /// <para>
    /// It is an icon tile (a <see cref="UIcons"/> glyph, an arbitrary component, or an image
    /// thumbnail) on a colored background, followed by a label, an optional second line, and an optional
    /// kind pill ("Doc", "Sheet", "Table"). Passing a remove handler to
    /// <see cref="OnRemove(Action{ContextCard})"/> adds a round (x) button hanging just off the card's
    /// top-right corner that fades in while the card is hovered, focused, or on touch devices where there
    /// is no hover to speak of.
    /// </para>
    /// <para>
    /// <see cref="Compact(bool)"/> turns it into a one-line pill, and <see cref="ContextCards"/> groups
    /// several of them behind one expandable summary.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.ContextCard")]
    public sealed class ContextCard : ComponentBase<ContextCard, HTMLElement>
    {
        // A longer tail than this isn't a file extension worth keeping out of the ellipsis.
        private const int maxExtensionLength = 7;

        private readonly HTMLElement       _iconContainer;
        private readonly HTMLElement       _labelContainer;
        private readonly HTMLElement       _labelText;
        private readonly HTMLElement       _labelExtension;
        private readonly HTMLElement       _subLabelContainer;
        private readonly HTMLElement       _textContainer;
        private readonly HTMLElement       _kindContainer;
        private readonly HTMLElement       _chevron;
        private          HTMLButtonElement _removeButton;
        private          string            _label;
        private          string            _labelStem;
        private          string            _subLabel;
        private          bool              _keepExtensionVisible = true;
        private          bool              _waitingForMount;

        private event Action<ContextCard> RemoveRequested;

        /// <summary>
        /// Initializes a new instance of this class showing the given icon.
        /// </summary>
        public ContextCard(string label, UIcons icon = UIcons.File, UIconsWeight weight = UIconsWeight.Regular)
            : this()
        {
            SetLabel(label);
            SetIcon(icon, weight);
        }

        /// <summary>
        /// Initializes a new instance of this class showing the given icon or image component - an
        /// <see cref="Icon"/>, an <see cref="Image"/>, an emoji, or anything else that renders small.
        /// </summary>
        public ContextCard(string label, IComponent iconOrImage)
            : this()
        {
            SetLabel(label);
            SetIcon(iconOrImage);
        }

        private ContextCard()
        {
            _iconContainer = Div(Att("tss-contextcard-icon"));

            // The label is two elements, not one: a trailing file extension is held outside the
            // ellipsized part, so a narrow card reads "Quarterly repo….pdf" rather than
            // "Quarterly repor…" - the extension is usually the most useful part of a file name.
            _labelText      = Span(Att("tss-contextcard-label-text"));
            _labelExtension = Span(Att("tss-contextcard-label-extension"));
            _labelContainer = Div(Att("tss-contextcard-label"), _labelText, _labelExtension);

            _subLabelContainer = Div(Att("tss-contextcard-sublabel"));

            _textContainer = Div(Att("tss-contextcard-text"), _labelContainer, _subLabelContainer);

            _kindContainer = Div(Att("tss-contextcard-kind"));
            _chevron       = I(UIcons.AngleDown, cssClass: "tss-contextcard-chevron");

            InnerElement = Div(Att("tss-contextcard"), _iconContainer, _textContainer, _kindContainer, _chevron);

            SetLabel(null);
            SetSubLabel(null);
            SetKind(null);
            WithChevron(false);

            AttachClick();
            AttachContextMenu();
        }

        /// <summary>
        /// Gets the label shown by the component.
        /// </summary>
        public string Label    => _label;

        /// <summary>
        /// Gets the secondary line shown below the label, or null when the card has none.
        /// </summary>
        public string SubLabel => _subLabel;

        /// <summary>
        /// Returns a value indicating whether the component shows a remove button.
        /// </summary>
        public bool IsRemovable => _removeButton != null;

        /// <summary>
        /// Gets or sets an arbitrary payload associated with this card - the document, record or file it
        /// stands for, so a remove or click handler can act on it without a lookup.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Sets the label of the component. The label is ellipsized to the width the card is given,
        /// and carries the full text as its native tooltip.
        /// </summary>
        public ContextCard SetLabel(string label)
        {
            _label = label ?? string.Empty;

            var extension = SplitExtension(_label, _keepExtensionVisible, out _labelStem);

            _labelText.textContent        = _labelStem;
            _labelExtension.textContent   = extension ?? string.Empty;
            _labelExtension.style.display = string.IsNullOrEmpty(extension) ? "none" : "";

            // The card is narrow by design and the interesting part of a file name is often its tail,
            // so the untruncated text stays reachable on hover.
            _labelContainer.setAttribute("title", _label);

            FitLabelWhenMeasurable();

            return this;
        }

        /// <summary>
        /// Configures whether a trailing file extension is held out of the ellipsis, so a label too long
        /// for the card reads "Quarterly repo….pdf" rather than "Quarterly repor…". On by default.
        /// </summary>
        public ContextCard KeepExtensionVisible(bool value = true)
        {
            if (_keepExtensionVisible == value) return this;

            _keepExtensionVisible = value;

            return SetLabel(_label);
        }

        /// <summary>
        /// Caps the width the label is ellipsized at, for a dense row where every card should stay short.
        /// The extension, when kept visible, sits outside this width.
        /// </summary>
        public ContextCard MaxLabelWidth(UnitSize size)
        {
            _labelText.style.maxWidth = size is null ? "" : size.ToString();

            FitLabelWhenMeasurable();

            return this;
        }

        /// <summary>
        /// Sets the kind of context this card stands for - "Doc", "Sheet", "Email", "Table", "Folder" -
        /// shown as a small pill at the end of the card. A null or empty value hides it.
        /// </summary>
        public ContextCard SetKind(string kind)
        {
            var isEmpty = string.IsNullOrEmpty(kind);

            _kindContainer.innerText = isEmpty ? string.Empty : kind;
            _kindContainer.UpdateClassIf(isEmpty, "tss-contextcard-empty");

            return this;
        }

        /// <summary>
        /// Renders the secondary line in the monospace font, for a path, a table name or a size - the same
        /// treatment <see cref="ToolCall"/> gives the command it names.
        /// </summary>
        public ContextCard MonospaceSubLabel(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-contextcard-mono");
            return this;
        }

        /// <summary>
        /// Shows a chevron at the end of the card, the hint that clicking it opens what it stands for.
        /// Pair it with <see cref="OnClick(ComponentEventHandler{ContextCard, MouseEvent}, bool)"/>.
        /// </summary>
        public ContextCard WithChevron(bool value = true)
        {
            _chevron.UpdateClassIf(!value, "tss-contextcard-empty");
            return this;
        }

        /// <summary>
        /// Sets the secondary line shown below the label ("PDF", "3 pages", a folder). A null or empty
        /// value hides the line and leaves the card as a single centered row.
        /// </summary>
        public ContextCard SetSubLabel(string subLabel)
        {
            _subLabel = subLabel;

            var isEmpty = string.IsNullOrEmpty(subLabel);

            _subLabelContainer.innerText = isEmpty ? string.Empty : subLabel;
            _subLabelContainer.UpdateClassIf(isEmpty, "tss-contextcard-empty");

            return this;
        }

        /// <summary>
        /// Sets the icon shown on the tile.
        /// </summary>
        public ContextCard SetIcon(UIcons icon, UIconsWeight weight = UIconsWeight.Regular)
        {
            ClearChildren(_iconContainer);
            _iconContainer.classList.remove("tss-contextcard-icon-image");
            _iconContainer.appendChild(I(icon, weight));

            return this;
        }

        /// <summary>
        /// Sets the icon of the component to an arbitrary component - an <see cref="Icon"/> with its own
        /// color, an emoji, an <see cref="Image"/>, a file-type badge. A null value empties the tile,
        /// keeping its background as a plain colored square.
        /// </summary>
        public ContextCard SetIcon(IComponent iconOrImage)
        {
            ClearChildren(_iconContainer);
            _iconContainer.classList.remove("tss-contextcard-icon-image");

            if (iconOrImage != null)
            {
                _iconContainer.appendChild(iconOrImage.Render());
            }

            return this;
        }

        /// <summary>
        /// Sets a thumbnail that fills the whole tile (cropped to cover it), for context that has a
        /// preview of its own - an image, a screenshot, a site favicon. The tile's background color is
        /// dropped, as the image covers it.
        /// </summary>
        public ContextCard SetImage(string url)
        {
            ClearChildren(_iconContainer);

            if (!string.IsNullOrEmpty(url))
            {
                _iconContainer.appendChild(Image(Att("tss-contextcard-image", src: url)));
            }

            _iconContainer.UpdateClassIf(!string.IsNullOrEmpty(url), "tss-contextcard-icon-image");

            return this;
        }

        /// <summary>
        /// Sets the background color of the icon tile (any CSS color, e.g. "#ef4444" or
        /// "var(--tss-danger-background-color)").
        /// </summary>
        public ContextCard IconBackground(string color)
        {
            _iconContainer.style.background = color ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the color of the glyph on the icon tile.
        /// </summary>
        public ContextCard IconForeground(string color)
        {
            _iconContainer.style.color = color ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Tints the icon tile with the given color: a wash of it behind the glyph, and the glyph itself in
        /// full strength. The quieter alternative to a saturated tile, and what a row of many cards usually
        /// wants so the colors read as file types rather than as decoration.
        /// </summary>
        public ContextCard IconTint(string color, int percent = 14)
        {
            if (string.IsNullOrEmpty(color)) return this;

            _iconContainer.style.background = $"color-mix(in srgb, {color} {percent}%, transparent)";
            _iconContainer.style.color      = color;

            return this;
        }

        /// <summary>
        /// Configures the icon tile to have no background, letting the glyph or image sit directly on
        /// the card.
        /// </summary>
        public ContextCard NoIconBackground()
        {
            _iconContainer.classList.add("tss-contextcard-icon-nobackground");
            return this;
        }

        /// <summary>
        /// Sets the background color of the card itself.
        /// </summary>
        public ContextCard Background(string color)
        {
            InnerElement.style.background = color ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the user clicks the remove button, and adds that button to
        /// the card if it isn't there yet. The card does not remove itself - the handler owns whatever
        /// list the card lives in, and can also drop the context it stands for.
        /// </summary>
        public ContextCard OnRemove(Action<ContextCard> onRemove)
        {
            RemoveRequested += onRemove;
            EnsureRemoveButton();

            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the user clicks the remove button, and adds that button to
        /// the card if it isn't there yet.
        /// </summary>
        public ContextCard OnRemove(Action onRemove) => OnRemove(_ => onRemove?.Invoke());

        /// <summary>
        /// Configures whether the card shows its remove button. Turning it off keeps any handler
        /// registered with <see cref="OnRemove(Action{ContextCard})"/>, so turning it back on restores
        /// the same behaviour.
        /// </summary>
        public ContextCard Removable(bool value = true)
        {
            if (value)
            {
                EnsureRemoveButton();
            }
            else if (_removeButton != null)
            {
                InnerElement.classList.remove("tss-contextcard-removable");
                InnerElement.removeChild(_removeButton);
                _removeButton = null;
            }

            return this;
        }

        /// <summary>
        /// Configures the component to show no remove button.
        /// </summary>
        public ContextCard NoRemove() => Removable(false);

        /// <summary>
        /// Configures the card as a single tighter row - a pill with a small tile, the label, and the
        /// secondary line beside it rather than below. Use it when many cards share a composer, or for one
        /// card named inline in a sentence.
        /// </summary>
        public ContextCard Compact(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-contextcard-compact");
            FitLabelWhenMeasurable();
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the click event fires. A clickable card also becomes
        /// keyboard reachable and is activated with Enter or Space.
        /// </summary>
        public override ContextCard OnClick(ComponentEventHandler<ContextCard, MouseEvent> onClick, bool clearPrevious = true)
        {
            if (!InnerElement.classList.contains("tss-contextcard-clickable"))
            {
                InnerElement.classList.add("tss-contextcard-clickable");
                InnerElement.setAttribute("role",     "button");
                InnerElement.setAttribute("tabindex", "0");

                InnerElement.addEventListener("keydown", e =>
                {
                    var ev = e.As<KeyboardEvent>();

                    if (ev.key == "Enter" || ev.key == " ")
                    {
                        StopEvent(ev);
                        InnerElement.click();
                    }
                });
            }

            return base.OnClick(onClick, clearPrevious);
        }

        private void EnsureRemoveButton()
        {
            if (_removeButton != null) return;

            _removeButton = Button(Att("tss-contextcard-remove", type: "button", ariaLabel: "Remove"), I(UIcons.CrossSmall));

            _removeButton.addEventListener("click", ev =>
            {
                // The card itself can be clickable (opening the context it stands for), so removing it
                // must not read as opening it.
                StopEvent(ev);
                RemoveRequested?.Invoke(this);
            });

            InnerElement.appendChild(_removeButton);
            InnerElement.classList.add("tss-contextcard-removable");
        }

        // CSS alone leaves the tail of a max-width label box unused, which shows up as a gap between the
        // ellipsis and the extension - "Q3 rev… .xlsx". Truncating the text ourselves puts the ellipsis
        // right where the name stops, so the two read as one file name. `text-overflow: ellipsis` still
        // covers us until this can measure.
        private void FitLabelWhenMeasurable()
        {
            if (_labelText.isConnected)
            {
                FitLabel();
                return;
            }

            if (_waitingForMount) return;

            _waitingForMount = true;

            DomObserver.WhenMounted(InnerElement, () =>
            {
                _waitingForMount = false;
                FitLabel();
            });
        }

        private void FitLabel()
        {
            _labelText.textContent = _labelStem ?? string.Empty;

            if (string.IsNullOrEmpty(_labelStem)) return;

            var limit = _labelText.clientWidth;

            if (limit <= 0 || _labelText.scrollWidth <= limit) return;

            // Longest prefix that still fits once the ellipsis is appended.
            var low  = 0;
            var high = _labelStem.Length;

            while (low < high)
            {
                var middle = (low + high + 1) / 2;

                _labelText.textContent = _labelStem.Substring(0, middle) + "…";

                if (_labelText.scrollWidth <= limit) low = middle;
                else                                high = middle - 1;
            }

            // Trimmed so a cut landing on a space doesn't read as "Q3 revenue ….xlsx".
            var kept = low <= 0 ? string.Empty : _labelStem.Substring(0, low).TrimEnd();

            _labelText.textContent = kept + "…";
        }

        private static string SplitExtension(string label, bool keepExtensionVisible, out string stem)
        {
            stem = label;

            if (!keepExtensionVisible || string.IsNullOrEmpty(label)) return null;

            var dot = label.LastIndexOf('.');

            if (dot <= 0 || dot == label.Length - 1) return null;

            var extension = label.Substring(dot);

            if (extension.Length > maxExtensionLength || extension.Contains(" ")) return null;

            stem = label.Substring(0, dot);

            return extension;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
