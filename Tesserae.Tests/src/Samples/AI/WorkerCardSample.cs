using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.AI, Order = 26, Icon = UIcons.CodeFork, Description = "The work an assistant handed to other agents")]
    public class WorkerCardSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public WorkerCardSample()
        {
            var card = WorkerCard("Started 3 workers").Hint("Each in its own worktree").SetItems(Workers());

            var single = WorkerCard("Started 1 worker").SetItems(Workers().Take(1));

            var withReview = WorkerCard("Started 3 workers").SetItems(Workers().Select((w, i) =>
            {
                if (i == 0) { w.Tone = ConversationStateTone.Attention; w.StateLabel = "In review"; w.Trailing = Button("Review").Primary().LessPadding().OnClick(() => Toast().Information("Opened the review")); }
                if (i == 1) { w.Tone = ConversationStateTone.Success;   w.StateLabel = "Done"; }
                return w;
            }));

            var controls = HStack().WS().Wrap().Children(
                Button("Finish a worker").SetIcon(UIcons.Check).OnClick(() => Finish(card)),
                Button("Fail a worker").SetIcon(UIcons.CrossCircle).OnClick(() => Fail(card)));

            card.OnSelect(item => Toast().Information("Opened \"" + item.Title + "\""));

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(WorkerCardSample), UIcons.CodeFork, "The work an assistant handed to other agents")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("A WorkerCard is what an assistant leaves in the chat when it splits a job and hands the parts to other agents: a header saying how many were started, with an optional note at its end, and a row per worker - the agent's logo (Leading, or an Icon), the task, the name of the agent doing it on a quiet line under the title, and a live status. An optional Agent column can name who does it in a column of its own, and is left off here. A running worker has a spinner. UpdateItem changes one row without redrawing the others."),
                        TextBlock("Each row can end in a component of its own through Trailing - a Review button once the work is waiting for a person - and OnSelect makes the rows clickable.").PaddingTop(8.px())
                    )).SetTitle("Overview"),
                    Card(VStack().WS().Children(
                        controls.PaddingBottom(8.px()),
                        card.WS().MaxWidth(720.px())
                    )).SetTitle("Started workers"),
                    Card(VStack().WS().Children(
                        withReview.WS().MaxWidth(720.px())
                    )).SetTitle("Waiting for a review"),
                    Card(VStack().WS().Children(
                        single.WS().MaxWidth(720.px())
                    )).SetTitle("One worker")))
               .SeeAlso(typeof(ConversationListSample), typeof(ToolCallSample));
        }

        private static IEnumerable<WorkerCardItem> Workers()
        {
            yield return new WorkerCardItem { Id = "w1", Title = "Meter API calls per workspace", Subtitle = "Metering agent", Tone = ConversationStateTone.Running, StateLabel = "Working", Icon = UIcons.Sparkles, IconColor = "#d97757" };
            yield return new WorkerCardItem { Id = "w2", Title = "Chart usage on the billing page", Subtitle = "Charts agent", Tone = ConversationStateTone.Running, StateLabel = "Working", Icon = UIcons.Bolt,     IconColor = "#6b7cff" };
            yield return new WorkerCardItem { Id = "w3", Title = "Email a monthly invoice",         Subtitle = "Billing agent", Tone = ConversationStateTone.Running, StateLabel = "Working", Icon = UIcons.Box };
        }

        private static void Finish(WorkerCard card)
        {
            var next = card.Items.FirstOrDefault(i => i.Tone == ConversationStateTone.Running);
            if (next is null) return;

            card.UpdateItem(new WorkerCardItem { Id = next.Id, Title = next.Title, Subtitle = next.Subtitle, Icon = next.Icon, IconColor = next.IconColor, Tone = ConversationStateTone.Success, StateLabel = "Done" });
        }

        private static void Fail(WorkerCard card)
        {
            var next = card.Items.FirstOrDefault(i => i.Tone == ConversationStateTone.Running);
            if (next is null) return;

            card.UpdateItem(new WorkerCardItem { Id = next.Id, Title = next.Title, Subtitle = next.Subtitle, Icon = next.Icon, IconColor = next.IconColor, Tone = ConversationStateTone.Danger, StateLabel = "Failed" });
        }

        public HTMLElement Render() => _content.Render();
    }
}
