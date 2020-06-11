namespace Tesserae
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Depending upon the use case, the code may be clearer if this extension method or used or it may be clearer to rely upon and explicit or implicit cast from array to ReadOnlyArray
        /// </summary>
        public static ReadOnlyArray<T> AsReadOnlyArray<T>(this T[] source) => (ReadOnlyArray<T>)source;
    }
}
