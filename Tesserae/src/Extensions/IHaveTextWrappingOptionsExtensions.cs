namespace Tesserae
{
    [H5.Name("tss.IHTWOX")]
    public static class IHaveTextWrappingOptionsExtensions
    {
        public static T SetCanWrap<T>(this T component, bool canWrap) where T : ICanWrap
        {
            component.CanWrap = canWrap;
            return component;
        }
    }
}