using Bridge;

namespace Tesserae
{
    [Enum(Emit.Value)]
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
        ViewportWidth
    }
}
