using Tesserae.Helpers.HTML;

namespace Tesserae.Components
{
    public static class IntExtensions
    {
        public static WidthDimension ToPercentageWidth(this int value)
        {
            return new WidthDimension(value, DimensionUnit.Percentage);
        }

        public static WidthDimension ToPixelWidth(this int value)
        {
            return new WidthDimension(value, DimensionUnit.Pixels);
        }

        public static HeightDimension ToPercentageHeight(this int value)
        {
            return new HeightDimension(value, DimensionUnit.Percentage);
        }

        public static HeightDimension ToPixelHeight(this int value)
        {
            return new HeightDimension(value, DimensionUnit.Pixels);
        }
    }
}
