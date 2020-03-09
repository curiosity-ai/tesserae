using System;
using static Tesserae.UI;
using static Retyped.dom;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Tesserae.Components
{
    public class Nav : ComponentBase<Nav, HTMLUListElement>, IContainer<Nav.NavLink, Nav.NavLink>
    {
        public Nav()
        {
            InnerElement = Ul(_("tss-nav"));
        }

        public NavLink SelectedLink { get; private set; }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        public void Add(NavLink component)
        {
            ScrollBar.GetCorrectContainer(InnerElement).appendChild(component.Render());
            component.OnSelect += OnNavLinkSelected;
            if (component.IsSelected)
            {
                if (SelectedLink != null) SelectedLink.IsSelected = false;
                RaiseOnChange(component);
                SelectedLink = component;
            }

            if (component.SelectedChild != null)
            {
                if (SelectedLink != null) SelectedLink.IsSelected = false;
                RaiseOnChange(component.SelectedChild);
                SelectedLink = component.SelectedChild;
            }
        }

        public void Clear()
        {
            ClearChildren(ScrollBar.GetCorrectContainer(InnerElement));
        }

        public void Replace(NavLink newComponent, NavLink oldComponent)
        {
            ScrollBar.GetCorrectContainer(InnerElement).replaceChild(newComponent.Render(), oldComponent.Render());

            newComponent.OnSelect += OnNavLinkSelected;
            if (newComponent.IsSelected)
            {
                if (SelectedLink != null) SelectedLink.IsSelected = false;
                RaiseOnChange(newComponent);
                SelectedLink = newComponent;
            }
        }
        public Nav Links(params Nav.NavLink[] children)
        {
            children.ForEach(x => Add(x));
            return this;
        }

        public Nav InlineContent(IComponent content)
        {
            Add(new Nav.ComponentInNavLink(content));
            return this;
        }

        private void OnNavLinkSelected(object sender, NavLink e)
        {
            if (SelectedLink != null) SelectedLink.IsSelected = false;
            RaiseOnChange(e);
            SelectedLink = e;
        }

        public class ComponentInNavLink : NavLink
        {
            private IComponent Content;

            private bool AlreadyRendered = false;

            public ComponentInNavLink(IComponent content) : base()
            {
                Content = content;
            }

            public override HTMLElement Render()
            {
                if (!AlreadyRendered)
                {
                    AlreadyRendered = true;
                    ClearChildren(_headerDiv);
                    _headerDiv.removeEventListener("click", ClickHandler);
                    _headerDiv.appendChild(Content.Render());
                }

                return InnerElement;
            }
        }

        public class NavLink : ComponentBase<NavLink, HTMLLIElement>, IContainer<NavLink, NavLink>, IHasTextSize
        {
            protected readonly HTMLSpanElement _textSpan;
            protected HTMLElement _iconSpan;
            protected readonly HTMLDivElement _headerDiv;
            protected readonly HTMLUListElement _childContainer;
            protected readonly HTMLButtonElement _expandButton;

            private int _Level;
            private readonly List<NavLink> Children = new List<NavLink>();
            public NavLink(string text = null, string icon = null)
            {
                _textSpan = Span(_(text: text));
                _childContainer = Ul(_("tss-nav-link-container"));
                _expandButton = Button(_("tss-nav-link-button"));
                _headerDiv = Div(_("tss-nav-link-header"), _expandButton, _textSpan);
                _headerDiv.addEventListener("click", ClickHandler);
                InnerElement = Li(_("tss-nav-link"), _headerDiv, _childContainer);
                Size = TextSize.Small;
                Weight = TextWeight.Regular;
            }

            public NavLink(IComponent content)
            {
                _childContainer = Ul(_("tss-nav-link-container"));
                _expandButton = Button(_("tss-nav-link-button"));
                _headerDiv = Div(_("tss-nav-link-header"), _expandButton, content.Render());
                _headerDiv.addEventListener("click", ClickHandler);
                InnerElement = Li(_("tss-nav-link"), _headerDiv, _childContainer);
                Size = TextSize.Small;
                Weight = TextWeight.Regular;
            }


            internal event EventHandler<NavLink> OnExpanded;

            internal NavLink SelectedChild { get; private set; }

            public event EventHandler<NavLink> OnSelect;

            private void ThrowIfUsingComponent(string method)
            {
                if (_textSpan is null) throw new Exception($"Not allowed to call {method} when using a custom component for rendering the Navlink");
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
                get => InnerElement.classList.contains("expanded");
                set
                {
                    if (value)
                    {
                        if (!IsExpanded)
                        {
                            OnExpanded?.Invoke(this, this);
                        }
                        InnerElement.classList.add("expanded");
                    }
                    else InnerElement.classList.remove("expanded");
                }
            }

            public bool IsSelected
            {
                get => _headerDiv.classList.contains("selected");
                set
                {
                    if (value)
                    {
                        if(!IsSelected)
                        {
                            OnSelect?.Invoke(this, this);
                        }
                        _headerDiv.classList.add("selected");
                    }
                    else _headerDiv.classList.remove("selected");
                }
            }

            public bool HasChildren => _childContainer.hasChildNodes();

            internal int Level
            {
                get => _Level;
                set
                {
                    _Level = value;
                    foreach (var c in Children)
                    {
                        c.Level = Level + 1;
                    }
                }
            }

            public TextSize Size
            {
                get => TextSizeExtensions.FromClassList(InnerElement, TextSize.Small);
                set
                {
                    InnerElement.classList.remove(Size.ToClassName());
                    InnerElement.classList.add(value.ToClassName());
                }
            }

            public TextWeight Weight
            {
                get => TextSizeExtensions.FromClassList(InnerElement, TextWeight.Regular);
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

            public override HTMLElement Render()
            {
                return InnerElement;
            }

            public void Add(NavLink component)
            {
                Children.Add(component);
                ScrollBar.GetCorrectContainer(_childContainer).appendChild(component.Render());
                _headerDiv.classList.add("expandable");
                component.Level = Level + 1;
                component.OnSelect += OnChildSelected;
                if (component.IsSelected)
                {
                    OnSelect?.Invoke(this, component);

                    if (SelectedChild != null) SelectedChild.IsSelected = false;
                    SelectedChild = component;
                }

                if (component.SelectedChild != null)
                {
                    OnSelect?.Invoke(component, component.SelectedChild);

                    if (SelectedChild != null) SelectedChild.IsSelected = false;
                    SelectedChild = component.SelectedChild;
                }
            }

            private void OnChildSelected(object sender, NavLink e)
            {
                OnSelect?.Invoke(this, e);
            }

            public void Clear()
            {
                ClearChildren(ScrollBar.GetCorrectContainer(_childContainer));
                Children.Clear();
                _headerDiv.classList.remove("expandable");

            }

            public void Replace(NavLink newComponent, NavLink oldComponent)
            {
                ScrollBar.GetCorrectContainer(_childContainer).replaceChild(newComponent.Render(), oldComponent.Render());
                newComponent.OnSelect += OnChildSelected;
                if (newComponent.IsSelected) OnSelect?.Invoke(this, newComponent);
            }

            public void Remove(NavLink oldComponent)
            {
                _childContainer.removeChild(oldComponent.Render());
            }
            public NavLink InlineContent(IComponent content)
            {
                Add(new Nav.ComponentInNavLink(content));
                return this;
            }

            public NavLink Selected()
            {
                IsSelected = true;
                return this;
            }

            public NavLink SelectedIf(bool shouldSelect)
            {
                if (shouldSelect)
                {
                    IsSelected = true;
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

            public NavLink OnSelected(EventHandler<Nav.NavLink> onSelected)
            {
                OnSelect += onSelected;
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
                OnExpanded += (s, e) =>
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
                return this;
            }

            protected void ClickHandler(object sender)
            {
                if (HasChildren) IsExpanded = !IsExpanded;
                else IsSelected = true;
            }
        }
    }
}
