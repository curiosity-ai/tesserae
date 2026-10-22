using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Utilities, Order = 70, Icon = UIcons.MagicWand, Description = "Shake, pop, flash, shimmer, reveal and swipe on any component")]
    public class MicroInteractionsSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public MicroInteractionsSample()
        {
            var random = new SampleRandom(70);

            // Shake: a refused action and an invalid submit.
            var locked = Button("Restricted action").SetIcon(UIcons.Lock);
            locked.OnClick(() => locked.Shake());

            var email  = TextBox().SetPlaceholder("name@example.com").W(240);
            var submit = Button("Subscribe").Primary();
            submit.OnClick(() =>
            {
                if (!email.Text.Contains("@"))
                {
                    email.Error     = "Check the email address";
                    email.IsInvalid = true;
                    email.Shake();
                }
                else
                {
                    email.IsInvalid = false;
                    Toast().Success("Subscribed");
                }
            });

            // Pop: a reaction.
            var star = Button().SetIcon(UIcons.Star).Tooltip("Star");
            var starred = false;
            star.OnClick(() =>
            {
                starred = !starred;
                star.SetIcon(UIcons.Star, weight: starred ? UIconsWeight.Solid : UIconsWeight.Regular, color: starred ? "#f5b301" : "");
                star.Pop();
            });

            // Nudge: a dismissal that is not allowed.
            var nudgeCard = Card(VStack().Children(TextBlock("Unsaved changes").SemiBold(), TextBlock("Click outside this card: it nudges, because it cannot be dismissed that way.").Small().Secondary())).W(320);
            var nudgeArea = VStack().WS().H(140).AlignItemsCenter().JustifyContent(ItemJustify.Center)
               .Style(s => { s.border = "1px dashed var(--tss-default-border-color)"; s.borderRadius = "8px"; s.cursor = "pointer"; })
               .Children(nudgeCard);
            nudgeArea.Render().addEventListener("click", e =>
            {
                if (!nudgeCard.Render().contains(e.target.As<Node>())) nudgeCard.Nudge();
            });

            // SpinOnce: a refresh.
            var refreshed = TextBlock("Updated just now").Secondary().Small();
            var refresh   = Button("Refresh").SetIcon(UIcons.Refresh);
            refresh.OnClick(() =>
            {
                refresh.SpinOnce();
                refreshed.Text = $"Updated, {random.Next(3, 40)} new items";
            });

            // Flash: insert, pin.
            var list = VStack().WS();
            var count = 3;
            for (var i = 1; i <= 3; i++) list.Add(ListRow($"Item {i}", list));

            var add = Button("Add item").SetIcon(UIcons.Plus).OnClick(() =>
            {
                count++;
                var row = ListRow($"Item {count}", list);
                list.Add(row);
                row.Flash();
            });

            // Shimmer.
            var shimmerCard = Card(VStack().Children(TextBlock("Quarterly report").SemiBold(), TextBlock("Revenue, churn and usage for Q3.").Small().Secondary())).W(320);
            var shimmering  = false;
            var shimmerBtn  = Button("Toggle loading");
            shimmerBtn.OnClick(() => { shimmering = !shimmering; shimmerCard.Shimmer(shimmering); });

            // Reveal on hover.
            var revealCards = HStack().Wrap().Gap(12.px()).Children(RevealCard("design-tokens.json"), RevealCard("release-notes.md"));

            // Swipe to dismiss.
            var deck = VStack().WS().Gap(8.px());
            Action refill = null;
            refill = () =>
            {
                deck.Clear();
                foreach (var title in new[] { "New comment on Roadmap", "Build #4821 passed", "Invoice ready" })
                {
                    deck.Add(SwipeCard(title));
                }
            };
            refill();

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(MicroInteractionsSample), UIcons.MagicWand, "Small feedback animations for any component")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("These extension methods add the small motions that tell a user what just happened, to any IComponent - a toolkit component or one of your own. The one-shots (Shake, Pop, Nudge, SpinOnce, Flash) are called from an event handler at the moment the feedback is due, restart if they are already playing, and leave nothing behind. The others switch a behaviour on: PressFeedback, Shimmer, RevealOnHoverOf and SwipeToDismiss."),
                        TextBlock("Under reduced motion a shake turns into a brief red ring and the rest stand still, so no information is carried by motion alone. The components built on the same pieces are HoldButton, ConfirmButton, CopyButton, AnimatedNumber, FlipCard, CompareSlider and CodeInput.").PT(8))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleDo("Pair a shake with a message that says why: the shake only draws the eye there."),
                        SampleDo("Keep a visible control beside a swipe: swiping is a shortcut, not the only way."),
                        SampleDont("Animate for decoration. Each of these answers something the user did."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Shake - a refused action, an invalid submit"),
                        HStack().Wrap().AlignItems(ItemAlign.Start).Gap(8.px()).Children(locked, email, submit),
                        SampleSubTitle("Pop - a reaction"),
                        star,
                        SampleSubTitle("Nudge - a dismissal that is not allowed"),
                        nudgeArea,
                        SampleSubTitle("SpinOnce - a refresh"),
                        HStack().AlignItemsCenter().Gap(8.px()).Children(refresh, refreshed),
                        SampleSubTitle("Flash - where the new row landed"),
                        add, list.PT(8),
                        SampleSubTitle("PressFeedback - a slight press-in"),
                        HStack().Gap(8.px()).Children(
                            Card(TextBlock("Press me").NoWrap()).PressFeedback().Style(s => s.cursor = "pointer"),
                            Button("Me too").PressFeedback()),
                        SampleSubTitle("Shimmer - content on its way, in place"),
                        HStack().AlignItemsCenter().Gap(16.px()).Children(shimmerCard, shimmerBtn),
                        SampleSubTitle("RevealOnHoverOf - actions that appear on hover or focus"),
                        revealCards,
                        SampleSubTitle("SwipeToDismiss - drag a card sideways"),
                        deck.MaxWidth(420.px()),
                        Button("Bring them back").OnClick(() => refill()).MT(8))).SetTitle("Usage")))
               .SeeAlso(typeof(GesturesSample), typeof(HoldButtonSample), typeof(ConfirmButtonSample), typeof(CopyButtonSample), typeof(AnimatedNumberSample), typeof(FlipCardSample));
        }

        private static IComponent ListRow(string text, Stack list)
        {
            var row = HStack().WS().AlignItemsCenter().JustifyContent(ItemJustify.Between).PL(12).PR(4).H(40).Style(s => s.borderRadius = "6px");
            row.Children(TextBlock(text), Button().SetIcon(UIcons.Thumbtack).NoBackground().Tooltip("Pin to top").OnClick(() =>
            {
                var el = row.Render();
                el.parentElement.insertBefore(el, el.parentElement.firstElementChild);
                row.Flash();
            }));
            return row;
        }

        private static IComponent RevealCard(string name)
        {
            var card = Card(VStack().Children(
                TextBlock(name).SemiBold(),
                TextBlock("Hover, focus or tap").Small().Secondary())).W(240);

            var actions = HStack().Gap(4.px()).PT(8).Children(
                Button().SetIcon(UIcons.Download).Tooltip("Download"),
                Button().SetIcon(UIcons.Share).Tooltip("Share"),
                Button().SetIcon(UIcons.Trash).Tooltip("Delete")).RevealOnHoverOf(card);

            return VStack().Children(card.SetFooter(actions));
        }

        private static IComponent SwipeCard(string title)
        {
            var card = Card(HStack().WS().AlignItemsCenter().JustifyContent(ItemJustify.Between).Children(
                VStack().Children(TextBlock(title).SemiBold(), TextBlock("Swipe left or right to dismiss").Small().Secondary()),
                Button().SetIcon(UIcons.Cross).NoBackground().Tooltip("Dismiss"))).WS();

            card.SwipeToDismiss(() => Toast().Information("Dismissed: " + title));

            var close = card.Render().querySelector("button").As<HTMLElement>();
            close.addEventListener("click", e =>
            {
                e.stopPropagation();
                card.DismissWithSwipe(() => Toast().Information("Dismissed: " + title));
            });

            return card;
        }

        public HTMLElement Render() => _content.Render();
    }
}
