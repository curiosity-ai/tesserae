using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.AI, Order = 22, Icon = UIcons.TimeQuarterPast, Description = "Marks where a chat picks up again after a pause")]
    public class ChatTimeDividerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        private sealed class Message
        {
            public DateTimeOffset Time;
            public bool           FromUser;
            public string         Text;
        }

        public ChatTimeDividerSample()
        {
            //Fixed clock times on days counted back from today, so the page reads the same on every run of a day.
            var today = new DateTimeOffset(DateTime.Today);

            var messages = new[]
            {
                new Message { Time = today.AddYears(-1).AddHours(10).AddMinutes(5),  FromUser = true,  Text = "Can Catalyst detect the language of a single tweet?" },
                new Message { Time = today.AddYears(-1).AddHours(10).AddMinutes(6),  FromUser = false, Text = "Yes - the FastText language detector works on short texts, though accuracy drops under ~20 characters." },
                new Message { Time = today.AddDays(-4).AddHours(14).AddMinutes(30),  FromUser = true,  Text = "Back to this: which model should I download for German?" },
                new Message { Time = today.AddDays(-4).AddHours(14).AddMinutes(31),  FromUser = false, Text = "Use the German English-trained pipeline; it bundles the tokenizer and the sentence detector." },
                new Message { Time = today.AddDays(-1).AddHours(16).AddMinutes(20),  FromUser = true,  Text = "The sentence detector splits on 'z.B.' - is that expected?" },
                new Message { Time = today.AddDays(-1).AddHours(16).AddMinutes(22),  FromUser = false, Text = "It is a missing abbreviation. Add it to the abbreviation list and retrain." },
                new Message { Time = today.AddHours(9).AddMinutes(12),               FromUser = true,  Text = "Retrained, the split is gone. Anything else to watch for?" },
                new Message { Time = today.AddHours(9).AddMinutes(55),               FromUser = false, Text = "Ordinal numbers like '3.' at the end of a line - check a few of those too." },
                new Message { Time = today.AddHours(18).AddMinutes(7),               FromUser = true,  Text = "Perfect! New PR for this work?" },
            };

            var transcript = ChatArea().DefaultScrollPosition(ChatArea.StartPosition.Start);
            transcript.Render().classList.add("tss-sample-chat-time-divider");

            void Render(TimeSpan? gap)
            {
                transcript.Clear();

                for (int i = 0; i < messages.Length; i++)
                {
                    var message = messages[i];

                    //The first message gets one too: a transcript opening on its first day says which day that was.
                    if (gap.HasValue && (i == 0 || ChatTimeDivider.IsGap(messages[i - 1].Time, message.Time, gap.Value)))
                    {
                        transcript.Add(ChatTimeDivider(message.Time));
                    }

                    var bubble = ChatMessage(TextBlock(message.Text), Avatar(null, message.FromUser ? "U" : "AI"));

                    transcript.Add(message.FromUser ? bubble.RightAligned() : bubble);
                }
            }

            var gapPicker = ChoiceGroup()
               .Horizontal()
               .Choices(
                    Choice("30 minutes").OnSelected(_ => Render(TimeSpan.FromMinutes(30))),
                    Choice("1 hour").Selected().OnSelected(_ => Render(TimeSpan.FromHours(1))),
                    Choice("3 hours").OnSelected(_ => Render(TimeSpan.FromHours(3))),
                    Choice("Off").OnSelected(_ => Render(null)));

            Render(ChatTimeDivider.DefaultGap);

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ChatTimeDividerSample), UIcons.TimeQuarterPast, "Marks where a chat picks up again after a pause")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("A ChatTimeDivider is a quiet wavy rule with the time between two messages of a chat transcript, drawn where the conversation picked up again after a pause. ChatTimeDivider.IsGap(previous, next, gap) is the test for where: the pause is longer than the gap (an hour by default), or the two messages fall on different days. The time is formatted in the browser's locale and relative to today - the time alone today, then Yesterday, the weekday within the last week, and the date before that."),
                        TextBlock("The divider is a separator to assistive technology, labelled with the same text; hovering it shows the full date.").PaddingTop(8.px())
                    )).SetTitle("Overview"),
                    Card(VStack().WS().Children(
                        HStack().WS().AlignItemsCenter().Children(TextBlock("Divider after a pause of").SemiBold().PaddingRight(12.px()), gapPicker),
                        VStack().WS().MaxWidth(720.px()).H(640).PaddingTop(8.px()).Children(transcript.S())
                    )).SetTitle("In a transcript"),
                    Card(VStack().WS().Children(
                        ChatTimeDivider(today.AddHours(18).AddMinutes(7)),
                        ChatTimeDivider(today.AddDays(-1).AddHours(16).AddMinutes(20)),
                        ChatTimeDivider(today.AddDays(-4).AddHours(14).AddMinutes(30)),
                        ChatTimeDivider(today.AddYears(-1).AddHours(10).AddMinutes(5)),
                        ChatTimeDivider(today, "Conversation resumed")
                    ).MaxWidth(720.px())).SetTitle("Labels")))
               .SeeAlso(typeof(ChatSample), typeof(ConversationListSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
