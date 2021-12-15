using H5;

namespace Tesserae
{
    public class WeakRef<T> where T : class
    {
        [Template("new WeakRef({data})")]
        public extern WeakRef(T data);

        [Template("{this}.deref()")]
        public extern T Deref();
    }
}