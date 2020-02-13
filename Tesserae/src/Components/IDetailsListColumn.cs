using System;

namespace Tesserae.Components
{
    public interface IDetailsListColumn<TDetailsListItem> : IComponent
        where TDetailsListItem : class, IDetailsListItem
    {
        string Title                           { get; }

        UnitSize Width                         { get; }

         bool IsRowHeader                      { get; }

        Action<TDetailsListItem> OnColumnClick { get; }
    }
}
