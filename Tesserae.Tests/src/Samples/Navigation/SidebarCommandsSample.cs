using Tesserae;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    /// <summary>
    /// The workspace rail an app builds on top of the sidebar: a logo, the workspace's name, and the
    /// rail's own controls as commands beside it. It is here because that row is where the two rules
    /// about commands meet - they are drawn over the row, and a row that draws them at all times has to
    /// keep its label out from under them - and a long workspace name is what proves both.
    /// </summary>
    [SampleDetails(Group = SampleGroup.Navigation, Order = 35, Icon = UIcons.SidebarFlip)]
    public class SidebarCommandsSample : IComponent, ISample
    {
        private const string LOGO       = "/assets/img/curiosity-logo.svg";
        private const string SHORT_NAME = "Curiosity";
        private const string NAME       = "Technical Support";
        private const string LONG_NAME  = "Technical Support Workspace EMEA";

        private readonly IComponent _content;

        public SidebarCommandsSample()
        {
            var name         = new SettableObservable<string>(NAME);
            var commandCount = new SettableObservable<int>(2);

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SidebarCommandsSample), UIcons.SidebarFlip, "Commands on a sidebar row, and the label beside them")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("A SidebarCommand is drawn over its row rather than in it, and only while the pointer is on that row - so the label runs the full width of the rail and a command covers its tail for as long as it is shown. .CommandsAlwaysVisible() makes the commands part of the rail's chrome instead: they are drawn at all times, the row reserves exactly the room its own commands take, and the label truncates before them."),
                        TextBlock("The row writes how wide its strip of commands is and the stylesheet reads it back, so the room is right for one command and for three. A skin that moves the strip further in from the edge sets --tss-sidebar-commands-inset on the row, and the label's reservation follows it."))).SetTitle("Overview"),

                    Card(VStack().WS().Children(
                        TextBlock("The same long name in three rails: with the commands on hover, with the same commands always drawn, and with a skin that insets them 12px from the edge. Hover the first one to see what the other two are avoiding.").Secondary(),
                        HStack().WS().PT(8).Children(
                            Rail("On hover",              Brand("hover", LONG_NAME, 2)),
                            Rail("Always visible",        Brand("always", LONG_NAME, 2).CommandsAlwaysVisible()),
                            Rail("Always visible, inset", Brand("inset", LONG_NAME, 2).CommandsAlwaysVisible().Class("tss-sample-commands-inset"))))).SetTitle("A label beside its commands"),

                    Card(VStack().WS().Children(
                        TextBlock("The rail a workspace app puts together: the brand carries the chat search and the way out, and the history sits under it. Change the name and the number of commands to watch the room the row keeps for them.").Secondary(),
                        HStack().WS().PT(8).Children(
                            ChoiceGroup("Workspace name").Choices(
                                Choice(SHORT_NAME).OnSelected(_ => name.Value = SHORT_NAME),
                                Choice(NAME).Selected().OnSelected(_ => name.Value = NAME),
                                Choice(LONG_NAME).OnSelected(_ => name.Value = LONG_NAME)),
                            ChoiceGroup("Commands").PL(32).Choices(
                                Choice("One").OnSelected(_ => commandCount.Value              = 1),
                                Choice("Two").Selected().OnSelected(_ => commandCount.Value   = 2),
                                Choice("Three").OnSelected(_ => commandCount.Value            = 3))),
                        DeferSync(name, commandCount, (n, c) => WorkspaceRail(n, c)).PT(8))).SetTitle("The workspace rail")))
               .SeeAlso(typeof(SidebarSample), typeof(SidebarShiftSample), typeof(SidebarSeparatorSample), typeof(NavbarSample));
        }

        /// <summary>One rail with nothing in it but the brand row, so the row is the only thing to read.</summary>
        private static IComponent Rail(string title, SidebarButton brand) => VStack().PR(16).Children(
            TextBlock(title).XSmall().Secondary().PB(4),
            Sidebar().AddHeader(brand).H(96.px()));

        private static SidebarButton Brand(string id, string workspaceName, int commandCount) =>
            new SidebarButton($"brand-{id}", new ImageIcon(LOGO), workspaceName, CommandsFor(commandCount));

        /// <summary>
        /// A fresh set every time: a command renders in one place, so two rails cannot share one.
        /// </summary>
        private static SidebarCommand[] CommandsFor(int count)
        {
            var search = new SidebarCommand(UIcons.Search).Tooltip("Search your chats").OnClick(() => Toast().Information("Search your chats"));
            var leave  = new SidebarCommand(UIcons.AngleLeft).Tooltip("Leave the assistant").OnClick(() => Toast().Information("Leave the assistant"));
            var more   = new SidebarCommand(UIcons.MenuDots).Tooltip("More").OnClick(() => Toast().Information("More"));

            if (count <= 1) return new[] { search };
            if (count == 2) return new[] { search, leave };
            return new[] { search, leave, more };
        }

        private static IComponent WorkspaceRail(string workspaceName, int commandCount)
        {
            var rail = Sidebar();

            rail.AddHeader(Brand("workspace", workspaceName, commandCount).CommandsAlwaysVisible());

            rail.AddHeader(new SidebarButton("new-chat", UIcons.Edit, "New chat")
               .Primary()
               .Rounded()
               .OnClick(() => Toast().Success("New chat")));

            rail.AddContent(new SidebarSeparator("today", "Today"));
            rail.AddContent(new SidebarButton("chat-1", UIcons.Comment, "Phone will not power on"));
            rail.AddContent(new SidebarButton("chat-2", UIcons.Comment, "Battery drains overnight"));

            rail.AddContent(new SidebarSeparator("yesterday", "Yesterday"));
            rail.AddContent(new SidebarButton("chat-3", UIcons.Comment, "Screen flickers after update"));

            return rail.H(320.px());
        }

        public HTMLElement Render() => _content.Render();
    }
}
