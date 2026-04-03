using System;
using System.Linq;
using static H5.Core.dom;
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
            var chatArea = ChatArea().Background("var(--tss-colors-neutral-100)");

            var predefinedAnswers = new[]
            {
                "That's a very interesting question. Let me think about it for a second...",
                "I completely agree with you on that point. Here's some more context.",
                "Here are a few ways we could tackle this problem. First, we could...',",
                "I am not entirely sure, but based on the available data...",
                "Excellent! Let's proceed with the proposed plan."
            };

            var random = new Random();

            void AddAIAnswer()
            {
                var answer = predefinedAnswers[random.Next(predefinedAnswers.Length)];
                var words = answer.Split(' ');

                var msgComponent = ChatMessage(TextBlock(""), Avatar(null, "AI"), Button(UIcons.Copy).NoBorder().NoBackground().OnClick(() => { /* mock copy */ })).MaxWidth();
                chatArea.Add(msgComponent);

                int index = 0;
                string currentText = "";

                void TypeNextWord()
                {
                    if (index >= words.Length) return;

                    currentText += (index > 0 ? " " : "") + words[index];
                    msgComponent.ReplaceContent(TextBlock(currentText));
                    msgComponent.KeepVisible();

                    index++;
                    window.setTimeout(_ => TypeNextWord(), 150);
                }

                window.setTimeout(_ => TypeNextWord(), 500);
            }

            // Preload some messages
            chatArea.Add(ChatMessage(TextBlock("Hello there!"), Avatar(null, "U")).RightAligned().MaxWidth().Background("var(--tss-colors-blue-500)"));
            chatArea.Add(ChatMessage(TextBlock("Hi! How can I help you today?"), Avatar(null, "AI")).MaxWidth());

            var input = OmniBox(new OmniBox.Config(OmniBox.Mode.Search)
            {
                PlaceholderSearch = "Type a message...",
            });

            var sendBtn = Button("Send").Primary().OnClick(() =>
            {
                var text = input.SearchText;
                if (string.IsNullOrWhiteSpace(text)) return;

                chatArea.Add(ChatMessage(TextBlock(text), Avatar(null, "U")).RightAligned().MaxWidth().Background("var(--tss-colors-blue-500)"));
                input.SearchText = "";

                AddAIAnswer();
            });

            input.OnKeyDown((sender, e) =>
            {
                if (e.key == "Enter")
                {
                    sendBtn.Render().click();
                }
            });

            var inputStack = HStack().Padding("16px").AlignItemsCenter().Children(
                input.Width(100.percent()).Grow(),
                sendBtn.MarginLeft(8.px())
            );

            var chatContainer = VStack().Height(60.vh()).Children(
                chatArea.Grow(),
                inputStack
            );

            _content = SectionStack()
                .Title(SampleHeader(nameof(ChatSample)))
                .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("ChatArea and ChatMessage components allow building chat experiences with dynamic, animatable messages using DeltaComponent."),
                    chatContainer
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
