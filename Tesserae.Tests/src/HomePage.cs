using System.Collections.Generic;
using System.Linq;
using Tesserae;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Tests
{
    /// <summary>
    /// What the gallery shows before a sample is picked: what the toolkit is, where its code lives,
    /// and every sample as a card under its category.
    /// <para>
    /// The categories and their order come from <see cref="SampleGroup.InDisplayOrder"/> — the same
    /// list the sidebar groups by — so the page and the sidebar always read the same way, and a new
    /// category shows up here without anything being edited twice.
    /// </para>
    /// </summary>
    internal static class HomePage
    {
        private const string TesseraeRepository  = "https://github.com/curiosity-ai/tesserae";
        private const string TransposeRepository = "https://github.com/curiosity-ai/transpose";
        private const string Documentation       = "https://docs.curiosity.ai/tesserae/";

        /// <summary>
        /// Builds the home page for the given samples. Clicking a card routes to that sample through
        /// the same <c>#/view/{name}</c> route the sidebar and the "See also" links use.
        /// </summary>
        public static IComponent Create(IEnumerable<Sample> samples)
        {
            var all = samples.ToList();

            var page = VStack().S().ScrollY().Children(Intro(all));

            // One section per category, in sidebar order. A category with no samples is skipped
            // rather than drawn empty, so adding a constant before its first sample costs nothing.
            foreach (var category in SampleGroup.InDisplayOrder)
            {
                var inCategory = all.Where(s => s.Group == category.Name)
                                    .OrderBy(s => s.Order).ThenBy(s => s.Name.ToLower()).ToList();

                if (inCategory.Count == 0) continue;

                page.Add(Section(category, inCategory));
            }

            // Anything whose group isn't a listed category — a typo, or a sample added before its
            // category was. Better visible under a heading than silently missing from the page.
            var uncategorized = all.Where(s => SampleGroup.Describe(s.Group) is null)
                                   .OrderBy(s => s.Name.ToLower()).ToList();

            if (uncategorized.Count > 0)
            {
                page.Add(Section(new SampleGroup.Category("Others", "Samples whose group is not one of the categories above.", UIcons.Circle, "#94a3b8"), uncategorized));
            }

            return page.P(32).PB(48);
        }

        private static IComponent Intro(IReadOnlyList<Sample> all)
        {
            var categories = SampleGroup.InDisplayOrder.Count(c => all.Any(s => s.Group == c.Name));

            var links = HStack().Wrap().Gap(8.px()).PT(8).Children(
                Button("Tesserae on GitHub",  href: TesseraeRepository).SetIcon(UIcons.BrandsGithub).Primary().OpenInNewTab(),
                Button("Transpose on GitHub", href: TransposeRepository).SetIcon(UIcons.BrandsGithub).OpenInNewTab(),
                Button("Documentation",       href: Documentation).SetIcon(UIcons.Books).OpenInNewTab());

            return Card(VStack().WS().Children(
                    HStack().AlignItemsCenter().Gap(12.px()).Children(
                        Icon(UIcons.Apps, size: TextSize.XLarge, color: Theme.Primary.Background),
                        TextBlock("Tesserae").XLarge().Bold(),
                        Badge($"{all.Count} samples").Pill().Neutral(),
                        Badge($"{categories} categories").Pill().Neutral()),
                    TextBlock("A UI toolkit for building web applications in C#.").Medium().PT(4),
                    // Capped rather than stretched: prose set across a 1600px window is unreadable,
                    // while the card grids below want every pixel of it.
                    TextBlock("Components are created through the static UI class and configured with fluent extension methods. The C# is compiled to JavaScript by the Transpose compiler and runs in the browser — there is no JavaScript framework underneath it.")
                       .Small().Foreground(Theme.Secondary.Foreground).PT(8).MaxWidth(820.px()),
                    TextBlock("Every component has a page here showing what it renders, and the sidebar command on each entry opens the C# that produced it. Pick a category below, or search the sidebar by name.")
                       .Small().Foreground(Theme.Secondary.Foreground).PT(4).MaxWidth(820.px()),
                    links))
               .WS();
        }

        private static IComponent Section(SampleGroup.Category category, IReadOnlyList<Sample> samples)
        {
            // auto-fill rather than auto-fit: a category holding four samples should keep its cards
            // the same width as one holding seventeen, instead of stretching them across the row.
            var cards = Grid(new UnitSize("repeat(auto-fill, minmax(min(240px, 100%), 1fr))")).Gap(8.px()).WS();

            foreach (var sample in samples)
            {
                cards.Add(SampleCard(sample, category));
            }

            return VStack().WS().PT(28).Children(
                HStack().AlignItemsCenter().Gap(8.px()).Children(
                    Icon(category.Icon, size: TextSize.Medium, color: category.Tint),
                    TextBlock(category.Name).Large().SemiBold(),
                    Badge(samples.Count.ToString()).Pill().Neutral()),
                TextBlock(category.Blurb).Small().Foreground(Theme.Secondary.Foreground).PT(2).PB(10),
                cards);
        }

        private static IComponent SampleCard(Sample sample, SampleGroup.Category category) =>
            ContextCard(sample.Name, sample.Icon)
               .IconTint(category.Tint)
               .WithChevron()
               .Selectable(false)
               .Tooltip($"Open the {sample.Name} sample")
               .OnClick((_, __) => Router.Navigate($"#/view/{sample.Name}"))
               .WS();
    }
}
