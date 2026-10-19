using System;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Draws Mermaid diagram source as SVG, with <see href="https://pele.run">Pele</see> (not the Mermaid library),
    /// themed from Tesserae's tokens so it follows light/dark mode and custom themes without being redrawn.
    /// The same renderer draws a <c>```mermaid</c> fence in <see cref="MarkdownBlock"/>.
    /// </summary>
    /// <remarks>
    /// Pele is fetched the first time a diagram is drawn; until then the source is shown as text. Changing
    /// <see cref="Source"/> redraws in place, and a source that cannot be drawn is reported through
    /// <see cref="Error"/> and <see cref="OnError"/> (and shown in the diagram's place) rather than thrown.
    /// </remarks>
    [Transpose.Name("tss.mmd")]
    public class Mermaid : ComponentBase<Mermaid, HTMLElement>
    {
        private string          _source = string.Empty;
        private bool            _sanitize;
        private bool            _links  = true;
        private bool            _images = true;
        private bool            _zoom   = true;
        private object          _mounted;
        private string          _error;
        private Action<string>  _onError;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mermaid"/> class.
        /// </summary>
        /// <param name="source">The Mermaid source, e.g. <c>flowchart LR\n  A --&gt; B</c>.</param>
        /// <param name="sanitize">
        /// Whether the SVG is run through DOMPurify before it is inserted, on top of Pele's own escaping of diagram
        /// text. On by default; turn it off only for source you wrote yourself.
        /// </param>
        public Mermaid(string source = "", bool sanitize = true)
        {
            InnerElement = Div(Att("tss-mermaid"));
            _sanitize    = sanitize;
            Source       = source;
        }

        /// <summary>Gets or sets the Mermaid source. Setting it redraws the diagram.</summary>
        public string Source
        {
            get => _source;
            set
            {
                _source = value ?? string.Empty;
                Refresh();
            }
        }

        /// <summary>Gets whether the diagram's SVG is sanitized before it is inserted.</summary>
        public bool IsSanitized => _sanitize;

        /// <summary>Gets why the current source could not be drawn, or <c>null</c> when it was (or has not been tried yet).</summary>
        public string Error => _error;

        /// <summary>
        /// Sets whether the SVG is run through DOMPurify before it is inserted (on by default) and redraws.
        /// </summary>
        public Mermaid Sanitize(bool sanitize = true)
        {
            _sanitize = sanitize;
            Refresh();
            return this;
        }

        /// <summary>
        /// Sets whether the diagram's own hyperlinks (<c>click</c> statements) are kept (on by default) and redraws.
        /// Turn them off for source you do not trust to link anywhere.
        /// </summary>
        public Mermaid Links(bool allow = true)
        {
            _links = allow;
            Refresh();
            return this;
        }

        /// <summary>
        /// Sets whether the diagram's own images are kept (on by default) and redraws. An image is a request to
        /// whatever server hosts it, so turn them off for source you do not trust.
        /// </summary>
        public Mermaid Images(bool allow = true)
        {
            _images = allow;
            Refresh();
            return this;
        }

        /// <summary>
        /// Sets whether a diagram that has been shrunk to fit its container can be zoomed and panned (on by default) and redraws.
        /// </summary>
        public Mermaid Zoom(bool enabled = true)
        {
            _zoom = enabled;
            Refresh();
            return this;
        }

        /// <summary>
        /// Registers a callback for when the source cannot be drawn (a syntax error, the library failing to load).
        /// It is called with the reason, once per failed attempt.
        /// </summary>
        public Mermaid OnError(Action<string> callback)
        {
            _onError = callback;
            return this;
        }

        private object Options() => Pele.Options(_sanitize, _links, _images, _zoom);

        private void Refresh()
        {
            if (string.IsNullOrWhiteSpace(_source))
            {
                Clear();
                return;
            }

            if (_mounted != null)
            {
                Pele.Update(_mounted, _source, Options());
                SetError(Pele.ErrorOf(_mounted));
            }
            else if (Pele.IsLoaded)
            {
                Mount();
            }
            else
            {
                ShowSource();
                MountWhenLoaded().FireAndForget();
            }
        }

        private async System.Threading.Tasks.Task MountWhenLoaded()
        {
            try
            {
                await Pele.LoadAsync();
            }
            catch
            {
                SetError("The diagram library could not be loaded.");
                return;
            }

            //The source may have been cleared, or drawn by a later call, while the library was on its way.
            if (_mounted == null && !string.IsNullOrWhiteSpace(_source)) Mount();
        }

        private void Mount()
        {
            InnerElement.innerHTML = string.Empty;
            _mounted               = Pele.Mount(InnerElement, _source, Options());
            SetError(Pele.ErrorOf(_mounted));
        }

        private void Clear()
        {
            if (_mounted != null)
            {
                Pele.Destroy(_mounted);
                _mounted = null;
            }

            InnerElement.innerHTML = string.Empty;
            SetError(null);
        }

        //What is on the page while the library loads: the source, as text.
        private void ShowSource()
        {
            InnerElement.innerHTML = string.Empty;
            var pre                = Pre(Att("tss-mermaid-source"));
            pre.textContent        = _source;
            InnerElement.appendChild(pre);
            SetError(null);
        }

        private void SetError(string error)
        {
            _error = error;
            InnerElement.UpdateClassIf(error != null, "tss-mermaid-failed");
            if (error != null) _onError?.Invoke(error);
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
