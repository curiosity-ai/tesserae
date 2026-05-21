using System;
using System.Threading.Tasks;
using H5;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Controls how a <see cref="CodeDiff"/> renders the diff.
    /// </summary>
    [H5.Name("tss.CodeDiffFormat")]
    public enum CodeDiffFormat
    {
        /// <summary>Renders additions and deletions one after the other, on a single column.</summary>
        LineByLine,
        /// <summary>Renders the original file on the left and the new file on the right.</summary>
        SideBySide
    }

    /// <summary>
    /// A component that renders a unified diff using the
    /// <see href="https://github.com/rtfpessoa/diff2html">diff2html</see> library.
    /// The slim UI bundle and its CSS are fetched on-demand from jsDelivr the first
    /// time a <see cref="CodeDiff"/> is constructed.
    /// </summary>
    [H5.Name("tss.cd")]
    public class CodeDiff : ComponentBase<CodeDiff, HTMLElement>
    {
        private const string ScriptUrl = "https://cdn.jsdelivr.net/npm/diff2html/bundles/js/diff2html-ui-slim.min.js";
        private const string StyleUrl  = "https://cdn.jsdelivr.net/npm/diff2html/bundles/css/diff2html.min.css";

        private static Task _loadTask;

        private string         _diff;
        private CodeDiffFormat _format;
        private bool           _drawFileList;
        private bool           _highlightCode;
        private string         _matching;
        private double         _timeout;

        /// <summary>
        /// Initializes a new <see cref="CodeDiff"/> for the given unified diff text.
        /// </summary>
        /// <param name="diff">Diff text in unified-diff format (e.g. the output of <c>git diff</c>).</param>
        /// <param name="format">Whether to render line-by-line or side-by-side.</param>
        public CodeDiff(string diff = "", CodeDiffFormat format = CodeDiffFormat.LineByLine)
        {
            InnerElement = Div(_("tss-codediff"));
            _diff        = diff ?? string.Empty;
            _format      = format;
            _matching    = "lines";
            ScheduleRedraw();
        }

        /// <summary>Gets or sets the unified diff text to render.</summary>
        public string Diff
        {
            get => _diff;
            set { _diff = value ?? string.Empty; ScheduleRedraw(); }
        }

        /// <summary>Gets or sets the output layout (line-by-line or side-by-side).</summary>
        public CodeDiffFormat Format
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
        /// Controls how diff2html matches lines between the two sides. Valid values are
        /// <c>"lines"</c>, <c>"words"</c> or <c>"none"</c>. Defaults to <c>"lines"</c>.
        /// </summary>
        public string MatchingLines
        {
            get => _matching;
            set { _matching = string.IsNullOrEmpty(value) ? "lines" : value; ScheduleRedraw(); }
        }

        private static Task LoadAsync()
        {
            if (_loadTask == null)
            {
                Require.LoadStyle(StyleUrl);
                _loadTask = Require.LoadScriptAsync(ScriptUrl);
            }
            return _loadTask;
        }

        private void ScheduleRedraw()
        {
            window.clearTimeout(_timeout);
            _timeout = window.setTimeout(_ => Redraw(), 16);
        }

        private async void Redraw()
        {
            try
            {
                await LoadAsync();
            }
            catch (Exception ex)
            {
                console.error("CodeDiff: failed to load diff2html", ex);
                return;
            }

            var outputFormat = _format == CodeDiffFormat.SideBySide ? "side-by-side" : "line-by-line";

            try
            {
                var ui = Script.Write<object>(
                    "new globalThis.Diff2HtmlUI({0}, {1}, { drawFileList: {2}, fileListToggle: false, matching: {3}, outputFormat: {4} })",
                    InnerElement, _diff, _drawFileList, _matching, outputFormat);

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
