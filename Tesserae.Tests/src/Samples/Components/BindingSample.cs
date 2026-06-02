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
