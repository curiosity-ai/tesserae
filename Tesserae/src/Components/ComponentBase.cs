using System;
using static Retyped.dom;

namespace Tesserae.Components
{
    public abstract class ComponentBase<T, THTML> : IComponent where T : ComponentBase<T, THTML> where THTML : HTMLElement
    {
        #region Properties

        public THTML InnerElement { get; protected set; }

        #endregion

        #region Events

        public event EventHandler<T> OnClick;
        public event EventHandler<T> OnChange;
        public event EventHandler<T> OnInput;
        public event EventHandler<T> OnFocus;
        public event EventHandler<T> OnBlur;

        #endregion

        #region Methods

        public abstract HTMLElement Render();

        protected void AttachClick()
        {
            InnerElement.addEventListener("click", (s) => OnClick?.Invoke(s, (T)this));
        }

        protected void AttachChange()
        {
            InnerElement.addEventListener("change", (s) => RaiseOnChange(s));
        }

        protected void RaiseOnChange(object s)
        {
            OnChange?.Invoke(s, (T)this);
        }

        protected void AttachInput()
        {
            InnerElement.addEventListener("input", (s) => RaiseOnInput(s));
        }

        protected void RaiseOnInput(object s)
        {
            OnInput?.Invoke(s, (T)this);
        }

        protected void AttachFocus()
        {
            InnerElement.addEventListener("focus", (s) => OnFocus?.Invoke(s, (T)this));
        }

        protected void AttachBlur()
        {
            InnerElement.addEventListener("blur", (s) => OnBlur?.Invoke(s, (T)this));
        }

        #endregion

        #region Fluent

        public T OnClicked(EventHandler<T> onClick)
        {
            OnClick += onClick;
            return (T)this;
        }

        public T OnChanged(EventHandler<T> onChange)
        {
            OnChange += onChange;
            return (T)this;
        }

        public T OnInputed(EventHandler<T> onInput)
        {
            OnInput += onInput;
            return (T)this;
        }

        public T OnFocused(EventHandler<T> onFocus)
        {
            OnFocus += onFocus;
            return (T)this;
        }

        public T OnBlured(EventHandler<T> onBlur)
        {
            OnBlur += onBlur;
            return (T)this;
        }

        #endregion
    }
}

