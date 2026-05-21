using System;
using H5;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A surface that traps keyboard focus inside its content while open, used by modals, dialogs and panels.
    /// </summary>
    [H5.Name("tss.FocusTrap")]
    public sealed class FocusTrap : IComponent
    {
        private readonly HTMLElement _root;
        private Action _onEscape;
        private Action _onEnter;

        public FocusTrap(IComponent child)
        {
            _root = child.Render();
            AttachFocusTrap();
        }

        public FocusTrap(HTMLElement element)
        {
            _root = element;
            AttachFocusTrap();
        }

        public FocusTrap TrapEscape(Action onEscape)
        {
            _onEscape = onEscape;
            return this;
        }

        public FocusTrap TrapEnter(Action onEnter)
        {
            _onEnter = onEnter;
            return this;
        }

        private void AttachFocusTrap()
        {
            _root.addEventListener("keydown", ev =>
            {
                var ke = ev as KeyboardEvent;
                if (ke == null) return;

                if (ke.key == "Escape" && _onEscape != null)
                {
                    _onEscape();
                    ke.preventDefault();
                    return;
                }

                if (ke.key == "Enter" && _onEnter != null)
                {
                    _onEnter();
                    ke.preventDefault();
                    return;
                }

                if (ke.key == "Tab")
                {
                    var focusableElements = _root.querySelectorAll("a[href], button:not(:disabled), textarea:not(:disabled), input:not(:disabled), select:not(:disabled), [tabindex]:not([tabindex='-1'])");
                    if (focusableElements.length == 0) return;

                    var firstElement = (HTMLElement)focusableElements[0];
                    var lastElement = (HTMLElement)focusableElements[focusableElements.length - 1];

                    if (ke.shiftKey)
                    {
                        if (document.activeElement == firstElement)
                        {
                            lastElement.focus();
                            ke.preventDefault();
                        }
                    }
                    else
                    {
                        if (document.activeElement == lastElement)
                        {
                            firstElement.focus();
                            ke.preventDefault();
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render()
        {
            return _root;
        }
    }
}
