using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Carousel")]
    public sealed class Carousel : ComponentBase<Carousel, HTMLElement>
    {
        private readonly HTMLElement       _viewport;
        private readonly HTMLElement       _track;
        private readonly HTMLElement       _controls;
        private readonly HTMLElement       _indicators;
        private readonly HTMLButtonElement _prevButton;
        private readonly HTMLButtonElement _nextButton;
        private readonly List<HTMLElement> _slides;
        private          int               _currentIndex;
        private          Action<Carousel>  _onSlideChanged;

        public Carousel(params IComponent[] slides)
        {
            _slides     = new List<HTMLElement>();
            _track      = Div(_("tss-carousel-track"));
            _viewport   = Div(_("tss-carousel-viewport"), _track);
            _controls   = Div(_("tss-carousel-controls"));
            _indicators = Div(_("tss-carousel-indicators"));

            _prevButton = Button(_("tss-carousel-nav", type: "button", ariaLabel: "Previous"), I(UIcons.AngleLeft));
            _nextButton = Button(_("tss-carousel-nav", type: "button", ariaLabel: "Next"), I(UIcons.AngleRight));

            _prevButton.addEventListener("click", _ => Previous());
            _nextButton.addEventListener("click", _ => Next());

            _controls.appendChild(_prevButton);
            _controls.appendChild(_nextButton);

            InnerElement = Div(_("tss-carousel"), _viewport, _controls, _indicators);

            AddSlides(slides);
            SetIndex(0, false);
        }

        public int CurrentIndex
        {
            get => _currentIndex;
            set => SetIndex(value);
        }

        public int SlideCount => _slides.Count;

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

        public Carousel OnSlideChange(Action<Carousel> onSlideChanged)
        {
            _onSlideChanged += onSlideChanged;
            return this;
        }

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

            if (raiseEvent)
            {
                _onSlideChanged?.Invoke(this);
            }

            return this;
        }

        public Carousel Next()
        {
            return SetIndex(_currentIndex + 1);
        }

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

        public override HTMLElement Render() => InnerElement;
    }
}
