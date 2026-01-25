using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.ActionButton")]
    public class ActionButton : IComponent, IHasBackgroundColor
    {
        public HTMLDivElement    Container          { get; protected set; }
        public HTMLDivElement    DisplayButton      { get; protected set; }
        public HTMLButtonElement ActionBtn          { get; protected set; }
        public IComponent        ActionBtnComponent { get; protected set; }

        private readonly IComponent  _content;
        private          HTMLElement _iconSpan;

        public delegate void ActionButtonEventHandler<TSender, TEventArgs>(TSender element, TEventArgs e);

        protected event ActionButtonEventHandler<HTMLDivElement, MouseEvent>    ClickedDisplay;
        protected event ActionButtonEventHandler<HTMLDivElement, MouseEvent>    ContextMenuDisplay;
        protected event ActionButtonEventHandler<HTMLButtonElement, MouseEvent> ClickedAction;
        protected event ActionButtonEventHandler<HTMLButtonElement, MouseEvent> ContextMenuAction;

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
            : this(HStack().Children(Icon(displayIcon, displayIconWeight, displayIconSize, displayColor), TextBlock(textContent).PL(8)),
                Icon.Transform(actionIcon, actionIconWeight),
                actionColor,
                actionIconSize)
        {
        }

        public ActionButton(
            string       textContent,
            UIcons       displayIcon,
            UIconsWeight actionIconWeight = UIconsWeight.Regular,
            UIcons       actionIcon       = UIcons.AngleCircleDown,
            string       actionColor      = null,
            TextSize     actionIconSize   = TextSize.Small)
            : this(HStack().Children(Icon(displayIcon), TextBlock(textContent).PL(8)),
                Icon.Transform(actionIcon, actionIconWeight),
                actionColor,
                actionIconSize)
        {
        }
        public ActionButton(
            string       textContent,
            UIcons       actionIcon       = UIcons.AngleCircleDown,
            UIconsWeight actionIconWeight = UIconsWeight.Regular,
            string       actionColor      = null,
            TextSize     actionIconSize   = TextSize.Small)
            : this(TextBlock(textContent),
                Icon.Transform(actionIcon, actionIconWeight),
                actionColor,
                actionIconSize)
        {
        }

        public ActionButton(
            IComponent contnent,
            string     actionIcon     = null,
            string     actionColor    = null,
            TextSize   actionIconSize = TextSize.Small)
        {
            if (string.IsNullOrWhiteSpace(actionIcon)) actionIcon = Icon.Transform(UIcons.AngleCircleDown, UIconsWeight.Regular);

            _content = contnent;

            DisplayButton = Div(_("tss-actionbutton-displaybutton"), _content.Render());

            _iconSpan = I(_("tss-icon " + actionIcon + " " + actionIconSize));

            if (actionColor is object)
            {
                _iconSpan.SetStyle(s => s.color = actionColor);
            }

            _iconSpan.dataset["icon"] = actionIcon;

            ActionBtn          = Button(_("tss-btn-remove-padding tss-actionbutton-actionbtn"), _iconSpan);
            ActionBtnComponent = Raw(ActionBtn);

            Container = Div(_("tss-actionbutton-container tss-default-component-margin"), DisplayButton, ActionBtnComponent.Render());

            DisplayButton.addEventListener("click",       @event => ClickedDisplay?.Invoke(DisplayButton, @event.As<MouseEvent>()));
            DisplayButton.addEventListener("contextmenu", @event => ContextMenuDisplay?.Invoke(DisplayButton, @event.As<MouseEvent>()));

            ActionBtn.addEventListener("click",       @event => ClickedAction?.Invoke(ActionBtn, @event.As<MouseEvent>()));
            ActionBtn.addEventListener("contextmenu", @event => ContextMenuAction?.Invoke(ActionBtn, @event.As<MouseEvent>()));

        }

        public string Background
        {
            get => DisplayButton.style.background;
            set
            {
                DisplayButton.style.background = value;
                ActionBtn.style.background = value;
                DisplayButton.UpdateClassIf(!string.IsNullOrWhiteSpace(value), "tss-filter-effects");
                ActionBtn.UpdateClassIf(!string.IsNullOrWhiteSpace(value), "tss-btn-filter-effects");
            }
        }

        public HTMLElement Render()
        {
            return Container;
        }

        public virtual ActionButton OnClickDisplay(ActionButtonEventHandler<HTMLDivElement, MouseEvent> onClick, bool clearPrevious = true)
        {
            if (ClickedDisplay != null && clearPrevious)
            {
                foreach (Delegate d in ClickedDisplay.GetInvocationList())
                {
                    ClickedDisplay -= (ActionButtonEventHandler<HTMLDivElement, MouseEvent>)d;
                }
            }

            ClickedDisplay += onClick;

            return this;
        }

        public virtual ActionButton OnClickAction(ActionButtonEventHandler<HTMLButtonElement, MouseEvent> onClick, bool clearPrevious = true)
        {
            if (ClickedAction != null && clearPrevious)
            {
                foreach (Delegate d in ClickedAction.GetInvocationList())
                {
                    ClickedAction -= (ActionButtonEventHandler<HTMLButtonElement, MouseEvent>)d;
                }
            }

            ClickedAction += onClick;

            return this;
        }

        public ActionButton ModifyActionButton(Action<IComponent> modify)
        {
            modify(ActionBtnComponent);
            return this;
        }

        /// <summary>
        /// Gets or set whenever button is danger
        /// </summary>
        public bool IsDanger
        {
            get => DisplayButton.classList.contains("tss-btn-danger");
            set
            {
                if (value)
                {
                    DisplayButton.classList.add("tss-btn-danger");
                    DisplayButton.classList.remove("tss-btn-default", "tss-btn-primary", "tss-btn-success");
                    ActionBtn.classList.add("tss-btn-danger");
                    ActionBtn.classList.remove("tss-btn-default", "tss-btn-primary", "tss-btn-success");
                }
                else
                {
                    DisplayButton.classList.add("tss-btn-default");
                    DisplayButton.classList.remove("tss-btn-success", "tss-btn-danger", "tss-btn-primary");
                    ActionBtn.classList.add("tss-btn-default");
                    ActionBtn.classList.remove("tss-btn-success", "tss-btn-danger", "tss-btn-primary");
                }
            }
        }

        /// <summary>
        /// Gets or set whenever button is primary
        /// </summary>
        public bool IsPrimary
        {
            get => DisplayButton.classList.contains("tss-btn-primary");
            set
            {
                if (value)
                {
                    DisplayButton.classList.add("tss-btn-primary");
                    DisplayButton.classList.remove("tss-btn-default", "tss-btn-success", "tss-btn-danger");
                    ActionBtn.classList.add("tss-btn-primary");
                    ActionBtn.classList.remove("tss-btn-default", "tss-btn-success", "tss-btn-danger");
                }
                else
                {
                    DisplayButton.classList.add("tss-btn-default");
                    DisplayButton.classList.remove("tss-btn-success", "tss-btn-danger", "tss-btn-primary");
                    ActionBtn.classList.add("tss-btn-default");
                    ActionBtn.classList.remove("tss-btn-success", "tss-btn-danger", "tss-btn-primary");
                }
            }
        }


        public ActionButton Primary()
        {
            IsPrimary = true;
            return this;
        }

        public ActionButton Danger()
        {
            IsDanger = true;
            return this;
        }

        public ActionButton Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        /// <summary>
        /// Gets or sets whenever button is enabled
        /// </summary>
        public bool IsEnabled
        {
            get => !Container.classList.contains("tss-disabled");
            set => Container.UpdateClassIfNot(value, "tss-disabled");
        }
    }
}