using System;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The bar at the top of the content while a <see cref="Sidebar"/> renders as a page
    /// (<see cref="Sidebar.AsPage"/>): a back button that brings the sidebar back, the app's brand and the
    /// title of the page on screen, with room for commands on the right.
    /// </summary>
    /// <remarks>
    /// It is part of the content rather than of the sidebar, since the sidebar is hidden while the content is on
    /// screen, and it draws itself only while its sidebar is a page - so it can stay mounted on every layout
    /// and cost a desktop nothing.
    /// </remarks>
    [Transpose.Name("tss.SidebarPageBar")]
    public sealed class SidebarPageBar : IComponent
    {
        private readonly Sidebar   _sidebar;
        private readonly Stack     _bar;
        private readonly Stack     _brand;
        private readonly TextBlock _title;
        private readonly Stack     _commands;

        /// <summary>
        /// Initializes a new instance of the SidebarPageBar class.
        /// </summary>
        /// <param name="sidebar">The sidebar its back button brings back.</param>
        public SidebarPageBar(Sidebar sidebar)
        {
            _sidebar = sidebar;

            var back = Button().SetIcon(UIcons.AngleLeft).Class("tss-sidebar-page-bar-back").OnClick(() => _sidebar.ShowSidebar());

            _brand    = HStack().Class("tss-sidebar-page-bar-brand").AlignItemsCenter().NoShrink();
            _title    = TextBlock().Class("tss-sidebar-page-bar-title").SemiBold().NoWrap();
            _commands = HStack().Class("tss-sidebar-page-bar-commands").AlignItemsCenter().NoShrink();

            _brand.Collapse();

            _bar = HStack().Class("tss-sidebar-page-bar").AlignItemsCenter().WS().NoShrink().Children(back, _brand, _title.W(10).Grow(), _commands);

            sidebar.PageMode.Observe(isPage =>
            {
                if (isPage) _bar.Show();
                else        _bar.Collapse();
            });
        }

        /// <summary>
        /// Gets or sets the title of the page on screen.
        /// </summary>
        public string Title
        {
            get => _title.Text;
            set => _title.Text = value ?? "";
        }

        /// <summary>
        /// Sets the brand shown between the back button and the title - typically the app's logo.
        /// </summary>
        /// <param name="brand">The brand, or null for none.</param>
        /// <returns>The current instance.</returns>
        public SidebarPageBar Brand(IComponent brand)
        {
            _brand.Clear();

            if (brand is object)
            {
                _brand.Add(brand);
                _brand.Show();
            }
            else
            {
                _brand.Collapse();
            }

            return this;
        }

        /// <summary>
        /// Sets the title of the page on screen.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <returns>The current instance.</returns>
        public SidebarPageBar SetTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary>
        /// Replaces the commands on the right of the bar.
        /// </summary>
        /// <param name="commands">The commands.</param>
        /// <returns>The current instance.</returns>
        public SidebarPageBar Commands(params IComponent[] commands)
        {
            _commands.Children(commands);
            return this;
        }

        /// <summary>
        /// Renders the bar.
        /// </summary>
        /// <returns>The rendered HTMLElement.</returns>
        public HTMLElement Render() => _bar.Render();
    }
}
