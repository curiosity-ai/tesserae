using System;

namespace Tesserae
{
    internal static class TypeExtensions
    {
        public static bool IsObservable(this Type source)
        {
            return typeof(IBaseObservable).IsAssignableFrom(source);
        }
    }
}