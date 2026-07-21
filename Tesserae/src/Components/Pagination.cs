using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A page-number navigation strip used to walk through pages of results.
    /// </summary>
    [Transpose.Name("tss.Pagination")]
    public sealed class Pagination : ComponentBase<Pagination, HTMLElement>, IBindableComponent<int>
    {
        private readonly HTMLElement             _buttonContainer;
        private readonly HTMLSpanElement         _status;
        private readonly SettableObservable<int> _observable;
        private          int                     _totalItems;
        private          int                     _pageSize;
        private          int                     _currentPage;
        private          int                     _maxPageButtons;
        private          bool                    _showStatus;
        private          Action<Pagination>      _pageChanged;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Pagination(int totalItems = 0, int pageSize = 10, int currentPage = 1)
        {
            _buttonContainer = Div(_("tss-pagination-buttons"));
            _status          = Span(_("tss-pagination-status"));
            _observable      = new SettableObservable<int>(currentPage);

            InnerElement = Div(_("tss-pagination", role: "navigation", ariaLabel: "Pagination"), _buttonContainer, _status);

            _maxPageButtons = 7;
            _showStatus     = true;

            SetTotalItems(totalItems);
            SetPageSize(pageSize);
            SetPage(currentPage, false);
            Update();
        }

        /// <summary>
        /// Gets or sets the total items.
        /// </summary>
        public int TotalItems
        {
            get => _totalItems;
            set => SetTotalItems(value);
        }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => SetPageSize(value);
        }

        /// <summary>
        /// Gets or sets the current page.
        /// </summary>
        public int CurrentPage
        {
            get => _currentPage;
            set => SetPage(value);
        }

        /// <summary>
        /// Gets or sets the total pages.
        /// </summary>
        public int TotalPages
        {
            get
            {
                if (_pageSize <= 0)
                {
                    return 1;
                }

                var pages = (int)Math.Ceiling(_totalItems / (double)_pageSize);
                return Math.Max(1, pages);
            }
        }

        /// <summary>
        /// Gets or sets the max page buttons.
        /// </summary>
        public int MaxPageButtons
        {
            get => _maxPageButtons;
            set
            {
                _maxPageButtons = Math.Max(5, value);
                Update();
            }
        }

        /// <summary>
        /// Shows the status.
        /// </summary>
        public bool ShowStatus
        {
            get => _showStatus;
            set
            {
                _showStatus           = value;
                _status.style.display = _showStatus ? "inline-flex" : "none";
            }
        }

        /// <summary>
        /// Sets the total items of the component.
        /// </summary>
        public Pagination SetTotalItems(int totalItems)
        {
            _totalItems = Math.Max(0, totalItems);
            ClampPage();
            Update();
            return this;
        }

        /// <summary>
        /// Sets the page size of the component.
        /// </summary>
        public Pagination SetPageSize(int pageSize)
        {
            _pageSize = Math.Max(1, pageSize);
            ClampPage();
            Update();
            return this;
        }

        /// <summary>
        /// Sets the page of the component.
        /// </summary>
        public Pagination SetPage(int page, bool raiseEvent = true)
        {
            var clamped = Math.Max(1, Math.Min(page, TotalPages));

            if (_currentPage == clamped)
            {
                return this;
            }

            _currentPage = clamped;
            Update();
            _observable.Value = _currentPage;

            if (raiseEvent)
            {
                _pageChanged?.Invoke(this);
            }

            return this;
        }

        /// <summary>
        /// Returns an observable that tracks the current page number.
        /// </summary>
        public IObservable<int> AsObservable() => _observable;

        /// <summary>
        /// Programmatically updates the current page as part of a two-way binding.
        /// </summary>
        public void SetBoundValue(int value) => SetPage(value);

        /// <summary>
        /// Registers a callback invoked when the page change event fires.
        /// </summary>
        public Pagination OnPageChange(Action<Pagination> onPageChange)
        {
            _pageChanged += onPageChange;
            return this;
        }

        /// <summary>
        /// Configures the component to next.
        /// </summary>
        public Pagination Next()
        {
            return SetPage(CurrentPage + 1);
        }

        /// <summary>
        /// Configures the component to previous.
        /// </summary>
        public Pagination Previous()
        {
            return SetPage(CurrentPage - 1);
        }

        /// <summary>
        /// Configures the component to first.
        /// </summary>
        public Pagination First()
        {
            return SetPage(1);
        }

        /// <summary>
        /// Configures the component to last.
        /// </summary>
        public Pagination Last()
        {
            return SetPage(TotalPages);
        }

        private void ClampPage()
        {
            _currentPage = Math.Max(1, Math.Min(_currentPage, TotalPages));
        }

        private void Update()
        {
            ClearChildren(_buttonContainer);
            var totalPages = TotalPages;

            _buttonContainer.appendChild(CreateNavButton("First",    UIcons.AngleDoubleLeft, CurrentPage == 1, () => First()));
            _buttonContainer.appendChild(CreateNavButton("Previous", UIcons.AngleLeft,       CurrentPage == 1, () => Previous()));

            foreach (var page in GetPageNumbers(totalPages))
            {
                if (page == 0)
                {
                    _buttonContainer.appendChild(Span(_("tss-pagination-ellipsis", text: "…")));
                    continue;
                }

                _buttonContainer.appendChild(CreatePageButton(page));
            }

            _buttonContainer.appendChild(CreateNavButton("Next", UIcons.AngleRight,       CurrentPage == totalPages, () => Next()));
            _buttonContainer.appendChild(CreateNavButton("Last", UIcons.AngleDoubleRight, CurrentPage == totalPages, () => Last()));

            _status.innerText     = $"Page {CurrentPage} of {totalPages}";
            _status.style.display = _showStatus ? "inline-flex" : "none";
        }

        private HTMLButtonElement CreatePageButton(int page)
        {
            var isActive = page == CurrentPage;
            var button   = Button(_("tss-pagination-button", text: page.ToString(), type: "button", ariaLabel: $"Page {page}"));
            button.UpdateClassIf(isActive, "tss-active");
            if (isActive) button.setAttribute("aria-current", "page");
            button.addEventListener("click", _ => SetPage(page));
            return button;
        }

        private HTMLButtonElement CreateNavButton(string label, UIcons icon, bool disabled, Action onClick)
        {
            var button = Button(_("tss-pagination-button tss-pagination-nav", type: "button", ariaLabel: label), I(icon));
            button.disabled = disabled;
            button.UpdateClassIf(disabled, "tss-disabled");

            button.addEventListener("click", _ =>
            {
                if (disabled)
                {
                    return;
                }
                onClick?.Invoke();
            });
            return button;
        }

        private IEnumerable<int> GetPageNumbers(int totalPages)
        {
            if (totalPages <= _maxPageButtons)
            {
                for (var i = 1; i <= totalPages; i++)
                {
                    yield return i;
                }
                yield break;
            }

            yield return 1;

            var windowSize = _maxPageButtons - 2;
            var half       = windowSize / 2;
            var start      = Math.Max(2, CurrentPage - half);
            var end        = Math.Min(totalPages - 1, start + windowSize - 1);

            start = Math.Max(2, end - windowSize + 1);

            if (start > 2)
            {
                yield return 0;
            }

            for (var i = start; i <= end; i++)
            {
                yield return i;
            }

            if (end < totalPages - 1)
            {
                yield return 0;
            }

            yield return totalPages;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}