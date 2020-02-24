using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae.HTML;
using static Tesserae.UI;
using static Retyped.dom;
using static Retyped.dom.Node;

namespace Tesserae.Components
{
    public sealed class Picker<TPickerItem> : IComponent where TPickerItem : class, IPickerItem
    {
        private readonly List<TPickerItem> _pickerItems;
        private readonly HTMLElement _container;
        private readonly HTMLElement _pickerContainer;
        private readonly TextBox _textBox;
        private readonly HTMLElement _suggestionsElement;

        private bool _pickerAlreadyCreated;
        private HTMLElement _textBoxElement;
        private IComponent _selectionsComponent;
        private HTMLElement _selectionsElement;

        public Picker()
        {
            _pickerItems          = new List<TPickerItem>();
            _container            = DIV();
            _pickerContainer      = Div(_("tss-picker-container"));
            _textBox              = TextBox();
            _suggestionsElement   = Div(_("tss-picker-suggestions"));
        }

        public IEnumerable<TPickerItem> PickerItems           => _pickerItems;

        public IEnumerable<TPickerItem> SelectedPickerItems   => _pickerItems.Where(pickerItem => pickerItem.IsSelected);

        public IEnumerable<TPickerItem> UnselectedPickerItems => _pickerItems.Where(pickerItem => !pickerItem.IsSelected);

        public int? MaximumAllowedSelections                  { get; private set; }

        public bool DuplicateSelectionsAllowed                { get; private set; }

        public int? SuggestionsTolerance                      { get; private set; }

        public Picker<TPickerItem> WithItems(params TPickerItem[] items)
        {
            return WithItems(items.AsEnumerable());
        }

        public Picker<TPickerItem> WithItems(IEnumerable<TPickerItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            items = items.ToArray();

            if (!items.Any())
            {
                throw new ArgumentException(nameof(items));
            }

            if (PickerItems.Any(pickerItem => items.Any(item => pickerItem.Name.Equals(item.Name))))
            {
                throw new ArgumentException("Can not add duplicate items");
            }

            _pickerItems.AddRange(items);

            return this;
        }

        public Picker<TPickerItem> AllowDuplicatedSelections()
        {
            DuplicateSelectionsAllowed = true;

            return this;
        }

        public Picker<TPickerItem> WithMaximumAllowedSelections(int maximumAllowedSelections)
        {
            MaximumAllowedSelections = maximumAllowedSelections;

            return this;
        }

        public Picker<TPickerItem> WithoutMaximumAllowedSelections()
        {
            MaximumAllowedSelections = null;

            return this;
        }

        public Picker<TPickerItem> WithSuggestionsTolerance(int suggestionsTolerance)
        {
            SuggestionsTolerance = suggestionsTolerance;

            return this;
        }

        public Picker<TPickerItem> WithoutSuggestionsTolerance()
        {
            SuggestionsTolerance = null;

            return this;
        }

        public Picker<TPickerItem> WithSelectionsComponent(IComponent selectionsComponent)
        {
            _selectionsComponent = selectionsComponent;

            return this;
        }

        public HTMLElement Render()
        {
            if (!_pickerAlreadyCreated)
            {
                CreatePicker();
            }

            return _container;
        }

        private void CreatePicker()
        {
            _container.appendChild(_pickerContainer);

            AttachTextBoxOnInputEvent();

            _textBoxElement = _textBox.Render();

            _pickerContainer.appendChild(_textBoxElement);
            _pickerContainer.appendChild(_suggestionsElement);

            if (_selectionsComponent == null)
            {
                _selectionsElement = Div(_("tss-picker-selections"));
                _pickerContainer.insertBefore(_selectionsElement, _textBoxElement);
            }
            else
            {
                _selectionsElement = _selectionsComponent.Render();
            }

            _pickerAlreadyCreated = true;
        }

        private void AttachTextBoxOnInputEvent() => _textBox.OnInput(OnTextBoxInput);

        private void OnTextBoxInput(TextBox textBox, Event @event)
        {
            ClearSuggestions();

            if (SuggestionsTolerance.HasValue && textBox.Text.Length < SuggestionsTolerance)
            {
                return;
            }

            var suggestions = GetSuggestions(textBox.Text);

            CreateSuggestions(suggestions);
        }

        private IEnumerable<TPickerItem> GetPickerItems()
        {
            if (!MaximumAllowedSelections.HasValue || SelectedPickerItems.Count() < MaximumAllowedSelections)
            {
                return DuplicateSelectionsAllowed ? PickerItems : UnselectedPickerItems;
            }

            return Enumerable.Empty<TPickerItem>();
        }

        private IEnumerable<TPickerItem> GetSuggestions(string textBoxText)
        {
            textBoxText = textBoxText.ToUpper();

            return GetPickerItems().Where(pickerItem => pickerItem.Name.ToUpper().Contains(textBoxText));
        }

        private void CreateSuggestions(IEnumerable<TPickerItem> suggestions)
        {
            foreach (var suggestion in suggestions)
            {
                // TODO: Add to a component cache.
                var suggestionContainerElement = Div(_("tss-picker-suggestion"));
                var suggestionElement = suggestion.RenderSuggestion();

                suggestionContainerElement.appendChild(suggestionElement);

                AttachSuggestionOnClickEvent(suggestionElement, suggestion);

                _suggestionsElement.appendChild(suggestionContainerElement);
            }
        }

        private void ClearSuggestions() => _suggestionsElement.RemoveChildElements();

        private void AttachSuggestionOnClickEvent(HTMLElement suggestionElement, TPickerItem suggestion)
        {
            suggestionElement.onclick = _ => OnSuggestionClick(suggestion);
        }

        private void OnSuggestionClick(TPickerItem suggestion) => CreateSelection(suggestion);

        private void CreateSelection(TPickerItem selectedItem)
        {
            UpdateSelection(selectedItem, true);

            var selectionContainerElement                  = Div(_("tss-picker-selection"));
            var (selectionElement, removeSelectionElement) = selectedItem.RenderSelection();

            if (removeSelectionElement != null)
            {
                AttachRemoveSelectionElementOnClickEvent(removeSelectionElement, selectionContainerElement, selectedItem);
            }

            selectionContainerElement.appendChild(selectionElement);

            _selectionsElement.appendChild(selectionContainerElement);
        }

        private void AttachRemoveSelectionElementOnClickEvent(HTMLElement removeSelectionElement, HTMLElement selectionContainerElement, TPickerItem selectedItem)
        {
            removeSelectionElement.onclick = _ => OnRemoveSelectionElementClick(selectionContainerElement, selectedItem);
        }

        private void OnRemoveSelectionElementClick(HTMLElement selectionContainerElement, TPickerItem selectedItem)
        {
            UpdateSelection(selectedItem, false);
            selectionContainerElement.remove();
        }

        private void UpdateSelection(TPickerItem selectedItem, bool isSelected)
        {
            selectedItem.IsSelected = isSelected;
            _textBox.ClearText();
            ClearSuggestions();
        }
    }
}
