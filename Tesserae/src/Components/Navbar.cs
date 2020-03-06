using System;
using System.Collections.Generic;
using System.Linq;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class Navbar : IComponent
    {
        private HTMLElement _navbarContainer;
        private HTMLElement _contentContainer;
        private HTMLElement _container;

        public Navbar()
        {
            _navbarContainer = Div(_("tss-navbar"));
            _contentContainer = Div(_("tss-navbar-content"));
            _container = Div(_("tss-navbar-host"), _navbarContainer, _contentContainer);
        }

        public bool IsVisible
        {
            get => !_navbarContainer.classList.contains("hidden");
            set
            {
                if(value) _navbarContainer.classList.remove("hidden");
                else _navbarContainer.classList.add("hidden");
            }
        }

        public Navbar SetTop(IComponent top)
        {
            ClearChildren(_navbarContainer);
            _navbarContainer.appendChild(top.Render());
            return this;
        }

        public Navbar SetContent(IComponent content)
        {
            ClearChildren(_contentContainer);
            _contentContainer.appendChild(content.Render());
            return this;
        }

        public HTMLElement Render()
        {
            return _container;
        }
    }
}
