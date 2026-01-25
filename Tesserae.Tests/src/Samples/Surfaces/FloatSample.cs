using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 0, Icon = UIcons.GameBoardAlt)]
    public class FloatSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public FloatSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(FloatSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Float components are used to place content in absolute-positioned overlays within a relative container. They allow for precise placement of UI elements, such as badges, help icons, or status indicators, without affecting the layout of surrounding components.")))
               .Section(Stack().WidthStretch().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use Float when you need to position an element independently of the normal document flow. Always ensure the parent container is set to 'Relative' positioning to constrain the floated element. Be careful not to obscure important content or interactive elements beneath the overlay. Use meaningful positions that correlate logically with the parent content.")))
               .Section(
                    Stack().Children(
                        SampleTitle("Usage"),
                        TextBlock("Possible Positions").Medium(),
                        Stack().Relative().Horizontal().WS().Height(400.px()).Children(
                            Float(Button("TopLeft"),      Float.Position.TopLeft),
                            Float(Button("TopMiddle"),    Float.Position.TopMiddle),
                            Float(Button("TopRight"),     Float.Position.TopRight),
                            Float(Button("LeftCenter"),   Float.Position.LeftCenter),
                            Float(Button("Center"),       Float.Position.Center),
                            Float(Button("RightCenter"),  Float.Position.RightCenter),
                            Float(Button("BottomLeft"),   Float.Position.BottomLeft),
                            Float(Button("BottonMiddle"), Float.Position.BottonMiddle),
                            Float(Button("BottomRight"),  Float.Position.BottomRight)
                        )));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}