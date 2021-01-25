using Tesserae;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class TutorialModalSample : IComponent
    {
        private readonly IComponent _content;

        public TutorialModalSample()
        {

            _content = SectionStack()
               .Title(SampleHeader(nameof(TutorialModalSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Tutorial modals are used for processes where the user can be heavily guided, but still needs to enter data."),
                    TextBlock("For usage requiring a quick choice from the user, Dialog may be a more appropriate control.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    Stack().Horizontal().Children(
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Do"),
                            SampleDo("Use Modals for interactions where something is created and has multiple fields, such as creating a user."),
                            SampleDo("Always have at least one focusable element inside a Modal.")),
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Don't"),
                            SampleDont("Don’t overuse Tutorial Modals. In some cases they can be perceived as interrupting workflow, and too many can be a bad user experience.")))))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    Button("Open Tutorial Modal").OnClick((s, e) => TutorialModal()
                       .Var(out var tutorialModal)
                       .SetTitle("This is a Tutorial Modal")
                       .SetHelpText("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. ")
                       .SetImageSrc("./assets/img/box-img.svg")
                       .SetContent(
                            Label("Input 1").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                            Label("Input 1").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                            Label("Input 1").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                            Label("Input 1").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                            Label("Input 1").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                            Label("Input 1").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                            Label("Input 1").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                            Label("Input 1").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                            Label("Input 1").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                            Label("Input 1").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                            Label("Input 1").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                            Label("Input 1").SetContent(TextBox().SetPlaceholder("Enter your input here...")),
                            Label("Input 1").SetContent(TextBox().SetPlaceholder("Enter your input here...")))
                       .SetFooterCommands(
                            Button("Discard").OnClick((_,        __) => tutorialModal.Hide()),
                            Button("Save").Primary().OnClick((_, __) => tutorialModal.Hide())).Show())
                ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}