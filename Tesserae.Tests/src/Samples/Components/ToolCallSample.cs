using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 102, Icon = UIcons.Tools)]
    public class ToolCallSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ToolCallSample()
        {
            _content = SectionStack().Secondary()
                .SampleTitle(typeof(ToolCallSample), UIcons.Tools, "Inline tool-call indicators and a multi-tool summary popup")
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("ToolCall renders a single tool invocation inline. It behaves like an accordion: a compact header with an icon and label, expanding to reveal arbitrary content the first time it is clicked (the content component is created lazily). A ToolCall without content automatically renders as a plain, non-expandable chip — no chevron is shown until content is set."),
                        TextBlock("ToolsUsed groups many ToolCalls behind a compact summary. Clicking it opens a popup with the list of tools on the left; selecting one slides to the detail view on the right, with a back button to return to the list.")
                    )).SetTitle("Overview")))
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Inline ToolCall"),
                        TextBlock("A single expandable tool call. The content factory only runs when the user first expands it."),
                        ToolCall(UIcons.Terminal, "Bash ls -la && git status", () => TextBlock("total 16\ndrwxr-xr-x  3 user user 4096 Jan 1 12:00 .\n\nOn branch main\nnothing to commit, working tree clean").BreakSpaces()),
                        ToolCall(UIcons.Eye, "Read /home/user/project/README.md", () => TextBlock("# My Project\n\nA sample project demonstrating the ToolCall component.\n\n## Usage\n\n...").BreakSpaces()).Expanded(),
                        ToolCall(UIcons.Search, "Grep \"useEffect\" src/", () => TextBlock("src/App.tsx:5: import { useEffect } from 'react';\nsrc/hooks/useData.ts:1: import { useEffect, useState } from 'react';").BreakSpaces()),
                        ToolCall(UIcons.ListCheck, "Update todos"), // no content -> renders non-expandable automatically

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
                            ToolCall(UIcons.Tools, "ToolSearch max_results, query", () => TextBlock("Found 3 candidate tools matching query 'tokenizer'.")),
                            ToolCall(UIcons.ListCheck, "Update todos").NotExpandable(),
                            ToolCall(UIcons.Eye, "Read /home/user/needle/needle/model/run.py", () => TextBlock("import torch\n\ndef run(model, x): ...").BreakSpaces()),
                            ToolCall(UIcons.Eye, "Read /home/user/needle/src/Needle/Tokenizer/Nee...", () => TextBlock("namespace Needle.Tokenizer;\n\npublic class NeedleTokenizer { ... }").BreakSpaces())
                        ).SetSummary("Ran 14 commands, read 9 files, used a tool").SetTitle("Tools used")
                    )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
