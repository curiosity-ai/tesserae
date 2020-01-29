using static Tesserae.UI;
using static Retyped.dom;
using System.Linq;
using System;

namespace Tesserae.Components
{
    public class Spinner : ComponentBase<Spinner, HTMLDivElement>
    {
        private HTMLElement _container;
        private HTMLElement _label;

        public Spinner(string text = string.Empty)
        {
            InnerElement = Div(_("tss-spinner"));
            _label = Label(_("tss-spinner-label", text: text));
            _container = Div(_("tss-spinner-container tss-spinner-position-right tss-spinner-size-small"), InnerElement, _label);
        }

        public LabelPosition Position
        {
            get
            {
                var s = _container.classList.FirstOrDefault(x => x.StartsWith("tss-spinner-position-"));
                if (s != null && Enum.TryParse<LabelPosition>(s, true, out LabelPosition result)) return result;
                return LabelPosition.Right;
            }
            set
            {
                var s = _container.classList.FirstOrDefault(x => x.StartsWith("tss-spinner-position-"));
                if (s != null) _container.classList.remove(s);
                _container.classList.add($"tss-spinner-position-{value.ToString().ToLower()}");
            }
        }

        public CircleSize Size
        {
            get
            {
                var s = _container.classList.FirstOrDefault(x => x.StartsWith("tss-spinner-size-"));
                if (s != null && Enum.TryParse<CircleSize>(s, true, out CircleSize result)) return result;
                return CircleSize.Small;
            }
            set
            {
                var s = _container.classList.FirstOrDefault(x => x.StartsWith("tss-spinner-size-"));
                if (s != null) _container.classList.remove(s);
                _container.classList.add($"tss-spinner-size-{value.ToString().ToLower()}");
            }
        }

        public string Text
        {
            get { return _label.innerText; }
            set { _label.innerText = value; }
        }

        public override HTMLElement Render()
        {
            return _container;
        }

        public enum LabelPosition
        {
            Above,
            Below,
            Left,
            Right
        }

        public enum CircleSize
        {
            XSmall,
            Small,
            Medium,
            Large
        }
    }

    public static class SpinnerExtensions
    {
        public static Spinner Left(this Spinner spinner)
        {
            spinner.Position = Spinner.LabelPosition.Left;
            return spinner;
        }
        public static Spinner Right(this Spinner spinner)
        {
            spinner.Position = Spinner.LabelPosition.Right;
            return spinner;
        }
        public static Spinner Above(this Spinner spinner)
        {
            spinner.Position = Spinner.LabelPosition.Above;
            return spinner;
        }
        public static Spinner Below(this Spinner spinner)
        {
            spinner.Position = Spinner.LabelPosition.Below;
            return spinner;
        }

        public static Spinner XSmall(this Spinner spinner)
        {
            spinner.Size = Spinner.CircleSize.XSmall;
            return spinner;
        }
        public static Spinner Small(this Spinner spinner)
        {
            spinner.Size = Spinner.CircleSize.Small;
            return spinner;
        }
        public static Spinner Medium(this Spinner spinner)
        {
            spinner.Size = Spinner.CircleSize.Medium;
            return spinner;
        }
        public static Spinner Large(this Spinner spinner)
        {
            spinner.Size = Spinner.CircleSize.Large;
            return spinner;
        }

        public static Spinner Text(this Spinner spinner, string text)
        {
            spinner.Text = text;
            return spinner;
        }
    }
}
