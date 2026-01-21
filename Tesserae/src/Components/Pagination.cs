using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Pagination")]
    public class Pagination : ComponentBase<Pagination, HTMLDivElement>
    {
        private int _totalItems;
        private int _itemsPerPage;
        private int _currentPage;

        public delegate void PageChangedHandler(Pagination sender, int newPage);
        public event PageChangedHandler PageChanged;

        public Pagination(int totalItems = 0, int itemsPerPage = 10, int currentPage = 1)
        {
            _totalItems = totalItems;
            _itemsPerPage = itemsPerPage;
            _currentPage = currentPage;
            InnerElement = Div(_("tss-pagination"));
            Rebuild();
        }

        public int TotalItems
        {
            get => _totalItems;
            set { _totalItems = value; Rebuild(); }
        }

        public int ItemsPerPage
        {
            get => _itemsPerPage;
            set { _itemsPerPage = value; Rebuild(); }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; Rebuild(); }
        }

        private void Rebuild()
        {
            ClearChildren(InnerElement);
            var totalPages = (int)Math.Ceiling((double)_totalItems / _itemsPerPage);
            if (totalPages <= 1) return;

            // Previous
            InnerElement.appendChild(CreateLink("<", _currentPage > 1, () => ChangePage(_currentPage - 1)));

            // Pages
            for (int i = 1; i <= totalPages; i++)
            {
                var pageNum = i;
                var link = CreateLink(i.ToString(), true, () => ChangePage(pageNum));
                if (i == _currentPage) link.classList.add("tss-active");
                InnerElement.appendChild(link);
            }

            // Next
            InnerElement.appendChild(CreateLink(">", _currentPage < totalPages, () => ChangePage(_currentPage + 1)));
        }

        private HTMLElement CreateLink(string text, bool enabled, Action onClick)
        {
            var link = Button(_("tss-pagination-link", text: text));
            if (!enabled)
            {
                link.classList.add("tss-disabled");
            }
            else
            {
                link.onclick = (e) =>
                {
                    StopEvent(e);
                    onClick();
                };
            }
            return link;
        }

        private void ChangePage(int newPage)
        {
            if (newPage == _currentPage) return;
            _currentPage = newPage;
            Rebuild();
            PageChanged?.Invoke(this, _currentPage);
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
