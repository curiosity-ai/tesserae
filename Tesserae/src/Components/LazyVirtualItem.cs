using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A placeholder used inside virtualised lists that defers building its real content until it scrolls into view.
    /// </summary>
    [Transpose.Name("tss.LazyVirtualItem")]
    public class LazyVirtualItem : IComponent
    {
        private readonly HTMLElement _innerElement;
        private readonly HTMLElement _component;
        private bool _isRendered;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public LazyVirtualItem(IComponent component, UnitSize height)
        {
            _component = component.Render();
            _innerElement = DIV();
            _innerElement.style.height = height.ToString();
            _innerElement.style.width  = "100%";
            _innerElement.style.overflow = "hidden";
            _innerElement.style.boxSizing = "border-box";
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _innerElement;

        /// <summary>
        /// Updates the visibility.
        /// </summary>
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
