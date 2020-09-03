using H5;

namespace Tesserae.Components
{
    [Enum(Emit.Value)]
    public enum TooltipPosition
    {
        [Name("left")] Left,
        [Name("right")] Right,
        [Name("top")] Top,
        [Name("bottom")] Bottom
    }
}
