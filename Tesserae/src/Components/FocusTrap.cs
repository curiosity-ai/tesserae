using System;
using Transpose;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A surface that traps keyboard focus inside its content while open, used by modals, dialogs and panels.
    /// </summary>
    [Transpose.Name("tss.FocusTrap")]
    public sealed class FocusTrap : IComponent
    {
        private readonly HTMLElement _root;
        private Action _onEscape;
        private Action _onEnter;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public FocusTrap(IComponent child)
        {
            _root = child.Render();
            AttachFocusTrap();
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public FocusTrap(HTMLElement element)
        {
            _root = element;
            AttachFocusTrap();
        }

        /// <summary>
        /// Configures the trap escape on the component.
        /// </summary>
        public FocusTrap TrapEscape(Action onEscape)
        {
            _onEscape = onEscape;
            return this;
        }

        /// <summary>
        /// Configures the trap enter on the component.
        /// </summary>
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
                    var tabbable = Tabbable();
                    if (tabbable.Count == 0) return;

                    var firstElement = tabbable[0];
                    var lastElement  = tabbable[tabbable.Count - 1];
                    var active       = document.activeElement as HTMLElement;

                    // The focus is on the surface itself (focused programmatically as it opened, tabindex -1)
                    // or on something the browser can tab to but this list leaves out: the browser's own next
                    // stop could be outside the trap, so the trap picks one inside.
                    var outsideList = active is null || !tabbable.Contains(active);

                    if (ke.shiftKey)
                    {
                        if (active == firstElement || outsideList)
                        {
                            lastElement.focus();
                            ke.preventDefault();
                        }
                    }
                    else
                    {
                        if (active == lastElement || outsideList)
                        {
                            firstElement.focus();
                            ke.preventDefault();
                        }
                    }
                }
            });
        }

        // What Tab can actually reach inside the trap: no tabindex -1 (an inert copy, a roving row that is not
        // the current one), nothing disabled, nothing without a box (display: none) or hidden from sight.
        private System.Collections.Generic.List<HTMLElement> Tabbable()
        {
            var candidates = _root.querySelectorAll("a[href], button, textarea, input, select, iframe, summary, [contenteditable], [tabindex]");
            var result     = new System.Collections.Generic.List<HTMLElement>();

            for (uint i = 0; i < candidates.length; i++)
            {
                var element = (HTMLElement)candidates[i];

                if (element.tabIndex < 0) continue;
                if (element.hasAttribute("disabled")) continue;
                if (element.closest("[inert]") is object) continue;
                if (element.getClientRects().As<DOMRectList>().length == 0) continue;
                if (window.getComputedStyle(element).visibility == "hidden") continue;

                result.Add(element);
            }

            return result;
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
