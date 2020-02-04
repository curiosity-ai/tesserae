using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    public class EditableLabelSample : IComponent
    {
        private IComponent _content;

        public EditableLabelSample()
        {
            _content = SectionStack()
                      .Title(
                             TextBlock("EditableLabel")
                                .XLarge()
                                .Bold())
                      .Section(Stack()
                                  .Children(SampleTitle("Overview"),
                                            TextBlock("TODO")))
                      .Section(Stack()
                                  .Children(SampleTitle("Best Practices"),
                                            Stack()
                                               .Horizontal()
                                               .Children(Stack()
                                                        .Width(40, Unit.Percents)
                                                        .Children(SampleSubTitle("Do")
//                    SampleDo("Use sentence casing, e.g. “First name”."),
//                    SampleDo("Be short and concise."),
//                    SampleDo("When adding a Label to components, use the text as a noun or short noun phrase.")
                                                                 ),
                                                         Stack().Width(40, Unit.Percents).Children(SampleSubTitle("Don't")
//                SampleDo("Use Labels as instructional text, e.g. “Click to get started”."),
//                                                                                                   SampleDo("Don’t use full sentences or complex punctuation (colons, semicolons, etc.)."))
                                                                                                  )
                                                        )
                                           )
                              )
                      .Section(Stack().Children(
                                                SampleTitle("Usage"),
                                                TextBlock("Label").Medium(),
                                                EditableLabel("I'm Label")
//                                                Label("I'm a disabled Label").Disabled(),
//                                                Label("I'm a required Label").Required(),
//                                                Label("A Label for An Input").SetContent(TextBox())
                                               ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}