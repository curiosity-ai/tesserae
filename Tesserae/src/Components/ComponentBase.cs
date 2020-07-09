using H5;
using static H5.Core.dom;

namespace Tesserae.Components
{
    public abstract class ComponentBase<T, THTML> : IComponent, IHasMarginPadding where T : ComponentBase<T, THTML> where THTML : HTMLElement
    {
        protected event ComponentEventHandler<T, MouseEvent> Clicked;
        protected event ComponentEventHandler<T, Event> Changed;
        protected event ComponentEventHandler<T, Event> InputUpdated;
        protected event ComponentEventHandler<T, Event> ReceivedFocus;
        protected event ComponentEventHandler<T, Event> LostFocus;
        protected event ComponentEventHandler<T, KeyboardEvent> KeyPushedDown;
        protected event ComponentEventHandler<T, KeyboardEvent> KeyReleased;
        protected event ComponentEventHandler<T, KeyboardEvent> KeyFullyPressed;

        public THTML InnerElement { get; protected set; }
        public string Margin { get => InnerElement.style.margin; set => InnerElement.style.margin = value; }
        public string Padding { get => InnerElement.style.padding; set => InnerElement.style.padding = value; }
        
        public abstract HTMLElement Render();
        
        public virtual T OnClick(ComponentEventHandler<T, MouseEvent> onClick)
        {
            Clicked += onClick;

            if (this is TextBlock textBlock)
                textBlock.Cursor = "pointer";

            return (T)this;
        }

        public virtual T OnChange(ComponentEventHandler<T, Event> onChange)
        {
            Changed += onChange;
            return (T)this;
        }

        public virtual T OnInput(ComponentEventHandler<T, Event> onInput)
        {
            InputUpdated += onInput;
            return (T)this;
        }

        public virtual T OnFocus(ComponentEventHandler<T, Event> onFocus)
        {
            ReceivedFocus += onFocus;
            return (T)this;
        }

        public virtual T OnBlur(ComponentEventHandler<T, Event> onBlur)
        {
            LostFocus += onBlur;
            return (T)this;
        }

        public virtual T OnKeyDown(ComponentEventHandler<T, KeyboardEvent> onKeyDown)
        {
            KeyPushedDown += onKeyDown;
            return (T)this;
        }

        public virtual T OnKeyUp(ComponentEventHandler<T, KeyboardEvent> onKeyUp)
        {
            KeyReleased += onKeyUp;
            return (T)this;
        }

        public virtual T OnKeyPress(ComponentEventHandler<T, KeyboardEvent> onKeyPress)
        {
            KeyFullyPressed += onKeyPress;
            return (T)this;
        }

        protected void AttachClick() => InnerElement.addEventListener("click", e => RaiseOnClick(UncheckedCastTo<MouseEvent>(e)));

        protected void AttachChange() => InnerElement.addEventListener("change", s => RaiseOnChange(s));

        // 2020-06-30 DWR: This was previously public, not protected, but nothing outside of a component itself should be faking events like this
        protected void RaiseOnClick(MouseEvent ev) => Clicked?.Invoke((T)this, ev);

        // 2020-06-30 DWR: This was previously public, not protected, but nothing outside of a component itself should be faking events like this
        protected void RaiseOnChange(Event ev) => Changed?.Invoke((T)this, ev);

        protected void AttachInput() => InnerElement.addEventListener("input", ev => RaiseOnInput(ev));

        protected void AttachKeys()
        {
            InnerElement.addEventListener("keypress", ev => RaiseOnKeyPress(UncheckedCastTo<KeyboardEvent>(ev)));
            InnerElement.addEventListener("keydown", ev => RaiseOnKeyDown(UncheckedCastTo<KeyboardEvent>(ev)));
            InnerElement.addEventListener("keyup", ev => RaiseOnKeyUp(UncheckedCastTo<KeyboardEvent>(ev)));
        }

        protected void AttachFocus() => InnerElement.addEventListener("focus", s => RaiseOnFocus(s));

        protected void AttachBlur() => InnerElement.addEventListener("blur", s => RaiseOnBlur(s));
        
        protected void RaiseOnInput(Event ev) => InputUpdated?.Invoke((T)this, ev);

        protected void RaiseOnKeyDown(KeyboardEvent ev) => KeyPushedDown?.Invoke((T)this, ev);

        protected void RaiseOnKeyUp(KeyboardEvent ev) => KeyReleased?.Invoke((T)this, ev);

        protected void RaiseOnKeyPress(KeyboardEvent ev) => KeyFullyPressed?.Invoke((T)this, ev);

        private void RaiseOnFocus(Event ev) => ReceivedFocus?.Invoke((T)this, ev);

        private void RaiseOnBlur(Event ev) =>  LostFocus?.Invoke((T)this, ev);

        /// <summary>
        /// This should be used when you're sure that the object is of type T at runtime but we don't have that information available to the compiler and we don't the runtime to perform any checks
        /// </summary>
        [Template("{value}")]
        private extern static TTarget UncheckedCastTo<TTarget>(object value);
    }
}