using System;
using System.Collections.Generic;
using System.Linq;
using H5.Core;
using TNT;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A Button component for use within a Sidebar, supporting both open and closed states.
    /// </summary>
    public class SidebarButton : ISearchableSidebarItem
    {
        private readonly Button                   _closedButton;
        private readonly Button                   _openButton;
        private readonly IComponent               _open;
        private readonly SidebarCommand[]         _commands;
        private readonly SidebarBadge             _badge;
        private          Action<IComponent>       _tooltipClosed;
        private readonly ISidebarIcon             _image;
        private          Action<IComponent>       _tooltipOpen;
        private readonly SettableObservable<bool> _selected;
        private readonly string                   _text;

        private event Action<HTMLElement> _onRendered;

        /// <summary>Gets or sets whether the button is currently selected.</summary>
        public bool IsSelected { get { return _selected.Value; } set { _selected.Value = value; if (value) CurrentRendered.ScrollIntoView(); } }

        /// <summary>Gets the component that is currently rendered.</summary>
        public IComponent CurrentRendered => _closedButton.IsMounted() ? _closedButton : _open;

        /// <summary>Gets the full identifier of the button.</summary>
        public string Identifier { get; private set; }
        
        /// <summary>Gets the own identifier of the button.</summary>
        public string OwnIdentifier => Sidebar.GetOwnIdentifier(Identifier);

        /// <summary>Adds a group identifier prefix to the button's identifier.</summary>
        public void AddGroupIdentifier(string groupIdentifier)
        {
            Identifier = groupIdentifier + Sidebar.GroupIdentifierSeparator + Identifier;
        }

        public SidebarButton(string identifier, Emoji        icon,  string       text,   params SidebarCommand[] commands) : this(identifier, text, null, Button().SetIcon(icon), Button().SetIcon(icon), commands) { }
        public SidebarButton(string identifier, Emoji        icon,  string       text,   SidebarBadge            badge, params SidebarCommand[] commands) : this(identifier, text, badge, Button().SetIcon(icon), Button().SetIcon(icon), commands) { }
        public SidebarButton(string identifier, ISidebarIcon image, string       text,   params SidebarCommand[] commands) : this(identifier, image, text, null, commands) { }
        public SidebarButton(string identifier, UIcons       icon,  string       text,   SidebarBadge            badge, params SidebarCommand[] commands) : this(identifier, icon, UIconsWeight.Regular, text, badge, commands) { }
        public SidebarButton(string identifier, UIcons       icon,  string       text,   params SidebarCommand[] commands) : this(identifier, icon, text, null, commands) { }
        public SidebarButton(string identifier, UIcons       icon,  UIconsWeight weight, string                  text, params SidebarCommand[] commands) : this(identifier, icon, weight, text, null, commands) { }

        public SidebarButton(string identifier, UIcons icon, UIconsWeight weight, string text, SidebarBadge badge, params SidebarCommand[] commands) : this(identifier, text, badge, Button().SetIcon(icon, weight: weight), Button().SetIcon(icon, weight: weight), commands) { }
        private SidebarButton(string identifier, string text, SidebarBadge badge, Button buttonWithIconClosed, Button buttonWithIconOpen, params SidebarCommand[] commands)
        {
            Identifier     = identifier;
            _text          = text;
            _selected      = new SettableObservable<bool>(false);
            _tooltipClosed = (b) => b.Tooltip(text, placement: TooltipPlacement.Right);
            
            _closedButton  = buttonWithIconClosed.Class("tss-sidebar-btn").Id(identifier);
            _openButton    = buttonWithIconOpen.SetText(text).Class("tss-sidebar-btn").Id(identifier);

            _commands = commands;
            _badge    = badge;

            _open = Wrap(_openButton);

            var hookContextMenu = _commands.FirstOrDefault(c => c.ShouldHookToContextMenu);

            if (hookContextMenu is object)
            {
                OnContextMenu((b, e) => hookContextMenu.RaiseOnClick(e));
            }

            _selected.Observe(isSelected =>
            {
                if (isSelected)
                {
                    _closedButton.Class("tss-sidebar-selected");
                    _open.Class("tss-sidebar-selected");
                }
                else
                {
                    _closedButton.RemoveClass("tss-sidebar-selected");
                    _open.RemoveClass("tss-sidebar-selected");
                }
            });

            IComponent Wrap(Button button)
            {
                var div = Div(_("tss-sidebar-btn-open"));
                div.appendChild(button.Render());

                if (_badge is object)
                {
                    var divCmd = Div(_("tss-sidebar-badges"));
                    div.appendChild(divCmd);
                    divCmd.appendChild(_badge.Render());
                }

                if (_commands is object && _commands.Length > 0)
                {
                    var divCmd = Div(_("tss-sidebar-commands"));
                    div.appendChild(divCmd);

                    foreach (var c in _commands)
                    {
                        divCmd.appendChild(c.Render());
                    }
                }
                return Raw(div);
            }
        }

        /// <summary>Shows the button.</summary>
        public void Show()
        {
            _closedButton.Show();
            _openButton.Show();
        }

        /// <summary>Collapses the button.</summary>
        public void Collapse()
        {
            _closedButton.Collapse();
            _openButton.Collapse();
        }

        /// <summary>
        /// Marks the button as not sortable.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton NotSortable()
        {
            _closedButton.Class("tss-sortable-disable");
            _openButton.Class("tss-sortable-disable");
            return this;
        }

        public SidebarButton(string identifier, ISidebarIcon image, string text, SidebarBadge badge, params SidebarCommand[] commands)
        {
            Identifier = identifier;
            _text      = text;
            _selected  = new SettableObservable<bool>(false);

            _tooltipClosed = (b) => b.Tooltip(text, placement: TooltipPlacement.Right);

            _image = image;

            _closedButton = Button().Class("tss-sidebar-btn").ReplaceContent(image).Id(identifier);

            _openButton = Button(text).ReplaceContent(Raw(Div(_("tss-btn-with-image"), image.Clone().Render(), Span(_(text: text))))).Class("tss-sidebar-btn").Id(identifier);

            _commands = commands;
            _badge    = badge;

            _open = Wrap(_openButton);

            _selected.Observe(isSelected =>
            {
                if (isSelected)
                {
                    _closedButton.Class("tss-sidebar-selected");
                    _open.Class("tss-sidebar-selected");
                }
                else
                {
                    _closedButton.RemoveClass("tss-sidebar-selected");
                    _open.RemoveClass("tss-sidebar-selected");
                }
            });

            IComponent Wrap(Button button)
            {
                var div = Div(_("tss-sidebar-btn-open"));
                div.appendChild(button.Render());

                if (_badge is object)
                {
                    var divCmd = Div(_("tss-sidebar-badges"));
                    div.appendChild(divCmd);
                    divCmd.appendChild(_badge.Render());
                }

                if (_commands.Length > 0)
                {
                    var divCmd = Div(_("tss-sidebar-commands"));
                    div.appendChild(divCmd);

                    foreach (var c in _commands)
                    {
                        divCmd.appendChild(c.Render());
                    }
                }
                return Raw(div);
            }
        }

        /// <summary>
        /// Clears the progress indication background.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton ClearProgress()
        {
            _openButton.Background   = "";
            _closedButton.Background = "";
            return this;
        }

        /// <summary>
        /// Sets a progress indication background.
        /// </summary>
        /// <param name="progress">The progress value (0 to 1).</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton Progress(float progress)
        {
            var p = $"linear-gradient(to right, rgba(var(--tss-primary-background-color-root),0.2), rgba(var(--tss-primary-background-color-root),0.2) {progress * 100:0.0}%, transparent 0)";
            _openButton.Background   = p;
            _closedButton.Background = p;
            return this;
        }


        /// <summary>
        /// Sets the text of the button.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton SetText(string text)
        {
            if (_image is object)
            {
                _openButton.ReplaceContent(Raw(Div(_("tss-btn-with-image"), _image.Clone().Render(), Span(_(text: text)))));
            }
            else
            {
                _openButton.SetText(text);
            }
            return this;
        }

        /// <summary>
        /// Ensures that commands associated with the button are always visible when the sidebar is open.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton CommandsAlwaysVisible()
        {
            _open.Class("tss-sidebar-commands-always-open");
            return this;
        }

        /// <summary>
        /// Sets the button to use a light style.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton Light()
        {
            _open.Class("tss-sidebar-btn-light");
            _closedButton.Class("tss-sidebar-btn-light");
            return this;
        }

        /// <summary>
        /// Sets the button to use a danger style.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton Danger()
        {
            _openButton.Danger();
            _closedButton.Danger();
            return this;
        }

        /// <summary>
        /// Sets the button to use the default style.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton Default()
        {
            _openButton.IsPrimary   = false;
            _closedButton.IsPrimary = false;
            return this;
        }

        /// <summary>
        /// Sets the button to use a success style.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton Success()
        {
            _openButton.Success();
            _closedButton.Success();
            return this;
        }

        /// <summary>
        /// Sets the button to use the primary style.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton Primary()
        {
            _openButton.Primary();
            _closedButton.Primary();
            return this;
        }

        /// <summary>
        /// Sets whether the button is selected.
        /// </summary>
        /// <param name="isSelected">Whether the button is selected.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton Selected(bool isSelected = true)
        {
            _selected.Value = isSelected;
            return this;
        }

        /// <summary>
        /// Sets a tooltip for the closed state button.
        /// </summary>
        /// <param name="text">The tooltip text.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton Tooltip(string text)
        {
            _tooltipClosed = (b) => b.Tooltip(text, placement: TooltipPlacement.Right);
            _tooltipClosed(_closedButton);
            return this;
        }

        /// <summary>
        /// Sets a tooltip component for the closed state button.
        /// </summary>
        /// <param name="tooltip">The tooltip component.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton Tooltip(IComponent tooltip)
        {
            _tooltipClosed = (b) => b.Tooltip(tooltip, placement: TooltipPlacement.Right);
            _tooltipClosed(_closedButton);
            return this;
        }

        /// <summary>
        /// Sets a tooltip generator function for the closed state button.
        /// </summary>
        /// <param name="tooltip">The tooltip generator function.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton Tooltip(Func<IComponent> tooltip)
        {
            _tooltipClosed = (b) => b.Tooltip(tooltip(), placement: TooltipPlacement.Right);
            _tooltipClosed(_closedButton);
            return this;
        }

        /// <summary>
        /// Sets a tooltip for the open state button.
        /// </summary>
        /// <param name="text">The tooltip text.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton OpenedTooltip(string text)
        {
            _tooltipOpen = (b) => b.Tooltip(text, placement: TooltipPlacement.Right);
            _tooltipOpen(_openButton);
            return this;
        }

        /// <summary>
        /// Sets a tooltip component for the open state button.
        /// </summary>
        /// <param name="tooltip">The tooltip component.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton OpenedTooltip(IComponent tooltip)
        {
            _tooltipOpen = (b) => b.Tooltip(tooltip, placement: TooltipPlacement.Right);
            _tooltipOpen(_openButton);
            return this;
        }

        /// <summary>
        /// Sets a tooltip generator function for the open state button.
        /// </summary>
        /// <param name="tooltip">The tooltip generator function.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton OpenedTooltip(Func<IComponent> tooltip)
        {
            _tooltipOpen = (b) => b.Tooltip(tooltip(), placement: TooltipPlacement.Right);
            _tooltipOpen(_openButton);
            return this;
        }

        /// <summary>
        /// Adds a click event handler to the button.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton OnClick(Action action)
        {
            _closedButton.OnClick(action);
            _openButton.OnClick(action);
            return this;
        }

        /// <summary>
        /// Adds a click event handler to the icon when the button is open.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton OnOpenIconClick(Action<HTMLElement, MouseEvent> action)
        {
            _openButton.OnIconClick(action);
            _openButton.Class("tss-sidebar-btn-has-icon-click");
            return this;
        }

        /// <summary>
        /// Sets the ID of the button.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton Id(string id)
        {
            _open.Id(id);
            _closedButton.Id(id);
            return this;

        }

        /// <summary>
        /// Adds a click event handler to the icon when the button is open.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton OnOpenIconClick(Action action)
        {
            _openButton.OnIconClick((_, __) => action());
            _openButton.Class("tss-sidebar-btn-has-icon-click");
            return this;
        }

        /// <summary>
        /// Adds a context menu event handler to the button.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton OnContextMenu(Action action)
        {
            _closedButton.OnContextMenu(action);
            _openButton.OnContextMenu(action);
            return this;
        }

        /// <summary>
        /// Adds a click event handler with button and mouse event arguments.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton OnClick(Action<Button, MouseEvent> action)
        {
            _closedButton.OnClick((b, e) => action(b, e));
            _openButton.OnClick((b,   e) => action(b, e));
            return this;
        }

        /// <summary>
        /// Adds a context menu event handler with button and mouse event arguments.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton OnContextMenu(Action<Button, MouseEvent> action)
        {
            _closedButton.OnContextMenu((b, e) => action(b, e));
            _openButton.OnContextMenu((b,   e) => action(b, e));
            return this;
        }

        /// <summary>
        /// Sets the icon for the button.
        /// </summary>
        /// <param name="icon">The icon.</param>
        /// <param name="color">The color of the icon.</param>
        /// <param name="weight">The weight of the icon.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton SetIcon(UIcons icon, string color = "", UIconsWeight weight = UIconsWeight.Regular)
        {
            _closedButton.SetIcon(icon, color, weight: weight);
            _openButton.SetIcon(icon, color, weight: weight);
            return this;
        }

        /// <summary>
        /// Sets an emoji icon for the button.
        /// </summary>
        /// <param name="icon">The emoji icon.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarButton SetIcon(Emoji icon)
        {
            _closedButton.SetIcon(icon);
            _openButton.SetIcon(icon);
            return this;
        }

        /// <summary>
        /// Adds a rendered event handler.
        /// </summary>
        /// <param name="onRendered">The rendered event handler.</param>
        /// <returns>The current instance of the type.</returns>
        public ISidebarItem OnRendered(Action<HTMLElement> onRendered)
        {
            _onRendered += onRendered;
            return this;
        }
        /// <summary>Renders the button for the closed state of the sidebar.</summary>
        public IComponent RenderClosed()
        {
            _onRendered?.Invoke(_closedButton.Render());
            _closedButton.RemoveTooltip();

            DomObserver.WhenMounted(_closedButton.Render(), () =>
            {
                window.setTimeout(_ => { _tooltipClosed?.Invoke(_closedButton); }, Sidebar.SIDEBAR_TRANSITION_TIME);
            });
            return _closedButton;
        }

        /// <summary>Renders the button for the open state of the sidebar.</summary>
        public IComponent RenderOpen()
        {
            foreach (var c in _commands) c.RefreshTooltip();
            _tooltipOpen?.Invoke(_openButton);
            _onRendered?.Invoke(_open.Render());
            return _open;
        }

        public bool Search(string searchTerm)
        {
            if(string.IsNullOrWhiteSpace(searchTerm))
            {
                Show();
                return true;
            }

            if(_text.ToLower().Contains(searchTerm.ToLower()))
            {
                Show();
                return true;
            }

            Collapse();
            return false;
        }
    }
}
