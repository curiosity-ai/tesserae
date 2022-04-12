using H5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Dropdown")]
    public sealed class Dropdown : Layer<Dropdown>, ICanValidate<Dropdown>, IObservableListComponent<Dropdown.Item>, ITabIndex
    {
        private const string _multiSelectDropdownClassName = "tss-dropdown-multi";

        private static HTMLElement _firstItem;

        private readonly HTMLElement _childContainer;
        private readonly HTMLDivElement _container;
        private readonly HTMLSpanElement _noItemsSpan;
        private readonly HTMLSpanElement _errorSpan;
        private readonly ObservableList<Item> _selectedChildren;
        private IComponent _placeholder;

        private HTMLDivElement _spinner;
        private bool _isChanged;
        private bool _callSelectOnChangingItemSelections;
        private Func<Task<Item[]>> _itemsSource;
        private ReadOnlyArray<Item> _lastRenderedItems;
        private HTMLDivElement _popupDiv;
        private string _search;
        private int _latestRequestID;

        public Dropdown(HTMLSpanElement noItemsSpan = null)
        {
            _noItemsSpan = noItemsSpan ?? Span(_(text: "There are no options available"));

            InnerElement = Div(_("tss-dropdown"), _noItemsSpan);

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

            _latestRequestID = 0;
        }

        public Dropdown SuppressSelectedOnChangingItemSelections()
        {
            _callSelectOnChangingItemSelections = false;
            return this;
        }

        public int TabIndex
        {
            set
            {
                InnerElement.tabIndex = value;
            }
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

        public Dropdown Focus()
        {
            // 2020-12-29 DWR: Seems like this setTimeout is required then the element is rendered within a container that uses "simplebar" scrolling - without the delay, if the element getting focus is out of view then it will not be
            // scrolled into view (even though it has successfully received focus)
            DomObserver.WhenMounted(InnerElement, () => 
            {
                try
                {
                    InnerElement.scrollIntoViewIfNeeded();
                }
                catch
                {
                    InnerElement.scrollIntoView();
                }
                InnerElement.focus(); 
            });
            return this;
        }

        /// <summary>
        /// This will initiate the last async data retrieval specified via a call to the Items method overload that takes a Func-of-Task-of-array-of-T. If there has not been call to that method (or if LoadItemsAsync has already been called after
        /// it was called) then this will throw an InvalidOperationException. If another async data retrieval had already been initiated but has not completed yet, its results will be ignored when it DOES complete because this call came after it
        /// and it is presumed that this data will be more current).
        /// </summary>
        public async Task LoadItemsAsync()
        {
            if (_itemsSource is null)
                throw new InvalidOperationException("Only valid with async items");

            // Each request (whether sync or async) will get a unique and incrementing ID - if requests overlap then the results of requests that were initiated later are preferred as they are going to be the results of interactions that User
            // performed since the earlier requests started
            var currentRequestID = ++_latestRequestID;

            var itemsSourceLocal = _itemsSource;
            _itemsSource = null; // Clear so we don't call this twice

            // Ensure that the loading (aka spinner) state is being shown to indicate work in the background - if this async task runs to completion and no other request (either sync or async) is made then this state will be removed when the
            // synchronous Items method is called. If another retrieval supercedes this one (by starting after it and thus having a larger currentRequestID value) then that retrieval will call the synchronous Items method (unless it, too, is
            // superceded) and the loading visual state will be removed then.
            EnsureAsyncLoadingStateEnabled();

            var items = await itemsSourceLocal();
            if (currentRequestID != _latestRequestID)
            {
                // If another (async-)Items/LoadItemsAsync or synchronous Items call was made after the retrieving of this data began then presume that it is more recent and we should ignore this data (it will be the responsibility of the
                // later request that 'wins' to update the items and disable any loading state)
                return;
            }

            Items(items);
        }

        public override Dropdown Show()
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
                    return this;
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

            return this;
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

        public void Attach(ComponentEventHandler<Dropdown> handler)
        {
            InputUpdated += (s, _) => handler(this);
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

        /// <summary>
        /// This will set items to the available options, replacing any that are already rendered (and meaning that any async data retrievals that have started but not completed yet will be ignored when they DO complete because this call came
        /// after it and it is presumed that this data will be more current)
        /// </summary>
        public Dropdown Items(params Item[] children)
        {
            // 2020-06-17 DWR: Whenever new items are presented to render, remove any previous ones - there used to be a distinct "Clear" method that did this that would be automatically called when loading items async but NOT when calling this
            // method directly to synchronously update items, which was both inconsistent and surprising behaviour. I checked the code base here and where we use this library and we only called Clear from within this class and once in external
            // code.. just before adding new items via this method, so by incorporating it here it makes everything simpler.
            // - IMPORTANT: If we went back to having a distinct "Clear" method that could be called externally then it would need to incorporate similar logic to that found here; it should increment the "latestRequestID" value (because an items
            //              update is effectively being said to take place that should supercede any async requests that have started but not completed and the loading state, current-selections-bar state and no-items state all would need to be
            //              maintained correctly). If we ever wanted to add a Clear method for some reason then the best implementation be for it to call this method directly with an empty children array, which probably begs the question WHY
            //              you would ever want it in the first place if calling this method automatically replaces any existing items.
            ClearChildren(ScrollBar.GetCorrectContainer(_childContainer));

            // 2020-06-11 DWR: We need to do this, otherwise the entries in there will relate to drop down items that are no longer rendered - it's fine, since we'll be rebuilding the items (including selected states) if we've just called clear
            // TODO [2020-07-01 DWR]: It doesn't LOOK to me like this is required any more since we will always call it in UpdateStateBasedUponCurrentSelections a little further below.. but I want to test with it removed before I'm fully confident
            _selectedChildren.Clear();

            // Each request (whether sync or async) will get a unique and incrementing ID - if requests overlap then the results of requests that were initiated later are preferred as they are going to be the results of interactions that User
            // performed since the earlier requests started (since this code is browser-based, and so single-threaded, it's only possible for async requests to overlap - synchronous requests never can - but it's important to increment the
            // "latestRequestID" value here to ensure that if a synchronous Items call is made after an async retrieval is initiated (but before it completes) that the later-made synchronous call "wins".
            _latestRequestID++;

            // 2020-06-13 DWR: When the items are replaced in an already-existing instance (eg. if a Dropdown instance has already been initialised with items but then the Items are updated - either directly by calling this method or by calling
            // the method overload that takes a Task and then calling LoadItemsAsync.. we still end up here), the selected children data will be taken from the IsSelected values on the children items provided here. That means that if the User
            // had selected two items and then the available items got updated, if it is desirable for those two items to remain selected then the caller has to ensure that it sets IsSelected to true on those two Dropdown.Item instances.
            // - Note: The OnItemSelected would call RaiseOnInput if any items were provided here that were had IsSelected set to true and if the _callSelectOnChangingItemSelections setting is set to true but we might as well hold off on that
            //   and call RaiseOnInput ONCE after ALL of the item changes have been applied. To do that, we'll set _callSelectOnChangingItemSelections to false while we 
            _lastRenderedItems = children;
            children.ForEach(component =>
            {
                ScrollBar.GetCorrectContainer(_childContainer).appendChild(component.Render());
                component.SelectedItem += OnItemSelected;
            });
            EnsureAsyncLoadingStateDisabled(); // If we got here because an async request completed OR while one was in flight but a synchronous call to this method came in after it started but before finishing then ensure to remove its loading state
            UpdateStateBasedUponCurrentSelections();
            if (children.Any())
            {
                Disabled(false);
                _noItemsSpan.style.display = "none";
            }
            else
            {
                Hide();
                Disabled(true);
                _noItemsSpan.style.display = "";
            }
            return this;
        }

        /// <summary>
        /// This will specify an asynchronous callback that describes how to get available options - note that they will not be retrieved until LoadItemsAsync is called (when that successfully gets new item data, any existing items will be
        /// removed first and the list will be completely replace with the new data). LoadItemsAsync may be called explicitly (and immediately after setting this) - if not, it will be called automatically when the User clicks to open the dropdown.
        /// </summary>
        public Dropdown Items(Func<Task<Item[]>> itemsSource)
        {
            // 2020-06-30 DWR: We should only show the no-items message if we KNOW that there are no options to select and if we've just specified an async retrieval then we won't know whether there are any items or not until it completes - so we'll
            // ensure that the message is hidden here and then it will be displayed/hidden as appropriate when the async retrieval completes (the non-async Items method will be called and that updates the display state of the no-items message)
            _noItemsSpan.style.display = "none";
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

        public Dropdown NoBackground()
        {
            _container.classList.add("tss-dropdown-nobg");
            return this;
        }

        public Dropdown Required()
        {
            IsRequired = true;
            return this;
        }


        public Dropdown Placeholder(string text)
        {
            _placeholder = TextBlock(text).Secondary();
            return this;
        }

        public Dropdown Placeholder(IComponent placeholder)
        {
            _placeholder = placeholder;
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

        private void OnItemSelected(Item sender)
        {
            if (Mode == SelectMode.Single)
            {
                if (sender.IsSelected)
                {
                    // For single select drop downs, when one item is selected then we need to ensure here that any other are UN-selected ("there can be only one")
                    foreach (var item in _lastRenderedItems)
                    {
                        if (item != sender)
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

            if (SelectedItems.Any())
            {
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
            else
            {
                if(_placeholder is object)
                {
                    var rendered = _placeholder.Render();
                    rendered.classList.add("tss-dropdown-item-on-box");
                    InnerElement.appendChild(rendered);
                }
            }

            // 2020-06-30 DWR: This may or may not be visible right now, it doesn't matter - we just need to ensure that we add it back in after clearing InnerElement
            // - Note: Put it at the end of the list because there is a styling rule for selections within InnerElement that sayas to add a comma before the item if it's not the first one and that styling will see the no-items element as the first item otherwise :(
            InnerElement.appendChild(_noItemsSpan);
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

        [Enum(Emit.StringName)] //Don't change the emit type without updating the FromClassList method
        public enum ItemType
        {
            [Name("tss-contextmenu-item")] Item,
            [Name("tss-contextmenu-header")] Header,
            [Name("tss-contextmenu-divider")] Divider
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
                RecursiveUnhighlight(item.item);
            }
        }

        private void ClearSearch() => _search = string.Empty;

        /// <summary>
        /// When a LoadItemsAsync call starts, there should be a spinner to indicate that something is happening in the background - but if a further async request comes in before the previous one has completed then there is no
        /// need to change anything as the spinner state will already be enabled. When multiple async requests overlap, which was started later will take precedence and the results of the earlier-started one will be ignored -
        /// when the 'winning' request completes, it will call the synchronous Items method and that will ensure that any loading state is disabled.
        /// </summary>
        private void EnsureAsyncLoadingStateEnabled()
        {
            if (_spinner is object)
                return;

            _spinner = Div(_("tss-spinner"));
            _container.appendChild(_spinner);
            _container.style.pointerEvents = "none";
        }

        /// <summary>
        /// When data is successfully retrieved from a LoadItemsAsync call, it will call the synchronous Items method that will call this and ensure that any spinner state is disabled. This happens in the simple case where
        /// there is only that single async retrieval and it runs to completion and it also happens if it is superceded by a more recent LoadItemsAsync call or by a separate synchronous Items call. 
        /// </summary>
        private void EnsureAsyncLoadingStateDisabled()
        {
            if (_spinner is null)
                return;

            _spinner.remove();
            _container.style.pointerEvents = "unset";
        }

        private (HTMLElement item, string textContent)[] GetItems() => _childContainer.children.Select(child => ((HTMLElement)child, child.textContent)).ToArray();

        private void SearchItems()
        {
            var searchTerm = _search.Trim().ToLower();

            var items = GetItems();
            var itemsToRemove = items.Where(item => !(item.textContent.ToLower().Contains(searchTerm)));
            var itemsToReset = items.Except(itemsToRemove);
            _firstItem = itemsToReset.FirstOrDefault().item;

            ResetSearchItems(itemsToReset);

            foreach (var (item, textContent) in itemsToRemove)
            {
                item.style.display = "none";
            }

            var regex = new Regex("(" + Regex.Escape(searchTerm) + ")", RegexOptions.IgnoreCase);
            foreach(var (item, _) in itemsToReset)
            {
                RecursiveUnhighlight(item);
                if(searchTerm.Length > 0)
                {
                    RecursiveHighlight(item, regex);
                }
            } 

            RecomputePopupPosition();
        }

        private static void RecursiveHighlight(HTMLElement baseElement, Regex highlighter)
        {
            if (baseElement.childElementCount > 0)
            {
                foreach (var e in baseElement.children)
                {
                    RecursiveHighlight((HTMLElement)e, highlighter);
                }
            }
            else
            {
                if (highlighter.IsMatch(baseElement.textContent))
                {
                    var txt = baseElement.textContent;
                    baseElement.textContent = "";
                    baseElement.innerHTML = highlighter.Replace(txt, "<mark>$1</mark>");
                }
            }
        }

        private static void RecursiveUnhighlight(HTMLElement baseElement)
        {
            if(baseElement.tagName == "MARK")
            {
                var newChild = document.createTextNode(baseElement.textContent);
                baseElement.parentElement.replaceChild(newChild, baseElement);
            }
            else if (baseElement.childElementCount > 0)
            {
                foreach (var e in baseElement.children)
                {
                    RecursiveUnhighlight((HTMLElement)e);
                }
            }
        }

        public sealed class Item : IComponent
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

            private event BeforeSelectEventHandler<Item> BeforeSelectedItem;
            internal event ComponentEventHandler<Item> SelectedItem;

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
                    InnerElement.classList.remove(Type.ToString());
                    InnerElement.classList.add(value.ToString());

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
                    if (value && BeforeSelectedItem is object)
                    {
                        var shouldSelect = BeforeSelectedItem(this);
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
                    SelectedItem?.Invoke(this);
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

            public HTMLElement RenderSelected() => SelectedElement;

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

            public Item OnSelected(ComponentEventHandler<Item> whenSelected, ComponentEventHandler<Item> whenDeselected = null)
            {
                SelectedItem += sender =>
                {
                    if (sender.IsSelected)
                    {
                        whenSelected?.Invoke(sender);
                    }
                    else
                    {
                        whenDeselected?.Invoke(sender);
                    }
                };
                return this;
            }

            public Item OnBeforeSelected(BeforeSelectEventHandler<Item> onBeforeSelect)
            {
                BeforeSelectedItem += onBeforeSelect;
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
                if (Type == ItemType.Item)
                    InnerElement.focus();
            }
        }
    }
}