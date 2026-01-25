using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 10, Icon = UIcons.CaretSquareDown)]
    public class PickerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public PickerSample()
        {
            _content = SectionStack()
                   .Title(SampleHeader(nameof(PickerSample)))
                   .Section(Stack().Children(
                        SampleTitle("Overview"),
                        TextBlock("Pickers are used to select one or more items, such as people or tags, from a large list. They provide a search-based interface with suggestions."),
                        TextBlock("This component is highly flexible, allowing for custom item rendering, single or multiple selections, and different suggestion behaviors.")))
                   .Section(Stack().Children(
                        SampleTitle("Best Practices"),
                        TextBlock("Use Pickers when the number of options is too large for a standard Dropdown. Ensure that the items can be easily searched by text. Use clear icons or visual indicators if it helps users identify the correct item quickly. For multiple selections, consider how the selected items will be displayed—either inline or in a separate list. Provide a helpful 'suggestions title' to guide the user when they interact with the picker.")))
                   .Section(Stack().Children(
                        SampleTitle("Usage"),
                        SampleSubTitle("Multi-selection Picker"),
                        TextBlock("Allows selecting multiple tags from the suggestions."),
                        Picker<PickerSampleItem>(suggestionsTitleText: "Suggested Names").Items(GetPickerItems()).MB(32),
                        SampleSubTitle("Single Selection Picker"),
                        TextBlock("Limits selection to only one item at a time."),
                        Picker<PickerSampleItem>(suggestionsTitleText: "Select one", maximumAllowedSelections: 1).Items(GetPickerItems()).MB(32),
                        SampleSubTitle("Custom Rendered Items"),
                        TextBlock("Using icons and complex components for both suggestions and selections."),
                        Picker<PickerSampleItemWithComponents>(suggestionsTitleText: "System Items", renderSelectionsInline: false).Items(GetComponentPickerItems())
                    ));
        }

        public HTMLElement Render() => _content.Render();

        private PickerSampleItem[] GetPickerItems()
        {
            return new[]
            {
                new PickerSampleItem("Bob"),
                new PickerSampleItem("BOB"),
                new PickerSampleItem("Donuts by J Dilla"),
                new PickerSampleItem("Donuts"),
                new PickerSampleItem("Coffee"),
                new PickerSampleItem("Chicken Coop"),
                new PickerSampleItem("Cherry Pie"),
                new PickerSampleItem("Chess"),
                new PickerSampleItem("Cooper")
            };
        }

        private PickerSampleItemWithComponents[] GetComponentPickerItems()
        {
            return new[]
            {
                new PickerSampleItemWithComponents("Bob", UIcons.Bomb),
                new PickerSampleItemWithComponents("BOB", UIcons.BlenderPhone),
                new PickerSampleItemWithComponents("Donuts", UIcons.Carrot),
                new PickerSampleItemWithComponents("Coffee", UIcons.Coffee),
                new PickerSampleItemWithComponents("Chess", UIcons.Chess),
                new PickerSampleItemWithComponents("Cooper", UIcons.Interrogation)
            };
        }
    }

    public class PickerSampleItem : IPickerItem
    {
        public PickerSampleItem(string name) => Name = name;
        public string Name { get; }
        public bool IsSelected { get; set; }
        public IComponent Render() => TextBlock(Name);
    }

    public class PickerSampleItemWithComponents : IPickerItem
    {
        private readonly UIcons _icon;
        public PickerSampleItemWithComponents(string name, UIcons icon) { Name = name; _icon = icon; }
        public string Name { get; }
        public bool IsSelected { get; set; }
        public IComponent Render() => HStack().AlignContent(ItemAlign.Center).Children(Icon(_icon).MinWidth(16.px()), TextBlock(Name).ML(8));
    }
}
