using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A hierarchical, vertically-stacked navigation tree with support for nested sections, icons and badges.
    /// </summary>
    [H5.Name("tss.Nav")]
    public sealed class Nav : ComponentBase<Nav, HTMLUListElement>, IContainer<Nav.NavLink, Nav.NavLink>, IHasBackgroundColor, IBindableComponent<Nav.NavLink>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Nav() => InnerElement = Ul(_("tss-nav"));

        /// <summary>
        /// Gets or sets the selected link.
        /// </summary>
        public NavLink SelectedLink { get; private set; }

        private readonly List<NavLink>               _children   = new List<NavLink>();
        private readonly SettableObservable<NavLink> _observable = new SettableObservable<NavLink>();

        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public string Background { get => InnerElement.style.background; set => InnerElement.style.background = value; }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return InnerElement;
        }

        /// <summary>
        /// Adds the given item to the component.
        /// </summary>
        public void Add(NavLink component)
        {
            InnerElement.appendChild(component.Render());
            component.InternalSelectedLink += OnNavLinkSelected;

            if (component.IsSelected)
            {
                if (SelectedLink != null)
                    SelectedLink.IsSelected = false;
                RaiseOnChange(ev: null);
                SelectedLink      = component;
                _observable.Value = component;
            }

            if (component.SelectedChild != null)
            {
                if (SelectedLink != null)
                    SelectedLink.IsSelected = false;
                RaiseOnChange(ev: null);
                SelectedLink      = component.SelectedChild;
                _observable.Value = component.SelectedChild;
            }

            _children.Add(component);
        }

        /// <summary>
        /// Clears the component's current state.
        /// </summary>
        public void Clear()
        {
            _children.Clear();
            ClearChildren(InnerElement);
        }

        /// <summary>
        /// Replaces an existing item with a new one.
        /// </summary>
        public void Replace(NavLink newComponent, NavLink oldComponent)
        {
            var index = _children.IndexOf(oldComponent);

            if (index >= 0)
            {
                _children[index] = newComponent;

                InnerElement.replaceChild(newComponent.Render(), oldComponent.Render());

                newComponent.InternalSelectedLink += OnNavLinkSelected;

                if (newComponent.IsSelected)
                {
                    if (SelectedLink != null) SelectedLink.IsSelected = false;
                    RaiseOnChange(ev: null);
                    SelectedLink = newComponent;
                }
            }
        }
        /// <summary>
        /// Configures the component to links.
        /// </summary>
        public Nav Links(params Nav.NavLink[] children)
        {
            children.ForEach(x => Add(x));
            return this;
        }

        /// <summary>
        /// Sets a piece of content shown inline next to the navigation item.
        /// </summary>
        public Nav InlineContent(IComponent content, bool disableMouseEvents = false)
        {
            Add(new Nav.ComponentInNavLink(content, disableMouseEvents));
            return this;
        }

        private void OnNavLinkSelected(NavLink sender)
        {
            foreach (var c in _children)
            {
                c.UnselectRecursively(sender);
            }

            RaiseOnChange(ev: null);

            SelectedLink      = sender;
            _observable.Value = sender;
        }

        /// <summary>
        /// Returns an observable that tracks the currently-selected nav link.
        /// </summary>
        public IObservable<NavLink> AsObservable() => _observable;

        /// <summary>
        /// Programmatically selects a nav link as part of a two-way binding.
        /// </summary>
        public void SetBoundValue(NavLink value)
        {
            if (value == null) return;
            if (SelectedLink == value) return;
            value.IsSelected = true;
        }

        /// <summary>
        /// Renders the component in a compact form.
        /// </summary>
        public Nav Compact()
        {
            InnerElement.classList.add("tss-nav-small");
            return this;
        }

        /// <summary>
        /// Removes / disables the link style on the component.
        /// </summary>
        public Nav NoLinkStyle()
        {
            InnerElement.classList.add("tss-nav-no-underline");
            return this;
        }

        /// <summary>
        /// Renders the selection marker on the right side of the nav.
        /// </summary>
        public Nav SelectMarkerOnRight()
        {
            InnerElement.classList.add("tss-nav-right");
            return this;
        }

        public class ComponentInNavLink : NavLink
        {
            private readonly IComponent _content;
            private readonly bool       _disableMouseEvents;
            private          bool       _alreadyRendered = false;

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public ComponentInNavLink(IComponent content, bool disableMouseEvents) : base()
            {
                _content            = content;
                _disableMouseEvents = disableMouseEvents;
            }

            /// <summary>
            /// Renders the component's root HTML element.
            /// </summary>
            public override HTMLElement Render()
            {
                if (!_alreadyRendered)
                {
                    _alreadyRendered = true;
                    ClearChildren(_headerDiv);
                    _headerDiv.onclick -= ClickHandler;

                    if (_disableMouseEvents)
                    {
                        _headerDiv.style.pointerEvents = "none";
                    }

                    _headerDiv.appendChild(_content.Render());
                }

                return InnerElement;
            }
        }

        public class NavLink : ComponentBase<NavLink, HTMLLIElement>, IContainer<NavLink, NavLink>, ITextFormating, IHasBackgroundColor
        {
            private event ComponentEventHandler<NavLink> SelectedLink;
            private event ComponentEventHandler<NavLink> ExpandedLink;

            // TODO [2020-07-09 DWR]: It would be really helpful to know what circumstances this (rather than the SelectedLink event is required, I think) is needed in
            internal event ComponentEventHandler<NavLink> InternalSelectedLink;

            protected readonly HTMLSpanElement   _textSpan;
            protected          HTMLElement       _iconSpan;
            protected readonly HTMLDivElement    _headerDiv;
            protected readonly HTMLUListElement  _childContainer;
            protected readonly HTMLButtonElement _expandButton;

            private          bool          _canSelectAndExpand = false;
            private          int           _Level;
            private          bool          _shouldExpandOnFirstAdd;
            private readonly List<NavLink> _childLinks = new List<NavLink>();

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public NavLink(string text = null)
            {
                _textSpan             =  Span(_(text: text));
                _childContainer       =  Ul(_("tss-nav-link-container"));
                _expandButton         =  Button(_("tss-nav-link-button"));
                _headerDiv            =  Div(_("tss-nav-link-header"), _expandButton, _textSpan);
                _headerDiv.onclick    += ClickHandler;
                _expandButton.onclick += ExpandHandler;
                InnerElement          =  Li(_("tss-nav-link"), _headerDiv, _childContainer);
                Size                  =  TextSize.Small;
                Weight                =  TextWeight.Regular;
            }

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public NavLink(IComponent content)
            {
                _childContainer       =  Ul(_("tss-nav-link-container"));
                _expandButton         =  Button(_("tss-nav-link-button"));
                _headerDiv            =  Div(_("tss-nav-link-header"), _expandButton, content.Render());
                _headerDiv.onclick    += ClickHandler;
                _expandButton.onclick += ExpandHandler;
                InnerElement          =  Li(_("tss-nav-link"), _headerDiv, _childContainer);
                Size                  =  TextSize.Small;
                Weight                =  TextWeight.Regular;
            }

            internal NavLink SelectedChild { get; private set; }

            private void ThrowIfUsingComponent(string method)
            {
                if (_textSpan is null)
                    throw new Exception($"Not allowed to call {method} when using a custom component for rendering the Navlink");
            }

            /// <summary>
            /// Gets or sets NavLink text
            /// </summary>
            public string Text
            {
                get
                {
                    ThrowIfUsingComponent(nameof(Text));
                    return _textSpan?.innerText;
                }
                set
                {
                    ThrowIfUsingComponent(nameof(Text));
                    _textSpan.innerText = value;
                }
            }

            /// <summary>
            /// Gets or sets NavLink icon (icon class)
            /// </summary>
            public string Icon
            {
                get
                {
                    ThrowIfUsingComponent(nameof(Icon));
                    return _iconSpan?.className;
                }
                set
                {
                    ThrowIfUsingComponent(nameof(Icon));

                    if (string.IsNullOrEmpty(value))
                    {
                        if (_iconSpan != null)
                        {
                            _headerDiv.removeChild(_iconSpan);
                            _iconSpan = null;
                        }

                        return;
                    }

                    if (_iconSpan == null)
                    {
                        _iconSpan = I(_());
                        _headerDiv.insertBefore(_iconSpan, _textSpan);
                    }

                    _iconSpan.className = value;
                }
            }

            /// <summary>
            /// Returns a value indicating whether the component is expanded.
            /// </summary>
            public bool IsExpanded
            {
                get => InnerElement.classList.contains("tss-expanded");
                set
                {
                    if (value)
                    {
                        if (!IsExpanded)
                        {
                            ExpandedLink?.Invoke(this);
                            ScrollIntoView();
                        }
                        InnerElement.classList.add("tss-expanded");
                    }
                    else InnerElement.classList.remove("tss-expanded");
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the component is selected.
            /// </summary>
            public bool IsSelected
            {
                get => _headerDiv.classList.contains("tss-selected");
                set
                {
                    if (value && !IsSelected)
                    {
                        InternalSelectedLink?.Invoke(this);
                        SelectedLink?.Invoke(this);
                        ScrollIntoView();
                    }
                    UpdateSelectedClass(value);
                }
            }

            private void ScrollIntoView()
            {
                DomObserver.WhenMounted(InnerElement, () => InnerElement.scrollIntoView(new ScrollIntoViewOptions() { block = ScrollLogicalPosition.nearest, inline = ScrollLogicalPosition.nearest, behavior = ScrollBehavior.smooth }));
            }

            private void UpdateSelectedClass(bool isSelected)
            {
                if (isSelected)
                    _headerDiv.classList.add("tss-selected");
                else
                    _headerDiv.classList.remove("tss-selected");
            }

            /// <summary>
            /// Returns a value indicating whether the component has the given children.
            /// </summary>
            public bool HasChildren => _childContainer.hasChildNodes();

            internal int Level
            {
                get => _Level;
                set
                {
                    _Level = value;

                    foreach (var c in _childLinks)
                    {
                        c.Level = Level + 1;
                    }
                }
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
                get
                {
                    return ITextFormatingExtensions.FromClassList(InnerElement, TextAlign.Left);
                }
                set
                {
                    InnerElement.classList.remove(TextAlign.ToString());
                    InnerElement.classList.add(value.ToString());
                }
            }

            /// <summary>
            /// Gets or sets the CSS background of the component.
            /// </summary>
            public string Background
            {
                get => _headerDiv.style.background;
                set
                {
                    _headerDiv.style.background = value;
                    _headerDiv.UpdateClassIf(!string.IsNullOrWhiteSpace(value), "tss-filter-effects");
                }
            }

            /// <summary>
            /// Renders the component's root HTML element.
            /// </summary>
            public override HTMLElement Render() => InnerElement;

            /// <summary>
            /// Adds the given item to the component.
            /// </summary>
            public void Add(NavLink component)
            {
                _childLinks.Add(component);
                _childContainer.appendChild(component.Render());
                _headerDiv.classList.add("tss-expandable");
                component.Level                =  Level + 1;
                component.InternalSelectedLink += OnChildSelected;

                if (component.IsSelected)
                {
                    InternalSelectedLink?.Invoke(component);

                    if (SelectedChild != null) SelectedChild.IsSelected = false;
                    SelectedChild = component;
                }

                if (component.SelectedChild != null)
                {
                    InternalSelectedLink?.Invoke(component.SelectedChild);

                    if (SelectedChild != null) SelectedChild.IsSelected = false;
                    SelectedChild = component.SelectedChild;
                }

                if (HasChildren && _shouldExpandOnFirstAdd)
                {
                    IsExpanded = true;
                }
            }

            private void OnChildSelected(NavLink sender)
            {
                InternalSelectedLink?.Invoke(this);
            }

            /// <summary>
            /// Clears the component's current state.
            /// </summary>
            public void Clear()
            {
                ClearChildren(_childContainer);
                _childLinks.Clear();
                _headerDiv.classList.remove("tss-expandable");
            }

            /// <summary>
            /// Replaces an existing item with a new one.
            /// </summary>
            public void Replace(NavLink newComponent, NavLink oldComponent)
            {
                _childContainer.replaceChild(newComponent.Render(), oldComponent.Render());
                newComponent.InternalSelectedLink += OnChildSelected;

                if (newComponent.IsSelected)
                    InternalSelectedLink?.Invoke(newComponent);
            }

            /// <summary>
            /// Removes the given item from the component.
            /// </summary>
            public void Remove(NavLink oldComponent)
            {
                _childContainer.removeChild(oldComponent.Render());
            }
            /// <summary>
            /// Sets a piece of content shown inline next to the navigation item.
            /// </summary>
            public NavLink InlineContent(IComponent content, bool disableMouseEvents = false)
            {
                Add(new Nav.ComponentInNavLink(content, disableMouseEvents));
                return this;
            }

            /// <summary>
            /// Marks the component as selected.
            /// </summary>
            public NavLink Selected()
            {
                IsSelected = true;
                return this;
            }

            /// <summary>
            /// Returns a value indicating whether the component can select and expand.
            /// </summary>
            public NavLink CanSelectAndExpand()
            {
                _canSelectAndExpand = true;
                return this;
            }

            /// <summary>
            /// Marks this link as selected (and expanded) when the supplied predicate holds.
            /// </summary>
            public NavLink SelectedOrExpandedIf(bool shouldSelect)
            {
                if (shouldSelect)
                {
                    if (HasChildren)
                    {
                        IsExpanded = true;

                        if (_canSelectAndExpand)
                        {
                            IsSelected = true;
                        }
                    }
                    else
                    {
                        IsSelected = true;
                    }

                    _shouldExpandOnFirstAdd = true;
                }
                return this;
            }

            /// <summary>
            /// Expands the component.
            /// </summary>
            public NavLink Expanded()
            {
                IsExpanded = true;
                return this;
            }

            /// <summary>
            /// Sets the text of the component.
            /// </summary>
            public NavLink SetText(string text)
            {
                Text = text;
                return this;
            }

            /// <summary>
            /// Registers a callback invoked when the selected event fires.
            /// </summary>
            public NavLink OnSelected(ComponentEventHandler<NavLink> onSelected)
            {
                SelectedLink += onSelected;
                return this;
            }

            /// <summary>
            /// Registers a callback invoked when the expanded event fires.
            /// </summary>
            public NavLink OnExpanded(ComponentEventHandler<NavLink> onExpanded)
            {
                ExpandedLink += onExpanded;
                return this;
            }

            /// <summary>
            /// Configures the component to links.
            /// </summary>
            public NavLink Links(params Nav.NavLink[] children)
            {
                children.ForEach(x => Add(x));
                return this;
            }

            /// <summary>
            /// Asynchronously loads the child links of this nav node from the supplied factory.
            /// </summary>
            public NavLink LinksAsync(Func<Task<Nav.NavLink[]>> childrenAsync)
            {
                bool alreadyRun = false;
                var  dummy      = new Nav.NavLink("loading...");
                Add(dummy);

                ExpandedLink += s =>
                {
                    if (!alreadyRun)
                    {
                        alreadyRun = true;

                        Task.Run(async () =>
                        {
                            var children = await childrenAsync();
                            Remove(dummy);
                            children.ForEach(x => Add(x));
                        }).FireAndForget();
                    }
                };

                if (IsExpanded)
                {
                    ExpandedLink.Invoke(this);
                }
                return this;
            }

            protected void ClickHandler(MouseEvent e)
            {
                StopEvent(e);

                if (HasChildren)
                {
                    if (_canSelectAndExpand && !IsSelected)
                    {
                        IsSelected = true;
                    }
                    else
                    {
                        IsExpanded = !IsExpanded;
                    }
                }
                else
                {
                    IsSelected = true;
                }
            }

            protected void ExpandHandler(MouseEvent e)
            {
                if (HasChildren)
                {
                    IsExpanded = !IsExpanded;
                    StopEvent(e);
                }
            }

            internal void UnselectRecursively(NavLink sender)
            {
                if (this == sender)
                {
                    foreach (var child in _childLinks)
                    {
                        child.UnselectRecursively(sender);
                    }
                }
                else if (!_childLinks.Any(l => l.IsOrHasChild(sender)))
                {
                    IsSelected = false;

                    foreach (var child in _childLinks)
                    {
                        child.UnselectRecursively(sender);
                    }
                }
            }

            [Obsolete("Use UnselectRecursively (corrected spelling)")]
            internal void UnselectRecursivelly(NavLink sender) => UnselectRecursively(sender);

            private bool IsOrHasChild(NavLink sender) => this == sender || _childLinks.Any(l => l.IsOrHasChild(sender));
        }
    }
}