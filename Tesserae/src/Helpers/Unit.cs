using H5;

namespace Tesserae
{
    /// <summary>
    /// Specifies the units for size measurements (CSS units).
    /// </summary>
    [Enum(Emit.StringName)]
    [H5.Name("tss.Unit")]
    public enum Unit
    {
        /// <summary>No unit specified.</summary>
        Default,

        /// <summary>Automatic sizing.</summary>
        [Name("auto")]
        Auto,

        /// <summary>Percentage unit (%).</summary>
        [Name("%")]
        Percent,

        /// <summary>Pixel unit (px).</summary>
        [Name("px")]
        Pixels,

        /// <summary>Viewport Height unit (vh).</summary>
        [Name("vh")]
        ViewportHeight,

        /// <summary>Viewport Width unit (vw).</summary>
        [Name("vw")]
        ViewportWidth,

        /// <summary>Inherit from parent.</summary>
        [Name("inherit")]
        Inherit,

        /// <summary>Fractional unit (fr), used in grid layouts.</summary>
        [Name("fr")]
        FR,
    }
}