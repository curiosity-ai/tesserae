using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tesserae.HTML;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public sealed class Dropdown : Layer<Dropdown>, ICanValidate<Dropdown>, IObservableListComponent<Dropdown.Item>
    {
        private const string _multiSelectDropdownClassName = "tss-dropdown-multi";

        private static HTMLElement _firstItem;

        private readonly HTMLElement _childContainer;
        private readonly HTMLDivElement _container;
        private readonly HTMLSpanElement _errorSpan;

        private HTMLDivElement _spinner;
        private bool _isChanged;
        private bool _callSelectOnChangingItemSelections;
        private Func<Task<Item[]>> _itemsSource;
        private ReadOnlyArray<Item> _lastRenderedItems;
        private ObservableList<Item> _selectedChildren;
        private HTMLDivElement _popupDiv;
        private string _search;

        public Dropdown()
        {
            InnerElement = Div(_("tss-dropdown"));
            _errorSpan = Span(_("tss-dropdown-error"));

            _container = Div(_("tss-dropdown-container"), InnerElement, _errorSpan);

            _childContainer = Div(_());

            InnerElement.onclick = (e) =>
            {
                StopEvent(e);
                if (!IsVisible && IsEnabled)
                    Show();
            };

            _container.onclick = (e) =>
            {
                StopEvent(e);
                if (!IsVisible && IsEnabled)
                    Show();
            };

            _callSelectOnChangingItemSelections = true;
            _selectedChildren = new ObservableList<Item>();
        }

        public Dropdown SuppressSelectedOnChangingItemSelections()
        {
            _callSelectOnChangingItemSelections = false;
            return this;
        }

        public SelectMode Mode
        {
            get => _childContainer.classList.contains(_multiSelectDropdownClassName) ? SelectMode.Multi : SelectMode.Single;
            set
            {
                if (value == SelectMode.Single)
                {
                    _childContainer.classList.remove(_multiSelectDropdownClassName);
                }
                else
                {
                    _childContainer.classList.add(_multiSelectDropdownClassName);
                }
            }
        }

        public Item[] SelectedItems => _selectedChildren.ToArray();

        public string SelectedText
        {
            get
            {
                return string.Join(", ", _selectedChildren.Select(x => x.Text));
            }
        }

        public string Error
        {
            get => _errorSpan.innerText;
            set => _errorSpan.innerText = value;
        }

        public bool HasBorder
        {
            get => !_container.classList.contains("tss-noborder");
            set
            {
                if (value)
                {
                    _container.classList.remove("tss-noborder");
                }
                else
                {
                    _container.classList.add("tss-noborder");
                }
            }
        }

        public bool IsInvalid
        {
            get => _container.classList.contains("tss-invalid");
            set
            {
                if (value)
                {
                    _container.classList.add("tss-invalid");
                }
                else
                {
                    _container.classList.remove("tss-invalid");
                }
            }
        }

        public bool IsEnabled
        {
            get => !_container.classList.contains("tss-disabled");
            set
            {
                if (value)
                {
                    _container.classList.remove("tss-disabled");
                }
                else
                {
                    _container.classList.add("tss-disabled");
                }
            }
        }

        public bool IsRequired
        {
            get => _container.classList.contains("tss-required");
            set
            {
                if (value)
                {
                    _container.classList.add("tss-required");
                }
                else
                {
                    _container.classList.remove("tss-required");
                }
            }
        }

        public override HTMLElement Render()
        {
            DomObserver.WhenMounted(_container, () =>
            {
                DomObserver.WhenRemoved(_container, () =>
                {
                    Hide();
                });
            });
            return _container;
        }

        public async Task LoadItemsAsync()
        {
            if (_itemsSource is null)
                throw new InvalidOperationException("Only valid with async items");

            var itemsSourceLocal = _itemsSource;
            _itemsSource = null; // Clear so we don't call this twice
            _spinner = Div(_("tss-spinner"));
            _container.appendChild(_spinner);
            _container.style.pointerEvents = "none";

            var items = await itemsSourceLocal();
            Clear();
            Items(items);
            _container.removeChild(_spinner);
            _container.style.pointerEvents = "unset";
        }

        public override void Show()
        {
            if (_contentHtml == null)
            {
                _popupDiv = Div(_("tss-dropdown-popup"), _childContainer);
                _contentHtml = Div(_("tss-dropdown-layer"), _popupDiv);

                _contentHtml.addEventListener("click", OnWindowClick);
                _contentHtml.addEventListener("dblclick", OnWindowClick);
                _contentHtml.addEventListener("contextmenu", OnWindowClick);
                _contentHtml.addEventListener("wheel", OnWindowClick);

                if (_itemsSource is object)
                {
                    LoadItemsAsync().ContinueWith(t => Show()).FireAndForget();
                    return;
                }
            }

            _popupDiv.style.height = "unset";
            _popupDiv.style.left = "-1000px";
            _popupDiv.style.top = "-1000px";

            base.Show();

            _isChanged = false;

            if (!_popupDiv.classList.contains("tss-no-focus")) _popupDiv.classList.add("tss-no-focus");

            RecomputePopupPosition();

            DomObserver.WhenMounted(_popupDiv, () =>
            {
                document.addEventListener("keydown", OnPopupKeyDown);
                if (_selectedChildren.Count > 0)
                {
                    _selectedChildren[_selectedChildren.Count - 1].Render().focus();
                }
            });
        }

        private void RecomputePopupPosition()
        {
            ClientRect rect = (ClientRect)_container.getBoundingClientRect();
            var contentRect = (ClientRect)_popupDiv.getBoundingClientRect();
            _popupDiv.style.top = rect.bottom - 1 + "px";
            _popupDiv.style.minWidth = rect.width + "px";

            var finalLeft = rect.left;
            if (rect.left + contentRect.width + 1 > window.innerWidth)
            {
                finalLeft = window.innerWidth - contentRect.width - 1;
            }

            _popupDiv.style.left = finalLeft + "px";

            if (window.innerHeight - rect.bottom - 1 < contentRect.height)
            {
                var top = rect.top - contentRect.height;
                if (top < 0)
                {
                    if (rect.top > window.innerHeight - rect.bottom - 1)
                    {
                        _popupDiv.style.top = "1px";
                        _popupDiv.style.height = rect.top - 1 + "px";
                    }
                    else
                    {
                        _popupDiv.style.height = window.innerHeight - rect.bottom - 1 + "px";
                    }
                }
                else
                {
                    _popupDiv.style.top = top + "px";
                }
            }
        }

        public override void Hide(Action onHidden = null)
        {
            ClearSearch();
            ResetSearchItems();
            document.removeEventListener("click", OnWindowClick);
            document.removeEventListener("dblclick", OnWindowClick);
            document.removeEventListener("contextmenu", OnWindowClick);
            document.removeEventListener("wheel", OnWindowClick);
            document.removeEventListener("keydown", OnPopupKeyDown);
            base.Hide(onHidden);
            if (_isChanged)
                RaiseOnChange(ev: null);
        }

        public void Attach(EventHandler<Event> handler, Validation.Mode mode)
        {
            if (mode == Validation.Mode.OnBlur)
            {
                onChange += (s, e) => handler(this, e);
            }
            else
            {
                onInput += (s, e) => handler(this, e);
            }
        }

        public Dropdown Single()
        {
            Mode = SelectMode.Single;
            return this;
        }

        public Dropdown Multi()
        {
            Mode = SelectMode.Multi;
            return this;
        }

        public Dropdown NoArrow()
        {
            InnerElement.classList.add("tss-dropdown-noarrow");
            return this;
        }

        public void Clear()
        {
            ClearChildren(ScrollBar.GetCorrectContainer(_childContainer));

            // 2020-06-11 DWR: We need to do this, otherwise the entries in there will relate to drop down items that are no longer rendered - it's fine, since we'll be rebuilding the items (including selected states) if we've just called clear
            _selectedChildren.Clear();
        }

        /// <summary>
        /// This will add items to the available options - note that it will NOT clear any existing ones first (unlike the method overload that takes a Task-returning delegate) and so you may want to call Clear before calling this
        /// </summary>
        public Dropdown Items(params Item[] children)
        {
            // 2020-06-13 DWR: When the items are replaced in an already-existing instance (eg. if a Dropdown instance has already been initialised with items but then the Items are updated - either directly by calling this method or by calling
            // the method overload that takes a Task and then calling LoadItemsAsync.. we still end up here), the selected children data will be taken from the IsSelected values on the children items provided here. That means that if the User
            // had selected two items and then the available items got updated, if it is desirable for those two items to remain selected then the caller has to ensure that it sets IsSelected to true on those two Dropdown.Item instances.
            // - Note: The OnItemSelected would call RaiseOnInput if any items were provided here that were had IsSelected set to true and if the _callSelectOnChangingItemSelections setting is set to true but we might as well hold off on that
            //   and call RaiseOnInput ONCE after ALL of the item changes have been applied. To do that, we'll set _callSelectOnChangingItemSelections to false while we 
            _lastRenderedItems = children;
            children.ForEach(component =>
            {
                ScrollBar.GetCorrectContainer(_childContainer).appendChild(component.Render());
                component._onSelected += OnItemSelected;
            });
            UpdateStateBasedUponCurrentSelections();
            if (!children.Any())
            {
                // If there no options to choose from then ensure that the dropdown list is hidden (it might be open if the User was looking at the options in a multi-select configuration and then the Items were updated in the background)
                // TODO [2020-06-13 DWR]: We should probably introduce a "no items" state that doesn't allow the User to interact with the component since there's nothing that they can do - in the meantime, I'll settle for setting to Disabled
                Hide();
                Disabled(true);
            }
            else
            {
                Disabled(false);
            }
            return this;
        }

        /// <summary>
        /// This will specify an asynchronous callback that describes how to get available options - note that they will not be retrieved until LoadItemsAsync is called (when that successfully gets new item data, any existing items will be
        /// removed first and the list will be completely replace with the new data)
        /// </summary>
        public Dropdown Items(Func<Task<Item[]>> itemsSource)
        {
            _itemsSource = itemsSource;
            return this;
        }

        public Dropdown Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        public Dropdown NoBorder()
        {
            HasBorder = false;
            return this;
        }

        public Dropdown Required()
        {
            IsRequired = true;
            return this;
        }

        private void OnWindowClick(Event e)
        {
            if (e.srcElement != _childContainer && !_childContainer.contains(e.srcElement))
                Hide();
        }

        private void UpdateStateBasedUponCurrentSelections()
        {
            // 2020-06-14 DWR: Whenever redrawing the items in the selected-items bar (after the User has added or removed an option or after the Items data has changed), it's better to clear the list out completely and then add in ALL of the
            // now-selected items because this will list them in the same order in the selected-items bar as they do in the dropdown. This doesn't sound like a huge deal (there could even be an argument for having them appear in the "selected"
            // bar in the order in which the User selected them) but it IS a big problem if the consumer of this component replaces the Items data because it is the responsibility of the caller to know which of the new items should appear as
            // selected - based on the items that were selected here before the update. This is because THIS component only deals with Dropdown.Item instances and NOT the source values and it can't be 100% sure that if an item with text "ABC"
            // was selected before and then new items arrive that any item with text "ABC" should still be selected - only the caller knows strongly-typed values that these dropdown items relate to.
            // ^ This is less of an issue with single-select configurations since they can only show zero or one selections and so ordering is not important
            _selectedChildren.Clear();
            _selectedChildren.AddRange(_lastRenderedItems.Where(item => item.IsSelected));
            RenderSelected();
        }

        private void OnItemSelected(object sender, Item e)
        {
            if (Mode == SelectMode.Single)
            {
                if (e.IsSelected)
                {
                    // For single select drop downs, when one item is selected then we need to ensure here that any other are UN-selected ("there can be only one")
                    foreach (var item in _lastRenderedItems)
                    {
                        if (item != e)
                            item.IsSelected = false;
                    }
                }
                else
                {
                    // If this is an item getting unselected in a Single-only dropdown then it was probably from the "selectedChild.IsSelected = false" call just above and we can ignore it since we're already processing the OnItemSelected logic
                    return;
                }
                Hide();
            }
            else
            {
                _isChanged = true;
            }

            UpdateStateBasedUponCurrentSelections();

            if (_callSelectOnChangingItemSelections)
            {
                RaiseOnInput(ev: null);
            }
        }

        private void RenderSelected()
        {
            ClearChildren(InnerElement);

            for (var i = 0; i < SelectedItems.Length; i++)
            {
                var sel = SelectedItems[i];
                var clone = sel.RenderSelected();
                clone.classList.remove("tss-dropdown-item");
                clone.classList.remove("tss-selected");
                clone.classList.add("tss-dropdown-item-on-box");
                InnerElement.appendChild(clone);
            }
        }

        private void OnPopupKeyDown(Event e)
        {
            var ev = e as KeyboardEvent;

            if (ev.key == "ArrowUp")
            {
                var visibleItems = _childContainer.children.Where(he => ((HTMLElement)he).style.display != "none").ToArray();

                if (_popupDiv.classList.contains("tss-no-focus")) _popupDiv.classList.remove("tss-no-focus");

                if (document.activeElement != null && _childContainer.contains(document.activeElement))
                {
                    if (visibleItems.TakeWhile(x => !x.Equals(document.activeElement)).LastOrDefault(x => (x as HTMLElement).tabIndex != -1) is HTMLElement el)
                    {
                        _firstItem = el;
                    }
                    else
                    {
                        _firstItem = visibleItems.LastOrDefault(x => (x as HTMLElement).tabIndex != -1) as HTMLElement;
                    }
                }
                else
                {
                    _firstItem = visibleItems.LastOrDefault(x => (x as HTMLElement).tabIndex != -1) as HTMLElement;
                }

                if (_firstItem is object)
                {
                    _firstItem.focus();
                }
            }
            else if (ev.key == "ArrowDown")
            {
                var visibleItems = _childContainer.children.Where(he => ((HTMLElement)he).style.display != "none").ToArray();

                if (_popupDiv.classList.contains("tss-no-focus")) _popupDiv.classList.remove("tss-no-focus");

                if (document.activeElement != null && _childContainer.contains(document.activeElement))
                {
                    if (visibleItems.SkipWhile(x => !x.Equals(document.activeElement)).Skip(1).FirstOrDefault(x => (x as HTMLElement).tabIndex != -1) is HTMLElement el)
                    {
                        _firstItem = el;
                    }
                    else
                    {
                        _firstItem = visibleItems.FirstOrDefault(x => (x as HTMLElement).tabIndex != -1) as HTMLElement;
                    }
                }
                else
                {
                    _firstItem = visibleItems.FirstOrDefault(x => (x as HTMLElement).tabIndex != -1) as HTMLElement;
                }

                if (_firstItem is object)
                {
                    _firstItem.focus();
                }
            }
            else
            {
                UpdateSearch(ev);
            }
        }

        public IObservable<IReadOnlyList<Item>> AsObservable()
        {
            return _selectedChildren;
        }

        public enum SelectMode
        {
            Single,
            Multi
        }

        public enum ItemType
        {
            Item,
            Header,
            Divider
        }

        private void UpdateSearch(KeyboardEvent e)
        {
            StopEvent(e);

            if (e.key == "Backspace")
            {
                if (!string.IsNullOrWhiteSpace(_search))
                {
                    _search = _search.Substring(0, _search.Length - 1);
                    SearchItems();
                }
            }
            else if (e.key == "Enter")
            {
                _firstItem?.click();
            }
            else if (e.key == "Escape")
            {
                ClearSearch();
                ResetSearchItems();
                Hide();
            }
            else if (e.key.Length == 1 && Regex.IsMatch(e.key, "[a-z0-9 _\\-.,;:!?\"'/$]", RegexOptions.IgnoreCase))
            {
                _search += e.key;
                SearchItems();
            }

            if (string.IsNullOrWhiteSpace(_search))
            {
                ResetSearchItems();
            }
        }

        private void ResetSearchItems(IEnumerable<(HTMLElement item, string textContent)> itemsToReset = null)
        {
            itemsToReset = itemsToReset ?? GetItems();

            foreach (var item in itemsToReset)
            {
                item.item.style.display = "block";
            }
        }

        private void ClearSearch()
        {
            _search = string.Empty;
        }

        private (HTMLElement item, string textContent)[] GetItems() => _childContainer.children.Select(child => ((HTMLElement)child, child.textContent)).ToArray();

        private void SearchItems()
        {
            var items = GetItems();
            var itemsToRemove = items.Where(item => !(item.textContent.ToLower().Contains(_search.ToLower())));
            var itemsToReset = items.Except(itemsToRemove);
            _firstItem = itemsToReset.FirstOrDefault().item;

            ResetSearchItems(itemsToReset);

            foreach (var itemToRemove in itemsToRemove)
            {
                itemToRemove.item.style.display = "none";
            }
            RecomputePopupPosition();
        }

        public class Item : IComponent
        {
            private readonly HTMLElement InnerElement;
            private readonly HTMLElement SelectedElement;
            public Item(string text, string selectedText = null) : this(TextBlock(text), TextBlock(string.IsNullOrEmpty(selectedText) ? text : selectedText)) { }

            public dynamic Data { get; private set; }

            public Item(IComponent content, IComponent selectedContent)
            {
                InnerElement = Button(_("tss-dropdown-item"));
                InnerElement.appendChild(content.Render());

                if (selectedContent is null || selectedContent == content)
                {
                    SelectedElement = (HTMLElement)InnerElement.cloneNode(true);
                }
                else
                {
                    SelectedElement = Button(_("tss-dropdown-item"));
                    SelectedElement.appendChild(selectedContent.Render());
                }

                InnerElement.addEventListener("click", OnItemClick);
                InnerElement.addEventListener("mouseover", OnItemMouseOver);
            }

            private event BeforeSelectEventHandler<Item> _onBeforeSelected;
            internal event EventHandler<Item> _onSelected;

            public ItemType Type
            {
                get
                {
                    if (InnerElement.classList.contains("tss-dropdown-item")) return ItemType.Item;
                    if (InnerElement.classList.contains("tss-dropdown-header")) return ItemType.Header;
                    return ItemType.Divider;
                }

                set
                {
                    InnerElement.classList.remove($"tss-dropdown-{Type.ToString().ToLower()}");
                    InnerElement.classList.add($"tss-dropdown-{value.ToString().ToLower()}");

                    if (value == ItemType.Item) InnerElement.tabIndex = 0;
                    else InnerElement.tabIndex = -1;
                }
            }

            public bool IsEnabled
            {
                get => !InnerElement.classList.contains("tss-disabled");
                set
                {
                    if (value)
                    {
                        InnerElement.classList.remove("tss-disabled");
                        if (Type == ItemType.Item) InnerElement.tabIndex = 0;
                    }
                    else
                    {
                        InnerElement.classList.add("tss-disabled");
                        InnerElement.tabIndex = -1;
                    }
                }
            }

            public bool IsSelected
            {
                get => InnerElement.classList.contains("tss-selected");
                set
                {
                    if (value && _onBeforeSelected is object)
                    {
                        var shouldSelect = _onBeforeSelected(this);
                        if (!shouldSelect)
                            return;
                    }

                    if (value)
                        InnerElement.classList.add("tss-selected");
                    else
                        InnerElement.classList.remove("tss-selected");

                    // 2020-06-11 DWR: We previously had a check here to only fire this even if the value had changed but that would mean that if you had a single-select drop down and you clicked on the option that was already selected
                    // then nothing would happen (because the value hadn't changed and this callback wouldn't be made) because it's this callback that the parent class uses to hide the drop down list of options. I considered making
                    // adding the logic to OnItemClick instead (because we want a way to say "hide the drop down list if the User clicked an option) but that bypasses  the _onBeforeSelected and that could be useful (we might want
                    // to have a callback that prevents you from clicking the item if.. something). While I removed the was-changed check for single-select configurations, it does not harm for multi as well.
                    _onSelected?.Invoke(this, this);
                }
            }

            public string Text
            {
                get => InnerElement.innerText;
                set => InnerElement.innerText = value;
            }

            public HTMLElement Render()
            {
                return InnerElement;
            }

            public HTMLElement RenderSelected()
            {
                return SelectedElement;
            }

            public Item Header()
            {
                Type = ItemType.Header;
                return this;
            }
            public Item Divider()
            {
                Type = ItemType.Divider;
                return this;
            }
            public Item Disabled(bool value = true)
            {
                IsEnabled = !value;
                return this;
            }

            public Item Selected()
            {
                IsSelected = true;
                return this;
            }

            public Item SelectedIf(bool shouldSelect)
            {
                if (shouldSelect)
                {
                    IsSelected = true;
                }
                return this;
            }

            public Item SetData(dynamic data)
            {
                Data = data;
                return this;
            }

            public Item OnSelected(EventHandler<Item> onSelected, EventHandler<Item> onDeselected = null)
            {
                _onSelected += (s, e) =>
                {
                    if (e.IsSelected)
                    {
                        onSelected?.Invoke(s, e);
                    }
                    else
                    {
                        onDeselected?.Invoke(s, e);
                    }
                };
                return this;
            }

            public Item OnBeforeSelected(BeforeSelectEventHandler<Item> onBeforeSelect)
            {
                _onBeforeSelected += onBeforeSelect;
                return this;
            }

            private void OnItemClick(Event e)
            {
                if (Type == ItemType.Item)
                {
                    if (IsMountedWithinMultiSelectDropdown)
                    {
                        // If this is a multi-select dropdown then clicking an item should toggle its selection on or off
                        IsSelected = !IsSelected;
                    }
                    else
                    {
                        // If this is a single-select dropdown then clciking it will ALWAYS set it to selected (and setting this property will fire off loads of other logic that will result in the dropdown's list of options being hidden)
                        IsSelected = true;
                    }
                }
            }

            private bool IsMountedWithinMultiSelectDropdown => InnerElement.parentElement.classList.contains(_multiSelectDropdownClassName);

            private void OnItemMouseOver(Event ev)
            {
                if (Type == ItemType.Item) InnerElement.focus();
            }
        }
    }
}