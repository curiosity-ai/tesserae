using H5;

namespace Tesserae
{
    [External]
    public class WeakRef<T>
    {
        [Template("new WeakRef({data})")]
        public extern WeakRef(T data);

        [Template("{this}.deref()")]
        public extern T Deref();
    }
}
