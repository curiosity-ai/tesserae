using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A star-rating component for collecting or displaying ratings.
    /// </summary>
    [H5.Name("tss.Rating")]
    public sealed class Rating : ComponentBase<Rating, HTMLElement>, IBindableComponent<int>
    {
        private readonly HTMLElement[]            _stars;
        private readonly SettableObservable<int>  _observable;
        private          int                      _value;
        private          int                      _hoverValue;
        private          int                      _maxStars;
        private          bool                     _readOnly;
        private          bool                     _allowHalf;
        private          string                   _color;

        private event Action<int> ValueChanged;

        /// <summary>
        /// Initializes a new Rating component.
        /// </summary>
        /// <param name="maxStars">Maximum number of stars (default 5).</param>
        public Rating(int maxStars = 5)
        {
            _maxStars   = Math.Max(1, maxStars);
            _value      = 0;
            _hoverValue = 0;
            _color      = "var(--tss-rating-color, #f5c518)";
            _observable = new SettableObservable<int>(0);

            InnerElement = Div(_("tss-rating", role: "radiogroup", ariaLabel: "Rating"));
            _stars       = new HTMLElement[_maxStars];

            for (int i = 0; i < _maxStars; i++)
            {
                var index = i + 1;
                var star  = Span(_("tss-rating-star"));
                star.setAttribute("role",       "radio");
                star.setAttribute("aria-label", $"{index} star{(index == 1 ? "" : "s")}");
                star.setAttribute("tabindex",   "0");
                star.innerHTML = "★";

                star.addEventListener("click",     _ => SetStarValue(index));
                star.addEventListener("mouseenter", _ => SetHover(index));
                star.addEventListener("mouseleave", _ => ClearHover());
                star.addEventListener("keydown",    e =>
                {
                    var kb = e.As<KeyboardEvent>();
                    if (kb.key == "Enter" || kb.key == " ")
                    {
                        SetStarValue(index);
                        e.preventDefault();
                    }
                });

                _stars[i] = star;
                InnerElement.appendChild(star);
            }

            UpdateStars();
        }

        /// <summary>Gets or sets the current rating value (0 = unrated).</summary>
        public int Value
        {
            get => _value;
            set
            {
                var clamped = Math.Max(0, Math.Min(value, _maxStars));
                if (_value != clamped)
                {
                    _value = clamped;
                    InnerElement.setAttribute("aria-valuenow", _value.ToString());
                    UpdateStars();
                    _observable.Value = _value;
                    ValueChanged?.Invoke(_value);
                }
            }
        }

        /// <summary>
        /// Returns an observable that tracks the rating value.
        /// </summary>
        public IObservable<int> AsObservable() => _observable;

        /// <summary>
        /// Programmatically updates the rating as part of a two-way binding.
        /// </summary>
        public void SetBoundValue(int value) => Value = value;

        /// <summary>Gets or sets whether the rating is read-only.</summary>
        public bool IsReadOnly
        {
            get => _readOnly;
            set
            {
                _readOnly = value;
                InnerElement.UpdateClassIf(_readOnly, "tss-rating-readonly");
                foreach (var star in _stars)
                {
                    if (_readOnly)
                        star.setAttribute("tabindex", "-1");
                    else
                        star.setAttribute("tabindex", "0");
                }
            }
        }

        /// <summary>Sets a custom star color.</summary>
        public Rating Color(string color)
        {
            _color = color;
            UpdateStars();
            return this;
        }

        /// <summary>Registers a callback for when the value changes.</summary>
        public Rating OnChange(Action<int> onChange)
        {
            ValueChanged += onChange;
            return this;
        }

        /// <summary>Sets the current value.</summary>
        public Rating SetValue(int value)
        {
            Value = value;
            return this;
        }

        /// <summary>Makes the rating read-only.</summary>
        public Rating ReadOnly(bool value = true)
        {
            IsReadOnly = value;
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;

        private void SetStarValue(int index)
        {
            if (_readOnly) return;
            Value = (Value == index) ? 0 : index;
        }

        private void SetHover(int index)
        {
            if (_readOnly) return;
            _hoverValue = index;
            UpdateStars();
        }

        private void ClearHover()
        {
            if (_readOnly) return;
            _hoverValue = 0;
            UpdateStars();
        }

        private void UpdateStars()
        {
            var active = _hoverValue > 0 ? _hoverValue : _value;
            for (int i = 0; i < _stars.Length; i++)
            {
                var filled = i < active;
                _stars[i].UpdateClassIf(filled,  "tss-rating-star-filled");
                _stars[i].UpdateClassIf(!filled, "tss-rating-star-empty");
                _stars[i].UpdateClassIf(_hoverValue > 0 && i < _hoverValue, "tss-rating-star-hover");
                _stars[i].setAttribute("aria-checked", filled ? "true" : "false");
                if (filled)
                    _stars[i].style.color = _color;
                else
                    _stars[i].style.color = "";
            }
        }
    }
}
