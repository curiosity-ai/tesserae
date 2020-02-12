using System;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Label : TextBlock
    {
        private static int _labelForId = 0;

        private readonly HTMLLabelElement _label;
        private readonly HTMLDivElement _content;

        public Label(string text = string.Empty)
        {
            _label = Label(_("tss-fontsize-small tss-fontweight-semibold", text: text));
            _content = Div(_());
            InnerElement = Div(_("tss-label"), _label, _content);
        }

        public Label(IComponent component)
        {
            _label = Label(_("tss-fontsize-small tss-fontweight-semibold"), component.Render());
            _content = Div(_());
            InnerElement = Div(_("tss-label"), _label, _content);
        }


        public override string Text
        {
            get { return _label.innerText; }
            set { _label.innerText = value; }
        }

        public override bool IsRequired
        {
            get { return _label.classList.contains("tss-required"); }
            set
            {
                if (value != IsInvalid)
                {
                    if (value)
                    {
                        _label.classList.add("tss-required");
                    }
                    else
                    {
                        _label.classList.remove("tss-required");
                    }
                }
            }
        }

        public bool IsInline
        {
            get { return _label.style.display == "inline-block"; }
            set
            {
                if (value != IsInline)
                {
                    //TODO make this a css style
                    if (value)
                    {
                        _label.style.display = "inline-block";
                        _content.style.display = "inline-block";
                        InnerElement.style.display = "inline-flex";
                        InnerElement.style.flexWrap = "nowrap";
                        InnerElement.style.alignItems = "center";
                    }
                    else
                    {
                        _label.style.display = "block";
                        _content.style.display = "block";
                        InnerElement.style.display = "";
                        InnerElement.style.flexWrap = "";
                        InnerElement.style.alignItems = "";
                    }
                }
            }
        }

        public IComponent Content
        {
            set
            {
                var id = string.Empty;
                ClearChildren(_content);
                if (value != null)
                {
                    _content.appendChild(value.Render());

                    if ((value as dynamic).InnerElement is HTMLInputElement el)
                    {
                        id = $"elementForLabelN{_labelForId}";
                        _labelForId++;
                        el.id = id;
                    }
                }

                _label.htmlFor = id;
            }
        }
        public Label SetContent( IComponent content)
        {
            Content = content;
            return this;
        }

        public Label Inline()
        {
            IsInline = true;
            return this;
        }

        public Label SetMinLabelWidth(Unit unit, double value)
        {
            _label.style.minWidth = Units.Translate(unit, value);
            return this;
        }
    }
}
