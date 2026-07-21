using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 10, Icon = UIcons.Palette)]
    public class ThemeBuilderSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ThemeBuilderSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ThemeBuilderSample), UIcons.Palette, "Build a custom theme with one fluent call")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("UI.Theme.Build() exposes every color CSS variable the toolkit understands, in one fluent API. Each setter takes a light value and a dark value, and Apply() injects a single <style> element that overrides the defaults from tss.common.css."),
                    TextBlock("Use this when you need to fully re-brand the toolkit. For lightweight tweaks (just the primary color, or just the background) the existing UI.Theme.SetPrimary / SetBackground helpers are sharper tools."),
                    TextBlock("Call UI.Theme.ResetBuild() to drop the injected theme and return to the defaults."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Try a few presets"),
                    HStack().Children(
                        Button("Ocean").OnClick(() => Theme.Build()
                            .Primary(Color.FromString("#0078d4"), Color.FromString("#2899f5"))
                            .Link   (Color.FromString("#0078d4"), Color.FromString("#55b3fb"))
                            .DefaultBackground       (Color.FromString("#eaf3fb"), Color.FromString("#0b1320"))
                            .DefaultBackgroundHover  (Color.FromString("#d6e6f4"), Color.FromString("#101d33"))
                            .DefaultBackgroundActive (Color.FromString("#c4dbef"), Color.FromString("#162644"))
                            .DefaultForeground       (Color.FromString("#0b3559"), Color.FromString("#e6f1fb"))
                            .DefaultBorder           (Color.FromString("#bcd6ea"), Color.FromString("#1f3252"))
                            .Apply()).MR(8),
                        Button("Forest").OnClick(() => Theme.Build()
                            .Primary(Color.FromString("#16a34a"), Color.FromString("#22c55e"))
                            .Link   (Color.FromString("#15803d"), Color.FromString("#22c55e"))
                            .DefaultBackground       (Color.FromString("#eaf7ee"), Color.FromString("#08160d"))
                            .DefaultBackgroundHover  (Color.FromString("#d4ebda"), Color.FromString("#0f2418"))
                            .DefaultBackgroundActive (Color.FromString("#bbdec5"), Color.FromString("#173324"))
                            .DefaultForeground       (Color.FromString("#143922"), Color.FromString("#dcfce7"))
                            .DefaultBorder           (Color.FromString("#bdddc7"), Color.FromString("#1d3a25"))
                            .Apply()).MR(8),
                        Button("Sunset").OnClick(() => Theme.Build()
                            .Primary(Color.FromString("#ea580c"), Color.FromString("#fb923c"))
                            .Link   (Color.FromString("#c2410c"), Color.FromString("#fb923c"))
                            .DefaultBackground       (Color.FromString("#fff2e3"), Color.FromString("#1c1209"))
                            .DefaultBackgroundHover  (Color.FromString("#fbe4cb"), Color.FromString("#2a1c10"))
                            .DefaultBackgroundActive (Color.FromString("#f6d2ad"), Color.FromString("#3a2716"))
                            .DefaultForeground       (Color.FromString("#5b2410"), Color.FromString("#fde2c5"))
                            .DefaultBorder           (Color.FromString("#f1cba0"), Color.FromString("#3b271a"))
                            .Apply()).MR(8),
                        Button("Grape").OnClick(() => Theme.Build()
                            .Primary(Color.FromString("#7c3aed"), Color.FromString("#a78bfa"))
                            .Link   (Color.FromString("#6d28d9"), Color.FromString("#c4b5fd"))
                            .DefaultBackground       (Color.FromString("#f3edff"), Color.FromString("#14091f"))
                            .DefaultBackgroundHover  (Color.FromString("#e6daff"), Color.FromString("#1e1230"))
                            .DefaultBackgroundActive (Color.FromString("#d6c4ff"), Color.FromString("#2c1d44"))
                            .DefaultForeground       (Color.FromString("#2e1065"), Color.FromString("#ede9fe"))
                            .DefaultBorder           (Color.FromString("#d4c2f7"), Color.FromString("#2d1c4a"))
                            .Apply()).MR(8),
                        Button("Reset").Danger().OnClick(() => Theme.ResetBuild()).MR(8),
                        Button("Toggle Dark").OnClick(() => Theme.IsDark = !Theme.IsDark)
                    ),
                    SampleSubTitle("Sample surfaces"),
                    Card(VStack().Children(
                        TextBlock("This card and its children re-skin when you click a preset above. Watch the page background and surrounding chrome too — the presets also tint the default surface so the change is visible across the whole app."),
                        HStack().Children(
                            Button("Primary").Primary().MR(8),
                            Button("Success").Success().MR(8),
                            Button("Danger").Danger().MR(8),
                            Button("Default")
                        ).MT(8),
                        Label("A textbox").SetContent(TextBox()).MT(8),
                        Label("A link").SetContent(Link("https://curiosity.ai", "Visit curiosity.ai")).MT(8))
                    )
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
