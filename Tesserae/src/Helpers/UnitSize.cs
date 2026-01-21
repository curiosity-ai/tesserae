using H5;
using System;
using System.Collections.Generic;

namespace Tesserae
{
    /// <summary>
    /// Represents a size measurement with a value and a unit.
    /// </summary>
    [H5.Name("tss.us")]
    public sealed class UnitSize
    {
        private static UnitSize[] _cachedIntegers = CreateCache(32);

        /// <summary>Gets a UnitSize instance from the pixel cache.</summary>
        public static UnitSize FromPixelCache(int value) => _cachedIntegers[value];
        private static UnitSize[] CreateCache(int count)
        {
            var us = new UnitSize[count];

            for (int i = 0; i < us.Length; i++)
            {
                us[i] = new UnitSize(i, Unit.Pixels);
            }
            return us;
        }

        /// <summary>
        /// Initializes a new instance of the UnitSize class.
        /// </summary>
        /// <param name="size">The numeric size value.</param>
        /// <param name="unit">The unit of measurement.</param>
        public UnitSize(float size, Unit unit)
        {
            if (unit == Unit.Default)
            {
                throw new ArgumentException(nameof(unit));
            }

            Size = size;
            Unit = unit;
        }

        /// <summary>
        /// Initializes a new instance of the UnitSize class with a custom CSS string.
        /// </summary>
        /// <param name="custom">The custom CSS string.</param>
        public UnitSize(string custom)
        {
            Size         = -1;
            Unit         = Unit.Default;
            _cachedValue = custom;
        }

        private string _cachedValue = null;

        private UnitSize() => Unit = Unit.Auto;

        /// <summary>Returns a UnitSize instance representing 'auto'.</summary>
        public static UnitSize Auto()    => new UnitSize();
        /// <summary>Returns a UnitSize instance representing 'fit-content'.</summary>
        public static UnitSize FitContent()    => new UnitSize("fit-content");
        /// <summary>Returns a UnitSize instance representing 'inherit'.</summary>
        public static UnitSize Inherit() => new UnitSize() { Unit = Unit.Inherit };

        /// <summary>Gets the numeric size value.</summary>
        public float Size { get; private set; }

        /// <summary>Gets the unit of measurement.</summary>
        public Unit Unit { get; private set; }

        /// <summary>Returns the CSS string representation of the UnitSize.</summary>
        public override string ToString()
        {
            if (_cachedValue.As<bool>()) return _cachedValue; //Compile to a faster check than string.IsNullOrEmpty in javascript

            switch (Unit)
            {
                case Unit.Default:
                    throw new ArgumentException(nameof(Unit));
                case Unit.Auto:
                    return "auto";
                case Unit.Inherit:
                    return "inherit";
                case Unit.Percent:
                    _cachedValue = Script.Write<string>("{0} + '%'", Size);
                    break;
                case Unit.Pixels:
                    _cachedValue = Script.Write<string>("{0} + 'px'", Size);
                    break;
                case Unit.ViewportHeight:
                    _cachedValue = Script.Write<string>("{0} + 'vh'", Size);
                    break;
                case Unit.ViewportWidth:
                    _cachedValue = Script.Write<string>("{0} + 'vw'", Size);
                    break;
                case Unit.FR:
                    _cachedValue = Script.Write<string>("{0} + 'fr'", Size);
                    break;
            }

            return _cachedValue;
        }

        public static UnitSize operator -(UnitSize a) => new UnitSize(-a.Size, a.Unit);
        public static UnitSize operator +(UnitSize a) => new UnitSize(a.Size,  a.Unit);

        //TODO: add other operators so we can perform math on UnitSizes and have it convert to calc(...)
        //     will need to remove Size and Unit properties or add a way to limit their usage for only "pure" units (without calc)
    }
}