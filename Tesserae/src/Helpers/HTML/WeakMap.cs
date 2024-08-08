using H5;

namespace Tesserae
{
    [H5.Name("tss.WeakMap")]
    public class WeakMap
    {
        private object _map;

        public WeakMap()
        {
            _map = Script.Write<object>("new WeakMap()");
        }

        public object Get(object key)
        {
            return Script.Write<object>("{0}.get({1}, {2})", _map, key);
        }

        public void Set(object key, object value)
        {
            Script.Write("{0}.set({1}, {2})", _map, key, value);
        }

        public bool Has(object key)
        {
            return Script.Write<bool>("{0}.has({1})", _map, key);
        }

        public void Delete(object key)
        {
            Script.Write("{0}.delete({1})", _map, key);
        }
    }
}