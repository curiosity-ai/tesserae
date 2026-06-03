using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A horizontal carousel that cycles through a sequence of items (slides) one at a time, with arrows and
    /// pagination.
    /// </summary>
    [H5.Name("tss.Carousel")]
    public sealed class Carousel : ComponentBase<Carousel, HTMLElement>, IBindableComponent<int>
    {
        private readonly HTMLElement             _viewport;
        private readonly HTMLElement             _track;
        private readonly HTMLElement             _controls;
        private readonly HTMLElement             _indicators;
        private readonly HTMLButtonElement       _prevButton;
        private readonly HTMLButtonElement       _nextButton;
        private readonly List<HTMLElement>       _slides;
        private readonly SettableObservable<int> _observable;
        private          int                     _currentIndex;
        private          Action<Carousel>        _onSlideChanged;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Carousel(params IComponent[] slides)
        {
            _slides     = new List<HTMLElement>();
            _track      = Div(_("tss-carousel-track"));
            _viewport   = Div(_("tss-carousel-viewport"), _track);
            _controls   = Div(_("tss-carousel-controls"));
            _indicators = Div(_("tss-carousel-indicators"));
            _observable = new SettableObservable<int>(0);

            _prevButton = Button(_("tss-carousel-nav", type: "button", ariaLabel: "Previous"), I(UIcons.AngleLeft));
            _nextButton = Button(_("tss-carousel-nav", type: "button", ariaLabel: "Next"),     I(UIcons.AngleRight));

            _prevButton.addEventListener("click", _ => Previous());
            _nextButton.addEventListener("click", _ => Next());

            _controls.appendChild(_prevButton);
            _controls.appendChild(_nextButton);

            InnerElement = Div(_("tss-carousel"), _viewport, _controls, _indicators);

            AddSlides(slides);
            SetIndex(0, false);
        }

        /// <summary>
        /// Gets or sets the current index.
        /// </summary>
        public int CurrentIndex
        {
            get => _currentIndex;
            set => SetIndex(value);
        }

        /// <summary>
        /// Gets or sets the slide count.
        /// </summary>
        public int SlideCount => _slides.Count;

        /// <summary>
        /// Adds the given slide to the component.
        /// </summary>
        public Carousel AddSlide(IComponent content)
        {
            if (content == null)
            {
                return this;
            }

            var slide = Div(_("tss-carousel-slide"), content.Render());
            _slides.Add(slide);
            _track.appendChild(slide);
            UpdateIndicators();
            UpdateControls();
            return this;
        }

        /// <summary>
        /// Adds the given slides to the component.
        /// </summary>
        public Carousel AddSlides(params IComponent[] slides)
        {
            if (slides == null)
            {
                return this;
            }

            foreach (var slide in slides)
            {
                AddSlide(slide);
            }

            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the slide change event fires.
        /// </summary>
        public Carousel OnSlideChange(Action<Carousel> onSlideChanged)
        {
            _onSlideChanged += onSlideChanged;
            return this;
        }

        /// <summary>
        /// Configures the pad slides on the component.
        /// </summary>
        public Carousel PadSlides()
        {
            _track.classList.add("tss-carousel-track-pad-slides");
            return this;
        }
        /// <summary>
        /// Sets the index of the component.
        /// </summary>
        public Carousel SetIndex(int index, bool raiseEvent = true)
        {
            if (_slides.Count == 0)
            {
                _currentIndex = 0;
                UpdateTrack();
                return this;
            }

            var clamped = Math.Max(0, Math.Min(index, _slides.Count - 1));

            if (_currentIndex == clamped)
            {
                return this;
            }

            _currentIndex = clamped;
            UpdateTrack();
            UpdateIndicators();
            UpdateControls();
            _observable.Value = _currentIndex;

            if (raiseEvent)
            {
                _onSlideChanged?.Invoke(this);
            }

            return this;
        }

        /// <summary>
        /// Returns an observable that tracks the active slide index.
        /// </summary>
        public IObservable<int> AsObservable() => _observable;

        /// <summary>
        /// Programmatically updates the active slide as part of a two-way binding.
        /// </summary>
        public void SetBoundValue(int value) => SetIndex(value);

        /// <summary>
        /// Configures the component to next.
        /// </summary>
        public Carousel Next()
        {
            return SetIndex(_currentIndex + 1);
        }

        /// <summary>
        /// Configures the component to previous.
        /// </summary>
        public Carousel Previous()
        {
            return SetIndex(_currentIndex - 1);
        }

        private void UpdateTrack()
        {
            var offset = _currentIndex * 100;
            _track.style.transform = $"translateX(-{offset}%)";
        }

        private void UpdateControls()
        {
            _prevButton.disabled = _currentIndex == 0;
            _nextButton.disabled = _currentIndex >= _slides.Count - 1;
            _prevButton.UpdateClassIf(_prevButton.disabled, "tss-disabled");
            _nextButton.UpdateClassIf(_nextButton.disabled, "tss-disabled");
        }

        private void UpdateIndicators()
        {
            ClearChildren(_indicators);

            for (var i = 0; i < _slides.Count; i++)
            {
                var dot = Button(_("tss-carousel-indicator", type: "button", ariaLabel: $"Go to slide {i + 1}"));
                dot.UpdateClassIf(i == _currentIndex, "tss-active");
                var index = i;
                dot.addEventListener("click", _ => SetIndex(index));
                _indicators.appendChild(dot);
            }
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}