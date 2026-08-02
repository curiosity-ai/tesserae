using System;
using Tesserae;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 101, Icon = UIcons.AngleRight)]
    public class SidebarShiftSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SidebarShiftSample()
        {
            var sidebar    = Sidebar();
            var chatBar    = Sidebar();
            var homePane   = HomePane();
            var chatPane   = ChatPane();

            BuildMainSidebar(sidebar, chatBar);
            BuildChatSidebar(sidebar, chatBar);

            // The child sidebar always belongs to a new interface, so the content area follows it
            chatPane.Collapse();

            sidebar.OnShiftChanged(isShifted =>
            {
                if (isShifted)
                {
                    homePane.Collapse();
                    chatPane.Show();
                }
                else
                {
                    chatPane.Collapse();
                    homePane.Show();
                }
            });

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SidebarShiftSample), UIcons.AngleRight, "Sliding a sidebar into a nested sidebar")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Use .ShiftTo(childSidebar) when navigating into an interface that has its own navigation: the child sidebar slides in horizontally from the right and takes over the sidebar. .ShiftBack() slides back to the main sidebar. The panel that ends up out of view is set to display:none once the animation finishes, so it can't be tabbed into. Only one depth level is supported."),
                        TextBlock("Click \"AI assistant\" below to shift into the chat sidebar, and the back arrow in its header to come back."))).SetTitle("Overview"),
                    Card(VStack().WS().Children(
                        SplitView().WS().H(800).LeftIsSmaller(280.px())
                                   .Left(sidebar.S())
                                   .Right(Stack().WS().HS().Children(homePane, chatPane))
                    )).SetTitle("Usage")))
               .SeeAlso(typeof(SidebarSample), typeof(SidebarSeparatorSample), typeof(SidenavSample), typeof(NavbarSample));
        }

        private static void BuildMainSidebar(Sidebar sidebar, Sidebar chatBar)
        {
            sidebar.AddHeader(new SidebarButton("brand", new ImageIcon("/assets/img/curiosity-logo.svg"), "Curiosity"));

            sidebar.AddHeader(new SidebarSearchBox("search-everything", "Search everything")
               .Rounded()
               .SetKeyboardShortcut("Ctrl", "K")
               .OnSearch(term => sidebar.Search(term)));

            sidebar.AddContent(new SidebarButton("search", UIcons.Search, "Search"));

            // The chevron hints that this entry opens an interface with its own sidebar
            var assistant = new SidebarButton("assistant", UIcons.Sparkles, "AI assistant", new SidebarCommand(UIcons.AngleRight))
               .CommandsAlwaysVisible()
               .OnClick(() => sidebar.ShiftTo(chatBar));

            sidebar.AddContent(assistant);

            sidebar.AddContent(new SidebarButton("favorites", UIcons.Bookmark,    "Favorites"));
            sidebar.AddContent(new SidebarButton("mail",      UIcons.Envelope,    "Mail"));
            sidebar.AddContent(new SidebarButton("contacts",  UIcons.User,        "Contacts"));
            sidebar.AddContent(new SidebarButton("notes",     UIcons.NotebookAlt, "Notes"));
            sidebar.AddContent(new SidebarButton("spaces",    UIcons.Table,       "Spaces"));

            sidebar.AddContent(new SidebarSeparator("apps", "Apps"));

            sidebar.AddContent(new SidebarButton("outlook", UIcons.Envelope, "Outlook", Count("4.2k")));
            sidebar.AddContent(new SidebarButton("teams",   UIcons.Users,    "Teams",   Count("1.8k")));
            sidebar.AddContent(new SidebarButton("box",     UIcons.Cube,     "Box",     Count("9.4k")));

            sidebar.AddFooter(CurrentUser("main"));

            // Collapsing is shared with the shifted sidebar, so both rails stay in sync
            sidebar.AddFooter(new SidebarButton("collapse", UIcons.AngleLeft, "Collapse").OnClick(() => sidebar.Toggle()));
        }

        private static void BuildChatSidebar(Sidebar sidebar, Sidebar chatBar)
        {
            chatBar.AddHeader(new SidebarButton("back", UIcons.AngleLeft, "AI assistant")
               .OnClick(() => sidebar.ShiftBack()));

            chatBar.AddHeader(new SidebarButton("new-chat", UIcons.Plus, "New chat")
               .Primary()
               .Rounded()
               .OnClick(() => Toast().Success("New chat")));

            chatBar.AddHeader(new SidebarSearchBox("search-chats", "Search chats")
               .Rounded()
               .OnSearch(term => chatBar.Search(term)));

            chatBar.AddContent(new SidebarSeparator("today", "Today"));
            chatBar.AddContent(new SidebarButton("chat-1", UIcons.Comment, "Brake sensor calibration"));
            chatBar.AddContent(new SidebarButton("chat-2", UIcons.Comment, "Q2 supplier risk summary"));

            chatBar.AddContent(new SidebarSeparator("yesterday", "Yesterday"));
            chatBar.AddContent(new SidebarButton("chat-3", UIcons.Comment, "Line 3 handover recap"));
            chatBar.AddContent(new SidebarButton("chat-4", UIcons.Comment, "Draft: Bismuth response"));

            chatBar.AddContent(new SidebarSeparator("last-7-days", "Last 7 days"));
            chatBar.AddContent(new SidebarButton("chat-5", UIcons.Comment, "Ingolstadt drift report"));
            chatBar.AddContent(new SidebarButton("chat-6", UIcons.Comment, "Harness rev C rollout"));

            chatBar.AddFooter(CurrentUser("chat"));
        }

        private static SidebarBadge Count(string count) => new SidebarBadge(count).Foreground(Theme.Secondary.Foreground).Background(Theme.Secondary.Background);

        // Both sidebars carry the same account row at the bottom, but each needs its own instance
        // because a sidebar item can only be rendered in one place at a time.
        private static SidebarButton CurrentUser(string suffix) => new SidebarButton($"user-{suffix}", UIcons.User, "Pius Neuhaus", new SidebarCommand(UIcons.MenuDots));

        private static IComponent HomePane() => Stack().WS().HS().AlignItems(ItemAlign.Center).JustifyContent(ItemJustify.Center).Children(
            VStack().Children(
                TextBlock("Hi Pius — what are you working on?").XLarge().SemiBold(),
                TextBlock("Pick AI assistant on the left to shift the sidebar into the chat interface.").Medium().Secondary().PT(8)));

        private static IComponent ChatPane() => Stack().WS().HS().AlignItems(ItemAlign.Center).JustifyContent(ItemJustify.Center).Children(
            VStack().Children(
                TextBlock("AI assistant").XLarge().SemiBold(),
                TextBlock("The sidebar now shows the chat history. Use the back arrow to shift back.").Medium().Secondary().PT(8)));

        public HTMLElement Render() => _content.Render();
    }
}
