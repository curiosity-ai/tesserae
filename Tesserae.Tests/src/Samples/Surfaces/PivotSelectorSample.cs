using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using System.Collections.Generic;
using System.Linq;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 20, Icon = UIcons.TableLayout)]
    public class PivotSelectorSample : IComponent, ISample
    {
        private readonly IComponent content;

        public PivotSelectorSample()
        {
            content = SectionStack()
               .Title(SampleHeader(nameof(PivotSelectorSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("The PivotSelector component is similar to the Pivot component but uses a Dropdown for navigation between tabs. This is useful when you have many tabs or when horizontal space is limited."),
                    TextBlock("It also allows adding custom buttons next to the dropdown selector.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic PivotSelector"),
                    PivotSelector()
                        .Pivot("tab1", "Tab 1", () => Card(TextBlock("Content for Tab 1").P(32)))
                        .Pivot("tab2", "Tab 2", () => Card(TextBlock("Content for Tab 2").P(32)))
                        .Pivot("tab3", "Tab 3", () => Card(TextBlock("Content for Tab 3").P(32))),
                    SampleSubTitle("PivotSelector with custom buttons").PT(16),
                    PivotSelector()
                        .SetCommands(
                            Button().SetIcon(UIcons.Add).NoBorder().NoBackground().OnClick(() => alert("Add clicked")),
                            Button().SetIcon(UIcons.Settings).NoBorder().NoBackground().OnClick(() => alert("Settings clicked"))
                        )
                        .Pivot("tab1", () => Button("Tab 1").SetIcon(UIcons.Rocket), () => Card(TextBlock("Content for Tab 1").P(32)))
                        .Pivot("tab2", () => Button("Tab 2").SetIcon(UIcons.Car),    () => Card(TextBlock("Content for Tab 2").P(32))),
                    SampleSubTitle("PivotSelector with large number of tabs").PT(16),
                    PivotSelector()
                        .Pivot(Enumerable.Range(1, 20).Select(i => ($"tab{i}", $"Tab {i}", (Func<IComponent>)(() => Card(TextBlock($"Content for Tab {i}").P(32))))).ToArray())
                ));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
