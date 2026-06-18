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
        private readonly IComponent               _clickable;
        private readonly HTMLElement              _anchor;
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
            var content = Div(_("tss-sidenav-btn-content"), iconEl, labelEl);

            _root = Div(_("tss-sidenav-btn-wrap"));
            _dot  = Div(_("tss-sidenav-btn-dot"));

            if (string.IsNullOrWhiteSpace(href))
            {
                _button    = Button().Class("tss-sidenav-btn").ReplaceContent(Raw(content));
                _clickable = _button;
            }
            else
            {
                // Render the clickable element as a real <a href> so the browser handles
                // Ctrl/Cmd/middle-click ("open in new tab") natively. It stays a direct child
                // of .tss-sidenav-btn-wrap and carries tss-btn + tss-sidenav-btn so the rail CSS
                // keeps matching, plus tss-link-no-underline to suppress the anchor hover underline.
                _anchor    = A(_("tss-btn tss-sidenav-btn tss-link-no-underline", href: href), content);
                _clickable = Raw(_anchor);
            }

            _clickable.Render().setAttribute("data-id", identifier);

            _root.appendChild(_clickable.Render());
            _root.appendChild(_dot);

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

            _tooltip(_clickable);
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
            if (_button is object)
            {
                _button.OnClick(action);
            }
            else if (_anchor is object)
            {
                // Anchor-backed (href) button: run the action on a plain click but let
                // modified clicks (Ctrl/Cmd/Shift/middle) fall through to the browser so
                // they still open the href in a new tab/window.
                _anchor.onclick = (e) =>
                {
                    var me = e.As<MouseEvent>();
                    if (me.ctrlKey || me.metaKey || me.shiftKey || me.button != 0) return;
                    action();
                };
            }
            return this;
        }

        /// <summary>Adds a click handler with the underlying button and mouse event.</summary>
        public SidenavButton OnClick(Action<Button, MouseEvent> action)
        {
            if (_button is object)
            {
                _button.OnClick((b, e) => action(b, e));
            }
            return this;
        }

        /// <summary>Sets a custom tooltip text shown on hover.</summary>
        public SidenavButton Tooltip(string text)
        {
            _tooltip = (b) => b.Tooltip(text, placement: TooltipPlacement.Right);
            _tooltip(_clickable);
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
