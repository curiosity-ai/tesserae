using H5;
using static H5.Core.dom;
using H5.Core;
using System;

namespace Tesserae
{


    /// <summary>
    /// Base class for every Tesserae component. Provides DOM event wiring, click / focus / change events, ARIA and
    /// margin/padding support.
    /// </summary>
    [H5.Name("tss.CB")]
    public abstract class ComponentBase<T, THTML> : IComponent, IHasClickHandler, IHasMarginPadding, IAccessibility where T : ComponentBase<T, THTML> where THTML : HTMLElement
    {
        protected event ComponentEventHandler<T, MouseEvent>     Clicked;
        protected event ComponentEventHandler<T, MouseEvent>     MouseOver;
        protected event ComponentEventHandler<T, MouseEvent>     MouseOut;
        protected event ComponentEventHandler<T, MouseEvent>     ContextMenu;
        protected event ComponentEventHandler<T, Event>          Changed;
        protected event ComponentEventHandler<T, ClipboardEvent> Pasted;
        protected event ComponentEventHandler<T, Event>          InputUpdated;
        protected event ComponentEventHandler<T, Event>          ReceivedFocus;
        protected event ComponentEventHandler<T, Event>          LostFocus;
        protected event ComponentEventHandler<T, KeyboardEvent>  KeyDown;
        protected event ComponentEventHandler<T, KeyboardEvent>  KeyUp;
        protected event ComponentEventHandler<T, KeyboardEvent>  KeyPress;

        /// <summary>
        /// Gets the underlying DOM element backing this component.
        /// </summary>
        public THTML  InnerElement { get;                               protected set; }
        /// <summary>
        /// Gets or sets the CSS margin of the component.
        /// </summary>
        public string Margin       { get => InnerElement.style.margin;  set => InnerElement.style.margin = value; }
        /// <summary>
        /// Gets or sets the CSS padding of the component.
        /// </summary>
        public string Padding      { get => InnerElement.style.padding; set => InnerElement.style.padding = value; }

        /// <summary>
        /// Sets the ARIA role of the component.
        /// </summary>
        public string AriaRole        { set => InnerElement.setAttribute("role", value); }
        /// <summary>
        /// Sets the ARIA accessible-name label of the component.
        /// </summary>
        public string AriaLabel       { set => InnerElement.setAttribute("aria-label", value); }
        /// <summary>
        /// Gets or sets the aria labelled by.
        /// </summary>
        public string AriaLabelledBy  { set => InnerElement.setAttribute("aria-labelledby", value); }
        /// <summary>
        /// Gets or sets the aria described by.
        /// </summary>
        public string AriaDescribedBy { set => InnerElement.setAttribute("aria-describedby", value); }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public abstract HTMLElement Render();

        /// <summary>
        /// Registers a callback invoked when the click base event fires.
        /// </summary>
        public void OnClickBase(ComponentEventHandler<IComponent, MouseEvent> onClick, bool clearPrevious = true)
        {
            OnClick((a, b) => onClick(a, b), clearPrevious);
        }

        /// <summary>
        /// Registers a callback invoked when the context menu base event fires.
        /// </summary>
        public void OnContextMenuBase(ComponentEventHandler<IComponent, MouseEvent> onContextMenu, bool clearPrevious = true)
        {
            OnContextMenu((a, b) => onContextMenu(a, b), clearPrevious);
        }

        /// <summary>
        /// Registers a callback invoked when the click event fires.
        /// </summary>
        public virtual T OnClick(ComponentEventHandler<T, MouseEvent> onClick, bool clearPrevious = true)
        {
            if (Clicked != null && clearPrevious)
            {
                foreach (Delegate d in Clicked.GetInvocationList())
                {
                    Clicked -= (ComponentEventHandler<T, MouseEvent>)d;
                }
            }

            Clicked += onClick;

            if (this is TextBlock textBlock)
                textBlock.Cursor = "pointer";

            if (this is Image img)
                img.Cursor = "pointer";

            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the mouse over event fires.
        /// </summary>
        public virtual T OnMouseOver(ComponentEventHandler<T, MouseEvent> onEnter, ComponentEventHandler<T, MouseEvent> onLeave = null, bool clearPrevious = true)
        {
            if (MouseOver != null && clearPrevious)
            {
                foreach (Delegate d in MouseOver.GetInvocationList())
                {
                    MouseOver -= (ComponentEventHandler<T, MouseEvent>)d;
                }

                foreach (Delegate d in MouseOut.GetInvocationList())
                {
                    MouseOut -= (ComponentEventHandler<T, MouseEvent>)d;
                }
            }

            MouseOver += onEnter;

            if (onLeave is object)
            {
                MouseOut += onLeave;
            }

            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the context menu event fires.
        /// </summary>
        public virtual T OnContextMenu(ComponentEventHandler<T, MouseEvent> onContextMenu, bool clearPrevious = true)
        {
            if (ContextMenu != null && clearPrevious)
            {
                foreach (Delegate d in ContextMenu.GetInvocationList())
                {
                    ContextMenu -= (ComponentEventHandler<T, MouseEvent>)d;
                }
            }

            ContextMenu += onContextMenu;

            if (this is TextBlock textBlock)
                textBlock.Cursor = "pointer";

            if (this is Image img)
                img.Cursor = "pointer";

            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the change event fires.
        /// </summary>
        public virtual T OnChange(ComponentEventHandler<T, Event> onChange)
        {
            Changed += onChange;
            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the input event fires.
        /// </summary>
        public virtual T OnInput(ComponentEventHandler<T, Event> onInput)
        {
            InputUpdated += onInput;
            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the focus event fires.
        /// </summary>
        public virtual T OnFocus(ComponentEventHandler<T, Event> onFocus)
        {
            ReceivedFocus += onFocus;
            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the blur event fires.
        /// </summary>
        public virtual T OnBlur(ComponentEventHandler<T, Event> onBlur)
        {
            LostFocus += onBlur;
            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the key down event fires.
        /// </summary>
        public virtual T OnKeyDown(ComponentEventHandler<T, KeyboardEvent> onKeyDown)
        {
            KeyDown += onKeyDown;
            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the key up event fires.
        /// </summary>
        public virtual T OnKeyUp(ComponentEventHandler<T, KeyboardEvent> onKeyUp)
        {
            KeyUp += onKeyUp;
            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the key press event fires.
        /// </summary>
        public virtual T OnKeyPress(ComponentEventHandler<T, KeyboardEvent> onKeyPress)
        {
            KeyPress += onKeyPress;
            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the pasted event fires.
        /// </summary>
        public virtual T OnPasted(ComponentEventHandler<T, ClipboardEvent> onPasted)
        {
            Pasted += onPasted;
            return (T)this;
        }

        protected void AttachClick()
        {
            InnerElement.addEventListener("click",     e => RaiseOnClick(e.As<MouseEvent>()));
            InnerElement.addEventListener("mouseover", e => RaiseOnMouseOver(e.As<MouseEvent>()));
            InnerElement.addEventListener("mouseout",  e => RaiseOnMouseOut(e.As<MouseEvent>()));
        }

        protected void AttachContextMenu() => InnerElement.addEventListener("contextmenu", e => RaiseOnContextMenu(e.As<MouseEvent>()));

        protected void AttachChange() => InnerElement.addEventListener("change", s => RaiseOnChange(s));

        /// <summary>
        /// Raises the on click event on the component.
        /// </summary>
        public void RaiseOnClick(MouseEvent     ev) => Clicked?.Invoke((T)this, ev);
        /// <summary>
        /// Raises the on mouse over event on the component.
        /// </summary>
        public void RaiseOnMouseOver(MouseEvent ev) => MouseOver?.Invoke((T)this, ev);
        /// <summary>
        /// Raises the on mouse out event on the component.
        /// </summary>
        public void RaiseOnMouseOut(MouseEvent  ev) => MouseOut?.Invoke((T)this, ev);

        /// <summary>
        /// Raises the on context menu event on the component.
        /// </summary>
        public void RaiseOnContextMenu(MouseEvent ev) => ContextMenu?.Invoke((T)this, ev);

        //Some controls won't change the underlying value till after this event. As we usually want the final value and not the previous state, we raise the event on a timer
        protected void RaiseOnChange(Event ev) => window.setTimeout((_) => Changed?.Invoke((T)this, ev), 1);

        protected void AttachInput() => InnerElement.addEventListener("input", ev => RaiseOnInput(ev));

        protected void AttachKeys()
        {
            InnerElement.addEventListener("keypress", ev => RaiseOnKeyPress(ev.As<KeyboardEvent>()));
            InnerElement.addEventListener("keydown",  ev => RaiseOnKeyDown(ev.As<KeyboardEvent>()));
            InnerElement.addEventListener("keyup",    ev => RaiseOnKeyUp(ev.As<KeyboardEvent>()));
            InnerElement.addEventListener("paste",    ev => RaiseOnPaste(ev.As<ClipboardEvent>()));
        }

        protected void AttachFocus() => InnerElement.addEventListener("focus", s => RaiseOnFocus(s));

        protected void AttachBlur() => InnerElement.addEventListener("blur", s => RaiseOnBlur(s));

        protected void RaiseOnPaste(ClipboardEvent ev) => Pasted?.Invoke((T)this, ev);
        protected void RaiseOnInput(Event          ev) => InputUpdated?.Invoke((T)this, ev);

        protected void RaiseOnKeyDown(KeyboardEvent ev) => KeyDown?.Invoke((T)this, ev);

        protected void RaiseOnKeyUp(KeyboardEvent ev) => KeyUp?.Invoke((T)this, ev);

        protected void RaiseOnKeyPress(KeyboardEvent ev) => KeyPress?.Invoke((T)this, ev);

        private void RaiseOnFocus(Event ev) => ReceivedFocus?.Invoke((T)this, ev);

        private void RaiseOnBlur(Event ev) => LostFocus?.Invoke((T)this, ev);
    }
}