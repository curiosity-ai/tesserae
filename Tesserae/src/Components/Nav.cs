using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tesserae.HTML;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public sealed class Nav : ComponentBase<Nav, HTMLUListElement>, IContainer<Nav.NavLink, Nav.NavLink>, IHasBackgroundColor
    {
        public Nav() => InnerElement = Ul(_("tss-nav"));

        public NavLink SelectedLink { get; private set; }

        private readonly List<NavLink> _children = new List<NavLink>();

        public string Background { get => InnerElement.style.background; set => InnerElement.style.background = value; }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        public void Add(NavLink component)
        {
            ScrollBar.GetCorrectContainer(InnerElement).appendChild(component.Render());
            component.InternalSelectedLink += OnNavLinkSelected;
            if (component.IsSelected)
            {
                if (SelectedLink != null)
                    SelectedLink.IsSelected = false;
                RaiseOnChange(ev: null);
                SelectedLink = component;
            }

            if (component.SelectedChild != null)
            {
                if (SelectedLink != null)
                    SelectedLink.IsSelected = false;
                RaiseOnChange(ev: null);
                SelectedLink = component.SelectedChild;
            }

            _children.Add(component);
        }

        public void Clear()
        {
            _children.Clear();
            ClearChildren(ScrollBar.GetCorrectContainer(InnerElement));
        }

        public void Replace(NavLink newComponent, NavLink oldComponent)
        {
            var index = _children.IndexOf(oldComponent);
            if (index >= 0)
            {
                _children[index] = newComponent;

                ScrollBar.GetCorrectContainer(InnerElement).replaceChild(newComponent.Render(), oldComponent.Render());

                newComponent.InternalSelectedLink += OnNavLinkSelected;
                if (newComponent.IsSelected)
                {
                    if (SelectedLink != null) SelectedLink.IsSelected = false;
                    RaiseOnChange(ev: null);
                    SelectedLink = newComponent;
                }
            }
        }
        public Nav Links(params Nav.NavLink[] children)
        {
            children.ForEach(x => Add(x));
            return this;
        }

        public Nav InlineContent(IComponent content, bool disableMouseEvents = false)
        {
            Add(new Nav.ComponentInNavLink(content, disableMouseEvents));
            return this;
        }

        private void OnNavLinkSelected(NavLink sender)
        {
            foreach(var c in _children)
            {
                c.UnselectRecursivelly(sender);
            }
            
            RaiseOnChange(ev: null);

            SelectedLink = sender;
        }

        public Nav Compact()
        {
            InnerElement.classList.add("tss-nav-small");
            return this;
        }

        public Nav NoLinkStyle()
        {
            InnerElement.classList.add("tss-nav-no-underline");
            return this;
        }

        public Nav SelectMarkerOnRight()
        {
            InnerElement.classList.add("tss-nav-right");
            return this;
        }

        public class ComponentInNavLink : NavLink
        {
            private readonly IComponent _content;
            private readonly bool _disableMouseEvents;
            private bool _alreadyRendered = false;

            public ComponentInNavLink(IComponent content, bool disableMouseEvents) : base()
            {
                _content = content;
                _disableMouseEvents = disableMouseEvents;
            }

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

            protected readonly HTMLSpanElement _textSpan;
            protected HTMLElement _iconSpan;
            protected readonly HTMLDivElement _headerDiv;
            protected readonly HTMLUListElement _childContainer;
            protected readonly HTMLButtonElement _expandButton;

            private bool _canSelectAndExpand = false;
            private int _Level;
            private bool _shouldExpandOnFirstAdd;
            private readonly List<NavLink> _childLinks = new List<NavLink>();

            public NavLink(string text = null)
            {
                _textSpan = Span(_(text: text));
                _childContainer = Ul(_("tss-nav-link-container"));
                _expandButton = Button(_("tss-nav-link-button"));
                _headerDiv = Div(_("tss-nav-link-header"), _expandButton, _textSpan);
                _headerDiv.onclick += ClickHandler;
                _expandButton.onclick += ExpandHandler;
                InnerElement = Li(_("tss-nav-link"), _headerDiv, _childContainer);
                Size = TextSize.Small;
                Weight = TextWeight.Regular;
            }

            public NavLink(IComponent content)
            {
                _childContainer = Ul(_("tss-nav-link-container"));
                _expandButton = Button(_("tss-nav-link-button"));
                _headerDiv = Div(_("tss-nav-link-header"), _expandButton, content.Render());
                _headerDiv.onclick += ClickHandler;
                _expandButton.onclick+= ExpandHandler;
                InnerElement = Li(_("tss-nav-link"), _headerDiv, _childContainer);
                Size = TextSize.Small;
                Weight = TextWeight.Regular;
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
                get { ThrowIfUsingComponent(nameof(Text));  return _textSpan?.innerText ; }
                set { ThrowIfUsingComponent(nameof(Text)); _textSpan.innerText = value; }
            }

            /// <summary>
            /// Gets or sets NavLink icon (icon class)
            /// </summary>
            public string Icon
            {
                get { ThrowIfUsingComponent(nameof(Icon)); return _iconSpan?.className; }
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
                DomObserver.WhenMounted(InnerElement, () => InnerElement.scrollIntoView(new ScrollIntoViewOptions() { block = ScrollLogicalPosition.nearest, inline = ScrollLogicalPosition.nearest , behavior = ScrollBehavior.smooth }));
            }

            private void UpdateSelectedClass(bool isSelected)
            {
                if (isSelected)
                    _headerDiv.classList.add("tss-selected");
                else
                    _headerDiv.classList.remove("tss-selected");
            }

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

            public TextSize Size
            {
                get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
                set
                {
                    InnerElement.classList.remove(Size.ToClassName());
                    InnerElement.classList.add(value.ToClassName());
                }
            }

            public TextWeight Weight
            {
                get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
                set
                {
                    InnerElement.classList.remove(Weight.ToClassName());
                    InnerElement.classList.add(value.ToClassName());
                }
            }

            public TextAlign TextAlign
            {
                get
                {
                    var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-textalign-"));
                    if (curFontSize is object && Enum.TryParse<TextAlign>(curFontSize.Substring("tss-textalign-".Length), true, out var result))
                    {
                        return result;
                    }
                    else
                    {
                        return TextAlign.Left;
                    }
                }
                set
                {
                    var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-textalign-"));
                    if (curFontSize is object)
                    {
                        InnerElement.classList.remove(curFontSize);
                    }
                    InnerElement.classList.add($"tss-textalign-{value.ToString().ToLower()}");
                }
            }

            public string Background { get => _headerDiv.style.background; set => _headerDiv.style.background = value; }

            public override HTMLElement Render() => InnerElement;

            public void Add(NavLink component)
            {
                _childLinks.Add(component);
                ScrollBar.GetCorrectContainer(_childContainer).appendChild(component.Render());
                _headerDiv.classList.add("tss-expandable");
                component.Level = Level + 1;
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

                if(HasChildren && _shouldExpandOnFirstAdd)
                {
                    IsExpanded = true;
                }
            }

            private void OnChildSelected(NavLink sender)
            {
                InternalSelectedLink?.Invoke(this);
            }

            public void Clear()
            {
                ClearChildren(ScrollBar.GetCorrectContainer(_childContainer));
                _childLinks.Clear();
                _headerDiv.classList.remove("tss-expandable");
            }

            public void Replace(NavLink newComponent, NavLink oldComponent)
            {
                ScrollBar.GetCorrectContainer(_childContainer).replaceChild(newComponent.Render(), oldComponent.Render());
                newComponent.InternalSelectedLink += OnChildSelected;
                if (newComponent.IsSelected)
                    InternalSelectedLink?.Invoke(newComponent);
            }

            public void Remove(NavLink oldComponent)
            {
                _childContainer.removeChild(oldComponent.Render());
            }
            public NavLink InlineContent(IComponent content, bool disableMouseEvents = false)
            {
                Add(new Nav.ComponentInNavLink(content, disableMouseEvents));
                return this;
            }

            public NavLink Selected()
            {
                IsSelected = true;
                return this;
            }
            
            public NavLink CanSelectAndExpand()
            {
                _canSelectAndExpand = true;
                return this;
            }

            public NavLink SelectedOrExpandedIf(bool shouldSelect)
            {
                if (shouldSelect)
                {
                    if (HasChildren)
                    {
                        IsExpanded = true;

                        if(_canSelectAndExpand)
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

            public NavLink Expanded()
            {
                IsExpanded = true;
                return this;
            }

            public NavLink SetText(string text)
            {
                Text = text;
                return this;
            }

            public NavLink SetIcon(string icon)
            {
                Icon = icon;
                return this;
            }

            public NavLink OnSelected(ComponentEventHandler<NavLink> onSelected)
            {
                SelectedLink += onSelected;
                return this;
            }

            public NavLink OnExpanded(ComponentEventHandler<NavLink> onExpanded)
            {
                ExpandedLink += onExpanded;
                return this;
            }

            public NavLink Links(params Nav.NavLink[] children)
            {
                children.ForEach(x => Add(x));
                return this;
            }

            public NavLink LinksAsync(Func<Task<Nav.NavLink[]>> childrenAsync)
            {
                bool alreadyRun = false;
                var dummy = new Nav.NavLink("loading...");
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
                    if(_canSelectAndExpand && !IsSelected)
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
                if(HasChildren)
                {
                    IsExpanded = !IsExpanded;
                    StopEvent(e);
                }
            }

            internal void UnselectRecursivelly(NavLink sender)
            {
                if (this == sender)
                {
                    foreach (var child in _childLinks)
                    {
                        child.UnselectRecursivelly(sender);
                    }
                }
                else if (!_childLinks.Any(l => l.IsOrHasChild(sender)))
                {
                    IsSelected = false;

                    foreach (var child in _childLinks)
                    {
                        child.UnselectRecursivelly(sender);
                    }
                }
            }

            private bool IsOrHasChild(NavLink sender) => this == sender || _childLinks.Any(l => l.IsOrHasChild(sender));
        }
    }
}