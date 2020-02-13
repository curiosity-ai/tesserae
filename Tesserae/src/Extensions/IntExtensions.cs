namespace Tesserae.Components
{
    public static class IntExtensions
    {
        public static UnitSize percent(this int value)  => ((double)value).percent();

        public static UnitSize px(this int value)       => ((double)value).px();

        public static UnitSize viewport(this int value) => ((double)value).viewport();
    }
}