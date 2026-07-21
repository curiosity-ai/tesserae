using System;
using System.Collections.Generic;
using System.Text;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A locked-down <c>&lt;iframe&gt;</c> for rendering untrusted HTML / apps.
    ///
    /// By default the frame is <b>fully sandboxed</b>: the content is loaded through
    /// <c>srcdoc</c> with <c>sandbox="allow-scripts allow-forms"</c> (no <c>allow-same-origin</c>,
    /// so the document runs in an opaque origin and cannot touch the host page, cookies or storage),
    /// and a strict <c>Content-Security-Policy</c> meta tag is injected as the very first thing in the
    /// document so the sandboxed code cannot exfiltrate data over the network.
    ///
    /// A small bootstrap script is injected into the document that wires up a <see cref="MessageChannel"/>
    /// based post-message flow: uncaught errors, unhandled promise rejections and CSP violations are
    /// captured inside the frame and posted back to the host where they can be surfaced through
    /// <see cref="OnError"/>. The same channel is used for arbitrary host &lt;-&gt; sandbox messaging via
    /// <see cref="OnMessage"/> / <see cref="PostMessage"/>.
    /// </summary>
    [Transpose.Name("tss.Sandbox")]
    public sealed class Sandbox : ComponentBase<Sandbox, HTMLIFrameElement>, ISpecialCaseStyling
    {
        /// <summary>The fully-locked default CSP: inline scripts and styles only, images limited to data/blob URIs, no network access.</summary>
        public const string DefaultContentSecurityPolicy = "default-src 'none'; script-src 'unsafe-inline'; style-src 'unsafe-inline'; img-src data: blob:;";

        // Injected into the sandboxed document. Captures errors / CSP violations / unhandled rejections
        // and (when the host asks for it) reports the document height, posting everything back over the
        // MessageChannel port handed to it by the host. Kept dependency-free so it runs under the strict
        // default CSP.
        //
        // Height reporting is only armed when the host sends `fit:true` in the init message - i.e. when
        // FitHeightToContent is on AND the host cannot measure the frame itself (no same-origin access).
        // Same-origin frames are measured host-side instead (see SetupHostSideFit). Once armed it forces
        // html/body to auto height and measures the content box (getBoundingClientRect, not scrollHeight,
        // which the root element clamps to the viewport height and so would never shrink) so the
        // measurement tracks content in both directions (grow *and* shrink), watches for changes with both
        // a ResizeObserver and a MutationObserver, batches updates through requestAnimationFrame, and
        // de-dupes identical heights to avoid flooding the port.
        private const string BootstrapScript =
            "(function(){var port=null,queue=[],fit=false,raf=0,lastH=-1;" +
            "function flush(){if(port){while(queue.length){port.postMessage(queue.shift());}}}" +
            "function send(m){queue.push(m);flush();}" +
            "function measure(){try{var d=document.documentElement,b=document.body,h=b?b.scrollHeight:0;if(d){var r=d.getBoundingClientRect();if(r&&r.height>h){h=Math.ceil(r.height);}}return h;}catch(_){return 0;}}" +
            "function report(){raf=0;if(!fit)return;var h=measure();if(h>0&&h!==lastH){lastH=h;send({kind:'height',height:h});}}" +
            "function schedule(){if(!fit||raf)return;raf=(window.requestAnimationFrame||function(cb){return setTimeout(cb,16);})(report);}" +
            "function enableFit(){if(fit)return;fit=true;try{document.documentElement.style.setProperty('height','auto','important');if(document.body){document.body.style.setProperty('height','auto','important');}}catch(_){}" +
            "try{if(window.ResizeObserver){new ResizeObserver(schedule).observe(document.documentElement);}}catch(_){}" +
            "try{if(window.MutationObserver){new MutationObserver(schedule).observe(document.body||document.documentElement,{attributes:true,childList:true,subtree:true,characterData:true});}}catch(_){}" +
            "window.addEventListener('load',schedule);schedule();}" +
            "window.addEventListener('message',function(e){if(e.data&&e.data.__tssSandboxInit&&e.ports&&e.ports[0]){port=e.ports[0];port.onmessage=function(ev){try{window.dispatchEvent(new MessageEvent('tss:message',{data:ev.data}));}catch(_){}};if(e.data.fit){enableFit();}flush();}});" +
            "window.addEventListener('error',function(e){send({kind:'error',message:e.message||'Script error',source:e.filename||'',line:e.lineno||0,column:e.colno||0,stack:(e.error&&e.error.stack)?e.error.stack:''});});" +
            "window.addEventListener('unhandledrejection',function(e){var r=e.reason;send({kind:'unhandledrejection',message:(r&&r.message)?r.message:String(r),source:'',line:0,column:0,stack:(r&&r.stack)?r.stack:''});});" +
            "document.addEventListener('securitypolicyviolation',function(e){send({kind:'csp',message:'Blocked by Content-Security-Policy: '+(e.violatedDirective||'')+' '+(e.blockedURI||''),source:e.sourceFile||'',line:e.lineNumber||0,column:e.columnNumber||0,stack:''});});" +
            "window.tssSandbox={post:function(m){send({kind:'message',data:m});}};" +
            "})();";

        /// <summary>The element that receives sizing styles (the iframe itself).</summary>
        public HTMLElement StylingContainer           => InnerElement;
        /// <summary>Styling propagates up to the stack item parent.</summary>
        public bool        PropagateToStackItemParent => true;

        private string _html;
        private string _url;

        private bool _allowScripts    = true;  // fully sandboxed default = allow-scripts allow-forms
        private bool _allowForms      = true;
        private bool _allowSameOrigin = false;
        private bool _allowPopups     = false;
        private bool _allowModals     = false;
        private bool _allowDownloads  = false;
        private bool _disableSandbox  = false;

        private readonly List<string> _extraSandboxTokens = new List<string>();
        private string _sandboxOverride;

        private bool   _injectCsp = true;
        private string _csp       = DefaultContentSecurityPolicy;

        private bool _fitHeightToContent = false;
        private bool _scrolling          = true;

        private Action<HTMLIFrameElement> _onLoaded;
        private Action<SandboxError>      _onError;
        private Action<object>            _onMessage;

        private bool _built;

        /// <summary>Creates a new fully-sandboxed frame, optionally with initial HTML content.</summary>
        public Sandbox(string html = null)
        {
            _html        = html;
            InnerElement = IFrame(Att("tss-sandbox"));
            InnerElement.style.border = "none";
        }

        /// <summary>The underlying iframe element. Only safe to reach into when <see cref="AllowSameOrigin"/> is set.</summary>
        public HTMLIFrameElement IFrameElement => InnerElement;

        #region Content

        /// <summary>Sets the HTML document rendered inside the sandbox (loaded via <c>srcdoc</c>).</summary>
        public Sandbox FromHtml(string html)
        {
            _html = html;
            _url  = null;
            if (_built) ApplyContent();
            return this;
        }

        /// <summary>Loads an external URL into the frame (via <c>src</c>). CSP / bootstrap injection do not apply to cross-origin documents.</summary>
        public Sandbox FromUrl(string url)
        {
            _url  = url;
            _html = null;
            if (_built) ApplyContent();
            return this;
        }

        #endregion

        #region Sandbox flags

        /// <summary>Allows the sandboxed content to run scripts (on by default).</summary>
        public Sandbox AllowScripts(bool allow = true)    { _allowScripts = allow;    return this; }
        /// <summary>Allows the sandboxed content to submit forms (on by default).</summary>
        public Sandbox AllowForms(bool allow = true)      { _allowForms = allow;      return this; }
        /// <summary>Allows popups (e.g. <c>target="_blank"</c>, <c>window.open</c>) to escape the sandbox.</summary>
        public Sandbox AllowPopups(bool allow = true)     { _allowPopups = allow;     return this; }
        /// <summary>Allows the content to open modal dialogs (<c>alert</c>, <c>confirm</c>, <c>prompt</c>).</summary>
        public Sandbox AllowModals(bool allow = true)     { _allowModals = allow;     return this; }
        /// <summary>Allows downloads initiated from within the frame.</summary>
        public Sandbox AllowDownloads(bool allow = true)  { _allowDownloads = allow;  return this; }

        /// <summary>
        /// Lets the framed document share the host origin. This <b>weakens the sandbox</b> - it is only
        /// needed when the host has to reach into <c>contentDocument</c> / <c>contentWindow</c> (for example
        /// to hook events or measure layout). Do not combine with untrusted content.
        /// </summary>
        public Sandbox AllowSameOrigin(bool allow = true) { _allowSameOrigin = allow; return this; }

        /// <summary>Adds a raw sandbox token (e.g. <c>"allow-pointer-lock"</c>) on top of the configured flags.</summary>
        public Sandbox AllowToken(string token)
        {
            if (!string.IsNullOrWhiteSpace(token)) _extraSandboxTokens.Add(token.Trim());
            return this;
        }

        /// <summary>Replaces the computed sandbox value with an explicit one.</summary>
        public Sandbox SandboxAttribute(string value)
        {
            _sandboxOverride = value;
            _disableSandbox  = false;
            return this;
        }

        /// <summary>Removes the sandbox attribute entirely - the frame is no longer sandboxed. Use with extreme care.</summary>
        public Sandbox Unsandboxed()
        {
            _disableSandbox = true;
            return this;
        }

        #endregion

        #region Content-Security-Policy

        /// <summary>Overrides the injected Content-Security-Policy (only applied to <c>srcdoc</c> HTML content).</summary>
        public Sandbox ContentSecurityPolicy(string policy)
        {
            _csp       = policy;
            _injectCsp = !string.IsNullOrWhiteSpace(policy);
            return this;
        }

        /// <summary>Disables injecting a Content-Security-Policy meta tag into the document.</summary>
        public Sandbox NoContentSecurityPolicy()
        {
            _injectCsp = false;
            return this;
        }

        #endregion

        #region Layout

        /// <summary>Grows the frame to match its content height (driven by height messages from the bootstrap script).</summary>
        public Sandbox FitHeightToContent(bool fit = true)
        {
            _fitHeightToContent = fit;
            if (_built)
            {
                InnerElement.setAttribute("scrolling", fit ? "no" : (_scrolling ? "auto" : "no"));
            }
            return this;
        }

        /// <summary>Enables or disables scrolling within the frame.</summary>
        public Sandbox Scrolling(bool enabled)
        {
            _scrolling = enabled;
            if (_built) InnerElement.setAttribute("scrolling", enabled ? "auto" : "no");
            return this;
        }

        #endregion

        #region Hooks

        /// <summary>
        /// Invoked every time the frame finishes loading, with the underlying iframe element. This is the
        /// place to hook into the document - but reading <c>contentDocument</c> requires <see cref="AllowSameOrigin"/>.
        /// </summary>
        public Sandbox OnLoaded(Action<HTMLIFrameElement> onLoaded)
        {
            _onLoaded = onLoaded;
            return this;
        }

        /// <summary>
        /// Invoked when the sandboxed content reports an uncaught error, an unhandled promise rejection or a
        /// CSP violation back over the post-message channel.
        /// </summary>
        public Sandbox OnError(Action<SandboxError> onError)
        {
            _onError = onError;
            return this;
        }

        /// <summary>
        /// Invoked with the payload of any custom message the content sends via <c>window.tssSandbox.post(...)</c>.
        /// </summary>
        public Sandbox OnMessage(Action<object> onMessage)
        {
            _onMessage = onMessage;
            return this;
        }

        /// <summary>Sends a message into the sandboxed document. Received inside the frame as a <c>tss:message</c> window event.</summary>
        public Sandbox PostMessage(object message)
        {
            Script.Write("(function(f,m){ try { if (f.__tssPort) { f.__tssPort.postMessage(m); } } catch(e){} })({0}, {1});", InnerElement, message);
            return this;
        }

        #endregion

        /// <summary>Renders the component's root iframe element.</summary>
        public override HTMLElement Render()
        {
            if (!_built)
            {
                Build();
                _built = true;
            }
            return InnerElement;
        }

        private void Build()
        {
            if (!_disableSandbox)
            {
                InnerElement.setAttribute("sandbox", _sandboxOverride ?? BuildSandboxValue());
            }

            InnerElement.setAttribute("scrolling", (_fitHeightToContent || !_scrolling) ? "no" : "auto");

            // Re-dispatch port messages as a DOM CustomEvent so we can handle them from managed code without
            // passing a delegate through Script.Write.
            InnerElement.addEventListener("tss-sandbox-msg", e =>
            {
                DispatchMessage(Script.Write<object>("{0}.detail", e));
            });

            InnerElement.onload = _ =>
            {
                SetupMessaging();
                SetupHostSideFit();
                _onLoaded?.Invoke(InnerElement);
            };

            ApplyContent();
        }

        private void ApplyContent()
        {
            if (_url is object)
            {
                InnerElement.removeAttribute("srcdoc");
                InnerElement.src = _url;
            }
            else
            {
                InnerElement.removeAttribute("src");
                InnerElement.setAttribute("srcdoc", ComposeSrcDoc(_html ?? ""));
            }
        }

        private string BuildSandboxValue()
        {
            var tokens = new List<string>();

            if (_allowScripts)    tokens.Add("allow-scripts");
            if (_allowForms)      tokens.Add("allow-forms");
            if (_allowSameOrigin) tokens.Add("allow-same-origin");
            if (_allowPopups)     tokens.Add("allow-popups");
            if (_allowModals)     tokens.Add("allow-modals");
            if (_allowDownloads)  tokens.Add("allow-downloads");

            foreach (var token in _extraSandboxTokens)
            {
                if (!tokens.Contains(token)) tokens.Add(token);
            }

            return string.Join(" ", tokens);
        }

        private string ComposeSrcDoc(string html)
        {
            var head = new StringBuilder();

            if (_injectCsp && !string.IsNullOrWhiteSpace(_csp))
            {
                head.Append("<meta http-equiv=\"Content-Security-Policy\" content=\"").Append(_csp).Append("\">");
            }

            // The bootstrap is only meaningful when scripts can run, but injecting it otherwise is harmless.
            if (_allowScripts || !_disableSandbox)
            {
                head.Append("<script>").Append(BootstrapScript).Append("</script>");
            }

            var inject = head.ToString();

            return inject.Length == 0 ? html : InjectIntoHead(html, inject);
        }

        // Inserts `inject` as the first thing inside the document head so the CSP meta takes effect before
        // any other content. Falls back to wrapping fragments in a minimal document.
        private static string InjectIntoHead(string html, string inject)
        {
            if (string.IsNullOrEmpty(html))
            {
                return "<!DOCTYPE html><html><head>" + inject + "</head><body></body></html>";
            }

            var headIndex = IndexOfTag(html, "head");

            if (headIndex >= 0)
            {
                var close = html.IndexOf('>', headIndex);
                if (close >= 0)
                {
                    return html.Substring(0, close + 1) + inject + html.Substring(close + 1);
                }
            }

            var htmlIndex = IndexOfTag(html, "html");

            if (htmlIndex >= 0)
            {
                var close = html.IndexOf('>', htmlIndex);
                if (close >= 0)
                {
                    return html.Substring(0, close + 1) + "<head>" + inject + "</head>" + html.Substring(close + 1);
                }
            }

            return "<!DOCTYPE html><html><head>" + inject + "</head><body>" + html + "</body></html>";
        }

        // Case-insensitive search for an opening tag (e.g. "<head" / "<head ").
        private static int IndexOfTag(string html, string tag)
        {
            var needle = "<" + tag;
            var lower  = html.ToLower();
            var idx    = lower.IndexOf(needle, StringComparison.Ordinal);

            while (idx >= 0)
            {
                var next = idx + needle.Length;
                if (next >= lower.Length) return idx;
                var c = lower[next];
                if (c == '>' || c == ' ' || c == '\t' || c == '\r' || c == '\n') return idx;
                idx = lower.IndexOf(needle, next, StringComparison.Ordinal);
            }

            return -1;
        }

        private void SetupMessaging()
        {
            if (_disableSandbox && !_allowScripts) return;

            // The in-iframe bootstrap only needs to drive height fitting when we cannot measure the frame
            // from the host side. A same-origin frame is measured host-side (see SetupHostSideFit), which
            // also works when scripts are disabled inside the frame, so we don't arm the in-iframe reporter
            // there and avoid two mechanisms fighting over the height.
            var fit = _fitHeightToContent && !_allowSameOrigin;

            Script.Write(@"(function(f, fit){
  try {
    var ch = new MessageChannel();
    f.__tssPort = ch.port1;
    ch.port1.onmessage = function(ev){ f.dispatchEvent(new CustomEvent('tss-sandbox-msg', { detail: ev.data })); };
    if (f.contentWindow) { f.contentWindow.postMessage({ __tssSandboxInit: true, fit: fit }, '*', [ch.port2]); }
  } catch (e) { /* cross-origin frame without our bootstrap - messaging is unavailable */ }
})({0}, {1});", InnerElement, fit);
        }

        // When the host shares the frame's origin it can measure the document directly. This keeps
        // FitHeightToContent working even when scripts are disabled inside the frame (the injected
        // bootstrap never runs without allow-scripts, so it cannot report the height itself). Mirrors the
        // in-iframe reporter: forces html/body to auto height so the measurement tracks content in both
        // directions, watches with a ResizeObserver + MutationObserver, and batches updates through
        // requestAnimationFrame.
        private void SetupHostSideFit()
        {
            if (!_fitHeightToContent || !_allowSameOrigin) return;

            Script.Write(@"(function(f){
  try {
    var doc = f.contentDocument; if (!doc) return;
    var win = f.contentWindow || window;
    var de = doc.documentElement, b = doc.body;
    try { if (de) { de.style.setProperty('height', 'auto', 'important'); } if (b) { b.style.setProperty('height', 'auto', 'important'); } } catch (_) {}
    var lastH = -1, raf = 0;
    // Measure the actual content box (documentElement is forced to height:auto above) rather than
    // scrollHeight: the root element's scrollHeight is clamped to at least the viewport height, which
    // equals the frame's current height, so once the frame has grown it could never shrink again.
    function measure(){ try { var h = b ? b.scrollHeight : 0; if (de) { var r = de.getBoundingClientRect(); if (r && r.height > h) { h = Math.ceil(r.height); } } return h; } catch (_) { return 0; } }
    function apply(){ raf = 0; var h = measure(); if (h > 0 && h !== lastH) { lastH = h; f.style.height = h + 'px'; } }
    function schedule(){ if (raf) return; raf = win.requestAnimationFrame ? win.requestAnimationFrame(apply) : setTimeout(apply, 16); }
    try { if (win.ResizeObserver && de) { new win.ResizeObserver(schedule).observe(de); } } catch (_) {}
    try { if (win.MutationObserver) { new win.MutationObserver(schedule).observe(b || de, { attributes: true, childList: true, subtree: true, characterData: true }); } } catch (_) {}
    apply();
  } catch (_) { /* cross-origin document - not reachable from the host */ }
})({0});", InnerElement);
        }

        private void DispatchMessage(object data)
        {
            if (data is null) return;

            var kind = Script.Write<string>("({0} && {0}.kind) || ''", data);

            switch (kind)
            {
                case "height":
                {
                    if (_fitHeightToContent)
                    {
                        var h = Script.Write<int>("({0}.height | 0)", data);
                        if (h > 0) InnerElement.style.height = h + "px";
                    }
                    break;
                }
                case "message":
                {
                    _onMessage?.Invoke(Script.Write<object>("{0}.data", data));
                    break;
                }
                case "error":
                case "csp":
                case "unhandledrejection":
                {
                    if (_onError is object)
                    {
                        _onError(new SandboxError(
                            kind,
                            Script.Write<string>("{0}.message || ''", data),
                            Script.Write<string>("{0}.source  || ''", data),
                            Script.Write<int>("{0}.line   | 0", data),
                            Script.Write<int>("{0}.column | 0", data),
                            Script.Write<string>("{0}.stack   || ''", data)));
                    }
                    break;
                }
            }
        }
    }

    /// <summary>An error, unhandled rejection or CSP violation reported back from a <see cref="Sandbox"/>'s content.</summary>
    [Transpose.Name("tss.SandboxError")]
    public sealed class SandboxError
    {
        internal SandboxError(string kind, string message, string source, int line, int column, string stack)
        {
            Kind    = kind;
            Message = message;
            Source  = source;
            Line    = line;
            Column  = column;
            Stack   = stack;
        }

        /// <summary>One of <c>error</c>, <c>unhandledrejection</c> or <c>csp</c>.</summary>
        public string Kind    { get; }
        /// <summary>Human-readable error message.</summary>
        public string Message { get; }
        /// <summary>The source file the error originated from, when available.</summary>
        public string Source  { get; }
        /// <summary>Line number, when available.</summary>
        public int    Line    { get; }
        /// <summary>Column number, when available.</summary>
        public int    Column  { get; }
        /// <summary>JavaScript stack trace, when available.</summary>
        public string Stack   { get; }

        /// <summary>True for a Content-Security-Policy violation report.</summary>
        public bool IsContentSecurityPolicyViolation => Kind == "csp";

        /// <summary>Returns a single-line, human readable description of the error.</summary>
        public override string ToString()
        {
            var location = string.IsNullOrEmpty(Source) ? "" : $" ({Source}:{Line}:{Column})";
            return $"[{Kind}] {Message}{location}";
        }
    }
}
