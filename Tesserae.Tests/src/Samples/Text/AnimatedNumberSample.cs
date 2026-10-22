using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Text, Order = 90, Icon = UIcons.Calculator, Description = "A number that counts or ticks to each new value")]
    public class AnimatedNumberSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public AnimatedNumberSample()
        {
            var random = new SampleRandom(90);

            // A total that is recalculated from a selection.
            var total   = AnimatedNumber(100).Prefix("$").Format("N0");
            var options = new[] { (Name: "Priority support", Price: 40), (Name: "Extra storage", Price: 25), (Name: "Audit log", Price: 60) };
            var picks   = VStack();
            var sum     = 100;

            foreach (var option in options)
            {
                var o = option;
                picks.Add(CheckBox($"{o.Name} (+${o.Price})").OnChange((cb, _) =>
                {
                    sum += cb.IsChecked ? o.Price : -o.Price;
                    total.SetValue(sum);
                }));
            }

            // A like count that ticks, and pops.
            var liked = false;
            var likes = AnimatedNumber(128).Tick();
            var heart = Button().SetIcon(UIcons.Heart);
            heart.ReplaceContent(HStack().AlignItemsCenter().Gap(6.px()).Children(Icon(UIcons.Heart), likes));
            heart.OnClick(() =>
            {
                liked = !liked;
                likes.SetValue(likes.Value + (liked ? 1 : -1));
                heart.Pop();
                if (liked) heart.Primary(); else heart.Render().classList.remove("tss-btn-primary");
            });

            // A percentage driven from a slider.
            var percent = AnimatedNumber(40, "0").Suffix("%").Duration(400);
            var slider  = Slider(40, 0, 100, 1).WS().OnInput((s, _) => percent.SetValue(s.Value));

            // A metric that is refreshed with new figures.
            var visitors = AnimatedNumber(12480).Duration(900);

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(AnimatedNumberSample), UIcons.Calculator, "Numbers that move when they change")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("AnimatedNumber shows a number that animates to each new value, so a change is seen rather than missed: a total recalculated after a selection, a percentage, a like count. Count() (the default) runs through the values in between, fast and then settling; Tick() slides the new value in from below when it went up and from above when it went down."),
                        TextBlock("Digits are tabular, so the text does not shift sideways while it counts. Under reduced motion the new value simply appears. It is bindable: Bind(observable) follows an IObservable<double>.").PT(8))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("A recalculated total"),
                        HStack().WS().AlignItemsCenter().JustifyContent(ItemJustify.Between).Children(picks, VStack().AlignItems(ItemAlign.End).Children(TextBlock("Total per month").Secondary().Small(), total.Style(s => { s.fontSize = "28px"; s.fontWeight = "600"; }))),
                        SampleSubTitle("A reaction count that ticks"),
                        heart,
                        SampleSubTitle("A percentage"),
                        HStack().WS().AlignItemsCenter().Gap(16.px()).Children(slider.Grow(), percent.Style(s => { s.fontSize = "20px"; s.minWidth = "56px"; s.textAlign = "right"; })),
                        SampleSubTitle("A metric refreshed with new data"),
                        HStack().AlignItemsCenter().Gap(16.px()).Children(
                            visitors.Style(s => { s.fontSize = "32px"; s.fontWeight = "700"; }),
                            Button("Refresh").SetIcon(UIcons.Refresh).OnClick((b, _) =>
                            {
                                b.SpinOnce();
                                visitors.SetValue(random.Next(8000, 20000));
                            })))).SetTitle("Usage")))
               .SeeAlso(typeof(MetricSample), typeof(ProgressIndicatorSample), typeof(SliderSample), typeof(MicroInteractionsSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
