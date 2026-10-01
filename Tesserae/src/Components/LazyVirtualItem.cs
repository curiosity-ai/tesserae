using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Shows only the <see cref="LazyVirtualItem"/>s near the visible part of a scrolling container,
    /// for the fixed-height virtualisation of <see cref="SearchableList{T}"/> and <see cref="SearchableGroupedList{T}"/>.
    /// </summary>
    [Transpose.Name("tss.LazyVirtualWindow")]
    internal sealed class LazyVirtualWindow
    {
        private readonly Func<HTMLElement> _container;
        private readonly Func<UnitSize>    _itemHeight;
        private readonly List<LazyVirtualItem> _items;
        private double _timeout = 0;
        private double _viewportMinTop = 0;
        private double _viewportMaxTop = 0;

        public LazyVirtualWindow(Func<HTMLElement> container, Func<UnitSize> itemHeight, List<LazyVirtualItem> items)
        {
            _container  = container;
            _itemHeight = itemHeight;
            _items      = items;
        }

        public void Recompute()
        {
            window.clearTimeout(_timeout);

            var container = _container();
            double scrollTop = container.parentElement.scrollTop;
            if (scrollTop < _viewportMinTop || scrollTop > _viewportMaxTop)
            {
                RecomputeInner();
            }
            else
            {
                _timeout = window.setTimeout((_) => RecomputeInner(), 50);
            }
        }

        private void RecomputeInner()
        {
            var itemHeight = _itemHeight();
            if (itemHeight is null || _items.Count == 0) return;
            var container = _container();
            double scrollTop = container.parentElement.scrollTop;
            double containerHeight = container.parentElement.clientHeight;
            if (containerHeight == 0) return;

            // We use the fixed height to calculate visible indices
            double itemH = itemHeight.Size;
            if (itemH <= 0) itemH = 1; // Prevent division by zero

            int firstVisibleIndex = (int)(scrollTop / itemH);
            int visibleCount = (int)(containerHeight / itemH) + 1;

            // Add overscan (e.g., 1x container height)
            int overscan = visibleCount;
            int startIndex = Math.Max(0, firstVisibleIndex - overscan);
            int endIndex = Math.Min(_items.Count - 1, firstVisibleIndex + visibleCount + overscan);

            for (int i = 0; i < _items.Count; i++)
            {
                bool isVisible = (i >= startIndex && i <= endIndex);
                _items[i].UpdateVisibility(isVisible);
            }

            _viewportMinTop = startIndex * itemH;
            _viewportMaxTop = endIndex   * itemH;
        }
    }
}
