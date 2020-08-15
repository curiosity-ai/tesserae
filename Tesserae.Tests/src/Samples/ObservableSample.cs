using System.Linq;
using H5.Core;
using Tesserae.Components;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    public class ObservableSample : IComponent
    {
        private readonly IComponent _content;

        public ObservableSample()
        {
            var boolObservable     = new SettableObservable<bool>(true);
            var stringObservable   = new SettableObservable<string>("Text that has been set in the Observable");
            var colorObservable    = new SettableObservable<string>("");
            var dateObservable     = new SettableObservable<string>("");
            var dateTimeObservable = new SettableObservable<string>("");
            var monthObservable    = new SettableObservable<string>("");
            var weekObservable     = new SettableObservable<string>("");
            var timeObservable     = new SettableObservable<string>("");
            var sliderObservable   = new SettableObservable<int>(0);

            _content = SectionStack()
               .Title(SampleHeader(nameof(ObservableSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Components that implement the \"Bindable\" interface can be bound to a \"Observable\" value."),
                    TextBlock("The internal state of the Component are then kept in sync with the Observable.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    Stack().Horizontal().Children(
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Do"),
                            SampleDo("TODO")),
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Don't"),
                            SampleDont("TODO")))))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SectionStack()
                       .Section(Stack().Children(
                            SampleTitle("Bindable to bool"),
                            Label("Value: ").Inline().SetContent(Defer(boolObservable, value => TextBlock(value.ToString()).AsTask())),
                            CheckBox("CheckBox global").Bind(boolObservable),
                            Toggle("Toggle global").Bind(boolObservable)))
                       .Section(Stack().Children(
                            SampleTitle("Bindable to string"),
                            Label("Value: ").Inline().SetContent(Defer(stringObservable, value => TextBlock(value.ToString()).AsTask())),
                            TextBox("Initial value TextBox").Bind(stringObservable),
                            TextArea("Initial value TextArea").Bind(stringObservable),
                            EditableLabel("Initial value EditableLabel").Bind(stringObservable),
                            EditableArea("Initial value EditableArea").Bind(stringObservable)))
                       .Section(Stack().Children(
                            SampleTitle("Slider"),
                            Slider(100, 0, 100, 1).Bind(sliderObservable),
                            Slider(100, 0, 100, 1).Bind(sliderObservable),
                            Label("Slider: ").Inline().SetContent(Defer(sliderObservable, value => TextBlock(value.ToString()).AsTask()))))
                       .Section(Stack().Children(
                            SampleTitle("ColorPicker"),
                            ColorPicker().Bind(colorObservable).Width(200.px()),
                            ColorPicker().Bind(colorObservable).Width(200.px()),
                            Label("Color: ").Inline().SetContent(Defer(colorObservable, value => TextBlock(value.ToString()).AsTask()))))
                       .Section(Stack().Children(
                            SampleTitle("DatePicker"),
                            new DatePicker().Bind(dateObservable).Width(200.px()),
                            new DatePicker().Bind(dateObservable).Width(200.px()),
                            Label("Date: ").Inline().SetContent(Defer(dateObservable, value => TextBlock(value.ToString()).AsTask()))))
                       .Section(Stack().Children(
                            SampleTitle("DateTimePicker"),
                            DateTimePicker().Bind(dateTimeObservable).Width(200.px()),
                            DateTimePicker().Bind(dateTimeObservable).Width(200.px()),
                            Label("DateTime: ").Inline().SetContent(Defer(monthObservable, value => TextBlock(value.ToString()).AsTask()))))
                       .Section(Stack().Children(
                            SampleTitle("MonthPicker"),
                            new MonthPicker((1, 1)).Bind(monthObservable).Width(200.px()),
                            new MonthPicker((1, 1)).Bind(monthObservable).Width(200.px()),
                            Label("Month: ").Inline().SetContent(Defer(colorObservable, value => TextBlock(value.ToString()).AsTask()))))
                       .Section(Stack().Children(
                            SampleTitle("TimePicker"),
                            new TimePicker().Bind(timeObservable).Width(200.px()),
                            new TimePicker().Bind(timeObservable).Width(200.px()),
                            Label("Time: ").Inline().SetContent(Defer(timeObservable, value => TextBlock(value.ToString()).AsTask()))))
                       .Section(Stack().Children(
                            SampleTitle("WeekPicker"),
                            new WeekPicker((1, 1)).Bind(weekObservable).Width(200.px()),
                            new WeekPicker((1, 1)).Bind(weekObservable).Width(200.px()),
                            Label("Week: ").Inline().SetContent(Defer(weekObservable, value => TextBlock(value.ToString()).AsTask()))))
                       .Section(Stack().Children(
                            SampleTitle("ChoiceGroup"),
                            ChoiceGroup("Observable ChoiceGroup").Required()
                               .Choices(
                                    Choice("Option A"),
                                    Choice("Option B"),
                                    Choice("Option C"),
                                    Choice("Option D")).Var(out var choiceGroup),
                            Label("Choice: ").Inline().SetContent(Defer(choiceGroup.AsObservable(), value => TextBlock(value?.Text ?? "").AsTask()))))
                       .Section(Stack().Children(
                            SampleTitle("Dropdown"),
                            Dropdown("Observable Multi Dropdown").Required().Width(200.px()).Multi()
                               .Items(
                                    DropdownItem("Option A"),
                                    DropdownItem("Option B"),
                                    DropdownItem("Option C"),
                                    DropdownItem("Option D")).Var(out var dropdownMulti),
                            Label("Multi Dropdown Selected: ").Inline().SetContent(Defer(dropdownMulti.AsObservableList(), value => TextBlock(string.Join(", ", value.Select(e => e.Text))).AsTask())),
                            Dropdown("Observable Dropdown").Required().Width(200.px())
                               .Items(
                                    DropdownItem("Option A"),
                                    DropdownItem("Option B"),
                                    DropdownItem("Option C"),
                                    DropdownItem("Option D")).Var(out var dropdown),
                            Label("Dropdown Selected: ").Inline().SetContent(Defer(dropdown.AsObservableList(), value => TextBlock(string.Join(", ", value.Select(e => e.Text))).AsTask()))))
                ));
        }

        public dom.HTMLElement Render()
        {
            return _content.Render();
        }
    }
}