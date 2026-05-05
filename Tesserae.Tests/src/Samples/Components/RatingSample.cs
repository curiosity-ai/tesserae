using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 50, Icon = UIcons.Star)]
    public class RatingSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public RatingSample()
        {
            var selectedValue = new SettableObservable<int>(0);
            var interactiveRating = Rating(5)
                .SetValue(3)
                .OnChange(v =>
                {
                    selectedValue.Value = v;
                    Toast().Information(v == 0 ? "Rating cleared" : $"Rated {v} star{(v == 1 ? "" : "s")}");
                });

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(RatingSample), UIcons.Star, "A component for collecting or displaying star ratings")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("The Rating component lets users express a value judgment on a 1-to-N star scale. It supports interactive selection, read-only display, and custom star counts."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use ratings to collect feedback or signal quality. Provide a way to clear the rating (clicking the selected star again). Show the read-only version when displaying aggregate scores. Keep the star count at 5 unless your domain convention differs."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Interactive Rating"),
                    HStack().AlignItems(ItemAlign.Center).Children(
                        interactiveRating,
                        DeferSync(selectedValue, v => TextBlock(v == 0 ? "Not rated" : $"{v} / 5").ML(12).Small())
                    ),
                    SampleSubTitle("Read-Only Ratings"),
                    VStack().Children(
                        HStack().AlignItems(ItemAlign.Center).Children(
                            Rating(5).SetValue(5).ReadOnly(),
                            TextBlock("5.0 – Excellent").ML(8).Small()
                        ),
                        HStack().AlignItems(ItemAlign.Center).Children(
                            Rating(5).SetValue(3).ReadOnly(),
                            TextBlock("3.0 – Average").ML(8).Small()
                        ),
                        HStack().AlignItems(ItemAlign.Center).Children(
                            Rating(5).SetValue(1).ReadOnly(),
                            TextBlock("1.0 – Poor").ML(8).Small()
                        ),
                        HStack().AlignItems(ItemAlign.Center).Children(
                            Rating(5).SetValue(0).ReadOnly(),
                            TextBlock("Not yet rated").ML(8).Small()
                        )
                    ),
                    SampleSubTitle("Custom Star Count"),
                    VStack().Children(
                        HStack().AlignItems(ItemAlign.Center).Children(
                            TextBlock("3 stars:").Small().W(80.px()),
                            Rating(3).SetValue(2)
                        ),
                        HStack().AlignItems(ItemAlign.Center).Children(
                            TextBlock("10 stars:").Small().W(80.px()),
                            Rating(10).SetValue(7)
                        )
                    )
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
