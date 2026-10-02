using System;
using System.Text;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A component that renders Markdown text as sanitized HTML.
    /// Uses the bundled <c>marked</c> and <c>DOMPurify</c> libraries via <see cref="Markdown"/>.
    /// </summary>
    [Transpose.Name("tss.mdb")]
    public class MarkdownBlock : ComponentBase<MarkdownBlock, HTMLElement>, ICanWrap
    {
        private string                _text;
        private Action<HTMLElement>   _onAfterRender;
        private MarkdownSanitization  _sanitization;
        private bool                  _tableCopyButtons = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownBlock"/> class.
        /// </summary>
        /// <param name="text">The Markdown source text to render.</param>
        /// <param name="sanitization">
        /// How strictly to sanitize the rendered HTML. Pass
        /// <see cref="MarkdownSanitization.NoLinksOrEmbeddedContent"/> for Markdown from a source you
        /// don't trust to link or to load a remote URL.
        /// </param>
        public MarkdownBlock(string text = "", MarkdownSanitization sanitization = MarkdownSanitization.Default)
        {
            InnerElement                  = Div(Att("tss-markdown"));
            InnerElement.style.whiteSpace = "break-spaces";
            _sanitization                 = sanitization;
            Text                          = text ?? string.Empty;
        }

        /// <summary>Gets or sets the Markdown source text. Setting this re-renders the sanitized HTML.</summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                DestroyTableActionTooltips();
                InnerElement.innerHTML = Tesserae.Markdown.ConvertMarkdownSanitized(_text, _sanitization);
                if (_tableCopyButtons) AddTableCopyButtons(InnerElement);
                _onAfterRender?.Invoke(InnerElement);
            }
        }

        /// <summary>
        /// Sets how strictly the rendered HTML is sanitized and re-renders the current source with it.
        /// </summary>
        public MarkdownBlock Sanitization(MarkdownSanitization sanitization)
        {
            _sanitization = sanitization;
            Text          = _text;
            return this;
        }

        /// <summary>
        /// Sets whether every table in the rendered Markdown gets a "Copy Table" button (copies it as HTML, so
        /// it pastes as a table into a document, a spreadsheet or an email) and a "Copy as CSV" button beside
        /// it. On by default; re-renders the current source.
        /// </summary>
        public MarkdownBlock TableCopyButtons(bool show = true)
        {
            _tableCopyButtons = show;
            Text              = _text;
            return this;
        }

        /// <summary>Gets the rendered sanitized HTML (derived from <see cref="Text"/>).</summary>
        public string HTML => InnerElement.innerHTML;

        /// <summary>Gets or sets whether the rendered Markdown can wrap.</summary>
        public bool CanWrap
        {
            get => !InnerElement.classList.contains("tss-text-nowrap");
            set => InnerElement.UpdateClassIfNot(value, "tss-text-nowrap");
        }

        /// <summary>
        /// Registers a callback invoked every time the Markdown source is re-parsed into the
        /// inner HTML element. The callback receives the inner element so that callers can
        /// post-process the rendered tree (e.g. wrap <c>&lt;code&gt;</c> blocks with custom
        /// controls, rewrite links, attach copy buttons, ...).
        ///
        /// The callback is fired immediately with the current rendered content so the first
        /// pass does not require the caller to also trigger an explicit render. Re-registering
        /// replaces any previously-attached callback.
        /// </summary>
        public MarkdownBlock OnAfterRender(Action<HTMLElement> callback)
        {
            _onAfterRender = callback;
            callback?.Invoke(InnerElement);
            return this;
        }

        //The buttons are added after DOMPurify has run, so the sanitization profile - which forbids buttons
        //under NoLinksOrEmbeddedContent - only ever sees the Markdown's own HTML, never these.
        private static void AddTableCopyButtons(HTMLElement root)
        {
            foreach (var found in root.querySelectorAll("table"))
            {
                var table   = found.As<HTMLElement>();
                var wrapper = Div(Att("tss-markdown-table"));
                table.parentElement.insertBefore(wrapper, table);
                wrapper.appendChild(table);

                var copyTable = Button().Compact().SetIcon(UIcons.Copy).Tooltip("Copy Table")
                   .OnClick(() => Clipboard.CopyHtml(TableToHtml(table), TableToDelimited(table, '\t'), customMessage: "📋 Table copied"));

                var copyCsv = Button().Compact().SetIcon(UIcons.FileCsv).Tooltip("Copy as CSV")
                   .OnClick(() => Clipboard.Copy(TableToDelimited(table, ','), customMessage: "📋 Table copied as CSV"));

                wrapper.appendChild(Div(Att("tss-markdown-table-actions"), copyTable.Render(), copyCsv.Render()));
            }
        }

        //A tooltip that is showing lives on document.body, so it would outlive the button innerHTML is about to drop.
        private void DestroyTableActionTooltips()
        {
            foreach (var button in InnerElement.querySelectorAll(".tss-markdown-table-actions .tss-btn"))
            {
                Script.Write("if ({0}._tippy) {0}._tippy.destroy();", button);
            }
        }

        //A bare copy of the table with its borders inline: the stylesheet that draws them here does not travel
        //to wherever the HTML is pasted, and a word processor would otherwise paste a table with no grid at all.
        private static string TableToHtml(HTMLElement table)
        {
            var copy = table.cloneNode(true).As<HTMLElement>();
            copy.removeAttribute("class");
            copy.setAttribute("border", "1");
            copy.style.borderCollapse = "collapse";

            foreach (var cell in copy.querySelectorAll("th, td"))
            {
                var c = cell.As<HTMLElement>();
                c.style.border  = "1px solid #999";
                c.style.padding = "4px 8px";
            }

            return copy.outerHTML;
        }

        //CSV (RFC 4180) for ',' and tab-separated text for '\t' - the plain-text flavour a spreadsheet reads into
        //cells when it is pasted somewhere that ignores the HTML. A tab or a line break inside a cell would split
        //it in TSV, which has no quoting, so there they become spaces.
        private static string TableToDelimited(HTMLElement table, char separator)
        {
            var sb = new StringBuilder();

            foreach (var row in table.querySelectorAll("tr"))
            {
                var first = true;

                foreach (var cell in row.As<HTMLElement>().querySelectorAll(":scope > th, :scope > td"))
                {
                    if (!first) sb.Append(separator);
                    first = false;

                    string content = cell.textContent ?? string.Empty;
                    string text    = content.Trim();

                    if (separator == '\t')
                    {
                        sb.Append(text.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' '));
                    }
                    else if (text.IndexOfAny(new[] { separator, '"', '\r', '\n' }) >= 0)
                    {
                        sb.Append('"').Append(text.Replace("\"", "\"\"")).Append('"');
                    }
                    else
                    {
                        sb.Append(text);
                    }
                }

                sb.Append("\r\n");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
