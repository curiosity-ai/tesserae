using H5;

namespace Tesserae
{
    /// <summary>
    /// Specifies the animation types for tooltips.
    /// </summary>
    [Enum(Emit.StringName)]
    [H5.Name("tss.TooltipAnimation")]
    public enum TooltipAnimation
    {
        /// <summary>No animation.</summary>
        [Name("none")]                None,
        /// <summary>Shifts away subtly.</summary>
        [Name("shift-away-subtle")]   ShiftAway,
        /// <summary>Shifts toward subtly.</summary>
        [Name("shift-toward-subtle")] ShiftToward,
        /// <summary>Scales subtly.</summary>
        [Name("scale-subtle")]        Scale,
        /// <summary>Perspective change subtly.</summary>
        [Name("perspective-subtle")]  Perspective
    }

    /// <summary>
    /// Specifies the placement of tooltips relative to their target element.
    /// </summary>
    [Enum(Emit.StringName)]
    [H5.Name("tss.TooltipPlacement")]
    public enum TooltipPlacement
    {
        [Name("top")]          Top,
        [Name("top-start")]    TopStart,
        [Name("top-end")]      TopEnd,
        [Name("right")]        Right,
        [Name("right-start")]  RightStart,
        [Name("right-end")]    RightEnd,
        [Name("bottom")]       Bottom,
        [Name("bottom-start")] BottomStart,
        [Name("bottom-end")]   BottomEnd,
        [Name("left")]         Left,
        [Name("left-start")]   LeftStart,
        [Name("left-end")]     LeftEnd,
        [Name("auto")]         Auto,
        [Name("auto-start")]   AutoStart,
        [Name("auto-end")]     AutoEnd,
    }
}