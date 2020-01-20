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

        /// <summary>
        /// Creates a wrapper IComponent from an HTML element
        /// </summary>
        /// <param name="element">HTML element to be wrapped</param>
        /// <returns></returns>
        public static Raw Raw(HTMLElement element) => new Raw(element);

        public static Stack Stack(Stack.Orientation orientation = Components.Stack.Orientation.Vertical) => new Stack(orientation);

        public static SectionStack SectionStack() => new SectionStack();

        public static Button Button(string text = string.Empty) => new Button(text);

        public static CheckBox CheckBox(string text = string.Empty) => new CheckBox(text);

        public static Toggle Toggle(string text = string.Empty) => new Toggle(text);
        
        public static Toggle Toggle(string onText, string offText) => new Toggle(onText: onText, offText: offText);

        public static Option Option(string label = string.Empty) => new Option(label);

        public static ChoiceGroup ChoiceGroup(string label = "Pick one") => new ChoiceGroup(label);

        public static TextBlock TextBlock(string text = string.Empty) => new TextBlock(text);

        public static Label Label(string text = string.Empty) => new Label(text);

        public static TextBox TextBox(string text = string.Empty) => new TextBox(text);

        public static Slider Slider(int val = 0, int min = 0, int max = 100, int step = 10) => new Slider(val, min, max, step);

        public static Layer Layer() => new Layer();

        public static LayerHost LayerHost() => new LayerHost();

        public static Nav Nav() => new Nav();

        public static NavLink NavLink(string text = null, string icon = null) => new NavLink(text, icon);

        public static Panel Panel() => new Panel();

        public static Modal Modal(string header = string.Empty) => new Modal(header);

        public static Dialog Dialog(string header = string.Empty) => new Dialog(header);

        public static Pivot Pivot() => new Pivot();
    }
}
