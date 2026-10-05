using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.AI, Order = 70, Icon = UIcons.BullseyeArrow, Description = "Actions to take on one object, beside its identity")]
    public class ActionCardSample : IComponent, ISample
    {
        private readonly IComponent _content;
        private readonly TextBlock  _lastAction;

        public ActionCardSample()
        {
            _lastAction = TextBlock("Nothing chosen yet.").Small().Foreground(Theme.Secondary.Foreground);

            _content = SectionStack().Secondary()
                .SampleTitle(typeof(ActionCardSample), UIcons.BullseyeArrow, "Actions to take on one object, drawn beside its identity")
                .FlatSection(VStack().WS().Children(Overview()))
                .FlatSection(VStack().WS().Children(Facts()))
                .FlatSection(VStack().WS().Children(Narrow()))
                .FlatSection(VStack().WS().Children(States()))
                .FlatSection(VStack().WS().Children(Compact()))
                .FlatSection(VStack().WS().Children(Group()))
                .FlatSection(VStack().WS().Children(InAChat()))
                .SeeAlso(typeof(ContextCardSample), typeof(ToolCallSample), typeof(ChatSample));
        }

        private static Card FeatureCard(string title, string subTitle, string description, params IComponent[] content)
        {
            var stack = VStack().WS().Children(SampleSubTitle(subTitle), TextBlock(description).MB(8));

            foreach (var c in content)
            {
                stack.Add(c);
            }

            return Card(stack).SetTitle(title);
        }

        private void Report(ActionCard<string> card, ActionCard<string>.Item q)
        {
            _lastAction.Text = $"Chose \"{q.Text}\" on {card.Label} (data: {q.Data ?? "none"})";
        }

        private ActionCard<string> Acme()
        {
            return ActionCard<string>("Acme Logistics GmbH", UIcons.Building)
                .SetSubLabel("Company · ACC-20931").MonospaceSubLabel()
                .SetDetail("Hamburg · Key account")
                .SetTitle("Ask about this company")
                .AddAction("Which open support tickets mention Acme Logistics?", UIcons.Inbox, data: "tickets:open acme")
                .AddAction("How has their order volume changed this quarter?",  UIcons.ChartLineUp)
                .AddAction("Who are our main contacts there?",                  UIcons.Users)
                .AddAction("Summarize the latest framework contract with Acme", UIcons.FileSignature)
                .OnAction(Report);
        }

        // ---------- Overview ----------

        private IComponent Overview()
        {
            return FeatureCard("Overview", "The object on the left, what to do with it on the right",
                "ActionCard offers actions about one object. The left column names the object - an icon tile, a label, a second line and an optional detail line, the same parts as a ContextCard. The right column lists the actions as ToolCall-style rows, each with an icon saying what kind of action it is. Clicking one calls OnAction with the item, whose Data is typed by the card (here a string) so the handler needs no cast.",
                Acme(),
                _lastAction);
        }

        // ---------- Facts ----------

        private IComponent Facts()
        {
            return FeatureCard("Facts", "A few numbers beside the actions",
                "AddFact puts key/value pairs in the identity column, for an object whose actions make more sense with some of its numbers in view. A monospace value suits an amount, a date or an id, and a component value (a Tag, a Link) is accepted too.",
                ActionCard<string>("Framework Agreement 2026", UIcons.FileSignature)
                    .IconBackground("#f59e0b")
                    .SetSubLabel("Contract")
                    .AddFact("Party",  "Acme Logistics")
                    .AddFact("Value",  "€ 1.2M",     monospace: true)
                    .AddFact("Renews", "2027-03-31", monospace: true)
                    .AddFact("Pages",  "42",         monospace: true)
                    .AddAction("What are the termination clauses?",            UIcons.Search)
                    .AddAction("How does pricing change after the first year?", UIcons.ChartLineUp)
                    .AddAction("Which obligations fall on us, and by when?",   UIcons.ListCheck)
                    .AddAction("Compare it with the 2024 agreement",           UIcons.Exchange)
                    .OnAction(Report));
        }

        // ---------- Narrow ----------

        private IComponent Narrow()
        {
            return FeatureCard("Narrow", "Stacks itself below about 520px",
                "The card measures its own width with a container query, so in a phone-width chat or a side panel it stacks without being told: the identity becomes a header strip and the actions wrap onto several lines. Stacked() forces that layout at any width.",
                HStack().Wrap().Gap(16.px()).AlignItems(ItemAlign.Start).Children(
                    VStack().W(360.px()).Children(Acme()),
                    VStack().W(360.px()).Children(
                        ActionCard<string>("shipments", UIcons.Database).IconTint("#16a34a")
                            .SetSubLabel("Dataset · 42,109 rows").MonospaceSubLabel()
                            .AddAction("Which routes had the most delays last month?", UIcons.ChartHistogram)
                            .AddAction("What share of shipments arrived late?",      UIcons.ChartPie)
                            .Stacked()
                            .OnAction(Report))));
        }

        // ---------- States ----------

        private IComponent States()
        {
            var loading = ActionCard<string>("Acme Logistics GmbH", UIcons.Building)
                .SetSubLabel("Company").Loading();

            var more = Acme().MaxVisible(2);

            ActionCard<string> failing = null;

            failing = ActionCard<string>("Acme Logistics GmbH", UIcons.Building)
                .SetSubLabel("Company")
                .SetError("Couldn't suggest actions for this company.", () =>
                {
                    failing.ClearError().Loading();
                    window.setTimeout(_ => failing.Loading(false)
                                                 .AddAction("Which open support tickets mention Acme Logistics?", UIcons.Inbox)
                                                 .AddAction("Who are our main contacts there?", UIcons.Users), 1200);
                });

            return FeatureCard("States", "Loading, more, and failed",
                "Loading() shows placeholder rows while the actions are generated, with the identity already in place. MaxVisible keeps a long list short behind a \"Show N more\" button. SetError replaces the list with a message and, given a handler, a Retry button.",
                VStack().WS().Gap(12.px()).Children(
                    loading,
                    HStack().Gap(8.px()).Children(
                        Button("Toggle loading").OnClick(() => loading.Loading(!loading.IsLoading))),
                    more,
                    failing));
        }

        // ---------- Compact ----------

        private IComponent Compact()
        {
            return FeatureCard("Compact", "One wrapping line",
                "Compact() draws the object as a chip followed by the actions as pills, for a transcript where a full card under every answer would be too much.",
                ActionCard<string>("Acme Logistics", UIcons.Building)
                    .AddActions("Open tickets?", "Order trend this quarter", "Main contacts", "Contract summary")
                    .Compact()
                    .OnAction(Report));
        }

        // ---------- Group ----------

        private IComponent Group()
        {
            return FeatureCard("Group", "Several objects in one card",
                "ActionCardGroup draws several cards as one, a row per object, for an answer that mentions more than one thing. Each card keeps its own actions and states; OnAction on the group reaches all of them.",
                ActionCardGroup(
                    ActionCard<string>("Acme Logistics", UIcons.Building).IconTint("#248efa").SetSubLabel("Company").MonospaceSubLabel()
                        .AddAction("How has their order volume changed this quarter?", UIcons.ChartLineUp)
                        .AddAction("Who are our main contacts there?", UIcons.Users),
                    ActionCard<string>("Framework Agreement", UIcons.FileSignature).IconTint("#d97706").SetSubLabel("Contract · 42 pp").MonospaceSubLabel()
                        .AddAction("What are the termination clauses?", UIcons.Search)
                        .AddAction("Compare it with the 2024 agreement", UIcons.Exchange),
                    ActionCard<string>("shipments", UIcons.Database).IconTint("#16a34a").SetSubLabel("Dataset · 42,109 rows").MonospaceSubLabel()
                        .AddAction("Which routes had the most delays last month?", UIcons.ChartHistogram)
                ).OnAction(Report));
        }

        // ---------- In a chat ----------

        private IComponent InAChat()
        {
            var transcript = VStack().WS().Gap(12.px());

            transcript.Children(
                TextBlock("Acme Logistics GmbH had the most escalations: 14 tickets in September, up from 5 in August. Most concern delayed customs paperwork on the Hamburg–Rotterdam route.").Wrap(),
                ActionCard<string>("Acme Logistics GmbH", UIcons.Building)
                    .SetSubLabel("Company · ACC-20931").MonospaceSubLabel()
                    .SetTitle("Follow up on Acme")
                    .AddAction("Which of the 14 tickets are still open?",          UIcons.Inbox)
                    .AddAction("How has their order volume changed this quarter?", UIcons.ChartLineUp)
                    .AddAction("Who owns the account on our side?",                UIcons.Users)
                    .OnAction(text => transcript.Add(
                        TextBlock(text).Wrap().Padding(8.px()).Background(Theme.Secondary.Background).AlignEnd())));

            return FeatureCard("In a chat", "Under the answer that mentioned the object, as a chat would",
                "The usual home: below the answer, after any tool calls. Here OnAction appends the chosen action to the transcript, as a chat would send it.",
                ToolCall(UIcons.Search, "Search tickets status:escalated month:2026-09"),
                transcript);
        }

        public HTMLElement Render() => _content.Render();
    }
}
