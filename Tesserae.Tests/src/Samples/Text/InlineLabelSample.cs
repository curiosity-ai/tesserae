using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Text, Order = 30, Icon = UIcons.Bookmark)]
    public class InlineLabelSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public InlineLabelSample()
        {
            _content = SectionStack().Secondary()
                .SampleTitle(typeof(InlineLabelSample), UIcons.Tags, "One small fact - a mark, some text - drawn as a compact button on its own and as plain type in a footer")
                .FlatSection(VStack().WS().Children(Overview()))
                .FlatSection(VStack().WS().Children(Interactive()))
                .FlatSection(VStack().WS().Children(InAGrid()))
                .FlatSection(VStack().WS().Children(InAFooter()))
                .FlatSection(VStack().WS().Children(LookedUp()))
                .SeeAlso(typeof(OmniResultSample), typeof(DetailsGridSample), typeof(BadgeSample), typeof(ButtonSample));
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

        private IComponent Overview()
        {
            return FeatureCard("Overview", "A mark, then text - every combination",
                "An InlineLabel is an optional mark - a glyph, an image, or a rounded square of colour - followed by optional text. Text alone, a mark alone, or both: whatever the mark is it takes the same box, so a line of labels built out of different kinds still sits on one baseline. On its own it draws as a compact button, which is the shape a chip of related things wants; in an OmniResult footer the chrome comes off and it reads as plain type.",
                HStack().WS().Wrap().Gap(8.px()).AlignItemsCenter().PT(8).PB(8).Children(
                    InlineLabel("Plain text"),
                    InlineLabel("Icon and text").SetIcon(UIcons.User),
                    InlineLabel("Colour and text").SetColor("#0061d5"),
                    InlineLabel("Image and text").SetImage("./assets/img/box-img.svg"),
                    InlineLabel().SetIcon(UIcons.Lock),
                    InlineLabel().SetImage("./assets/img/curiosity-logo.svg"),
                    InlineLabel("A component as the mark").SetIcon(Avatar(initials: "PN").Size(AvatarSize.XSmall))),
                TextBlock("The text ellipsizes rather than wrapping, so a long one gives way to whatever it shares the line with:").Small().MT(8).MB(8),
                InlineLabel("All Files / sample-files / procedures / BRK-SEN-447 calibration procedure.pdf").SetIcon(UIcons.Folder).MaxWidth(280.px()));
        }

        private IComponent Interactive()
        {
            return FeatureCard("Pressable, and real links", "OnClick and SetHref",
                "OnClick makes a label pressable: it takes a tab stop of its own, answers Enter and Space, and the click stops at the label so pressing it never also counts as a click on the row it sits in. SetHref makes it a real link - the label is an anchor either way, so a link is middle-clickable and shows its address in the status bar instead of being a div pretending to be one.",
                HStack().WS().Wrap().Gap(8.px()).AlignItemsCenter().PT(8).PB(8).Children(
                    InlineLabel("Pressable").SetIcon(UIcons.Folder).OnClick(l => Toast().Information($"Pressed \"{l.Text}\"")),
                    InlineLabel("A real link").SetIcon(UIcons.Globe).SetHref("https://github.com/curiosity-ai/tesserae", openInNewTab: true),
                    InlineLabel("Not pressable").SetIcon(UIcons.Lock)),
                TextBlock("Hovering either kind lifts the background; only a real link also underlines, since a label that just runs a handler is a button and has no address to promise.").Small().MT(8).MB(8),
                TextBlock("A label says as much as fits, so the rest belongs in a tooltip - the full path, what a code stands for, when \"2 days ago\" actually was:").Small().MT(16).MB(8),
                HStack().WS().Wrap().Gap(8.px()).AlignItemsCenter().PT(8).PB(8).Children(
                    InlineLabel("sample-files / procedures").SetIcon(UIcons.Folder).Tooltip("All Files / sample-files / procedures / BRK-SEN-447"),
                    InlineLabel("2 days ago").SetIcon(UIcons.Clock).Tooltip("Apr 12, 2024 at 14:03"),
                    InlineLabel("BRK-447").SetColor("#6366f1").Tooltip("Brake sensor family 447"),
                    InlineLabel().SetIcon(UIcons.Lock).Tooltip("Confidential - do not share outside the company"),
                    InlineLabel("Marie Lang").SetIcon(UIcons.User).Tooltip("marie.lang@example.com").OnClick(_ => Toast().Information("Opening the owner"))));
        }

        private IComponent InAGrid()
        {
            return FeatureCard("In a DetailsGrid", "Related things in the value column",
                "A grid's value slot takes a component, which is where a line of labels belongs: the folders a file sits in, the people on a ticket, the sources a note was built from. Each one carries its own mark and its own action, and they stay the same size whether they were built from a glyph, a logo or a square of colour.",
                DetailsGrid()
                    .Row("Owner",  InlineLabel("Pius Neuhaus").SetIcon(Avatar(initials: "PN").Size(AvatarSize.XSmall)).OnClick(_ => Toast().Information("Opening the owner")))
                    .Row("Folder", InlineLabel("sample-files / procedures").SetIcon(UIcons.Folder).OnClick(_ => Toast().Information("Opening the folder")))
                    .Row("Source", InlineLabel("Box").SetImage("./assets/img/box-img.svg").SetHref("https://box.com", openInNewTab: true))
                    .Row("Labels", HStack().Wrap().Gap(6.px()).Children(
                        InlineLabel("brakes").SetColor("#ef4444"),
                        InlineLabel("calibration").SetColor("#16a34a"),
                        InlineLabel("line 3").SetColor("#6366f1")))
                    .Row("Size",   "2.4 MB")
                    .MaxWidth(520.px()));
        }

        private IComponent InAFooter()
        {
            return FeatureCard("In an OmniResult footer", "The same labels, drawn small",
                "A footer is a line of facts rather than a row of chips, so a label inside one drops the pill's border and background and takes the footer's own colour and type size - no flag to pass, the stylesheet does it by where the label is. A pressable one still says so: it underlines on hover instead of filling.",
                OmniResult("footer-sample", "BRK-SEN-447 calibration procedure.pdf")
                    .SetIcon("PDF", "#ef4444")
                    .SetSource("#0061d5", "Box")
                    .SetText("Torque the mount to 12 Nm before starting brake sensor work. Full calibration steps sit on page 14.")
                    .SetFooterEntries(
                        InlineLabel("sample-files / pdfs").SetIcon(UIcons.Folder).OnClick(_ => Toast().Information("Opening the folder")),
                        InlineLabel("2.4 MB"),
                        InlineLabel("Pius Neuhaus").SetIcon(UIcons.User),
                        InlineLabel("Apr 12, 2024"),
                        InlineLabel("Confidential").SetColor("#ef4444")));
        }

        private IComponent LookedUp()
        {
            // The whole area is deferred so the Reload button can rebuild it: Refresh() re-runs the
            // generator, which builds a fresh set of labels that look themselves up all over again.
            var lookups = DeferSync(BuildLookups);

            var reload = Button("Reload").SetIcon(UIcons.Refresh).OnClick(() => lookups.Refresh());

            return FeatureCard("Facts it has to look up", "InlineLabel(async label => ...)",
                "Built from a task, a label draws as a skeleton rectangle while the task runs. If the task ends without giving it anything to say, the label takes itself out of the document - and the slot it was standing in with it, so the line it belonged to closes up rather than keeping a gap for something that turned out not to exist.",
                reload.MT(8).MB(8),
                lookups);
        }

        private static IComponent BuildLookups()
        {
            return VStack().WS().Children(
                TextBlock("Two of the five below resolve to nothing. Watch the row close up:").Small().MT(8).MB(8),
                HStack().WS().Wrap().Gap(8.px()).AlignItemsCenter().PT(8).PB(8).Children(
                    InlineLabel(async label => { await LookUp(600); label.SetText("Marie Lang").SetIcon(UIcons.User); }),
                    InlineLabel(async label => { await LookUp(1400); /* nothing to say */ }),
                    InlineLabel(async label => { await LookUp(900); label.SetText("sample-files / procedures").SetIcon(UIcons.Folder); }),
                    InlineLabel(async label => { await LookUp(2000); /* nothing to say either */ }),
                    InlineLabel(async label => { await LookUp(1100); label.SetText("Box").SetImage("./assets/img/box-img.svg"); })),
                TextBlock("In a footer the entry goes with it, dot and all:").Small().MT(16).MB(8),
                OmniResult("looked-up", "Supplier notice — Bismuth BRK-447.pdf")
                    .SetIcon("PDF", "#ef4444")
                    .SetSource("#0061d5", "Box")
                    .SetFooterEntries(
                        InlineLabel(async label => { await LookUp(700); label.SetText("sample-files / suppliers").SetIcon(UIcons.Folder); }),
                        InlineLabel("320 KB"),
                        InlineLabel(async label => { await LookUp(1800); /* the file has no author recorded */ }),
                        InlineLabel("Mar 19, 2024")),
                TextBlock("In a details grid the whole row goes - a label with nothing in it would leave a labelled blank:").Small().MT(16).MB(8),
                DetailsGrid()
                    .Row("Owner",     InlineLabel(async label => { await LookUp(800);  label.SetText("Anja Vogt").SetIcon(UIcons.User); }))
                    .Row("Retention", InlineLabel(async label => { await LookUp(1600); /* no policy on this one */ }))
                    .Row("Folder",    InlineLabel(async label => { await LookUp(1200); label.SetText("sample-files / suppliers").SetIcon(UIcons.Folder); }))
                    .Row("Size",      "320 KB")
                    .MaxWidth(520.px()));
        }

        // Stands in for whatever the host would really be waiting on.
        private static Task LookUp(int milliseconds)
        {
            var waited = new TaskCompletionSource<bool>();

            window.setTimeout(_ => waited.SetResult(true), milliseconds);

            return waited.Task;
        }

        public HTMLElement Render() => _content.Render();
    }
}
