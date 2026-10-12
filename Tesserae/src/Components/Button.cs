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
    public class Button : ComponentBase<Button, HTMLElement>, ITextFormating, IHasBackgroundColor, IHasForegroundColor, ICanWrap, IRoundedStyle
    {
        private HTMLSpanElement   _textSpan;
        private HTMLElement       _iconSpan;
        private HTMLElement       _spinOverlay;
        private HTMLElement       _spinLabel;
        private bool              _isSpinningInPlace;
        private int               _spinGeneration;

        /// <summary>
        /// Initializes a new instance of this class. Give it an <paramref name="href"/> and it becomes a real
        /// link - an anchor rather than a button - so it is middle-clickable and shows where it goes in the
        /// status bar, while looking exactly like any other button.
        /// </summary>
        public Button(string text = string.Empty, string href = null)
        {
            const string CLASSES = "tss-btn tss-btn-default tss-default-component-margin";

            _textSpan    = Span(Att(text: text));
            InnerElement = string.IsNullOrEmpty(href) ? (HTMLElement)Button(Att(CLASSES), _textSpan) : A(Att(CLASSES, href: href), _textSpan);
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

                if (InnerElement.classList.contains(HIDE_TEXT_ON_MOBILE_CLASS)) UpdateMobileLabel();
            }
        }

        private const string HIDE_TEXT_ON_MOBILE_CLASS = "tss-btn-hide-text-mobile";

        /// <summary>
        /// Shows only the button's icon while the page is in mobile mode (<see cref="UI.Theme.IsMobileMode"/>), for
        /// a toolbar that has room for every command's label on a desktop and for none of them on a phone. A button
        /// without an icon keeps its text, since it would otherwise be left with nothing to show.
        /// </summary>
        /// <remarks>
        /// The text stays the button's accessible name (as its <c>aria-label</c>), because a hidden label is also
        /// hidden from a screen reader.
        /// </remarks>
        /// <param name="hide">Whether to hide the text on mobile.</param>
        /// <returns>The current instance.</returns>
        public Button HideTextOnMobile(bool hide = true)
        {
            InnerElement.UpdateClassIf(hide, HIDE_TEXT_ON_MOBILE_CLASS);

            if (hide) UpdateMobileLabel();
            else      InnerElement.removeAttribute("aria-label");

            return this;
        }

        private void UpdateMobileLabel()
        {
            if (string.IsNullOrEmpty(Text)) InnerElement.removeAttribute("aria-label");
            else                            InnerElement.setAttribute("aria-label", Text);
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
        /// Gets or sets where this button goes. Setting it on a button that was not built with an href has
        /// no effect - the element is decided when the button is built.
        /// </summary>
        public string Href
        {
            get => InnerElement.getAttribute("href");
            set => InnerElement.setAttribute("href", value ?? string.Empty);
        }

        /// <summary>
        /// Opens this button's href in a new tab. Only means anything on a button built with one.
        /// </summary>
        public Button OpenInNewTab()
        {
            InnerElement.setAttribute("target", "_blank");
            InnerElement.setAttribute("rel",    "noopener noreferrer");
            return this;
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
            get => InnerElement.classList.contains("tss-text-ellipsis");
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
        /// Shows the button's spinner (commonly used while an async action is in progress). The button spins in
        /// place: its icon and label fade while a dot appears on a ring where they were, orbits and stretches into
        /// an arc. Given a <paramref name="text"/>, the ring sits beside that text instead (ellipsized if it does
        /// not fit). The button keeps its width, height, tone and position throughout.
        /// </summary>
        public void ToSpinner(string text = null)
        {
            if (_spinOverlay is null)
            {
                _spinLabel   = Span(Att("tss-btn-spin-label"));
                _spinOverlay = Span(Att("tss-btn-spin"), CreateSpinRing(), _spinLabel);
                _spinOverlay.setAttribute("aria-hidden", "true");
                InnerElement.appendChild(_spinOverlay);
                InnerElement.classList.add("tss-btn-has-spin-ring");

                // Resolve the ring's resting style before it starts spinning, or the first spin has no state to
                // transition from and the arc appears already stretched.
                _spinOverlay.getBoundingClientRect();
            }

            _spinGeneration++;
            _spinLabel.innerText = text ?? "";

            _isSpinningInPlace = true;
            RemoveSpinClasses();
            InnerElement.classList.add("tss-btn-spinning");
            InnerElement.setAttribute("aria-busy", "true");
            PinSpinLabelWidth();

            if (InnerElement.HasOwnProperty("_tippy"))
            {
                Transpose.Script.Write("{0}._tippy.hide(); {0}._tippy.disable();", InnerElement);
            }
        }

        /// <summary>
        /// The orbiting-dot ring the button spins with: a track and an arc that turn, and a check and an x that
        /// do not. r = 20 on a 48 box, as drawn in the spec: the dash lengths in tss.button.css are fractions of
        /// its 125.66 circumference.
        /// </summary>
        internal static HTMLElement CreateSpinRing(string extraClass = null)
        {
            var ring = Span(Att(extraClass is null ? "tss-btn-spin-ring" : "tss-btn-spin-ring " + extraClass));
            ring.innerHTML = "<svg class=\"tss-btn-spin-orbit\" viewBox=\"0 0 48 48\" aria-hidden=\"true\"><circle class=\"tss-btn-spin-track\" cx=\"24\" cy=\"24\" r=\"20\"/><circle class=\"tss-btn-spin-arc\" cx=\"24\" cy=\"24\" r=\"20\" transform=\"rotate(-90 24 24)\"/></svg>"
                           + "<svg class=\"tss-btn-spin-mark\" viewBox=\"0 0 48 48\" aria-hidden=\"true\"><path class=\"tss-btn-spin-check\" pathLength=\"1\" d=\"M16 24.5l5.5 5.5L32 19\"/><path class=\"tss-btn-spin-x tss-btn-spin-x1\" pathLength=\"1\" d=\"M18.5 18.5l11 11\"/><path class=\"tss-btn-spin-x tss-btn-spin-x2\" pathLength=\"1\" d=\"M29.5 18.5l-11 11\"/></svg>";
            return ring;
        }

        // The spinner overlays the button rather than replacing its content, so the content holds the button's
        // width and height and nothing around it moves; a spinner text that does not fit is ellipsized. The text's
        // width is pinned so an outcome can collapse it (a max-width transition) and centre the mark.
        private void PinSpinLabelWidth()
        {
            _spinLabel.style.maxWidth = "";

            if (string.IsNullOrEmpty(_spinLabel.innerText) || !InnerElement.IsMounted()) return;

            _spinLabel.style.maxWidth = Math.Ceiling(_spinLabel.getBoundingClientRect().As<DOMRect>().width).px().ToString();
        }

        // How long a resolved check or x is held before it fades back to the button's content, and how long that
        // fade takes. They match the transitions in tss.button.css (the close-and-draw, then the fade).
        private const int SPIN_RESOLVE_HOLD_MS = 1000;
        private const int SPIN_RESOLVE_FADE_MS = 300;

        private const string SPIN_CLASSES = "tss-btn-spinning tss-btn-spin-resolved tss-btn-spin-success tss-btn-spin-error tss-btn-spin-fading";

        private void RemoveSpinClasses()
        {
            foreach (var c in SPIN_CLASSES.Split(' ')) InnerElement.classList.remove(c);
        }

        private void EndSpinInPlace(ButtonSpinnerOutcome outcome)
        {
            _isSpinningInPlace = false;

            if (outcome == ButtonSpinnerOutcome.None)
            {
                RemoveSpinClasses();
                ReleaseSpinInPlace();
                return;
            }

            // The arc closes into a full circle and the mark draws in; the button stays busy while it is shown.
            var generation = _spinGeneration;
            InnerElement.classList.add("tss-btn-spin-resolved", outcome == ButtonSpinnerOutcome.Success ? "tss-btn-spin-success" : "tss-btn-spin-error");

            window.setTimeout(_ =>
            {
                if (generation != _spinGeneration) return;

                RemoveSpinClasses();
                InnerElement.classList.add("tss-btn-spin-fading", outcome == ButtonSpinnerOutcome.Success ? "tss-btn-spin-success" : "tss-btn-spin-error");
                ReleaseSpinInPlace();

                window.setTimeout(__ =>
                {
                    if (generation == _spinGeneration) RemoveSpinClasses();
                }, SPIN_RESOLVE_FADE_MS);
            }, SPIN_RESOLVE_HOLD_MS);
        }

        private void ReleaseSpinInPlace()
        {
            InnerElement.removeAttribute("aria-busy");

            if (InnerElement.HasOwnProperty("_tippy"))
            {
                Transpose.Script.Write("{0}._tippy.enable();", InnerElement);
            }
        }

        /// <summary>
        /// Ends the spinner started by <see cref="ToSpinner"/> on the given outcome:
        /// <see cref="ButtonSpinnerOutcome.Success"/> and <see cref="ButtonSpinnerOutcome.Error"/> close the ring
        /// around a check or an x before the button's content returns, <see cref="ButtonSpinnerOutcome.None"/> is
        /// <see cref="UndoSpinner()"/>.
        /// </summary>
        public void UndoSpinner(ButtonSpinnerOutcome outcome)
        {
            if (_isSpinningInPlace) EndSpinInPlace(outcome);
        }

        /// <summary>
        /// Ends the spinner started by <see cref="ToSpinner"/> and brings the button's content back.
        /// </summary>
        public void UndoSpinner() => UndoSpinner(ButtonSpinnerOutcome.None);

        /// <summary>
        /// Spins while the action runs, then fades back to the button's content.
        /// </summary>
        public Button OnClickSpinWhile(Func<Task> action, string text = null, Action<Button, Exception> onError = null)
        {
            return OnClickSpinWhile((MouseEvent e) => action(), text, onError);
        }

        /// <summary>
        /// Spins while the action runs, then fades back to the button's content.
        /// </summary>
        public Button OnClickSpinWhile(Func<MouseEvent, Task> action, string text = null, Action<Button, Exception> onError = null)
        {
            return OnClick((_, e) =>
            {
                StopEvent(e);
                Task.Run(() => SpinWhileAsync(async () => { await action(e); return ButtonSpinnerOutcome.None; }, text, onError, ButtonSpinnerOutcome.None)).FireAndForget();
            });
        }

        /// <summary>
        /// Spins while the action runs, then ends on the outcome the action returns: a check for
        /// <see cref="ButtonSpinnerOutcome.Success"/>, an x for <see cref="ButtonSpinnerOutcome.Error"/>, straight
        /// back for <see cref="ButtonSpinnerOutcome.None"/>. An action that throws ends on the x.
        /// </summary>
        public Button OnClickSpinWhile(Func<Task<ButtonSpinnerOutcome>> action, string text = null, Action<Button, Exception> onError = null)
        {
            return OnClickSpinWhile((MouseEvent e) => action(), text, onError);
        }

        /// <summary>
        /// Spins while the action runs, then ends on the outcome the action returns: a check for
        /// <see cref="ButtonSpinnerOutcome.Success"/>, an x for <see cref="ButtonSpinnerOutcome.Error"/>, straight
        /// back for <see cref="ButtonSpinnerOutcome.None"/>. An action that throws ends on the x.
        /// </summary>
        public Button OnClickSpinWhile(Func<MouseEvent, Task<ButtonSpinnerOutcome>> action, string text = null, Action<Button, Exception> onError = null)
        {
            return OnClick((_, e) =>
            {
                StopEvent(e);
                Task.Run(() => SpinWhileAsync(() => action(e), text, onError, ButtonSpinnerOutcome.Error)).FireAndForget();
            });
        }

        private async Task SpinWhileAsync(Func<Task<ButtonSpinnerOutcome>> action, string text, Action<Button, Exception> onError, ButtonSpinnerOutcome outcomeOnError)
        {
            ToSpinner(text);

            ButtonSpinnerOutcome outcome;

            try
            {
                outcome = await action();
            }
            catch (Exception e)
            {
                UndoSpinner(outcomeOnError);

                if (onError is object) onError(this, e);
                else                   console.error(e);

                throw;
            }

            UndoSpinner(outcome);
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
        /// Runs the given async action while the button spins, then fades back to the button's content (also on
        /// error, optionally invoking the supplied error handler).
        /// </summary>
        public void SpinWhile(Func<Task> action, string text = null, Action<Button, Exception> onError = null)
        {
            Task.Run(() => SpinWhileAsync(async () => { await action(); return ButtonSpinnerOutcome.None; }, text, onError, ButtonSpinnerOutcome.None)).FireAndForget();
        }

        /// <summary>
        /// Runs the given async action while the button spins, then ends on the outcome it returns (an x if it
        /// throws, optionally invoking the supplied error handler).
        /// </summary>
        public void SpinWhile(Func<Task<ButtonSpinnerOutcome>> action, string text = null, Action<Button, Exception> onError = null)
        {
            Task.Run(() => SpinWhileAsync(action, text, onError, ButtonSpinnerOutcome.Error)).FireAndForget();
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
        /// Styles the component as an AI action - a filled purple-to-blue gradient. It is the AI
        /// equivalent of <see cref="Primary"/>, so like a primary button there should be one of them on a
        /// surface at a time; the second and third AI action beside it want <see cref="AISubtle"/>.
        /// <para>
        /// Unless the button already carries an icon it takes the Sparkles glyph, which is what the rest
        /// of the toolkit uses to mean "a model is involved". Pass false to leave the label on its own.
        /// </para>
        /// </summary>
        /// <param name="strongEffect">Asks for the louder form of the AI styling. A custom theme that tones the AI
        /// look down (e.g. Tesserae.Themes.Curiosity) brings more of it back for this element; the default theme
        /// draws both forms the same today. Sets the <c>tss-ai-strong</c> class.</param>
        public Button AI(bool withSparklesIcon = true, bool strongEffect = false)
        {
            InnerElement.classList.add("tss-ai");
            InnerElement.UpdateClassIf(strongEffect, "tss-ai-strong");
            InnerElement.classList.remove("tss-ai-subtle", "tss-btn-primary", "tss-btn-success", "tss-btn-danger");
            InnerElement.classList.add("tss-btn-default");

            if (withSparklesIcon && _iconSpan is null) SetIcon(UIcons.Sparkles);

            return this;
        }

        /// <summary>
        /// Styles the component as a quiet AI action: the same hue as <see cref="AI"/> as a tinted surface
        /// with the accent's own text, so a row of AI actions reads as a row of buttons rather than a row
        /// of gradients.
        /// </summary>
        /// <param name="strongEffect">Asks for the louder form of the AI styling. A custom theme that tones the AI
        /// look down (e.g. Tesserae.Themes.Curiosity) brings more of it back for this element; the default theme
        /// draws both forms the same today. Sets the <c>tss-ai-strong</c> class.</param>
        public Button AISubtle(bool withSparklesIcon = true, bool strongEffect = false)
        {
            AI(withSparklesIcon, strongEffect);
            InnerElement.classList.add("tss-ai-subtle");
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
        /// Renders the component as a hyperlink: no border and no background, so it reads as text rather
        /// than as a box, and - on a button built with an href, which is the only one that really goes
        /// somewhere - an underline on hover.
        /// <para>
        /// A tone still applies, as the label's colour rather than as a filled surface, so a destructive
        /// action that should read as text rather than as a box is <c>Link().Danger()</c>.
        /// </para>
        /// </summary>
        public Button Link()
        {
            IsLink = true;
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
            Icon = $"ec {icon.ToCssClass()}";

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