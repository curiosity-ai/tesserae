using System;
using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class NavLink : ComponentBase<NavLink, HTMLLIElement>, IContainer<NavLink>
    {
        #region Fields

        private HTMLSpanElement _TextSpan;
        private HTMLElement _IconSpan;
        private HTMLDivElement _HeaderDiv;
        private HTMLUListElement _ChildContainer;
        private HTMLButtonElement _ExpandButton;

        private int _Level;

        #endregion

        #region Events

        internal NavLink SelectedChild { get; private set; }

        public event EventHandler<NavLink> OnSelect;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets NavLink text
        /// </summary>
        public string Text
        {
            get { return _TextSpan.innerText; }
            set { _TextSpan.innerText = value; }
        }

        /// <summary>
        /// Gets or sets NavLink icon (icon class)
        /// </summary>
        public string Icon
        {
            get { return _IconSpan?.className; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (_IconSpan != null)
                    {
                        _HeaderDiv.removeChild(_IconSpan);
                        _IconSpan = null;
                    }

                    return;
                }

                if (_IconSpan == null)
                {
                    _IconSpan = I(_());
                    _HeaderDiv.insertBefore(_IconSpan, _TextSpan);
                }

                _IconSpan.className = value;
            }
        }

        public bool IsExpanded
        {
            get { return InnerElement.classList.contains("expanded"); }
            set
            {
                if (value != IsExpanded)
                {
                    if (value) InnerElement.classList.add("expanded");
                    else InnerElement.classList.remove("expanded");
                }
            }
        }

        public bool IsSelected
        {
            get { return _HeaderDiv.classList.contains("selected"); }
            set
            {
                if (value != IsSelected)
                {
                    if (value)
                    {
                        OnSelect?.Invoke(this, this);
                        _HeaderDiv.classList.add("selected");
                    }
                    else _HeaderDiv.classList.remove("selected");
                }
            }
        }

        public bool HasChildren
        {
            get { return _ChildContainer.hasChildNodes(); }
        }

        internal int Level
        {
            get { return _Level; }
            set
            {
                _Level = value;
                _HeaderDiv.style.paddingLeft = $"{_Level * 25}px";
            }
        }

        #endregion

        public NavLink(string text = null, string icon = null)
        {
            _TextSpan = Span(_(text: text));
            _ChildContainer = Ul(_("tss-nav-link-container"));
            _ExpandButton = Button(_("tss-nav-link-button"));
            _HeaderDiv = Div(_("tss-nav-link-header"), _ExpandButton, _TextSpan);
            _HeaderDiv.addEventListener("click", (s) =>
            {
                if (HasChildren) IsExpanded = !IsExpanded;
                else IsSelected = true;
            });
            InnerElement = Li(_("tss-nav-link"), _HeaderDiv, _ChildContainer);
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        public void Add(IComponent component)
        {
            _ChildContainer.appendChild(component.Render());
            _HeaderDiv.classList.add("expandable");
            var navLink = component as NavLink;
            navLink.Level = Level + 1;
            navLink.OnSelect += OnChildSelected;
            if (navLink.IsSelected)
            {
                OnSelect?.Invoke(this, navLink);

                if (SelectedChild != null) SelectedChild.IsSelected = false;
                SelectedChild = navLink;
            }

            if (navLink.SelectedChild != null)
            {
                OnSelect?.Invoke(navLink, navLink.SelectedChild);

                if (SelectedChild != null) SelectedChild.IsSelected = false;
                SelectedChild = navLink.SelectedChild;
            }
        }

        private void OnChildSelected(object sender, NavLink e)
        {
            OnSelect?.Invoke(this, e);
        }

        public void Clear()
        {
            ClearChildren(_ChildContainer);
            _HeaderDiv.classList.remove("expandable");

        }

        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            _ChildContainer.replaceChild(newComponent.Render(), oldComponent.Render());
            var navLink = oldComponent as NavLink;
            navLink.OnSelect += OnChildSelected;
            if (navLink.IsSelected) OnSelect?.Invoke(this, navLink);
        }
    }

    public class Nav : ComponentBase<Nav, HTMLUListElement>, IContainer<NavLink>
    {
        public NavLink SelectedLink { get; private set; }

        public Nav()
        {
            InnerElement = Ul(_("tss-nav"));
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        public void Add(IComponent component)
        {
            InnerElement.appendChild(component.Render());
            var navLink = component as NavLink;
            if (navLink != null) navLink.OnSelect += OnNavLinkSelected;
            if (navLink.IsSelected)
            {
                if (SelectedLink != null) SelectedLink.IsSelected = false;
                RaiseOnChange(navLink);
                SelectedLink = navLink;
            }

            if (navLink.SelectedChild != null)
            {
                if (SelectedLink != null) SelectedLink.IsSelected = false;
                RaiseOnChange(navLink.SelectedChild);
                SelectedLink = navLink.SelectedChild;
            }
        }

        public void Clear()
        {
            ClearChildren(InnerElement);
        }

        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            InnerElement.replaceChild(newComponent.Render(), oldComponent.Render());

            var navLink = newComponent as NavLink;
            if (navLink != null) navLink.OnSelect += OnNavLinkSelected;
            if (navLink.IsSelected)
            {
                if (SelectedLink != null) SelectedLink.IsSelected = false;
                RaiseOnChange(navLink);
                SelectedLink = navLink;
            }
        }

        private void OnNavLinkSelected(object sender, NavLink e)
        {
            if (SelectedLink != null) SelectedLink.IsSelected = false;
            RaiseOnChange(e);
            SelectedLink = e;
        }
    }

    public static class NavExtensions
    {
        public static Nav Links(this Nav container, params NavLink[] children)
        {
            children.ForEach(x => container.Add(x));
            return container;
        }
        public static NavLink Links(this NavLink container, params NavLink[] children)
        {
            children.ForEach(x => container.Add(x));
            return container;
        }

        public static NavLink Selected(this NavLink link)
        {
            link.IsSelected = true;
            return link;
        }
        public static NavLink Expanded(this NavLink link)
        {
            link.IsExpanded = true;
            return link;
        }

        public static NavLink Text(this NavLink link, string text)
        {
            link.Text = text;
            return link;
        }

        public static NavLink Icon(this NavLink link, string icon)
        {
            link.Icon = icon;
            return link;
        }

        public static NavLink OnSelected(this NavLink link, EventHandler<NavLink> onSelected)
        {
            link.OnSelect += onSelected;
            return link;
        }
    }
}
