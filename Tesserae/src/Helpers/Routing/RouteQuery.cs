using System;
using System.Linq;

namespace Tesserae
{
    /// <summary>
    /// The hash query string of the address bar as the app's state: <see cref="Get"/> and <see cref="TryGet"/> read a key, and the writes
    /// keep view state in it (<see cref="Set"/>, <see cref="Clear"/>, <see cref="Update"/>, <see cref="Consume(string, out string)"/>).
    /// It never looks at the path; <see cref="RoutePath"/> is the other half.
    /// </summary>
    /// <remarks>
    /// <para>Reads come from <see cref="Router.CurrentHash"/>, so they work before the first route has matched.</para>
    /// <para>Writes go through <see cref="Router.ReplaceQueryParameters"/>: only the query segment changes, every other key is kept, no route
    /// handler runs, <see cref="Router.OnNavigated"/> does not fire and the back-button guard is not asked. Every write but <c>Consume</c> takes a
    /// <see cref="QueryHistory"/>: whether it adds a history entry is the caller's decision, never a default. Before the first route has matched
    /// there is nothing to write to and they do nothing.</para>
    /// <para>Keys and values are case-sensitive and are URI-encoded when written and decoded when read, so pass them unencoded.</para>
    /// </remarks>
    [Transpose.Name("tss.RouteQuery")]
    public static class RouteQuery
    {
        private static RouteLocation Current => new RouteLocation(Router.CurrentHash);

        /// <summary>The value of the key, <c>""</c> for <c>?x</c> and <c>?x=</c>, null when the key is not in the query.</summary>
        public static string Get(string key) => Current.Get(key);

        /// <summary>True when the key is in the query, whatever its value (<c>?x</c> and <c>?x=</c> give an empty one).</summary>
        public static bool TryGet(string key, out string value) => Current.TryGet(key, out value);

        /// <summary>Sets one key, keeping the others. Does nothing when it already has that value.</summary>
        public static void Set(string key, string value, QueryHistory history) => Write(p => p.With(key, value), history, key);

        /// <summary>Removes one key, keeping the others. Does nothing when it is not there.</summary>
        public static void Clear(string key, QueryHistory history) => Write(p => p.Without(key), history, key);

        /// <summary>
        /// Changes several keys in one write, so they are one step back: <c>Update(p => p.With("a", "1").Without("b"), QueryHistory.Replace)</c>.
        /// <see cref="Parameters"/> is immutable, so the lambda returns the result. <see cref="QueryHistory.ReplaceFirstThenPush"/> needs the keys
        /// it looks at in <paramref name="keys"/> and pushes only when every one of them already has a value; the other modes take no keys.
        /// </summary>
        public static void Update(Func<Parameters, Parameters> update, QueryHistory history, params string[] keys)
        {
            if ((history == QueryHistory.ReplaceFirstThenPush) != (keys.Length > 0))
            {
                throw new ArgumentException(history == QueryHistory.ReplaceFirstThenPush
                    ? "QueryHistory.ReplaceFirstThenPush needs the keys whose values decide between replacing and pushing."
                    : "Keys are only read by QueryHistory.ReplaceFirstThenPush.", nameof(keys));
            }

            Write(update, history, keys);
        }

        /// <summary>
        /// For a key that asks for something once (a toast, a dialog): reads it and removes it from the URL, so a refresh or a shared link does not ask again.
        /// Always replaces the history entry: an entry that still carries the key would ask again on Back.
        /// </summary>
        public static bool Consume(string key, out string value)
        {
            var found = TryGet(key, out value);

            if (found)
            {
                Clear(key, QueryHistory.Replace);
            }

            return found;
        }

        /// <summary>As <see cref="Consume(string, out string)"/> for several keys that belong together, removed in one write. True when any of them was there.</summary>
        public static bool Consume(params string[] keys)
        {
            var current = Current;
            var any     = keys.Any(key => current.TryGet(key, out _));

            if (any)
            {
                Update(p => keys.Aggregate(p, (acc, key) => acc.Without(key)), QueryHistory.Replace);
            }

            return any;
        }

        private static void Write(Func<Parameters, Parameters> update, QueryHistory history, params string[] keys)
        {
            var push = history == QueryHistory.Push
                    || (history == QueryHistory.ReplaceFirstThenPush && keys.All(key => HasValue(key)));

            Router.ReplaceQueryParameters(update, pushToHistory: push);
        }

        // a key that is there but empty or blank is "no value yet": writing it for the first time is arriving, not a step to go back over
        private static bool HasValue(string key) => !string.IsNullOrWhiteSpace(Get(key));
    }

    /// <summary>What a <see cref="RouteQuery"/> write does to the browser history.</summary>
    [Transpose.Name("tss.QueryHistory")]
    public enum QueryHistory
    {
        /// <summary>Rewrites the current entry: Back skips over the change. For state that is not a step the user would go back over.</summary>
        Replace,

        /// <summary>Adds an entry: Back undoes the change.</summary>
        Push,

        /// <summary>
        /// For a key that picks a tab or a filter: replaces the entry while the key has no value yet (arriving with no key is not a step to go
        /// back over), and adds one once it has a value, so Back returns to the previous choice. A blank value counts as none.
        /// </summary>
        ReplaceFirstThenPush,
    }
}
