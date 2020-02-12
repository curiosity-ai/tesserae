using System;

namespace Tesserae
{
    public sealed class UnitSize
    {
        private const string AutoSuffix = "auto";

        public UnitSize(double size, Unit unit)
        {
            if (unit == Unit.Default)
            {
                throw new ArgumentException(nameof(unit));
            }

            Size = size;
            Unit = unit;
        }

        public UnitSize() => Unit = Unit.Auto;

        public static string Auto => AutoSuffix;

        public double Size { get; }

        public Unit Unit   { get; }

        [Obsolete("Replace call with .percent, .px or .vh extension methods available on the int and double types")]
        public static string Translate(Unit unit, double size) => new UnitSize(size, unit).ToString();

        public override string ToString()
        {
            string suffix;

            switch (Unit)
            {
                case Unit.Auto:
                    return AutoSuffix;
                case Unit.Percent:
                    suffix = "%";
                    break;
                case Unit.Pixels:
                    suffix = "px";
                    break;
                case Unit.Viewport:
                    suffix = "vh";
                    break;
                default:
                    throw new InvalidOperationException("Can not generate style for default Unit");
            }

            return $"{Size:0.####}{suffix}";
        }
    }
}
