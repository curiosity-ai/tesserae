using System;
using System.Collections.Generic;
using System.Linq;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public sealed class Picker<TPickerItem> : IComponent where TPickerItem : class, IPickerItem
    {
        private readonly List<TPickerItem> _pickerItems;
        private readonly HTMLElement _container;
        private readonly TextBox _textBox;
        private readonly SuggestionsLayer _suggestionsLayer;
        private readonly bool _renderSelectionsInline;
        private readonly HTMLElement _selectionsElement;

        private HTMLElement _textBoxElement;

        public Picker(int maximumAllowedSelections = 5, bool duplicateSelectionsAllowed = false, int suggestionsTolerance = 2,  bool renderSelectionsInline = true, string suggestionsTitleText = null)
        {
            MaximumAllowedSelections   = maximumAllowedSelections;
            DuplicateSelectionsAllowed = duplicateSelectionsAllowed;
            SuggestionsTolerance       = suggestionsTolerance;
            _renderSelectionsInline   = renderSelectionsInline;
            _selectionsElement         = Div(_("tss-picker-selections"));

            var pickerContainer = Div(_("tss-picker-container"));

            if (_renderSelectionsInline)
            {
                pickerContainer.classList.add("tss-picker-container-inline-selections");
                _selectionsElement.classList.add("tss-picker-selections-inline");
            }

            _pickerItems          = new List<TPickerItem>();
            _container            = DIV();
            _textBox              = TextBox();
            _suggestionsLayer     = new SuggestionsLayer(new Suggestions(suggestionsTitleText));

            CreatePicker(pickerContainer);
        }

        public IEnumerable<TPickerItem> PickerItems           => _pickerItems;

        public IEnumerable<TPickerItem> SelectedPickerItems   => _pickerItems.Where(pickerItem => pickerItem.IsSelected);

        public IEnumerable<TPickerItem> UnselectedPickerItems => _pickerItems.Where(pickerItem => !pickerItem.IsSelected);

        public int? MaximumAllowedSelections                  { get; }

        public bool DuplicateSelectionsAllowed                { get; }

        public int SuggestionsTolerance                       { get; }

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

            items = items.ToList();

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

        public event EventHandler<TPickerItem> onItemSelected;

        public Picker<TPickerItem> OnItemSelected(EventHandler<TPickerItem> eventHandler)
        {
            onItemSelected += eventHandler;

            return this;
        }

        public HTMLElement Render() => _container;

        private void CreatePicker(HTMLElement pickerContainer)
        {
            _container.appendChild(pickerContainer);

            AttachTextBoxOnInputEvent();
            AttachTextBoxOnFocusEvent();
            AttachTextBoxOnBlurEvent();

            _textBoxElement = _textBox.Render();

            pickerContainer.appendChild(_textBoxElement);

            if (_renderSelectionsInline)
            {
                pickerContainer.insertBefore(_selectionsElement, _textBoxElement);
            }
            else
            {
                pickerContainer.appendChild(_selectionsElement);
            }
        }

        private void AttachTextBoxOnInputEvent() => _textBox.OnInput(OnTextBoxInput);

        private void AttachTextBoxOnFocusEvent() => _textBox.OnFocus(OnTextBoxInput);

        private void AttachTextBoxOnBlurEvent()  => _textBox.OnBlur(OnTextBoxBlur);

        private void OnTextBoxInput(TextBox textBox, Event @event)
        {
            ClearSuggestions();

            if (string.IsNullOrWhiteSpace(textBox.Text) || (textBox.Text.Length < SuggestionsTolerance))
            {
                _suggestionsLayer.Hide();
                return;
            }

            var suggestions = GetSuggestions(textBox.Text);

            CreateSuggestions(suggestions);
        }

        private void OnTextBoxBlur(TextBox textBox, Event @event)
        {
            window.setTimeout(_ =>
            {
                ClearSuggestions();
                _suggestionsLayer.Hide();
            }, 1000);
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
            suggestions = suggestions.ToList();

            if (!suggestions.Any())
            {
                _suggestionsLayer.Hide();
                return;
            }

            foreach (var suggestion in suggestions)
            {
                // TODO: Add to a component cache.
                var suggestionContainerElement = Div(_("tss-picker-suggestion"));
                var suggestionElement = suggestion.RenderSuggestion();

                suggestionContainerElement.appendChild(suggestionElement);

                AttachSuggestionOnClickEvent(suggestionElement, suggestion);

                _suggestionsLayer.SuggestionsContent.appendChild(suggestionContainerElement);
            }

            if (!_suggestionsLayer.IsVisible)
            {
                _suggestionsLayer.Show();
            }

            PositionSuggestions();
        }

        private void ClearSuggestions()
        {
            var suggestions = _suggestionsLayer.SuggestionsContent.getElementsByClassName("tss-picker-suggestion");

            while (suggestions.length > 0)
            {
                suggestions[0].parentNode.removeChild(suggestions[0]);
            }
        }

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

            onItemSelected?.Invoke(this, selectedItem);
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
            _suggestionsLayer.Hide();
        }

        private void PositionSuggestions()
        {
            _suggestionsLayer.SuggestionsContainer.classList.add("tss-layer-picker-suggestions");

            var suggestionsContentClientHeight = _suggestionsLayer.SuggestionsContent.clientHeight;
            var textBoxClientRect              = (ClientRect)_textBoxElement.getBoundingClientRect();
            var bodyClientRect                 = (ClientRect)document.body.getBoundingClientRect();

            if (suggestionsContentClientHeight + textBoxClientRect.bottom + 10 >= bodyClientRect.height)
            {
                _suggestionsLayer.SuggestionsContainer.style.top = $"{(textBoxClientRect.bottom - suggestionsContentClientHeight - textBoxClientRect.height - 10).px()}";
            }
            else
            {
                _suggestionsLayer.SuggestionsContainer.style.top = $"{(textBoxClientRect.bottom + 10).px()}";
            }

            _suggestionsLayer.SuggestionsContainer.style.left  = textBoxClientRect.left.px().ToString();
            _suggestionsLayer.SuggestionsContainer.style.width = $"{(textBoxClientRect.width / 2).px()}";
        }

        private class SuggestionsLayer : Layer
        {
            private readonly HTMLElement _suggestions;

            public SuggestionsLayer(IComponent suggestions)
            {
                _suggestions = suggestions.Render();
                _contentHtml = Div(_("tss-layer-content"), suggestions.Render());
            }

            public Element SuggestionsContent       => _suggestions;

            public HTMLElement SuggestionsContainer => _renderedContent;
        }

        private class Suggestions : IComponent
        {
            private readonly HTMLElement _suggestions;

            public Suggestions(string suggestionsTitleText)
            {
                _suggestions = Div(_("tss-picker-suggestions"));

                if (!string.IsNullOrWhiteSpace(suggestionsTitleText))
                {
                    var suggestionsLabel = Label(_(text: suggestionsTitleText));

                    _suggestions.appendChild(suggestionsLabel);
                }
            }

            public HTMLElement Render() => _suggestions;
        }
    }
}
