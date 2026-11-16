using System;
using static Tesserae.UI;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// The header row of a <see cref="Hub"/>: a breadcrumb trail that starts at the hub's own name, followed by the
    /// page's commands on the right.
    /// <para>
    /// The trail takes whatever room the commands leave and clips what does not fit; the commands keep their own
    /// width and are never squeezed. Every crumb after the first carries its own separator, so collapsing a crumb
    /// (<c>.Collapse()</c> on the component handed to <see cref="Crumb(IComponent)"/>) takes its separator with it.
    /// </para>
    /// <para>
    /// A <see cref="Hub"/> builds one of these as its first title. Use it on its own to put the same header on a
    /// <see cref="SectionStack"/> you manage yourself, or above several groups on one page.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.HubHeader")]
    public sealed class HubHeader : IComponent, IHasMarginPadding
    {
        private readonly HTMLElement _root;
        private readonly HTMLElement _trail;
        private readonly HTMLElement _commands;
        private          HTMLElement _current;

        /// <summary>
        /// Creates a header whose first crumb is <paramref name="title"/>, linking to <paramref name="href"/> when one
        /// is given.
        /// </summary>
        public HubHeader(string title, string href = null) : this()
        {
            AddCrumb(TextCrumb(TextBlock(title), href, null), isRoot: true);
        }

        /// <summary>
        /// Creates a header whose first crumb is <paramref name="title"/>, running <paramref name="onClick"/> when it
        /// is clicked.
        /// </summary>
        public HubHeader(string title, Action onClick) : this()
        {
            AddCrumb(TextCrumb(TextBlock(title), null, onClick), isRoot: true);
        }

        /// <summary>
        /// Creates a header whose first crumb follows <paramref name="title"/>, linking to <paramref name="href"/>
        /// when one is given.
        /// </summary>
        public HubHeader(IObservable<string> title, string href = null) : this()
        {
            AddCrumb(TextCrumb(TextBlock(title), href, null), isRoot: true);
        }

        /// <summary>
        /// Creates a header whose first crumb is a component of your own.
        /// </summary>
        public HubHeader(IComponent title) : this()
        {
            AddCrumb(title.Render(), isRoot: true);
        }

        private HubHeader()
        {
            _trail    = Div(Att("tss-hub-trail"));
            _commands = Div(Att("tss-hub-commands"));
            _root     = Div(Att("tss-hub-header"), _trail, _commands);

            _trail.setAttribute("role", "navigation");
            _trail.setAttribute("aria-label", "Breadcrumb");
        }

        /// <summary>
        /// Adds a text crumb, linking to <paramref name="href"/> when one is given. A crumb without a link is the
        /// page you are on.
        /// </summary>
        public HubHeader Crumb(string text, string href = null)
        {
            AddCrumb(TextCrumb(TextBlock(text), href, null), isRoot: false);
            return this;
        }

        /// <summary>
        /// Adds a text crumb that runs <paramref name="onClick"/> when clicked.
        /// </summary>
        public HubHeader Crumb(string text, Action onClick)
        {
            AddCrumb(TextCrumb(TextBlock(text), null, onClick), isRoot: false);
            return this;
        }

        /// <summary>
        /// Adds a text crumb that follows <paramref name="text"/>, linking to <paramref name="href"/> when one is
        /// given.
        /// </summary>
        public HubHeader Crumb(IObservable<string> text, string href = null)
        {
            AddCrumb(TextCrumb(TextBlock(text), href, null), isRoot: false);
            return this;
        }

        /// <summary>
        /// Adds a component as a crumb - a picker, a title with an avatar, a name that changes. Collapsing the component
        /// hides the crumb and its separator together.
        /// </summary>
        public HubHeader Crumb(IComponent component)
        {
            AddCrumb(component.Render(), isRoot: false);
            return this;
        }

        /// <summary>
        /// Adds several crumbs, in order.
        /// </summary>
        public HubHeader Crumbs(params IComponent[] components)
        {
            if (components is null) return this;

            foreach (var component in components)
            {
                if (component is object) Crumb(component);
            }
            return this;
        }

        /// <summary>
        /// Adds commands to the right of the trail. They keep their own width; use the gap the header puts between them
        /// rather than a margin of your own.
        /// </summary>
        public HubHeader Commands(params IComponent[] commands)
        {
            if (commands is null) return this;

            foreach (var command in commands)
            {
                if (command is null) continue;

                var el = command.Render();
                el.classList.add("tss-hub-command");
                _commands.appendChild(el);
            }
            return this;
        }

        /// <summary>
        /// Removes every command.
        /// </summary>
        public HubHeader ClearCommands()
        {
            ClearChildren(_commands);
            return this;
        }

        /// <summary>Gets or sets the CSS margin of the header.</summary>
        public string Margin
        {
            get => _root.style.margin;
            set => _root.style.margin = value;
        }

        /// <summary>Gets or sets the CSS padding of the header.</summary>
        public string Padding
        {
            get => _root.style.padding;
            set => _root.style.padding = value;
        }

        /// <summary>Renders the header.</summary>
        public HTMLElement Render() => _root;

        private void AddCrumb(HTMLElement content, bool isRoot)
        {
            // The separator is the wrapper's ::before, so a collapsed crumb (tss-collapse on the content) hides it too:
            // see tss.hub.css
            var crumb = Div(Att(isRoot ? "tss-hub-crumb tss-hub-crumb-root" : "tss-hub-crumb"), content);

            // The last crumb is the page you are on, whatever it is drawn with
            _current?.removeAttribute("aria-current");
            _current = content;
            _current.setAttribute("aria-current", "page");

            _trail.appendChild(crumb);
        }

        private static HTMLElement TextCrumb(TextBlock text, string href, Action onClick)
        {
            text.NoWrap().Ellipsis().Class("tss-hub-crumb-label");

            HTMLElement el;

            if (!string.IsNullOrEmpty(href))
            {
                el = A(Att("tss-hub-crumb-text tss-hub-crumb-link", href: href), text.Render());

                el.addEventListener("click", e =>
                {
                    var me = e.As<MouseEvent>();

                    // A modified click is the browser's: a new tab, a new window, a download
                    if (me.button != 0 || me.ctrlKey || me.metaKey || me.shiftKey || me.altKey) return;

                    e.preventDefault();
                    Router.Navigate(href);
                });
            }
            else if (onClick is object)
            {
                el = Button(Att("tss-hub-crumb-text tss-hub-crumb-link"), text.Render());
                el.setAttribute("type", "button");
                el.addEventListener("click", _ => onClick());
            }
            else
            {
                el = Span(Att("tss-hub-crumb-text"), text.Render());
            }

            return el;
        }
    }
}
