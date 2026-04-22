using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 22, Icon = UIcons.CreditCard)]
    public class CardPivotSample : IComponent, ISample
    {
        private readonly IComponent content;

        public CardPivotSample()
        {
            content = SectionStack()
               .SampleTitle(nameof(CardPivotSample), UIcons.Apps, "A pivot using cards")
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("A CardPivot is a tabbed interface where the tabs are presented as connected cards with a shared border. It is useful for displaying selectable metrics that control the view below them."))).SetTitle("Overview")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic Usage"),
                    CardPivot()
                        .CardPivot("tab1", () => Metric("Requests", "1.1k").Change(TextBlock("").SmallPlus().Foreground(Theme.Colors.Neutral600)), () => Card(TextBlock("Content for Requests").P(32)))
                        .CardPivot("tab2", () => Metric("Tokens", "196.97k"), () => Card(TextBlock("Content for Tokens").P(32)))
                        .CardPivot("tab3", () => Metric("Cost", "$0.09"), () => Card(TextBlock("Content for Cost").P(32)))
                        .CardPivot("tab4", () => Metric("Errors", "194"), () => Card(TextBlock("Content for Errors").P(32)))
                        .CardPivot("tab5", () => Metric("Cached", "408"), () => Card(TextBlock("Content for Cached").P(32)))
               )).SetTitle("Usage")));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
