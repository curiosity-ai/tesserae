using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A page-number navigation strip used to walk through pages of results. It reads as a footer under
    /// the thing it pages: how much there is on the left, the controls on the right.
    /// <para>
    /// A set that fits on one page renders nothing at all, since a lone "1" button beside two greyed
    /// chevrons says only that there is nothing to navigate. Call <see cref="ShowForSinglePage"/> when
    /// the strip should hold its place regardless.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.Pagination")]
    public sealed class Pagination : ComponentBase<Pagination, HTMLElement>, IBindableComponent<int>
    {
        private const string PAGE_KEY = "data-tss-page";

        private readonly HTMLElement             _buttonContainer;
        private readonly HTMLSpanElement         _status;
        private readonly SettableObservable<int> _observable;
        private          int                     _totalItems;
        private          int                     _pageSize;
        private          int                     _currentPage;
        private          int                     _maxPageButtons;
        private          bool                    _showStatus;
        private          bool                    _showForSinglePage;
        private          bool                    _showFirstLast;
        private          Action<Pagination>      _pageChanged;

        private Func<int, int, int, string> _format = (from, to, total) => $"{from}-{to} of {total}";

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Pagination(int totalItems = 0, int pageSize = 10, int currentPage = 1)
        {
            _buttonContainer = Div(Att("tss-pagination-buttons"));
            _status          = Span(Att("tss-pagination-status"));

            _observable = new SettableObservable<int>(currentPage);

            //The status comes first so the count sits under the content and the controls sit at the far
            //end, which is the shape a list footer is read in.
            InnerElement = Div(Att("tss-pagination", role: "navigation", ariaLabel: "Pagination"), _status, _buttonContainer);

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
        /// Gets or sets whether the strip renders for a set that fits on a single page. Off by default,
        /// which is what keeps a short list from carrying a control that can do nothing.
        /// </summary>
        public bool ShowForSinglePage
        {
            get => _showForSinglePage;
            set
            {
                _showForSinglePage = value;
                Update();
            }
        }

        /// <summary>
        /// Gets or sets whether the jump-to-first and jump-to-last chevrons render. Off by default: the
        /// strip always numbers the first and the last page, so the pair duplicates the two buttons
        /// sitting immediately beside them.
        /// </summary>
        public bool ShowFirstLastButtons
        {
            get => _showFirstLast;
            set
            {
                _showFirstLast = value;
                Update();
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
        /// Changes how the range and the count are written - for another language, or for "1 to 25 of 118".
        /// </summary>
        public Pagination SetFormat(Func<int, int, int, string> format)
        {
            _format = format ?? ((from, to, total) => $"{from}-{to} of {total}");

            Update();
            return this;
        }

        /// <summary>
        /// Returns the component styled as the footer of the list above it - a rule along the top and the
        /// padding that separates the controls from the last row.
        /// </summary>
        public Pagination AsListFooter()
        {
            InnerElement.classList.add("tss-pagination-footer");

            return this;
        }

        /// <summary>
        /// Returns the component configured to render the jump-to-first and jump-to-last chevrons.
        /// </summary>
        public Pagination WithFirstLastButtons()
        {
            ShowFirstLastButtons = true;

            return this;
        }

        /// <summary>
        /// Returns the component configured to render for a set that fits on a single page.
        /// </summary>
        public Pagination AlwaysVisible()
        {
            ShowForSinglePage = true;

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
            var totalPages = TotalPages;
            var isVisible  = totalPages > 1 || _showForSinglePage;

            InnerElement.style.display = isVisible ? "flex" : "none";

            if (!isVisible)
            {
                return;
            }

            var focusedKey = GetFocusedKey();

            ClearChildren(_buttonContainer);

            if (_showFirstLast)
            {
                _buttonContainer.appendChild(CreateNavButton("First", UIcons.AngleDoubleLeft, "first", CurrentPage == 1, () => First()));
            }

            _buttonContainer.appendChild(CreateNavButton("Previous", UIcons.AngleLeft, "prev", CurrentPage == 1, () => Previous()));

            foreach (var page in GetPageNumbers(totalPages))
            {
                if (page == 0)
                {
                    _buttonContainer.appendChild(Span(Att("tss-pagination-ellipsis", text: "…")));
                    continue;
                }

                _buttonContainer.appendChild(CreatePageButton(page));
            }

            _buttonContainer.appendChild(CreateNavButton("Next", UIcons.AngleRight, "next", CurrentPage == totalPages, () => Next()));

            if (_showFirstLast)
            {
                _buttonContainer.appendChild(CreateNavButton("Last", UIcons.AngleDoubleRight, "last", CurrentPage == totalPages, () => Last()));
            }

            RenderStatus();
            RestoreFocus(focusedKey);
        }

        private void RenderStatus()
        {
            if (_totalItems > 0)
            {
                var from = ((CurrentPage - 1) * _pageSize) + 1;
                var to   = Math.Min(CurrentPage * _pageSize, _totalItems);

                _status.innerText = _format(from, to, _totalItems);
            }
            else
            {
                _status.innerText = "";
            }

            _status.style.display = _showStatus ? "inline-flex" : "none";
        }

        /// <summary>
        /// Reads which control the keyboard is on, so the rebuild below can put it back. Without it,
        /// paging with the keyboard drops focus to the body and the next Tab starts from the top.
        /// </summary>
        private string GetFocusedKey()
        {
            var active = document.activeElement.As<HTMLElement>();

            if (active is null || !_buttonContainer.contains(active))
            {
                return null;
            }

            return active.getAttribute(PAGE_KEY);
        }

        private void RestoreFocus(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            //Stepping onto the last page disables Next under the cursor, so fall back to the page button
            //that is now current rather than leaving focus nowhere.
            var target = FindButton(key) ?? FindButton(CurrentPage.ToString());

            if (target is object)
            {
                target.focus();
            }
        }

        private HTMLButtonElement FindButton(string key)
        {
            var found = _buttonContainer.querySelector($"[{PAGE_KEY}=\"{key}\"]:not(:disabled)");

            return found.As<HTMLButtonElement>();
        }

        private HTMLButtonElement CreatePageButton(int page)
        {
            var isActive = page == CurrentPage;
            var button   = UI.Button(Att("tss-pagination-button", text: page.ToString(), type: "button", ariaLabel: $"Page {page}"));
            button.setAttribute(PAGE_KEY, page.ToString());
            button.UpdateClassIf(isActive, "tss-active");
            if (isActive) button.setAttribute("aria-current", "page");
            button.addEventListener("click", _ => SetPage(page));
            return button;
        }

        private HTMLButtonElement CreateNavButton(string label, UIcons icon, string key, bool disabled, Action onClick)
        {
            var button = UI.Button(Att("tss-pagination-button tss-pagination-nav", type: "button", ariaLabel: label), I(icon));
            button.setAttribute(PAGE_KEY, key);
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
