using Tesserae.Components;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public static class SamplesHelper
    {
        public static IComponent SampleTitle(string text) => TextBlock(text).SemiBold().MediumPlus().PaddingBottom(16.px());
        public static IComponent SampleSubTitle(string text) => TextBlock(text).SemiBold().Medium().PaddingBottom(16.px());
        public static IComponent SampleDo(string text) => Label(Raw(I(_("las la-check", styles: s => s.color = "#107c10"))).PaddingRight(8.px())).SetContent(TextBlock(text)).Inline();
        public static IComponent SampleDont(string text) => Label(Raw(I(_("las la-times", styles: s => s.color = "#e81123"))).PaddingRight(8.px())).SetContent(TextBlock(text)).Inline();
    }
}
