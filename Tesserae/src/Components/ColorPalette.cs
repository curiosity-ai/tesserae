using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A grid of named colour swatches for letting users pick from a predefined palette.
    /// </summary>
    [H5.Name("tss.ColorPalette")]
    public sealed class ColorPalette : ComponentBase<ColorPalette, HTMLElement>
    {
        private readonly List<SwatchEntry>     _swatches      = new List<SwatchEntry>();
        private          HTMLElement           _selectedSwatch;
        private          string               _value;
        private          bool                 _withCustom;
        private readonly HTMLInputElement     _customInput;
        private          Action<string>       _valueChanged;

        public ColorPalette()
        {
            _customInput = document.createElement("input") as HTMLInputElement;
            _customInput.type      = "color";
            _customInput.className = "tss-colorpalette-custom-input";
            _customInput.setAttribute("aria-label", "Custom color");
            _customInput.addEventListener("input",  _ => SelectColor(_customInput.value, null));
            _customInput.addEventListener("change", _ => SelectColor(_customInput.value, null));

            InnerElement = Div(_("tss-colorpalette", role: "radiogroup", ariaLabel: "Color palette"));
        }

        /// <summary>Gets the currently selected hex color string.</summary>
        public string Value => _value;

        /// <summary>Adds a swatch with a label and hex color.</summary>
        public ColorPalette AddSwatch(string label, string hexColor)
        {
            var swatch = new SwatchEntry(label, hexColor);
            _swatches.Add(swatch);
            var el = CreateSwatchElement(swatch);
            InnerElement.appendChild(el);
            return this;
        }

        /// <summary>Adds multiple swatches from an array of (label, hexColor) tuples.</summary>
        public ColorPalette Swatches(params (string label, string color)[] entries)
        {
            foreach (var (label, color) in entries)
                AddSwatch(label, color);
            return this;
        }

        /// <summary>Selects a color by hex value programmatically.</summary>
        public ColorPalette SetValue(string hexColor)
        {
            SelectColor(hexColor, null);
            return this;
        }

        /// <summary>Adds a "custom…" swatch that opens the browser color picker.</summary>
        public ColorPalette WithCustomColor(bool value = true)
        {
            _withCustom = value;

            if (_withCustom && !InnerElement.contains(_customInput))
            {
                var wrapper = Span(_("tss-colorpalette-swatch tss-colorpalette-custom"));
                wrapper.setAttribute("role",       "radio");
                wrapper.setAttribute("aria-label", "Custom color");
                wrapper.setAttribute("tabindex",   "0");
                wrapper.style.background = "conic-gradient(red, yellow, lime, cyan, blue, magenta, red)";
                wrapper.Tooltip("Custom color");
                wrapper.addEventListener("click",   _ => _customInput.click());
                wrapper.addEventListener("keydown", e =>
                {
                    var kb = e.As<KeyboardEvent>();
                    if (kb.key == "Enter" || kb.key == " ") { _customInput.click(); e.preventDefault(); }
                });
                InnerElement.appendChild(wrapper);
                InnerElement.appendChild(_customInput);
            }
            else if (!_withCustom && InnerElement.contains(_customInput))
            {
                InnerElement.removeChild(_customInput);
            }

            return this;
        }

        /// <summary>Registers a callback for when the selected color changes.</summary>
        public ColorPalette OnChange(Action<string> onChange)
        {
            _valueChanged += onChange;
            return this;
        }

        public override HTMLElement Render() => InnerElement;

        private HTMLElement CreateSwatchElement(SwatchEntry entry)
        {
            var el = Span(_("tss-colorpalette-swatch"));
            el.setAttribute("role",       "radio");
            el.setAttribute("aria-label", entry.Label);
            el.setAttribute("tabindex",   "0");
            el.style.background = entry.Color;
            el.Tooltip(entry.Label);

            el.addEventListener("click",   _ => SelectColor(entry.Color, el));
            el.addEventListener("keydown", e =>
            {
                var kb = e.As<KeyboardEvent>();
                if (kb.key == "Enter" || kb.key == " ") { SelectColor(entry.Color, el); e.preventDefault(); }
            });

            entry.Element = el;
            return el;
        }

        private void SelectColor(string color, HTMLElement sourceEl)
        {
            _value = color;

            if (_selectedSwatch != null)
            {
                _selectedSwatch.classList.remove("tss-selected");
                _selectedSwatch.removeAttribute("aria-checked");
            }

            _selectedSwatch = sourceEl;

            if (sourceEl != null)
            {
                sourceEl.classList.add("tss-selected");
                sourceEl.setAttribute("aria-checked", "true");
            }

            foreach (var s in _swatches)
            {
                if (s.Element != null && s.Element != sourceEl)
                {
                    s.Element.classList.remove("tss-selected");
                    s.Element.removeAttribute("aria-checked");
                }
            }

            _valueChanged?.Invoke(_value);
        }

        private class SwatchEntry
        {
            public string       Label   { get; }
            public string       Color   { get; }
            public HTMLElement  Element { get; set; }

            public SwatchEntry(string label, string color)
            {
                Label = label;
                Color = color;
            }
        }
    }
}
