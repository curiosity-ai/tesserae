﻿using System;
using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Label : TextBlock
    {
        private static int _LabelForId = 0;

        private HTMLLabelElement _Label;
        private HTMLDivElement _Content;

        public override string Text
        {
            get { return _Label.innerText; }
            set { _Label.innerText = value; }
        }

        public override bool IsRequired
        {
            get { return _Label.classList.contains("mss-required"); }
            set
            {
                if (value != IsInvalid)
                {
                    if (value)
                    {
                        _Label.classList.add("mss-required");
                    }
                    else
                    {
                        _Label.classList.remove("mss-required");
                    }
                }
            }
        }

        public bool IsInline
        {
            get { return _Label.style.display == "inline-block"; }
            set
            {
                if (value != IsInline)
                {
                    if (value)
                    {
                        _Label.style.display = "inline-block";
                        _Content.style.display = "inline-block";
                    }
                    else
                    {
                        _Label.style.display = "block";
                        _Content.style.display = "block";
                    }
                }
            }
        }

        public IComponent Content
        {
            set
            {
                var id = string.Empty;
                ClearChildren(_Content);
                if (value != null)
                {
                    _Content.appendChild(value.Render());

                    var el = (value as dynamic).InnerElement as HTMLInputElement;
                    if (el != null)
                    {
                        id = $"elementForLabelN{_LabelForId}";
                        _LabelForId++;
                        el.id = id;
                    }
                }

                _Label.htmlFor = id;
            }
        }

        public Label(string text = string.Empty)
        {
            _Label = Label(_(text: text));
            _Content = Div(_());
            InnerElement = Div(_("mss-label mss-fontSize-small mss-fontWeight-semibold"), _Label, _Content);
        }
    }

    public static class LabelExtensions
    {
        public static Label Content(this Label label, IComponent content)
        {
            label.Content = content;
            return label;
        }

        public static Label Inline(this Label label)
        {
            label.IsInline = true;
            return label;
        }
    }
}