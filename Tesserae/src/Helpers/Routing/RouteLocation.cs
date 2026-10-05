using System;
using System.Collections.Generic;
using System.Linq;
using Transpose;

namespace Tesserae
{
    /// <summary>
    /// A hash such as <c>#/spaces/app?uid=U&amp;preview=a,b</c>, read the way the <see cref="Router"/> reads it. It is a value built
    /// from a string: it does not look at the browser or at the router, so it can be made from any hash, before the first route
    /// has matched, and in a test. <see cref="RouteState.Current"/> is the one for the address bar.
    /// </summary>
    /// <remarks>
    /// <para>Paths are compared by segment, ignoring case, empty segments and a trailing <c>/</c> - the way the router matches a route.
    /// Query keys and values are case-sensitive. A route <c>:variable</c> is part of the path, not of <see cref="Query"/>.</para>
    /// <para>A malformed percent escape (<c>?a=%</c>) never throws: the value is kept as it was written.</para>
    /// </remarks>
    [Transpose.Name("tss.RouteLocation")]
    public sealed class RouteLocation
    {
        private readonly string[]                   _segments;
        private readonly Dictionary<string, string> _query = new Dictionary<string, string>();

        public RouteLocation(string hash)
        {
            Hash = hash ?? "";

            var text       = Hash.TrimStart('#');
            var queryStart = text.IndexOf('?');
            var path       = queryStart >= 0 ? text.Substring(0, queryStart) : text;

            _segments = SegmentsOf(path);
            Path      = "#/" + string.Join("/", _segments);

            if (queryStart >= 0)
            {
                ParseQueryInto(text.Substring(queryStart + 1), _query);
            }
        }

        /// <summary>The hash this was made from, exactly as given.</summary>
        public string Hash { get; }

        /// <summary>The path alone, as <c>#/a/b</c>: no query, no trailing <c>/</c>, <c>#/</c> when there is none. Case is kept.</summary>
        public string Path { get; }

        /// <summary>A copy of the query string's keys and values.</summary>
        public Parameters Query => new Parameters(new Dictionary<string, string>(_query));

        /// <summary>The path of any hash or route, normalised like <see cref="Path"/>; a query on it is ignored.</summary>
        public static string PathOf(string hashOrRoute) => new RouteLocation(hashOrRoute).Path;

        /// <summary>
        /// True when the path is exactly <paramref name="route"/>. A query on the hash does not matter, except for the keys in
        /// <paramref name="childQueryKeys"/>: they name something that has its own place in the app (a calendar, a clipboard
        /// filter), so while one is present - with any value - the page belongs to that, not to <paramref name="route"/>.
        /// </summary>
        public bool IsOn(string route, params string[] childQueryKeys)
        {
            return _segments.SequenceEqual(new RouteLocation(route)._segments, StringComparer.OrdinalIgnoreCase) && childQueryKeys?.Any(_query.ContainsKey) != true;
        }

        /// <summary>True when the path is <paramref name="route"/> or below it, on a segment boundary: <c>#/a/b</c> is under <c>#/a</c>, <c>#/ab</c> is not. Every path is under <c>#/</c>.</summary>
        public bool IsUnder(string route)
        {
            var routeSegments = new RouteLocation(route)._segments;

            return routeSegments.Length <= _segments.Length && _segments.Take(routeSegments.Length).SequenceEqual(routeSegments, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// True when the path is the one in <paramref name="url"/> and every query pair in it is present here with the same value.
        /// Other keys, and the order of the keys, do not matter. <paramref name="childQueryKeys"/> work as in <see cref="IsOn"/>.
        /// </summary>
        public bool Matches(string url, params string[] childQueryKeys)
        {
            var wanted = new RouteLocation(url);

            return _segments.SequenceEqual(wanted._segments, StringComparer.OrdinalIgnoreCase)
                && childQueryKeys?.Any(_query.ContainsKey) != true
                && wanted._query.All(pair => _query.TryGetValue(pair.Key, out var value) && value == pair.Value);
        }

        /// <summary>
        /// Of <paramref name="routes"/>, the one the path is under that has the most segments - the most specific - or null when none is.
        /// Routes that name the same path give the same answer, so every item sharing one is selected together.
        /// </summary>
        public string Deepest(params string[] routes)
        {
            return routes.Where(route => route is object && IsUnder(route)).OrderByDescending(route => new RouteLocation(route)._segments.Length).FirstOrDefault();
        }

        /// <summary>
        /// True when <paramref name="text"/> appears anywhere in the hash - the path, a route variable, a query key or value - compared
        /// exactly, case included. For "is this node open here in any role", where an id can sit in several places.
        /// </summary>
        public bool Mentions(string text) => !string.IsNullOrEmpty(text) && Hash.IndexOf(text, StringComparison.Ordinal) >= 0;

        /// <summary>True when <paramref name="key"/> has a value that is not empty or whitespace.</summary>
        public bool Has(string key) => _query.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value);

        /// <summary>The value of the key, <c>""</c> for <c>?x</c> and <c>?x=</c>, null when the key is not in the query.</summary>
        public string Get(string key) => _query.TryGetValue(key, out var value) ? value : null;

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
