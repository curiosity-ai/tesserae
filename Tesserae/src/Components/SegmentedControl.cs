using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.SegmentedControl")]
    public class SegmentedControl : ComponentBase<SegmentedControl, HTMLDivElement>
    {
        private readonly Stack _stack;
        private readonly List<Button> _buttons = new List<Button>();
        private string _selectedValue;

        public event ComponentEventHandler<SegmentedControl, string> onChange;

        public SegmentedControl()
        {
            _stack = HStack().Class("tss-segmentedcontrol");
            InnerElement = Div(_("tss-segmentedcontrol-wrapper", styles: s =>
            {
                s.display = "inline-flex";
                s.backgroundColor = Theme.Secondary.Background;
                s.borderRadius = "4px";
                s.padding = "2px";
            }), _stack.Render());
        }

        public SegmentedControl AddOption(string value, string label = null, UIcons? icon = null)
        {
            label = label ?? value;
            var button = Button(label).NoBorder().NoBackground();
            button.Render().style.padding = "4px 12px";

            if (icon.HasValue)
            {
                button.SetIcon(icon.Value);
            }

            button.OnClick((b, e) =>
            {
                Select(value);
            });

            button.Render().dataset["value"] = value;
            _buttons.Add(button);
            _stack.Children(button);

            if (_buttons.Count == 1 && _selectedValue == null)
            {
                Select(value);
            }

            return this;
        }

        public SegmentedControl Select(string value)
        {
            if (_selectedValue == value) return this;

            _selectedValue = value;

            foreach (var button in _buttons)
            {
                var isSelected = (string)button.Render().dataset["value"] == value;
                if (isSelected)
                {
                    button.Render().style.backgroundColor = Theme.Primary.Background;
                    button.Render().style.color = Theme.Primary.Foreground;
                    button.Render().style.borderRadius = "2px";
                    button.Render().style.boxShadow = "0 1px 3px rgba(0,0,0,0.12), 0 1px 2px rgba(0,0,0,0.24)";
                }
                else
                {
                    button.Render().style.backgroundColor = "transparent";
                    button.Render().style.color = Theme.Primary.Background;
                    button.Render().style.boxShadow = "none";
                }
            }

            onChange?.Invoke(this, value);

            return this;
        }

        public SegmentedControl OnChange(ComponentEventHandler<SegmentedControl, string> handler)
        {
            onChange += handler;
            return this;
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}