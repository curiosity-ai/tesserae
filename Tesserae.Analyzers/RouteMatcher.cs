using System;

namespace Tesserae.Analyzers
{
    /// <summary>
    /// Compile-time mirror of the runtime route matching in Tesserae's Router
    /// (Router.Register normalization + Route/RoutePart.IsMatch): leading '#' and '/'
    /// are irrelevant, matching is per-segment and case-insensitive, and a pattern
    /// segment starting with ':' captures any value.
    /// </summary>
    internal static class RouteMatcher
    {
        /// <summary>Turns a path passed to Router.Register into its matchable segments.</summary>
        public static string[] ParsePattern(string registeredPath)
        {
            var path = registeredPath.Trim();

            if (path.StartsWith("#", StringComparison.Ordinal)) path = path.TrimStart('#');

            return SplitSegments(path);
        }

        /// <summary>
        /// Turns a path passed to Router.Navigate into its matchable segments. Returns false
        /// for absolute/external URLs, which never go through the route table.
        /// </summary>
        public static bool TryGetNavigationParts(string navigatePath, out string[] parts)
        {
            parts = null;

            var path = navigatePath.Trim();

            if (path.Contains("://") || path.StartsWith("//", StringComparison.Ordinal))
            {
                return false;
            }

            // Routes are matched against window.location.hash, so only the fragment matters when one is present
            var hashIndex = path.IndexOf('#');

            if (hashIndex >= 0) path = path.Substring(hashIndex + 1);

            // Query-string parameters are parsed separately by the Router and play no part in route matching
            var queryIndex = path.IndexOf('?');

            if (queryIndex >= 0) path = path.Substring(0, queryIndex);

            parts = SplitSegments(path);
            return true;
        }

        public static bool IsMatch(string[] patternParts, string[] navigationParts)
        {
            if (patternParts.Length != navigationParts.Length) return false;

            for (var i = 0; i < patternParts.Length; i++)
            {
                var patternPart = patternParts[i];

                if (patternPart.StartsWith(":", StringComparison.Ordinal)) continue; // ':name' segments capture any value

                if (!string.Equals(patternPart, navigationParts[i], StringComparison.InvariantCultureIgnoreCase)) return false;
            }

            return true;
        }

        private static string[] SplitSegments(string path) => path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
    }
}
