namespace Tesserae
{
    public static class IHaveTextWrappingOptionsExtensions
    {
        public static T SetCanWrap<T>(this T component, bool canWrap) where T : IHaveTextWrappingOptions
        {
            component.CanWrap = canWrap;
            return component;
        }
    }
}