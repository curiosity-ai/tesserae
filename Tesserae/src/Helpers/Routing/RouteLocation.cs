using System;
using System.Collections.Generic;
using System.Linq;
using Transpose;

namespace Tesserae
{
    /// <summary>
    /// A hash such as <c>#/spaces/app?uid=U&amp;preview=a,b</c>, read the way the <see cref="Router"/> reads it. It is a value built
    /// from a string: it does not look at the browser or at the router, so it can be made from any hash, before the first route
    /// has matched, and in a test. <see cref="RoutePath"/> and <see cref="RouteQuery"/> are the same reads for the address bar.
    /// </summary>
    /// <remarks>
    /// <para>The path and the query are read independently: the query is parsed the first time a key is asked for, so a question
    /// about the path never depends on it.</para>
    /// <para>Paths are compared by segment, ignoring case, empty segments and a trailing <c>/</c> - the way the router matches a route.
    /// Query keys and values are case-sensitive. A route <c>:variable</c> is part of the path, not of the query.</para>
    /// <para>A malformed percent escape (<c>?a=%</c>) never throws: the value is kept as it was written.</para>
    /// </remarks>
    [Transpose.Name("tss.RouteLocation")]
    public sealed class RouteLocation
    {
        private readonly string[]                   _segments;
        private readonly string                     _queryText;
        private          Dictionary<string, string> _query;

        public RouteLocation(string hash)
        {
            Hash = hash ?? "";

            var text       = Hash.TrimStart('#');
            var queryStart = text.IndexOf('?');
            var path       = queryStart >= 0 ? text.Substring(0, queryStart) : text;

            _segments  = SegmentsOf(path);
            _queryText = queryStart >= 0 ? text.Substring(queryStart + 1) : "";
            Path       = "#/" + string.Join("/", _segments);
        }

        /// <summary>The hash this was made from, exactly as given.</summary>
        public string Hash { get; }

        /// <summary>The path alone, as <c>#/a/b</c>: no query, no trailing <c>/</c>, <c>#/</c> when there is none. Case is kept.</summary>
        public string Path { get; }

        /// <summary>A copy of the query string's keys and values.</summary>
        internal Parameters Query => new Parameters(new Dictionary<string, string>(QueryPairs));

        /// <summary>True when the path is exactly <paramref name="route"/>. The query does not matter.</summary>
        public bool IsExactly(string route) => SameSegments(_segments, new RouteLocation(route)._segments);

        /// <summary>
        /// True when the path is below <paramref name="route"/>, at any depth, on a segment boundary: <c>#/a/b/c</c> is a descendant of
        /// <c>#/a</c>, <c>#/a</c> and <c>#/ab</c> are not. Every path but the root is a descendant of <c>#/</c>.
        /// </summary>
        public bool IsDescendantOf(string route)
        {
            var routeSegments = new RouteLocation(route)._segments;

            return routeSegments.Length < _segments.Length && SameSegments(_segments.Take(routeSegments.Length), routeSegments);
        }

        /// <summary>True when the key is in the query, whatever its value (<c>?x</c> and <c>?x=</c> give an empty one).</summary>
        public bool TryGet(string key, out string value) => QueryPairs.TryGetValue(key, out value);

        /// <summary>The value of the key, <c>""</c> for <c>?x</c> and <c>?x=</c>, null when the key is not in the query.</summary>
        public string Get(string key) => QueryPairs.TryGetValue(key, out var value) ? value : null;

        private Dictionary<string, string> QueryPairs
        {
            get
            {
                if (_query is null)
                {
                    _query = new Dictionary<string, string>();

                    ParseQueryInto(_queryText, _query);
                }

                return _query;
            }
        }

        // the way the router compares a route's segments (StringComparer has no such member under Transpose)
        private static bool SameSegments(IEnumerable<string> a, IEnumerable<string> b)
        {
            return a.Count() == b.Count() && a.Zip(b, (x, y) => string.Equals(x, y, StringComparison.InvariantCultureIgnoreCase)).All(same => same);
        }

        private static string[] SegmentsOf(string path) => (path ?? "").Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>The query string's pairs into <paramref name="par"/>: split on <c>&amp;</c>, then on the first <c>=</c> only, so a value may contain one. The router parses with this too.</summary>
        internal static void ParseQueryInto(string query, Dictionary<string, string> par)
        {
            foreach (var queryPart in query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var eq = queryPart.IndexOf('=');

                if (eq < 0)
                {
                    par[Decode(queryPart)] = "";
                }
                else if (eq > 0)
                {
                    par[Decode(queryPart.Substring(0, eq))] = Decode(queryPart.Substring(eq + 1));
                }
            }
        }

        // decodeURIComponent throws on a stray '%', and an address bar can hold one: it is kept as written rather than failing the page
        private static string Decode(string value) => Script.Write<string>("(function(v){try{return decodeURIComponent(v);}catch(e){return v;}})({0})", value);
    }
}
