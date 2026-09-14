using System;
using Transpose.Core;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A single-line search input with a leading magnifier glyph, a trailing clear button and search-on-enter /
    /// debounced-input semantics.
    /// </summary>
    [Transpose.Name("tss.SearchBox")]
    public class SearchBox : ComponentBase<SearchBox, HTMLInputElement>, ITextFormating, IHasBackgroundColor, ITabIndex, IRoundedStyle
    {
        private readonly HTMLDivElement  _container;
        private readonly HTMLSpanElement _icon;
        private readonly HTMLElement     _iconContainer;
        private readonly HTMLElement     _shortcutContainer;
        private readonly HTMLElement     _clearButton;
        private readonly HTMLElement     _status;
        private readonly HTMLElement     _cancelButton;

        private string[]                       _shortcutKeys;
        private Action<Event>                  _globalShortcutHandler;
        private Action                         _onShortcut;

        protected event SearchEventHandler Searched;
        public delegate void               SearchEventHandler(SearchBox sender, string value);

        protected event SearchCancelledEventHandler SearchCancelled;
        public delegate void                        SearchCancelledEventHandler(SearchBox sender, string value);

        private double _timeoutTriggerSearch = 0;
        private double _timeoutFailure       = 0;
        private string _lastSearchedValue    = string.Empty;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public SearchBox(string placeholder = string.Empty)
        {
            InnerElement = UI.TextBox(Att(className: "tss-searchbox tss-fontsize-small tss-fontweight-regular", type: "search", placeholder: placeholder));
            _icon              = Span(Att(UIcons.Search.ToCssClass()));
            _iconContainer     = Div(Att("tss-searchbox-icon"), _icon);
            _shortcutContainer = Div(Att("tss-searchbox-shortcut"));

            //The browser's own cancel button on a type="search" input is hidden by the stylesheet: it is drawn in
            //the browser's colours and weight rather than the toolkit's, Chromium alone shows it, and it comes and
            //goes with focus. This one is drawn the same way CommandPalette draws its close button, and is
            //shown whenever there is something to clear.
            _clearButton = UI.Button(Att("tss-searchbox-clear", type: "button", title: "Clear", ariaLabel: "Clear"),
                                     I(Att($"tss-searchbox-clear-icon {UIcons.CrossSmall.ToCssClass()}")));

            //Only ever shown by OnCancel, which is what says a running search can be called off at all.
            _cancelButton = UI.Button(Att("tss-searchbox-cancel", type: "button", title: "Cancel search", ariaLabel: "Cancel search"),
                                      I(Att($"tss-searchbox-cancel-icon {UIcons.CrossSmall.ToCssClass()}")));

            //One slot for what became of the last search: the spinner while it is out, the warning glyph when
            //it did not answer, and - on a box that has said it can cancel - the cancel button under a pointer.
            //It stands where the clear button does, which is where the user is already looking.
            _status = Div(Att("tss-searchbox-status"),
                          Div(Att("tss-spinner")),
                          I(Att($"tss-searchbox-failed-icon {UIcons.TriangleWarning.ToCssClass()}")),
                          _cancelButton);

            _container = Div(Att("tss-searchbox-container"), _iconContainer, InnerElement, _status, _clearButton, _shortcutContainer);

            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
            AttachKeys();

            SubscribeInputUpdated((_, __) => UpdateHasText());

            //Pressing the button with the pointer must not take the caret out of the box - the user is about
            //to type the next query.
            _clearButton.addEventListener("mousedown", e => e.preventDefault());

            _clearButton.addEventListener("click", e =>
            {
                StopEvent(e);
                Clear();
            });

            _cancelButton.addEventListener("mousedown", e => e.preventDefault());

            //Raises the event and does nothing else: the box is not emptied and no search is raised, because
            //what calling off a search means is the caller's to say.
            _cancelButton.addEventListener("click", e =>
            {
                StopEvent(e);
                SearchCancelled?.Invoke(this, InnerElement.value);
            });

            //The input is only as tall as its text and sits centred in a box twice its height, with the box's
            //own padding either side of it - so most of what looks like the search box is the container, and
            //a press there would land on nothing. The container hands the press to the input, on mousedown
            //rather than click so the caret appears on the press as it does in a native input, and the default
            //is cancelled so the container does not take focus away from an input already holding it.
            _container.addEventListener("mousedown", e =>
            {
                if (!IsEnabled) return;

                var target = e.target.As<HTMLElement>();

                if (target == InnerElement || _clearButton.contains(target) || _cancelButton.contains(target)) return;

                e.preventDefault();
                InnerElement.focus();
            });

            OnKeyPress((s, e) =>
            {
                if (e.key == "Enter")
                {
                    TriggerSearch();
                }
            });

            //Escape empties a search input in Chromium and raises this event rather than an input event, so the
            //clear button has to be told here too.
            InnerElement.addEventListener("search", (_) =>
            {
                UpdateHasText();
                TriggerSearch();
            });
        }

        /// <summary>
        /// Empties the box, puts the caret in it and raises <see cref="OnSearch"/> with the empty query - what
        /// pressing the clear button does.
        /// </summary>
        public SearchBox Clear()
        {
            if (!IsEnabled) return this;

            InnerElement.value = string.Empty;
            RaiseOnInput(null);
            InnerElement.focus();
            TriggerSearch();

            return this;
        }

        private void UpdateHasText()
        {
            _container.classList.toggle("tss-searchbox-has-text", !string.IsNullOrEmpty(InnerElement.value));
        }

        private void TriggerSearch()
        {
            window.clearTimeout(_timeoutTriggerSearch);
            _timeoutTriggerSearch = window.setTimeout((_) =>
            {
                _lastSearchedValue = InnerElement.value;
                Searched?.Invoke(this, InnerElement.value);
            }, 50);
        }

        /// <summary>
        /// Sets the keyboard tab order of the component.
        /// </summary>
        public int TabIndex
        {
            set
            {
                InnerElement.tabIndex = value;
            }
        }

        /// <summary>
        /// Returns a value indicating whether the component is focused.
        /// </summary>
        public bool IsFocused => document.activeElement == InnerElement;

        /// <summary>
        /// Gets or sets a value indicating whether the component is interactive (enabled).
        /// </summary>
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

        /// <summary>
        /// Returns a value indicating whether the component is underlined.
        /// </summary>
        public bool IsUnderlined
        {
            get => _container.classList.contains("tss-underlined");
            set
            {
                if (value) _container.classList.add("tss-underlined");
                else _container.classList.remove("tss-underlined");
            }
        }

        /// <summary>
        /// Gets or sets the text shown in the component.
        /// </summary>
        public string Text
        {
            get => InnerElement.value;
            set
            {
                InnerElement.value = value;
                RaiseOnInput(null);
            }
        }

        /// <summary>
        /// Gets or sets the placeholder text shown when the component is empty.
        /// </summary>
        public string Placeholder
        {
            get => InnerElement.placeholder;
            set => InnerElement.placeholder = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the component is currently in an invalid state.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the component is waiting on the search it asked for. While
        /// set, a spinner stands where the clear button does and the box reports itself as busy to assistive
        /// technology; the box stays editable throughout. On a box with an <see cref="OnCancel"/> handler, a
        /// pointer on it swaps the spinner for the cancel button.
        /// <para>
        /// It says nothing about <see cref="Failed"/>, which the caller takes down itself - usually with
        /// <see cref="ClearFailure"/> where it starts the next search.
        /// </para>
        /// </summary>
        public bool IsBusy
        {
            get => _container.classList.contains("tss-searchbox-is-busy");
            set
            {
                if (value)
                {
                    _container.classList.add("tss-searchbox-is-busy");
                    InnerElement.setAttribute("aria-busy", "true");
                }
                else
                {
                    _container.classList.remove("tss-searchbox-is-busy");
                    InnerElement.removeAttribute("aria-busy");
                }
            }
        }

        /// <summary>
        /// Returns a value indicating whether the box is showing that its last search did not answer.
        /// </summary>
        public bool IsFailed => _container.classList.contains("tss-searchbox-failed");

        /// <summary>
        /// Gets or sets the size of the component.
        /// </summary>
        public TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
            set
            {
                InnerElement.classList.remove(Size.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the font weight of the component.
        /// </summary>
        public TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                InnerElement.classList.remove(Weight.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the text alignment of the component.
        /// </summary>
        public TextAlign TextAlign
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextAlign.Center);
            set
            {
                InnerElement.classList.remove(TextAlign.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }
        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public string Background { get => _container.style.background; set => _container.style.background = value; }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return _container;
        }

        /// <summary>
        /// Attaches a handler to the component's value-changed event.
        /// </summary>
        public void Attach(ComponentEventHandler<SearchBox> handler)
        {
            SubscribeInputUpdated((s, _) => handler(s));
        }

        /// <summary>
        /// Sets the text of the component.
        /// </summary>
        public SearchBox SetText(string text)
        {
            Text = text;
            return this;
        }

        /// <summary>
        /// Sets the placeholder of the component.
        /// </summary>
        public SearchBox SetPlaceholder(string error)
        {
            Placeholder = error;
            return this;
        }

        /// <summary>
        /// Disables the component.
        /// </summary>
        public SearchBox Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        /// <summary>
        /// Shows (or hides) the spinner that says the box is waiting on its search - see <see cref="IsBusy"/>.
        /// </summary>
        public SearchBox Busy(bool value = true)
        {
            IsBusy = value;
            return this;
        }

        /// <summary>
        /// Says the last search did not answer: the box is outlined in the danger colour and carries a warning
        /// glyph where the spinner was, for <paramref name="millisecondsVisible"/> (pass 0 to leave it up until
        /// something clears it, which is <see cref="ClearFailure"/>). Nothing else takes it down: it describes
        /// one search, so the caller that starts the next one is what says the last one stopped mattering.
        /// <para>
        /// It says that the search itself failed, which is not the same as a query the user should fix; a box
        /// whose *contents* are wrong is <see cref="IsInvalid"/>, and that one stays until it is put right.
        /// </para>
        /// </summary>
        public SearchBox Failed(int millisecondsVisible = 5000)
        {
            window.clearTimeout(_timeoutFailure);

            _container.classList.add("tss-searchbox-failed");

            if (millisecondsVisible > 0)
            {
                _timeoutFailure = window.setTimeout((_) => _container.classList.remove("tss-searchbox-failed"), millisecondsVisible);
            }

            return this;
        }

        /// <summary>
        /// Takes down the failure set by <see cref="Failed"/> now, rather than when it would have expired.
        /// </summary>
        public SearchBox ClearFailure()
        {
            window.clearTimeout(_timeoutFailure);
            _container.classList.remove("tss-searchbox-failed");

            return this;
        }

        /// <summary>
        /// Configures the component to underlined.
        /// </summary>
        public SearchBox Underlined()
        {
            IsUnderlined = true;
            return this;
        }

        /// <summary>
        /// Removes the underline from the component.
        /// </summary>
        public SearchBox NotUnderlined()
        {
            IsUnderlined = false;
            return this;
        }

        /// <summary>
        /// Sets the icon of the component.
        /// </summary>
        public SearchBox SetIcon(UIcons icon)
        {
            _icon.className = icon.ToCssClass();
            return this;
        }


        /// <summary>
        /// Removes / disables the icon on the component.
        /// </summary>
        public SearchBox NoIcon()
        {
            _container.classList.add("tss-noicon");
            return this;
        }

        /// <summary>
        /// Moves keyboard focus to the component.
        /// </summary>
        public SearchBox Focus()
        {
            DomObserver.WhenMounted(InnerElement, () => InnerElement.focus());
            return this;
        }

        /// <summary>
        /// Configures the search box to fire its callback after every keystroke (debounced) rather than only on submit.
        /// </summary>
        public SearchBox SearchAsYouType()
        {
            OnKeyUp((s, e) =>
            {
                if (e.altKey || e.ctrlKey || e.metaKey || e.key == "Enter" || e.key == "Escape")
                {
                    return;
                }
                if (InnerElement.value == _lastSearchedValue)
                {
                    return;
                }
                TriggerSearch();
            });

            InnerElement.attributes["incremental"] = true;

            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the search event fires.
        /// </summary>
        public SearchBox OnSearch(SearchEventHandler onSearch)
        {
            Searched += onSearch;
            return this;
        }

        /// <summary>
        /// Registers what to do when the user calls off a search that is still running, and by registering it
        /// says a search <em>can</em> be called off: while <see cref="IsBusy"/> a pointer on the box then swaps
        /// the spinner for a cancel button, in the same place and at the same size, and pressing it raises this
        /// and nothing else.
        /// <para>
        /// The box is not emptied and no search is raised - a handler that wants either does it itself
        /// (<c>box.Clear()</c> empties it and raises <see cref="OnSearch"/> with the empty query). Without a
        /// handler there is no cancel button and a busy box keeps its clear button, since nothing would be
        /// offered in its place.
        /// </para>
        /// </summary>
        public SearchBox OnCancel(SearchCancelledEventHandler onCancel)
        {
            SearchCancelled += onCancel;
            _container.classList.add("tss-searchbox-can-cancel");

            return this;
        }

        /// <summary>
        /// Registers a global keyboard shortcut that focuses this SearchBox when pressed,
        /// and renders a visual chip showing the shortcut on the right side of the box.
        /// Modifier names are case-insensitive ("Ctrl", "Cmd", "Meta", "Alt", "Shift").
        /// Example: <c>SetKeyboardShortcut("Ctrl", "K")</c>.
        /// </summary>
        public SearchBox SetKeyboardShortcut(params string[] keys)
        {
            if (_globalShortcutHandler is object)
            {
                window.removeEventListener("keydown", _globalShortcutHandler);
                _globalShortcutHandler = null;
            }

            while (_shortcutContainer.firstChild is object)
            {
                _shortcutContainer.removeChild(_shortcutContainer.firstChild);
            }

            if (keys is null || keys.Length == 0)
            {
                _shortcutKeys = null;
                _container.classList.remove("tss-searchbox-has-shortcut");
                return this;
            }

            _shortcutKeys = keys;
            _shortcutContainer.appendChild(KeyboardShortcut(keys).Render());
            _container.classList.add("tss-searchbox-has-shortcut");

            _globalShortcutHandler = ev =>
            {
                if (!_container.IsMounted()) return;
                if (!IsEnabled) return;

                var e = ev.As<KeyboardEvent>();
                if (!KeyboardShortcut.Matches(e, _shortcutKeys)) return;

                StopEvent(e);

                //A box that leads somewhere - one whose shortcut opens a palette or a page - is pressed
                //rather than focused: focusing a box the user is not going to type in would only make the
                //next keystroke go nowhere, and pressing the key again while it already had focus would
                //do nothing at all.
                if (_onShortcut is object)
                {
                    _onShortcut();
                    return;
                }

                InnerElement.focus();
                InnerElement.select();
            };

            window.addEventListener("keydown", _globalShortcutHandler);
            return this;
        }

        /// <summary>
        /// What the shortcut set by <see cref="SetKeyboardShortcut"/> does instead of focusing the box - for
        /// a box that stands for a search happening somewhere else. Pass null to have it focus again.
        /// </summary>
        public SearchBox OnShortcut(Action onShortcut)
        {
            _onShortcut = onShortcut;

            return this;
        }

        /// <summary>
        /// Gets or sets the CSS height of the component.
        /// </summary>
        public SearchBox Height(UnitSize unitSize)
        {
            var h = unitSize.ToString();
            InnerElement.style.height     = h;
            InnerElement.style.lineHeight = h;
            _container.style.height       = h;
            return this;
        }

        /// <summary>
        /// Shortcut for setting the height in pixels.
        /// </summary>
        public SearchBox H(int unitSize) => Height(unitSize.px());
    }
}