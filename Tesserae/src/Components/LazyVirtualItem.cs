using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.LazyVirtualItem")]
    public class LazyVirtualItem : IComponent
    {
        private readonly HTMLElement _innerElement;
        private readonly HTMLElement _component;
        private bool _isRendered;

        public LazyVirtualItem(IComponent component, UnitSize height)
        {
            _component = component.Render();
            _innerElement = DIV();
            _innerElement.style.height = height.ToString();
            _innerElement.style.width  = "100%";
            _innerElement.style.overflow = "hidden";
            _innerElement.style.boxSizing = "border-box";
        }

        public HTMLElement Render() => _innerElement;

        public void UpdateVisibility(bool isVisible)
        {
            if (isVisible)
            {
                if (!_isRendered)
                {
                    _isRendered = true;
                    _innerElement.appendChild(_component);
                }

                _component.style.display = "";
            }
            else
            {
                _component.style.display = "none";
            }
        }
    }
}
