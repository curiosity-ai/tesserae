using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae.Components;
using static Retyped.dom;

namespace Tesserae
{
    public sealed class ListPageCache
    {
        private readonly int _componentsPerPage;
        private readonly Func<int, HTMLElement> _createPageHtmlElementExpression;
        private readonly Func<KeyValuePair<int, IComponent>, HTMLElement> _afterComponentCreatedExpression;
        private readonly Dictionary<int, HTMLElement> _pageCache;
        private readonly List<List<KeyValuePair<int, IComponent>>> _pages;

        private Dictionary<int, IComponent> _components;

        public ListPageCache(
            int componentsPerPage,
            Func<int, HTMLElement> createPageHtmlElementExpression,
            Func<KeyValuePair<int, IComponent>, HTMLElement> afterComponentCreatedExpression)
        {
            if (componentsPerPage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(componentsPerPage));
            }

            _componentsPerPage = componentsPerPage;

            _createPageHtmlElementExpression =
                createPageHtmlElementExpression ??
                    throw new ArgumentNullException(nameof(createPageHtmlElementExpression));

            _afterComponentCreatedExpression =
                afterComponentCreatedExpression ??
                    throw new ArgumentNullException(nameof(afterComponentCreatedExpression));

            _pageCache  = new Dictionary<int, HTMLElement>();
            _pages      = new List<List<KeyValuePair<int, IComponent>>>();
            _components = new Dictionary<int, IComponent>();
        }

        public int ComponentsCount => _components.Count;

        public int PagesCount      => _pages.Count;

        public ListPageCache AddComponents(IEnumerable<IComponent> components)
        {
            var componentNumberToPageFrom = AddToComponents(components);

            AddToPages(componentNumberToPageFrom);

            return this;
        }

        public HTMLElement RetrievePageFromCache(int pageNumberToRetrieve)
        {
            if (_pageCache.ContainsKey(pageNumberToRetrieve))
            {
                console.log($"Retrieved page number {pageNumberToRetrieve} from cache");
                return _pageCache.GetValueOrDefault(pageNumberToRetrieve);
            }

            var page = _createPageHtmlElementExpression(pageNumberToRetrieve);

            page.AppendChildren(
                GetComponentsForPage(pageNumberToRetrieve)
                    .Select(_afterComponentCreatedExpression)
                    .ToArray());

            _pageCache.Add(pageNumberToRetrieve, page);

            return page;
        }

        public IEnumerable<HTMLElement> RetrievePagesFromCache(IEnumerable<int> rangeOfPageNumbersToRetrieve)
        {
            return rangeOfPageNumbersToRetrieve.Select(RetrievePageFromCache);
        }

        public ListPageCache Clear()
        {
            _pages.Clear();
            _components.Clear();
            _pageCache.Clear();

            return this;
        }

        private int? AddToComponents(IEnumerable<IComponent> components)
        {
            var componentsToAdd =
                components
                    .Select((component, index) => new
                    {
                        component,
                        componentNumber = index + 1
                    })
                    .Where(
                        item => _components.All(component => item.componentNumber != component.Key));

            foreach (var componentToAdd in componentsToAdd)
            {
                _components.Add(componentToAdd.componentNumber, componentToAdd.component);
            }


            var lastComponentToAdd = componentsToAdd.FirstOrDefault();

            return lastComponentToAdd?.componentNumber;
        }

        private void AddToPages(int? componentNumberToPageFrom)
        {
            var pagesToAdd =
                _components.Skip(componentNumberToPageFrom.GetValueOrDefault()).InGroupsOf(_componentsPerPage);

            _pages.AddRange(pagesToAdd);
        }

        private IEnumerable<KeyValuePair<int, IComponent>> GetComponentsForPage(int pageNumber)
        {
            return _pages.ElementAt(pageNumber - 1);
        }
    }
}
