using System.Threading.Tasks;
using Transpose;

namespace Tesserae
{
    /// <summary>
    /// Loads <see href="https://docs.yjs.dev">Yjs</see>, the CRDT for real-time collaborative editing. Yjs is not part
    /// of the bundle a page starts with: it ships as its own file (<c>tss-yjs.min.js</c>, built by <c>Build.Yjs/</c>,
    /// with y-protocols' sync and awareness) and is fetched the first time something calls <see cref="LoadAsync"/>.
    /// Nothing declared on <see cref="Y"/>, <see cref="Awareness"/> or <see cref="YProtocols"/> exists until then.
    /// </summary>
    public static class Yjs
    {
        /// <summary>Where the compiler puts the unlinked <c>tss-yjs.min.js</c> resource of <c>tps.json</c>.</summary>
        public const string ScriptUrl = "assets/js/tss-yjs.min.js";

        private static Task _loading;

        /// <summary>Whether Yjs is on the page, so a document can be created without waiting.</summary>
        public static bool IsLoaded { get; private set; }

        /// <summary>
        /// Loads the library once. Every caller shares the one fetch, and a failed load is forgotten so the next call
        /// tries again.
        /// </summary>
        public static async Task LoadAsync()
        {
            if (IsLoaded) return;

            if (_loading is null) _loading = global::Transpose.Require.RequireAsync(global::Transpose.RequireKind.Script, ScriptUrl);

            try
            {
                await _loading;
                IsLoaded = true;
            }
            catch
            {
                _loading = null;
                throw;
            }
        }
    }
}
