using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 26, Icon = UIcons.Megaphone)]
    public class BannerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public BannerSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(BannerSample), UIcons.Megaphone, "A notice strip: an icon, a title, a message, an action and a dismiss")
               .FlatSection(VStack().WS().Children(Overview()))
               .FlatSection(VStack().WS().Children(Tones()))
               .FlatSection(VStack().WS().Children(IconsAndBadges()))
               .FlatSection(VStack().WS().Children(ActionsAndDismiss()))
               .FlatSection(VStack().WS().Children(Shapes()))
               .FlatSection(VStack().WS().Children(AsToast()))
               .SeeAlso(typeof(ToastSample), typeof(MessageSample), typeof(NotificationCenterSample), typeof(SavingToastSample));
        }

        private static Card FeatureCard(string title, UIcons icon, string subTitle, string description, params IComponent[] content)
        {
            var stack = VStack().WS().Gap(12.px()).Children(SampleSubTitle(subTitle), TextBlock(description).MB(4));

            foreach (var c in content)
            {
                stack.Add(c);
            }

            return Card(stack).SetTitle(title, icon, Theme.Colors.Purple600);
        }

        private IComponent Overview()
        {
            return FeatureCard("Overview", UIcons.Megaphone, "One strip, inline or floated",
                "A Banner is a notice the user should read but doesn't have to answer: an IconTile saying what kind of notice it is, a title with an optional badge, a message under it, an action at the far end and a dismiss button after that. It is a plain IComponent, so it renders wherever you put it — and it is also exactly what a Toast floats over the page, so the same strip reads the same in both places.",
                Banner("3 items need your review", "They were flagged as high priority and are waiting in your queue — the oldest has been there for two days.")
                   .Danger()
                   .SetIcon(UIcons.Flame)
                   .SetBadge("Priority")
                   .Action("Review now", () => Toast().Success("Opening the queue"))
                   .OnDismiss(() => Toast().Information("Banner dismissed")));
        }

        private IComponent Tones()
        {
            return FeatureCard("Tones", UIcons.Palette, "Primary, Secondary, Success, Warning, Danger",
                "A banner takes the same tones a Button does, and every color it draws is derived from that one accent — the wash behind it, its border, the tile, the badge and the text. Each tone also brings a default icon, which any SetIcon call replaces.",
                Banner("Secondary", "The neutral tone: something the user should know, drawn in the page's own colors.").Secondary(),
                Banner("Primary", "The accent tone: something worth pointing at.").Primary(),
                Banner("Success", "Something went right — the import finished, the record was saved.").Success(),
                Banner("Warning", "Something needs care but hasn't failed yet.").Warning(),
                Banner("Danger", "Something failed, or will if it is left alone.").Danger());
        }

        private IComponent IconsAndBadges()
        {
            return FeatureCard("Icons and badges", UIcons.Tags, "A glyph, a few letters, or nothing at all",
                "SetIcon takes a UIcons glyph, a short string drawn in place of one, or a component of your own. Without a color the tile follows the banner's tone; pass one to say something the tone doesn't. NoIcon() drops the tile for a banner whose tone already says everything. SetBadge puts a pill beside the title — the reference the notice is about.",
                Banner("Design freeze in 10 days", "MSN 0142 onwards. Changes after 30 Oct 2025 need a concession.")
                   .Primary().SetIcon(UIcons.Snowflake).SetBadge("PAH3.5.2"),
                Banner("Quarterly export ready", "18 documents, 42 MB. The link expires in seven days.")
                   .Success().SetIcon("ZIP"),
                Banner("Two connectors are running behind", "Box and SharePoint last synced more than six hours ago.")
                   .Warning().SetIcon(UIcons.CloudExclamation, Theme.Colors.Orange600),
                Banner("Read-only mode", "You are looking at a snapshot from 12 Apr 2024.").Secondary().NoIcon());
        }

        private IComponent ActionsAndDismiss()
        {
            var log = TextBlock("Nothing pressed yet.").Small().Foreground(Theme.Secondary.Foreground);

            return FeatureCard("Actions and dismissing", UIcons.CursorFingerClick, "A button, or anything you like",
                "Action(text, handler) puts a button at the far end drawn in the banner's own tone; Action(component) takes whatever you build instead — a pair of buttons, a link, a dropdown. OnDismiss(handler) adds the [x] after it: pressing it runs your handler and takes the banner out of the page.",
                Banner("Your session expires in 5 minutes", "Save what you are working on, or extend the session now.")
                   .Warning()
                   .Action("Extend session", () => log.Text = "Extend session pressed.")
                   .OnDismiss(() => log.Text = "Session banner dismissed."),
                Banner("Three documents failed to index", "They are still searchable by name — the text of them isn't.")
                   .Danger()
                   .Action(HStack().Gap(8.px()).AlignItemsCenter().Children(
                        Button("Retry").Danger().OnClick(() => log.Text = "Retry pressed."),
                        Button("Details").OnClick(() => log.Text = "Details pressed.")))
                   .OnDismiss(() => log.Text = "Indexing banner dismissed."),
                log);
        }

        private IComponent Shapes()
        {
            return FeatureCard("Shapes", UIcons.Resize, "Compact and flat",
                "Compact() tightens the strip and shrinks the tile, for a banner inside something small. Flat() drops the rounding and the side rules, for one pinned edge to edge across a page — which is what a Toast in banner mode uses.",
                Banner("Draft saved", "Every change since 14:02 is in this draft.").Success().Compact(),
                Banner("Offline", "Changes are kept locally and sent when the connection comes back.").Warning().Compact().Action("Retry", () => Toast().Information("Retrying")),
                VStack().WS().Children(
                    Banner("Scheduled maintenance tonight, 23:00 – 01:00 UTC", "Search stays available; indexing is paused for the window.")
                       .Primary().Flat().OnDismiss(() => Toast().Information("Maintenance banner dismissed"))));
        }

        private IComponent AsToast()
        {
            return FeatureCard("Shown as a toast", UIcons.BreadSlice, "The same strip, floated over the page",
                "Toast().Show(banner) floats a banner instead of rendering it inline. The banner's dismiss button is hooked to the toast's own hiding — chained after whatever handler you set — so the [x] closes the toast. A toast asked not to dismiss (NoDismiss) gets no button at all, and an edge-to-edge banner follows its showHideButton setting.",
                HStack().WS().Wrap().Gap(8.px()).Children(
                    Button("Show as toast").OnClick(() => Toast().Show(
                        Banner("Export finished", "18 documents, 42 MB.").Success().SetIcon(UIcons.Download).Action("Download", () => Toast().Information("Downloading")))),
                    Button("Toast, no dismiss").OnClick(() => Toast().NoDismiss().Show(
                        Banner("Indexing", "This one has no [x]: the toast was told not to dismiss.").Primary())),
                    Button("As a page banner").OnClick(() => Toast().TopFull().Banner().Duration(System.TimeSpan.FromSeconds(15)).Show(
                        Banner("3 items need your review", "They were flagged as high priority and are waiting in your queue.")
                           .Danger().SetIcon(UIcons.Flame).SetBadge("Priority").Flat()
                           .Action("Review now", () => Toast().Success("Opening the queue"))))));
        }

        public HTMLElement Render() => _content.Render();
    }
}
