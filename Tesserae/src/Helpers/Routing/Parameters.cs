using System.Collections.Generic;
using System.Linq;

namespace Tesserae
{
    [H5.Name("tss.Parameters")]
    public sealed class Parameters
    {
        private readonly Dictionary<string, string> _parameters;
        public Parameters(Dictionary<string, string> parameters) => _parameters = parameters;

        public new string this[string key] => _parameters[key];

        public IEnumerable<string> Keys   => _parameters.Keys;
        public IEnumerable<string> Values => _parameters.Values;

        public int Count => _parameters.Count;

        public bool ContainsKey(string key)                   => _parameters.ContainsKey(key);
        public bool TryGetValue(string key, out string value) => _parameters.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _parameters.GetEnumerator();

        public Parameters With(string key, string value) 
        {
            _parameters[key] = value;
            return this;
        }
        
        public Parameters Remove(string key) 
        {
            _parameters.Remove(key);
            return this;
        }

        public string     ToQueryString() => _parameters.Any() ? "?" + string.Join("&", _parameters.Select(p => p.Key + "=" + p.Value)) : "";
        public Parameters Clone()         => new Parameters(_parameters);
    }
}