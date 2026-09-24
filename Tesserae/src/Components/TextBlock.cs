using System;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A text block component.
    /// </summary>
    [Transpose.Name("tss.txt")]
    public class TextBlock : ComponentBase<TextBlock, HTMLElement>, ITextFormating, IHasBackgroundColor, IHasForegroundColor, ICanWrap, IRoundedStyle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextBlock"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="treatAsHTML">Whether to treat the text as HTML.</param>
        /// <param name="selectable">Whether the text is selectable.</param>
        /// <param name="textSize">The text size.</param>
        /// <param name="textWeight">The text weight.</param>
        /// <param name="afterText">Optional text to append.</param>
        public TextBlock(string text = string.Empty, bool treatAsHTML = false, bool selectable = false, TextSize textSize = TextSize.Small, TextWeight textWeight = TextWeight.Regular, string afterText = null)
        {
            text = text ?? string.Empty;

            if (!string.IsNullOrEmpty(afterText))
            {
                var first  = Div(Att("tss-text-ellipsis"));
                var second = Div(Att("tss-text-nowrap"));
                InnerElement = Div(Att("tss-textblock tss-fontcolor-default tss-textblock-with-after " + textSize.ToString() + " " + textWeight.ToString()), first, second);

                if (treatAsHTML)
                {
                    first.innerHTML  = text;
                    second.innerHTML = afterText;
                }
                else
                {
                    first.textContent  = text;
                    second.textContent = afterText;
                }
            }
            else
            {
                InnerElement = Div(Att("tss-textblock tss-fontcolor-default " + textSize.ToString() + " " + textWeight.ToString()));

                if (treatAsHTML)
                {
                    InnerElement.innerHTML = text;
                }
                else
                {
                    InnerElement.textContent = text;
                }
            }


            if (selectable)
            {
                InnerElement.classList.add("tss-textblock-selectable");
            }

            AttachClick();
            AttachContextMenu();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBlock"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public TextBlock(string text)
        {
            text                     = text ?? string.Empty;
            InnerElement             = Div(Att("tss-textblock tss-fontcolor-default " + TextSize.Small.ToString() + " " + TextWeight.Regular.ToString()));
            InnerElement.textContent = text;
            AttachClick();
            AttachContextMenu();
        }

        /// <summary>Gets or sets the background color.</summary>
        public string Background { get => InnerElement.style.background; set => InnerElement.style.background = value; }

        /// <summary>Gets or sets the foreground color.</summary>
        public string Foreground { get => GetTarget().style.color; set => GetTarget().style.color = value; }

        /// <summary>Gets or sets whether the component is enabled.</summary>
        public bool IsEnabled
        {
            get => !InnerElement.classList.contains("tss-disabled");
            set
            {
                if (value)
                {
                    InnerElement.classList.remove("tss-disabled");
                }
                else
                {
                    InnerElement.classList.add("tss-disabled");
                }
            }
        }

        /// <summary>Gets or sets whether the text is selectable.</summary>
        public bool IsSelectable
        {
            get => GetTarget().style.userSelect != "none";
            set => GetTarget().style.userSelect = value ? "" : "none";
        }

        /// <summary>Gets or sets the text.</summary>
        public string Text
        {
            get => GetTarget().innerText;
            set => GetTarget().innerText = value;
        }

        /// <summary>Gets or sets the HTML content.</summary>
        public string HTML
        {
            get => GetTarget().innerHTML;
            set => GetTarget().innerHTML = value;
        }

        /// <summary>Gets or sets the title.</summary>
        public string Title
        {
            get => GetTarget().title;
            set => GetTarget().title = value;
        }

        private Action _unbindText;

        /// <summary>
        /// Keeps the text in step with <paramref name="source"/>: every change is written into this same
        /// element, so the block is never rebuilt. One-way, since a text block has no input to push back; the
        /// two-way <see cref="BindingExtensions"/>.Bind is for input components over a SettableObservable. This is the way to show text that changes; wrapping a
        /// <see cref="TextBlock"/> in <c>DeferSync</c> constructs a new block and swaps the element on every
        /// change, which remounts it and flickers.
        /// The subscription is held only while the block is mounted: it is taken on mount (writing the current
        /// value), dropped on removal and taken again if the block is mounted again. Binding again replaces the
        /// previous binding.
        /// </summary>
        public TextBlock Bind(IObservable<string> source) => Bind(source, v => v);

        /// <summary>
        /// Keeps the text in step with <paramref name="source"/>, formatted by <paramref name="format"/>. See
        /// <see cref="Bind(IObservable{string})"/>.
        /// </summary>
        public TextBlock Bind<T>(IObservable<T> source, Func<T, string> format)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (format is null) throw new ArgumentNullException(nameof(format));

            _unbindText?.Invoke();

            var bound     = true;
            var observing = false;

            ObservableEvent.ValueChanged<T> write = value =>
            {
                var text = format(value) ?? string.Empty;
                var el   = GetTarget();

                // Skip identical writes: they would still be a childList mutation for every DomObserver on the page.
                if (el.textContent != text) el.textContent = text;
            };

            void Mounted()
            {
                if (!bound || observing) return;
                observing = true;
                source.Observe(write); // fires now with the current value, catching up on anything missed while unmounted
                DomObserver.WhenRemoved(InnerElement, Removed);
            }

            void Removed()
            {
                if (!observing) return;
                observing = false;
                source.StopObserving(write);
                if (bound) DomObserver.WhenMounted(InnerElement, Mounted);
            }

            // Show the current value straight away, so the first paint is already right.
            write(source.Value);

            if (InnerElement.IsMounted()) Mounted();
            else DomObserver.WhenMounted(InnerElement, Mounted);

            _unbindText = () =>
            {
                bound = false;
                if (observing)
                {
                    observing = false;
                    source.StopObserving(write);
                }
            };

            return this;
        }

        private HTMLElement GetTarget()
        {
            if (InnerElement.classList.contains("tss-label")) return InnerElement.firstElementChild.As<HTMLElement>();
            return InnerElement;
        }

        /// <summary>Gets or sets the text size.</summary>
        public TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(GetTarget(), TextSize.Small);
            set
            {
                var el = GetTarget();
                el.classList.remove(Size.ToString());
                el.classList.add(value.ToString());
            }
        }

        /// <summary>Gets or sets the text weight.</summary>
        public TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(GetTarget(), TextWeight.Regular);
            set
            {
                var el = GetTarget();
                el.classList.remove(Weight.ToString());
                el.classList.add(value.ToString());
            }
        }

        /// <summary>Gets or sets the text alignment.</summary>
        public TextAlign TextAlign
        {
            get
            {
                return ITextFormatingExtensions.FromClassList(GetTarget(), TextAlign.Left);
            }
            set
            {
                var el = GetTarget();
                el.classList.remove(TextAlign.ToString());
                el.classList.add(value.ToString());
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is primary
        /// </summary>
        /// <summary>Gets or sets whether the text is primary color.</summary>
        public bool IsPrimary
        {
            get => GetTarget().classList.contains("tss-fontcolor-primary");
            set
            {
                var el = GetTarget();

                if (value)
                {
                    el.classList.add("tss-fontcolor-primary");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-success", "tss-fontcolor-secondary", "tss-fontcolor-danger", "tss-fontcolor-default");
                }
                else
                {
                    el.classList.add("tss-fontcolor-default");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-success", "tss-fontcolor-secondary", "tss-fontcolor-danger", "tss-fontcolor-primary");
                }
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is primary
        /// </summary>
        /// <summary>Gets or sets whether the text is secondary color.</summary>
        public bool IsSecondary
        {
            get => GetTarget().classList.contains("tss-fontcolor-secondary");
            set
            {
                var el = GetTarget();

                if (value)
                {
                    el.classList.add("tss-fontcolor-secondary");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-success", "tss-fontcolor-primary", "tss-fontcolor-danger", "tss-fontcolor-default");
                }
                else
                {
                    el.classList.add("tss-fontcolor-default");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-success", "tss-fontcolor-secondary", "tss-fontcolor-danger", "tss-fontcolor-primary");
                }
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is success
        /// </summary>
        /// <summary>Gets or sets whether the text is success color.</summary>
        public bool IsSuccess
        {
            get => GetTarget().classList.contains("tss-fontcolor-success");
            set
            {
                var el = GetTarget();

                if (value)
                {
                    el.classList.add("tss-fontcolor-success");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-secondary", "tss-fontcolor-primary", "tss-fontcolor-danger", "tss-fontcolor-default");
                }
                else
                {
                    el.classList.add("tss-fontcolor-default");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-success", "tss-fontcolor-secondary", "tss-fontcolor-danger", "tss-fontcolor-primary");
                }
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is danger
        /// </summary>
        /// <summary>Gets or sets whether the text is danger color.</summary>
        public bool IsDanger
        {
            get => GetTarget().classList.contains("tss-fontcolor-danger");
            set
            {
                var el = GetTarget();

                if (value)
                {
                    el.classList.add("tss-fontcolor-danger");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-secondary", "tss-fontcolor-primary", "tss-fontcolor-success", "tss-fontcolor-default");
                }
                else
                {
                    el.classList.add("tss-fontcolor-default");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-success", "tss-fontcolor-secondary", "tss-fontcolor-danger", "tss-fontcolor-primary");
                }
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is invalid
        /// </summary>
        /// <summary>Gets or sets whether the text is invalid color.</summary>
        public bool IsInvalid
        {
            get => GetTarget().classList.contains("tss-fontcolor-invalid");
            set
            {
                var el = GetTarget();

                if (value)
                {
                    el.classList.add("tss-fontcolor-invalid");
                }
                else
                {
                    el.classList.remove("tss-fontcolor-invalid");
                }
            }
        }

        /// <summary>Gets or sets whether the text block represents a required field.</summary>
        public virtual bool IsRequired
        {
            get => GetTarget().classList.contains("tss-required");
            set
            {
                var el = GetTarget();

                if (value)
                {
                    el.classList.add("tss-required");
                }
                else
                {
                    el.classList.remove("tss-required");
                }
            }
        }

        /// <summary>Gets or sets whether the text can wrap.</summary>
        public bool CanWrap
        {
            get => !GetTarget().classList.contains("tss-text-nowrap");
            set => GetTarget().UpdateClassIfNot(value, "tss-text-nowrap");
        }

        /// <summary>Gets or sets whether to enable ellipsis for overflowing text.</summary>
        public bool EnableEllipsis
        {
            get => !GetTarget().classList.contains("tss-text-ellipsis");
            set => GetTarget().UpdateClassIf(value, "tss-text-ellipsis");
        }

        /// <summary>Gets or sets whether to enable break-spaces.</summary>
        public bool EnableBreakSpaces
        {
            get => !GetTarget().classList.contains("tss-text-breakspaces");
            set => GetTarget().UpdateClassIf(value, "tss-text-breakspaces");
        }

        /// <summary>Gets or sets the cursor.</summary>
        public string Cursor
        {
            get => GetTarget().style.cursor;
            set => GetTarget().style.cursor = value;
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <returns>The rendered HTML element.</returns>
        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}