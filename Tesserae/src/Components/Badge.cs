using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Predefined visual tones used to color the <see cref="Badge"/> component.
    /// </summary>
    public enum BadgeTone
    {
        Neutral,
        Primary,
        Success,
        Warning,
        Danger,
        Info
    }

    public abstract class TokenBase<T> : ComponentBase<T, HTMLElement> where T : TokenBase<T>
    {
        protected readonly HTMLSpanElement _textSpan;
        protected readonly HTMLElement     _content;
        protected          HTMLElement     _icon;
        protected          HTMLButtonElement _removeButton;
        private            string          _variantClass;
        private            Action<T>       _removeRequested;

        protected TokenBase(string cssClass, string text)
        {
            _textSpan  = Span(Att("tss-token-text", text: text ?? string.Empty));
            _content   = Span(Att("tss-token-content"), _textSpan);
            InnerElement = Span(Att($"tss-token {cssClass}"), _content);
        }

        /// <summary>
        /// Gets or sets the text shown in the component.
        /// </summary>
        public string Text
        {
            get => _textSpan.innerText;
            set => _textSpan.innerText = value ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets the icon shown by the component.
        /// </summary>
        public string Icon
        {
            get => _icon?.className;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (_icon != null)
                    {
                        _content.removeChild(_icon);
                        _icon = null;
                    }
                    return;
                }

                if (_icon == null)
                {
                    _icon = I(Att("tss-token-icon"));
                    _content.insertBefore(_icon, _textSpan);
                }

                _icon.className = $"tss-token-icon {value}";
            }
        }

        /// <summary>
        /// Returns a value indicating whether the component is pill.
        /// </summary>
        public bool IsPill
        {
            get => InnerElement.classList.contains("tss-token-pill");
            set => InnerElement.UpdateClassIf(value, "tss-token-pill");
        }

        /// <summary>
        /// Returns a value indicating whether the component is outline.
        /// </summary>
        public bool IsOutline
        {
            get => InnerElement.classList.contains("tss-token-outline");
            set => InnerElement.UpdateClassIf(value, "tss-token-outline");
        }

        /// <summary>
        /// Returns a value indicating whether the component is filled.
        /// </summary>
        public bool IsFilled
        {
            get => InnerElement.classList.contains("tss-token-filled");
            set => InnerElement.UpdateClassIf(value, "tss-token-filled");
        }

        /// <summary>
        /// Sets the text of the component.
        /// </summary>
        public T SetText(string text)
        {
            Text = text;
            return (T)this;
        }

        /// <summary>
        /// Sets the icon of the component.
        /// </summary>
        public T SetIcon(string icon)
        {
            Icon = icon;
            return (T)this;
        }

        /// <summary>
        /// Configures the component to pill.
        /// </summary>
        public T Pill(bool value = true)
        {
            IsPill = value;
            return (T)this;
        }

        /// <summary>
        /// Configures the component to outline.
        /// </summary>
        public T Outline(bool value = true)
        {
            IsOutline = value;
            if (value)
            {
                IsFilled = false;
            }
            return (T)this;
        }

        /// <summary>
        /// Configures the component to filled.
        /// </summary>
        public T Filled(bool value = true)
        {
            IsFilled = value;
            if (value)
            {
                IsOutline = false;
            }
            return (T)this;
        }

        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public T Background(string color)
        {
            InnerElement.style.backgroundColor = color;
            return (T)this;
        }

        /// <summary>
        /// Gets or sets the CSS color (foreground) of the component.
        /// </summary>
        public T Foreground(string color)
        {
            InnerElement.style.color = color;
            return (T)this;
        }

        /// <summary>
        /// Configures the component to tone.
        /// </summary>
        public T Tone(BadgeTone tone)
        {
            var className = tone == BadgeTone.Neutral ? null : $"tss-token-{tone.ToString().ToLower()}";
            SetVariantClass(className);
            return (T)this;
        }

        /// <summary>
        /// Styles the component using the primary tone.
        /// </summary>
        public T Primary() => Tone(BadgeTone.Primary);
        /// <summary>
        /// Styles the component using the success tone.
        /// </summary>
        public T Success() => Tone(BadgeTone.Success);
        /// <summary>
        /// Styles the component using the warning tone.
        /// </summary>
        public T Warning() => Tone(BadgeTone.Warning);
        /// <summary>
        /// Styles the component using the danger tone.
        /// </summary>
        public T Danger()  => Tone(BadgeTone.Danger);
        /// <summary>
        /// Styles the component using the informational tone.
        /// </summary>
        public T Info()    => Tone(BadgeTone.Info);
        /// <summary>
        /// Configures the component to neutral.
        /// </summary>
        public T Neutral() => Tone(BadgeTone.Neutral);

        /// <summary>
        /// Registers a callback invoked when the remove event fires.
        /// </summary>
        public T OnRemove(Action<T> onRemove)
        {
            _removeRequested += onRemove;
            EnsureRemoveButton();
            return (T)this;
        }

        /// <summary>
        /// Configures the component to removable.
        /// </summary>
        public T Removable(bool value = true)
        {
            if (value)
            {
                EnsureRemoveButton();
            }
            else if (_removeButton != null)
            {
                InnerElement.classList.remove("tss-token-removable");
                InnerElement.removeChild(_removeButton);
                _removeButton = null;
            }

            return (T)this;
        }

        protected void EnsureRemoveButton()
        {
            if (_removeButton != null)
            {
                return;
            }

            _removeButton = Button(Att("tss-token-remove", type: "button", ariaLabel: "Remove"), I(UIcons.CrossSmall));
            _removeButton.addEventListener("click", ev =>
            {
                StopEvent(ev);
                _removeRequested?.Invoke((T)this);
            });

            InnerElement.appendChild(_removeButton);
            InnerElement.classList.add("tss-token-removable");
        }

        protected void SetVariantClass(string className)
        {
            if (!string.IsNullOrEmpty(_variantClass))
            {
                InnerElement.classList.remove(_variantClass);
            }

            _variantClass = className;

            if (!string.IsNullOrEmpty(_variantClass))
            {
                InnerElement.classList.add(_variantClass);
            }
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }

    [Transpose.Name("tss.Badge")]
    public sealed class Badge : TokenBase<Badge>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Badge(string text = null) : base("tss-badge", text)
        {
        }
    }

    [Transpose.Name("tss.Tag")]
    public sealed class Tag : TokenBase<Tag>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Tag(string text = null) : base("tss-tag", text)
        {
        }
    }

    [Transpose.Name("tss.Chip")]
    public sealed class Chip : TokenBase<Chip>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Chip(string text = null) : base("tss-chip", text)
        {
        }
    }
}
