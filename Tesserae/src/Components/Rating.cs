using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Rating")]
    public class Rating : ComponentBase<Rating, HTMLDivElement>, IObservableComponent<int>
    {
        private int _max;
        private int _value;
        private bool _isReadOnly;
        private readonly SettableObservable<int> _observable;

        public Rating(int value = 0, int max = 5)
        {
            _value = value;
            _max = max;
            _observable = new SettableObservable<int>(value);
            InnerElement = Div(_("tss-rating"));
            Rebuild();
        }

        public int Value
        {
            get => _value;
            set { _value = value; _observable.Value = value; Rebuild(); }
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set { _isReadOnly = value; Rebuild(); }
        }

        private void Rebuild()
        {
            ClearChildren(InnerElement);
            for (int i = 1; i <= _max; i++)
            {
                var starValue = i;
                var star = I(_("tss-rating-star " + UIcons.Star.ToString()));
                if (i <= _value) star.classList.add("tss-active");
                if (_isReadOnly) star.classList.add("tss-readonly");
                else
                {
                    star.onclick = (e) =>
                    {
                        StopEvent(e);
                        Value = starValue;
                        RaiseOnChange(ev: null);
                    };
                }
                InnerElement.appendChild(star);
            }
        }

        public IObservable<int> AsObservable() => _observable;

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
