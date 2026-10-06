using System.Collections.Generic;
using System.Linq;

namespace Tesserae
{
    /// <summary>
    /// A route's parameters: its <c>:variables</c> and its query string. Immutable: <see cref="With"/> and <see cref="Without"/> return a
    /// new instance and leave this one as it was, so a handler or a <see cref="Router.GetQueryParameters"/> caller holding one cannot change
    /// the router's state, or anyone else's copy, by accident. To change the URL, write through <see cref="RouteQuery"/> or
    /// <see cref="Router.ReplaceQueryParameters"/>.
    /// </summary>
    [Transpose.Name("tss.Parameters")]
    public sealed class Parameters
    {
        private readonly Dictionary<string, string> _parameters;
        public Parameters() => _parameters = new Dictionary<string, string>();

        // Copied, so the caller keeping the dictionary cannot change this instance afterwards
        public Parameters(Dictionary<string, string> parameters) => _parameters = parameters is null ? new Dictionary<string, string>() : new Dictionary<string, string>(parameters);

        public new string this[string key] => _parameters[key];

        public IEnumerable<string> Keys   => _parameters.Keys;
        public IEnumerable<string> Values => _parameters.Values;

        public int Count => _parameters.Count;

        public bool ContainsKey(string key)                   => _parameters.ContainsKey(key);
        public bool TryGetValue(string key, out string value) => _parameters.TryGetValue(key, out value);

        public bool SameAs(Parameters other)
        {
            if (other is null) return _parameters.Count == 0;
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

        /// <summary>A new instance with the key set to the value. This one is not changed.</summary>
        public Parameters With(string key, string value)
        {
            var copy = new Parameters(_parameters);
            copy._parameters[key] = value;
            return copy;
        }

        /// <summary>A new instance without the key, or this one when the key is not there. This one is not changed.</summary>
        public Parameters Without(string key)
        {
            if (!_parameters.ContainsKey(key)) return this;

            var copy = new Parameters(_parameters);
            copy._parameters.Remove(key);
            return copy;
        }

        // Kept only so that code which removed a key from its handler's parameters, expecting the URL to follow, fails to compile
        // instead of silently doing nothing.
        [System.Obsolete("Parameters is immutable. To remove a key from the URL use RouteQuery.Clear(key); inside Router.ReplaceQueryParameters or RouteQuery.Update use p.Without(key).", error: true)]
        public Parameters Remove(string key) => Without(key);

        public string     ToQueryString() => _parameters.Any() ? "?" + string.Join("&", _parameters.Select(p => Transpose.Script.EncodeURIComponent(p.Key) + "=" + Transpose.Script.EncodeURIComponent(p.Value))) : "";

        [System.Obsolete("Parameters is immutable, so a copy is the same as the original.")]
        public Parameters Clone()         => this;
    }
}