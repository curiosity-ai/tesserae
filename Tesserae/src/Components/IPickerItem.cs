using System;
using static Retyped.dom;

namespace Tesserae.Components
{
    public interface IPickerItem
    {
        string Name     { get; }

        bool IsSelected { get; set; }

        HTMLElement RenderSuggestion();

        (HTMLElement selectionElement, HTMLElement removeSelectionElement) RenderSelection();
    }
}
