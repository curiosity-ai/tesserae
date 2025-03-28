using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using System.Collections.Generic;
using System.Linq;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 20, Icon = UIcons.TableLayout)]
    public class PivotSample : IComponent, ISample
    {
        private readonly IComponent content;

        public PivotSample()
        {
            content = SectionStack()
               .Title(SampleHeader(nameof(PivotSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("TODO"),
                    TextBlock("Examples of experiences that use Panels").MediumPlus()))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    HStack().Children(
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Do"),
                            SampleDo("TODO")
                        ),
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Don't"),
                            SampleDont("TODO")
                        )
                    )))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Normal Style"),
                    GetPivot(),
                    SampleSubTitle("Justified Style"),
                    GetPivot().Justified(),
                    SampleSubTitle("Centered Style"),
                    GetPivot().Centered(),
                    SampleSubTitle("Cached vs. Not Cached Tabs"),
                    Pivot().Pivot("tab1", PivotTitle("Cached"),     () => TextBlock(DateTimeOffset.UtcNow.ToString()).Regular(), cached: true)
                           .Pivot("tab2", PivotTitle("Not Cached"), () => TextBlock(DateTimeOffset.UtcNow.ToString()).Regular(), cached: false),
                    SampleSubTitle("Cached vs. Not Cached Tabs"),
                    SampleSubTitle("Scroll with limited height"),
                    Pivot().MaxHeight(500.px())
                       .Pivot("tab1", PivotTitle("5 Items"),   () => ItemsList(GetSomeItems(5)).PB(16),   cached: true)
                       .Pivot("tab2", PivotTitle("10 Items"),  () => ItemsList(GetSomeItems(20)).PB(16),  cached: true)
                       .Pivot("tab3", PivotTitle("50 Items"),  () => ItemsList(GetSomeItems(50)).PB(16),  cached: true)
                       .Pivot("tab4", PivotTitle("100 Items"), () => ItemsList(GetSomeItems(100)).PB(16), cached: true),
                    SampleSubTitle("Tab Overflow"),
                    Pivot().MaxHeight(500.px()).MaxWidth(300.px())
                       .Pivot("tab1", PivotTitle("Tab 1"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true)
                       .Pivot("tab2", PivotTitle("Tab 2"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true)
                       .Pivot("tab3", PivotTitle("Tab 3"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true)
                       .Pivot("tab4", PivotTitle("Tab 4"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true)
                       .Pivot("tab5", PivotTitle("Tab 5"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true)
                       .Pivot("tab6", PivotTitle("Tab 6"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true)
                       .Pivot("tab7", PivotTitle("Tab 7"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true)
                       .Pivot("tab8", PivotTitle("Tab 8"), () => ItemsList(GetSomeItems(5)).PB(16), cached: true)
                ));
        }

        private Pivot GetPivot()
        {
            return Pivot().Pivot("first-tab",  PivotTitle("First Tab"),  () => TextBlock("First Tab"))
                          .Pivot("second-tab", PivotTitle("Second Tab"), () => TextBlock("Second Tab"))
                          .Pivot("third-tab",  PivotTitle("Third Tab"),  () => TextBlock("Third Tab"));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }

        private IComponent[] GetSomeItems(int count)
        {
            return Enumerable
               .Range(1, count)
               .Select(number => Card(TextBlock($"Lorem Ipsum {number}").NonSelectable()).MinWidth(200.px()))
               .ToArray();
        }
    }
}