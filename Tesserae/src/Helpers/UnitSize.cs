using H5;
using System;
using System.Collections.Generic;

namespace Tesserae
{
    /// <summary>
    /// Represents a CSS length value: a numeric size paired with a unit (px, %, vh, vw, fr, auto, inherit, fit-content),
    /// or an arbitrary CSS expression such as <c>calc(...)</c>, <c>var(--name)</c>, <c>clamp(...)</c>, etc.
    /// </summary>
    /// <remarks>
    /// Construct typed values via the extension helpers — <c>16.px()</c>, <c>50.percent()</c>, <c>100.vh()</c>,
    /// <c>10.vw()</c>, <c>1.fr()</c> — and use the static helpers <see cref="Auto"/>, <see cref="Inherit"/>,
    /// or <see cref="FitContent"/> for keyword values. For anything else (arithmetic across mixed units,
    /// CSS variables, <c>calc()</c>, <c>min/max/clamp</c>) use the <see cref="UnitSize(string)"/> constructor,
    /// which forwards the raw CSS verbatim.
    /// </remarks>
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
        /// Initializes a new instance of the <see cref="UnitSize"/> class using a raw CSS string value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use this overload to express any value that CSS accepts as a length but that is not covered by
        /// the typed unit helpers (<c>.px()</c>, <c>.percent()</c>, <c>.vh()</c>, <c>.vw()</c>, <c>.fr()</c>).
        /// The string is passed through to the DOM verbatim, so it must be valid CSS.
        /// </para>
        /// <para>
        /// The most common reason to reach for the string constructor is to perform <c>calc(...)</c>
        /// arithmetic that mixes units the typed API cannot otherwise express (e.g. mixing pixels and
        /// percentages, or subtracting a fixed offset from a viewport-relative size):
        /// </para>
        /// <code>
        /// // Fill the viewport height minus a 64px header
        /// stack.Height(new UnitSize("calc(100vh - 64px)"));
        ///
        /// // Half the parent width, minus a fixed gutter
        /// panel.Width(new UnitSize("calc(50% - 16px)"));
        ///
        /// // Clamp between a min and max
        /// card.Width(new UnitSize("clamp(280px, 40vw, 640px)"));
        ///
        /// // CSS variables and other functions are also fair game
        /// box.Height(new UnitSize("var(--sidebar-height, 48px)"));
        /// box.MinHeight(new UnitSize("max(120px, 10vh)"));
        /// </code>
        /// <para>
        /// Whenever the value can be expressed with the typed helpers prefer those instead, e.g.
        /// <c>16.px()</c>, <c>50.percent()</c>, <c>100.vh()</c>, <c>1.fr()</c> — they are slightly cheaper
        /// at runtime (the integer pixel cache short-circuits common cases) and read better at the call
        /// site. Reach for the string constructor only when you genuinely need <c>calc()</c>, CSS
        /// variables, <c>min()</c>/<c>max()</c>/<c>clamp()</c>, or other raw CSS expressions.
        /// </para>
        /// </remarks>
        /// <param name="custom">
        /// A raw CSS length expression. Examples: <c>"calc(100% - 32px)"</c>, <c>"var(--width)"</c>,
        /// <c>"clamp(200px, 30vw, 600px)"</c>, <c>"min(100%, 800px)"</c>.
        /// </param>
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