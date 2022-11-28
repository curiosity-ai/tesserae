using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 0, Icon = LineAwesome.Sign)]
    public class FloatSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public FloatSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(FloatSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Floats are used to create absolute-positioned overlays within other containers"))
                )
               .Section(Stack().WidthStretch().Children(
                    SampleTitle("Best Practices"),
                    Stack().WidthStretch().Horizontal().Children(
                        Stack().WidthStretch().Children(
                            SampleSubTitle("Do"),
                            SampleDo("Make sure you're not covering anything else under your container")
                        ),
                        Stack().WidthStretch().Children(
                            SampleSubTitle("Don't"),
                            SampleDont("Don't forget to add .Relative() to the parent Stack or Grid where you place a Float component!")
                        ))
                ))
               .Section(
                    Stack().Children(
                        SampleTitle("Usage"),
                        TextBlock("Possible Positions").Medium(),
                        Stack().Relative().Horizontal().WS().Height(400.px()).Children(
                            Float(Button("TopLeft"), Float.Position.TopLeft),
                            Float(Button("TopMiddle"), Float.Position.TopMiddle),
                            Float(Button("TopRight"), Float.Position.TopRight),
                            Float(Button("LeftCenter"), Float.Position.LeftCenter),
                            Float(Button("Center"), Float.Position.Center),
                            Float(Button("RightCenter"), Float.Position.RightCenter),
                            Float(Button("BottomLeft"), Float.Position.BottomLeft),
                            Float(Button("BottonMiddle"), Float.Position.BottonMiddle),
                            Float(Button("BottomRight"), Float.Position.BottomRight)
                        )));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}