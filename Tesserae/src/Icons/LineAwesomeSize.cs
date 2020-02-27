using Bridge;

namespace Tesserae
{
    [Enum(Emit.Value)]
    public enum LineAwesomeWeight
    {
        [Name("la")]
        Default,

        [Name("lal")]
        Light,

        [Name("lar")]
        Regular,

        [Name("las")]
        Solid,

        [Name("lab")]
        Brand
    }

    [Enum(Emit.Value)]
    public enum LineAwesomeSize
    {
        [Name("la-1x")] x1,
        [Name("la-2x")] x2,
        [Name("la-3x")] x3,
        [Name("la-4x")] x4,
        [Name("la-5x")] x5,
        [Name("la-6x")] x6,
        [Name("la-7x")] x7,
        [Name("la-8x")] x8,
        [Name("la-9x")] x9,
        [Name("la-10x")] x10
    }
}
