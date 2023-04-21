using H5;

namespace Tesserae
{
    [Enum(Emit.StringName)]
    [H5.Name("tss.uiconweight")]
    public enum UIconsWeight
    {
        [Name("ff-rr-")]
        Regular,
        
        [Name("ff-sr-")]
        Solid,
        
        [Name("ff-br-")]
        Bold
    }
}