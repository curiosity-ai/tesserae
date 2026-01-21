using System;

namespace Tesserae
{
    /// <summary>
    /// Provides extension methods for converting numeric types to UnitSize instances.
    /// </summary>
    #pragma warning disable IDE1006 // Naming Styles (2020-06-30 DWR: We'll allow these as special naming violation cases because we've decided that we like how look in the calling code)
    [H5.Name("tss.usX")]
    public static class UnitSizeExtensions
    {
        /// <summary>Converts an integer to UnitSize in pixels.</summary>
        public static UnitSize px(this int value)
        {
            if (value > 0 && value < 32) return UnitSize.FromPixelCache(value);
            return new UnitSize(value, Unit.Pixels);
        }

        /// <summary>Converts an integer to UnitSize in percentage.</summary>
        public static UnitSize percent(this int value) => new UnitSize(value, Unit.Percent);
        /// <summary>Converts an integer to UnitSize in viewport height.</summary>
        public static UnitSize vh(this      int value) => new UnitSize(value, Unit.ViewportHeight);
        /// <summary>Converts an integer to UnitSize in viewport width.</summary>
        public static UnitSize vw(this      int value) => new UnitSize(value, Unit.ViewportWidth);
        /// <summary>Converts an integer to UnitSize in fractional units.</summary>
        public static UnitSize fr(this      int value) => new UnitSize(value, Unit.FR);

        /// <summary>Converts a double to UnitSize in percentage.</summary>
        public static UnitSize percent(this double value) => new UnitSize(value.As<float>(), Unit.Percent);
        /// <summary>Converts a double to UnitSize in pixels.</summary>
        public static UnitSize px(this      double value) => new UnitSize(value.As<float>(), Unit.Pixels);
        /// <summary>Converts a double to UnitSize in viewport height.</summary>
        public static UnitSize vh(this      double value) => new UnitSize(value.As<float>(), Unit.ViewportHeight);
        /// <summary>Converts a double to UnitSize in viewport width.</summary>
        public static UnitSize vw(this      double value) => new UnitSize(value.As<float>(), Unit.ViewportWidth);
        /// <summary>Converts a double to UnitSize in fractional units.</summary>
        public static UnitSize fr(this      double value) => new UnitSize(value.As<float>(), Unit.FR);

        /// <summary>Converts a float to UnitSize in percentage.</summary>
        public static UnitSize percent(this float value) => new UnitSize(value, Unit.Percent);
        /// <summary>Converts a float to UnitSize in pixels.</summary>
        public static UnitSize px(this      float value) => new UnitSize(value, Unit.Pixels);
        /// <summary>Converts a float to UnitSize in viewport height.</summary>
        public static UnitSize vh(this      float value) => new UnitSize(value, Unit.ViewportHeight);
        /// <summary>Converts a float to UnitSize in viewport width.</summary>
        public static UnitSize vw(this      float value) => new UnitSize(value, Unit.ViewportWidth);
        /// <summary>Converts a float to UnitSize in fractional units.</summary>
        public static UnitSize fr(this      float value) => new UnitSize(value, Unit.FR);
    }
    #pragma warning restore IDE1006 // Naming Styles
}