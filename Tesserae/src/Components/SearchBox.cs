using System;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.SearchBox")]
    public class SearchBox : ComponentBase<SearchBox, HTMLInputElement>, ITextFormating, IHasBackgroundColor, ITabIndex, IRoundedStyle
    {
        private readonly HTMLDivElement  _container;
        private readonly HTMLSpanElement _icon;
        private readonly HTMLElement     _iconContainer;
        private readonly HTMLElement     _shortcutContainer;
        private readonly HTMLElement     _paddingContainer;

        private string[]                       _shortcutKeys;
        private Action<Event>                  _globalShortcutHandler;

        protected event SearchEventHandler Searched;
        public delegate void               SearchEventHandler(SearchBox sender, string value);

        private double _timeoutTriggerSearch = 0;
        private string _lastSearchedValue    = string.Empty;

        public SearchBox(string placeholder = string.Empty)
        {
            InnerElement       = TextBox(_(className: "tss-searchbox tss-fontsize-small tss-fontweight-regular", type: "search", placeholder: placeholder));
            _icon              = Span(_(UIcons.Search.ToString()));
            _iconContainer     = Div(_("tss-searchbox-icon"), _icon);
            _shortcutContainer = Div(_("tss-searchbox-shortcut"));
            _paddingContainer  = Div(_("tss-searchbox-padding"));
            _container         = Div(_("tss-searchbox-container"), _iconContainer, InnerElement, _shortcutContainer, _paddingContainer);

            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
            AttachKeys();

            OnKeyPress((s, e) =>
            {
                if (e.key == "Enter")
                {
                    TriggerSearch();
                }
            });

            InnerElement.addEventListener("search", (_) =>
            {
                TriggerSearch();
            });
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

        public int TabIndex
        {
            set
            {
                InnerElement.tabIndex = value;
            }
        }

        public bool IsFocused => document.activeElement == InnerElement;

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

        public bool IsUnderlined
        {
            get => _container.classList.contains("tss-underlined");
            set
            {
                if (value) _container.classList.add("tss-underlined");
                else _container.classList.remove("tss-underlined");
            }
        }

        public string Text
        {
            get => InnerElement.value;
            set
            {
                InnerElement.value = value;
                RaiseOnInput(null);
            }
        }

        public string Placeholder
        {
            get => InnerElement.placeholder;
            set => InnerElement.placeholder = value;
        }

        public bool IsInvalid
        {
            get => _container.classList.contains("tss-invalid");
            set
            {
                if (value)
                {
                    _container.classList.add("tss-invalid");
                    InnerElement.setAttribute("aria-invalid", "true");
                }
                else
                {
                    _container.classList.remove("tss-invalid");
                    InnerElement.removeAttribute("aria-invalid");
                }
            }
        }

        public TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
            set
            {
                InnerElement.classList.remove(Size.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        public TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                InnerElement.classList.remove(Weight.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        public TextAlign TextAlign
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextAlign.Center);
            set
            {
                InnerElement.classList.remove(TextAlign.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }
        public string Background { get => _container.style.background; set => _container.style.background = value; }

        public override HTMLElement Render()
        {
            return _container;
        }

        public void Attach(ComponentEventHandler<SearchBox> handler)
        {
            InputUpdated += (s, _) => handler(s);
        }

        public SearchBox SetText(string text)
        {
            Text = text;
            return this;
        }

        public SearchBox SetPlaceholder(string error)
        {
            Placeholder = error;
            return this;
        }

        public SearchBox Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        public SearchBox Underlined()
        {
            IsUnderlined = true;
            return this;
        }

        public SearchBox NotUnderlined()
        {
            IsUnderlined = false;
            return this;
        }

        public SearchBox SetIcon(UIcons icon)
        {
            _icon.className = icon.ToString();
            return this;
        }


        public SearchBox NoIcon()
        {
            _container.classList.add("tss-noicon");
            return this;
        }

        public SearchBox Focus()
        {
            DomObserver.WhenMounted(InnerElement, () => InnerElement.focus());
            return this;
        }

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

        public SearchBox OnSearch(SearchEventHandler onSearch)
        {
            Searched += onSearch;
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
                if (!MatchesShortcut(e, _shortcutKeys)) return;

                StopEvent(e);
                InnerElement.focus();
                InnerElement.select();
            };

            window.addEventListener("keydown", _globalShortcutHandler);
            return this;
        }

        private static bool MatchesShortcut(KeyboardEvent e, string[] keys)
        {
            bool needCtrl  = false;
            bool needAlt   = false;
            bool needShift = false;
            bool needMeta  = false;
            string mainKey = null;

            foreach (var raw in keys)
            {
                if (raw is null) continue;
                var k = raw.Trim();
                switch (k)
                {
                    case "Ctrl":
                    case "ctrl":
                    case "Control":
                        needCtrl = true;
                        break;
                    case "Alt":
                    case "alt":
                        needAlt = true;
                        break;
                    case "Shift":
                    case "shift":
                        needShift = true;
                        break;
                    case "Meta":
                    case "meta":
                    case "Cmd":
                    case "cmd":
                        needMeta = true;
                        break;
                    default:
                        mainKey = k;
                        break;
                }
            }

            // On macOS, treat "Ctrl" in the shortcut as either Ctrl or Cmd so a single
            // declaration like ("Ctrl", "K") renders ⌘K and triggers on Cmd+K.
            bool isApple = navigator.userAgent.IndexOf("Mac") >= 0 || navigator.userAgent.IndexOf("iPhone") >= 0 || navigator.userAgent.IndexOf("iPad") >= 0;

            if (needCtrl && !needMeta && isApple)
            {
                if (!e.metaKey && !e.ctrlKey) return false;
            }
            else
            {
                if (needCtrl != e.ctrlKey) return false;
                if (needMeta != e.metaKey) return false;
            }

            if (needAlt   != e.altKey)   return false;
            if (needShift != e.shiftKey) return false;

            if (mainKey is null) return false;

            return string.Equals(e.key, mainKey, StringComparison.OrdinalIgnoreCase);
        }

        public SearchBox Height(UnitSize unitSize)
        {
            var h = unitSize.ToString();
            InnerElement.style.height     = h;
            InnerElement.style.lineHeight = h;
            _container.style.height       = h;
            return this;
        }

        public SearchBox H(int unitSize) => Height(unitSize.px());
    }
}