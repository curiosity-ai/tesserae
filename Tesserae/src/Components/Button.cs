﻿using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Button")]
    public class Button : ComponentBase<Button, HTMLButtonElement>, ITextFormating, IHasBackgroundColor, IHasForegroundColor, ICanWrap
    {
        private  HTMLSpanElement   _textSpan;
        private  HTMLElement       _iconSpan;
        private  HTMLButtonElement _spinner;

        public Button(string text = string.Empty)
        {
            _textSpan    = Span(_(text: text));
            InnerElement = Button(_("tss-btn tss-btn-default tss-default-component-margin"), _textSpan);
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

        public string Background
        {
            get => InnerElement.style.background;
            set => InnerElement.style.background = value;
        }

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
                    _iconSpan = I(_());
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
        public bool CanWrap
        {
            get => !InnerElement.classList.contains("tss-btn-nowrap");
            set => InnerElement.UpdateClassIfNot(value, "tss-btn-nowrap");
        }


        public bool EnableEllipsis
        {
            get => !InnerElement.classList.contains("tss-text-ellipsis");
            set => InnerElement.UpdateClassIf(value, "tss-text-ellipsis");
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

        public override HTMLElement Render()
        {
            return InnerElement;
        }


        public Button Compact()
        {
            IsCompact = true;
            return this;
        }

        public Button NoMargin()
        {
            InnerElement.classList.add("tss-btn-remove-margin");
            return this;
        }

        public Button NoPadding()
        {
            InnerElement.classList.add("tss-btn-remove-padding");
            return this;
        }

        public Button LessPadding()
        {
            InnerElement.classList.add("tss-btn-less-padding");
            return this;
        }

        public Button Link()
        {
            IsLink = true;
            return this;
        }

        public Button DefaultLink()
        {
            IsLink = true;
            InnerElement.classList.add("tss-dark");
            return this;
        }

        public Button DangerLink()
        {
            IsLink = true;
            InnerElement.classList.add("tss-danger");
            return this;
        }

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
                    H5.Script.Write("{0}._tippy.disable();", InnerElement);
                }

                _spinner.style.height = rect.height.px().ToString();

                InnerElement.parentElement.replaceChild(_spinner, InnerElement);
            }
        }

        public void UndoSpinner()
        {
            if (_spinner is object && _spinner.IsMounted())
            {
                _spinner.parentElement.replaceChild(InnerElement, _spinner);

                if (InnerElement.HasOwnProperty("_tippy"))
                {
                    H5.Script.Write("{0}._tippy.enable();", InnerElement);
                }
            }

            _spinner = null;
        }

        public Button OnClickSpinWhile(Func<Task> action, string text = null, Action<Button, Exception> onError = null)
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
                        await action();
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

        public Button OnClick(Action action) => OnClick((_, e) =>
        {
            StopEvent(e);
            action.Invoke();
        });
        public Button OnContextMenu(Action action) => OnContextMenu((_, e) =>
        {
            StopEvent(e);
            action.Invoke();
        });

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

        public Button Primary()
        {
            IsPrimary = true;
            return this;
        }

        public Button Success()
        {
            IsSuccess = true;
            return this;
        }

        public Button Danger()
        {
            IsDanger = true;
            return this;
        }

        public Button Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        public Button NoBorder()
        {
            InnerElement.classList.add("tss-btn-noborder");
            return this;
        }
        public Button NoMinSize()
        {
            InnerElement.classList.add("tss-btn-nominsize");
            return this;
        }

        public Button NoBackground()
        {
            InnerElement.classList.add("tss-btn-nobg");
            return this;
        }

        public Button LinkOnHover()
        {
            InnerElement.classList.add("tss-btn-linkonhover");
            return this;
        }

        public Button NoHover()
        {
            InnerElement.classList.add("tss-btn-nohover");
            return this;
        }

        public Button Color(string background, string textColor = "white", string borderColor = "white", string iconColor = "")
        {
            InnerElement.classList.add("tss-btn-nobg");
            InnerElement.style.background  = background;
            InnerElement.style.color       = textColor;
            InnerElement.style.borderColor = borderColor;

            if (_iconSpan is object)
            {
                _iconSpan.style.color = iconColor;
            }
            return this;
        }

        public Button SetText(string text)
        {
            Text = text;
            return this;
        }

        public Button SetTitle(string title)
        {
            Title = title;
            return this;
        }

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

        public Button IconOnHover()
        {
            InnerElement.classList.add("tss-btn-icononhover");
            return this;
        }

        public Button ReplaceContent(IComponent content)
        {
            ClearChildren(InnerElement);
            InnerElement.appendChild(content.Render());
            InnerElement.classList.remove("tss-btn-only-icon");
            return this;
        }
        public Button ReplaceText(HTMLSpanElement textSpan)
        {
            InnerElement.replaceChild(textSpan, _textSpan);
            _textSpan = textSpan;
            return this;
        }

        public Button Wrap()
        {
            CanWrap = true;
            return this;
        }

        public Button Ellipsis()
        {
            EnableEllipsis = true;
            return this;
        }

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