using System;
using System.Collections.Generic;

namespace Tesserae
{
    public sealed class UnitSize
    {
        public UnitSize(float size, Unit unit)
        {
            if (unit == Unit.Default)
            {
                throw new ArgumentException(nameof(unit));
            }

            Size = size;
            Unit = unit;
        }

        internal UnitSize Cache(int val)
        {
            if(!_cache.TryGetValue(Unit, out var dict))
            {
                dict = new Dictionary<int, string>();
                _cache[Unit] = dict;
            }

            if(dict.TryGetValue(val, out var s))
            {
                _cachedValue = s;
            }
            else
            {
                _cachedValue = ToString();
                dict[val] = _cachedValue;
            }

            return this;
        }

        private static readonly Dictionary<Unit, Dictionary<int, string>> _cache = new Dictionary<Unit, Dictionary<int, string>>();

        private string _cachedValue = null;

        private UnitSize()            => Unit = Unit.Auto;

        public static UnitSize Auto() => new UnitSize();
        public static UnitSize Inherit() => new UnitSize() { Unit = Unit.Inherit };

        public float Size            { get; private set; }

        public Unit Unit              { get; private set; }

        public override string ToString()
        {
            if (_cachedValue != null ) return _cachedValue;

            switch (Unit)
            {
                case Unit.Default:
                    throw new ArgumentException(nameof(Unit));
                case Unit.Auto:
                    return "auto";
                case Unit.Inherit:
                    return "inherit";
                case Unit.Percent:
                    _cachedValue = $"{Size:0.####}%"; break;
                case Unit.Pixels:
                    _cachedValue = $"{Size:0.####}px"; break;
                case Unit.ViewportHeight:
                    _cachedValue = $"{Size:0.####}vh"; break;
                case Unit.ViewportWidth:
                    _cachedValue = $"{Size:0.####}vw"; break;
            }

            return _cachedValue;
        }

        public static UnitSize operator -(UnitSize a) => new UnitSize(-a.Size, a.Unit);
        public static UnitSize operator +(UnitSize a) => new UnitSize(a.Size, a.Unit);

        //TODO: add other operators so we can perform math on UnitSizes and have it convert to calc(...)
        //     will need to remove Size and Unit properties or add a way to limit their usage for only "pure" units (without calc)
    }
}
