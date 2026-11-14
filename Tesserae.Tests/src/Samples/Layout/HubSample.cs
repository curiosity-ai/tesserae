using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Layout, Order = 55, Icon = UIcons.BrowserUi, Description = "A top-level page with a breadcrumb header")]
    public class HubSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public HubSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(HubSample), UIcons.BrowserUi, "A top-level page: where you are, what you can do, then the content")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Hub is the frame of a top-level page. Its header is a breadcrumb trail that starts at the hub's own name, with the page's commands on the right; below it, the page's content is added as sections, exactly like a SectionStack (which Hub is)."),
                    TextBlock("The trail takes whatever room the commands leave and clips what does not fit: the commands are never squeezed. Every crumb after the first carries its own separator, so collapsing a crumb takes its separator with it."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Give every top-level page a Hub, named after where it lives in the navigation, and link that name back to the page's root. Add a crumb for each level below it, the last one being the page itself. Put page-wide actions in Commands, not in the content, and let the header space them rather than adding margins of your own. A section added with grow: true takes the height that is left, which is what a list or a split view wants."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("A hub with commands"),
                        Frame(Hub("Users", () => Toast().Information("Users clicked"))
                           .Commands(
                                SearchBox("Search users").W(220),
                                Button("Invite").SetIcon(UIcons.UserAdd).Primary(),
                                Button().SetIcon(UIcons.Refresh).Tooltip("Refresh"))
                           .Section(Placeholder("The list of users"), grow: true)),

                        SampleSubTitle("A trail of crumbs"),
                        Frame(Hub("Data", () => Toast().Information("Data clicked"))
                           .Crumb("Schemas", () => Toast().Information("Schemas clicked"))
                           .Crumb("Invoice")
                           .Commands(Button("Edit schema").SetIcon(UIcons.Pencil), Button().SetIcon(UIcons.Download).Tooltip("Download"))
                           .Section(Placeholder("The schema's fields"), grow: true)),

                        SampleSubTitle("A crumb that comes and goes"),
                        TextBlock("A crumb can be any component. This one is the open conversation: collapsing it hides its separator too, and a crumb can follow an observable."),
                        Conversations(),

                        SampleSubTitle("A narrow page"),
                        TextBlock("The root crumb keeps its name; the crumbs after it ellipsize first, and the commands keep their width."),
                        Frame(Hub("Integrations", () => Toast().Information("Integrations clicked"))
                           .Crumb("Connectors", () => Toast().Information("Connectors clicked"))
                           .Crumb("A data connector with a rather long name")
                           .Commands(Button("Run").SetIcon(UIcons.Play).Primary(), Button().SetIcon(UIcons.MenuDots))
                           .Section(Placeholder("The connector's runs"), grow: true), width: 560),

                        SampleSubTitle("A header on its own"),
                        TextBlock("HubHeader is the same row without the page around it, for a SectionStack you manage yourself or for several groups on one page."),
                        Frame(SectionStack().S()
                           .Title(HubHeader("Organizations", () => Toast().Information("Organizations clicked")).Commands(Button().SetIcon(UIcons.Search).Tooltip("Search all organizations")))
                           .Section(Placeholder("The most frequent organizations"))
                           .Title(HubHeader("People", () => Toast().Information("People clicked")).Commands(Button().SetIcon(UIcons.Search).Tooltip("Search all people")))
                           .Section(Placeholder("The most frequent people"), grow: true), height: 360)
                    )).SetTitle("Usage")))
               .SeeAlso(typeof(SectionStackSample), typeof(TextBreadcrumbsSample), typeof(BreadcrumbSample), typeof(CommandBarSample));
        }

        private static IComponent Conversations()
        {
            var open   = new SettableObservable<string>(null);
            var name   = TextBlock("").Class("tss-hub-crumb-label");
            var crumb  = HStack().AlignItemsCenter().Children(Icon(UIcons.Comment, size: TextSize.XSmall).PL(8).PR(4), name).Collapse();

            open.Observe(v =>
            {
                if (string.IsNullOrEmpty(v))
                {
                    crumb.Collapse();
                }
                else
                {
                    name.Text = v;
                    crumb.Show();
                }
            });

            var hubName = new SettableObservable<string>("Assistant");

            var hub = Hub(hubName)
               .Crumb(crumb)
               .Commands(
                    Button("Open a chat").SetIcon(UIcons.CommentAlt).OnClick(() => open.Value = "Quarterly revenue by region"),
                    Button("Close it").SetIcon(UIcons.Cross).OnClick(() => open.Value = null),
                    Button("Rename hub").OnClick(() => hubName.Value = hubName.Value == "Assistant" ? "Helper" : "Assistant"))
               .Section(Placeholder("The conversation"), grow: true);

            return Frame(hub);
        }

        private static IComponent Frame(IComponent hub, int width = 0, int height = 260)
        {
            var frame = VStack().H(height).Children(hub).Style(s =>
            {
                s.border       = "1px solid " + Theme.Default.Border;
                s.borderRadius = "4px";
                s.overflow     = "hidden";
            }).MB(24);

            if (width > 0) frame.W(width);
            else frame.WS();

            return frame;
        }

        private static IComponent Placeholder(string text)
        {
            return VStack().S().MinHeight(60.px()).AlignItemsCenter().JustifyContent(ItemJustify.Center)
               .Children(TextBlock(text).Secondary())
               .Style(s => s.border = "1px dashed " + Theme.Default.Border);
        }

        public HTMLElement Render() => _content.Render();
    }
}
