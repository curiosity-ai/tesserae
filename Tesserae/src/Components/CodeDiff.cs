using System;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A component that renders a unified diff using the
    /// <see href="https://github.com/rtfpessoa/diff2html">diff2html</see> library.
    /// The slim UI bundle ships with Tesserae and is fetched the first time a diff is drawn (its CSS is
    /// part of Tesserae's stylesheet); there is nothing to preload.
    /// </summary>
    [Transpose.Name("tss.cd")]
    public class CodeDiff : ComponentBase<CodeDiff, HTMLElement>
    {
        /// <summary>
        /// Controls how a <see cref="CodeDiff"/> renders the diff.
        /// </summary>
        [Enum(Emit.StringName)]
        [Transpose.Name("tss.CDF")]
        public enum Format
        {
            /// <summary>Renders additions and deletions one after the other, on a single column.</summary>
            [Name("line-by-line")]  LineByLine,
            /// <summary>Renders the original file on the left and the new file on the right.</summary>
            [Name("side-by-side")]  SideBySide
        }

        /// <summary>
        /// Controls how diff2html matches lines between the two sides of a <see cref="CodeDiff"/>.
        /// </summary>
        [Enum(Emit.StringName)]
        [Transpose.Name("tss.CDM")]
        public enum Matching
        {
            /// <summary>No matching is performed between additions and deletions.</summary>
            [Name("none")]  None,
            /// <summary>Lines are matched between the two sides.</summary>
            [Name("lines")] Lines,
            /// <summary>Words are matched between the two sides.</summary>
            [Name("words")] Words
        }

        private string   _diff;
        private Format   _format;
        private bool     _drawFileList;
        private bool     _highlightCode;
        private Matching _matching;
        private double   _timeout;
        private int      _redrawRequest;

        /// <summary>
        /// Initializes a new <see cref="CodeDiff"/> for the given unified diff text.
        /// </summary>
        /// <param name="diff">Diff text in unified-diff format (e.g. the output of <c>git diff</c>).</param>
        /// <param name="format">Whether to render line-by-line or side-by-side.</param>
        public CodeDiff(string diff = "", Format format = Format.LineByLine)
        {
            InnerElement = Div(Att("tss-codediff"));
            _diff        = diff ?? string.Empty;
            _format      = format;
            _matching    = Matching.Lines;
            ScheduleRedraw();
        }

        /// <summary>Gets or sets the unified diff text to render.</summary>
        public string DiffText
        {
            get => _diff;
            set { _diff = value ?? string.Empty; ScheduleRedraw(); }
        }

        /// <summary>Gets or sets the output layout (line-by-line or side-by-side).</summary>
        public Format OutputFormat
        {
            get => _format;
            set { _format = value; ScheduleRedraw(); }
        }

        /// <summary>When <c>true</c>, a summary list of files in the diff is rendered above the diff.</summary>
        public bool DrawFileList
        {
            get => _drawFileList;
            set { _drawFileList = value; ScheduleRedraw(); }
        }

        /// <summary>
        /// When <c>true</c>, asks diff2html to call <c>highlightCode()</c> after drawing. This relies on
        /// a globally-available <c>hljs</c> (highlight.js); it is a no-op if highlight.js is not loaded.
        /// </summary>
        public bool HighlightCode
        {
            get => _highlightCode;
            set { _highlightCode = value; ScheduleRedraw(); }
        }

        /// <summary>
        /// Controls how diff2html matches lines between the two sides.
        /// </summary>
        public Matching LineMatching
        {
            get => _matching;
            set { _matching = value; ScheduleRedraw(); }
        }

        /// <summary>
        /// Where diff2html's UI bundle is published next to the application (a resource of Tesserae's that is copied
        /// but not linked from index.html), loaded the first time a CodeDiff draws.
        /// </summary>
        internal const string LibraryUrl = "assets/js/diff2html-ui-slim.min.js";

        private static bool IsLibraryLoaded() => Script.Write<bool>("typeof globalThis.Diff2HtmlUI !== 'undefined'");

        private void ScheduleRedraw()
        {
            window.clearTimeout(_timeout);
            _timeout = window.setTimeout(_ =>
            {
                if (IsLibraryLoaded())
                {
                    Redraw();
                }
                else
                {
                    LoadLibraryThenRedraw(++_redrawRequest).FireAndForget();
                }
            }, 16);
        }

        private async Task LoadLibraryThenRedraw(int request)
        {
            try
            {
                await global::Transpose.Require.RequireAsync(LibraryUrl);
            }
            catch (Exception ex)
            {
                console.error("CodeDiff: failed to load diff2html", ex);
                return;
            }

            // Every redraw asked for while the script loaded waits on the same load: the last one draws the latest state.
            if (request == _redrawRequest)
            {
                Redraw();
            }
        }

        private void Redraw()
        {
            try
            {
                var ui = Script.Write<object>(
                    "new globalThis.Diff2HtmlUI({0}, {1}, { drawFileList: {2}, fileListToggle: false, matching: {3}, outputFormat: {4} })",
                    InnerElement, _diff, _drawFileList, _matching, _format);

                Script.Write("{0}.draw()", ui);

                if (_highlightCode)
                {
                    Script.Write("if (typeof globalThis.hljs !== 'undefined') { {0}.highlightCode() }", ui);
                }
            }
            catch (Exception ex)
            {
                console.error("CodeDiff: failed to render diff", ex);
            }
        }

        /// <summary>Renders the component.</summary>
        public override HTMLElement Render() => InnerElement;
    }
}
