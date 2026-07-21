using System;
using static Tesserae.UI;
using static Transpose.Core.dom;
using System.Collections.Generic;
using System.Linq;

namespace Tesserae
{
    /// <summary>
    /// A horizontal set of items where any items that do not fit are automatically collapsed into an overflow menu.
    /// </summary>
    [Transpose.Name("tss.OverflowSet")]
    public class OverflowSet : IComponent, IContainer<Breadcrumb, IComponent>
    {
        private readonly string         _expandIcon = UIcons.ArrowDown.ToString();
        private readonly HTMLElement    _childContainer;
        private readonly ResizeObserver _resizeObserver;
        private          int            _maximumItemsToDisplay = 10;
        private          int            _overflowIndex         = 0;
        private          bool           _cacheSizes;
        private          double         _cachedFullWidth      = 0;
        private          HTMLElement    _chevronToUseAsButton = null;

        private readonly Dictionary<HTMLElement, double> _cachedSizes = new Dictionary<HTMLElement, double>();

        /// <summary>
        /// Gets or sets the maximum items to display.
        /// </summary>
        public int MaximumItemsToDisplay
        {
            get => _maximumItemsToDisplay;
            set
            {
                _maximumItemsToDisplay = value;
                Recompute();
            }
        }

        /// <summary>
        /// Gets or sets the overflow index.
        /// </summary>
        public int OverflowIndex
        {
            get => _overflowIndex;
            set
            {
                _overflowIndex = value;
                Recompute();
            }
        }

        /// <summary>
        /// Returns a value indicating whether the component is small.
        /// </summary>
        public bool IsSmall
        {
            get => _childContainer.classList.contains("tss-small");
            set
            {
                if (value) { _childContainer.classList.add("tss-small"); }
                else { _childContainer.classList.remove("tss-small"); }
            }
        }


        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public OverflowSet()
        {
            _childContainer = Div(Att("tss-overflowset"));
            DomObserver.WhenMounted(_childContainer, Recompute);
            _resizeObserver = new ResizeObserver((entries, obs) => Recompute());
            _resizeObserver.observe(document.body);
        }

        private void Recompute()
        {
            int childElementCount = (int)_childContainer.childElementCount;
            if (childElementCount <= 1) return;

            if (_chevronToUseAsButton is object)
            {
                //Reset modified chevron if any
                _chevronToUseAsButton.classList.remove("las", _expandIcon, "tss-overflowset-opencolapsed");

                _chevronToUseAsButton.onclick = null;
                _chevronToUseAsButton         = null;
            }

            UpdateChildrenSizes();

            bool isChevron(HTMLElement e) => e.classList.contains("tss-overflowset-separator");

            var keep = new int[childElementCount];

            const int KEEP        = 2;
            const int COLLAPSE    = 1;
            const int NOTMEASURED = 0;

            if (_overflowIndex >= 0)
            {
                keep[0] = KEEP;

                for (int i = 0; i <= Math.Min(keep.Length - 1, ((_overflowIndex) * 2)); i++)
                {
                    keep[i] = KEEP;
                    int nextIndex = i + 1;

                    if ((nextIndex < _overflowIndex - 2) && nextIndex < childElementCount)
                    {
                        var child = (HTMLElement)_childContainer.children[(uint)nextIndex];

                        if (isChevron(child))
                        {
                            keep[i + 1] = KEEP;
                        }
                    }
                }
            }

            if (!keep.Any(k => k == KEEP))
            {
                keep[0] = KEEP;
            }

            keep[keep.Length - 1] = NOTMEASURED;

            var debt = _cachedFullWidth - _cachedSizes.Values.Sum() - 32;

            while (debt < 0)
            {
                var candidate = Array.LastIndexOf(keep, NOTMEASURED);

                if (candidate >= 0)
                {
                    keep[candidate] = COLLAPSE;
                    var child = (HTMLElement)_childContainer.children[(uint)candidate];
                    debt += _cachedSizes[child];
                }
                else
                {
                    break;
                }
            }

            var hidden = new List<HTMLElement>();

            for (uint i = 0; i < _childContainer.childElementCount; i++)
            {
                var child = (HTMLElement)_childContainer.children[i];

                if (keep[i] == COLLAPSE)
                {
                    if (_chevronToUseAsButton is null)
                    {
                        if (isChevron(child))
                        {
                            _chevronToUseAsButton = child;
                            continue; //Don't collapse this, instead keep for menu button
                        }
                        else if (i > 0)
                        {
                            //previous element is a chevron, so use it instead
                            _chevronToUseAsButton = (HTMLElement)_childContainer.children[i - 1];
                        }
                    }

                    if (!isChevron(child)) hidden.Add(child);
                    child.classList.add("tss-overflowset-collapse");
                }
                else
                {
                    child.classList.remove("tss-overflowset-collapse");
                }
            }


            IComponent clone(Node node)
            {
                var c = (HTMLElement)(node.cloneNode(true));
                c.classList.remove("tss-overflowset-collapse");
                return Raw(c);
            }

            if (_chevronToUseAsButton is object)
            {
                _chevronToUseAsButton.classList.add("las", _expandIcon, "tss-overflowset-opencolapsed");
                _chevronToUseAsButton.classList.remove("tss-overflowset-collapse");

                _chevronToUseAsButton.onclick = (e) =>
                {
                    StopEvent(e);
                    var clones = hidden.Select(element => ContextMenuItem(clone(element)).OnClick((s2, e2) => element.click())).ToArray();
                    ContextMenu().Items(clones).ShowFor(_chevronToUseAsButton);
                };
            }

        }

        /// <summary>
        /// Sets the justify-content css property for this set
        /// </summary>
        /// <param name="justify"></param>
        /// <returns></returns>
        public OverflowSet JustifyContent(ItemJustify justify)
        {
            _childContainer.style.justifyContent = justify.ToString();
            return this;
        }

        private void UpdateChildrenSizes()
        {
            if (!_cacheSizes)
            {
                _cachedSizes.Clear();

                for (uint i = 0; i < _childContainer.childElementCount; i++)
                {
                    var child = (HTMLElement)_childContainer.children[i];
                    child.classList.remove("tss-overflowset-collapse");
                }

                var rect = (DOMRect)_childContainer.getBoundingClientRect();
                _cachedFullWidth = rect.width;
            }


            foreach (HTMLElement child in _childContainer.children)
            {
                if (!_cachedSizes.ContainsKey(child))
                {
                    var childRect = (DOMRect)child.getBoundingClientRect();
                    _cachedSizes[child] = childRect.width;
                }
            }
        }

        /// <summary>
        /// Clears the component's current state.
        /// </summary>
        public void Clear()
        {
            ClearChildren(_childContainer);
        }

        /// <summary>
        /// Replaces an existing item with a new one.
        /// </summary>
        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            _childContainer.replaceChild(newComponent.Render(), oldComponent.Render());
        }

        /// <summary>
        /// Adds the given item to the component.
        /// </summary>
        public void Add(IComponent component)
        {
            if (_childContainer.childElementCount > 0)
            {
                _childContainer.appendChild(I(Att("tss-overflowset-separator")));
            }
            _childContainer.appendChild(component.Render());
        }

        /// <summary>
        /// Adds the given items to the component.
        /// </summary>
        public OverflowSet Items(params IComponent[] children)
        {
            children.ForEach(x => Add(x));
            return this;
        }

        /// <summary>
        /// Disables the size cache on the component.
        /// </summary>
        public OverflowSet DisableSizeCache()
        {
            _cacheSizes = false;
            return this;
        }

        /// <summary>
        /// Sets the overflow index of the component.
        /// </summary>
        public OverflowSet SetOverflowIndex(int i)
        {
            _overflowIndex = i;
            return this;
        }

        /// <summary>
        /// Renders the component at small size.
        /// </summary>
        public OverflowSet Small()
        {
            IsSmall = true;
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render()
        {
            return _childContainer;
        }
    }
}