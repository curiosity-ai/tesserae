using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Media, Order = 40, Icon = UIcons.Copy, Description = "A fanning stack of page thumbnails")]
    public class PagesStackSample : IComponent, ISample
    {
        private static readonly string[] Thumbnails =
        {
            "./assets/img/box-img.svg",
            "./assets/img/curiosity-logo.svg",
            "./assets/img/box-img.svg",
            "./assets/img/curiosity-logo.svg"
        };

        private static readonly string[] Slides =
        {
            "./assets/img/slide-16-9.svg",
            "./assets/img/slide-16-9.svg",
            "./assets/img/slide-16-9.svg",
            "./assets/img/slide-16-9.svg"
        };

        private readonly IComponent _content;

        public PagesStackSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(PagesStackSample), UIcons.Copy, "A fanning stack of page thumbnails, at every size it is drawn at")
               .FlatSection(VStack().WS().Children(Overview()))
               .FlatSection(VStack().WS().Children(Sizes()))
               .FlatSection(VStack().WS().Children(Counts()))
               .FlatSection(VStack().WS().Children(Shapes()))
               .FlatSection(VStack().WS().Children(Interaction()))
               .SeeAlso(typeof(OmniResultSample), typeof(CarouselSample), typeof(MasonrySample));
        }

        private static Card FeatureCard(string title, UIcons icon, string subTitle, string description, params IComponent[] content)
        {
            var stack = VStack().WS().Children(SampleSubTitle(subTitle), TextBlock(description).MB(8));

            foreach (var c in content)
            {
                stack.Add(c);
            }

            return Card(stack).SetTitle(title, icon, Theme.Colors.Blue600);
        }

        private static IComponent Labelled(string label, PagesStack pages)
            => VStack().AlignItems(ItemAlign.Start).Children(pages, TextBlock(label).XSmall().Foreground(Theme.Secondary.Foreground).MT(8));

        private IComponent Overview()
        {
            return FeatureCard("Overview", UIcons.Copy, "A pile of paper you can nudge open",
                "PagesStack draws a document as a few overlapping, slightly rotated pages. Hovering fans them out along a shallow arc; a \"+N\" badge over the top-right counts whatever the stack doesn't draw. Given thumbnail urls it draws them, all cropped to one page size; given only a count it draws blank ruled pages instead — for a document whose thumbnails haven't been generated, or aren't worth generating.",
                TextBlock("The holder is sized to the width the fan needs and the stack is pinned to its right edge, so opening the fan never widens the row it sits in — which is what lets one sit in the rail of every search result.").MB(8),
                HStack().WS().Wrap().Gap(32.px()).PT(8).PB(8).AlignItems(ItemAlign.End).Children(
                    Labelled("Blank pages", PagesStack(4)),
                    Labelled("Thumbnails", PagesStack(Thumbnails).TotalPages(9)),
                    Labelled("Held open", PagesStack(4).Fanned())));
        }

        private IComponent Sizes()
        {
            return FeatureCard("Sizes", UIcons.Resize, "PageSize(width, height), from a chip to a card",
                "PageSize sets the size every page is drawn at, portrait. All pages share one size whatever their thumbnails' aspect ratios are, so the stack reads as one document. The fan step, the rotation and the rail the fan opens into are all measured from that size, so a stack stays itself at any scale.",
                HStack().WS().Wrap().Gap(32.px()).PT(8).PB(8).AlignItems(ItemAlign.End).Children(
                    Labelled("24 × 31 (chip)", PagesStack(4).PageSize(24, 31)),
                    Labelled("36 × 47", PagesStack(4).PageSize(36, 47)),
                    Labelled("48 × 62 (default)", PagesStack(4)),
                    Labelled("64 × 83", PagesStack(4).PageSize(64, 83)),
                    Labelled("96 × 124 (card)", PagesStack(4).PageSize(96, 124))),
                TextBlock("The same sizes with thumbnails in them:").Small().MT(8).MB(8),
                HStack().WS().Wrap().Gap(32.px()).PB(8).AlignItems(ItemAlign.End).Children(
                    Labelled("24 × 31", PagesStack(Thumbnails).PageSize(24, 31).TotalPages(9)),
                    Labelled("36 × 47", PagesStack(Thumbnails).PageSize(36, 47).TotalPages(9)),
                    Labelled("48 × 62", PagesStack(Thumbnails).TotalPages(9)),
                    Labelled("64 × 83", PagesStack(Thumbnails).PageSize(64, 83).TotalPages(9)),
                    Labelled("96 × 124", PagesStack(Thumbnails).PageSize(96, 124).TotalPages(9))));
        }

        private IComponent Counts()
        {
            return FeatureCard("How many pages", UIcons.Layers, "MaxVisible and TotalPages",
                "MaxVisible sets how many pages are drawn before the rest collapse into the \"+N\" badge — five by default, enough to read as a stack and few enough that the fan stays narrow. TotalPages says how many the document actually has, for a stack given fewer thumbnails than that.",
                HStack().WS().Wrap().Gap(32.px()).PT(8).PB(8).AlignItems(ItemAlign.End).Children(
                    Labelled("1 page", PagesStack(1)),
                    Labelled("2 pages", PagesStack(2)),
                    Labelled("3 pages", PagesStack(3)),
                    Labelled("5 pages", PagesStack(5)),
                    Labelled("24 pages", PagesStack(5).TotalPages(24)),
                    Labelled("MaxVisible(3)", PagesStack(12).MaxVisible(3)),
                    Labelled("MaxVisible(8)", PagesStack(12).MaxVisible(8))));
        }

        private IComponent Shapes()
        {
            return FeatureCard("Shape", UIcons.Expand, "MatchThumbnailShape",
                "Pages are drawn portrait until a thumbnail says otherwise: the first one that loads wider than it is tall turns the whole stack landscape, keeping the long side of PageSize and taking the short one from the thumbnail's aspect ratio — so a deck of slides isn't previewed as a pile of A4. Pass false to keep the configured size whatever loads.",
                HStack().WS().Wrap().Gap(32.px()).PT(8).PB(8).AlignItems(ItemAlign.End).Children(
                    Labelled("16:9 slides", PagesStack(Slides).TotalPages(18)),
                    Labelled("Larger slides", PagesStack(Slides).PageSize(72, 94).TotalPages(18)),
                    Labelled("MatchThumbnailShape(false)", PagesStack(Slides).MatchThumbnailShape(false).TotalPages(18))));
        }

        private IComponent Interaction()
        {
            var opened = TextBlock("No page opened yet.").Small().Foreground(Theme.Secondary.Foreground);

            return FeatureCard("Clicking a page", UIcons.CursorFingerClick, "OnPageClick",
                "OnPageClick makes every drawn page clickable and hands the handler the page's index, so opening a document at the page the user pointed at is one call. The click is the page's alone — it does not also count as a click on the row the stack sits in — and each page takes a tab stop of its own and answers Enter and Space.",
                HStack().WS().Wrap().Gap(32.px()).PT(8).PB(8).AlignItems(ItemAlign.End).Children(
                    Labelled("Click a page", PagesStack(5).PageSize(64, 83).OnPageClick(i => opened.Text = $"Opened page {i + 1}.")),
                    Labelled("Thumbnails", PagesStack(Thumbnails).TotalPages(9).OnPageClick(i => opened.Text = $"Opened thumbnail {i + 1}."))),
                opened);
        }

        public HTMLElement Render() => _content.Render();
    }
}
