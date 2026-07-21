using System;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 101, Icon = UIcons.CommentAlt)]
    public class ChatSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ChatSample()
        {
            var chatArea = ChatArea();

            var predefinedAnswers = new[]
            {
                "That's a very interesting question. Let me think about it for a second...",
                "I completely agree with you on that point. Here's some more context.",
                "Here are a few ways we could tackle this problem. First, we could...',",
                "I am not entirely sure, but based on the available data...",
                "Excellent! Let's proceed with the proposed plan."
            };

            var random = new Random();

            // Shared cancellation flag — set to true to abort the current typing animation
            var cancelled = false;

            OmniBox input = null;

            void AddAIAnswer()
            {
                cancelled = false;

                var answer = predefinedAnswers[random.Next(predefinedAnswers.Length)];
                var words = answer.Split(' ');

                var copyButton = Button(UIcons.Copy).NoBorder().NoBackground().Tooltip("Copy");
                copyButton.Render().classList.remove("tss-btn-default");
                copyButton.Render().classList.add("tss-btn-icon-only");

                var msgComponent = ChatMessage(TextBlock(""), Avatar(null, "AI"), copyButton).Animated().MaxWidth();
                chatArea.Add(msgComponent);
                chatArea.Busy(true);

                int index = 0;
                string currentText = "";

                void TypeNextWord()
                {
                    if (cancelled)
                    {
                        input.IsGenerating = false;
                        chatArea.Busy(false);
                        return;
                    }

                    if (index >= words.Length)
                    {
                        input.IsGenerating = false;
                        chatArea.Busy(false);
                        return;
                    }

                    currentText += (index > 0 ? " " : "") + words[index];

                    // Rebuild a "Used N tools" pill above the streaming text on every token, exactly as a
                    // real streaming chat does, so the summary can be clicked while the reply is still
                    // arriving. Combined with the random per-token delay below, this is the scenario to
                    // exercise: the pill must stay clickable even as the content re-renders and scrolls.
                    var toolsPill = ToolsUsed(
                        ToolCall(UIcons.Terminal, "Bash ls -la && git status", () => TextBlock("On branch main\nnothing to commit").BreakSpaces()),
                        ToolCall(UIcons.Eye,      "Read /home/user/project/README.md", () => TextBlock("# Project\n\nA sample app.").BreakSpaces()),
                        ToolCall(UIcons.Search,   "Grep \"TODO\" src/", () => TextBlock("src/App.tsx:14: // TODO: add routing").BreakSpaces()));

                    msgComponent.ReplaceContent(VStack().WS().Children(toolsPill, TextBlock(currentText)));
                    msgComponent.KeepVisible();

                    index++;

                    // Random 0-1000ms gap between tokens so the stream pauses long enough to click mid-reply.
                    window.setTimeout(_ => TypeNextWord(), random.Next(0, 1001));
                }

                window.setTimeout(_ => TypeNextWord(), 500);
            }

            // Preload some messages
            chatArea.Add(ChatMessage(TextBlock("Hello there!"), Avatar(null, "U")).RightAligned().MaxWidth());
            chatArea.Add(ChatMessage(TextBlock("Hi! How can I help you today?"), Avatar(null, "AI")).MaxWidth());

            // Demonstrate an AI message that uses a single tool inline via ToolCall.
            chatArea.Add(ChatMessage(VStack().WS().Children(
                ToolCall(UIcons.Eye, "Read /home/user/project/src/App.tsx", () => TextBlock("import React from 'react';\n\nexport default function App() {\n    return <div>Hello</div>;\n}").BreakSpaces()),
                TextBlock("I just read App.tsx — it's a minimal React component. Want me to add routing?")
            ), Avatar(null, "AI")).MaxWidth());

            // Demonstrate an AI message that used many tools, summarized via ToolsUsed.
            chatArea.Add(ChatMessage(VStack().WS().Children(
                ToolsUsed(
                    ToolCall(UIcons.Terminal, "Bash ls -la && git status",                       () => TextBlock("On branch main\nnothing to commit, working tree clean").BreakSpaces()),
                    ToolCall(UIcons.Eye,      "Read /home/user/project/README.md",              () => TextBlock("# Project\n\nA sample app.").BreakSpaces()),
                    ToolCall(UIcons.Search,   "Grep \"TODO\" src/",                              () => TextBlock("src/App.tsx:14: // TODO: add routing").BreakSpaces()),
                    ToolCall(UIcons.Terminal, "Bash dotnet build",                                () => TextBlock("Build succeeded.\n    0 Warning(s)\n    0 Error(s)").BreakSpaces()),
                    ToolCall(UIcons.Eye,      "Read /home/user/project/src/index.tsx",          () => TextBlock("import { createRoot } from 'react-dom/client';\n...").BreakSpaces()),
                    ToolCall(UIcons.Tools,    "ToolSearch routing",                              () => TextBlock("Found: react-router-dom v6")),
                    ToolCall(UIcons.ListCheck, "Update todos").NotExpandable()
                ).SetSummary("Ran 4 commands, read 2 files, used a tool"),
                TextBlock("I scanned the project, ran the build, and looked at routing options. Here's what I found:")
            ), Avatar(null, "AI")).MaxWidth());

            input = OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat = "Ask anything...",
                IconStop = UIcons.Stop,
                IconChat = UIcons.ArrowRight
            })
            .OnChat((sender, msg) =>
            {
                var text = msg.Text;
                if (string.IsNullOrWhiteSpace(text)) return;

                // Anchor the user's turn: it settles near the top and the reply grows into the screen below it.
                chatArea.Add(ChatMessage(TextBlock(text), Avatar(null, "U")).RightAligned().MaxWidth().ScrollAnchor());

                sender.IsGenerating = true;
                AddAIAnswer();
            })
            .OnStop((sender) =>
            {
                cancelled = true;
                sender.IsGenerating = false;
            });

            var chatContainer = VStack().WS().H(10).Grow().Children(
                chatArea.WS().H(10).Grow(),
                input.WS().H(150)
            );

            _content = SectionStack().Secondary()
                .SampleTitle(typeof(ChatSample), UIcons.Comments, "A component to display a chat")
                .FlatSection(Card(VStack().S().Children(
                    TextBlock("ChatArea and ChatMessage components allow building modern chat experiences with dynamic, animatable messages using DeltaComponent."),
                    chatContainer.MT(16)
                )).SetTitle("Overview").S(), grow: true);
        }

        public HTMLElement Render() => _content.Render();
    }
}
