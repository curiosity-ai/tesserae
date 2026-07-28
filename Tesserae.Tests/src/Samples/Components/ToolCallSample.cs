using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 102, Icon = UIcons.Tools)]
    public class ToolCallSample : IComponent, ISample
    {
        // Handed over compact on purpose: ToolCallInspect re-indents JSON as it is set.
        private const string CONSULT_ARGUMENTS = @"{ ""fileUID"": ""WAwweAZJPrs6nE95L25GbX"", ""page"": 188, ""maxCharacters"": 8000, ""extractTables"": true }";

        private const string CONSULT_RESPONSE = @"{ ""uid"": ""WAwweAZJPrs6nE95L25GbX"", ""name"": ""SETR2025_web-240128.pdf"", ""source"": ""Mailbox"", ""contentType"": ""application/pdf"", ""extension"": ""pdf"", ""sizeBytes"": 13199821, ""language"": ""English"", ""pages"": 240, ""extracted"": { ""page"": 188, ""characters"": 8000, ""text"": ""ACKNOWLEDGMENTS\n\nThe review was prepared with the help of a long list of contributors, whose names run over the next several pages and wrap rather than scroll sideways in the response block."" } }";

        private const string FETCH_ARGUMENTS = @"{ ""url"": ""https://api.example.com/v1/status"", ""method"": ""GET"", ""timeoutMs"": 30000 }";

        private readonly IComponent _content;

        private readonly ToolCall                     _runningCall;
        private readonly LiveProgress                 _standaloneProgress;
        private readonly SettableObservable<string>   _streamedProgress = new SettableObservable<string>(string.Empty);
        private          double                       _timer;

        public ToolCallSample()
        {
            _runningCall        = ToolCall(UIcons.Search, "Search documentation \"tesserae popover\"", () => TextBlock("3 pages matched.").BreakSpaces());
            _standaloneProgress = LiveProgress().Stream(_streamedProgress);

            _runningCall.SetProgress(_streamedProgress);

            _content = SectionStack().Secondary()
                .SampleTitle(typeof(ToolCallSample), UIcons.Tools, "Inline tool-call indicators and a multi-tool summary popup")
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("ToolCall renders a single tool invocation inline. It behaves like an accordion: a compact header with an icon and label, expanding to reveal arbitrary content the first time it is clicked (the content component is created lazily). A ToolCall without content automatically renders as a plain, non-expandable chip — no chevron is shown until content is set."),
                        TextBlock("ToolsUsed groups many ToolCalls behind a compact summary. Clicking it opens a popup with the list of tools on the left; selecting one slides to the detail view on the right, with a back button to return to the list."),
                        TextBlock("ToolCallInspect is the ready-made body for a call: the arguments it was called with, one row per property, above the response it returned in a read-only code block. Each section scrolls on its own, and inside a ToolsUsed detail pane the arguments take at most half the height so a long response never scrolls them away.")
                    )).SetTitle("Overview")))
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Inline ToolCall"),
                        TextBlock("A single expandable tool call. The content factory only runs when the user first expands it."),
                        ToolCall(UIcons.Terminal, "Bash ls -la && git status", () => TextBlock("total 16\ndrwxr-xr-x  3 user user 4096 Jan 1 12:00 .\n\nOn branch main\nnothing to commit, working tree clean").BreakSpaces()),
                        ToolCall(UIcons.Eye, "Read /home/user/project/README.md", () => TextBlock("# My Project\n\nA sample project demonstrating the ToolCall component.\n\n## Usage\n\n...").BreakSpaces()).Expanded(),
                        ToolCall(UIcons.Search, "Grep \"useEffect\" src/", () => TextBlock("src/App.tsx:5: import { useEffect } from 'react';\nsrc/hooks/useData.ts:1: import { useEffect, useState } from 'react';").BreakSpaces()),
                        ToolCall(UIcons.ListCheck, "Update todos"), // no content -> renders non-expandable automatically

                        SampleSubTitle("Arguments and response"),
                        TextBlock("ToolCallInspect is the ready-made body for a call: arguments as name/value rows, response in a read-only code block. JSON is re-indented as it is set, so handing it the raw payload is enough."),
                        ToolCall(UIcons.FilePdf, "Consult 'SETR2025_web-240128.pdf'", () => ToolCallInspect(CONSULT_ARGUMENTS, CONSULT_RESPONSE)).Expanded(),
                        TextBlock("A call that failed carries its error above the response it never produced."),
                        ToolCall(UIcons.Globe, "Fetch https://api.example.com/v1/status", () => ToolCallInspect(FETCH_ARGUMENTS).SetError("HTTP 503 - the upstream did not respond within 30s")),

                        SampleSubTitle("ToolsUsed summary popup"),
                        TextBlock("When an AI uses many tools, surface a compact summary that opens a list/detail popup, similar to a master-detail navigation on mobile."),
                        ToolsUsed(
                            ToolCall(UIcons.Terminal, "Bash ls -la && git status && git branch --show-current", () => TextBlock("total 348\ndrwxr-xr-x ...\nOn branch claude/add-tool-components\nnothing to commit, working tree clean").BreakSpaces()),
                            ToolCall(UIcons.Terminal, "Bash cat Needle.slnx && echo \"---\" && ls src/ && ...", () => TextBlock("<Solution>...\n---\nNeedle/\nNeedle.Tests/").BreakSpaces()),
                            ToolCall(UIcons.Terminal, "Bash ls src/Needle/ && echo \"---\" && ls tests/N...", () => TextBlock("Inference/\nModel/\nTokenizer/\n---\nIntegration/\nUnit/").BreakSpaces()),
                            ToolCall(UIcons.Eye, "Read /home/user/needle/README.md", () => TextBlock("# Needle\n\nA tiny ML library written in C#.").BreakSpaces()),
                            ToolCall(UIcons.Terminal, "Bash cat src/Needle/Needle.csproj && echo \"---\"", () => TextBlock("<Project Sdk=\"Microsoft.NET.Sdk\">...</Project>").BreakSpaces()),
                            ToolCall(UIcons.Terminal, "Bash find src/Needle -type f | head -50", () => TextBlock("src/Needle/Needle.csproj\nsrc/Needle/Inference/Runner.cs\n...").BreakSpaces()),
                            ToolCall(UIcons.Terminal, "Bash find tests -type f && echo \"---\" && find n...", () => TextBlock("tests/Needle/Integration/RunnerTests.cs\n---").BreakSpaces()),
                            ToolCall(UIcons.Eye, "Read /home/user/needle/src/Needle/Inference/Run...", () => TextBlock("namespace Needle.Inference;\n\npublic class Runner { ... }").BreakSpaces()),
                            ToolCall(UIcons.Eye, "Read /home/user/needle/src/Needle/Weights/Weigh...", () => TextBlock("namespace Needle.Weights;\n\npublic class Weights { ... }").BreakSpaces()),
                            ToolCall(UIcons.Eye, "Read /home/user/needle/src/Needle/Model/NeedleM...", () => TextBlock("namespace Needle.Model;\n\npublic class NeedleModel { ... }").BreakSpaces()),
                            ToolCall(UIcons.FilePdf, "Consult 'SETR2025_web-240128.pdf'", () => ToolCallInspect(CONSULT_ARGUMENTS, CONSULT_RESPONSE)),
                            ToolCall(UIcons.Tools, "ToolSearch max_results, query", () => ToolCallInspect(@"{ ""query"": ""tokenizer"", ""max_results"": 3 }", @"{ ""matches"": [""Tokenize"", ""DetectLanguage"", ""CountTokens""], ""searchedTools"": 148 }")),
                            ToolCall(UIcons.ListCheck, "Update todos").NotExpandable(),
                            ToolCall(UIcons.Eye, "Read /home/user/needle/needle/model/run.py", () => TextBlock("import torch\n\ndef run(model, x): ...").BreakSpaces()),
                            ToolCall(UIcons.Eye, "Read /home/user/needle/src/Needle/Tokenizer/Nee...", () => TextBlock("namespace Needle.Tokenizer;\n\npublic class NeedleTokenizer { ... }").BreakSpaces())
                        ).SetSummary("Ran 14 commands, read 9 files, used a tool").SetTitle("Tools used"),

                        SampleSubTitle("Live progress while a call runs"),
                        TextBlock("A ToolCall can carry a LiveProgress line on its header row: SetProgress writes into the line already on screen, so a stream of updates never re-renders the call and never replays an animation. Hovering the line shows its full text; expanding the call still opens the content full width underneath."),
                        _runningCall,
                        TextBlock("The same line stands on its own before any tool has been called."),
                        _standaloneProgress,
                        Button("Stream progress").Primary().OnClick(() => StartProgressDemo())
                    )).SetTitle("Usage")));
        }

        // Walks the demo through a few stages, then clears the line - the same element stays in place
        // the whole time, only its text changes.
        private void StartProgressDemo()
        {
            window.clearInterval(_timer);

            var step = 0;

            _streamedProgress.Value = "Reading documents · 0%";

            _timer = window.setInterval(_ =>
            {
                step += 5;

                if (step > 100)
                {
                    window.clearInterval(_timer);
                    _streamedProgress.Value = string.Empty;
                    return;
                }

                _streamedProgress.Value = step < 50
                    ? $"Reading documents · {step}%"
                    : $"Encoding chunks · {step}%";
            }, 150);
        }

        public HTMLElement Render() => _content.Render();
    }
}
