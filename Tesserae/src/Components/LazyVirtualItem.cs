using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.LazyVirtualItem")]
    public class LazyVirtualItem : IComponent
    {
        private readonly HTMLElement _innerElement;
        private readonly Func<IComponent> _componentFactory;
        private IComponent _component;
        private bool _isRendered;

        public LazyVirtualItem(Func<IComponent> componentFactory, UnitSize height)
        {
            _componentFactory = componentFactory;
            _innerElement = DIV();
            _innerElement.style.height = height.ToString();
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
                    if (_component == null)
                    {
                        _component = _componentFactory();
                        _innerElement.appendChild(_component.Render());
                    }
                    else
                    {
                        var el = _component.Render();
                        el.style.display = "";
                    }
                }
            }
            else
            {
                if (_isRendered)
                {
                    _isRendered = false;
                    if (_component != null)
                    {
                        var el = _component.Render();
                        el.style.display = "none";
                    }
                }
            }
        }
    }
}
