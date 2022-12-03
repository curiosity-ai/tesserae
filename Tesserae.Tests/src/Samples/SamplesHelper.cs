using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public static class SamplesHelper
    {
        public static IComponent SampleHeader(string sampleType)
        {
            var text = sampleType.Replace("Sample", "");

            return Stack()
               .Horizontal()
               .WidthStretch()
               .Children(
                    TextBlock(text).XLarge().Bold(),
                    Raw().Grow(1),
                    Button().SetIcon(LineAwesome.Code).SetTitle("View code for this sample").OnClick(() => ShowSampleCode(sampleType)));
        }

        public static void ShowSampleCode(string sampleType)
        {
            var text = sampleType.Replace("Sample", "");

            Modal(text + " sample code")
               .LightDismiss().W(80.vh()).ShowCloseButton()
               .Content(TextArea(SamplesSourceCode.GetCodeForSample(sampleType)).WS().H(80.vh()))
               .Show();
        }

        public static IComponent SampleTitle(string    text) => TextBlock(text).SemiBold().MediumPlus().PaddingBottom(16.px());
        public static IComponent SampleSubTitle(string text) => TextBlock(text).SemiBold().Medium().PaddingBottom(16.px());
        public static IComponent SampleDo(string       text) => Label(Raw(I(_("las la-check", styles: s => s.color = "#107c10"))).PaddingRight(8.px())).SetContent(TextBlock(text)).Inline();
        public static IComponent SampleDont(string     text) => Label(Raw(I(_("las la-times", styles: s => s.color = "#e81123"))).PaddingRight(8.px())).SetContent(TextBlock(text)).Inline();
    }
}