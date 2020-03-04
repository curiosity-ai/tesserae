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
            get => _label.innerText;
            set => _label.innerText = value;
        }

        public override bool IsRequired
        {
            get => _label.classList.contains("tss-required");
            set
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

        public bool IsInline
        {
            get => InnerElement.classList.contains("inline");
            set
            {
                if (value)
                {
                    InnerElement.classList.add("inline");
                }
                else
                {
                    InnerElement.classList.add("remove");
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

        public Label SetMinLabelWidth(UnitSize unitSize)
        {
            _label.style.minWidth = unitSize.ToString();
            return this;
        }
    }
}
