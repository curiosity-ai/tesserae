using System;
using System.Collections.Generic;
using System.Linq;
using static Retyped.dom;

namespace Tesserae
{
    public class ComponentCache<TComponent> : ComponentCacheBase<TComponent>
        where TComponent : class
    {
        private readonly Func<(int Key, TComponent Component), HTMLElement> _createComponentExpression;
        private readonly List<(int Key, HTMLElement HtmlElement)> _componentCache;

        public ComponentCache(
            Func<(int Key, TComponent Component), HTMLElement> createComponentExpression)
        {
            _createComponentExpression =
                createComponentExpression ?? throw new ArgumentNullException(nameof(createComponentExpression));

            _componentCache = new List<(int Key, HTMLElement HtmlElement)>();
        }

        public ComponentCache<TComponent> AddComponents(IEnumerable<TComponent> components)
        {
            AddToComponents(components);

            return this;
        }

        public HTMLElement RetrieveComponentFromCache(int keyOfComponentToRetrieve)
        {
            var cachedComponent =
                _componentCache.SingleOrDefault(component => component.Key == keyOfComponentToRetrieve);

            if (cachedComponent.HtmlElement != null)
            {
                console.log($"Retrieved component {keyOfComponentToRetrieve} from cache");
                return cachedComponent.HtmlElement;
            }

            console.log($"Adding component {keyOfComponentToRetrieve} to cache");

            var componentAndKey = Components.SingleOrDefault(component => component.Key == keyOfComponentToRetrieve);

            if (componentAndKey.Component == null)
            {
                throw new
                    InvalidOperationException($"Can not locate component with a key of {keyOfComponentToRetrieve}");
            }

            var htmlElement = _createComponentExpression(componentAndKey);

            _componentCache.Add((keyOfComponentToRetrieve, htmlElement));

            return htmlElement;
        }

        public IEnumerable<HTMLElement> RetrieveComponentsFromCache(IEnumerable<int> rangeOfComponentsToRetrieve)
        {
            return rangeOfComponentsToRetrieve.Select(RetrieveComponentFromCache);
        }

        public IEnumerable<HTMLElement> RetrieveAllComponentsFromCache()
        {
            return Enumerable.Range(1, Components.Count).Select(RetrieveComponentFromCache);
        }

        public void SortComponents()
        {
        }

        public ComponentCache<TComponent> Clear()
        {
            Components.Clear();
            _componentCache.Clear();

            return this;
        }

    }
}
