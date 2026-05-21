using static H5.Core.dom;
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
                    TextBlock("UI.Theme.Build() exposes every color CSS variable the toolkit understands, in one fluent API. Each setter takes a light value and a dark value, and Apply() injects a single <style> element that ovrrides defaults."),
                    TextBlock("Use this when you need to fully re-brand the toolkit. For lightweight tweaks (just the primary color, or just the background) the existing UI.Theme.SetPrimary / SetBackground helpers are sharper tools."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Try a few presets"),
                    HStack().Children(
                        Button("Ocean").OnClick(() => Theme.Build()
                            .Primary(Color.FromString("#0078d4"), Color.FromString("#2899f5"))
                            .Link   (Color.FromString("#0078d4"), Color.FromString("#55b3fb"))
                            .DefaultBackground(Color.FromString("#ffffff"), Color.FromString("#0f172a"))
                            .DefaultForeground(Color.FromString("#1f2937"), Color.FromString("#f1f5f9"))
                            .Apply()).MR(8),
                        Button("Forest").OnClick(() => Theme.Build()
                            .Primary(Color.FromString("#16a34a"), Color.FromString("#22c55e"))
                            .Link   (Color.FromString("#16a34a"), Color.FromString("#22c55e"))
                            .DefaultBackground(Color.FromString("#f8fafc"), Color.FromString("#0a0f0a"))
                            .DefaultForeground(Color.FromString("#1f2937"), Color.FromString("#dcfce7"))
                            .Apply()).MR(8),
                        Button("Sunset").OnClick(() => Theme.Build()
                            .Primary(Color.FromString("#ea580c"), Color.FromString("#fb923c"))
                            .Link   (Color.FromString("#ea580c"), Color.FromString("#fb923c"))
                            .DefaultBackground(Color.FromString("#fff7ed"), Color.FromString("#1c1917"))
                            .DefaultForeground(Color.FromString("#1c1917"), Color.FromString("#fef3c7"))
                            .Apply()).MR(8),
                        Button("Toggle Dark").OnClick(() => Theme.IsDark = !Theme.IsDark)
                    ),
                    SampleSubTitle("Sample surfaces"),
                    Card(VStack().Children(
                        TextBlock("This card and its children re-skin when you click a preset above."),
                        HStack().Children(
                            Button("Primary").Primary().MR(8),
                            Button("Success").Success().MR(8),
                            Button("Danger").Danger().MR(8),
                            Button("Default")
                        ).MT(8),
                        Label("A textbox").SetContent(TextBox()).MT(8))
                    )
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
