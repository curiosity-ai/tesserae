using Tesserae.Components;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public static class SamplesHelper
    {
        public static IComponent SampleTitle(string text) => TextBlock(text).SemiBold().MediumPlus().Padding(0, 0, 0, 16);
        public static IComponent SampleSubTitle(string text) => TextBlock(text).SemiBold().Medium().Padding(0, 0, 0, 16);
        public static IComponent SampleDo(string text) => Label(Raw(I(_("fal fa-check", styles: s => s.color = "#107c10"))).Padding(0,0,8,0)).SetContent(TextBlock(text)).Inline();
        public static IComponent SampleDont(string text) => Label(Raw(I(_("fal fa-times", styles: s => s.color = "#e81123"))).Padding(0, 0, 8, 0)).SetContent(TextBlock(text)).Inline();
    }
}
