using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.CommandBar")]
    public sealed class CommandBar : ComponentBase<CommandBar, HTMLElement>
    {
        private readonly HTMLElement _primarySection;
        private readonly HTMLElement _farSection;

        public CommandBar(params IComponent[] items)
        {
            _primarySection = Div(_("tss-commandbar-section"));
            _farSection     = Div(_("tss-commandbar-section tss-commandbar-far"));

            InnerElement = Div(_("tss-commandbar"), _primarySection, _farSection);

            AddItems(items);
        }

        public CommandBar AddItem(IComponent item)
        {
            if (item != null)
            {
                _primarySection.appendChild(item.Render());
            }
            return this;
        }

        public CommandBar AddItems(params IComponent[] items)
        {
            if (items == null)
            {
                return this;
            }

            foreach (var item in items)
            {
                AddItem(item);
            }

            return this;
        }

        public CommandBar AddFarItem(IComponent item)
        {
            if (item != null)
            {
                _farSection.appendChild(item.Render());
            }
            return this;
        }

        public CommandBar AddFarItems(params IComponent[] items)
        {
            if (items == null)
            {
                return this;
            }

            foreach (var item in items)
            {
                AddFarItem(item);
            }

            return this;
        }

        public CommandBar Items(params IComponent[] items) => AddItems(items);
        public CommandBar FarItems(params IComponent[] items) => AddFarItems(items);

        public override HTMLElement Render() => InnerElement;
    }

    [H5.Name("tss.CommandBarItem")]
    public sealed class CommandBarItem : ComponentBase<CommandBarItem, HTMLButtonElement>
    {
        private readonly HTMLSpanElement _textSpan;
        private          HTMLElement     _icon;

        public CommandBarItem(string text = null, string icon = null)
        {
            _textSpan  = Span(_("tss-commandbar-item-text", text: text ?? string.Empty));
            InnerElement = Button(_("tss-commandbar-item", type: "button"), _textSpan);

            AttachClick();
            AttachContextMenu();

            if (!string.IsNullOrEmpty(icon))
            {
                SetIcon(icon);
            }
        }

        public string Text
        {
            get => _textSpan.innerText;
            set => _textSpan.innerText = value ?? string.Empty;
        }

        public string Icon
        {
            get => _icon?.className;
            set => SetIcon(value);
        }

        public bool IsEnabled
        {
            get => !InnerElement.classList.contains("tss-disabled");
            set => InnerElement.UpdateClassIfNot(value, "tss-disabled");
        }

        public CommandBarItem SetText(string text)
        {
            Text = text;
            return this;
        }

        public CommandBarItem SetIcon(string icon)
        {
            if (string.IsNullOrEmpty(icon))
            {
                if (_icon != null)
                {
                    InnerElement.removeChild(_icon);
                    _icon = null;
                }
                return this;
            }

            if (_icon == null)
            {
                _icon = I(_("tss-commandbar-item-icon"));
                InnerElement.insertBefore(_icon, _textSpan);
            }

            _icon.className = $"tss-commandbar-item-icon {icon}";
            return this;
        }

        public CommandBarItem Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        public CommandBarItem Primary(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-commandbar-item-primary");
            return this;
        }

        public CommandBarItem OnClick(Action action)
        {
            return OnClick((_, __) => action?.Invoke());
        }

        public override HTMLElement Render() => InnerElement;
    }
}
