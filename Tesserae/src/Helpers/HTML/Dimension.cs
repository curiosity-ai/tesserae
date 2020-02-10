using System;

namespace Tesserae.Helpers.HTML
{
    public class Dimension
    {
        public Dimension(int dimensionValue, DimensionUnit dimensionUnit)
        {
            if (dimensionUnit == DimensionUnit.Default)
            {
                throw new ArgumentException(nameof(dimensionUnit));
            }

            Value = dimensionValue;
            Unit  = dimensionUnit;
        }

        public int Value          { get; }

        public DimensionUnit Unit { get; }

        public override string ToString()
        {
            var suffix = string.Empty;

            if (Unit == DimensionUnit.Percentage)
            {
                suffix = "%";
            }
            else if (Unit == DimensionUnit.Pixels)
            {
                suffix = "px";
            }

            return $"{Value}{suffix}";
        }
    }
}
