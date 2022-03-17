using System;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class SchedulePickerSample : IComponent
    {
        private readonly IComponent _content;
        public SchedulePickerSample()
        {
            var picker = new SchedulePicker().InactiveTooltip("Inactive").HalfActiveTooltip("Half Active").ActiveTooltip("Active");

            _content = SectionStack()
               .Title(SampleHeader(nameof(SchedulePickerSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("TODO")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    Stack().Horizontal().Children(
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Do")
//                    SampleDo("Filter files by supported types"),
//                    SampleDo("Provide a message for the file drop area"),
//                    SampleDo($"Attach the {nameof(SchedulePicker.OnFileDropped)} event handler")
                        ),
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Don't"),
                            SampleDont("TODO")))))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("File Selector"),
                    Label("Selected file size: ").Inline().SetContent(TextBlock("").Var(out var size)),
//                    FileSelector().OnFileSelected((fs, e) => size.Text = fs.SelectedFile.size.ToString() + " bytes"),
//                    FileSelector().SetPlaceholder("You must select a zip file").Required().SetAccepts(".zip").OnFileSelected((fs,e) => size.Text = fs.SelectedFile.size.ToString() + " bytes"),
//                    FileSelector().SetPlaceholder("Please select any image").SetAccepts("image/*").OnFileSelected((fs, e) => size.Text = fs.SelectedFile.size.ToString() + " bytes"),
//                    SampleSubTitle("File Drop Area"),
//                    Label("Dropped Files: ").SetContent(Stack().Var(out var droppedFiles)),
                    Button("current").OnClick(() =>
                    {
                        var now = DateTimeOffset.Now;
                        var currentDay = now.DayOfWeek.ToWeekStartMonday();
                        var currentHour = now.Hour;
                        console.log("currentDay",               SchedulePicker.GetWeekDays[currentDay], "currentHour", currentHour);
                        console.log("picker.CurrentState() : ", Enum.GetName(typeof(SchedulePicker.ScheduleState), picker.CurrentState()));
                    }),
                    Button("Next").OnClick(() =>
                    {
                        var next = picker.NextStateChange();
                        if (next.HasValue)
                        {
                            console.log("picker.NextStateChange() : ", SchedulePicker.GetWeekDays[next.Value.changeDateTime.DayOfWeek.ToWeekStartMonday()],
                                "day ", next.Value.changeDateTime.Day,  "hour ", next.Value.changeDateTime.Hour, "state ", Enum.GetName(typeof(SchedulePicker.ScheduleState), next.Value.changeTo));
                        }
                        else
                        {
                            console.log("picker.NextStateChange() : ", null);
                        }
                    }),
                    picker
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}