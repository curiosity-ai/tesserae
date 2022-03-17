using H5;
using System;
using System.Collections.Generic;

namespace Tesserae
{
    [H5.Name("tss.us")]
    public sealed class UnitSize
    {
        private static UnitSize[] _cachedIntegers = CreateCache(32);

        public static UnitSize FromPixelCache(int value) => _cachedIntegers[value];
        private static UnitSize[] CreateCache(int count)
        {
            var us = new UnitSize[count];
            for(int i = 0; i < us.Length; i++)
            {
                us[i] = new UnitSize(i, Unit.Pixels);
            }
            return us;
        }

        public UnitSize(float size, Unit unit)
        {
            if (unit == Unit.Default)
            {
                throw new ArgumentException(nameof(unit));
            }

            Size = size;
            Unit = unit;
        }

        private string _cachedValue = null;

        private UnitSize()            => Unit = Unit.Auto;

        public static UnitSize Auto() => new UnitSize();
        public static UnitSize Inherit() => new UnitSize() { Unit = Unit.Inherit };

        public float Size            { get; private set; }

        public Unit Unit              { get; private set; }

        public override string ToString()
        {
            if (Script.Write<bool>("{0}", _cachedValue)) return _cachedValue;

            switch (Unit)
            {
                case Unit.Default:
                    throw new ArgumentException(nameof(Unit));
                case Unit.Auto:
                    return "auto";
                case Unit.Inherit:
                    return "inherit";
                case Unit.Percent:
                    _cachedValue = Script.Write<string>("{0} + '%'", Size); break;
                case Unit.Pixels:
                    _cachedValue = Script.Write<string>("{0} + 'px'", Size); break;
                case Unit.ViewportHeight:
                    _cachedValue = Script.Write<string>("{0} + 'vh'", Size); break;
                case Unit.ViewportWidth:
                    _cachedValue = Script.Write<string>("{0} + 'vw'", Size); break;
                case Unit.FractionalUnit:
                    _cachedValue = Script.Write<string>("{0} + 'fr'", Size); break;
            }

            return _cachedValue;
        }

        public static UnitSize operator -(UnitSize a) => new UnitSize(-a.Size, a.Unit);
        public static UnitSize operator +(UnitSize a) => new UnitSize(a.Size, a.Unit);

        //TODO: add other operators so we can perform math on UnitSizes and have it convert to calc(...)
        //     will need to remove Size and Unit properties or add a way to limit their usage for only "pure" units (without calc)
    }
}
