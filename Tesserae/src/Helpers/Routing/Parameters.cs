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

        public bool SameAs(Parameters other)
        {
            if (other is null) return _parameters.Count > 0;
            if (other._parameters.Count != _parameters.Count) return false;
            if (other._parameters.Count == 0 && _parameters.Count == 0) return true;
            else
            {
                foreach (var key in _parameters)
                {
                    if (!other._parameters.TryGetValue(key.Key, out var val) || val != key.Value)
                    {
                        return false;
                    }
                }
                foreach (var key in other._parameters)
                {
                    if (!_parameters.TryGetValue(key.Key, out var val) || val != key.Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
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

        public string     ToQueryString() => _parameters.Any() ? "?" + string.Join("&", _parameters.Select(p => p.Key + "=" + H5.Script.EncodeURIComponent(p.Value))) : "";
        public Parameters Clone()         => new Parameters(_parameters);
    }
}