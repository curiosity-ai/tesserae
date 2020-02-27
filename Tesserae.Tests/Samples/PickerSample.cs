using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class PickerSample : IComponent
    {
        private readonly IComponent _content;

        public PickerSample()
        {
            _content =
                SectionStack()
                    .Title(
                        TextBlock("Picker")
                            .XLarge()
                            .Bold())
                    .Section(
                        Stack()
                            .Children(
                                SamplesHelper.SampleTitle("Overview"),
                                TextBlock("Pickers are used to pick recipients.")))
                    .Section(
                        Stack()
                            .Width(40.percent())
                            .Children(
                                SamplesHelper.SampleTitle("Usage"),
                                TextBlock("Picker with text suggestions and tag-like selections")
                                    .Medium()
                                    .PaddingBottom(16.px()),
                                Picker<PickerSampleItem>(suggestionsTitleText: "Suggested Tags").WithItems(GetPickerItems())
                                    .PaddingBottom(32.px()),
                                TextBlock("Picker with icon and text suggestions and component based selections")
                                    .Medium()
                                    .PaddingBottom(16.px()),
                                Picker<PickerSampleItemWithComponents>(suggestionsTitleText: "Suggested Items", renderSelectionsInline: false).WithItems(GetComponentPickerItems())));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }

        private PickerSampleItem[] GetPickerItems()
        {
            return new []
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
            return new []
            {
                new PickerSampleItemWithComponents("Bob", LineAwesome.Bomb),
                new PickerSampleItemWithComponents("BOB", LineAwesome.Blender),
                new PickerSampleItemWithComponents("Donuts by J Dilla", LineAwesome.Carrot),
                new PickerSampleItemWithComponents("Donuts", LineAwesome.CarBattery),
                new PickerSampleItemWithComponents("Coffee", LineAwesome.Coffee),
                new PickerSampleItemWithComponents("Chicken Coop", LineAwesome.Hamburger),
                new PickerSampleItemWithComponents("Cherry Pie", LineAwesome.ChartPie),
                new PickerSampleItemWithComponents("Chess", LineAwesome.Chess),
                new PickerSampleItemWithComponents("Cooper", LineAwesome.QuestionCircle)
            };
        }
    }
}
