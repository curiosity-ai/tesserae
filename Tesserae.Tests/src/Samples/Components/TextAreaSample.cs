using System;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 1, Icon = UIcons.TextSize)]
    public class TextAreaSample : IComponent, ISample
    {
        public HTMLElement Render()
        {
            var taShrink = TextArea("Type here...").AutoResize(allowShrink: true);
            var taNoShrink = TextArea("Type here...").AutoResize(allowShrink: false);

            taShrink.InnerElement.id = "ta-shrink";
            taNoShrink.InnerElement.id = "ta-no-shrink";

            return Stack().Children(
                TextBlock("TextArea AutoResize Sample").SemiBold().MediumPlus(),
                Label("AutoResize (allowShrink = true)").SetContent(taShrink),
                Label("AutoResize (allowShrink = false)").SetContent(taNoShrink)
            ).Render();
        }
    }
}
