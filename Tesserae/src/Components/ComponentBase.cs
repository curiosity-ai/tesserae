using H5;
using static H5.Core.dom;

namespace Tesserae.Components
{
    public abstract class ComponentBase<T, THTML> : IComponent, IHasMarginPadding where T : ComponentBase<T, THTML> where THTML : HTMLElement
    {
        public delegate void ComponentEventHandler<TEventArgs>(T sender, TEventArgs e);
        
        public event ComponentEventHandler<MouseEvent> onClick;
        public event ComponentEventHandler<Event> onChange;
        public event ComponentEventHandler<Event> onInput;
        public event ComponentEventHandler<Event> onFocus;
        public event ComponentEventHandler<Event> onBlur;
        public event ComponentEventHandler<KeyboardEvent> onKeyUp;
        public event ComponentEventHandler<KeyboardEvent> onKeyDown;
        public event ComponentEventHandler<KeyboardEvent> onKeyPress;

        public THTML InnerElement { get; protected set; }
        public string Margin { get => InnerElement.style.margin; set => InnerElement.style.margin = value; }
        public string Padding { get => InnerElement.style.padding; set => InnerElement.style.padding = value; }
        
        public abstract HTMLElement Render();
        
        public virtual T OnClick(ComponentEventHandler<MouseEvent> onClick)
        {
            this.onClick += onClick;

            if(this is TextBlock textBlock)
            {
                textBlock.Cursor = "pointer";
            }

            return (T)this;
        }

        public virtual T OnChange(ComponentEventHandler<Event> onChange)
        {
            this.onChange += onChange;
            return (T)this;
        }

        public virtual T OnInput(ComponentEventHandler<Event> onInput)
        {
            this.onInput += onInput;
            return (T)this;
        }

        public virtual T OnFocus(ComponentEventHandler<Event> onFocus)
        {
            this.onFocus += onFocus;
            return (T)this;
        }

        public virtual T OnBlur(ComponentEventHandler<Event> onBlur)
        {
            this.onBlur += onBlur;
            return (T)this;
        }

        public virtual T OnKeyDown(ComponentEventHandler<KeyboardEvent> onKeyDown)
        {
            this.onKeyDown += onKeyDown;
            return (T)this;
        }

        public virtual T OnKeyUp(ComponentEventHandler<KeyboardEvent> onKeyUp)
        {
            this.onKeyUp += onKeyUp;
            return (T)this;
        }

        public virtual T OnKeyPress(ComponentEventHandler<KeyboardEvent> onKeyPress)
        {
            this.onKeyPress += onKeyPress;
            return (T)this;
        }

        protected void AttachClick() => InnerElement.addEventListener("click", e => RaiseOnClick(UncheckedCastTo<MouseEvent>(e)));

        protected void AttachChange() => InnerElement.addEventListener("change", s => RaiseOnChange(s));

        public void RaiseOnClick(MouseEvent ev) => onClick?.Invoke((T)this, ev);

        public void RaiseOnChange(Event ev) => onChange?.Invoke((T)this, ev);

        protected void AttachInput() => InnerElement.addEventListener("input", ev => RaiseOnInput(ev));

        protected void AttachKeys()
        {
            InnerElement.addEventListener("keypress", ev => RaiseOnKeyPress(UncheckedCastTo<KeyboardEvent>(ev)));
            InnerElement.addEventListener("keydown", ev => RaiseOnKeyDown(UncheckedCastTo<KeyboardEvent>(ev)));
            InnerElement.addEventListener("keyup", ev => RaiseOnKeyUp(UncheckedCastTo<KeyboardEvent>(ev)));
        }

        protected void AttachFocus() => InnerElement.addEventListener("focus", s => RaiseOnFocus(s));

        protected void AttachBlur() => InnerElement.addEventListener("blur", s => RaiseOnBlur(s));
        
        protected void RaiseOnInput(Event ev) => onInput?.Invoke((T)this, ev);

        protected void RaiseOnKeyDown(KeyboardEvent ev) => onKeyDown?.Invoke((T)this, ev);

        protected void RaiseOnKeyUp(KeyboardEvent ev) => onKeyUp?.Invoke((T)this, ev);

        protected void RaiseOnKeyPress(KeyboardEvent ev) => onKeyPress?.Invoke((T)this, ev);

        private void RaiseOnFocus(Event ev) => onFocus?.Invoke((T)this, ev);

        private void RaiseOnBlur(Event ev) =>  onBlur?.Invoke((T)this, ev);

        /// <summary>
        /// This should be used when you're sure that the object is of type T at runtime but we don't have that information available to the compiler and we don't the runtime to perform any checks
        /// </summary>
        [Template("{value}")]
        private extern static TTarget UncheckedCastTo<TTarget>(object value);
    }
}