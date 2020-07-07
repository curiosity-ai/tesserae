using System;

namespace Tesserae.Components
{
#pragma warning disable IDE1006 // Naming Styles (2020-06-30 DWR: We'll allow these as special naming violation cases because we've decided that we like how look in the calling code)
    public static class UnitSizeExtensions
    {
        public static UnitSize percent(this int value) => new UnitSize(value, Unit.Percent).Cache(value);
        public static UnitSize px(this int value) => new UnitSize(value, Unit.Pixels).Cache(value);
        public static UnitSize vh(this int value) => new UnitSize(value, Unit.ViewportHeight).Cache(value);
        public static UnitSize vw(this int value) => new UnitSize(value, Unit.ViewportWidth).Cache(value);

        public static UnitSize percent(this double value) => new UnitSize(value.As<float>(), Unit.Percent);
        public static UnitSize px(this double value) => new UnitSize(value.As<float>(), Unit.Pixels);
        public static UnitSize vh(this double value) => new UnitSize(value.As<float>(), Unit.ViewportHeight);
        public static UnitSize vw(this double value) => new UnitSize(value.As<float>(), Unit.ViewportWidth);

        public static UnitSize percent(this float value) => new UnitSize(value, Unit.Percent);
        public static UnitSize px(this float value) => new UnitSize(value, Unit.Pixels);
        public static UnitSize vh(this float value) => new UnitSize(value, Unit.ViewportHeight);
        public static UnitSize vw(this float value) => new UnitSize(value, Unit.ViewportWidth);
    }
#pragma warning restore IDE1006 // Naming Styles
}
