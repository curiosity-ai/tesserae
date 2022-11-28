using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = LineAwesome.SlidersH)]
    public class SplitViewSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SplitViewSample()
        {
            var splitView = SplitView();

            _content = SectionStack()
               .S()
               .Title(SampleHeader(nameof(SplitViewSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("TODO")
                    //                TextBlock("ChoiceGroup emphasize all options equally, and that may draw more attention to the options than necessary. Consider using other controls, unless the options deserve extra attention from the user. For example, if the default option is recommended for most users in most situations, use a Dropdown component instead."),
                    //                TextBlock("If there are only two mutually exclusive options, combine them into a single Checkbox or Toggle switch. For example, use a Checkbox for \"I agree\" instead of ChoiceGroup buttons for \"I agree\" and \"I don't agree.\"")
                ))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    HStack().Children(
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Do"),
                            SampleDo("TODO")),
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Don't"),
                            SampleDont("TODO")))))
               .Section(
                    VStack().S().Children(
                        HStack().WS().Children(Button("Resizable").OnClick(() => splitView.Resizable()), Button("Non-resizable").OnClick(() => splitView.NoSplitter())),
                        splitView.Left(Stack().S().Background("green"))
                           .Right(Stack().S().Background("blue"))
                           .Resizable()
                           .WS().H(10).Grow()), grow: true);
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}