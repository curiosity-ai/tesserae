using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = UIcons.TextSize)]
    public class LabelSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public LabelSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(LabelSample), UIcons.Tags, "A component to display a label")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Labels provide a name or title for a component or a group of components. They are essential for accessibility and helping users understand the purpose of input fields."),
                    TextBlock("While many Tesserae components have built-in labels, the standalone Label component offers more flexibility in positioning and styling."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use sentence casing for label text. Keep labels short and concise, typically using a noun or a short noun phrase. Do not use labels as instructional text; use TextBlocks or tooltips for that purpose. Ensure labels are positioned close to the components they describe. Use the 'Required' flag to clearly indicate mandatory fields."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic Labels"),
                    VStack().Children(
                        Label("Standard Label"),
                        Label("Required Label").Required(),
                        Label("Disabled Label").Disabled(),
                        Label("Primary Colored Label").Primary()
                    ),
                    SampleSubTitle("Label with Content"),
                    Label("Username").SetContent(TextBox().SetPlaceholder("Enter your username")),
                    SampleSubTitle("Inline Layouts"),
                    TextBlock("Labels can be displayed inline with their content, with optional automatic width synchronization."),
                    VStack().Children(
                        Label("Name").Inline().AutoWidth().SetContent(TextBox()),
                        Label("Department").Inline().AutoWidth().SetContent(TextBox()),
                        Label("Role").Inline().AutoWidth().SetContent(TextBox())
                    ),
                    SampleSubTitle("Right Aligned Labels"),
                    VStack().Children(
                        Label("Short").Inline().AutoWidth(alignRight: true).SetContent(TextBox()),
                        Label("A much longer label").Inline().AutoWidth(alignRight: true).SetContent(TextBox())
                    ),
                    SampleSubTitle("Rounded Labels"),
                    VStack().Children(
                        Label("Small rounded").Rounded(BorderRadius.Small),
                        Label("Medium rounded").Rounded(BorderRadius.Medium),
                        Label("Full rounded").Rounded(BorderRadius.Full)
                    )
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
