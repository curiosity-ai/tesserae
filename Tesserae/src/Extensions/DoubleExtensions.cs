namespace Tesserae.Components
{
    public static class DoubleExtensions
    {
        public static UnitSize percent(this double value) => new UnitSize(value, Unit.Percent);

        public static UnitSize px(this double value)      => new UnitSize(value, Unit.Pixels);

        public static UnitSize vh(this double value) => new UnitSize(value, Unit.Viewport);
    }
}
