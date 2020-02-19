using Bridge;

namespace Tesserae
{
    [Enum(Emit.Value)]
    public enum LineAwesomeSize
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
}
