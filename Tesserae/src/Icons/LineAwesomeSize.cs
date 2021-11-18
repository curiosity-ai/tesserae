using H5;

namespace Tesserae
{
    [Enum(Emit.Value)]
    [H5.Name("tss.LineAwesomeWeight")]
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
}
