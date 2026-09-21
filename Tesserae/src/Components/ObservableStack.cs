using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A list container that keeps a <see cref="Stack"/> in sync with an observable list of items by
    /// performing a minimal, reference-keyed diff (common-prefix / common-suffix matching). Only the
    /// rows whose backing item actually changed are removed or re-created; rows whose item is
    /// unchanged keep their existing rendered <see cref="IComponent"/>, and therefore their DOM
    /// identity, focus and scroll state. This avoids the full subtree rebuild that re-rendering the
    /// whole list (e.g. with Defer) would cause.
    ///
    /// Items are matched by reference (<see cref="object.ReferenceEquals(object, object)"/>), so the
    /// observable must surface the same item instances across changes for the rows that should be
    /// preserved. A matched row keeps its existing component rather than being rebuilt, so per-row
    /// content is expected to refresh through the component's own observation rather than re-creation.
    /// </summary>
    /// <remarks>
    /// This is the reference-keyed counterpart to <see cref="KeyedObservableStack"/>. Use
    /// <see cref="ObservableStack{T}"/> when you have a list of data models with stable instance
    /// identity (matched by reference), want each row built lazily from a factory, and want matched
    /// rows preserved across updates. Use <see cref="KeyedObservableStack"/> instead when the source
    /// surfaces logically-equal-but-different instances that must still be matched (it keys pre-built
    /// <see cref="IComponentWithID"/> components by a stable string key plus a content hash), or when
    /// you need reordering or content-hash-driven re-rendering. <see cref="ObservableStack{T}"/> only
    /// diffs a common prefix/suffix, so a reorder of interior rows rebuilds that span, whereas
    /// <see cref="KeyedObservableStack"/> handles arbitrary reorders.
    /// </remarks>
    [Transpose.Name("tss.ObservableStack")]
    public sealed class ObservableStack<T> : IComponent where T : class
    {
        private readonly Stack                                          _host;
        private readonly Func<T, IComponent>                            _renderItem;
        private readonly List<Row>                                      _rows = new List<Row>();
        private readonly IObservable<IReadOnlyList<T>>                  _source;
        private readonly ObservableEvent.ValueChanged<IReadOnlyList<T>> _handler;

        /// <summary>
        /// Creates a list that reconciles <paramref name="source"/> into <paramref name="host"/>.
        /// </summary>
        /// <param name="source">The observable list of items to render. Reconciliation runs immediately with the current value and on every future change.</param>
        /// <param name="renderItem">Factory invoked once per newly-inserted item to build its component.</param>
        /// <param name="host">The Stack the rows are rendered into. Defaults to a vertical Stack.</param>
        public ObservableStack(IObservable<IReadOnlyList<T>> source, Func<T, IComponent> renderItem, Stack host = null)
        {
            _source     = source     ?? throw new ArgumentNullException(nameof(source));
            _renderItem = renderItem ?? throw new ArgumentNullException(nameof(renderItem));
            _host       = host ?? VStack();

            _handler = Reconcile;
            _source.Observe(_handler);

            DomObserver.WhenRemoved(_host.Render(), () => _source.StopObserving(_handler));
        }

        /// <summary>
        /// Renders the host element.
        /// </summary>
        public HTMLElement Render() => _host.Render();

        private void Reconcile(IReadOnlyList<T> next)
        {
            var selectedElement = document.activeElement;
            if (selectedElement is object && !_host.InnerElement.contains(selectedElement))
            {
                selectedElement = null;
            }

            if (next is null)
            {
                next = Array.Empty<T>();
            }

            int oldN = _rows.Count;
            int newN = next.Count;

            int p = 0;

            while (p < oldN && p < newN && ReferenceEquals(_rows[p].Item, next[p]))
            {
                p++;
            }

            int s = 0;

            while (s < oldN - p && s < newN - p && ReferenceEquals(_rows[oldN - 1 - s].Item, next[newN - 1 - s]))
            {
                s++;
            }

            int removeCount = oldN - s - p;
            int insertCount = newN - s - p;

            for (int i = 0; i < removeCount; i++)
            {
                _host.Remove(_rows[p].Component);
                _rows.RemoveAt(p);
            }

            IComponent insertBefore = (p < _rows.Count)
                ? _rows[p].Component
                : null;

            for (int k = 0; k < insertCount; k++)
            {
                int        newIndex  = p + k;
                T          item      = next[newIndex];
                IComponent component = _renderItem(item);

                if (insertBefore != null)
                {
                    _host.InsertBefore(component, insertBefore);
                }
                else
                {
                    _host.Add(component);
                }

                _rows.Insert(newIndex, new Row(item, component));
            }

            if (selectedElement is object && _host.InnerElement.contains(selectedElement))
            {
                selectedElement.As<HTMLElement>().focus();
            }
        }

        private readonly struct Row
        {
            public Row(T item, IComponent component)
            {
                Item      = item;
                Component = component;
            }

            public T          Item      { get; }
            public IComponent Component { get; }
        }
    }
}
