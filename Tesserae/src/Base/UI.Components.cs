using Tesserae.Components;
using static Retyped.dom;

namespace Tesserae
{
    public static partial class UI
    {
        /// <summary>
        /// Helper method to capture the current component inline on it's definition, as an out variable
        /// </summary>
        /// <typeparam name="T">Any component implementing <see cref="IComponent"/></typeparam>
        /// <param name="component"></param>
        /// <param name="var">Capture variable</param>
        /// <returns>itself</returns>
        public static T Var<T>(this T component, out T var) where T : IComponent
        {
            var = component;
            return component;
        }

        public static Stack Stack(StackOrientation orientation = StackOrientation.Vertical)
        {
            return new Stack(orientation);
        }

        public static Raw Raw(HTMLElement element)
        {
            return new Raw(element);
        }

        public static SectionStack SectionStack()
        {
            return new SectionStack();
        }
        public static Button Button(string text = string.Empty)
        {
            return new Button(text);
        }

        public static CheckBox CheckBox(string text = string.Empty)
        {
            return new CheckBox(text);
        }

        public static Toggle Toggle(string text = string.Empty)
        {
            return new Toggle(text);
        }

        public static Option Option(string label = string.Empty)
        {
            return new Option(label);
        }

        public static ChoiceGroup ChoiceGroup(string label = "Pick one")
        {
            return new ChoiceGroup(label);
        }

        public static TextBlock TextBlock(string text = string.Empty)
        {
            return new TextBlock(text);
        }

        public static Label Label(string text = string.Empty)
        {
            return new Label(text);
        }

        public static TextBox TextBox(string text = string.Empty)
        {
            return new TextBox(text);
        }

        public static Slider Slider(int val = 0, int min = 0, int max = 100, int step = 10)
        {
            return new Slider(val, min, max, step);
        }

        public static Layer Layer()
        {
            return new Layer();
        }

        public static LayerHost LayerHost()
        {
            return new LayerHost();
        }

        public static Nav Nav()
        {
            return new Nav();
        }

        public static NavLink NavLink(string text = null, string icon = null)
        {
            return new NavLink(text, icon);
        }

        public static Panel Panel()
        {
            return new Panel();
        }

        public static Modal Modal(string header = string.Empty)
        {
            return new Modal(header);
        }

        public static Dialog Dialog(string header = string.Empty)
        {
            return new Dialog(header);
        }

        public static Pivot Pivot()
        {
            return new Pivot();
        }
    }
}
