using System;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Lists, Order = 110, Icon = UIcons.IdCardClipAlt, Description = "A sidebar of assignees, labels and other facts")]
    public class MetadataPanelSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public MetadataPanelSample()
        {
            _content = SectionStack().Secondary().WidthStretch()
               .SampleTitle(typeof(MetadataPanelSample), UIcons.IdCardClipAlt, "The facts column beside a conversation")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("MetadataPanel is the narrow column an issue tracker shows next to the conversation: a stack of MetadataSections divided by hairlines, each with a small title, an optional gear button to edit it, and its content - or a muted line such as \"No one assigned\" while it has none."),
                        TextBlock("Sections have helpers for the usual content: Person (an Avatar and a name), Labels (Tags tinted with any colour through Tag.Tint), Item (an icon, a title and a muted line - a milestone, a linked pull request) and Avatars (a row of participants with a \"+N\" for the rest). Content(...) takes any component."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleDo("Give every section an empty text that says what is missing (\"No milestone\"), not just \"None\"."),
                        SampleDo("Use OnEdit to open a ContextMenu or Popover anchored to the gear button it hands you."),
                        SampleDont("Put long prose in a section: the panel is about 256px wide and meant to be scanned."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(HStack().WS().Wrap().Children(
                        VStack().W(256).PR(32).PB(16).Children(SampleSubTitle("Filled in"), BuildPanel()),
                        VStack().W(256).PB(16).Children(SampleSubTitle("Empty sections"), BuildEmptyPanel())
                    )).SetTitle("Usage")))
               .SeeAlso(typeof(ConversationSample), typeof(PropertyGridSample), typeof(AvatarSample), typeof(BadgeSample));
        }

        internal static MetadataPanel BuildPanel()
        {
            var labels = MetadataSection("Labels", "No labels")
               .Labels(ConversationSample.BugLabel(), ConversationSample.DarkModeLabel());

            labels.OnEdit(anchor => ContextMenu().Items(
                    ContextMenuItem("bug").OnClick(() => labels.SetContent().Labels(ConversationSample.BugLabel())),
                    ContextMenuItem("dark mode").OnClick(() => labels.SetContent().Labels(ConversationSample.DarkModeLabel())),
                    ContextMenuItem("good first issue").OnClick(() => labels.SetContent().Labels(Tag("good first issue").Pill().Tint("#7057ff"))),
                    ContextMenuItem().Divider(),
                    ContextMenuItem("Clear labels").OnClick(() => labels.Clear()))
               .ShowFor(anchor));

            return MetadataPanel().Children(
                MetadataSection("Assignees", "No one assigned")
                   .Person("Marco Ricci", Avatar(initials: "MR"), "marcor")
                   .OnEdit(anchor => Toast().Information("Assignees", "Open an assignee picker here")),
                labels,
                MetadataSection("Type", "No type")
                   .Content(Badge("Bug").Danger().Outline().Pill()),
                MetadataSection("Projects", "No projects")
                   .Item(UIcons.Table, "Tesserae roadmap", "Status: Done"),
                MetadataSection("Milestone", "No milestone")
                   .Item(UIcons.Flag, "v2026.6", "Due by June 30 · 82% complete", onClick: () => Toast().Information("Milestone", "v2026.6"))
                   .Content(ProgressIndicator().Progress(82).Height(4.px())),
                MetadataSection("Development", "No branches or pull requests")
                   .Item(UIcons.CodePullRequest, "Keep the rail above highlighted comments", "curiosity-ai/tesserae#1490", Theme.Success.Background),
                MetadataSection("Participants")
                   .Avatars(new[] { Avatar(initials: "AS"), Avatar(initials: "MR"), Avatar(initials: "LP"), Avatar(initials: "JK"), Avatar(initials: "TO") }, max: 5, total: 12),
                MetadataSection("Notifications")
                   .Content(Button("Unsubscribe").SetIcon(UIcons.BellSlash).WS())
                   .Text("You're receiving notifications because you're watching this repository."));
        }

        private static MetadataPanel BuildEmptyPanel()
        {
            return MetadataPanel().Children(
                MetadataSection("Assignees", "No one assigned").OnEdit(_ => { }),
                MetadataSection("Labels", "No labels").OnEdit(_ => { }),
                MetadataSection("Milestone", "No milestone").OnEdit(_ => { }),
                MetadataSection("Relationships"));
        }

        public HTMLElement Render() => _content.Render();
    }
}
