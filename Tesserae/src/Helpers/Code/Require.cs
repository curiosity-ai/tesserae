using System.Threading.Tasks;

namespace Tesserae
{
    /// <summary>
    /// Loads a script, an ES module or a stylesheet at run time.
    ///
    /// <para>
    /// The loading itself lives in the Transpose runtime now — <c>Transpose.Require</c> — so every
    /// library and application on Transpose shares one implementation instead of each growing its
    /// own. These members forward to it and stay for the applications that already call them; new
    /// code can call <see cref="global::Transpose.Require.RequireAsync(string[])"/> directly, which
    /// also picks the element from the URL rather than being told.
    /// </para>
    ///
    /// <para>
    /// What the shared loader adds over what this class used to do: URLs are resolved against the
    /// document base before anything else, so every spelling of one file is one entry and a file
    /// <c>index.html</c> already carries is waited on rather than fetched twice; a failed load is
    /// forgotten rather than remembered as done; and a <c>.min.js</c> that is not there falls back to
    /// the <c>.js</c> beside it (and the other way round), which is what lets a library published
    /// once work in a site built either way.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.Require")]
    public static class Require
    {
        /// <summary>Adds a stylesheet to the page, if it is not on it already. Fire and forget: a
        /// stylesheet that cannot be fetched is reported to the console, as it always was.</summary>
        public static void LoadStyle(params string[] styles)
            => LoadStyleAsync(styles).FireAndForget();

        /// <summary>Adds a stylesheet to the page and completes once the browser has applied it.</summary>
        public static Task LoadStyleAsync(params string[] styles)
            => global::Transpose.Require.RequireAsync(global::Transpose.RequireKind.Style, styles);

        /// <summary>Loads classic scripts, in the order given.</summary>
        public static Task LoadScriptAsync(params string[] libraries)
            => global::Transpose.Require.RequireAsync(global::Transpose.RequireKind.Script, libraries);

        /// <summary>Loads ES modules, in the order given.</summary>
        public static Task LoadModuleAsync(params string[] libraries)
            => global::Transpose.Require.RequireAsync(global::Transpose.RequireKind.Module, libraries);
    }
}
