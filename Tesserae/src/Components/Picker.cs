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

        public int MaximumAllowedSelections                   { get; private set; }

        public bool DuplicateSelectionsAllowed                { get; private set; }

        public int SuggestionsTolerance                       { get; private set; }

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

        public Picker<TPickerItem> WithSuggestionsTolerance(int suggestionsTolerance)
        {
            SuggestionsTolerance = suggestionsTolerance;

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

            _pickerAlreadyCreated = true;
        }

        private void AttachTextBoxOnInputEvent() => _textBox.OnInput(OnTextBoxInput);

        private void OnTextBoxInput(TextBox textBox, Event @event)
        {
            ClearSuggestions();

            if (textBox.Text.Length < SuggestionsTolerance)
            {
                return;
            }

            var suggestions = GetSuggestions(textBox.Text);

            CreateSuggestions(suggestions);
        }

        private IEnumerable<TPickerItem> GetSuggestions(string textBoxText)
        {
            textBoxText = textBoxText.ToUpper();

            return UnselectedPickerItems.Where(pickerItem => pickerItem.Name.ToUpper().Contains(textBoxText));
        }

        private void CreateSuggestions(IEnumerable<TPickerItem> suggestions)
        {
            foreach (var suggestion in suggestions)
            {
                // TODO: Add to a component cache.
                var suggestedPickerItemElement = Div(_("tss-picker-suggestedpickeritem", text: suggestion.Name));

                AttachSuggestionOnClickEvent(suggestedPickerItemElement);

                _suggestionsElement.appendChild(suggestedPickerItemElement);
            }
        }

        private void ClearSuggestions() => _suggestionsElement.RemoveChildElements();

        private void AttachSuggestionOnClickEvent(HTMLElement suggestionElement) => suggestionElement.onclick = OnSuggestionClick;

        private void OnSuggestionClick(MouseEvent mouseEvent)
        {
            CreateSelection(mouseEvent.toElement.textContent);
        }

        private void CreateSelection(string selection)
        {
            var selectedItem = UnselectedPickerItems.SingleOrDefault(pickerItem => pickerItem.Name.Equals(selection));

            if (selectedItem == null)
            {
                return;
            }

            selectedItem.IsSelected = true;
            _textBox.ClearText();

            var selectionElement = Div(_("tss-picker-selection"));

            var selectionElementContent = Div(_(text: selection));

            var removeSelectionIcon = I(LineAwesome.WindowClose);

            AttachRemoveSelectionElementOnClickEvent(removeSelectionIcon);

            selectionElementContent.appendChild(removeSelectionIcon);

            selectionElement.appendChild(selectionElementContent);

            _pickerContainer.insertBefore(selectionElement, _textBoxElement);

            ClearSuggestions();
        }

        private void AttachRemoveSelectionElementOnClickEvent(HTMLElement removeSelectionElement) => removeSelectionElement.onclick = OnRemoveSelectionElementClick;

        private void OnRemoveSelectionElementClick(MouseEvent mouseEvent)
        {
            var selection = mouseEvent.toElement.parentElement.textContent;

            var selectedItem = SelectedPickerItems.Single(pickerItem => pickerItem.Name.Equals(selection));

            selectedItem.IsSelected = false;

            _pickerContainer.removeChild(mouseEvent.toElement.parentElement.parentElement);
        }
    }
}
