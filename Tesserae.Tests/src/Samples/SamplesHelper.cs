using System;
using System.Linq;
using System.Reflection;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public static class SamplesHelper
    {
        public static SectionStack SampleTitle(this SectionStack stack, Type sampleType, UIcons icon, string subtitle)
        {
            var text = Sample.FormatSampleName(sampleType);
            return stack.Title(icon, text, subtitle, Button("Documentation").SetIcon(UIcons.Books).OnClick(() => window.location.href = "https://docs.curiosity.ai/tesserae/"), Button("View Code").SetIcon(UIcons.SquareTerminal).Tooltip("View source-code for this sample page").OnClick(() => ShowSampleCode(sampleType.Name)));
        }

        public static SectionStack SampleTitle(this SectionStack stack, string sampleType, UIcons icon, string subtitle)
        {
            var text = Sample.FormatSampleName(sampleType);
            return stack.Title(icon, text, subtitle, Button("Documentation").SetIcon(UIcons.Books).OnClick(() => window.location.href = "https://docs.curiosity.ai/tesserae/"), Button("View Code").SetIcon(UIcons.SquareTerminal).Tooltip("View source-code for this sample page").OnClick(() => ShowSampleCode(sampleType)));
        }

        /// <summary>
        /// Closes a sample page with a "See also" section: one button per related sample, each navigating
        /// to that sample's page. Pass the sample types (e.g. <c>typeof(GridSample)</c>) in the order they
        /// should be read — most closely related first.
        /// </summary>
        public static SectionStack SeeAlso(this SectionStack stack, params Type[] relatedSamples)
        {
            var links = HStack().WS().Wrap().Gap(8.px()).PT(8);

            foreach (var sampleType in relatedSamples)
            {
                var name = Sample.FormatSampleName(sampleType);

                links.Add(Button(name).SetIcon(IconFor(sampleType)).OnClick(() => Router.Navigate(RouteFor(sampleType))));
            }

            return stack.FlatSection(VStack().WS().Children(
                Card(VStack().WS().Children(
                    TextBlock("Samples that usually come up together with this one — components it composes with, alternatives to it, or the layout and styling topics behind it."),
                    links)).SetTitle("See also")));
        }

        // Mirrors the routes App.cs registers for every sample.
        private static string RouteFor(Type sampleType) => $"#/view/{Sample.FormatSampleName(sampleType)}";

        // The icon a sample declares on its [SampleDetails], so a link looks like its sidebar entry.
        private static UIcons IconFor(Type sampleType)
        {
            var details = sampleType.GetCustomAttributes(typeof(SampleDetailsAttribute), true).FirstOrDefault() as SampleDetailsAttribute;

            return details is object ? details.Icon : UIcons.Circle;
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
        public static IComponent SampleDo(string       text) => Label(Raw(I(Att("las la-check", styles: s => s.color = "#107c10"))).PaddingRight(8.px())).SetContent(TextBlock(text)).Inline();
        public static IComponent SampleDont(string     text) => Label(Raw(I(Att("las la-times", styles: s => s.color = "#e81123"))).PaddingRight(8.px())).SetContent(TextBlock(text)).Inline();
    }
}
