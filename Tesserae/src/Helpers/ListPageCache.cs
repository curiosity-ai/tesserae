using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Provides caching and paging functionality for a list of components.
    /// </summary>
    /// <typeparam name="TComponent">The type of components in the list.</typeparam>
    [H5.Name("tss.ListPageCache")]
    public sealed class ListPageCache<TComponent> : ComponentCacheBase<TComponent>
        where TComponent : class
    {
        private readonly Func<int, HTMLElement>                             _createPageHtmlElementExpression;
        private readonly Func<(int Key, TComponent Component), HTMLElement> _afterComponentRetrievedExpression;
        private readonly Dictionary<int, HTMLElement>                       _pageCache;
        private readonly List<List<(int Key, TComponent Component)>>        _pages;

        /// <summary>
        /// Initializes a new instance of the ListPageCache class.
        /// </summary>
        /// <param name="rowsPerPage">The number of rows per page.</param>
        /// <param name="columnsPerRow">The number of columns per row.</param>
        /// <param name="createPageHtmlElementExpression">A function that creates the HTML element for a page.</param>
        /// <param name="afterComponentRetrievedExpression">A function that processes a component after it is retrieved.</param>
        public ListPageCache(
            int                                                rowsPerPage,
            int                                                columnsPerRow,
            Func<int, HTMLElement>                             createPageHtmlElementExpression,
            Func<(int key, TComponent component), HTMLElement> afterComponentRetrievedExpression)
        {
            if (rowsPerPage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rowsPerPage));
            }

            if (columnsPerRow <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columnsPerRow));
            }

            RowsPerPage       = rowsPerPage;
            ComponentsPerPage = RowsPerPage * columnsPerRow;

            _createPageHtmlElementExpression =
                createPageHtmlElementExpression ??
                throw new ArgumentNullException(nameof(createPageHtmlElementExpression));

            _afterComponentRetrievedExpression =
                afterComponentRetrievedExpression ??
                throw new ArgumentNullException(nameof(afterComponentRetrievedExpression));

            _pageCache = new Dictionary<int, HTMLElement>();
            _pages     = new List<List<(int key, TComponent component)>>();
        }

        /// <summary>Gets the number of rows per page.</summary>
        public int RowsPerPage { get; }

        /// <summary>Gets the total number of components per page.</summary>
        public int ComponentsPerPage { get; }

        /// <summary>Gets the total number of pages.</summary>
        public int PagesCount => _pages.Count;

        /// <summary>Gets the total number of rows across all pages.</summary>
        public int RowsCount => RowsPerPage * PagesCount;

        /// <summary>Adds a collection of components to the cache and distributes them into pages.</summary>
        public ListPageCache<TComponent> AddComponents(IEnumerable<TComponent> components)
        {
            var currentComponentsCount = ComponentsCount;

            AddToComponents(components);
            AddPages(currentComponentsCount);

            return this;
        }

        /// <summary>
        /// Retrieves the HTML element for a specific page from the cache, creating it if necessary.
        /// </summary>
        /// <param name="pageNumberToRetrieve">The page number to retrieve.</param>
        /// <returns>The HTML element for the page.</returns>
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
                   .Select(_afterComponentRetrievedExpression)
                   .ToArray());

            _pageCache.Add(pageNumberToRetrieve, page);

            return page;
        }

        /// <summary>Retrieves HTML elements for a range of pages from the cache.</summary>
        public IEnumerable<HTMLElement> RetrievePagesFromCache(IEnumerable<int> rangeOfPageNumbersToRetrieve)
        {
            return rangeOfPageNumbersToRetrieve.Select(RetrievePageFromCache);
        }

        /// <summary>Retrieves HTML elements for all pages from the cache.</summary>
        public IEnumerable<HTMLElement> RetrieveAllPagesFromCache()
        {
            return Enumerable.Range(1, PagesCount).Select(RetrievePageFromCache);
        }

        /// <summary>Clears the entire cache and pages.</summary>
        public ListPageCache<TComponent> Clear()
        {
            _componentsAndKeys.Clear();
            _pages.Clear();
            _pageCache.Clear();

            return this;
        }

        private void AddPages(int componentNumberToPageFrom)
        {
            var pagesToAdd =
                _componentsAndKeys.Skip(componentNumberToPageFrom).InGroupsOf(ComponentsPerPage);

            _pages.AddRange(pagesToAdd);
        }

        private List<(int key, TComponent component)> GetComponentsForPage(int pageNumber)
        {
            return _pages.ElementAt(pageNumber - 1);
        }
    }
}