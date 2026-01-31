using H5;
using System;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 100, Icon = UIcons.Clock)]
    public sealed class CronEditorSample : IComponent, ISample
    {
        private readonly IComponent _content;
        public CronEditorSample()
        {
            _content = SectionStack()
                .Title(SampleHeader(nameof(CronEditorSample)))
                .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("CronEditor allows users to schedule tasks using a simplified UI for daily schedules, with a fallback to raw cron expressions for advanced users.")
                ))
                .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic"),
                    CronEditor(),

                    SampleSubTitle("With Days Selection Disabled"),
                    CronEditor().DaysEnabled(false),

                    SampleSubTitle("Custom Interval (30 mins)"),
                    CronEditor().MinuteInterval(30),

                    SampleSubTitle("With Initial Value (Custom)"),
                    CronEditor("*/5 * * * *"),

                    SampleSubTitle("Observable"),
                    GetObservableExample()
                ));
        }

        private IComponent GetObservableExample()
        {
            var editor = CronEditor();
            var text = TextBlock("Current value: " + editor.Value);
            editor.OnChange((s) => text.Text = "Current value: " + s.Value);
            return VStack().Children(editor, text);
        }

        public HTMLElement Render() => _content.Render();
    }
}
