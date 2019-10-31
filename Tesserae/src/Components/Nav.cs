using System;
using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class NavLink : ComponentBase<NavLink, HTMLLIElement>, IContainer<NavLink>
    {
        private HTMLSpanElement textSpan;
        private HTMLElement iconSpan;
        private HTMLDivElement headerDiv;
        private HTMLUListElement childContainer;
        private HTMLButtonElement expandButton;

        public event EventHandler<NavLink> OnSelect;

        /// <summary>
        /// Gets or sets NavLink text
        /// </summary>
        public string Text
        {
            get { return textSpan.innerText; }
            set { textSpan.innerText = value; }
        }

        /// <summary>
        /// Gets or sets NavLink icon (icon class)
        /// </summary>
        public string Icon
        {
            get { return iconSpan?.className; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (iconSpan != null)
                    {
                        headerDiv.removeChild(iconSpan);
                        iconSpan = null;
                        textSpan.classList.remove("ml-2");
                    }

                    return;
                }

                if (iconSpan == null)
                {
                    iconSpan = I(_());
                    headerDiv.insertBefore(iconSpan, textSpan);
                    textSpan.classList.add("ml-2");
                }

                iconSpan.className = value;
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
            get { return headerDiv.classList.contains("selected"); }
            set
            {
                if (value != IsSelected)
                {
                    if (value)
                    {
                        OnSelect?.Invoke(this, this);
                        headerDiv.classList.add("selected");
                    }
                    else headerDiv.classList.remove("selected");
                }
            }
        }

        public bool HasChildren
        {
            get { return childContainer.hasChildNodes(); }
        }

        private int level;
        internal int Level
        {
            get { return level; }
            set
            {
                level = value;
                headerDiv.style.paddingLeft = $"{level * 25}px";
            }
        }

        public NavLink(string text = null, string icon = null)
        {
            textSpan = Span(_(text: text));
            childContainer = Ul(_("mss-nav-link-container"));
            expandButton = Button(_("mss-nav-link-button"));
            headerDiv = Div(_("mss-nav-link-header"), expandButton, textSpan);
            headerDiv.addEventListener("click", (s) =>
            {
                if (HasChildren) IsExpanded = !IsExpanded;
                else IsSelected = true;
            });
            InnerElement = Li(_("mss-nav-link"), headerDiv, childContainer);
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        public void Add(IComponent component)
        {
            childContainer.appendChild(component.Render());
            headerDiv.classList.add("expandable");
            var navLink = component as NavLink;
            navLink.Level = Level + 1;
            navLink.OnSelect += OnChildSelected;
            if (navLink.IsSelected) OnSelect?.Invoke(this, navLink);
        }

        private void OnChildSelected(object sender, NavLink e)
        {
            OnSelect?.Invoke(this, e);
        }

        public void Clear()
        {
            ClearChildren(childContainer);
            headerDiv.classList.remove("expandable");

        }

        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            childContainer.replaceChild(newComponent.Render(), oldComponent.Render());
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
            InnerElement = Ul(_("mr-4 mss-nav"));
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
    }
}
