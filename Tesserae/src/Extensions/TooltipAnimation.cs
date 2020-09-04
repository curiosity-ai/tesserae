using H5;

namespace Tesserae.Components
{
    [Enum(Emit.Value)]
    public enum TooltipAnimation
    {
        [Name("none")] None,
        [Name("shift-away-subtle")] ShiftAway,
        [Name("shift-toward-subtle")] ShiftToward,
        [Name("scale-subtle")] Scale,
        [Name("perspective-subtle")] Perspective
    }
}
