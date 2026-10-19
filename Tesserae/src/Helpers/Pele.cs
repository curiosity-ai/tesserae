using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// The one place that knows how <see href="https://pele.run">Pele</see> is loaded and called, shared by the
    /// <see cref="Mermaid"/> component and the diagrams <see cref="Markdown"/> draws. Pele is not part of the bundle
    /// the page starts with: it ships as its own file and is fetched the first time something needs it.
    /// </summary>
    internal static class Pele
    {
        /// <summary>Where the compiler puts the unlinked <c>tss-pele.min.js</c> resource of <c>tps.json</c>.</summary>
        internal const string ScriptUrl = "assets/js/tss-pele.min.js";

        private static Task _loading;

        /// <summary>Whether the library is on the page, so a diagram can be drawn without waiting.</summary>
        internal static bool IsLoaded { get; private set; }

        /// <summary>Loads the library once; a failed load is forgotten so the next call tries again.</summary>
        internal static async Task LoadAsync()
        {
            if (IsLoaded) return;

            if (_loading is null) _loading = Require.LoadScriptAsync(ScriptUrl);

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

        /// <summary>
        /// The options of <c>pele.mount</c>. Sanitizing runs the SVG through DOMPurify on top of Pele's own escaping
        /// (Pele inserts it as HTML by default); <paramref name="links"/> and <paramref name="images"/> are the
        /// diagram's own hyperlinks and pictures, which are a call out to a third party like the ones in prose.
        /// </summary>
        internal static object Options(bool sanitize, bool links, bool images, bool zoom)
        {
            // Pele takes a plain options object; there is no typed shape for it on the C# side.
            return Script.Write<object>(
                "{ links: {0}, images: {1}, zoom: {2} ? 'auto' : false, sanitize: {3} ? function (svg) { return DOMPurify.sanitize(svg, { USE_PROFILES: { svg: true, svgFilters: true }, RETURN_DOM_FRAGMENT: true }); } : undefined }",
                links, images, zoom, sanitize);
        }

        /// <summary>Draws <paramref name="text"/> into <paramref name="host"/> and keeps it fitted to the host's width.</summary>
        internal static object Mount(HTMLElement host, string text, object options)
            => Script.Write<object>("globalThis.pele.mount({0}, {1}, {2})", host, text, options);

        /// <summary>Replaces the source (and the options) of a diagram that is already mounted.</summary>
        internal static void Update(object mounted, string text, object options)
            => Script.Write("{0}.update({1}, {2});", mounted, text, options);

        /// <summary>Why the last source could not be drawn, or <c>null</c>.</summary>
        internal static string ErrorOf(object mounted)
            => Script.Write<string>("{0}.error ? String({0}.error.message) : null", mounted);

        /// <summary>Stops the diagram watching its container. It leaves what it drew in place.</summary>
        internal static void Destroy(object mounted)
            => Script.Write("{0}.destroy();", mounted);
    }
}
