using System;
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
            Action hidePicker = () => { };
            var    btn        = Button("Show Picker in Tippy");

            btn.OnClick(() =>
            {
                hidePicker();

                var tippyPicker = Picker<PickerSampleItem>()
                   .Items(GetPickerItems())
                   .OnItemSelected((_, __) => hidePicker());


                var tooltipContent = VStack().Gap(12.px())
                   .Children(
                        TextBlock("Pick a value from the tooltip."),
                        tippyPicker);


                Tippy.ShowFor(
                    hostComponent: btn,
                    tooltip: tooltipContent,
                    hide: out hidePicker,
                    placement: TooltipPlacement.BottomStart,
                    maxWidth: 420,
                    hideOnClick: false,
                    onClickOutside: (_, e) =>
                    {
                        if (IsWithinClass(e.target as HTMLElement, "tss-layer-picker-suggestions"))
                        {
                            return;
                        }

                        hidePicker();
                    }
                );
            });


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
                    Picker<PickerSampleItem>().Items(GetPickerItems()).MB(32),
                    SampleSubTitle("Single Selection Picker"),
                    TextBlock("Limits selection to only one item at a time."),
                    Picker<PickerSampleItem>(maximumAllowedSelections: 1).Items(GetPickerItems()).MB(32),
                    SampleSubTitle("Custom Rendered Items"),
                    TextBlock("Using icons and complex components for both suggestions and selections."),
                    Picker<PickerSampleItemWithComponents>().Items(GetComponentPickerItems()),
                    SampleSubTitle("Picker in Tippy"),
                    TextBlock("Opens the picker inside an interactive Tippy tooltip when the button is clicked."),
                    btn,
                    Empty().H(1000)
                ));
        }

        public HTMLElement Render() => _content.Render();

        private static bool IsWithinClass(HTMLElement element, string className)
        {
            while (element is object)
            {
                if (element.classList?.contains(className) == true)
                {
                    return true;
                }

                element = element.parentElement;
            }

            return false;
        }

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
                new PickerSampleItemWithComponents("Bob",    HStack().AlignContent(ItemAlign.Center).Children(Icon(UIcons.Bomb).MinWidth(16.px()),          TextBlock("Bob").ML(8))),
                new PickerSampleItemWithComponents("BOB",    HStack().AlignContent(ItemAlign.Center).Children(Icon(UIcons.BlenderPhone).MinWidth(16.px()),  TextBlock("BOB").ML(8))),
                new PickerSampleItemWithComponents("Donuts", HStack().AlignContent(ItemAlign.Center).Children(Icon(UIcons.Carrot).MinWidth(16.px()),        TextBlock("Donuts").ML(8))),
                new PickerSampleItemWithComponents("Coffee", HStack().AlignContent(ItemAlign.Center).Children(Icon(UIcons.Coffee).MinWidth(16.px()),        TextBlock("Coffee").ML(8))),
                new PickerSampleItemWithComponents("Chess",  HStack().AlignContent(ItemAlign.Center).Children(Icon(UIcons.Chess).MinWidth(16.px()),         TextBlock("Chess").ML(8))),
                new PickerSampleItemWithComponents("Cooper", HStack().AlignContent(ItemAlign.Center).Children(Icon(UIcons.Interrogation).MinWidth(16.px()), TextBlock("Cooper").ML(8)))
            };
        }
    }

    public class PickerSampleComponentItem : IPickerItem
    {
        public PickerSampleComponentItem(string name, IComponent component)
        {
            Name      = name;
            Component = component;
        }
        public IComponent Component  { get; }
        public string     Name       { get; }
        public bool       IsSelected { get; set; }
        public IComponent Render()   => Component;
    }

    public class PickerSampleItem : IPickerItem
    {
        public PickerSampleItem(string name) => Name = name;
        public string     Name       { get; }
        public bool       IsSelected { get; set; }
        public IComponent Render()   => TextBlock(Name);
    }

    public class PickerSampleItemWithComponents : IPickerItem
    {
        private readonly IComponent _component;
        public PickerSampleItemWithComponents(string name, IComponent component)
        {
            Name       = name;
            _component = component;
        }
        public string     Name       { get; }
        public bool       IsSelected { get; set; }
        public IComponent Render()   => _component;
    }
}