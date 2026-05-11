using System;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A vertical icon-with-label button intended for use inside a Sidenav.
    /// Displays an icon on top with a small text label below it.
    /// </summary>
    public class SidenavButton : ISidenavItem
    {
        private readonly Button                   _button;
        private readonly HTMLElement              _root;
        private readonly HTMLElement              _dot;
        private readonly SettableObservable<bool> _selected;
        private readonly string                   _text;
        private          Action<IComponent>       _tooltip;

        /// <summary>Gets or sets whether the button is currently selected.</summary>
        public bool IsSelected
        {
            get => _selected.Value;
            set => _selected.Value = value;
        }

        /// <summary>Gets the identifier of the button.</summary>
        public string Identifier { get; }

        public SidenavButton(string identifier, UIcons icon, string text)
            : this(identifier, null, icon, UIconsWeight.Regular, text) { }

        public SidenavButton(string identifier, UIcons icon, UIconsWeight weight, string text)
            : this(identifier, null, icon, weight, text) { }

        public SidenavButton(string identifier, string href, UIcons icon, string text)
            : this(identifier, href, icon, UIconsWeight.Regular, text) { }

        public SidenavButton(string identifier, string href, UIcons icon, UIconsWeight weight, string text)
        {
            Identifier = identifier;
            _text      = text;
            _selected  = new SettableObservable<bool>(false);
            _tooltip   = (b) => b.Tooltip(text, placement: TooltipPlacement.Right);

            var iconStr = $"{Tesserae.Icon.Transform(icon, weight)} medium";
            var iconEl  = I(_("tss-sidenav-btn-icon " + iconStr));
            var labelEl = Span(_("tss-sidenav-btn-label", text: text));

            _button = Button()
                .Class("tss-sidenav-btn")
                .ReplaceContent(Raw(Div(_("tss-sidenav-btn-content"), iconEl, labelEl)));

            _button.Render().setAttribute("data-id", identifier);

            _root = Div(_("tss-sidenav-btn-wrap"));
            _dot  = Div(_("tss-sidenav-btn-dot"));
            _root.appendChild(_button.Render());
            _root.appendChild(_dot);

            if (!string.IsNullOrWhiteSpace(href))
            {
                _button.OnClick(() => window.location.href = href);
            }

            _selected.Observe(isSelected =>
            {
                if (isSelected)
                {
                    _root.classList.add("tss-sidenav-selected");
                }
                else
                {
                    _root.classList.remove("tss-sidenav-selected");
                }
            });

            _tooltip(_button);
        }

        /// <summary>Sets the button as selected.</summary>
        public SidenavButton Selected(bool isSelected = true)
        {
            _selected.Value = isSelected;
            return this;
        }

        /// <summary>Adds a click handler to the button.</summary>
        public SidenavButton OnClick(Action action)
        {
            _button.OnClick(action);
            return this;
        }

        /// <summary>Adds a click handler with the underlying button and mouse event.</summary>
        public SidenavButton OnClick(Action<Button, MouseEvent> action)
        {
            _button.OnClick((b, e) => action(b, e));
            return this;
        }

        /// <summary>Sets a custom tooltip text shown on hover.</summary>
        public SidenavButton Tooltip(string text)
        {
            _tooltip = (b) => b.Tooltip(text, placement: TooltipPlacement.Right);
            _tooltip(_button);
            return this;
        }

        /// <summary>Shows a small dot indicator on the icon (e.g. to mark notifications).</summary>
        public SidenavButton ShowDot(bool show = true)
        {
            if (show)
            {
                _dot.classList.add("tss-sidenav-btn-dot-visible");
            }
            else
            {
                _dot.classList.remove("tss-sidenav-btn-dot-visible");
            }
            return this;
        }

        /// <summary>Sets a danger color for the dot indicator.</summary>
        public SidenavButton DotDanger()
        {
            _dot.classList.add("tss-sidenav-btn-dot-danger");
            return this;
        }

        /// <summary>Marks this button as the brand/logo at the top of the rail.</summary>
        public SidenavButton AsBrand()
        {
            _root.classList.add("tss-sidenav-btn-brand");
            return this;
        }

        /// <summary>Renders the button.</summary>
        public IComponent Render() => Raw(_root);

        /// <summary>Shows the button.</summary>
        public void Show()
        {
            _root.style.display = "";
        }

        /// <summary>Collapses the button.</summary>
        public void Collapse()
        {
            _root.style.display = "none";
        }
    }
}
