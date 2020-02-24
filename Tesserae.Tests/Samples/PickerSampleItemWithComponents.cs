using Tesserae.Components;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Tests.Samples
{
    public class PickerSampleItemWithComponents : IPickerItem
    {
        public LineAwesome _icon;
        private HTMLElement _suggestionElement;
        private HTMLElement _selectionElement;
        private HTMLElement _removeSelectionElement;

        public PickerSampleItemWithComponents(string name, LineAwesome icon)
        {
            Name  = name;
            _icon = icon;
        }

        public string Name     { get; }

        public bool IsSelected { get; set; }

        public HTMLElement RenderSuggestion()
        {
            return _suggestionElement ?? (_suggestionElement = Div(_(), I(_icon), Span(_(text: Name))));
        }

        public (HTMLElement selectionElement, HTMLElement removeSelectionElement) RenderSelection()
        {
            if (_selectionElement == null)
            {
                _selectionElement       = DIV();
                _selectionElement.appendChild(I(_icon));
                _selectionElement.appendChild(Span(_(text: Name)));

                _removeSelectionElement = I(LineAwesome.WindowClose);

                _selectionElement.appendChild(_removeSelectionElement);
            }

            return (_selectionElement, _removeSelectionElement);
        }
    }
}
