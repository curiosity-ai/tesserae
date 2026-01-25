using Tesserae;
using Tesserae.Tests;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 20, Icon = UIcons.Indent)]
    public class TutorialModalSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public TutorialModalSample()
        {
            var container = Raw();

            _content = SectionStack()
               .Title(SampleHeader(nameof(TutorialModalSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("TutorialModal is a specialized modal designed for guided processes, such as onboarding or feature walkthroughs. It combines a large content area with a dedicated help panel and an optional illustrative image, providing a structured environment for users to learn while they interact.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use TutorialModals for complex tasks that benefit from additional explanation and guidance. Ensure that the help text is clear and directly relates to the fields in the content area. Use images or icons to provide visual cues. Always provide a clear way for users to complete or discard the process. Avoid overwhelming users with too much information; keep both the content and the help text concise.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    Button("Open Tutorial Modal").OnClick((s,       e) => SampleTutorialModal().Show()),
                    Button("Open Large Tutorial Modal").OnClick((s, e) => SampleTutorialModal().Height(90.vh()).Width(90.vw()).Show()),
                    SampleTitle("Embedded Modal"),
                    Button("Open Modal Below").OnClick((s, e) => container.Content(SampleTutorialModal().Border("#ffaf66", 5.px()).ShowEmbedded())),
                    container
                ));
        }

        private static TutorialModal SampleTutorialModal()
        {
            return TutorialModal()
               .Var(out var tutorialModal)
               .SetTitle("This is a Tutorial Modal")
               .SetHelpText("Lorem ipsum dolor sit amet, consectetur adipiscing elit,<b> sed do </b> eiusmod tempor incididunt ut labore et dolore magna aliqua. ", treatAsHTML: true)
               .SetImageSrc("./assets/img/box-img.svg", 16.px())
               .SetContent(
                    VStack().S().ScrollY().Children(
                        Label("Input 1").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                        Label("Input 2").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                        Label("Input 3").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                        Label("Input 4").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                        Label("Input 5").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                        Label("Input 6").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                        Label("Input 7").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                        Label("Input 8").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                        Label("Input 9").SetContent(TextBox().SetPlaceholder("Enter your input here..."))))
               .SetFooterCommands(
                    Button("Discard").OnClick((_,        __) => tutorialModal.Hide()),
                    Button("Save").Primary().OnClick((_, __) => tutorialModal.Hide()));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}