using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Kept so code written against the old ActionButton keeps compiling: it is now a <see cref="ButtonAndIcon"/> (split button)
    /// with the ActionButton method names. New code should use <see cref="UI.ButtonAndIcon"/> directly.
    /// </summary>
    [Transpose.Name("tss.ActionButton")]
    public class ActionButton : ButtonAndIcon
    {
        public delegate void ActionButtonEventHandler<TSender, TEventArgs>(TSender element, TEventArgs e);

        private event ActionButtonEventHandler<HTMLElement, MouseEvent> ClickedAction;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ActionButton(
            string       textContent,
            UIcons       displayIcon,
            UIconsWeight displayIconWeight = UIconsWeight.Regular,
            string       displayColor      = null,
            TextSize     displayIconSize   = TextSize.Small,
            UIconsWeight actionIconWeight  = UIconsWeight.Regular,
            UIcons       actionIcon        = UIcons.AngleCircleDown,
            string       actionColor       = null,
            TextSize     actionIconSize    = TextSize.Small)
            : base(textContent, RaiseAction, displayIcon)
        {
            SetIcon(displayIcon, displayColor ?? "", displayIconSize, displayIconWeight);
            SetSecondaryIcon(actionIcon, actionColor ?? "", actionIconSize, actionIconWeight);
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ActionButton(
            string       textContent,
            UIcons       displayIcon,
            UIconsWeight actionIconWeight = UIconsWeight.Regular,
            UIcons       actionIcon       = UIcons.AngleCircleDown,
            string       actionColor      = null,
            TextSize     actionIconSize   = TextSize.Small)
            : base(textContent, RaiseAction, displayIcon)
        {
            SetSecondaryIcon(actionIcon, actionColor ?? "", actionIconSize, actionIconWeight);
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ActionButton(
            string       textContent,
            UIcons       actionIcon       = UIcons.AngleCircleDown,
            UIconsWeight actionIconWeight = UIconsWeight.Regular,
            string       actionColor      = null,
            TextSize     actionIconSize   = TextSize.Small)
            : base(textContent, RaiseAction)
        {
            Icon = null;
            SetSecondaryIcon(actionIcon, actionColor ?? "", actionIconSize, actionIconWeight);
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="actionIcon">The CSS class of the action icon, as returned by <see cref="Tesserae.Icon.Transform(UIcons, UIconsWeight)"/>.</param>
        public ActionButton(
            IComponent content,
            string     actionIcon     = null,
            string     actionColor    = null,
            TextSize   actionIconSize = TextSize.Small)
            : base(string.Empty, RaiseAction)
        {
            ReplaceContent(content);

            if (string.IsNullOrWhiteSpace(actionIcon))
            {
                SetSecondaryIcon(UIcons.AngleCircleDown, actionColor ?? "", actionIconSize);
            }
            else
            {
                IconButton.Icon = actionIcon + " " + actionIconSize;
                if (actionColor is object && IconButton.InnerElement.querySelector("i") is HTMLElement icon) icon.style.color = actionColor;
            }
        }

        // Static so it can be handed to the base constructor: the split button passes itself back as the first argument.
        private static void RaiseAction(ButtonAndIcon mainButton, Button iconButton, MouseEvent e)
        {
            var self = (ActionButton)mainButton;
            self.ClickedAction?.Invoke(iconButton.InnerElement, e);
        }

        /// <summary>
        /// Gets the root element of the component.
        /// </summary>
        public HTMLElement Container => Render();

        /// <summary>
        /// Gets the element of the main (display) half.
        /// </summary>
        public HTMLElement DisplayButton => InnerElement;

        /// <summary>
        /// Gets the element of the action (arrow) half.
        /// </summary>
        public HTMLElement ActionBtn => IconButton.InnerElement;

        /// <summary>
        /// Gets the action (arrow) half as a component.
        /// </summary>
        public IComponent ActionBtnComponent => IconButton;

        /// <summary>
        /// Registers a callback invoked when the main (display) half is clicked.
        /// </summary>
        public virtual ActionButton OnClickDisplay(ActionButtonEventHandler<HTMLElement, MouseEvent> onClick, bool clearPrevious = true)
        {
            OnClick((b, e) => onClick(b.InnerElement, e), clearPrevious);
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the action (arrow) half is clicked.
        /// </summary>
        public virtual ActionButton OnClickAction(ActionButtonEventHandler<HTMLElement, MouseEvent> onClick, bool clearPrevious = true)
        {
            if (clearPrevious) ClickedAction = null;
            ClickedAction += onClick;
            return this;
        }

        /// <summary>
        /// Configures the action (arrow) half.
        /// </summary>
        public ActionButton ModifyActionButton(Action<IComponent> modify)
        {
            modify(IconButton);
            return this;
        }

        /// <summary>
        /// Styles the component using the primary tone.
        /// </summary>
        public new ActionButton Primary()
        {
            IsPrimary = true;
            return this;
        }

        /// <summary>
        /// Styles the component using the danger tone.
        /// </summary>
        public new ActionButton Danger()
        {
            IsDanger = true;
            return this;
        }

        /// <summary>
        /// Disables the component.
        /// </summary>
        public new ActionButton Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }
    }
}
