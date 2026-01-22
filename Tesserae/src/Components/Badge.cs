using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
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
            _textSpan  = Span(_("tss-token-text", text: text ?? string.Empty));
            _content   = Span(_("tss-token-content"), _textSpan);
            InnerElement = Span(_($"tss-token {cssClass}"), _content);
        }

        public string Text
        {
            get => _textSpan.innerText;
            set => _textSpan.innerText = value ?? string.Empty;
        }

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
                    _icon = I(_("tss-token-icon"));
                    _content.insertBefore(_icon, _textSpan);
                }

                _icon.className = $"tss-token-icon {value}";
            }
        }

        public bool IsPill
        {
            get => InnerElement.classList.contains("tss-token-pill");
            set => InnerElement.UpdateClassIf(value, "tss-token-pill");
        }

        public bool IsOutline
        {
            get => InnerElement.classList.contains("tss-token-outline");
            set => InnerElement.UpdateClassIf(value, "tss-token-outline");
        }

        public bool IsFilled
        {
            get => InnerElement.classList.contains("tss-token-filled");
            set => InnerElement.UpdateClassIf(value, "tss-token-filled");
        }

        public T SetText(string text)
        {
            Text = text;
            return (T)this;
        }

        public T SetIcon(string icon)
        {
            Icon = icon;
            return (T)this;
        }

        public T Pill(bool value = true)
        {
            IsPill = value;
            return (T)this;
        }

        public T Outline(bool value = true)
        {
            IsOutline = value;
            if (value)
            {
                IsFilled = false;
            }
            return (T)this;
        }

        public T Filled(bool value = true)
        {
            IsFilled = value;
            if (value)
            {
                IsOutline = false;
            }
            return (T)this;
        }

        public T Background(string color)
        {
            InnerElement.style.backgroundColor = color;
            return (T)this;
        }

        public T Foreground(string color)
        {
            InnerElement.style.color = color;
            return (T)this;
        }

        public T Tone(BadgeTone tone)
        {
            var className = tone == BadgeTone.Neutral ? null : $"tss-token-{tone.ToString().ToLower()}";
            SetVariantClass(className);
            return (T)this;
        }

        public T Primary() => Tone(BadgeTone.Primary);
        public T Success() => Tone(BadgeTone.Success);
        public T Warning() => Tone(BadgeTone.Warning);
        public T Danger()  => Tone(BadgeTone.Danger);
        public T Info()    => Tone(BadgeTone.Info);
        public T Neutral() => Tone(BadgeTone.Neutral);

        public T OnRemove(Action<T> onRemove)
        {
            _removeRequested += onRemove;
            EnsureRemoveButton();
            return (T)this;
        }

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

            _removeButton = Button(_("tss-token-remove", type: "button", ariaLabel: "Remove"), I(UIcons.CrossSmall));
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

        public override HTMLElement Render() => InnerElement;
    }

    [H5.Name("tss.Badge")]
    public sealed class Badge : TokenBase<Badge>
    {
        public Badge(string text = null) : base("tss-badge", text)
        {
        }
    }

    [H5.Name("tss.Tag")]
    public sealed class Tag : TokenBase<Tag>
    {
        public Tag(string text = null) : base("tss-tag", text)
        {
        }
    }

    [H5.Name("tss.Chip")]
    public sealed class Chip : TokenBase<Chip>
    {
        public Chip(string text = null) : base("tss-chip", text)
        {
        }
    }
}
