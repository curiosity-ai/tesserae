using Transpose;

namespace Tesserae
{
    /// <summary>
    /// Defines the weights available for UIcons.
    /// </summary>
    [Enum(Emit.StringName)]
    [Transpose.Name("tss.uiconweight")]
    public enum UIconsWeight
    {
        [Name("fi-rr-")]
        Regular,

        [Name("fi-sr-")]
        Solid,

        [Name("fi-br-")]
        Bold,

        [Name("fi-tr-")]
        Thin,

        [Name("fi-rs-")]
        RegularStraight,

        [Name("fi-ss-")]
        SolidStraight,

        [Name("fi-bs-")]
        BoldStraight,

        [Name("fi-ts-")]
        ThinStraight
    }
}