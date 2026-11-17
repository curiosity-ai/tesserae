namespace Tesserae
{
    /// <summary>
    /// Where the address bar is: the path of the hash, without its query string. Asking about it never reads the query, so
    /// <c>#/ab/c/de/ef?x=1</c> is exactly <c>#/ab/c/de/ef</c>. <see cref="RouteQuery"/> is the other half.
    /// </summary>
    /// <remarks>
    /// <para>Reads come from <see cref="Router.CurrentHash"/>, so they work before the first route has matched.</para>
    /// <para>Paths are compared by segment, ignoring case, empty segments and a trailing <c>/</c> - the way the router matches a route.
    /// A leading <c>#</c> or <c>/</c> on a route you pass in does not matter: <c>#/ab/c</c>, <c>/ab/c</c> and <c>ab/c</c> are the same.</para>
    /// </remarks>
    [Transpose.Name("tss.RoutePath")]
    public static class RoutePath
    {
        /// <summary>The path of the address bar, as <c>#/ab/c/de/ef</c>: no query, no trailing <c>/</c>, <c>#/</c> when there is none. Case is kept.</summary>
        public static string Current => new RouteLocation(Router.CurrentHash).Path;

        /// <summary>True when the path is exactly <paramref name="route"/>: <c>#/ab/c/de/ef</c> is exactly <c>#/ab/c/de/ef</c> and nothing else.</summary>
        public static bool IsExactly(string route) => new RouteLocation(Router.CurrentHash).IsExactly(route);

        /// <summary>
        /// True when the path is below <paramref name="route"/>, at any depth, and not <paramref name="route"/> itself: <c>#/ab/c/de/ef</c>
        /// is a descendant of <c>#/ab/c</c>, <c>#/ab/c</c> and <c>#/ab/cx</c> are not. For "on this page or below it", ask both.
        /// </summary>
        public static bool IsDescendantOf(string route) => new RouteLocation(Router.CurrentHash).IsDescendantOf(route);

        /// <summary>
        /// Moves the address bar to <paramref name="route"/>, keeping the query, in place of the current entry. The same view stays on screen,
        /// no handler runs: for a page that finds its canonical address once it knows what it shows. Does nothing, and answers true, when the
        /// path is already <paramref name="route"/>. False when an <see cref="Router.OnBeforeNavigate"/> guard refused.
        /// </summary>
        public static bool ReplacePath(string route)
        {
            var current = new RouteLocation(Router.CurrentHash);

            return current.IsExactly(route) || Router.Replace(new RouteLocation(route).Path + current.Query.ToQueryString());
        }
    }
}
