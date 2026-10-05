using System;
using System.Linq;

namespace Tesserae
{
    /// <summary>
    /// The hash query string of the address bar as the app's state: <see cref="Get"/> and <see cref="TryGet"/> read a key, and the writes
    /// keep view state in it (<see cref="Set"/>, <see cref="Clear"/>, <see cref="Update"/>, <see cref="Consume(string, out string)"/>, ...).
    /// It never looks at the path; <see cref="RoutePath"/> is the other half.
    /// </summary>
    /// <remarks>
    /// <para>Reads come from <see cref="Router.CurrentHash"/>, so they work before the first route has matched.</para>
    /// <para>Writes go through <see cref="Router.ReplaceQueryParameters"/>: only the query segment changes, every other key is kept, no route
    /// handler runs, <see cref="Router.OnNavigated"/> does not fire and the back-button guard is not asked. They replace the history entry,
    /// except where the name says <c>WithHistory</c>. Before the first route has matched there is nothing to write to and they do nothing.</para>
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
        public static void Set(string key, string value) => Router.ReplaceQueryParameters(p => p.With(key, value));

        /// <summary>Removes one key, keeping the others. Does nothing when it is not there.</summary>
        public static void Clear(string key) => Router.ReplaceQueryParameters(p => p.Remove(key));

        /// <summary>Changes several keys in one write.</summary>
        public static void Update(Action<Parameters> update) => Router.ReplaceQueryParameters(p => { update(p); return p; });

        /// <summary>For a key that asks for something once (a toast, a dialog): reads it and removes it from the URL, so a refresh or a shared link does not ask again.</summary>
        public static bool Consume(string key, out string value)
        {
            var found = TryGet(key, out value);

            if (found)
            {
                Clear(key);
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
                Update(p => { foreach (var key in keys) p.Remove(key); });
            }

            return any;
        }

        /// <summary>
        /// <see cref="Set"/> for a key that picks a tab or a filter: the first time it is written it replaces the entry (arriving with no key is not a step
        /// to go back over), and once it has a value every change adds an entry, so Back returns to the previous choice.
        /// </summary>
        public static void SetWithHistory(string key, string value) => Router.ReplaceQueryParameters(p => p.With(key, value), pushToHistory: HasValue(key));

        /// <summary>
        /// <see cref="Update"/> that adds a history entry only when every key in <paramref name="pushWhenPresent"/> already has a value, and
        /// replaces the entry otherwise. One write, so several keys changed together are one step back.
        /// </summary>
        public static void UpdateWithHistory(Action<Parameters> update, params string[] pushWhenPresent)
        {
            var push = pushWhenPresent.Length > 0 && pushWhenPresent.All(key => HasValue(key));

            Router.ReplaceQueryParameters(p => { update(p); return p; }, push);
        }

        // a key that is there but empty or blank is "no value yet": writing it for the first time is arriving, not a step to go back over
        private static bool HasValue(string key) => !string.IsNullOrWhiteSpace(Get(key));
    }
}
