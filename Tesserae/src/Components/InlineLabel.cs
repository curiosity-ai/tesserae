using System;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// One small piece of metadata on a line of them: an optional mark - a glyph, an image, or a rounded
    /// square of colour - followed by optional text. It is what an <see cref="OmniResult{T}"/> footer is
    /// made of ("Box · 2.4 MB · Marie Lang · Apr 11, 2024"), and it reads the same wherever else a row of
    /// small facts is drawn.
    /// <para>
    /// Every combination is allowed: text alone, a mark alone, or a mark and text together. Whatever the
    /// mark is, it is drawn at one size, so a line of them stays on one baseline however they were built.
    /// </para>
    /// <para>
    /// A label can be pressed (<see cref="OnClick(Action{InlineLabel})"/>) or be a real link
    /// (<see cref="SetHref(string, bool)"/>) - it is an anchor either way, so a link is middle-clickable
    /// and shows its address in the status bar rather than being a div pretending.
    /// </para>
    /// <para>
    /// A label can also be built from a task (<see cref="InlineLabel(Func{InlineLabel, Task})"/>) for a
    /// fact that has to be looked up. It draws as a skeleton rectangle while the task runs, and if the
    /// task ends without giving it anything to say it takes itself out of the document - along with the
    /// slot it was standing in, so a line of facts doesn't keep a gap for something that turned out not
    /// to exist.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.InlineLabel")]
    public class InlineLabel : ComponentBase<InlineLabel, HTMLAnchorElement>
    {
        private readonly HTMLElement _mark;
        private readonly HTMLElement _text;

        private Action<InlineLabel> _clickHandler;

        /// <summary>
        /// Initializes a new instance of this class showing the given text, with no mark before it.
        /// </summary>
        public InlineLabel(string text = null)
        {
            _mark = Span(Att("tss-inlinelabel-mark"));
            _text = Span(Att("tss-inlinelabel-text"));

            InnerElement = A(Att("tss-inlinelabel"), _mark, _text);

            SetText(text);
            NoMark();

            AttachClick();

            InnerElement.addEventListener("click", e =>
            {
                if (_clickHandler is null) return;

                //A label that is also a link leaves Ctrl/Cmd-click and Shift-click to the browser, so it
                //opens where it points in a new tab or window instead of running the handler.
                if (IsModifiedLinkClick(InnerElement, e.As<MouseEvent>())) return;

                StopEvent(e);

                _clickHandler(this);
            });

            InnerElement.addEventListener("keydown", e =>
            {
                if (_clickHandler is null) return;

                var keyboardEvent = e.As<KeyboardEvent>();

                if (keyboardEvent.key != "Enter" && keyboardEvent.key != " ") return;

                StopEvent(keyboardEvent);

                _clickHandler(this);
            });
        }

        /// <summary>
        /// Initializes a new instance of this class that looks up what it says. It draws as a skeleton
        /// rectangle while <paramref name="load"/> runs, and takes itself - and the slot it stands in -
        /// out of the document if the task ends without setting any text or mark on it.
        /// </summary>
        public InlineLabel(Func<InlineLabel, Task> load) : this(deferred: load is object)
        {
            if (load is null) return;

            RunDeferred(load).FireAndForget();
        }

        /// <summary>
        /// Initializes a new instance of a derived label that looks up what it says, drawing as a skeleton
        /// rectangle from the moment it is built but starting nothing: a subclass has fields of its own to
        /// assign before its lookup can run, so it calls <see cref="RunDeferred"/> once it is ready.
        /// <para>
        /// Pass false to build a plain label - the same thing <see cref="InlineLabel(string)"/> gives you.
        /// </para>
        /// </summary>
        protected InlineLabel(bool deferred) : this((string)null)
        {
            if (deferred) ShowSkeleton();
        }

        /// <summary>
        /// Runs a lookup for what the label says: it shows the skeleton while the task runs and, if the task
        /// ends without giving it anything to say, takes the label - and the slot it stands in - out of the
        /// document. Safe to call more than once, which is how a label refreshes itself: every run starts
        /// from nothing, so one that used to say something and now finds nothing goes away rather than
        /// keeping what it said last time.
        /// </summary>
        protected Task RunDeferred(Func<InlineLabel, Task> load)
        {
            if (load is null) return Task.CompletedTask;

            //The skeleton goes on before the reset, so clearing the text doesn't briefly mark the label as
            //having nothing to say while it is in fact still looking.
            ShowSkeleton();

            SetText(null);
            NoMark();

            return LoadAsync(load);
        }

        private void ShowSkeleton()
        {
            InnerElement.classList.add("tss-inlinelabel-loading");

            if (InnerElement.querySelector(".tss-inlinelabel-skeleton") is null)
            {
                InnerElement.appendChild(Span(Att("tss-inlinelabel-skeleton tss-skeleton tss-skeleton-animated")));
            }
        }

        /// <summary>Gets the text the label shows, or null when it is a mark on its own.</summary>
        public string Text { get; private set; }

        /// <summary>Whether the label has nothing to show - no text, and no mark.</summary>
        public bool IsEmpty => string.IsNullOrEmpty(Text) && _mark.style.display == "none";

        /// <summary>
        /// Says on the element itself whether there is anything in it, so a container can leave out what
        /// stands for an empty label - the dot an <see cref="OmniResult{T}"/> footer puts before every
        /// entry - without having to look inside it. A label that is still loading is not empty: it is
        /// showing a skeleton in place of what it is about to say.
        /// </summary>
        private void UpdateEmptyClass()
        {
            InnerElement.UpdateClassIf(IsEmpty && !InnerElement.classList.contains("tss-inlinelabel-loading"), "tss-inlinelabel-empty");
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;

        /// <summary>
        /// Sets the text the label shows. A null or empty value leaves the label as its mark alone.
        /// </summary>
        public InlineLabel SetText(string text)
        {
            Text = text;

            var isEmpty = string.IsNullOrEmpty(text);

            _text.textContent   = isEmpty ? string.Empty : text;
            _text.style.display = isEmpty ? "none" : "";

            UpdateEmptyClass();

            return this;
        }

        /// <summary>
        /// Puts a glyph before the text, in a colour of its own when one is given - the accent a node type,
        /// a source or a status is known by - and in the label's own colour otherwise.
        /// </summary>
        public InlineLabel SetIcon(UIcons icon, UIconsWeight weight = UIconsWeight.Regular, string color = null)
        {
            var glyph = I(icon, weight, "tss-inlinelabel-glyph");

            if (!string.IsNullOrEmpty(color)) glyph.style.color = color;

            return SetMark(glyph, "tss-inlinelabel-mark-icon");
        }

        /// <summary>
        /// Puts a component of the host's own before the text - an <see cref="Avatar"/>, an emoji, a
        /// <see cref="Spinner"/>. It is drawn at the same size as any other mark.
        /// </summary>
        public InlineLabel SetIcon(IComponent iconOrImage)
        {
            return iconOrImage is null ? NoMark() : SetMark(iconOrImage.Render(), "tss-inlinelabel-mark-image");
        }

        /// <summary>
        /// Puts an image before the text - a source's logo, a favicon, a thumbnail - fitted into the mark
        /// rather than cropped.
        /// </summary>
        public InlineLabel SetImage(string url)
        {
            return string.IsNullOrEmpty(url) ? NoMark() : SetMark(Image(Att("tss-inlinelabel-image", src: url)), "tss-inlinelabel-mark-image");
        }

        /// <summary>
        /// Puts a small rounded square of the given colour before the text - the quiet way a source, a
        /// status or a category is named.
        /// </summary>
        public InlineLabel SetColor(string color)
        {
            if (string.IsNullOrEmpty(color)) return NoMark();

            SetMark(null, "tss-inlinelabel-mark-color");

            _mark.style.background = color;

            return this;
        }

        /// <summary>
        /// Takes the mark away, leaving the text alone.
        /// </summary>
        public InlineLabel NoMark()
        {
            ClearChildren(_mark);

            _mark.className        = "tss-inlinelabel-mark";
            _mark.style.background = string.Empty;
            _mark.style.display    = "none";

            UpdateEmptyClass();

            return this;
        }

        /// <summary>
        /// Makes the label a real link to the given address, opened in a new tab when asked. Pass null to
        /// make it plain text again.
        /// </summary>
        public InlineLabel SetHref(string href, bool openInNewTab = false)
        {
            if (string.IsNullOrEmpty(href))
            {
                InnerElement.removeAttribute("href");
                InnerElement.removeAttribute("target");
                InnerElement.removeAttribute("rel");
            }
            else
            {
                InnerElement.setAttribute("href", href);

                if (openInNewTab)
                {
                    InnerElement.setAttribute("target", "_blank");
                    InnerElement.setAttribute("rel",    "noopener noreferrer");
                }
            }

            return UpdateInteractive();
        }

        /// <summary>
        /// Registers what pressing the label does. It takes a tab stop of its own and answers Enter and
        /// Space; the click stops at the label, so pressing it never also counts as a click on the row it
        /// sits in. Pass null to make it plain text again.
        /// </summary>
        public InlineLabel OnClick(Action<InlineLabel> onClick)
        {
            _clickHandler = onClick;

            return UpdateInteractive();
        }

        /// <summary>
        /// Registers a click handler in the shape every other component takes. The label becomes pressable
        /// the same way <see cref="OnClick(Action{InlineLabel})"/> makes it.
        /// </summary>
        public override InlineLabel OnClick(ComponentEventHandler<InlineLabel, MouseEvent> onClick, bool clearPrevious = true)
        {
            base.OnClick(onClick, clearPrevious);

            return UpdateInteractive(force: onClick is object);
        }

        private InlineLabel SetMark(HTMLElement content, string markClass)
        {
            ClearChildren(_mark);

            _mark.className        = "tss-inlinelabel-mark " + markClass;
            _mark.style.background = string.Empty;
            _mark.style.display    = "";

            if (content is object) _mark.appendChild(content);

            UpdateEmptyClass();

            return this;
        }

        private async Task LoadAsync(Func<InlineLabel, Task> load)
        {
            try
            {
                await load(this);
            }
            finally
            {
                InnerElement.classList.remove("tss-inlinelabel-loading");

                foreach (var skeleton in InnerElement.querySelectorAll(".tss-inlinelabel-skeleton"))
                {
                    skeleton.As<HTMLElement>().remove();
                }

                UpdateEmptyClass();

                //Nothing to say: the label takes its slot with it rather than leaving a gap in the line.
                if (IsEmpty) this.WhenMounted(RemoveWithItsSlot);
            }
        }

        /// <summary>
        /// Takes the label out of the document along with whatever is standing in for it in its container:
        /// a stack's item wrapper, a footer's entry, or - when the label is all a details row has to show -
        /// the whole row, label cell included. Anywhere else, just the label goes.
        /// </summary>
        private void RemoveWithItsSlot()
        {
            HTMLElement node   = InnerElement;
            var         parent = node.parentElement;

            while (parent is object)
            {
                //A details row whose value is this label alone has nothing left to say either
                if (parent.classList.contains("tss-detailsgrid-value"))
                {
                    if (parent.childElementCount <= 1 && parent.parentElement is object && parent.parentElement.classList.contains("tss-detailsgrid-row"))
                    {
                        parent.parentElement.remove();
                        return;
                    }

                    break;
                }

                if (parent.classList.contains("tss-omniresult-footer-entry"))
                {
                    parent.remove();
                    return;
                }

                //A stack wraps every child in an item div - that wrapper is the slot, and it may itself
                //be sitting in a details value, so keep climbing.
                if (parent.classList.contains("tss-stack-item"))
                {
                    node   = parent;
                    parent = parent.parentElement;
                    continue;
                }

                break;
            }

            node.remove();
        }

        private InlineLabel UpdateInteractive(bool force = false)
        {
            var isInteractive = force || _clickHandler is object || InnerElement.hasAttribute("href");

            InnerElement.UpdateClassIf(isInteractive, "tss-inlinelabel-interactive");

            if (isInteractive)
            {
                InnerElement.setAttribute("tabindex", "0");
            }
            else
            {
                InnerElement.removeAttribute("tabindex");
            }

            return this;
        }
    }
}
