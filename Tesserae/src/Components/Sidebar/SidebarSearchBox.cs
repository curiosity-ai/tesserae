using System;
using Transpose.Core;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class SidebarSearchBox : ISidebarItem
    {
        private readonly IComponent  _closed;
        private readonly IComponent  _open;
        private readonly SearchBox   _searchBox;
        private readonly HTMLElement _closedElement;
        private readonly HTMLElement _openElement;

        public bool IsSelected { get; set; }

        public IComponent CurrentRendered => _open.IsMounted() ? _open : _closed;

        public string Identifier { get; private set; }

        public string OwnIdentifier => Sidebar.GetOwnIdentifier(Identifier);

        private event Action<string> Searched;

        private Action _onClick;

        public SidebarSearchBox(string identifier, string placeholder = "Search...")
        {
            Identifier = identifier;

            _closedElement = Div(Att("tss-sidebar-btn tss-sidebar-btn-closed-icon"), I(UIcons.Search));
            _closedElement.title = placeholder;

            _searchBox = SearchBox(placeholder).Underlined().SearchAsYouType().NoIcon();
            _searchBox.OnSearch((s, v) => Searched?.Invoke(v));

            _openElement = Div(Att("tss-sidebar-btn-open tss-sidebar-searchbox"));
            _openElement.appendChild(_searchBox.Render());

            _closed = Raw(_closedElement);
            _open   = Raw(_openElement);

            _openElement.addEventListener("click", RaiseClick);
            _closedElement.addEventListener("click", RaiseClick);
        }

        /// <summary>
        /// Renders the search box with rounded corners (defaults to a fully rounded "pill" shape,
        /// like the rounded SidebarButton). Removes the underlined style so the box shows a full
        /// rounded border.
        /// </summary>
        /// <param name="radius">The border radius to apply. Defaults to <see cref="BorderRadius.Full"/>.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarSearchBox Rounded(BorderRadius radius = BorderRadius.Full)
        {
            _searchBox.IsUnderlined = false;
            _searchBox.Rounded(radius);
            _openElement.classList.add("tss-sidebar-searchbox-rounded");
            _closedElement.classList.add("tss-sidebar-searchbox-rounded");
            return this;
        }

        public SidebarSearchBox OnSearch(Action<string> onSearch)
        {
            Searched += onSearch;
            return this;
        }

        /// <summary>
        /// The text in the box. Setting it does not raise <see cref="OnSearch"/> - a caller that clears the
        /// box usually has its own idea of what to do about the results.
        /// </summary>
        public string Text
        {
            get => _searchBox.Text;
            set => _searchBox.SetText(value);
        }

        /// <summary>Sets the text in the box, without raising <see cref="OnSearch"/>.</summary>
        public SidebarSearchBox SetText(string text)
        {
            _searchBox.SetText(text);
            return this;
        }

        /// <summary>Puts the caret in the box.</summary>
        public SidebarSearchBox Focus()
        {
            _searchBox.Focus();
            return this;
        }

        /// <summary>
        /// Makes the box a way in rather than a place to type: clicking it - open, or as the icon on the
        /// closed rail - runs the handler, and so does focusing it, which is what
        /// <see cref="SetKeyboardShortcut"/> does. Nothing can be typed into it, because what is typed
        /// belongs to whatever the handler opened.
        /// <para>
        /// This is the shape a search that answers somewhere else takes: the box says where to start and
        /// which key gets there, and a palette or a page does the searching.
        /// </para>
        /// </summary>
        public SidebarSearchBox OnClick(Action onClick)
        {
            _onClick = onClick;

            var input = _openElement.querySelector("input").As<HTMLInputElement>();

            if (input is object)
            {
                input.readOnly = onClick is object;
                input.style.cursor = onClick is object ? "pointer" : "";
            }

            _openElement.classList.toggle("tss-sidebar-searchbox-opens-elsewhere", onClick is object);

            //The shortcut presses the box rather than focusing it, so it works the same whether or not the
            //box happens to be holding the caret already.
            _searchBox.OnShortcut(onClick is null ? null : (Action)(() => RaiseClick(null)));

            return this;
        }

        private void RaiseClick(Event e)
        {
            if (_onClick is null) return;

            if (e is object) StopEvent(e);

            //A box that only leads somewhere should never be left holding the caret - the thing it opened
            //is what the next keystroke is for.
            var input = _openElement.querySelector("input").As<HTMLInputElement>();

            input?.blur();

            _onClick();
        }

        public SidebarSearchBox SetKeyboardShortcut(params string[] keys)
        {
            _searchBox.SetKeyboardShortcut(keys);
            return this;
        }

        /// <summary>
        /// Keeps the shortcut chip out of sight until the pointer is on the box - or the caret is in it, so a
        /// box reached by tabbing shows its key too. The binding is untouched: the key works the same whether
        /// or not the chip is on screen.
        /// <para>
        /// The room the chip takes at the end is reserved either way, so what is typed does not re-flow as the
        /// chip appears.
        /// </para>
        /// </summary>
        /// <param name="onlyOnHover">Whether the chip waits for a hover. False shows it at all times, the default.</param>
        public SidebarSearchBox ShortcutOnlyOnHover(bool onlyOnHover = true)
        {
            _openElement.classList.toggle("tss-sidebar-shortcut-on-hover", onlyOnHover);
            return this;
        }

        public void AddGroupIdentifier(string groupIdentifier)
        {
             Identifier = groupIdentifier + Sidebar.GroupIdentifierSeparator + Identifier;
        }

        public void Collapse()
        {
            _closed.Collapse();
            _open.Collapse();
        }

        public void Show()
        {
             _closed.Show();
             _open.Show();
        }

        public IComponent RenderClosed()
        {
            return _closed;
        }

        public IComponent RenderOpen()
        {
            return _open;
        }
    }
}
