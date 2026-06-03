using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 1, Icon = UIcons.Link)]
    public class BindingSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public BindingSample()
        {
            // Single source of truth: one observable, several views.
            var name    = SettableObservable.Of("Ada");
            var enabled = SettableObservable.Of(true);

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
                            Button("Clear").OnClick(() => name.Value        = "")
                        ),
                        SampleSubTitle("Toggle + CheckBox sharing one bool observable"),
                        TextBlock("Toggle and CheckBox both bound to the same observable stay in sync."),
                        HStack().Children(
                            Toggle().Bind(enabled),
                            CheckBox("Enabled").Bind(enabled),
                            DeferSync(enabled, e => TextBlock(e ? "ON" : "OFF").SemiBold().ML(8))
                        )
                    )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}