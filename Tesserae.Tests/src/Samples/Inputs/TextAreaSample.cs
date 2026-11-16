using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Inputs, Order = 20, Icon = UIcons.AlignJustify, Description = "Multi-line text input")]
    public class TextAreaSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public TextAreaSample()
        {
            var badged     = TextArea("Brake sensor calibration failed on line 3 in eleven of the last fourteen runs.").AI(badge: true).Id("sample-ai-badge-textarea");
            var regenerate = Button("Regenerate").AISubtle().Id("sample-ai-badge-textarea-regenerate");

            //SetText hands the field a new value from the model, so it counts as AI-generated again: the badge goes back to "AI".
            regenerate.OnClick(() => badged.SetText("Line 3's brake sensor calibration failed in 11 of the last 14 runs, always after a tool change."));

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(TextAreaSample), UIcons.AlignLeft, "A control to input multiple lines of text")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("TextAreas allow users to enter and edit multi-line text. They are commonly used for comments, descriptions, or any input that requires multiple lines of text."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use a TextArea when the expected input might be long or span multiple lines. Always pair it with a clear label. Consider using the AutoResize functionality to provide a better user experience without taking up too much initial screen real estate."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic TextAreas"),
                    VStack().Children(
                        Label("Standard").SetContent(TextArea()),
                        Label("Placeholder").SetContent(TextArea().SetPlaceholder("Enter your description...")),
                        Label("Disabled").Disabled().SetContent(TextArea("Disabled content").Disabled()),
                        Label("Read-only").SetContent(TextArea("Read-only content").ReadOnly())
                    ),
                    SampleSubTitle("Dictation"),
                    VStack().Children(
                        Label("With a microphone button").SetContent(TextArea().SetPlaceholder("Press the microphone and speak...").WithDictation())
                    ),
                    SampleSubTitle("Formatted initial value"),
                    VStack().Children(
                        TextBlock("InitialValue draws bold, italic and coloured text in the empty field. It is not the field's Text, and it disappears as soon as the user edits."),
                        Label("Template").SetContent(TextArea().InitialValue("<b>Summary</b><br><i>What happened?</i><br><span style=\"color:#e5484d\">Impact</span> and <font color=\"#3b9be8\"><b>next steps</b></font>...").H(120).Id("sample-initial-textarea"))
                    ),
                    SampleSubTitle("AI"),
                    VStack().Children(
                        Label("Generated").SetContent(TextArea("Brake sensor calibration failed on line 3 in eleven of the last fourteen runs.").AI()),
                        Label("With a badge (edit it to see the badge change)").SetContent(VStack().WS().Children(badged.WS(), regenerate.AlignStart())),
                        Label("Custom edited text, with a microphone button").SetContent(TextArea("Brake sensor calibration failed on line 3 in eleven of the last fourteen runs.").WithDictation().AI(badge: true, editedText: "Modified").Id("sample-ai-badge-textarea-custom"))
                    ),
                    SampleSubTitle("Auto Resize"),
                    VStack().Children(
                        Label("AutoResize (allowShrink = true)").SetContent(TextArea("Type multiple lines here...").AutoResize(allowShrink: true)),
                        Label("AutoResize (allowShrink = false)").SetContent(TextArea("Type multiple lines here...").AutoResize(allowShrink: false)),
                        Label("AutoResize (minHeight = 100)").SetContent(TextArea("Type here...").AutoResize(minHeight: 100)),
                        Label("AutoResize (maxHeight = 150)").SetContent(TextArea("Type many lines to see scrolling...").AutoResize(maxHeight: 150))
                    ),
                    SampleSubTitle("Filling the available height"),
                    VStack().Children(
                        TextBlock("A TextArea with Grow() stretches to fill whatever height is left over in its parent stack."),
                        VStack().WS().H(220).Children(
                            TextBlock("Fixed header"),
                            TextArea("This text area fills the rest of the 220px stack.").Grow().WS()
                        )
                    ),
                    SampleSubTitle("Validation"),
                    VStack().Children(
                        Label("Required").Required().SetContent(TextArea()),
                        Label("Custom Error").SetContent(TextArea().Error("Something went wrong").IsInvalid())
                    )
                )).SetTitle("Usage")))
               .SeeAlso(typeof(TextBoxSample), typeof(AnnotatedTextEditorSample), typeof(EditableLabelSample), typeof(MarkdownBlockSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
