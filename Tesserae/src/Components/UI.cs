namespace Tesserae.Components
{
    public static class UI
    {
        public static Stack Stack(StackOrientation orientation = StackOrientation.Vertical)
        {
            return new Stack(orientation);
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

        public static TextBlock TextBlock(string text = string.Empty)
        {
            return new TextBlock(text);
        }

        public static TextBox TextBox(string text = string.Empty)
        {
            return new TextBox(text);
        }
    }
}
