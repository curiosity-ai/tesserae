namespace Tesserae
{
    /// <summary>
    /// Per-call configuration for <see cref="MarkHighlighter"/> and <see cref="RegExpCreator"/>.
    /// A field left unset falls back to the matching static default, so two surfaces highlighting
    /// at the same time never fight over global state.
    /// </summary>
    [Transpose.Name("tss.MarkOptions")]
    public sealed class MarkOptions
    {
        /// <summary>Tag name of the wrapper element. Default: <see cref="MarkHighlighter.Element"/> ("mark").</summary>
        public string Element { get; set; }

        /// <summary>Name of the data-* attribute stamped on wrapped elements. Default: <see cref="MarkHighlighter.MarkData"/> ("marked").</summary>
        public string MarkData { get; set; }

        /// <summary>Extra class set on each wrapper element. Default: <see cref="MarkHighlighter.ClassName"/>.</summary>
        public string ClassName { get; set; }

        /// <summary>Case-sensitive matching. Default: <see cref="RegExpCreator.CaseSensitive"/>.</summary>
        public bool? CaseSensitive { get; set; }

        /// <summary>Fold diacritics, so 'cafe' also matches 'café'. Default: true.</summary>
        public bool Diacritics { get; set; } = true;

        /// <summary>Only match the keyword where it is not part of a longer word.</summary>
        public bool WholeWord { get; set; }

        /// <summary>Split a multi-word keyword on whitespace and mark each word on its own.</summary>
        public bool SeparateWordSearch { get; set; }

        /// <summary>Enable '*' (any run of non-space characters) and '?' (one optional non-space character) in the keyword.</summary>
        public bool Wildcards { get; set; }

        /// <summary>Match across soft hyphens and zero-width joiners, which hyphenated or justified documents carry mid-word.</summary>
        public bool IgnoreJoiners { get; set; }

        /// <summary>Keywords shorter than this are not marked. Default: 0 (everything is marked).</summary>
        public int MinLength { get; set; }

        /// <summary>
        /// Match across element boundaries, so a phrase split by inline tags
        /// (bold<b>web</b> applications) is still found.
        /// </summary>
        public bool AcrossElements { get; set; }

        /// <summary>
        /// Whether matches are painted through the CSS Custom Highlight API (CSS.highlights +
        /// the ::highlight() rules in tss.markhighlighter.css) instead of wrapping them in mark
        /// elements - no DOM mutation at all, so components that hold references to their own
        /// text nodes are untouched. Null (default): use it when the browser supports it and no
        /// custom Element/MarkData/ClassName is asked for. True: use it whenever supported.
        /// False: always wrap in elements. Text inside iframes is always wrapped in elements -
        /// a highlight registration is per document and an iframe's document doesn't carry the
        /// ::highlight() rules.
        /// </summary>
        public bool? UseHighlightApi { get; set; }
    }
}
