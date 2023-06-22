using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.ActionButton")]
    public class ActionButton : IComponent
    {
        public HTMLDivElement    Container     { get; protected set; }
        public HTMLDivElement    DisplayButton { get; protected set; }
        public HTMLButtonElement ActionBtn     { get; protected set; }

        private readonly IComponent  _content;
        private          HTMLElement _iconSpan;

        public delegate void ActionButtonEventHandler<TSender, TEventArgs>(TSender element, TEventArgs e);

        protected event ActionButtonEventHandler<HTMLDivElement, MouseEvent>    ClickedDisplay;
        protected event ActionButtonEventHandler<HTMLDivElement, MouseEvent>    ContextMenuDisplay;
        protected event ActionButtonEventHandler<HTMLButtonElement, MouseEvent> ClickedAction;
        protected event ActionButtonEventHandler<HTMLButtonElement, MouseEvent> ContextMenuAction;

        public ActionButton(IComponent contnent = null, string textContent = null, UIcons? display = null, UIcons actionIcon = UIcons.AngleCircleDown, string actionColor = null)
        {
            if (contnent is object)
            {
                _content = contnent;
            }
            else
            {
                if (display.HasValue)
                {
                    _content = HStack().Children(Icon(display.Value), TextBlock(textContent).PL(8));
                }
                else
                {
                    _content = TextBlock(textContent);
                }
            }

            DisplayButton = Div(_("tss-actionbutton-displaybutton"), _content.Render());

            var weight = UIconsWeight.Regular;
            var size = TextSize.Small;
            var iStr = Icon.Transform(actionIcon, weight);
            _iconSpan = I(_("tss-icon " + iStr + " " + size));

            if (actionColor is object)
            {
                _iconSpan.SetStyle(s => s.color = actionColor);
            }

            _iconSpan.dataset["icon"] = iStr;

            ActionBtn = Button(_("tss-btn-remove-padding tss-actionbutton-actionbtn"), _iconSpan);
            Container = Div(_("tss-actionbutton-container tss-default-component-margin"), DisplayButton, ActionBtn);

            DisplayButton.addEventListener("click", @event => ClickedDisplay?.Invoke(DisplayButton, @event.As<MouseEvent>()));
            DisplayButton.addEventListener("contextmenu", @event => ContextMenuDisplay?.Invoke(DisplayButton, @event.As<MouseEvent>()));

            ActionBtn.addEventListener("click", @event => ClickedAction?.Invoke(ActionBtn, @event.As<MouseEvent>()));
            ActionBtn.addEventListener("contextmenu", @event => ContextMenuAction?.Invoke(ActionBtn, @event.As<MouseEvent>()));
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