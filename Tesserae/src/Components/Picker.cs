using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Picker")]
    public sealed class Picker<TPickerItem> : IComponent, ITabIndex, IObservableListComponent<TPickerItem>, IRoundedStyle where TPickerItem : class, IPickerItem
    {
        private const string SearchPlaceholderText = "Type to search";

        private event ComponentEventHandler<Picker<TPickerItem>, ItemPickedEvent> SelectedItem;

        private readonly ObservableList<TPickerItem> _pickerItems = new ObservableList<TPickerItem>();
        private readonly HTMLElement                 _container;
        private readonly Dropdown                    _dropdown;
        private readonly HTMLElement                 _selectionsElement;

        public Picker(int maximumAllowedSelections = int.MaxValue, bool duplicateSelectionsAllowed = false)
        {
            MaximumAllowedSelections   = maximumAllowedSelections;
            DuplicateSelectionsAllowed = duplicateSelectionsAllowed;
            _selectionsElement         = Div(_("tss-picker-selections"));

            var pickerContainer = Div(_("tss-picker-container"));

            _dropdown = Dropdown()
               .Single()
               .Searchable(SearchPlaceholderText)
               .NoArrow()
               .ClosedControlEmptyState(visible: false, disableWhenEmpty: false)
               .WithCustomSelectionRender(_ => BuildControlContent())
               .Class("tss-picker-dropdown");

            _container = pickerContainer;
            CreatePicker(pickerContainer);
        }

        public int TabIndex
        {
            set
            {
                _dropdown.TabIndex = value;
            }
        }

        public IObservable<IReadOnlyList<TPickerItem>> AsObservable()
        {
            return _pickerItems;
        }

        public IEnumerable<TPickerItem> PickerItems => _pickerItems;

        public IEnumerable<TPickerItem> SelectedPickerItems => _pickerItems.Where(pickerItem => pickerItem.IsSelected);

        public IEnumerable<TPickerItem> UnselectedPickerItems => _pickerItems.Where(pickerItem => !pickerItem.IsSelected);

        public int? MaximumAllowedSelections { get; }

        public bool DuplicateSelectionsAllowed { get; }

        private bool IsSingleSelectionPicker => MaximumAllowedSelections == 1;

        public Picker<TPickerItem> Items(params TPickerItem[] items)
        {
            return Items(items.AsEnumerable());
        }

        public Picker<TPickerItem> Items(IEnumerable<TPickerItem> items)
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

            RefreshSelections();

            return this;
        }

        public Picker<TPickerItem> OnItemSelected(ComponentEventHandler<Picker<TPickerItem>, ItemPickedEvent> eventHandler)
        {
            SelectedItem += eventHandler;
            return this;
        }

        public HTMLElement Render() => _container;

        private void CreatePicker(HTMLElement pickerContainer)
        {
            if (IsSingleSelectionPicker)
            {
                pickerContainer.appendChild(_dropdown.Render());
                pickerContainer.appendChild(_selectionsElement);
                return;
            }

            var inlineRow = Div(_("tss-picker-inline-row"));
            inlineRow.appendChild(_selectionsElement);
            inlineRow.appendChild(_dropdown.Render());
            pickerContainer.appendChild(inlineRow);
        }

        private IEnumerable<TPickerItem> GetAvailablePickerItems()
        {
            if (MaximumAllowedSelections.HasValue && SelectedPickerItems.Count() >= MaximumAllowedSelections.Value)
            {
                return Enumerable.Empty<TPickerItem>();
            }

            return DuplicateSelectionsAllowed ? PickerItems : UnselectedPickerItems;
        }

        private void RefreshSelections()
        {
            RenderSelections();
            RefreshDropdownItems();
            RefreshSearchVisibility();
        }

        private void RefreshDropdownItems()
        {
            _dropdown.Items(GetAvailablePickerItems().Select(CreateDropdownItem).ToArray());
        }

        private void RefreshSearchVisibility()
        {
            _dropdown.Render().style.display = IsSingleSelectionPicker && SelectedPickerItems.Any() ? "none" : "";
        }

        private Dropdown.Item CreateDropdownItem(TPickerItem pickerItem)
        {
            return DropdownItem(pickerItem.Render())
               .SetData(pickerItem)
               .OnSelected(sender => OnDropdownItemSelected((TPickerItem)sender.Data));
        }

        private void OnDropdownItemSelected(TPickerItem selectedItem)
        {
            if (MaximumAllowedSelections.HasValue &&
                !selectedItem.IsSelected &&
                SelectedPickerItems.Count() >= MaximumAllowedSelections.Value)
            {
                RefreshDropdownItems();
                return;
            }

            UpdateSelection(selectedItem, true);
            SelectedItem?.Invoke(this, new ItemPickedEvent(selectedItem));
        }

        private void RenderSelections()
        {
            ClearChildren(_selectionsElement);

            foreach (var selectedItem in SelectedPickerItems)
            {
                _selectionsElement.appendChild(CreateSelectionElement(selectedItem));
            }
        }

        private HTMLElement CreateSelectionElement(TPickerItem selectedItem)
        {
            var selectionContainerElement = Div(_("tss-picker-selection"));
            var removeButton              = Button()
               .Link()
               .SetIcon(UIcons.Cross, size: TextSize.Tiny, color: "var(--tss-default-foreground-color)")
               .OnClick((_, ev) =>
                {
                    StopEvent(ev);
                    UpdateSelection(selectedItem, false);
                }).Render();

            removeButton.classList.add("tss-picker-remove");

            selectionContainerElement.appendChild(selectedItem.Render().Render());
            selectionContainerElement.appendChild(removeButton);

            return selectionContainerElement;
        }

        private IComponent BuildControlContent()
        {
            return new HtmlComponent(Div(_("tss-picker-control-content"), TextBlock(SearchPlaceholderText).Secondary().Class("tss-picker-search-placeholder").Render()));
        }

        private void UpdateSelection(TPickerItem selectedItem, bool isSelected)
        {
            selectedItem.IsSelected = isSelected;
            RefreshSelections();
        }

        public sealed class ItemPickedEvent
        {
            public ItemPickedEvent(TPickerItem item) => Item = item;
            public TPickerItem Item { get; }
        }

        private sealed class HtmlComponent : IComponent
        {
            private readonly HTMLElement _element;

            public HtmlComponent(HTMLElement element)
            {
                _element = element;
            }

            public HTMLElement Render() => _element;
        }
    }
}
