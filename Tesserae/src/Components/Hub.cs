using System;

namespace Tesserae
{
    /// <summary>
    /// A top-level page: a <see cref="HubHeader"/> (where you are, as a breadcrumb trail, and what you can do here) above
    /// the page's sections. It fills the area it is put in and is a <see cref="SectionStack"/>, so its content is added
    /// with <c>.Section(...)</c> / <c>.FlatSection(...)</c>; a section added with <c>grow: true</c> takes the height that
    /// is left.
    /// </summary>
    [Transpose.Name("tss.Hub")]
    public class Hub : SectionStack
    {
        /// <summary>
        /// Creates a hub named <paramref name="title"/>, whose name links to <paramref name="href"/> when one is given.
        /// </summary>
        public Hub(string title, string href = null) : this(new HubHeader(title, href)) { }

        /// <summary>
        /// Creates a hub whose name follows <paramref name="title"/>, linking to <paramref name="href"/> when one is
        /// given.
        /// </summary>
        public Hub(IObservable<string> title, string href = null) : this(new HubHeader(title, href)) { }

        /// <summary>
        /// Creates a hub with a header you built.
        /// </summary>
        public Hub(HubHeader header)
        {
            Header = header;
            InnerElement.classList.add("tss-hub");
            AddAnimatedTitle(Header);
        }

        /// <summary>
        /// The header row: the breadcrumb trail and the commands.
        /// </summary>
        public HubHeader Header { get; }

        /// <summary>
        /// Adds a text crumb after the ones already in the trail. See <see cref="HubHeader.Crumb(string, string)"/>.
        /// </summary>
        public Hub Crumb(string text, string href = null)
        {
            Header.Crumb(text, href);
            return this;
        }

        /// <summary>
        /// Adds a text crumb that runs <paramref name="onClick"/>. See <see cref="HubHeader.Crumb(string, Action)"/>.
        /// </summary>
        public Hub Crumb(string text, Action onClick)
        {
            Header.Crumb(text, onClick);
            return this;
        }

        /// <summary>
        /// Adds a text crumb that follows <paramref name="text"/>. See <see cref="HubHeader.Crumb(IObservable{string}, string)"/>.
        /// </summary>
        public Hub Crumb(IObservable<string> text, string href = null)
        {
            Header.Crumb(text, href);
            return this;
        }

        /// <summary>
        /// Adds a component as a crumb. See <see cref="HubHeader.Crumb(IComponent)"/>.
        /// </summary>
        public Hub Crumb(IComponent component)
        {
            Header.Crumb(component);
            return this;
        }

        /// <summary>
        /// Adds several component crumbs, in order.
        /// </summary>
        public Hub Crumbs(params IComponent[] components)
        {
            Header.Crumbs(components);
            return this;
        }

        /// <summary>
        /// Adds commands to the right of the header. See <see cref="HubHeader.Commands"/>.
        /// </summary>
        public Hub Commands(params IComponent[] commands)
        {
            Header.Commands(commands);
            return this;
        }

        /// <summary>
        /// Removes every section; the header stays.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            AddAnimatedTitle(Header);
        }
    }
}
