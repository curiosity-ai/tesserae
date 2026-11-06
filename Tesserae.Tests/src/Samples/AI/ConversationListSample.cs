using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.AI, Order = 25, Icon = UIcons.CommentClock, Description = "A chat history with pins, search and filters")]
    public class ConversationListSample : IComponent, ISample
    {
        private const string WORKING     = "working";
        private const string NEEDS_INPUT = "needs-input";
        private const string COMPLETED   = "completed";
        private const string ARCHIVED    = "archived";

        private readonly IComponent _content;
        private          int        _created;

        public ConversationListSample()
        {
            var random = new SampleRandom(4711);

            //The sidebar version and the standalone version are two lists over the same data, each its own instance.
            var sidebarList    = NewList(random);
            var standaloneList = NewList(new SampleRandom(4711));

            var sidebar = Sidebar();
            sidebar.AddHeader(new SidebarButton("new-chat", UIcons.Edit, "New chat").OnClick(() => AddConversation(sidebarList)));
            sidebar.AddContent(new SidebarComponent("history", sidebarList, sidebarList.RenderClosed()).Grow().NotSortable());

            var controls = HStack().WS().Wrap().Children(
                Button("Toggle sidebar").SetIcon(UIcons.Sidebar).OnClick(() => sidebar.Toggle()),
                Button("New conversation").SetIcon(UIcons.Plus).OnClick(() => AddConversation(sidebarList)),
                Button("Reload (2s)").SetIcon(UIcons.Refresh).OnClick(() => Reload(sidebarList)));

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ConversationListSample), UIcons.CommentClock, "A chat history with pins, search and filters")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("A ConversationList is the history of a chat surface. Pinned conversations lead in their own collapsible section; every row has a dot saying what the conversation is doing, its name, an optional second line and the time it last changed. The search icon opens a search box over the list, and the filter icon narrows it by state and by time. The states are the host's own (AddState): each brings a label and icon for the filter menu and a tone for the dot. A state added with includedInAll: false - an archive - is left out of \"All\" and shown only when picked."),
                        TextBlock("It is also an ISidebarItem. Collapse the sidebar below: the list leaves a single glyph on the rail, and hovering it opens the same list - same filter, same search, same scroll - in a flyout beside the rail.").PaddingTop(8.px())
                    )).SetTitle("Overview"),
                    Card(VStack().WS().Children(
                        controls.PaddingBottom(8.px()),
                        HStack().WS().H(560).Children(
                            sidebar.HS(),
                            VStack().Grow().HS().Padding(16.px()).Children(Message("Open a conversation in the sidebar"))
                        )
                    )).SetTitle("In a sidebar"),
                    Card(VStack().WS().Children(
                        TextBlock("The same component outside a sidebar, sized by its container.").PaddingBottom(8.px()),
                        standaloneList.WS().MaxWidth(340.px()).H(480)
                    )).SetTitle("On its own")))
               .SeeAlso(typeof(SidebarSample), typeof(ChatSample));
        }

        private ConversationList NewList(SampleRandom random)
        {
            var list = ConversationList("Conversations")
               .AddState(WORKING,     "Working",     UIcons.Spinner,     ConversationStateTone.Running)
               .AddState(NEEDS_INPUT, "Needs input", UIcons.TriangleWarning, ConversationStateTone.Attention)
               .AddState(COMPLETED,   "Completed",   UIcons.Check,       ConversationStateTone.Success)
               .AddState(ARCHIVED,    "Archived",    UIcons.Box,         ConversationStateTone.Muted, includedInAll: false)
               .ClosedLabel("Conversations")
               .EmptyMessage("Conversations you start will show up here");

            list.OnSelect(item => Toast().Information("Opened \"" + item.Title + "\""));
            list.ItemMenu(item => MenuFor(list, item));
            list.SetItems(Generate(random));

            return list;
        }

        private static ContextMenu.Item[] MenuFor(ConversationList list, ConversationListItem item)
        {
            var archived  = item.State == ARCHIVED;
            var completed = item.State == COMPLETED;

            return new[]
            {
                ContextMenuItem(item.IsPinned ? "Unpin" : "Pin").OnClick(() => list.UpdateItem(With(item, i => i.IsPinned = !i.IsPinned))),
                ContextMenuItem(completed ? "Reopen" : "Mark as completed").OnClick(() => list.UpdateItem(With(item, i => i.State = completed ? null : COMPLETED))),
                ContextMenuItem(archived ? "Unarchive" : "Archive").OnClick(() => list.UpdateItem(With(item, i => { i.State = archived ? null : ARCHIVED; i.IsPinned = false; }))),
                ContextMenuItem().Divider(),
                ContextMenuItem("Delete").OnClick(() => list.RemoveItem(item.Id)),
            };
        }

        private static ConversationListItem With(ConversationListItem item, Action<ConversationListItem> change)
        {
            var copy = new ConversationListItem
            {
                Id       = item.Id,
                Title    = item.Title,
                Subtitle = item.Subtitle,
                Time     = item.Time,
                IsPinned = item.IsPinned,
                State    = item.State,
                Icon     = item.Icon,
            };

            change(copy);
            return copy;
        }

        private void AddConversation(ConversationList list)
        {
            _created++;

            var item = new ConversationListItem
            {
                Id       = "new-" + _created,
                Title    = "New conversation " + _created,
                Subtitle = "curiosity-ai/tesserae",
                Time     = DateTimeOffset.Now,
                State    = WORKING,
            };

            list.UpdateItem(item).Select(item.Id);

            //The reply arrives a few seconds later, and the dot settles.
            window.setTimeout(_ => list.UpdateItem(With(item, i => i.State = null)), 4000);
        }

        private static void Reload(ConversationList list)
        {
            var items = list.Items.ToArray();

            list.Clear().Loading();

            window.setTimeout(_ => list.Loading(false).SetItems(items), 2000);
        }

        private static IEnumerable<ConversationListItem> Generate(SampleRandom random)
        {
            var titles = new[]
            {
                "RocksDB C# port for .NET 10/11",  "Chat history list component",   "Locked Facets feature bug",
                "Agentic website implementation",  "File content indexing fields",  "Tables OCR newline handling",
                "AppSettings share option flag",   "Mobile rendering issues review","Repo handover continuation",
                "UID128 front-end methods",        "Access control caching classes","Flint charts horizontal scaling",
                "Guidelines cleanup and review",   "Quarterly planning notes",      "Embedding model benchmark",
                "Release notes for 26.11",         "Customer onboarding checklist", "Search relevance tuning",
                "Dark mode contrast audit",        "Replica failover drill",        "Translation batch for German",
                "Graph schema migration",          "Invoice parser edge cases",     "Weekly changelog draft",
            };

            var repos  = new[] { "curiosity-ai/catalyst", "curiosity-ai/tesserae", "curiosity-ai/rocksdbsharp" };
            var states = new string[] { null, null, null, null, null, null, COMPLETED, COMPLETED, ARCHIVED, ARCHIVED, NEEDS_INPUT };

            //Minutes ago, growing down the list: a couple of fresh ones, then hours, days, weeks.
            var ages = new[] { 0, 1, 9 * 60, 26 * 60, 2 * 1440, 2 * 1440 + 90, 2 * 1440 + 300, 3 * 1440, 3 * 1440 + 60, 3 * 1440 + 200, 4 * 1440, 5 * 1440, 6 * 1440, 9 * 1440, 12 * 1440, 16 * 1440, 22 * 1440, 31 * 1440, 40 * 1440, 55 * 1440, 70 * 1440, 120 * 1440, 200 * 1440, 400 * 1440 };

            var now = DateTimeOffset.Now;

            for (int i = 0; i < titles.Length; i++)
            {
                var more = random.Next(0, 5);

                yield return new ConversationListItem
                {
                    Id       = "c" + i,
                    Title    = titles[i],
                    Subtitle = repos[random.Next(0, repos.Length)] + (more > 0 ? ", +" + more + " more" : ""),
                    Time     = now.AddMinutes(-ages[i]),
                    IsPinned = i == 2 || i == 7 || i == 13,
                    State    = i == 0 ? WORKING : i == 1 ? NEEDS_INPUT : states[random.Next(0, states.Length)],
                };
            }
        }

        public HTMLElement Render() => _content.Render();
    }
}
