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
                    TextBlock("Tutorial modals are used for processes where the user can be heavily guided, but still needs to enter data."),
                    TextBlock("For usage requiring a quick choice from the user, Dialog may be a more appropriate control.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    HStack().Children(
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Do"),
                            SampleDo("Use Modals for interactions where something is created and has multiple fields, such as creating a user."),
                            SampleDo("Always have at least one focusable element inside a Modal.")),
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Don't"),
                            SampleDont("Don’t overuse Tutorial Modals. In some cases they can be perceived as interrupting workflow, and too many can be a bad user experience.")))))
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