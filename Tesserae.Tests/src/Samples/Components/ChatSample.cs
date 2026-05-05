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

                var msgComponent = ChatMessage(TextBlock(""), Avatar(null, "AI"), copyButton).MaxWidth();
                chatArea.Add(msgComponent);

                int index = 0;
                string currentText = "";

                void TypeNextWord()
                {
                    if (cancelled)
                    {
                        input.IsGenerating = false;
                        return;
                    }

                    if (index >= words.Length)
                    {
                        input.IsGenerating = false;
                        return;
                    }

                    currentText += (index > 0 ? " " : "") + words[index];
                    msgComponent.ReplaceContent(TextBlock(currentText));
                    msgComponent.KeepVisible();

                    index++;
                    window.setTimeout(_ => TypeNextWord(), 150);
                }

                window.setTimeout(_ => TypeNextWord(), 500);
            }

            // Preload some messages
            chatArea.Add(ChatMessage(TextBlock("Hello there!"), Avatar(null, "U")).RightAligned().MaxWidth());
            chatArea.Add(ChatMessage(TextBlock("Hi! How can I help you today?"), Avatar(null, "AI")).MaxWidth());

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

                chatArea.Add(ChatMessage(TextBlock(text), Avatar(null, "U")).RightAligned().MaxWidth());

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
