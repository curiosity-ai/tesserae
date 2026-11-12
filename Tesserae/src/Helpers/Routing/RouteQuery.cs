using System;
using System.Collections.Generic;
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
    /// <para>Writes change only the query segment of the hash: every other key is kept, no route
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
        public static void Set(string key, string value, QueryHistory history) => Write(p => p.With(key, value), history);

        /// <summary>Removes one key, keeping the others. Does nothing when it is not there.</summary>
        public static void Clear(string key, QueryHistory history) => Write(p => p.Without(key), history);

        /// <summary>
        /// Changes several keys in one write, so they are one step back: <c>Update(p => p.With("a", "1").Without("b"), QueryHistory.Replace)</c>.
        /// <see cref="Parameters"/> is immutable, so the lambda returns the result. With <see cref="QueryHistory.ReplaceFirstThenPush"/> it pushes
        /// only when every key the write changes already had a value.
        /// </summary>
        public static void Update(Func<Parameters, Parameters> update, QueryHistory history) => Write(update, history);

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

        private static void Write(Func<Parameters, Parameters> update, QueryHistory history)
        {
            var before = Router.GetQueryParameters();
            var after  = update(before) ?? new Parameters();

            var push = history == QueryHistory.Push
                    || (history == QueryHistory.ReplaceFirstThenPush && ChangedKeys(before, after).All(key => HasValue(before, key)));

            Router.UpdateQuery(_ => after, pushToHistory: push);
        }

        private static IEnumerable<string> ChangedKeys(Parameters before, Parameters after)
        {
            return before.Keys.Union(after.Keys).Where(key => ValueOf(before, key) != ValueOf(after, key));
        }

        private static string ValueOf(Parameters parameters, string key) => parameters.TryGetValue(key, out var value) ? value : null;

        // a key that is there but empty or blank is "no value yet": writing it for the first time is arriving, not a step to go back over
        private static bool HasValue(Parameters parameters, string key) => !string.IsNullOrWhiteSpace(ValueOf(parameters, key));
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
        /// For a key that picks a tab or a filter: replaces the entry while a key the write changes has no value yet (arriving with no key is
        /// not a step to go back over), and adds one once they all have one, so Back returns to the previous choice. A blank value counts as none.
        /// </summary>
        ReplaceFirstThenPush,
    }
}
