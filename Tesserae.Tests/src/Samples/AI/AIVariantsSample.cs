using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.AI, Order = 10, Icon = UIcons.Sparkles)]
    public class AIVariantsSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public AIVariantsSample()
        {
            _content = SectionStack().Secondary()
                .SampleTitle(typeof(AIVariantsSample), UIcons.Sparkles, "One purple-to-blue language for what a model made, and for the actions that ask it for more")
                .FlatSection(VStack().WS().Children(Overview()))
                .FlatSection(VStack().WS().Children(BestPractices()))
                .FlatSection(VStack().WS().Children(TheGradient()))
                .FlatSection(VStack().WS().Children(EveryVariant()))
                .FlatSection(VStack().WS().Children(Waiting()))
                .FlatSection(VStack().WS().Children(PutTogether()))
                .SeeAlso(typeof(ChatSample), typeof(ToolCallSample), typeof(PlanSample), typeof(GradientsSample), typeof(CardSample));
        }

        private static Card FeatureCard(string title, UIcons icon, string description, params IComponent[] content)
        {
            var stack = VStack().WS().Children(TextBlock(description).MB(8));

            foreach (var c in content)
            {
                stack.Add(c);
            }

            return Card(stack).SetTitle(title, icon, Theme.Colors.Purple600);
        }

        private IComponent Overview()
        {
            return Card(VStack().WS().Children(
                TextBlock("A model's output and the buttons that ask for it are not a new kind of component - they are the components you already have, saying where the content came from. So every one of them says it the same way: AI() puts a quiet purple-to-blue gradient on the thing, and where a glyph is wanted it is Sparkles."),
                TextBlock("The variant exists on Card, TextBlock, Icon, Button, InlineLabel and Skeleton, and on five more that turned out to need it for the same reason - Badge (and Tag and Chip) through BadgeTone.AI, Spinner, ProgressIndicator, IconToggle and SegmentedPivot. Every one of them is the component's own stylesheet with the colour swapped: same geometry, same states, same size.").MT(8),
                TextBlock("The gradient is the same two colours everywhere, at the strength the surface can carry: filled between the mid weights on a button or a badge, a tenth of that as a tint on a card, and painted into the letters or the glyph where the thing is type. Nothing here animates at rest - the only movement is the Skeleton's shimmer and the ProgressIndicator's sweep, which were already moving.").MT(8))
            ).SetTitle("Overview");
        }

        private IComponent BestPractices()
        {
            return Card(VStack().WS().Children(
                HStack().WS().Wrap().Children(
                    VStack().Width(45.percent()).Children(
                        SampleSubTitle("Do"),
                        SampleDo("Use it to mean \"a model made this\" or \"this asks a model for something\" - not as a decorative accent."),
                        SampleDo("Keep one filled AI button per surface, the way you keep one Primary; the rest go subtle."),
                        SampleDo("Reach for AISurface() on a paragraph and AI() on its title - gradient words are for short strings."),
                        SampleDo("Let a component that already carries a source (an avatar, a logo) keep its own mark rather than taking Sparkles.")
                    ),
                    VStack().Width(45.percent()).Children(
                        SampleSubTitle("Don't"),
                        SampleDont("Don't put the gradient on a whole page or a nav - it stops meaning anything once everything has it."),
                        SampleDont("Don't stack an AI card inside an AI card; the tints add up and the inner one loses its edge."),
                        SampleDont("Don't animate a resting AI surface. Movement means work is happening."),
                        SampleDont("Don't combine AI() with Primary(), Success() or Danger() on the same button - the tone is the AI one now.")
                    ))
            )).SetTitle("Best Practices");
        }

        private IComponent TheGradient()
        {
            IComponent Swatch(string name, string cssVar, string note) =>
                HStack().WS().NoWrap().AlignItemsCenter().Gap(12.px()).MB(6).Children(
                    Stack().W(120).H(28).Background($"var({cssVar})").Rounded(BorderRadius.Small),
                    VStack().Grow().Children(
                        TextBlock(name).SemiBold(),
                        TextBlock(note).XSmall().Secondary()));

            return FeatureCard("The gradient, and the tokens behind it", UIcons.Palette,
                "Every variant reads from the same handful of CSS variables, declared once for the light theme and again for dark, so an app that wants a different AI hue overrides four values rather than a stylesheet. Theme.Gradients.AI is still there for the louder three-stop version; these are the quiet ones the variants use.",
                Swatch("--tss-ai-gradient",        "--tss-ai-gradient",        "The filled form: buttons, badges, a progress bar."),
                Swatch("--tss-ai-gradient-text",   "--tss-ai-gradient-text",   "Darker in light mode, lighter in dark - words have to stay readable."),
                Swatch("--tss-ai-surface",         "--tss-ai-surface",         "The tint, layered over whatever background the component already had."),
                Swatch("--tss-ai-surface-strong",  "--tss-ai-surface-strong",  "One step up: a card's header, a hovered tint."),
                TextBlock("The tints are translucent on purpose - a tinted card still sits on the page's own background instead of punching a hole in it, which is what keeps it working in both themes and on a coloured surface.").XSmall().Secondary().MT(8));
        }

        private IComponent EveryVariant()
        {
            IComponent Row(string label, IComponent normal, IComponent ai) =>
                HStack().WS().Wrap().AlignItemsCenter().Gap(16.px()).MB(12).Children(
                    TextBlock(label).SemiBold().W(150),
                    HStack().AlignItemsCenter().Gap(8.px()).W(220).Children(normal),
                    HStack().AlignItemsCenter().Gap(8.px()).Grow().Children(ai));

            return FeatureCard("Every variant, next to what it varies from", UIcons.Layers,
                "The default on the left, AI() on the right. The geometry is identical in each pair: the variant changes colour, never size or spacing, so dropping one into a laid-out page moves nothing.",
                HStack().WS().Gap(16.px()).MB(8).Children(
                    TextBlock("Component").XSmall().Secondary().W(150),
                    TextBlock("Default").XSmall().Secondary().W(220),
                    TextBlock("AI variant").XSmall().Secondary().Grow()),
                Row("Icon",
                    Icon(UIcons.Sparkles, size: TextSize.Large),
                    HStack().AlignItemsCenter().Gap(10.px()).Children(
                        AIIcon(size: TextSize.Large),
                        Icon(UIcons.Comment, size: TextSize.Large).AI(),
                        Icon(UIcons.ChartPieAlt, size: TextSize.Large).AI())),
                Row("TextBlock",
                    TextBlock("Quarterly summary").MediumPlus().SemiBold(),
                    TextBlock("Quarterly summary").MediumPlus().SemiBold().AI()),
                Row("Button",
                    Button("Summarise").NoMargin(),
                    HStack().AlignItemsCenter().Gap(8.px()).Children(
                        Button("Summarise").AI().NoMargin().OnClick(() => Toast().Information("The filled form: one per surface")),
                        Button("Rewrite").AISubtle().NoMargin().OnClick(() => Toast().Information("The quiet form, for the second and third")))),
                Row("InlineLabel",
                    InlineLabel("2.4 MB").SetIcon(UIcons.Folder),
                    HStack().AlignItemsCenter().Gap(8.px()).Children(
                        InlineLabel("Summarised by AI").AI(),
                        InlineLabel("94% confidence").AI(withSparklesIcon: false))),
                Row("Badge / Tag / Chip",
                    HStack().AlignItemsCenter().Gap(8.px()).Children(
                        Badge("Draft").Primary(),
                        Tag("Metadata").Outline()),
                    HStack().AlignItemsCenter().Gap(8.px()).Children(
                        AIBadge(),
                        Badge("Generated").AI(),
                        Tag("Suggested").AI().Outline().Pill())),
                Row("Skeleton",
                    Skeleton().W(180).H(12),
                    Skeleton().W(180).H(12).AI()),
                Row("Spinner",
                    HStack().AlignItemsCenter().Gap(12.px()).Children(
                        Spinner("Loading"),
                        Spinner().Progress(70).Medium()),
                    HStack().AlignItemsCenter().Gap(12.px()).Children(
                        Spinner("Thinking").AI(),
                        Spinner().AI().Progress(70).Medium())),
                Row("ProgressIndicator",
                    ProgressIndicator().Progress(45).W(180),
                    ProgressIndicator().Progress(45).AI().W(180)),
                Row("IconToggle",
                    IconToggle(
                        IconToggleItem(UIcons.Bolt,  "Fast",     "fast"),
                        IconToggleItem(UIcons.Scale, "Balanced", "balanced"),
                        IconToggleItem(UIcons.Brain, "Thorough", "thorough")).Compact(),
                    IconToggle(
                        IconToggleItem(UIcons.Bolt,  "Fast",     "fast"),
                        IconToggleItem(UIcons.Scale, "Balanced", "balanced"),
                        IconToggleItem(UIcons.Brain, "Thorough", "thorough")).AI().Compact()),
                Row("Card",
                    Card(TextBlock("A plain card.")).W(200),
                    Card(TextBlock("The same card, marked as the model's.")).AI().W(240)),
                TextBlock("SegmentedPivot is the same control one level up - tabs rather than a value - so it gets a row of its own rather than a cell:").Small().Secondary().MT(8).MB(8),
                HStack().WS().Wrap().Gap(24.px()).Children(
                    VStack().Width(45.percent()).MinWidth(280.px()).Children(
                        TextBlock("Default").XSmall().Secondary().MB(4),
                        SegmentedPivot()
                            .SegmentedPivot("d1", SegmentTitle("Answer"),    () => TextBlock("The answer."))
                            .SegmentedPivot("d2", SegmentTitle("Sources"),   () => TextBlock("The sources."))
                            .SegmentedPivot("d3", SegmentTitle("Reasoning"), () => TextBlock("The reasoning."))),
                    VStack().Width(45.percent()).MinWidth(280.px()).Children(
                        TextBlock("AI variant").XSmall().Secondary().MB(4),
                        SegmentedPivot().AI()
                            .SegmentedPivot("a1", SegmentTitle("Answer"),    () => TextBlock("The answer.").AISurface())
                            .SegmentedPivot("a2", SegmentTitle("Sources"),   () => InlineLabel("12 documents").AI())
                            .SegmentedPivot("a3", SegmentTitle("Reasoning"), () => TextBlock("The reasoning.").AISurface()))));
        }

        private IComponent Waiting()
        {
            return FeatureCard("Waiting on a model", UIcons.Hourglass,
                "A tinted Skeleton says the wait is for something being generated rather than for something being fetched - same shape, same shimmer, same speed, only the colour differs. Beside it, the two states of an AI answer: the placeholder it draws while the model works, and the answer it becomes.",
                HStack().WS().Wrap().Gap(24.px()).PT(8).Children(
                    VStack().Width(45.percent()).MinWidth(280.px()).Children(
                        TextBlock("Generating").SemiBold().MB(8),
                        Card(VStack().WS().Children(
                            HStack().AlignItemsCenter().Gap(8.px()).MB(12).Children(
                                Spinner("Reading 12 documents").AI()),
                            Skeleton().WS().H(12).AI(),
                            Skeleton().W(90.percent()).H(12).MT(8).AI(),
                            Skeleton().W(70.percent()).H(12).MT(8).AI(),
                            ProgressIndicator().Progress(35).AI().WS().MT(16))).AI()),
                    VStack().Width(45.percent()).MinWidth(280.px()).Children(
                        TextBlock("Answered").SemiBold().MB(8),
                        Card(VStack().WS().Children(
                            HStack().WS().AlignItemsCenter().Gap(8.px()).MB(8).Children(
                                AIIcon(),
                                TextBlock("Summary").SemiBold().AI(),
                                AIBadge().ML(4)),
                            TextBlock("Brake sensor calibration failed on line 3 in eleven of the last fourteen runs. Every failure follows a mount torque below 12 Nm, which the procedure calls for explicitly.").AISurface(),
                            HStack().WS().Wrap().Gap(6.px()).MT(12).Children(
                                InlineLabel("12 sources").AI(),
                                InlineLabel("94% confidence").AI(withSparklesIcon: false),
                                InlineLabel("Apr 12, 2024").SetIcon(UIcons.Clock))))))
            );
        }

        private IComponent PutTogether()
        {
            var body = VStack().WS().Children(
                TextBlock("Line 3's brake sensor failures all share one cause: the mount torque is under spec. Tightening to 12 Nm before calibration clears the fault in every run where it was recorded correctly.").AISurface(),
                HStack().WS().Wrap().Gap(6.px()).MT(12).Children(
                    InlineLabel("Drawn from 12 documents").AI(),
                    InlineLabel("BRK-SEN-447").SetColor(Theme.Colors.Blue600),
                    InlineLabel("Marie Lang").SetIcon(UIcons.User)),
                HStack().WS().AlignItemsCenter().Gap(8.px()).MT(16).Children(
                    ProgressIndicator().Progress(3, 4).AI().Grow(),
                    TextBlock("3 of 4 checks").XSmall().Secondary()));

            var card = Card(body)
                .SetTitle(HStack().WS().AlignItemsCenter().Gap(8.px()).Children(
                    AIIcon(),
                    TextBlock("What went wrong on line 3").SemiBold().AI(),
                    AIBadge("AI answer").ML(4)))
                .SetFooter(HStack().WS().AlignItemsCenter().Gap(8.px()).Children(
                    Button("Ask a follow-up").AI().NoMargin().OnClick(() => Toast().Information("The one filled AI action on this card")),
                    Button("Show sources").AISubtle(withSparklesIcon: false).SetIcon(UIcons.Books).NoMargin().OnClick(() => Toast().Information("Opening the 12 sources")),
                    Button("Dismiss").NoMargin().OnClick(() => Toast().Information("A plain action stays plain"))))
                .AI();

            return FeatureCard("All of it on one surface", UIcons.Sparkles,
                "A single answer card using nine of the eleven variants at once - a tinted card with a tinted header and footer, a gradient title led by the Sparkles glyph, a badge, generated prose on its own panel, the facts behind it as labels, a progress bar, and three buttons in descending loudness. Nothing on it moves.",
                card.MaxWidth(620.px()).MT(8));
        }

        public HTMLElement Render() => _content.Render();
    }
}
