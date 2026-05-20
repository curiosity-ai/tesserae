using H5;

namespace Tesserae
{
    /// <summary>
    /// Static helpers around the globally-loaded <c>DOMPurify</c> library.
    /// DOMPurify is bundled with Tesserae and always loaded - no preload step is required.
    /// </summary>
    [H5.Name("tss.SanitizeHTML")]
    public static class SanitizeHTML
    {
        /// <summary>
        /// Returns a DOMPurify-sanitized copy of the given <paramref name="html"/>.
        /// </summary>
        public static string Sanitize(string html)
        {
            return Script.Write<string>("DOMPurify.sanitize({0})", html);
        }
    }
}
