using H5;

namespace Tesserae
{
    [Enum(Emit.StringName)]
    [H5.Name("tss.TooltipAnimation")]
    public enum TooltipAnimation
    {
        [Name("none")] None,
        [Name("shift-away-subtle")] ShiftAway,
        [Name("shift-toward-subtle")] ShiftToward,
        [Name("scale-subtle")] Scale,
        [Name("perspective-subtle")] Perspective
    }

    [Enum(Emit.StringName)]
    [H5.Name("tss.TooltipPlacement")]
    public enum TooltipPlacement
    {
        [Name("top")] Top,
        [Name("top-start")] TopStart,
        [Name("top-end")] TopEnd,
        [Name("right")] Right,
        [Name("right-start")] RightStart,
        [Name("right-end")] RightEnd,
        [Name("bottom")] Bottom,
        [Name("bottom-start")] BottomStart,
        [Name("bottom-end")] BottomEnd,
        [Name("left")] Left,
        [Name("left-start")] LeftStart,
        [Name("left-end")] LeftEnd,
        [Name("auto")] Auto,
        [Name("auto-start")] AutoStart,
        [Name("auto-end")] AutoEnd,
    }
}
