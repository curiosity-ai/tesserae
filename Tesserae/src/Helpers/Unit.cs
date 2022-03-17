using H5;

namespace Tesserae
{
    [Enum(Emit.StringName)]
    [H5.Name("tss.Unit")]
    public enum Unit
    {
        Default,

        [Name("auto")]
        Auto,

        [Name("%")]
        Percent,

        [Name("px")]
        Pixels,

        [Name("vh")]
        ViewportHeight,

        [Name("vw")]
        ViewportWidth,

        [Name("inherit")]
        Inherit,
        
        [Name("fr")]
        FractionalUnit,
    }
}
