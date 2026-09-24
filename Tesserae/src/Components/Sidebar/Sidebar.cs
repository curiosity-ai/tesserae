using System;
using System.Collections.Generic;
using System.Linq;
using Transpose;
using TNT;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A Sidebar component that can be collapsed or expanded, containing header, middle, and footer sections.
    /// </summary>
    [Transpose.Name("tss.Sidebar")]
    public sealed class Sidebar : IComponent, IBindableComponent<bool>
    {
        private readonly ObservableList<ISidebarItem>                    _header;
        private readonly SettableObservable<IReadOnlyList<ISidebarItem>> _middleContent;
        private readonly ObservableList<ISidebarItem>                    _footer;
        private readonly SettableObservable<bool>                        _closed;
        private          double                                          _closedTimeout;
        private readonly Stack                                           _sidebar;
        private          bool                                            _isSortable;
        private          bool                                            _isNavbar;
        private readonly SettableObservable<bool>                        _pageMode;
        private readonly SettableObservable<bool>                        _pageShowsContent;
        private          bool                                            _pageClicksHooked;
        private readonly List<NavOverlay>                                _navOverlays = new List<NavOverlay>();
        private          bool                                            _pressingNavHeader;

        private Action<Dictionary<string, string[]>> _onSortingChanged;

        private List<string> _itemOrder = new List<string>();

        private HTMLDivElement _mobileBackdrop;

        // Shift support: when a child sidebar is mounted, the sections are moved inside a
        // horizontally sliding track that holds the main panel and the child panel side by side.
        private Sidebar        _shiftChild;
        private HTMLDivElement _shiftTrack;
        private HTMLDivElement _shiftChildPanel;
        private Stack          _shiftMainPanel;
        private IComponent     _shiftHost;
        private bool           _isShifted;
        private double         _shiftTimeout;
        private Action<bool>   _onShiftChanged;

        /// <summary>
        /// The transition time for sidebar animations in milliseconds.
        /// </summary>
        public const int SIDEBAR_TRANSITION_TIME = 300;

        /// <summary>
        /// How long shifting into a child sidebar and back takes - half the time the sidebar's own open/close
        /// animation takes, since both places are already there and the move wants to feel immediate. Kept in
        /// step with the transition on <c>.tss-sidebar-shift-track</c> in tss.sidebar.css.
        /// </summary>
        public const int SIDEBAR_SHIFT_TRANSITION_TIME = 150;

        /// <summary>
        /// Gets or sets whether the sidebar is closed.
        /// </summary>
        public bool IsClosed { get { return _closed.Value; } set { _closed.Value = value; } }

        /// <summary>
        /// Initializes a new instance of the Sidebar class.
        /// </summary>
        /// <param name="sortable">Whether the middle content items should be sortable.</param>
        public Sidebar(bool sortable = false)
        {
            _isSortable = sortable;

            _header        = new ObservableList<ISidebarItem>();
            _middleContent = new SettableObservable<IReadOnlyList<ISidebarItem>>(new List<ISidebarItem>());
            _footer        = new ObservableList<ISidebarItem>();
            _closed           = new SettableObservable<bool>(false);
            _pageMode         = new SettableObservable<bool>(false);
            _pageShowsContent = new SettableObservable<bool>(false);
            _sidebar          = VStack().Class("tss-sidebar");

            _closed.Observe(isClosed =>
            {
                // A shifted child sidebar is rendered inside this one, so it has to follow the same open/closed state
                if (_shiftChild is object)
                {
                    _shiftChild.IsClosed = isClosed && !IsPage;
                }

                // Show or hide the mobile backdrop (used in navbar/mobile mode)
                if (_mobileBackdrop is object)
                {
                    if (isClosed)
                    {
                        _mobileBackdrop.classList.remove("tss-mobile-backdrop-visible");
                    }
                    else
                    {
                        _mobileBackdrop.classList.add("tss-mobile-backdrop-visible");
                    }
                }

                //Do this on a timeout to improve the animation behaviour
                window.clearTimeout(_closedTimeout);

                _closedTimeout = window.setTimeout((_) =>
                {
                    //A page has no rail to collapse into: the state is kept for when the sidebar stops being one
                    if (IsPage) return;

                    if (isClosed)
                    {
                        _sidebar.Class(_isNavbar ? "tss-navbar-closed" : "tss-sidebar-closed");
                    }
                    else
                    {
                        _sidebar.RemoveClass(_isNavbar ? "tss-navbar-closed" : "tss-sidebar-closed");
                    }
                }, 15);
            });


            var combined = new CombinedObservable<IReadOnlyList<ISidebarItem>, IReadOnlyList<ISidebarItem>, IReadOnlyList<ISidebarItem>, bool>(_header, _middleContent, _footer, _closed);

            combined.ObserveFutureChanges(content => RenderSidebar(content.first, content.second, content.third, content.forth));

            _pageShowsContent.ObserveFutureChanges(_ => ApplyPageState());

            // disable Reordering in a closed sidebar
            _closed.ObserveFutureChanges(closed =>
            {
                if (closed)
                {
                    if (_isSortable)
                    {
                        var before = _isSortable;
                        _isSortable = false;
                        Refresh();
                        _isSortable = before;
                    }
                }
            });
        }

        /// <summary>
        /// Sets the sidebar background to the secondary background color.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public Sidebar Secondary()
        {
            _sidebar.Class("tss-sidebar-secondary");
            return this;
        }

        /// <summary>
        /// Sets whether the middle content items are sortable.
        /// </summary>
        /// <param name="sortable">Whether items are sortable.</param>
        public void Sortable(bool sortable = true)
        {
            _isSortable = sortable;
            Refresh();
        }

        /// <summary>
        /// Gets whether the sidebar is currently rendering as a navbar.
        /// </summary>
        public bool IsNavbar => _isNavbar;

        /// <summary>
        /// Configures the sidebar to render as a navbar - a horizontal bar with a hamburger that opens
        /// the items in a sliding drawer - or back to an ordinary vertical sidebar.
        /// </summary>
        /// <remarks>
        /// Both directions are supported so an app can follow <see cref="UI.Theme.OnMobileModeChanged"/>
        /// and switch on a window resize, rather than deciding once at startup and rendering a layout
        /// the stylesheet then reshapes underneath it.
        /// </remarks>
        /// <param name="isNavbar">Whether to render as a navbar.</param>
        /// <returns>The current instance.</returns>
        public Sidebar AsNavbar(bool isNavbar = true)
        {
            if (_isNavbar == isNavbar) return this;

            if (isNavbar && IsPage)
            {
                AsPage(false);
            }

            _isNavbar = isNavbar;

            if (isNavbar)
            {
                _sidebar.Horizontal();
                _sidebar.Class("tss-navbar");

                // The drawer always starts closed: on a phone it covers the page, so opening it
                // unasked would hide whatever the user came for.
                _closed.Value = true;

                // Apply the closed class synchronously so CSS states are correct before first paint,
                // and drop the one the vertical sidebar uses so only one of the two is ever on.
                _sidebar.RemoveClass("tss-sidebar-closed");
                _sidebar.Class("tss-navbar-closed");

                // Create a backdrop element appended to the body. On mobile it dims the background
                // when the drawer is open and closes the sidebar on click.
                if (_mobileBackdrop is null)
                {
                    _mobileBackdrop           = (HTMLDivElement)document.createElement("div");
                    _mobileBackdrop.className = "tss-mobile-backdrop";
                    _mobileBackdrop.addEventListener("click", (Event e) => { _closed.Value = true; });
                    document.body.appendChild(_mobileBackdrop);
                }
            }
            else
            {
                _sidebar.Vertical();
                _sidebar.RemoveClass("tss-navbar");
                _sidebar.RemoveClass("tss-navbar-closed");

                if (_closed.Value)
                {
                    _sidebar.Class("tss-sidebar-closed");
                }

                // The backdrop only ever belongs to a drawer, and it is a full-viewport click target,
                // so it goes away with the drawer rather than lingering over the page.
                if (_mobileBackdrop is object)
                {
                    _mobileBackdrop.remove();
                    _mobileBackdrop = null;
                }
            }

            Refresh();
            return this;
        }

        /// <summary>
        /// Gets whether the sidebar is currently rendering as a page - see <see cref="AsPage"/>.
        /// </summary>
        public bool IsPage => _pageMode.Value;

        /// <summary>
        /// Observes whether the sidebar renders as a page - see <see cref="AsPage"/>.
        /// </summary>
        public IObservable<bool> PageMode => _pageMode;

        /// <summary>
        /// Gets whether a sidebar rendering as a page has stepped aside for the content next to it.
        /// Always false while the sidebar is not a page.
        /// </summary>
        public bool IsShowingContent => IsPage && _pageShowsContent.Value;

        /// <summary>
        /// Observes whether a sidebar rendering as a page has stepped aside for the content (true) or is the
        /// page on screen (false).
        /// </summary>
        public IObservable<bool> ShowingContent => _pageShowsContent;

        /// <summary>
        /// Configures the sidebar to render as a page - the phone layout where the sidebar and the content next
        /// to it take turns filling the screen - or back to an ordinary sidebar.
        /// </summary>
        /// <remarks>
        /// As a page the sidebar is always open and fills its container, and everything after it in that
        /// container is hidden. Picking one of its buttons steps it aside (<see cref="ShowContent"/>), which
        /// hides the sidebar and brings the content back; <see cref="ShowSidebar"/> is the way back, and
        /// <see cref="SidebarPageBar"/> is a bar for the content that carries the button doing it.
        /// <para>
        /// The open/closed state is kept rather than cleared, so a sidebar that stops being a page returns to the
        /// rail the user left. Follow <see cref="UI.Theme.OnMobileModeChanged"/> to switch on a resize.
        /// </para>
        /// </remarks>
        /// <param name="isPage">Whether to render as a page.</param>
        /// <returns>The current instance.</returns>
        public Sidebar AsPage(bool isPage = true)
        {
            if (IsPage == isPage) return this;

            if (isPage && _isNavbar)
            {
                AsNavbar(false);
            }

            if (_shiftChild is object)
            {
                _shiftChild.IsClosed = _closed.Value && !isPage;
            }

            if (isPage)
            {
                _sidebar.Class("tss-sidebar-page");
                _sidebar.RemoveClass("tss-sidebar-closed");
                HookPageClicks();
            }
            else
            {
                CloseNavOverlays();
                _sidebar.RemoveClass("tss-sidebar-page");

                if (_closed.Value)
                {
                    _sidebar.Class("tss-sidebar-closed");
                }
            }

            _pageMode.Value = isPage;

            ApplyPageState();
            Refresh();
            return this;
        }

        /// <summary>
        /// Steps a sidebar rendering as a page aside, so the content next to it fills the screen. Does nothing
        /// visible while the sidebar is not a page, beyond remembering it for when it becomes one.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Sidebar ShowContent()
        {
            CloseNavOverlays();
            _pageShowsContent.Value = true;
            return this;
        }

        /// <summary>
        /// Brings a sidebar rendering as a page back in place of the content - the back button of
        /// <see cref="SidebarPageBar"/>.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Sidebar ShowSidebar()
        {
            _pageShowsContent.Value = false;
            return this;
        }

        private void ApplyPageState()
        {
            _sidebar.Render().UpdateClassIf(IsShowingContent, "tss-sidebar-page-hidden");
        }

        // Picking something in the sidebar is what steps it aside, and every row the sidebar draws passes
        // through its own element on the way, so one capture listener here covers the items a consumer builds
        // as well as the ones in this library - including those of a shifted child sidebar, which is mounted
        // inside this one. It runs after the click, so whatever the row does happens under the page it chose.
        private void HookPageClicks()
        {
            if (_pageClicksHooked) return;

            _pageClicksHooked = true;

            _sidebar.Render().addEventListener("click", (Action<Event>)(e =>
            {
                if (!IsPage) return;

                if (TryOpenNavOverlay(e)) return;

                var me = e.As<MouseEvent>();

                if (me.ctrlKey || me.metaKey || me.shiftKey) return; //opens in a new tab, this page stays where it is

                var target = e.target.As<HTMLElement>();

                if (target is null || !IsPageSelection(target)) return;

                window.setTimeout(_ => ShowContent(), 0);
            }), true);
        }

        // On a page a group does not expand in place: a row at a time is all a phone shows, and a list that
        // grows under the thumb moves everything below it. Pressing a group's header (or its arrow) opens its
        // children as a panel over the sidebar instead - the group's own children element, lifted out to the
        // sidebar while it is open (a placeholder keeps its place) so that every panel, however deep, has the
        // sidebar's own geometry rather than its parent panel's. A group inside a panel opens another on top,
        // and the ones behind step left by NAV_OVERLAY_PEEK each, so the depth reads as a deck the way
        // ModalStack's sheets do.
        private sealed class NavOverlay
        {
            public HTMLElement Nav;
            public HTMLElement Panel;
            public Node        Placeholder;
            public HTMLElement Backdrop;
            public HTMLElement Title;
        }

        /// <summary>How far each panel behind the front one steps left, in pixels - the strip of it that shows.</summary>
        private const int NAV_OVERLAY_PEEK = 12;

        /// <summary>How many panels behind the front one still peek out; deeper ones sit behind the last of them.</summary>
        private const int NAV_OVERLAY_MAX_PEEK_DEPTH = 3;

        private bool TryOpenNavOverlay(Event e)
        {
            if (_pressingNavHeader || _sidebar.Render().classList.contains("tss-sidebar-searching")) return false;

            var target = e.target.As<HTMLElement>();
            var header = target?.closest(".tss-sidebar-nav-header");

            if (header is null || header.classList.contains("tss-sidebar-nav-header-empty")) return false;
            if (target.closest(".tss-sidebar-commands") is object) return false;

            var nav = header.parentElement;

            if (nav is null || !nav.classList.contains("tss-sidebar-nav") || !nav.HasOwnProperty("tssOwner")) return false;
            if (nav.classList.contains("tss-sidebar-nav-overlay-open")) return false;

            e.preventDefault();
            e.stopPropagation();

            OpenNavOverlay(nav, nav["tssOwner"].As<SidebarNav>());
            return true;
        }

        private void OpenNavOverlay(HTMLElement nav, SidebarNav owner)
        {
            var panel = nav.querySelector(":scope > .tss-sidebar-nav-children").As<HTMLElement>();

            if (panel is null) return;

            var backdrop = Div(Att("tss-sidebar-nav-overlay-backdrop"));
            var back     = Button().SetIcon(UIcons.AngleLeft).Class("tss-sidebar-nav-overlay-back").OnClick(() => CloseNavOverlay());
            var label    = Span(Att("tss-sidebar-nav-overlay-label", text: owner.Text));
            var title    = Div(Att("tss-sidebar-nav-overlay-title"), back.Render(), label);

            // The group's own row, when it has one to go to: pressing the title is pressing the header
            if (owner.HasClickAction)
            {
                label.classList.add("tss-sidebar-nav-overlay-label-action");
                label.addEventListener("click", _ =>
                {
                    CloseNavOverlays();

                    _pressingNavHeader = true;
                    owner.PressHeader();
                    _pressingNavHeader = false;

                    window.setTimeout(__ => ShowContent(), 0);
                });
            }

            backdrop.addEventListener("click", _ => CloseNavOverlay());

            var placeholder = document.createComment("");
            panel.parentNode.insertBefore(placeholder, panel);

            var root = _sidebar.Render();
            root.appendChild(backdrop);
            root.appendChild(panel);

            panel.insertBefore(title, panel.firstChild);
            panel.classList.add("tss-sidebar-nav-overlay-panel");
            nav.classList.add("tss-sidebar-nav-overlay-open");

            _navOverlays.Add(new NavOverlay { Nav = nav, Panel = panel, Placeholder = placeholder, Backdrop = backdrop, Title = title });

            LayoutNavOverlays();
        }

        private void CloseNavOverlay()
        {
            if (_navOverlays.Count == 0) return;

            var overlay = _navOverlays[_navOverlays.Count - 1];
            _navOverlays.RemoveAt(_navOverlays.Count - 1);

            overlay.Nav.classList.remove("tss-sidebar-nav-overlay-open");
            overlay.Panel.classList.remove("tss-sidebar-nav-overlay-panel");
            overlay.Panel.style.zIndex    = "";
            overlay.Panel.style.transform = "";
            overlay.Title.remove();
            overlay.Backdrop.remove();

            // Back where the group draws it; a group re-rendered meanwhile has let go of this one, and it goes
            if (overlay.Placeholder.parentNode is object)
            {
                overlay.Placeholder.parentNode.insertBefore(overlay.Panel, overlay.Placeholder);
                overlay.Placeholder.parentNode.removeChild(overlay.Placeholder);
            }
            else
            {
                overlay.Panel.remove();
            }

            LayoutNavOverlays();
        }

        private void CloseNavOverlays()
        {
            while (_navOverlays.Count > 0) CloseNavOverlay();
        }

        // Each panel over the one it came from, and each backdrop between the two - so a panel behind is dimmed
        // by the backdrop of the one in front, and shows only the strip its step to the left uncovers.
        private void LayoutNavOverlays()
        {
            var count = _navOverlays.Count;

            for (var i = 0; i < count; i++)
            {
                var overlay = _navOverlays[i];
                var depth   = Math.Min(count - 1 - i, NAV_OVERLAY_MAX_PEEK_DEPTH);

                //Each backdrop dims everything under it, so the ones past the first are lighter or the sidebar goes black
                overlay.Backdrop.style.zIndex  = (20 + i * 2).ToString();
                overlay.Backdrop.style.opacity = i == 0 ? "" : "0.5";
                overlay.Panel.style.zIndex    = (21 + i * 2).ToString();
                overlay.Panel.style.transform = depth == 0 ? "" : $"translateX(-{depth * NAV_OVERLAY_PEEK}px)";

                overlay.Panel.UpdateClassIf(depth > 0, "tss-sidebar-nav-overlay-behind");
            }
        }

        // A row that goes somewhere, as opposed to the chrome around it: a row's own commands, a search box, and
        // the brand and profile rows, whose click opens a menu anchored on the sidebar that is about to be hidden.
        private static bool IsPageSelection(HTMLElement target)
        {
            if (target.closest(".tss-sidebar-btn-open") is null) return false;

            if (target.closest(".tss-sidebar-commands, .tss-sidebar-btn-searchbox, .tss-sidebar-searchbox, .tss-sidebar-identity") is object) return false;

            // A group's header is a row too once it has nothing to open (a group with children opened a panel)
            var header = target.closest(".tss-sidebar-nav-header");

            return header is null || header.classList.contains("tss-sidebar-nav-header-empty");
        }

        private void RenderSidebar(IReadOnlyList<ISidebarItem> header, IReadOnlyList<ISidebarItem> middle, IReadOnlyList<ISidebarItem> footer, bool closed)
        {
            closed = closed && !IsPage;

            CloseNavOverlays();

            var stackMiddle = VStack();

            if (_isSortable)
            {
                var sortable = new Sortable(stackMiddle.Render(), new SortableOptions()
                {
                    animation     = 150,
                    invertSwap    = true,
                    ghostClass    = "tss-sortable-ghost",
                    swapThreshold = 0.65,
                    filter        = ".tss-sortable-disable",
                    onEnd = e =>
                    {
                        if (e.oldIndex != e.newIndex)
                        {
                            var old = _itemOrder[e.oldIndex];
                            _itemOrder.RemoveAt(e.oldIndex);
                            _itemOrder.Insert(e.newIndex, old);
                            _onSortingChanged?.Invoke(GetCurrentSorting());
                        }
                    }
                });

                foreach (var middleItem in middle)
                {
                    if (middleItem is SidebarNav middleNavItem)
                    {
                        middleNavItem.Sortable();

                        middleNavItem.OnSortingChanged(newItemOrder =>
                        {
                            var itemOrder = GetCurrentSorting();

                            foreach (var newItem in newItemOrder)
                            {
                                itemOrder[newItem.Key] = newItem.Value;
                            }

                            _onSortingChanged?.Invoke(itemOrder);
                        });
                    }
                }
            }

            if (_isNavbar)
            {
                var hamburger = Button().Class("tss-navbar-burger").SetIcon(UIcons.MenuBurger).OnClick(() => _closed.Toggle());

                var mobileHeader = HStack().Class("tss-navbar-mobile-header").Children(
                    TextBlock("Navigation").SemiBold().Foreground(UI.Theme.Default.Foreground),
                    Button().Class("tss-navbar-mobile-close").SetIcon(UIcons.Cross).OnClick(() => _closed.Value = true)
                );

                // In navbar/mobile mode, search boxes belong inside the drawer so users can
                // search the sample list from the hamburger menu instead of the top bar.
                var topBarHeader = header.Where(si => !(si is SidebarSearchBox)).ToList();
                var drawerHeader = header.Where(si => si is SidebarSearchBox).ToList();

                var drawer = VStack().Class("tss-navbar-drawer")
                   .Children(
                        mobileHeader,
                        VStack().Class("tss-sidebar-header").Class("tss-navbar-drawer-header").WS().NoShrink().Children(drawerHeader.Select(si => si.RenderOpen())),
                        stackMiddle.Class("tss-sidebar-middle").WS().MinHeight(new UnitSize("fit-content")).MaxHeight(80.vh()).ScrollY().Children(middle.Select(si => AttachNavbarClose(si.RenderOpen()))),
                        VStack().Class("tss-sidebar-footer").WS().NoShrink().Children(footer.Select(si => AttachNavbarClose(si.RenderOpen())))
                    );

                if (closed)
                {
                    drawer.Collapse();
                }
                else
                {
                    drawer.Show();
                }

                _sidebar.Children(
                    HStack().Class("tss-sidebar-header").W(10).Grow().NoShrink().Children(topBarHeader.Select(si => si.RenderOpen())),
                    Stack().Grow(),
                    hamburger,
                    drawer
                );
            }
            else
            {
                var sections = new IComponent[]
                {
                    VStack().Class("tss-sidebar-header").WS().NoShrink().Children(header.Select(si => closed ? si.RenderClosed() : si.RenderOpen())),
                    stackMiddle.Class("tss-sidebar-middle").WS().H(10).Grow().ScrollY().Children(middle.Select(si => closed ? si.RenderClosed() : si.RenderOpen())),
                    VStack().Class("tss-sidebar-footer").WS().NoShrink().Children(footer.Select(si => closed ? si.RenderClosed() : si.RenderOpen()))
                };

                if (_shiftChild is object)
                {
                    // The sections live in the main panel of the sliding track instead of directly in the sidebar
                    _shiftMainPanel.Children(sections);
                    _sidebar.Children(_shiftHost);
                }
                else
                {
                    _sidebar.Children(sections);
                }
            }
        }

        private IComponent AttachNavbarClose(IComponent component)
        {
            var el = component.Render();

            if (!el.HasOwnProperty("_NAVBAR_CLOSE"))
            {
                el["_NAVBAR_CLOSE"] = true;
                el.addEventListener("click", (Action<Event>)(_ => _closed.Value = true), true);
            }
            return component;
        }

        /// <summary>
        /// Sets whether the sidebar is closed.
        /// </summary>
        /// <param name="isClosed">Whether the sidebar is closed.</param>
        /// <returns>The current instance of the type.</returns>
        public Sidebar Closed(bool isClosed = true)
        {
            _closed.Value = isClosed;
            return this;
        }

        /// <summary>
        /// Toggles the closed state of the sidebar.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public Sidebar Toggle()
        {
            _closed.Toggle();
            return this;
        }

        /// <summary>
        /// Gets whether the sidebar is currently showing the shifted (child) sidebar.
        /// </summary>
        public bool IsShifted => _isShifted;

        /// <summary>
        /// Gets the child sidebar that is currently mounted for shifting, if any.
        /// </summary>
        public Sidebar ShiftedSidebar => _shiftChild;

        /// <summary>
        /// Slides horizontally into a child sidebar, used when navigating into an interface that has its own sidebar.
        /// The child sidebar is rendered inside this one and follows its open/closed state.
        /// Only one depth level is supported: shifting into another child replaces the current one.
        /// Call <see cref="ShiftBack"/> to slide back into this sidebar.
        /// </summary>
        /// <param name="child">The sidebar to shift into.</param>
        /// <returns>The current instance of the type.</returns>
        public Sidebar ShiftTo(Sidebar child)
        {
            if (child is null)
            {
                return ShiftBack();
            }

            if (_isNavbar)
            {
                return this; //shifting is not supported while rendering as a navbar
            }

            if (!ReferenceEquals(_shiftChild, child))
            {
                EnsureShiftScaffolding();

                _shiftChild = child;
                child.IsClosed = _closed.Value && !IsPage;

                ClearChildren(_shiftChildPanel);
                _shiftChildPanel.appendChild(child.Render());

                Refresh(); //moves the sections into the main panel of the track
            }

            SetShifted(true);
            return this;
        }

        /// <summary>
        /// Slides back from the child sidebar into this sidebar.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public Sidebar ShiftBack()
        {
            SetShifted(false);
            return this;
        }

        /// <summary>
        /// Adds a handler that is called whenever the sidebar shifts into the child sidebar (true) or back (false).
        /// </summary>
        /// <param name="onShiftChanged">The event handler.</param>
        /// <returns>The current instance of the type.</returns>
        public Sidebar OnShiftChanged(Action<bool> onShiftChanged)
        {
            _onShiftChanged = onShiftChanged;
            return this;
        }

        private void EnsureShiftScaffolding()
        {
            if (_shiftTrack is object) return;

            _shiftMainPanel  = VStack().Class("tss-sidebar-shift-panel").Class("tss-sidebar-shift-panel-main");
            _shiftChildPanel = Div(Att("tss-sidebar-shift-panel tss-sidebar-shift-panel-child tss-sidebar-shift-panel-hidden"));
            _shiftTrack      = Div(Att("tss-sidebar-shift-track"), _shiftMainPanel.Render(), _shiftChildPanel);
            _shiftHost       = Raw(_shiftTrack).WS().Grow();

            _sidebar.Class("tss-sidebar-has-shift");
        }

        private void SetShifted(bool shifted)
        {
            if (_shiftTrack is null || _isShifted == shifted) return;

            _isShifted = shifted;

            window.clearTimeout(_shiftTimeout);

            var mainPanel  = _shiftMainPanel.Render();
            var enterPanel = shifted ? _shiftChildPanel : mainPanel;
            var leavePanel = shifted ? mainPanel : _shiftChildPanel;

            // The panel we're sliding into is display:none, so it has to be laid out before the
            // transform starts, otherwise the browser has nothing to animate towards.
            enterPanel.classList.remove("tss-sidebar-shift-panel-hidden");
            var _flush = _shiftTrack.offsetWidth;

            if (shifted)
            {
                _shiftTrack.classList.add("tss-sidebar-shifted");
            }
            else
            {
                _shiftTrack.classList.remove("tss-sidebar-shifted");
            }

            // Once the slide is over the panel that moved out of view is hidden, so it stops
            // taking focus and is no longer reachable by screen readers.
            _shiftTimeout = window.setTimeout((_) => leavePanel.classList.add("tss-sidebar-shift-panel-hidden"), SIDEBAR_SHIFT_TRANSITION_TIME);

            _onShiftChanged?.Invoke(shifted);
        }

        /// <summary>
        /// Adds an item to the sidebar header section.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The current instance of the type.</returns>
        public Sidebar AddHeader(ISidebarItem item)
        {
            _header.Add(item);
            return this;
        }

        /// <summary>
        /// Inserts an item after another item in the sidebar header section.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <param name="addAfter">The item to insert after.</param>
        /// <returns>The current instance of the type.</returns>
        public Sidebar InsertAfterHeader(ISidebarItem item, ISidebarItem addAfter)
        {
            var index = _header.IndexOf(addAfter);

            if (index >= 0)
            {
                _header.Insert(index + 1, item);
            }
            else
            {
                _header.Add(item);
            }
            return this;
        }

        /// <summary>
        /// The root identifier used for ordering.
        /// </summary>
        public const string ROOT_SIDEBAR_FOR_ORDERING = "ROOT";

        /// <summary>
        /// Adds an item to the middle content section.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The current instance of the type.</returns>
        public Sidebar AddContent(ISidebarItem item)
        {
            item.AddGroupIdentifier(ROOT_SIDEBAR_FOR_ORDERING);

            var existing = _middleContent.Value.FirstOrDefault(m => m.Identifier == item.Identifier);

            if (existing is object)
            {
                //The identifier says which row this is, so a newly built item takes the standing one's place
                //rather than being dropped. A sidebar is rebuilt out of data that changed and the item is that
                //data - dropping it leaves the sidebar showing what it was built from the previous time.
                if (ReferenceEquals(existing, item)) return this; //nothing to do...

                _middleContent.Value = _middleContent.Value.Select(m => ReferenceEquals(m, existing) ? item : m).ToList();
                return this;
            }

            _middleContent.Value = _middleContent.Value?.Concat(new[] { item }).ToList();
            _itemOrder.Add(item.Identifier);
            return this;
        }

        /// <summary>
        /// Inserts an item after another item in the middle content section.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <param name="addAfter">The item to insert after.</param>
        /// <returns>The current instance of the type.</returns>
        public Sidebar InsertAfterContent(ISidebarItem item, ISidebarItem addAfter)
        {
            item.AddGroupIdentifier(ROOT_SIDEBAR_FOR_ORDERING);

            //Already placed: leave it where it stands and let AddContent swap in the newly built item.
            if (_middleContent.Value.Any(m => m.Identifier == item.Identifier)) return AddContent(item);

            var middleContentList = _middleContent.Value.ToList();
            var index             = middleContentList.IndexOf(addAfter);

            if (index >= 0)
            {
                middleContentList.Insert(index + 1, item);
                _middleContent.Value = middleContentList;

                var orderIndex = _itemOrder.IndexOf(addAfter.Identifier);

                if (orderIndex >= 0)
                {
                    _itemOrder.Insert(orderIndex + 1, item.Identifier);
                }
                else
                {
                    _itemOrder.Add(item.Identifier);
                }
            }
            else
            {
                AddContent(item);
            }

            return this;
        }

        /// <summary>
        /// Removes an item from the middle content section.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>The current instance of the type.</returns>
        public Sidebar RemoveContent(ISidebarItem item)
        {
            //Match the stamped identifier AddContent gave the item standing there. A caller normally hands in a
            //freshly built item, whose identifier carries no group yet and so used to match nothing at all.
            var identifier = WithGroupIdentifier(item.Identifier, ROOT_SIDEBAR_FOR_ORDERING);

            if (_middleContent.Value.All(m => m.Identifier != identifier)) return this; //nothing to do

            _middleContent.Value = _middleContent.Value?.Where(m => m.Identifier != identifier).ToList();
            _itemOrder.Remove(identifier);
            return this;
        }

        /// <summary>
        /// Adds an item to the sidebar footer section.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The current instance of the type.</returns>
        public Sidebar AddFooter(ISidebarItem item)
        {
            _footer.Add(item);
            return this;
        }

        /// <summary>
        /// Inserts an item after another item in the sidebar footer section.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <param name="addAfter">The item to insert after.</param>
        /// <returns>The current instance of the type.</returns>
        public Sidebar InsertAfterFooter(ISidebarItem item, ISidebarItem addAfter)
        {
            var index = _footer.IndexOf(addAfter);

            if (index >= 0)
            {
                _footer.Insert(index + 1, item);
            }
            else
            {
                _footer.Add(item);
            }
            return this;
        }

        /// <summary>
        /// Clears all sections of the sidebar.
        /// </summary>
        public void Clear()
        {
            ClearHeader();
            ClearContent();
            ClearFooter();
        }
        /// <summary>Clears the header section.</summary>
        public void ClearHeader() => _header.Clear();
        /// <summary>Clears the middle content section.</summary>
        public void ClearContent() => _middleContent.Value = new List<ISidebarItem>();
        /// <summary>Clears the footer section.</summary>
        public void ClearFooter() => _footer.Clear();

        /// <summary>
        /// Renders the sidebar.
        /// </summary>
        /// <returns>The rendered HTMLElement.</returns>
        public HTMLElement Render() => _sidebar.Render();

        /// <summary>
        /// Returns an observable that tracks the closed/open state of the sidebar.
        /// </summary>
        public IObservable<bool> AsObservable() => _closed;

        /// <summary>
        /// Programmatically opens or closes the sidebar as part of a two-way binding.
        /// </summary>
        public void SetBoundValue(bool value) => _closed.Value = value;


        /// <summary>
        /// Loads the sorting order for sidebar items.
        /// Should be called after all items have been added.
        /// </summary>
        /// <param name="itemOrder">A dictionary mapping group identifiers to ordered item identifiers.</param>
        public void LoadSorting(Dictionary<string, string[]> itemOrder)
        {
            if (itemOrder.TryGetValue(ROOT_SIDEBAR_FOR_ORDERING, out var topLevelOrder) && topLevelOrder is object)
            {
                var dict = new object();

                for (var i = 0; i < topLevelOrder.Length; i++)
                {
                    dict[topLevelOrder[i]] = i;
                }

                var itemOrderSorted = _itemOrder.OrderBy(i => dict.HasOwnProperty(i) ? dict[i] : int.MaxValue).Distinct().ToList();

                if (!_itemOrder.SequenceEqual(itemOrderSorted))
                {
                    _middleContent.Value = _middleContent.Value.OrderBy(i => dict.HasOwnProperty(i.Identifier) ? dict[i.Identifier] : int.MaxValue).ToArray();
                    _itemOrder           = itemOrderSorted;
                }
            }

            foreach (var middleItem in _middleContent.Value)
            {
                if (middleItem is SidebarNav middleNavItem)
                {
                    middleNavItem.LoadSorting(itemOrder);
                }
            }
        }

        /// <summary>
        /// Gets the current sorting order of all items in the sidebar.
        /// </summary>
        /// <returns>A dictionary mapping group identifiers to ordered item identifiers.</returns>
        public Dictionary<string, string[]> GetCurrentSorting()
        {
            var dict = new Dictionary<string, string[]>();

            foreach (var item in _middleContent.Value)
            {
                if (item is SidebarNav nav)
                {
                    foreach (var sorting in nav.GetCurrentSorting())
                    {
                        dict[sorting.Key] = sorting.Value.Distinct().ToArray();
                    }
                }
            }

            dict[ROOT_SIDEBAR_FOR_ORDERING] = _itemOrder.Distinct().ToArray();

            return dict;
        }

        /// <summary>
        /// Refreshes the sidebar rendering.
        /// </summary>
        public void Refresh()
        {
            RenderSidebar(_header.Value, _middleContent.Value, _footer.Value, _closed.Value);
        }

        /// <summary>
        /// Adds a sorting change event handler.
        /// </summary>
        /// <param name="onSortingChanged">The event handler.</param>
        public void OnSortingChanged(Action<Dictionary<string, string[]>> onSortingChanged)
        {
            _onSortingChanged = onSortingChanged;
        }

        /// <summary>
        /// Searches/Filters the sidebar content items.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        public void Search(string searchTerm)
        {
            //A page shows the hits of a search in place: a panel per group would hide the ones in every other group
            _sidebar.Render().UpdateClassIf(!string.IsNullOrWhiteSpace(searchTerm), "tss-sidebar-searching");

            foreach (var item in _middleContent.Value)
            {
                if (item is ISearchableSidebarItem searchable)
                {
                    searchable.Search(searchTerm);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(searchTerm))
                    {
                        item.Show();
                    }
                    else
                    {
                        item.Collapse();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the own identifier of an item by removing group identifier prefix.
        /// </summary>
        /// <param name="identifier">The full identifier.</param>
        /// <returns>The own identifier.</returns>
        public static string GetOwnIdentifier(string identifier)
        {
            var ix = identifier.IndexOf(GroupIdentifierSeparator);

            if (ix >= 0)
            {
                return identifier.Substring(ix + GroupIdentifierSeparator.Length);
            }
            return identifier;
        }

        /// <summary>
        /// The separator used between group identifiers and item identifiers.
        /// </summary>
        public const string GroupIdentifierSeparator = "_|_";

        /// <summary>
        /// Stamps an item's identifier with the group it is being placed in, unless it already carries it.
        /// </summary>
        /// <remarks>
        /// The stamped identifier is how an item is found again once it is placed, so stamping twice would
        /// leave it unfindable. That happens whenever an item is placed, taken out and placed back, and
        /// whenever a caller hands in a freshly built item for a method that has to match the one already
        /// there - which is why every method that matches an identifier normalises through this first.
        /// </remarks>
        /// <param name="identifier">The item's current identifier.</param>
        /// <param name="groupIdentifier">The group the item is being placed in.</param>
        /// <returns>The identifier as it reads inside that group.</returns>
        public static string WithGroupIdentifier(string identifier, string groupIdentifier)
        {
            var prefix = groupIdentifier + GroupIdentifierSeparator;

            return identifier is object && identifier.StartsWith(prefix) ? identifier : prefix + identifier;
        }
    }
}