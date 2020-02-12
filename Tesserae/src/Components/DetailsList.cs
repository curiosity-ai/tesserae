using System;
using System.Collections.Generic;
using System.Linq;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsList<TDetailListItem> : IComponent
        where TDetailListItem : class, IDetailsListItem
    {
        private readonly List<IDetailsListColumn> _columns;
        private readonly List<IEnumerable<TDetailListItem>> _listItems;
        private readonly HTMLElement _innerElement;

        private bool _columnHeadersRendered;
        private bool _listItemsContainerRendered;
        private HTMLDivElement _detailsListContainer;

        public DetailsList()
        {
            _columns      = new List<IDetailsListColumn>();
            _listItems    = new List<IEnumerable<TDetailListItem>>();
            _innerElement = Div(_());
        }

        public HTMLElement Render() => _innerElement;

        private static HTMLDivElement CreateGridCell(
            IDetailsListColumn column,
            Func<HTMLElement> gridCellInnerHtmlExpression)
        {
            string role = column.IsRowHeader ?
                "rowheader"
                : "gridcell";

            var gridCellHtmlElement = Div(
                _("",
                    styles: cssStyleDeclaration =>
                    {
                        cssStyleDeclaration.minHeight = column.MinWidth.ToString();
                        cssStyleDeclaration.maxHeight = column.MaxWidth.ToString();
                    })
                    .WithRole(role));

            gridCellHtmlElement.appendChild(gridCellInnerHtmlExpression());

            return gridCellHtmlElement;
        }

        public DetailsList<TDetailListItem> WithColumn<TColumn>(TColumn column)
            where TColumn : class, IDetailsListColumn
        {
            _columns.Add(column);
            return this;
        }

        public DetailsList<TDetailListItem> WithColumns<TColumn>(IEnumerable<TColumn> columns)
            where TColumn : class, IDetailsListColumn
        {
            _columns.AddRange(columns);
            return this;
        }

        public DetailsList<TDetailListItem> WithListItems(params TDetailListItem[] listItems)
        {
            if (!_columnHeadersRendered)
            {
                RenderColumnHeaders();
            }

            if (!_listItemsContainerRendered)
            {
                RenderListItemsContainer();
            }

            _listItems.Add(listItems);

            RenderListItems(listItems);

            return this;
        }

        private void RenderColumnHeaders()
        {
            _detailsListContainer = Div(_("tss-detailslist").WithRole("grid"));
            _innerElement.appendChild(_detailsListContainer);

            var detailsListHeader = Div(_("tss-detailslist-header").WithRole("presentation"));
            _detailsListContainer.appendChild(detailsListHeader);

            foreach (var column in _columns)
            {
                var columnHeaderDiv = Div(_("tss-detailslist-column-header").WithRole("columnheader"));

                var columnHtmlElement = column.Render();

                columnHeaderDiv.SetStyle(cssStyleDeclaration =>
                {
                    cssStyleDeclaration.minWidth = column.MinWidth.ToString();
                    cssStyleDeclaration.maxWidth = column.MaxWidth.ToString();
                });

                columnHeaderDiv.appendChild(columnHtmlElement);
                detailsListHeader.appendChild(columnHeaderDiv);
            }

            _columnHeadersRendered = true;
        }

        private void RenderListItemsContainer()
        {
            var listItemsContainer = Div(_("tss-detailslist-list-items-container").WithRole("presentation"));

            _detailsListContainer.appendChild(listItemsContainer);
        }

        private void RenderListItems(IEnumerable<TDetailListItem> listItems)
        {
            foreach (var listItem in listItems)
            {
                var listItemContainer = Div(_("tss-detailslist-list-item-container").WithRole("presentation"));

                _detailsListContainer.appendChild(listItemContainer);

                listItemContainer.AppendChildren(listItem.Render(_columns, CreateGridCell).ToArray());
            }

            _listItemsContainerRendered = true;
        }
    }
}
