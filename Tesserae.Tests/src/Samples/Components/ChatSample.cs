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

            var input = OmniBox(new OmniBox.Config(OmniBox.Mode.Search)
            {
                PlaceholderSearch = "Message AI...",
            });

            var sendBtn = Button(UIcons.PaperPlane).Primary().OnClick(() =>
            {
                var text = input.SearchText;
                if (string.IsNullOrWhiteSpace(text)) return;

                chatArea.Add(ChatMessage(TextBlock(text), Avatar(null, "U")).RightAligned().MaxWidth());
                input.SearchText = "";

                AddAIAnswer();
            });

            // Adjust the send button to be icon-only for a cleaner look
            sendBtn.Render().classList.remove("tss-btn-default");
            sendBtn.Render().classList.add("tss-btn-icon-only");
            sendBtn.Render().style.borderRadius = "50%";
            sendBtn.Render().style.width = "40px";
            sendBtn.Render().style.height = "40px";
            sendBtn.Render().style.display = "flex";
            sendBtn.Render().style.alignItems = "center";
            sendBtn.Render().style.justifyContent = "center";


            input.OnKeyDown((sender, e) =>
            {
                if (e.key == "Enter")
                {
                    sendBtn.Render().click();
                }
            });

            var inputContainer = HStack().Padding("16px 24px").AlignItemsCenter().Children(
                input.Width(100.percent()).Grow(),
                sendBtn.MarginLeft(12.px())
            );

            var chatContainer = VStack().Height(70.vh()).Children(
                chatArea.Grow(),
                inputContainer
            );

            _content = SectionStack()
                .Title(SampleHeader(nameof(ChatSample)))
                .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("ChatArea and ChatMessage components allow building modern, ChatGPT/Gemini-like chat experiences with dynamic, animatable messages using DeltaComponent."),
                    chatContainer.MarginTop(16.px())
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
