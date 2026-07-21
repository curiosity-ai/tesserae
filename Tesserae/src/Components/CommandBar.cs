using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A horizontal bar of commands (buttons, dropdowns) typically anchored to the top of an application surface.
    /// </summary>
    [Transpose.Name("tss.CommandBar")]
    public sealed class CommandBar : ComponentBase<CommandBar, HTMLElement>
    {
        private readonly HTMLElement _primarySection;
        private readonly HTMLElement _farSection;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public CommandBar(params IComponent[] items)
        {
            _primarySection = Div(_("tss-commandbar-section"));
            _farSection     = Div(_("tss-commandbar-section tss-commandbar-far"));

            InnerElement = Div(_("tss-commandbar"), _primarySection, _farSection);

            AddItems(items);
        }

        /// <summary>
        /// Adds the given item to the component.
        /// </summary>
        public CommandBar AddItem(IComponent item)
        {
            if (item != null)
            {
                _primarySection.appendChild(item.Render());
            }
            return this;
        }

        /// <summary>
        /// Adds the given items to the component.
        /// </summary>
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

        /// <summary>
        /// Adds the given far item to the component.
        /// </summary>
        public CommandBar AddFarItem(IComponent item)
        {
            if (item != null)
            {
                _farSection.appendChild(item.Render());
            }
            return this;
        }

        /// <summary>
        /// Adds the given far items to the component.
        /// </summary>
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

        /// <summary>
        /// Adds the given items to the component.
        /// </summary>
        public CommandBar Items(params IComponent[] items) => AddItems(items);
        /// <summary>
        /// Configures the far items on the component.
        /// </summary>
        public CommandBar FarItems(params IComponent[] items) => AddFarItems(items);

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }

    [Transpose.Name("tss.CommandBarItem")]
    public sealed class CommandBarItem : ComponentBase<CommandBarItem, HTMLButtonElement>
    {
        private readonly HTMLSpanElement _textSpan;
        private          HTMLElement     _icon;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public CommandBarItem(string text = null,  UIcons? icon = null)
        {
            _textSpan  = Span(_("tss-commandbar-item-text", text: text ?? string.Empty));
            InnerElement = Button(_("tss-commandbar-item", type: "button"), _textSpan);

            AttachClick();
            AttachContextMenu();

            if (icon.HasValue)
            {
                SetIcon(icon.Value);
            }
        }

        /// <summary>
        /// Gets or sets the text shown in the component.
        /// </summary>
        public string Text
        {
            get => _textSpan.innerText;
            set => _textSpan.innerText = value ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets the icon shown by the component.
        /// </summary>
        public string Icon
        {
            get => _icon?.className;
            set 
            {
                if (string.IsNullOrEmpty(value) && _icon != null)
                {
                    InnerElement.removeChild(_icon);
                    _icon = null;

                    InnerElement.classList.remove("tss-btn-only-icon");

                    return;
                }

                if (_icon == null)
                {
                    _icon = I(_("tss-commandbar-item-icon"));
                    InnerElement.insertBefore(_icon, _textSpan);
                }

                _icon.className = $"tss-commandbar-item-icon {value}";
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the component is interactive (enabled).
        /// </summary>
        public bool IsEnabled
        {
            get => !InnerElement.classList.contains("tss-disabled");
            set => InnerElement.UpdateClassIfNot(value, "tss-disabled");
        }

        /// <summary>
        /// Sets the text of the component.
        /// </summary>
        public CommandBarItem SetText(string text)
        {
            Text = text;
            return this;
        }

        /// <summary>
        /// Sets the icon of the component.
        /// </summary>
        public CommandBarItem SetIcon(UIcons icon)
        {
            Icon = $"ec {icon}";
            return this;
        }

        /// <summary>
        /// Disables the component.
        /// </summary>
        public CommandBarItem Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        /// <summary>
        /// Styles the component using the primary tone.
        /// </summary>
        public CommandBarItem Primary(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-commandbar-item-primary");
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the click event fires.
        /// </summary>
        public CommandBarItem OnClick(Action action)
        {
            return OnClick((_, __) => action?.Invoke());
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
