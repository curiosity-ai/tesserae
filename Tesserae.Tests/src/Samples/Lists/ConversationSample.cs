using System;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Lists, Order = 100, Icon = UIcons.Comments, Description = "An issue-style thread of comments and events")]
    public class ConversationSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ConversationSample()
        {
            _content = SectionStack().Secondary().WidthStretch()
               .SampleTitle(typeof(ConversationSample), UIcons.Comments, "A discussion thread, like an issue's conversation")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Conversation lays out a discussion the way an issue tracker does: comment cards joined by a vertical rail, with one-line events in between for what changed - a label added, someone assigned, the thread closed - and a reply box at the foot."),
                        TextBlock("A ConversationComment has a header with the author's Avatar, what they did and when, role badges and an overflow menu; its body is Markdown rendered by a MarkdownBlock, with optional Image attachments and emoji reactions under it. A ConversationEvent puts an icon on the rail and a sentence beside it, with Tags or other components inline. The ConversationComposer has Write / Preview tabs over a TextArea."),
                        TextBlock("Put a MetadataPanel next to it for the assignees, labels, milestone and participants column. Both have a Compact() mode for dense layouts - use the toggle above the example to compare."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleDo("Keep events short: an actor, a verb, the thing that changed, and when."),
                        SampleDo("Give the first comment the action \"opened\" and the author an \"Author\" role, so later replies by them are easy to spot."),
                        SampleDo("Use Tone() only on events that change the state of the thread (closed, reopened, merged)."),
                        SampleDont("Render untrusted Markdown with the default sanitization if it should not link out or load images: pass MarkdownSanitization.NoLinksOrEmbeddedContent to Body()."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Issue page: conversation and metadata panel"),
                        BuildIssue())).SetTitle("Usage")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Comment on its own"),
                        ConversationComment("Lena Park", "Looks good to me - **ship it** 🚀", "2 hours ago")
                           .Role("Member")
                           .Reaction("🎉", 3)
                           .Highlighted(),
                        SampleSubTitle("Events"),
                        Conversation().Children(
                            ConversationEvent(UIcons.UserAdd, "Ana Souza", "assigned", "yesterday").Inline("Marco Ricci"),
                            ConversationEvent(UIcons.CodeCommit, "Marco Ricci", "added a commit that referenced this issue", "5 hours ago")
                               .Detail(TextBlock("Fix the rail under a collapsed comment   a41c9e2").Small().Foreground(Theme.Secondary.Foreground)),
                            ConversationEvent(UIcons.Refresh, "Ana Souza", "reopened this", "1 hour ago").Tone(BadgeTone.Success),
                            ConversationEvent(UIcons.Ban, "Lena Park", "closed this as not planned", "just now").Tone(BadgeTone.Danger))
                    )).SetTitle("Building blocks")))
               .SeeAlso(typeof(MetadataPanelSample), typeof(TimelineSample), typeof(ChatSample), typeof(MarkdownBlockSample), typeof(AvatarSample));
        }

        private static IComponent BuildIssue()
        {
            var ana   = "Ana Souza";
            var marco = "Marco Ricci";
            var lena  = "Lena Park";

            var title = VStack().WS().PB(16).Children(
                HStack().WS().AlignItemsCenter().Children(
                    TextBlock("Rail breaks under a highlighted comment in dark mode").MediumPlus().SemiBold(),
                    TextBlock("#1482").MediumPlus().Foreground(Theme.Secondary.Foreground).PL(8)),
                HStack().AlignItemsCenter().PT(8).Children(
                    Badge("Open").Success().Pill().SetIcon(Tesserae.Icon.Transform(UIcons.CircleDashed, UIconsWeight.Regular)),
                    TextBlock($"{ana} opened this issue on May 18 · 4 comments").Small().Foreground(Theme.Secondary.Foreground).PL(8)));

            var conversation = Conversation();

            conversation.Add(ConversationComment(ana,
                    "When a comment is highlighted in **dark mode**, the vertical rail stops just above it and resumes below, leaving a gap.\n\n" +
                    "### Steps to reproduce\n\n" +
                    "1. Switch to dark mode\n" +
                    "2. Open any issue and follow a link to a comment\n" +
                    "3. Look at the rail above the highlighted card\n\n" +
                    "Expected: the rail is continuous. Screenshot below.",
                    "on May 18")
               .Action("opened")
               .Role("Author")
               .Role("Member")
               .Attachment(Image("./assets/img/slide-16-9.svg").W(320))
               .Reaction("👍", 4)
               .Reaction("👀", 2, reacted: true)
               .Menu(MenuItems())
               .OnReact((c, emoji, added) => Toast().Information("Reaction", $"{(added ? "Added" : "Removed")} {emoji}")));

            conversation.Add(ConversationEvent(UIcons.Tags, ana, "added", "on May 18").Inline(BugLabel(), DarkModeLabel()));
            conversation.Add(ConversationEvent(UIcons.UserAdd, lena, "assigned", "on May 19").Inline(marco));

            conversation.Add(ConversationComment(marco,
                    "I can reproduce it. The highlight draws a `box-shadow` ring, and the card above has `position: relative` with an opaque background, so it covers the rail.\n\n" +
                    "```css\n.tss-conversation-comment-highlighted {\n    box-shadow: 0 0 0 1px var(--tss-primary-background-color);\n}\n```",
                    "on May 20")
               .Role("Contributor")
               .Edited(tooltip: "Edited by Marco Ricci on May 20")
               .Reaction("❤️", 1)
               .Menu(MenuItems())
               .OnReact((c, emoji, added) => { }));

            conversation.Add(ConversationEvent(UIcons.CodePullRequest, marco, "linked a pull request that will close this issue", "on May 21")
               .Detail(HStack().AlignItemsCenter().Children(
                    Icon(UIcons.CodePullRequest, color: Theme.Success.Background).PR(6),
                    TextBlock("Keep the rail above highlighted comments").SemiBold().Small(),
                    TextBlock("#1490").Small().Foreground(Theme.Secondary.Foreground).PL(4))));

            conversation.Add(ConversationComment(ana, "Thanks! Confirmed fixed on the preview build.", "on May 22")
               .Role("Author")
               .Role("Member")
               .Reaction("🎉", 2)
               .Menu(MenuItems())
               .OnReact((c, emoji, added) => { }));

            conversation.Add(ConversationEvent(UIcons.CheckCircle, ana, "closed this as completed", "on May 22").Tone(BadgeTone.AI));

            var composer = ConversationComposer(Avatar(initials: "ME"))
               .Actions(Button("Reopen issue").SetIcon(UIcons.Refresh));

            composer.OnSubmit((c, text) =>
            {
                conversation.Add(ConversationComment("You", text, "just now", Avatar(initials: "ME")).Menu(MenuItems()).OnReact((_, __, ___) => { }));
            });

            conversation.Composer(composer);

            var panel = MetadataPanelSample.BuildPanel();

            var compact = Toggle("Compact").OnChange((t, _) =>
            {
                conversation.Compact(t.IsChecked);
                panel.Compact(t.IsChecked);
            });

            return VStack().WS().Children(
                title,
                compact.PB(12),
                HStack().WS().Wrap().AlignItems(ItemAlign.Start).Children(
                    conversation.Grow().Style(s => { s.flexBasis = "480px"; s.minWidth = "0"; }).PR(24).PB(16),
                    panel.W(256).NoShrink()));
        }

        private static ContextMenu.Item[] MenuItems() => new[]
        {
            ContextMenuItem("Copy link").OnClick(() => Toast().Information("Copied", "Link copied")),
            ContextMenuItem("Quote reply"),
            ContextMenuItem().Divider(),
            ContextMenuItem("Edit"),
            ContextMenuItem("Delete"),
        };

        internal static Tag BugLabel()      => Tag("bug").Pill().Tint("#d73a4a");
        internal static Tag DarkModeLabel() => Tag("dark mode").Pill().Tint("#5319e7");

        public HTMLElement Render() => _content.Render();
    }
}
