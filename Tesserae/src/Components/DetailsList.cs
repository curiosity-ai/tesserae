using System;
using System.Collections.Generic;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsList<TListItem> : IComponent
    {
        private readonly List<IDetailsListColumn<TListItem>> _columns;
        private readonly HTMLElement _innerElement;

        public DetailsList()
        {
            _columns      = new List<IDetailsListColumn<TListItem>>();
            _innerElement = Div(_());
        }

        public HTMLElement Render()
        {
            var detailsListContainer = Div(_("tss-detailslist-container").WithRole("grid"));
            _innerElement.appendChild(detailsListContainer);

            var detailsListHeader = Div(_("tss-detailslist-header").WithRole("presentation"));
            detailsListContainer.appendChild(detailsListHeader);

            foreach (var column in _columns)
            {
                var columnHeader = Div(
                    _("tss-detaillist-column-header").WithRole("columnheader"));

                detailsListHeader.appendChild(columnHeader);
            }

            return _innerElement;
        }

        public DetailsList<TListItem> WithColumn<TColumn>(TColumn column)
            where TColumn : IDetailsListColumn<TListItem>
        {
            _columns.Add(column);
            return this;
        }

        public DetailsList<TListItem> WithColumns(IEnumerable<IDetailsListColumn<TListItem>> columns)
        {
            _columns.AddRange(columns);
            return this;
        }

        public DetailsList<TListItem> WithListItems(IEnumerable<TListItem> listItems)
        {
            return this;
        }
    }
}
