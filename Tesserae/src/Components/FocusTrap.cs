using H5;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.FocusTrap")]
    public sealed class FocusTrap : IComponent
    {
        private readonly HTMLElement _root;

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

        private void AttachFocusTrap()
        {
            _root.addEventListener("keydown", ev =>
            {
                var ke = ev as KeyboardEvent;
                if (ke != null && ke.key == "Tab")
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

        public HTMLElement Render()
        {
            return _root;
        }
    }
}
