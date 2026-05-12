using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public static class SamplesHelper
    {
        public static SectionStack SampleTitle(this SectionStack stack, Type sampleType, UIcons icon, string subtitle)
        {
            var text = Sample.FormatSampleName(sampleType);
            return stack.Title(SectionTitle(icon, text, subtitle, Button("Documentation").SetIcon(UIcons.Books).OnClick(() => window.location.href = "https://docs.curiosity.ai/tesserae/"), Button("View Code").SetIcon(UIcons.SquareTerminal).Tooltip("View source-code for this sample page").OnClick(() => ShowSampleCode(sampleType.Name))).NoWrap());
        }

        public static SectionStack SampleTitle(this SectionStack stack, string sampleType, UIcons icon, string subtitle)
        {
            var text = Sample.FormatSampleName(sampleType);
            return stack.Title(SectionTitle(icon, text, subtitle, Button("Documentation").SetIcon(UIcons.Books).OnClick(() => window.location.href = "https://docs.curiosity.ai/tesserae/"), Button("View Code").SetIcon(UIcons.SquareTerminal).Tooltip("View source-code for this sample page").OnClick(() => ShowSampleCode(sampleType))).NoWrap());
        }

        public static void ShowSampleCode(string sampleType)
        {
            var text = sampleType.Replace("Sample", "");

            Modal(text + " sample code")
               .LightDismiss().W(80.vh()).ShowCloseButton()
               .Content(TextArea(SamplesSourceCode.GetCodeForSample(sampleType)).WS().H(80.vh()))
               .Show();
        }

        public static IComponent SampleSubTitle(string text) => TextBlock(text).SemiBold().PT(16).PB(8);
        public static IComponent SampleDo(string       text) => Label(Raw(I(_("las la-check", styles: s => s.color = "#107c10"))).PaddingRight(8.px())).SetContent(TextBlock(text)).Inline();
        public static IComponent SampleDont(string     text) => Label(Raw(I(_("las la-times", styles: s => s.color = "#e81123"))).PaddingRight(8.px())).SetContent(TextBlock(text)).Inline();
    }
}