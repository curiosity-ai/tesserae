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
    /// thumbnail) on a colored background, followed by a label and an optional second line. Passing
    /// a remove handler to <see cref="OnRemove(Action{ContextCard})"/> adds a round (x) button hanging
    /// just off the card's top-right corner that fades in while the card is hovered, focused, or on
    /// touch devices where there is no hover to speak of.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.ContextCard")]
    public sealed class ContextCard : ComponentBase<ContextCard, HTMLElement>
    {
        private readonly HTMLElement       _iconContainer;
        private readonly HTMLElement       _labelContainer;
        private readonly HTMLElement       _subLabelContainer;
        private readonly HTMLElement       _textContainer;
        private          HTMLButtonElement _removeButton;
        private          string            _label;
        private          string            _subLabel;

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
            _iconContainer     = Div(Att("tss-contextcard-icon"));
            _labelContainer    = Div(Att("tss-contextcard-label"));
            _subLabelContainer = Div(Att("tss-contextcard-sublabel"));

            _textContainer = Div(Att("tss-contextcard-text"), _labelContainer, _subLabelContainer);

            InnerElement = Div(Att("tss-contextcard"), _iconContainer, _textContainer);

            SetLabel(null);
            SetSubLabel(null);

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
        /// Sets the label of the component. The label is ellipsized to the width the card is given,
        /// and carries the full text as its native tooltip.
        /// </summary>
        public ContextCard SetLabel(string label)
        {
            _label = label ?? string.Empty;

            _labelContainer.innerText = _label;
            // The card is narrow by design and the interesting part of a file name is often its tail,
            // so the untruncated text stays reachable on hover.
            _labelContainer.setAttribute("title", _label);

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
        /// Configures the card as a single, tighter row: the icon tile shrinks and the secondary line
        /// (if any) moves in beside the label. Use it when many cards share the composer.
        /// </summary>
        public ContextCard Compact()
        {
            InnerElement.classList.add("tss-contextcard-compact");
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

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
