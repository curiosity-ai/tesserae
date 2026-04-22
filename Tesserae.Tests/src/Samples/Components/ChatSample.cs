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
            var chatArea = ChatArea(); // Removed hardcoded background

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

                var copyButton = Button(UIcons.Copy).NoBorder().NoBackground().Tooltip("Copy");
                copyButton.Render().classList.remove("tss-btn-default");
                copyButton.Render().classList.add("tss-btn-icon-only");

                var msgComponent = ChatMessage(TextBlock(""), Avatar(null, "AI"), copyButton).MaxWidth();
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
            chatArea.Add(ChatMessage(TextBlock("Hello there!"), Avatar(null, "U")).RightAligned().MaxWidth());
            chatArea.Add(ChatMessage(TextBlock("Hi! How can I help you today?"), Avatar(null, "AI")).MaxWidth());

            var input = OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderSearch = "Ask anything...",
            }).OnChat((sender,msg) =>
            {
                var text = msg.Text;
                if (string.IsNullOrWhiteSpace(text)) return;

                chatArea.Add(ChatMessage(TextBlock(text), Avatar(null, "U")).RightAligned().MaxWidth());

                AddAIAnswer();
            });

            var chatContainer = VStack().Height(70.vh()).Children(
                chatArea.Grow(),
                input.WS().Grow()
            );

            _content = SectionStack()
                .SampleTitle(nameof(ChatSample), UIcons.Comments, "A component to display a chat")
                .Section(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("ChatArea and ChatMessage components allow building modern chat experiences with dynamic, animatable messages using DeltaComponent."),
                    chatContainer.MT(16)
                )).SetTitle("Overview")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
