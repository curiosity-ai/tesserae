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

        public override string ToString() => $"{Size:0.####}{Unit}";
    }
}
