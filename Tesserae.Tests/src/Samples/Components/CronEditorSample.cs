using Transpose;
using System;
using static Transpose.Core.dom;
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
            _content = SectionStack().Secondary()
                .SampleTitle(typeof(CronEditorSample), UIcons.Clock, "A component to edit cron expressions")
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("CronEditor allows users to schedule tasks using a simplified UI for daily schedules, with a fallback to raw cron expressions for advanced users.")
                )).SetTitle("Overview")))
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic"),
                    CronEditor(),

                    SampleSubTitle("With Days Selection Disabled"),
                    CronEditor().DaysEnabled(false),

                    SampleSubTitle("Custom Interval (30 mins)"),
                    CronEditor().MinuteInterval(30),

                    SampleSubTitle("With Initial Value (Custom)"),
                    CronEditor("*/5 * * * *"),

                    SampleSubTitle("Initially Disabled"),
                    CronEditor(initialEnabled: false),

                    SampleSubTitle("With Enable Checkbox Hidden"),
                    CronEditor().ShowEnableCheckbox(false),

                    SampleSubTitle("Observable"),
                    GetObservableExample()
                )).SetTitle("Usage")));
        }

        private IComponent GetObservableExample()
        {
            var editor = CronEditor();
            var text = TextBlock("Current value: " + editor.Value.cron + " (" + (editor.Value.enabled ? "Enabled" : "Disabled") + ")");
            editor.OnChange((s) => text.Text = "Current value: " + s.Value.cron + " (" + (s.Value.enabled ? "Enabled" : "Disabled") + ")");
            return VStack().Children(editor, text);
        }

        public HTMLElement Render() => _content.Render();
    }
}
