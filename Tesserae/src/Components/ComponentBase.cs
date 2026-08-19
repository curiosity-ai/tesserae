using Transpose;
using static Transpose.Core.dom;
using Transpose.Core;
using System;

namespace Tesserae
{


    /// <summary>
    /// Base class for every Tesserae component. Provides DOM event wiring, click / focus / change events, ARIA and
    /// margin/padding support.
    /// </summary>
    [Transpose.Name("tss.CB")]
    public abstract class ComponentBase<T, THTML> : IComponent, IHasClickHandler, IHasMarginPadding, IAccessibility where T : ComponentBase<T, THTML> where THTML : HTMLElement
    {
        // The DOM listener behind each of these is installed the first time something subscribes,
        // not when the component is built. A component that calls AttachClick() and is never given a
        // handler — most of the TextBlocks in a list, every Icon in a table — used to pay for three
        // listeners and three closures each, which on a page with a few thousand components is a
        // measurable share of both build time and retained memory. The Attach* methods still decide
        // *which* events a component participates in; they now only record the intent, and the
        // Ensure* methods do the wiring once a handler actually shows up.
        //
        // These are plain delegate fields with Subscribe*/Unsubscribe* helpers rather than C# events:
        // the Transpose compiler lowers `SomeEvent += handler` straight onto the backing field and
        // never calls a custom add accessor, so an event would silently skip the wiring above.
        private ComponentEventHandler<T, MouseEvent>     _clicked;
        private ComponentEventHandler<T, MouseEvent>     _mouseOver;
        private ComponentEventHandler<T, MouseEvent>     _mouseOut;
        private ComponentEventHandler<T, MouseEvent>     _contextMenu;
        private ComponentEventHandler<T, Event>          _changed;
        private ComponentEventHandler<T, ClipboardEvent> _pasted;
        private ComponentEventHandler<T, Event>          _inputUpdated;
        private ComponentEventHandler<T, Event>          _receivedFocus;
        private ComponentEventHandler<T, Event>          _lostFocus;
        private ComponentEventHandler<T, KeyboardEvent>  _keyDown;
        private ComponentEventHandler<T, KeyboardEvent>  _keyUp;
        private ComponentEventHandler<T, KeyboardEvent>  _keyPress;

        private bool _wantsClick, _wantsContextMenu, _wantsChange, _wantsInput, _wantsKeys, _wantsFocus, _wantsBlur;

        private bool _hasClickListener,   _hasMouseOverListener, _hasMouseOutListener, _hasContextMenuListener;
        private bool _hasChangeListener,  _hasInputListener,     _hasFocusListener,    _hasBlurListener;
        private bool _hasKeyDownListener, _hasKeyUpListener,     _hasKeyPressListener, _hasPasteListener;

        /// <summary>Subscribes to the click event, wiring the DOM listener on first use.</summary>
        protected void SubscribeClicked(ComponentEventHandler<T, MouseEvent> handler)
        {
            _clicked += handler;
            EnsureClickListener();
        }

        /// <summary>Unsubscribes from the click event.</summary>
        protected void UnsubscribeClicked(ComponentEventHandler<T, MouseEvent> handler) => _clicked -= handler;

        /// <summary>Subscribes to the mouse-over event, wiring the DOM listener on first use.</summary>
        protected void SubscribeMouseOver(ComponentEventHandler<T, MouseEvent> handler)
        {
            _mouseOver += handler;
            EnsureMouseOverListener();
        }

        /// <summary>Unsubscribes from the mouse-over event.</summary>
        protected void UnsubscribeMouseOver(ComponentEventHandler<T, MouseEvent> handler) => _mouseOver -= handler;

        /// <summary>Subscribes to the mouse-out event, wiring the DOM listener on first use.</summary>
        protected void SubscribeMouseOut(ComponentEventHandler<T, MouseEvent> handler)
        {
            _mouseOut += handler;
            EnsureMouseOutListener();
        }

        /// <summary>Unsubscribes from the mouse-out event.</summary>
        protected void UnsubscribeMouseOut(ComponentEventHandler<T, MouseEvent> handler) => _mouseOut -= handler;

        /// <summary>Subscribes to the context-menu event, wiring the DOM listener on first use.</summary>
        protected void SubscribeContextMenu(ComponentEventHandler<T, MouseEvent> handler)
        {
            _contextMenu += handler;
            EnsureContextMenuListener();
        }

        /// <summary>Unsubscribes from the context-menu event.</summary>
        protected void UnsubscribeContextMenu(ComponentEventHandler<T, MouseEvent> handler) => _contextMenu -= handler;

        /// <summary>Subscribes to the change event, wiring the DOM listener on first use.</summary>
        protected void SubscribeChanged(ComponentEventHandler<T, Event> handler)
        {
            _changed += handler;
            EnsureChangeListener();
        }

        /// <summary>Subscribes to the paste event, wiring the DOM listener on first use.</summary>
        protected void SubscribePasted(ComponentEventHandler<T, ClipboardEvent> handler)
        {
            _pasted += handler;
            EnsurePasteListener();
        }

        /// <summary>Subscribes to the input event, wiring the DOM listener on first use.</summary>
        protected void SubscribeInputUpdated(ComponentEventHandler<T, Event> handler)
        {
            _inputUpdated += handler;
            EnsureInputListener();
        }

        /// <summary>Subscribes to the focus event, wiring the DOM listener on first use.</summary>
        protected void SubscribeReceivedFocus(ComponentEventHandler<T, Event> handler)
        {
            _receivedFocus += handler;
            EnsureFocusListener();
        }

        /// <summary>Subscribes to the blur event, wiring the DOM listener on first use.</summary>
        protected void SubscribeLostFocus(ComponentEventHandler<T, Event> handler)
        {
            _lostFocus += handler;
            EnsureBlurListener();
        }

        /// <summary>Subscribes to the key-down event, wiring the DOM listener on first use.</summary>
        protected void SubscribeKeyDown(ComponentEventHandler<T, KeyboardEvent> handler)
        {
            _keyDown += handler;
            EnsureKeyDownListener();
        }

        /// <summary>Subscribes to the key-up event, wiring the DOM listener on first use.</summary>
        protected void SubscribeKeyUp(ComponentEventHandler<T, KeyboardEvent> handler)
        {
            _keyUp += handler;
            EnsureKeyUpListener();
        }

        /// <summary>Subscribes to the key-press event, wiring the DOM listener on first use.</summary>
        protected void SubscribeKeyPress(ComponentEventHandler<T, KeyboardEvent> handler)
        {
            _keyPress += handler;
            EnsureKeyPressListener();
        }

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
            if (_clicked != null && clearPrevious)
            {
                foreach (Delegate d in _clicked.GetInvocationList())
                {
                    UnsubscribeClicked((ComponentEventHandler<T, MouseEvent>)d);
                }
            }

            SubscribeClicked(onClick);

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
            if (_mouseOver != null && clearPrevious)
            {
                foreach (Delegate d in _mouseOver.GetInvocationList())
                {
                    UnsubscribeMouseOver((ComponentEventHandler<T, MouseEvent>)d);
                }

                foreach (Delegate d in _mouseOut.GetInvocationList())
                {
                    UnsubscribeMouseOut((ComponentEventHandler<T, MouseEvent>)d);
                }
            }

            SubscribeMouseOver(onEnter);

            if (onLeave is object)
            {
                SubscribeMouseOut(onLeave);
            }

            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the context menu event fires.
        /// </summary>
        public virtual T OnContextMenu(ComponentEventHandler<T, MouseEvent> onContextMenu, bool clearPrevious = true)
        {
            if (_contextMenu != null && clearPrevious)
            {
                foreach (Delegate d in _contextMenu.GetInvocationList())
                {
                    UnsubscribeContextMenu((ComponentEventHandler<T, MouseEvent>)d);
                }
            }

            SubscribeContextMenu(onContextMenu);

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
            SubscribeChanged(onChange);
            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the input event fires.
        /// </summary>
        public virtual T OnInput(ComponentEventHandler<T, Event> onInput)
        {
            SubscribeInputUpdated(onInput);
            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the focus event fires.
        /// </summary>
        public virtual T OnFocus(ComponentEventHandler<T, Event> onFocus)
        {
            SubscribeReceivedFocus(onFocus);
            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the blur event fires.
        /// </summary>
        public virtual T OnBlur(ComponentEventHandler<T, Event> onBlur)
        {
            SubscribeLostFocus(onBlur);
            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the key down event fires.
        /// </summary>
        public virtual T OnKeyDown(ComponentEventHandler<T, KeyboardEvent> onKeyDown)
        {
            SubscribeKeyDown(onKeyDown);
            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the key up event fires.
        /// </summary>
        public virtual T OnKeyUp(ComponentEventHandler<T, KeyboardEvent> onKeyUp)
        {
            SubscribeKeyUp(onKeyUp);
            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the key press event fires.
        /// </summary>
        public virtual T OnKeyPress(ComponentEventHandler<T, KeyboardEvent> onKeyPress)
        {
            SubscribeKeyPress(onKeyPress);
            return (T)this;
        }

        /// <summary>
        /// Registers a callback invoked when the pasted event fires.
        /// </summary>
        public virtual T OnPasted(ComponentEventHandler<T, ClipboardEvent> onPasted)
        {
            SubscribePasted(onPasted);
            return (T)this;
        }

        protected void AttachClick()
        {
            _wantsClick = true;
            EnsureClickListener();
            EnsureMouseOverListener();
            EnsureMouseOutListener();
        }

        protected void AttachContextMenu()
        {
            _wantsContextMenu = true;
            EnsureContextMenuListener();
        }

        protected void AttachChange()
        {
            _wantsChange = true;
            EnsureChangeListener();
        }

        private void EnsureClickListener()
        {
            if (_hasClickListener || !_wantsClick || _clicked is null) return;
            _hasClickListener = true;

            InnerElement.addEventListener("click", e =>
            {
                var mouseEvent = e.As<MouseEvent>();

                //A component that is a link - or that sits inside one, the way a Sidebar button sits inside
                //its anchor - answers Ctrl/Cmd-click and Shift-click with the browser's own "open in a new
                //tab / new window" rather than with its click handler. The handler is what would otherwise
                //stop the event (most of them do, to keep the press from counting twice), and the new tab
                //would never open.
                if (UI.IsModifiedLinkClick(InnerElement, mouseEvent)) return;

                RaiseOnClick(mouseEvent);
            });
        }

        private void EnsureMouseOverListener()
        {
            if (_hasMouseOverListener || !_wantsClick || _mouseOver is null) return;
            _hasMouseOverListener = true;
            InnerElement.addEventListener("mouseover", e => RaiseOnMouseOver(e.As<MouseEvent>()));
        }

        private void EnsureMouseOutListener()
        {
            if (_hasMouseOutListener || !_wantsClick || _mouseOut is null) return;
            _hasMouseOutListener = true;
            InnerElement.addEventListener("mouseout", e => RaiseOnMouseOut(e.As<MouseEvent>()));
        }

        private void EnsureContextMenuListener()
        {
            if (_hasContextMenuListener || !_wantsContextMenu || _contextMenu is null) return;
            _hasContextMenuListener = true;
            InnerElement.addEventListener("contextmenu", e => RaiseOnContextMenu(e.As<MouseEvent>()));
        }

        private void EnsureChangeListener()
        {
            if (_hasChangeListener || !_wantsChange || _changed is null) return;
            _hasChangeListener = true;
            InnerElement.addEventListener("change", s => RaiseOnChange(s));
        }

        private void EnsureInputListener()
        {
            if (_hasInputListener || !_wantsInput || _inputUpdated is null) return;
            _hasInputListener = true;
            InnerElement.addEventListener("input", ev => RaiseOnInput(ev));
        }

        private void EnsureFocusListener()
        {
            if (_hasFocusListener || !_wantsFocus || _receivedFocus is null) return;
            _hasFocusListener = true;
            InnerElement.addEventListener("focus", s => RaiseOnFocus(s));
        }

        private void EnsureBlurListener()
        {
            if (_hasBlurListener || !_wantsBlur || _lostFocus is null) return;
            _hasBlurListener = true;
            InnerElement.addEventListener("blur", s => RaiseOnBlur(s));
        }

        private void EnsureKeyDownListener()
        {
            if (_hasKeyDownListener || !_wantsKeys || _keyDown is null) return;
            _hasKeyDownListener = true;
            InnerElement.addEventListener("keydown", ev => RaiseOnKeyDown(ev.As<KeyboardEvent>()));
        }

        private void EnsureKeyUpListener()
        {
            if (_hasKeyUpListener || !_wantsKeys || _keyUp is null) return;
            _hasKeyUpListener = true;
            InnerElement.addEventListener("keyup", ev => RaiseOnKeyUp(ev.As<KeyboardEvent>()));
        }

        private void EnsureKeyPressListener()
        {
            if (_hasKeyPressListener || !_wantsKeys || _keyPress is null) return;
            _hasKeyPressListener = true;
            InnerElement.addEventListener("keypress", ev => RaiseOnKeyPress(ev.As<KeyboardEvent>()));
        }

        private void EnsurePasteListener()
        {
            if (_hasPasteListener || !_wantsKeys || _pasted is null) return;
            _hasPasteListener = true;
            InnerElement.addEventListener("paste", ev => RaiseOnPaste(ev.As<ClipboardEvent>()));
        }

        /// <summary>
        /// Raises the on click event on the component.
        /// </summary>
        public void RaiseOnClick(MouseEvent     ev) => _clicked?.Invoke((T)this, ev);
        /// <summary>
        /// Raises the on mouse over event on the component.
        /// </summary>
        public void RaiseOnMouseOver(MouseEvent ev) => _mouseOver?.Invoke((T)this, ev);
        /// <summary>
        /// Raises the on mouse out event on the component.
        /// </summary>
        public void RaiseOnMouseOut(MouseEvent  ev) => _mouseOut?.Invoke((T)this, ev);

        /// <summary>
        /// Raises the on context menu event on the component.
        /// </summary>
        public void RaiseOnContextMenu(MouseEvent ev) => _contextMenu?.Invoke((T)this, ev);

        //Some controls won't change the underlying value till after this event. As we usually want the final value and not the previous state, we raise the event on a timer
        protected void RaiseOnChange(Event ev) => window.setTimeout((_) => _changed?.Invoke((T)this, ev), 1);

        protected void AttachInput()
        {
            _wantsInput = true;
            EnsureInputListener();
        }

        protected void AttachKeys()
        {
            _wantsKeys = true;
            EnsureKeyPressListener();
            EnsureKeyDownListener();
            EnsureKeyUpListener();
            EnsurePasteListener();
        }

        protected void AttachFocus()
        {
            _wantsFocus = true;
            EnsureFocusListener();
        }

        protected void AttachBlur()
        {
            _wantsBlur = true;
            EnsureBlurListener();
        }

        protected void RaiseOnPaste(ClipboardEvent ev) => _pasted?.Invoke((T)this, ev);
        protected void RaiseOnInput(Event          ev) => _inputUpdated?.Invoke((T)this, ev);

        protected void RaiseOnKeyDown(KeyboardEvent ev) => _keyDown?.Invoke((T)this, ev);

        protected void RaiseOnKeyUp(KeyboardEvent ev) => _keyUp?.Invoke((T)this, ev);

        protected void RaiseOnKeyPress(KeyboardEvent ev) => _keyPress?.Invoke((T)this, ev);

        private void RaiseOnFocus(Event ev) => _receivedFocus?.Invoke((T)this, ev);

        private void RaiseOnBlur(Event ev) => _lostFocus?.Invoke((T)this, ev);
    }
}