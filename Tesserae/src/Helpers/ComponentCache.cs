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

        private  List<(int Key, HTMLElement HtmlElement)> _componentCache;

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

        public IEnumerable<HTMLElement> RetrieveAllComponentsFromCache()
        {
            foreach (var componentAndKey in ComponentsAndKeys)
            {
                var cachedComponent =
                    _componentCache.SingleOrDefault(component => component.Key == componentAndKey.Key);

                if (cachedComponent.HtmlElement != null)
                {
                    yield return cachedComponent.HtmlElement;
                }
                else
                {
                    var htmlElement = _createComponentExpression(componentAndKey);

                    _componentCache.Add((componentAndKey.Key, htmlElement));

                    yield return htmlElement;
                }
            }
        }

        public ComponentCache<TComponent> SortComponents(Comparison<TComponent> comparison)
        {
            ComponentsAndKeys
                .Sort(
                    (componentAndKey, otherComponentAndKey)
                        => comparison(componentAndKey.Component, otherComponentAndKey.Component));

            return this;
        }

        public ComponentCache<TComponent> ReverseComponentOrder()
        {
            ComponentsAndKeys.Reverse();

            return this;
        }

        public ComponentCache<TComponent> Clear()
        {
            ComponentsAndKeys.Clear();
            _componentCache.Clear();

            return this;
        }

    }
}
