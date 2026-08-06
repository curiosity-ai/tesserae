namespace Tesserae
{
    /// <summary>
    /// How strictly the HTML produced from a Markdown source is sanitized before it is inserted
    /// into the page. Both modes go through DOMPurify - the mode only picks the configuration.
    /// </summary>
    [Transpose.Name("tss.mds")]
    public enum MarkdownSanitization
    {
        /// <summary>
        /// DOMPurify's default profile: scripts and event handlers are removed, but links,
        /// images and other embedded media survive.
        /// </summary>
        Default,

        /// <summary>
        /// Everything <see cref="Default"/> removes, plus every link and every piece of embedded
        /// content: anchors, images, SVG, media elements and the attributes that fetch a remote
        /// URL. Link text is kept as plain text, so a Markdown link reads as its label and is not
        /// clickable, and an image is dropped entirely.
        ///
        /// Use it for Markdown written by something you don't trust to link or to load a remote
        /// URL - an LLM reply, for instance, where a rendered image is a way to call out to a
        /// third-party server and a rendered link is a way to phish the reader.
        /// </summary>
        NoLinksOrEmbeddedContent
    }
}
