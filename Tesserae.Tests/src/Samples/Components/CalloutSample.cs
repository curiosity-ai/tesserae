using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 18, Icon = UIcons.CommentInfo)]
    public class CalloutSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public CalloutSample()
        {
            var btn = Button("Click for Callout");
            var callout = Callout()
                .SetHeader(TextBlock("Callout Header").SemiBold())
                .SetContent(TextBlock("This is the content of the callout."));

            btn.OnClick(() => callout.ShowFor(btn));

            _content = SectionStack()
               .Title(SampleHeader(nameof(CalloutSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Callouts are popovers that anchor to a specific element.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    btn
                ));
        }

        public H5.Core.dom.HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
