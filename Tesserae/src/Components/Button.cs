using System;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The standard clickable button component, with optional icons, loading state, primary/secondary variants and
    /// dropdown / split-button support.
    /// </summary>
    [Transpose.Name("tss.Button")]
    public class Button : ComponentBase<Button, HTMLButtonElement>, ITextFormating, IHasBackgroundColor, IHasForegroundColor, ICanWrap, IRoundedStyle
    {
        private HTMLSpanElement   _textSpan;
        private HTMLElement       _iconSpan;
        private HTMLButtonElement _spinner;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Button(string text = string.Empty)
        {
            _textSpan    = Span(Att(text: text));
            InnerElement = Button(Att("tss-btn tss-btn-default tss-default-component-margin"), _textSpan);
            Weight       = TextWeight.Regular;
            Size         = TextSize.Small;

            AttachClick();
            AttachContextMenu();
            AttachFocus();
            AttachBlur();

            if (string.IsNullOrEmpty(text))
            {
                InnerElement.style.minWidth = "unset";
            }
        }

        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public string Background
        {
            get => InnerElement.style.background;
            set
            {
                InnerElement.style.background = value;
                InnerElement.UpdateClassIf(!string.IsNullOrWhiteSpace(value), "tss-btn-filter-effects");
            }
        }

        /// <summary>
        /// Gets or sets the CSS color (foreground) of the component.
        /// </summary>
        public string Foreground
        {
            get => InnerElement.style.color;
            set => InnerElement.style.color = value;
        }

        /// <summary>
        /// Gets or sets button text
        /// </summary>
        public string Text
        {
            get => _textSpan.innerText;
            set
            {
                bool isEmpty = string.IsNullOrEmpty(value);
                _textSpan.innerText         = value;
                InnerElement.style.minWidth = isEmpty ? "unset" : string.Empty;
                InnerElement.UpdateClassIf(isEmpty, "tss-btn-only-icon");
            }
        }

        /// <summary>
        /// Gets or sets button title
        /// </summary>
        public string Title
        {
            get => InnerElement.title;
            set => InnerElement.title = value;
        }

        /// <summary>
        /// Gets or sets button icon (icon class)
        /// </summary>
        public string Icon
        {
            get => _iconSpan?.className;
            set
            {
                if (string.IsNullOrEmpty(value) && _iconSpan != null)
                {
                    InnerElement.removeChild(_iconSpan);
                    _iconSpan = null;

                    InnerElement.classList.remove("tss-btn-only-icon");

                    return;
                }

                if (_iconSpan == null)
                {
                    _iconSpan = I(Att());
                    InnerElement.insertBefore(_iconSpan, _textSpan);
                }

                _iconSpan.className = value;
                InnerElement.UpdateClassIf(string.IsNullOrEmpty(Text), "tss-btn-only-icon");
            }
        }

        /// <summary>
        /// Gets or set whenever button is rendered in a compact form
        /// </summary>
        public bool IsCompact
        {
            get => InnerElement.classList.contains("tss-small");
            set => InnerElement.UpdateClassIf(value, "tss-small");
        }

        /// <summary>
        /// Gets or set whenever button is rendered like a link
        /// </summary>
        public bool IsLink
        {
            get => InnerElement.classList.contains("tss-btn-link");
            set => InnerElement.UpdateClassIf(value, "tss-btn-link");
        }

        /// <summary>
        /// Gets or set whenever button is primary
        /// </summary>
        public bool IsPrimary
        {
            get => InnerElement.classList.contains("tss-btn-primary");
            set
            {
                if (value)
                {
                    InnerElement.classList.add("tss-btn-primary");
                    InnerElement.classList.remove("tss-btn-default", "tss-btn-success", "tss-btn-danger");
                }
                else
                {
                    InnerElement.classList.add("tss-btn-default");
                    InnerElement.classList.remove("tss-btn-success", "tss-btn-danger", "tss-btn-primary");
                }
            }
        }

        /// <summary>
        /// Gets or set whenever button is primary
        /// </summary>
        public bool IsSuccess
        {
            get => InnerElement.classList.contains("tss-btn-success");
            set
            {
                if (value)
                {
                    InnerElement.classList.add("tss-btn-success");
                    InnerElement.classList.remove("tss-btn-default", "tss-btn-primary", "tss-btn-danger");
                }
                else
                {
                    InnerElement.classList.add("tss-btn-default");
                    InnerElement.classList.remove("tss-btn-success", "tss-btn-danger", "tss-btn-primary");
                }
            }
        }

        /// <summary>
        /// Gets or set whenever button is danger
        /// </summary>
        public bool IsDanger
        {
            get => InnerElement.classList.contains("tss-btn-danger");
            set
            {
                if (value)
                {
                    InnerElement.classList.add("tss-btn-danger");
                    InnerElement.classList.remove("tss-btn-default", "tss-btn-primary", "tss-btn-success");
                }
                else
                {
                    InnerElement.classList.add("tss-btn-default");
                    InnerElement.classList.remove("tss-btn-success", "tss-btn-danger", "tss-btn-primary");
                }
            }
        }

        /// <summary>
        /// Gets or sets whenever button is enabled
        /// </summary>
        public bool IsEnabled
        {
            get => !InnerElement.classList.contains("tss-disabled");
            set => InnerElement.UpdateClassIfNot(value, "tss-disabled");
        }
        /// <summary>
        /// Gets or sets a value indicating whether the component's text can wrap onto multiple lines.
        /// </summary>
        public bool CanWrap
        {
            get => !InnerElement.classList.contains("tss-btn-nowrap");
            set => InnerElement.UpdateClassIfNot(value, "tss-btn-nowrap");
        }


        /// <summary>
        /// Gets or sets a value indicating whether overflowing text is truncated with an ellipsis.
        /// </summary>
        public bool EnableEllipsis
        {
            get => !InnerElement.classList.contains("tss-text-ellipsis");
            set => InnerElement.UpdateClassIf(value, "tss-text-ellipsis");
        }

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
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return InnerElement;
        }


        /// <summary>
        /// Renders the component in a compact form.
        /// </summary>
        public Button Compact()
        {
            IsCompact = true;
            return this;
        }

        /// <summary>
        /// Removes / disables the margin on the component.
        /// </summary>
        public Button NoMargin()
        {
            InnerElement.classList.add("tss-btn-remove-margin");
            return this;
        }

        /// <summary>
        /// Removes / disables the padding on the component.
        /// </summary>
        public Button NoPadding()
        {
            InnerElement.classList.add("tss-btn-remove-padding");
            return this;
        }

        /// <summary>
        /// Reduces the component's padding.
        /// </summary>
        public Button LessPadding()
        {
            InnerElement.classList.add("tss-btn-less-padding");
            return this;
        }

        /// <summary>
        /// Renders the component as a hyperlink.
        /// </summary>
        public Button Link()
        {
            IsLink = true;
            return this;
        }

        /// <summary>
        /// Renders the component as a default-toned hyperlink.
        /// </summary>
        public Button DefaultLink()
        {
            IsLink = true;
            InnerElement.classList.add("tss-dark");
            return this;
        }

        /// <summary>
        /// Renders the component as a danger-toned hyperlink.
        /// </summary>
        public Button DangerLink()
        {
            IsLink = true;
            InnerElement.classList.add("tss-danger");
            return this;
        }

        /// <summary>
        /// Replaces the button's content with an inline spinner (commonly used while an async action is in progress).
        /// </summary>
        public void ToSpinner(string text = null)
        {
            if (_spinner is null)
            {
                var s = (HTMLButtonElement)InnerElement.cloneNode(false);
                _spinner = s;
                _spinner.classList.add("tss-btn-nominsize", "tss-disabled");
                ClearChildren(_spinner);
                _spinner.appendChild(Spinner(text).Medium().Render());

                if (InnerElement.IsMounted())
                {
                    MountSpinner();
                }
                else
                {
                    DomObserver.WhenMounted(InnerElement, () =>
                    {
                        if (_spinner == s)
                        {
                            MountSpinner();
                        }
                    });
                }
            }

            void MountSpinner()
            {
                var rect = (DOMRect)InnerElement.getBoundingClientRect();

                if (InnerElement.HasOwnProperty("_tippy"))
                {
                    Transpose.Script.Write("{0}._tippy.disable();", InnerElement);
                }

                _spinner.style.height = rect.height.px().ToString();
                _spinner.style.width  = rect.width.px().ToString();

                InnerElement.parentElement.replaceChild(_spinner, InnerElement);
            }
        }

        /// <summary>
        /// Restores the button's original content after <see cref="ToSpinner"/> was used.
        /// </summary>
        public void UndoSpinner()
        {
            if (_spinner is object && _spinner.IsMounted())
            {
                _spinner.parentElement.replaceChild(InnerElement, _spinner);

                if (InnerElement.HasOwnProperty("_tippy"))
                {
                    Transpose.Script.Write("{0}._tippy.enable();", InnerElement);
                }
            }

            _spinner = null;
        }

        /// <summary>
        /// Registers a callback invoked when the click spin while event fires.
        /// </summary>
        public Button OnClickSpinWhile(Func<Task> action, string text = null, Action<Button, Exception> onError = null)
        {
            return OnClickSpinWhile((MouseEvent e) => action(), text, onError);
        }

        /// <summary>
        /// Registers a callback invoked when the click spin while event fires.
        /// </summary>
        public Button OnClickSpinWhile(Func<MouseEvent, Task> action, string text = null, Action<Button, Exception> onError = null)
        {
            return OnClick((_, e) =>
            {
                StopEvent(e);

                Task.Run(async () =>
                {
                    Exception innerException = null;
                    ToSpinner(text);

                    try
                    {
                        await action(e);
                    }
                    catch (Exception E)
                    {
                        innerException = E;
                        throw;
                    }
                    finally
                    {
                        UndoSpinner();

                        if (innerException is object)
                        {
                            if (onError is object)
                            {
                                onError(this, innerException);
                            }
                            else
                            {
                                console.error(innerException);
                            }
                        }

                    }
                }).FireAndForget();
            });
        }

        /// <summary>
        /// Registers a callback invoked when the click event fires.
        /// </summary>
        public Button OnClick(Action action) => OnClick((_, e) =>
        {
            StopEvent(e);
            action.Invoke();
        });
                
        /// <summary>
        /// Registers a callback invoked when the context menu event fires.
        /// </summary>
        public Button OnContextMenu(Action action) => OnContextMenu((_, e) =>
        {
            StopEvent(e);
            action.Invoke();
        });

        /// <summary>
        /// Runs the given async action while showing an inline spinner on the button. Restores the original button content on completion (or on error, optionally invoking the supplied error handler).
        /// </summary>
        public void SpinWhile(Func<Task> action, string text = null, Action<Button, Exception> onError = null)
        {
            Task.Run(async () =>
            {
                Exception innerException = null;
                ToSpinner(text);

                try
                {
                    await action();
                }
                catch (Exception e)
                {
                    innerException = e;
                    throw;
                }
                finally
                {
                    UndoSpinner();

                    if (innerException is object)
                    {
                        if (onError is object)
                        {
                            onError(this, innerException);
                        }
                        else
                        {
                            console.error(innerException);
                        }
                    }

                }
                UndoSpinner();
            }).FireAndForget();
        }

        /// <summary>
        /// Styles the component using the primary tone.
        /// </summary>
        public Button Primary()
        {
            IsPrimary = true;
            return this;
        }

        /// <summary>
        /// Styles the component using the success tone.
        /// </summary>
        public Button Success()
        {
            IsSuccess = true;
            return this;
        }

        /// <summary>
        /// Styles the component using the danger tone.
        /// </summary>
        public Button Danger()
        {
            IsDanger = true;
            return this;
        }

        /// <summary>
        /// Returns the component configured with the given filter effects.
        /// </summary>
        public Button WithFilterEffects()
        {
            InnerElement.classList.add("tss-btn-filter-effects");
            return this;
        }

        /// <summary>
        /// Disables the component.
        /// </summary>
        public Button Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        /// <summary>
        /// Removes / disables the border on the component.
        /// </summary>
        public Button NoBorder()
        {
            InnerElement.classList.add("tss-btn-noborder");
            return this;
        }
        /// <summary>
        /// Removes / disables the min size on the component.
        /// </summary>
        public Button NoMinSize()
        {
            InnerElement.classList.add("tss-btn-nominsize");
            return this;
        }

        /// <summary>
        /// Removes / disables the background on the component.
        /// </summary>
        public Button NoBackground()
        {
            InnerElement.classList.add("tss-btn-nobg");
            return this;
        }

        /// <summary>
        /// Renders the component as a hyperlink only on hover.
        /// </summary>
        public Button LinkOnHover()
        {
            InnerElement.classList.add("tss-btn-linkonhover");
            return this;
        }

        /// <summary>
        /// Removes / disables the hover on the component.
        /// </summary>
        public Button NoHover()
        {
            InnerElement.classList.add("tss-btn-nohover");
            return this;
        }

        /// <summary>
        /// Configures the component to color.
        /// </summary>
        public Button Color(string background, string textColor = "white", string borderColor = "white", string iconColor = "")
        {
            InnerElement.classList.add("tss-btn-nobg");
            InnerElement.style.background  = background;
            InnerElement.style.color       = textColor;
            InnerElement.style.borderColor = borderColor;
            InnerElement.classList.add("tss-btn-filter-effects");

            if (_iconSpan is object)
            {
                _iconSpan.style.color = iconColor;
            }
            return this;
        }

        /// <summary>
        /// Sets the text of the component.
        /// </summary>
        public Button SetText(string text)
        {
            Text = text;
            return this;
        }

        /// <summary>
        /// Sets the title of the component.
        /// </summary>
        public Button SetTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary>
        /// Clears the icon.
        /// </summary>
        public Button ClearIcon()
        {
            Icon = null;

            if (_iconSpan is object)
            {
                _iconSpan.style.color = null;

                InnerElement.removeChild(_iconSpan);
            }
            return this;
        }

        internal void OnIconClick(Action<HTMLElement, MouseEvent> action)
        {
            _iconSpan.onclick += (e) =>
            {
                StopEvent(e);
                action(_iconSpan, e);
            };
        }

        /// <summary>
        /// Sets the icon of the component.
        /// </summary>
        public Button SetIcon(Emoji icon, bool afterText = false)
        {
            Icon = $"ec {icon}";

            if (_iconSpan is object)
            {
                _iconSpan.style.color = "";

                if (afterText)
                {
                    InnerElement.removeChild(_iconSpan);
                    InnerElement.appendChild(_iconSpan);
                }
                else
                {
                    InnerElement.insertBefore(_iconSpan, _textSpan);
                }
            }
            return this;
        }

        /// <summary>
        /// Sets the icon of the component.
        /// </summary>
        public Button SetIcon(UIcons icon, string color = "", TextSize size = TextSize.Small, UIconsWeight weight = UIconsWeight.Regular, bool afterText = false)
        {
            Icon = $"{Tesserae.Icon.Transform(icon, weight)} {size}";

            if (_iconSpan is object)
            {
                _iconSpan.style.color = color;

                if (afterText)
                {
                    InnerElement.removeChild(_iconSpan);
                    InnerElement.appendChild(_iconSpan);
                }
                else
                {
                    InnerElement.insertBefore(_iconSpan, _textSpan);
                }
            }
            return this;
        }

        /// <summary>
        /// Configures the icon on hover on the component.
        /// </summary>
        public Button IconOnHover()
        {
            InnerElement.classList.add("tss-btn-icononhover");
            return this;
        }

        /// <summary>
        /// Replaces the content in the component.
        /// </summary>
        public Button ReplaceContent(IComponent content)
        {
            ClearChildren(InnerElement);
            InnerElement.appendChild(content.Render());
            InnerElement.classList.remove("tss-btn-only-icon");
            return this;
        }
        /// <summary>
        /// Replaces the text in the component.
        /// </summary>
        public Button ReplaceText(HTMLSpanElement textSpan)
        {
            InnerElement.replaceChild(textSpan, _textSpan);
            _textSpan = textSpan;
            return this;
        }

        /// <summary>
        /// Allows the component's content to wrap onto multiple lines.
        /// </summary>
        public Button Wrap()
        {
            CanWrap = true;
            return this;
        }

        /// <summary>
        /// Configures the component to ellipsis.
        /// </summary>
        public Button Ellipsis()
        {
            EnableEllipsis = true;
            return this;
        }

        /// <summary>
        /// Removes / disables the wrap on the component.
        /// </summary>
        public Button NoWrap()
        {
            CanWrap = false;
            return this;
        }

        private void RaiseOnClick(Event e, Hotkeys.Handler handler)
        {
            if (IsEnabled)
            {
                RaiseOnClick(ev: null);
            }
        }

        /// <summary>
        /// Moves keyboard focus to the component.
        /// </summary>
        public Button Focus()
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
        /// Returns the component configured with the given hot key.
        /// </summary>
        public Button WithHotKey(string keys, Hotkeys.Option options = null)
        {
            DomObserver.WhenMounted(InnerElement, () =>
            {
                if (options is null)
                {
                    Hotkeys.BindGlobal(keys, RaiseOnClick);
                }
                else
                {
                    Hotkeys.Bind(keys, options, RaiseOnClick);
                }

                DomObserver.WhenRemoved(InnerElement, () =>
                {
                    if (options is null)
                    {
                        Hotkeys.UnbindGlobal(keys, RaiseOnClick);
                    }
                    else
                    {
                        Hotkeys.Unbind(keys, options, RaiseOnClick);
                    }
                });
            });
            return this;
        }
    }
}