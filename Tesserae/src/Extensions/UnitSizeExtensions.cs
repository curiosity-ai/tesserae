using System;

namespace Tesserae
{
#pragma warning disable IDE1006 // Naming Styles (2020-06-30 DWR: We'll allow these as special naming violation cases because we've decided that we like how look in the calling code)
    [H5.Name("tss.usX")]
    public static class UnitSizeExtensions
    {
        public static UnitSize px(this int value)
        {
            if (value > 0 && value < 32) return UnitSize.FromPixelCache(value);
            return new UnitSize(value, Unit.Pixels);
        }

        public static UnitSize fr(this int value) => new UnitSize(value, Unit.FractionalUnit);
        public static UnitSize percent(this int value) => new UnitSize(value, Unit.Percent);
        public static UnitSize vh(this int value) => new UnitSize(value, Unit.ViewportHeight);
        public static UnitSize vw(this int value) => new UnitSize(value, Unit.ViewportWidth);

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
