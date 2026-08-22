using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.AI, Order = 40, Icon = UIcons.Tools)]
    public class ToolCallSample : IComponent, ISample
    {
        // Handed over compact on purpose: ToolCallInspect re-indents JSON as it is set.
        private const string CONSULT_ARGUMENTS = @"{ ""fileUID"": ""WAwweAZJPrs6nE95L25GbX"", ""page"": 188, ""maxCharacters"": 8000, ""extractTables"": true }";

        private const string CONSULT_RESPONSE = @"{ ""uid"": ""WAwweAZJPrs6nE95L25GbX"", ""name"": ""SETR2025_web-240128.pdf"", ""source"": ""Mailbox"", ""contentType"": ""application/pdf"", ""extension"": ""pdf"", ""sizeBytes"": 13199821, ""language"": ""English"", ""pages"": 240, ""extracted"": { ""page"": 188, ""characters"": 8000, ""text"": ""ACKNOWLEDGMENTS\n\nThe review was prepared with the help of a long list of contributors, whose names run over the next several pages and wrap rather than scroll sideways in the response block."" } }";

        private const string FETCH_ARGUMENTS = @"{ ""url"": ""https://api.example.com/v1/status"", ""method"": ""GET"", ""timeoutMs"": 30000 }";

        private readonly IComponent _content;

        private readonly ToolCall                     _runningCall;
        private readonly LiveProgress                 _standaloneProgress;
        private readonly DeltaComponent               _diffedBubble;
        private readonly ToolsUsed                    _inlineTools;
        private readonly SettableObservable<string>   _streamedProgress = new SettableObservable<string>(string.Empty);
        private          double                       _timer;
        private          bool                         _diffedCallOpen;
        private          int                          _addedTools;

        public ToolCallSample()
        {
            _runningCall        = ToolCall(UIcons.Search, "Search documentation \"tesserae popover\"", () => TextBlock("3 pages matched.").BreakSpaces());
            _standaloneProgress = LiveProgress().Stream(_streamedProgress);
            _diffedBubble       = DeltaComponent(BuildDiffedBubbleContent()).Animated();

            _inlineTools = ToolsUsed(
                    ToolCall(UIcons.Search, "Grep \"Inline\" Tesserae/src/", () => TextBlock("Tesserae/src/Components/ToolCall.cs:  public ToolsUsed Inline(bool value = true)").BreakSpaces()),
                    ToolCall(UIcons.Terminal, "Bash git log --oneline -3", () => TextBlock("a1b2c3d Add inline mode to ToolsUsed\n4e5f6a7 Add ToolCallInspect\n8b9c0d1 Add ToolCall").BreakSpaces()))
               .Inline()
               .Expanded();

            _runningCall.SetProgress(_streamedProgress);

            _content = SectionStack().Secondary()
                .SampleTitle(typeof(ToolCallSample), UIcons.Tools, "Inline tool-call indicators and a multi-tool summary popup")
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("ToolCall renders a single tool invocation inline. It behaves like an accordion: a compact header with an icon and label, expanding to reveal arbitrary content the first time it is clicked (the content component is created lazily). A ToolCall without content automatically renders as a plain, non-expandable chip — no chevron is shown until content is set."),
                        TextBlock("ToolsUsed groups many ToolCalls behind a compact summary. Clicking it opens a popup with the list of tools on the left; selecting one slides to the detail view on the right, with a back button to return to the list. Inline() keeps it all in place instead: the summary expands into the calls underneath itself, each one opening its own content inline."),
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

                        SampleSubTitle("Inline instead of a popup"),
                        TextBlock("Inline() keeps the group where it is: the summary becomes an accordion that expands into the list of calls underneath itself, each one opening its own content inline the way a standalone ToolCall does. For a transcript where sending the reader to a modal for a one-line result is too much ceremony."),
                        ToolsUsed(
                            ToolCall(UIcons.Terminal, "Bash dotnet build", () => TextBlock("Build succeeded.\n    0 Warning(s)\n    0 Error(s)").BreakSpaces()),
                            ToolCall(UIcons.Eye, "Read /home/user/needle/README.md", () => TextBlock("# Needle\n\nA tiny ML library written in C#.").BreakSpaces()),
                            ToolCall(UIcons.FilePdf, "Consult 'SETR2025_web-240128.pdf'", () => ToolCallInspect(CONSULT_ARGUMENTS, CONSULT_RESPONSE)),
                            ToolCall(UIcons.ListCheck, "Update todos") // still a plain, non-expandable row inside the list
                        ).SetSummary("Used 4 tools").Inline(),

                        TextBlock("Expanded() opens the list from the start, and a call arriving while it is open joins the list on screen - the way a live transcript appends to it as the calls come in."),
                        _inlineTools,
                        Button("Add a tool").OnClick(() => AddInlineTool()),

                        SampleSubTitle("A button beside the call"),
                        TextBlock("AddAction hangs an icon button off the right of the call, outside the chip, for a way into what the call stands for that isn't its content - the run it started, say. It sits at a third of its strength until the pointer is anywhere on the call, so a transcript of them reads as calls rather than as a column of controls. Clicking it runs the handler only: the call is neither expanded nor collapsed by it. A ToolsUsed group takes the same button beside its summary pill."),
                        ToolCall(UIcons.UserRobot, "Invoke agent \"researcher\"", () => TextBlock("Summarised 4 sources.").BreakSpaces())
                           .AddAction(UIcons.Eye, "Watch this agent run", () => Toast().Information("Opening the run...")),
                        ToolsUsed(
                            ToolCall(UIcons.Search, "Search documentation", () => TextBlock("3 pages matched.").BreakSpaces()),
                            ToolCall(UIcons.Eye, "Read README.md", () => TextBlock("# Needle").BreakSpaces())
                        ).SetSummary("researcher").Inline()
                         .AddAction(UIcons.Eye, "Watch this agent run", () => Toast().Information("Opening the run...")),

                        SampleSubTitle("Live progress while a call runs"),
                        TextBlock("A ToolCall can carry a LiveProgress line on its header row: SetProgress writes into the line already on screen, so a stream of updates never re-renders the call and never replays an animation. Hovering the line shows its full text; expanding the call still opens the content full width underneath."),
                        _runningCall,
                        TextBlock("The same line stands on its own before any tool has been called."),
                        _standaloneProgress,

                        SampleSubTitle("Inside a diffing container"),
                        TextBlock("A streaming chat bubble refreshes itself by diffing a freshly built layout onto the DOM already on screen, so the call and its line are rebuilt on every update and the reader keeps the elements an earlier layout left behind. The line still reads as one text changing: it opts out of the fade the diff puts on patched content, and its tooltip follows the element rather than the instance that was last written to."),
                        TextBlock("The update that ends the run is a rebuild like all the others - it just carries an empty progress, which takes the line off the row instead of leaving an empty gap where it was."),
                        _diffedBubble,

                        Button("Stream progress").Primary().OnClick(() => StartProgressDemo())
                    )).SetTitle("Usage")))
                .SeeAlso(typeof(ChatSample), typeof(PlanSample), typeof(ContextCardSample), typeof(OmniBoxSample), typeof(CodeDiffSample), typeof(MarkdownBlockSample));
        }

        // Rebuilt from scratch on every update, the way a chat view rebuilds an in-flight reply. The
        // line takes the current progress as a plain string: a rebuilt call is a fresh instance whose
        // element the diff throws away, and Stream()ing into one would leave a subscription per
        // rebuild writing into a detached element. Two things a rebuild has to carry itself: the call
        // the reader opened (a rebuilt one is collapsed with its content unbuilt, and the diff would
        // take the open one on screen down to match), and the progress, which only reaches this
        // bubble through a rebuild.
        private IComponent BuildDiffedBubbleContent()
        {
            var call = ToolCall(UIcons.Database, "Fetch index statistics", () => TextBlock("4 indexes, 1.2M documents.").BreakSpaces())
               .SetProgress(_streamedProgress.Value)
               .OnToggle(c => _diffedCallOpen = c.IsExpanded);

            if (_diffedCallOpen) call.Expanded();

            return VStack().NoDefaultMargin().Children(call);
        }

        // A call appended to a group that is already open inline lands in the list on screen, without the
        // group being rebuilt around it.
        private void AddInlineTool()
        {
            _addedTools++;

            _inlineTools
               .Add(ToolCall(UIcons.Globe, $"Fetch https://api.example.com/v1/items?page={_addedTools}",
                             () => ToolCallInspect($@"{{ ""page"": {_addedTools}, ""timeoutMs"": 30000 }}", @"{ ""items"": [], ""hasMore"": false }")))
               .SetSummary($"Used {2 + _addedTools} tools")
               .Expand();
        }

        // Walks the demo through a few stages and then ends it with an empty progress. The lines on
        // screen are never rebuilt for an update - only their text changes.
        private void StartProgressDemo()
        {
            window.clearInterval(_timer);

            var step = 0;

            Publish("Reading documents · 0%");

            _timer = window.setInterval(_ =>
            {
                step += 5;

                if (step > 100)
                {
                    window.clearInterval(_timer);
                    Publish(string.Empty);
                    return;
                }

                Publish(step < 50
                    ? $"Reading documents · {step}%"
                    : $"Encoding chunks · {step}%");
            }, 150);
        }

        // The one path every update takes, the empty one that ends the run included: the observable is
        // the state, so publishing carries the progress into the lines already on screen and the
        // diffing bubble is rebuilt from the same value.
        private void Publish(string progress)
        {
            _streamedProgress.Value = progress;

            _diffedBubble.ReplaceContent(BuildDiffedBubbleContent());
        }

        public HTMLElement Render() => _content.Render();
    }
}
