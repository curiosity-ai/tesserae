using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 1, Icon = UIcons.Link)]
    public class BindingSample : IComponent, ISample
    {
        private readonly IComponent _content;
        private readonly Modal      _boundModal;
        private readonly Panel      _boundPanel;

        public BindingSample()
        {
            // Single source of truth: one observable, several views.
            var name        = SettableObservable.Of("Ada");
            var enabled     = SettableObservable.Of(true);
            var expanded    = SettableObservable.Of(false);
            var modalOpen   = SettableObservable.Of(false);
            var panelOpen   = SettableObservable.Of(false);
            var volume      = SettableObservable.Of(40);
            var rating      = SettableObservable.Of(0);
            var step        = SettableObservable.Of(0);
            var page        = SettableObservable.Of(1);
            var slide       = SettableObservable.Of(0);
            var birthday    = SettableObservable.Of<DateTime?>(new DateTime(1990, 1, 1));
            var meeting     = SettableObservable.Of<DateTime?>(null);
            var month       = SettableObservable.Of<(int year, int month)?>(null);
            var time        = SettableObservable.Of<DateTimeOffset?>(null);
            var trip        = SettableObservable.Of<(DateTime? from, DateTime? to)>((null, null));
            var pivotTab    = SettableObservable.Of("alpha");
            var sidebarOpen = SettableObservable.Of(true);

            var demoModal = Modal("Bound modal")
                .Content(Stack().Children(
                    TextBlock("This modal's visibility is bound to a SettableObservable<bool>."),
                    TextBlock("Closing it (via the X, Esc, or light-dismiss) flips the observable back to false.")))
                .LightDismiss()
                .Bind(modalOpen);

            var demoPanel = Panel("Bound panel")
                .Content(Stack().Children(
                    TextBlock("This panel's visibility is bound to a SettableObservable<bool>.")))
                .LightDismiss()
                .Bind(panelOpen);

            _content = SectionStack().Secondary()
                .SampleTitle(typeof(BindingSample), UIcons.Link, "Two-way binding against a SettableObservable")
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("`.Bind(observable)` wires a component to a SettableObservable in both directions: user input flows into the observable, and programmatic changes to the observable update the component. The observable is the single source of truth.")
                    )).SetTitle("Overview")))
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Two text boxes bound to the same observable"),
                        TextBlock("Typing in either text box updates the other and the live read-out below."),
                        TextBox().Bind(name).Width(240.px()),
                        TextBox().Bind(name).Width(240.px()),
                        DeferSync(name, n => TextBlock($"Hello, {n}!").SemiBold()),

                        SampleSubTitle("Programmatic mutation"),
                        TextBlock("Clicking these buttons mutates the observable directly — both text boxes update without any extra wiring."),
                        HStack().Children(
                            Button("Set to Grace").OnClick(() => name.Value = "Grace"),
                            Button("Set to Linus").OnClick(() => name.Value = "Linus"),
                            Button("Clear").OnClick(() => name.Value = "")
                        ),

                        SampleSubTitle("Toggle + CheckBox sharing one bool observable"),
                        TextBlock("Toggle and CheckBox both bound to the same observable stay in sync."),
                        HStack().Children(
                            Toggle().Bind(enabled),
                            CheckBox("Enabled").Bind(enabled),
                            DeferSync(enabled, e => TextBlock(e ? "ON" : "OFF").SemiBold().ML(8))
                        ),

                        SampleSubTitle("Expander bound to a bool observable"),
                        TextBlock("Clicking the expander header flips the observable; the buttons below flip it back. Two expanders share the same observable so they always agree."),
                        Expander("Bound section", TextBlock("Hidden until expanded.")).Bind(expanded),
                        Expander("Mirror", TextBlock("Same observable — opens and closes in lock-step.")).Bind(expanded),
                        HStack().Children(
                            Button("Open").OnClick(() => expanded.Value  = true),
                            Button("Close").OnClick(() => expanded.Value = false),
                            DeferSync(expanded, e => TextBlock(e ? "OPEN" : "CLOSED").SemiBold().ML(8))
                        ),

                        SampleSubTitle("Modal / Panel bound to a bool observable"),
                        TextBlock("Setting the observable to true shows the surface; closing it (X / Esc / light-dismiss) flips the observable back to false."),
                        HStack().Children(
                            Button("Open modal").OnClick(() => modalOpen.Value = true),
                            DeferSync(modalOpen, o => TextBlock(o ? "modal: OPEN" : "modal: closed").SemiBold().ML(8)),
                            Button("Open panel").OnClick(() => panelOpen.Value = true),
                            DeferSync(panelOpen, o => TextBlock(o ? "panel: OPEN" : "panel: closed").SemiBold().ML(8))
                        ),

                        SampleSubTitle("Slider + NumberPicker sharing one int observable"),
                        TextBlock("Both controls bind to the same SettableObservable<int>. Dragging the slider updates the number, typing a number moves the slider."),
                        HStack().Children(
                            Slider(val: 40, min: 0, max: 100, step: 1).Bind(volume),
                            NumberPicker(40).Bind(volume).Width(80.px()),
                            DeferSync(volume, v => TextBlock($"volume = {v}").SemiBold().ML(8))
                        ),

                        SampleSubTitle("Rating bound to an int observable"),
                        TextBlock("Clicking stars updates the observable; the buttons below drive it programmatically."),
                        HStack().Children(
                            Rating().Bind(rating),
                            HStack().Children(
                                Button("0").OnClick(() => rating.Value = 0),
                                Button("3").OnClick(() => rating.Value = 3),
                                Button("5").OnClick(() => rating.Value = 5)
                            ),
                            DeferSync(rating, r => TextBlock($"{r} / 5").SemiBold().ML(8))
                        ),

                        SampleSubTitle("Stepper bound to an int observable"),
                        TextBlock("The current step is the bound value. Clicking the Next/Back buttons inside the stepper updates the observable; the buttons below jump directly."),
                        Stepper(
                            new StepperStep("First",  TextBlock("Step 1 content")),
                            new StepperStep("Second", TextBlock("Step 2 content")),
                            new StepperStep("Third",  TextBlock("Step 3 content"))
                        ).Bind(step),
                        HStack().Children(
                            Button("Go to 0").OnClick(() => step.Value = 0),
                            Button("Go to 2").OnClick(() => step.Value = 2),
                            DeferSync(step, s => TextBlock($"step = {s}").SemiBold().ML(8))
                        ),

                        SampleSubTitle("Pagination bound to an int observable"),
                        TextBlock("Clicking a page number updates the observable; the buttons below jump directly."),
                        Pagination(totalItems: 100, pageSize: 10, currentPage: 1).Bind(page),
                        HStack().Children(
                            Button("First").OnClick(() => page.Value = 1),
                            Button("Page 5").OnClick(() => page.Value = 5),
                            Button("Last").OnClick(() => page.Value  = 10),
                            DeferSync(page, p => TextBlock($"page = {p}").SemiBold().ML(8))
                        ),

                        SampleSubTitle("Carousel bound to an int observable"),
                        TextBlock("The active slide index is the bound value. Use the arrows or click the dots; the buttons below jump directly."),
                        Carousel(
                            Card(TextBlock("Slide 0")).WS(),
                            Card(TextBlock("Slide 1")).WS(),
                            Card(TextBlock("Slide 2")).WS()
                        ).Bind(slide),
                        HStack().Children(
                            Button("0").OnClick(() => slide.Value = 0),
                            Button("1").OnClick(() => slide.Value = 1),
                            Button("2").OnClick(() => slide.Value = 2),
                            DeferSync(slide, s => TextBlock($"slide = {s}").SemiBold().ML(8))
                        ),

                        SampleSubTitle("DatePicker bound to a DateTime? observable"),
                        TextBlock("Two date pickers share the same observable; the buttons below mutate it."),
                        HStack().Children(
                            DatePicker().Bind(birthday),
                            DatePicker().Bind(birthday)
                        ),
                        HStack().Children(
                            Button("Today").OnClick(() => birthday.Value = DateTime.Today),
                            Button("Y2K").OnClick(()   => birthday.Value = new DateTime(2000, 1, 1)),
                            Button("Clear").OnClick(() => birthday.Value = null),
                            DeferSync(birthday, b => TextBlock(b.HasValue ? b.Value.ToString("yyyy-MM-dd") : "(empty)").SemiBold().ML(8))
                        ),

                        SampleSubTitle("DateTimePicker / MonthPicker / TimePicker — all the same shape"),
                        HStack().Children(
                            DateTimePicker().Bind(meeting),
                            DeferSync(meeting, m => TextBlock(m.HasValue ? m.Value.ToString("yyyy-MM-dd HH:mm") : "(no meeting)").SemiBold().ML(8))
                        ),
                        HStack().Children(
                            new MonthPicker(null).Bind(month),
                            DeferSync(month, m => TextBlock(m.HasValue ? $"{m.Value.year}-{m.Value.month:D2}" : "(no month)").SemiBold().ML(8))
                        ),
                        HStack().Children(
                            new TimePicker().Bind(time),
                            DeferSync(time, t => TextBlock(t.HasValue ? t.Value.ToString("HH:mm:ss") : "(no time)").SemiBold().ML(8))
                        ),

                        SampleSubTitle("DateRangePicker bound to a (DateTime?, DateTime?) observable"),
                        TextBlock("The range picker keeps its two halves in sync; the buttons below drive both at once."),
                        DateRangePicker().Bind(trip),
                        HStack().Children(
                            Button("This week").OnClick(() => trip.Value = (DateTime.Today, DateTime.Today.AddDays(7))),
                            Button("Clear").OnClick(()    => trip.Value = (null, null)),
                            DeferSync(trip, r => TextBlock(
                                (r.from.HasValue ? r.from.Value.ToString("yyyy-MM-dd") : "?") + " → " +
                                (r.to.HasValue   ? r.to.Value.ToString("yyyy-MM-dd")   : "?")).SemiBold().ML(8))
                        ),

                        SampleSubTitle("Pivot bound to a string observable"),
                        TextBlock("Clicking a tab updates the observable; the buttons below jump directly. Two pivots bound to the same observable stay in lock-step."),
                        Pivot()
                            .Pivot("alpha", PivotTitle("Alpha"), () => TextBlock("Alpha content").P(8))
                            .Pivot("beta",  PivotTitle("Beta"),  () => TextBlock("Beta content").P(8))
                            .Pivot("gamma", PivotTitle("Gamma"), () => TextBlock("Gamma content").P(8))
                            .Bind(pivotTab),
                        Pivot()
                            .Pivot("alpha", PivotTitle("Alpha"), () => TextBlock("Alpha mirror").P(8))
                            .Pivot("beta",  PivotTitle("Beta"),  () => TextBlock("Beta mirror").P(8))
                            .Pivot("gamma", PivotTitle("Gamma"), () => TextBlock("Gamma mirror").P(8))
                            .Bind(pivotTab),
                        HStack().Children(
                            Button("alpha").OnClick(() => pivotTab.Value = "alpha"),
                            Button("beta").OnClick(()  => pivotTab.Value = "beta"),
                            Button("gamma").OnClick(() => pivotTab.Value = "gamma"),
                            DeferSync(pivotTab, t => TextBlock($"selected = {t}").SemiBold().ML(8))
                        )
                    )).SetTitle("Usage")));

            // Keep the bound modal/panel instances alive — they mount themselves to document.body on Show()
            // but their .Bind() subscriptions need to outlive this constructor.
            _boundModal = demoModal;
            _boundPanel = demoPanel;
        }

        public HTMLElement Render() => _content.Render();
    }
}
