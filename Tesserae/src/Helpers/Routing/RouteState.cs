using System;
using System.Linq;

namespace Tesserae
{
    /// <summary>
    /// The URL as the app's state: where the address bar is (<see cref="Current"/>, and <see cref="Path"/>, <see cref="Get"/> and <see cref="Has"/> that forward to it), and the
    /// writes that keep view state in the hash query string (<see cref="Set"/>, <see cref="Update"/>, <see cref="Consume(string, out string)"/>, ...).
    /// </summary>
    /// <remarks>
    /// <para>Reads come from <see cref="Router.CurrentHash"/>, so they work before the first route has matched.</para>
    /// <para>Writes go through <see cref="Router.ReplaceQueryParameters"/>: only the query segment changes, every other key is kept, no route
    /// handler runs, <see cref="Router.OnNavigated"/> does not fire and the back-button guard is not asked. They replace the history entry,
    /// except where the name says <c>WithHistory</c>. Before the first route has matched there is nothing to write to and they do nothing.</para>
    /// </remarks>
    [Transpose.Name("tss.RouteState")]
    public static class RouteState
    {
        /// <summary>The address bar's hash, read now.</summary>
        public static RouteLocation Current => new RouteLocation(Router.CurrentHash);

        public static string Path            => Current.Path;
        public static bool   Has(string key) => Current.Has(key);
        public static string Get(string key) => Current.Get(key);

        /// <summary>Sets one key, keeping the others. Does nothing when it already has that value.</summary>
        public static void Set(string key, string value) => Router.ReplaceQueryParameters(p => p.With(key, value));

        /// <summary>Removes one key, keeping the others. Does nothing when it is not there.</summary>
        public static void Clear(string key) => Router.ReplaceQueryParameters(p => p.Remove(key));

        /// <summary>Changes several keys in one write.</summary>
        public static void Update(Action<Parameters> update) => Router.ReplaceQueryParameters(p => { update(p); return p; });

        /// <summary>For a key that asks for something once (a toast, a dialog): reads it and removes it from the URL, so a refresh or a shared link does not ask again.</summary>
        public static bool Consume(string key, out string value)
        {
            value = Get(key);

            if (value is object)
            {
                Clear(key);
            }

            return value is object;
        }

        /// <summary>As <see cref="Consume(string, out string)"/> for several keys that belong together, removed in one write. True when any of them was there.</summary>
        public static bool Consume(params string[] keys)
        {
            var current = Current;
            var any     = keys.Any(key => current.Get(key) is object);

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
        public static void SetWithHistory(string key, string value) => Router.ReplaceQueryParameters(p => p.With(key, value), pushToHistory: Has(key));

        /// <summary>
        /// <see cref="Update"/> that adds a history entry only when every key in <paramref name="pushWhenPresent"/> already has a value, and
        /// replaces the entry otherwise. One write, so several keys changed together are one step back.
        /// </summary>
        public static void UpdateWithHistory(Action<Parameters> update, params string[] pushWhenPresent)
        {
            var push = pushWhenPresent.Length > 0 && pushWhenPresent.All(key => Has(key));

            Router.ReplaceQueryParameters(p => { update(p); return p; }, push);
        }

        /// <summary>
        /// Moves the address bar to <paramref name="route"/>, keeping the query, in place of the current entry. The same view stays on screen,
        /// no handler runs: for a page that finds its canonical address once it knows what it shows. False when a guard refused.
        /// </summary>
        public static bool ReplacePath(string route)
        {
            var current = Current;

            return current.IsOn(route) || Router.Replace(RouteLocation.PathOf(route) + current.Query.ToQueryString());
        }
    }
}
