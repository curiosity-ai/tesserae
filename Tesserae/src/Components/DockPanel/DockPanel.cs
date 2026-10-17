using System;
using System.Collections.Generic;
using System.Linq;
using Transpose;
using Transpose.Core;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// An IDE-style docking layout: one document in the middle and any number of <see cref="DockPane"/>s docked
    /// around it. Panes on the same edge stack, and share a slot as tabs when they name the same tab group; every seam
    /// is a splitter. A pane can be closed (and reopened from the restore menu), unpinned to a rail on its edge from
    /// where it slides out over the document, and dragged by its header onto another edge or into another pane's slot,
    /// with drop guides showing where it would land. The arrangement can be saved with <see cref="GetLayout"/> and put
    /// back with <see cref="LoadLayout"/>.
    /// <para>
    /// The layout is rebuilt on every change, moving each pane's content element rather than recreating it, so the
    /// content keeps its state; the scroll position of each pane body is carried across.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.DockPanel")]
    public sealed class DockPanel : ComponentBase<DockPanel, HTMLElement>, ISpecialCaseStyling
    {
        private const double DragThreshold = 6;
        private const double GuideSize     = 34;

        private enum DropKind { None, Edge, Tab, Before, After }

        private sealed class Slot
        {
            public DockEdge       Edge;
            public string         Group;
            public List<DockPane> Panes;
            public DockPane       Active;
            public HTMLElement    Element;
        }

        private sealed class Guide
        {
            public DropKind    Kind;
            public DockEdge    Edge;
            public Slot        Target;
            public HTMLElement Element;
        }

        private readonly HTMLElement _main;
        private readonly HTMLElement _documentHost;
        private readonly HTMLElement _documentBody;
        private readonly HTMLElement _restoreButton;
        private readonly Dictionary<DockEdge, HTMLElement> _rails = new Dictionary<DockEdge, HTMLElement>();

        private readonly List<DockPane>             _panes        = new List<DockPane>();
        private readonly List<Slot>                 _slots        = new List<Slot>();
        private readonly Dictionary<string, string> _activeTab    = new Dictionary<string, string>();

        private DockEdge[]      _edgePriority  = { DockEdge.Top, DockEdge.Bottom, DockEdge.Left, DockEdge.Right };
        private DockPanelLabels _labels        = new DockPanelLabels();
        private bool            _restoreMenu   = true;
        private double          _collapseBelow = 600;
        private bool            _narrow;
        private bool            _rendered;
        private bool            _dirty         = true;
        private bool            _flushQueued;
        private ResizeObserver  _resizeObserver;

        private Action<string>      _onLayoutChanged;
        private Func<DockPane, bool> _onCloseRequested;
        private Action<DockPane>    _onFlyoutOpened;

        // the open flyout
        private DockPane      _flyoutPane;
        private HTMLElement   _flyout;
        private Action<Event> _onFlyoutOutside;
        private readonly Action<Event> _onFlyoutKey;

        // a header drag in progress
        private DockPane      _dragPane;
        private double        _dragStartX, _dragStartY;
        private bool          _dragActive;
        private HTMLElement   _dragGhost;
        private HTMLElement   _guideLayer;
        private HTMLElement   _preview;
        private readonly List<Guide> _guides = new List<Guide>();
        private Guide         _hoverGuide;
        private Slot          _compassSlot;
        private Action<Event> _onDragMove;
        private Action<Event> _onDragUp;
        private Action<Event> _onDragKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="DockPanel"/> class.
        /// </summary>
        public DockPanel()
        {
            _onFlyoutKey   = e => OnFlyoutKey(e);
            _documentBody  = Div(Att("tss-dockpanel-document-body"));
            _restoreButton = Button(Att("tss-dockpanel-restore", type: "button"), I(UIcons.MenuDots));
            _restoreButton.setAttribute("aria-label", _labels.ClosedPanes);
            _restoreButton.setAttribute("aria-haspopup", "menu");
            _restoreButton.addEventListener("click", e =>
            {
                e.stopPropagation();
                ShowRestoreMenu();
            });

            _documentHost = Div(Att("tss-dockpanel-document"), _documentBody, _restoreButton);
            _main         = Div(Att("tss-dockpanel-main"));

            foreach (var edge in new[] { DockEdge.Top, DockEdge.Left, DockEdge.Right, DockEdge.Bottom })
            {
                var rail = Div(Att("tss-dockpanel-rail tss-dockpanel-rail-" + edge.ToString().ToLower(), role: "toolbar"));
                rail.setAttribute("aria-orientation", edge == DockEdge.Left || edge == DockEdge.Right ? "vertical" : "horizontal");
                _rails[edge] = rail;
            }

            InnerElement = Div(Att("tss-dockpanel"), _rails[DockEdge.Top], _rails[DockEdge.Left], _main, _rails[DockEdge.Right], _rails[DockEdge.Bottom]);
        }

        /// <summary>Gets the element sizing helpers write to.</summary>
        public HTMLElement StylingContainer => InnerElement;

        /// <summary>Gets whether a sizing helper applied to this component should tag it so a wrapper-building container hoists the style onto the wrapper.</summary>
        public bool PropagateStylesToWrapper => true;

        /// <summary>The panes, in layout order.</summary>
        public IReadOnlyList<DockPane> CurrentPanes => _panes;

        /// <summary>Whether the panel is below <see cref="CollapseBelow"/> and shows its pinned panes on the rails.</summary>
        public bool IsNarrow => _narrow;

        // ------------------------------------------------------------- content

        /// <summary>Sets the document: the content in the middle that the panes are docked around.</summary>
        public DockPanel Document(IComponent document)
        {
            ClearChildren(_documentBody);
            if (document is object) _documentBody.appendChild(document.Render());
            return this;
        }

        /// <summary>Adds panes.</summary>
        public DockPanel Panes(params DockPane[] panes)
        {
            foreach (var pane in panes ?? new DockPane[0]) Add(pane);
            return this;
        }

        /// <summary>Adds a pane. A pane whose id is already taken replaces the old one in place.</summary>
        public DockPanel Add(DockPane pane)
        {
            if (pane is null) return this;

            var index = _panes.FindIndex(p => p.Id == pane.Id);
            if (index >= 0) _panes[index] = pane;
            else _panes.Add(pane);

            return Invalidate();
        }

        /// <summary>Removes the pane with the given id.</summary>
        public DockPanel Remove(string id)
        {
            _panes.RemoveAll(p => p.Id == id);
            if (_flyoutPane?.Id == id) CloseFlyout();
            return Invalidate();
        }

        /// <summary>Returns the pane with the given id, or null.</summary>
        public DockPane Pane(string id) => _panes.FirstOrDefault(p => p.Id == id);

        // --------------------------------------------------------------- options

        /// <summary>
        /// Decides which edges own the corners. The first edge is laid out outermost and spans the whole panel; each
        /// later one fits inside the earlier ones. Defaults to Top, Bottom, Left, Right: the top and bottom panes span
        /// the full width.
        /// </summary>
        public DockPanel EdgePriority(params DockEdge[] edges)
        {
            var list = (edges ?? new DockEdge[0]).Distinct().ToList();
            foreach (var edge in new[] { DockEdge.Top, DockEdge.Bottom, DockEdge.Left, DockEdge.Right }) if (!list.Contains(edge)) list.Add(edge);
            _edgePriority = list.ToArray();
            return Invalidate();
        }

        /// <summary>
        /// Below this width (in pixels) the panel shows every pinned pane on its rail instead of beside the document,
        /// without changing the pane; above it they dock again. Defaults to 600; 0 turns it off.
        /// </summary>
        public DockPanel CollapseBelow(double pixels)
        {
            _collapseBelow = Math.Max(0, pixels);
            UpdateNarrow(InnerElement.getBoundingClientRect().As<DOMRect>().width);
            return this;
        }

        /// <summary>Shows or hides the menu of closed panes at the top right of the document. Shown by default.</summary>
        public DockPanel RestoreMenu(bool show = true)
        {
            _restoreMenu = show;
            return Invalidate();
        }

        /// <summary>Replaces the text the panel draws on its own, for translation.</summary>
        public DockPanel Labels(DockPanelLabels labels)
        {
            _labels = labels ?? new DockPanelLabels();
            _restoreButton.setAttribute("aria-label", _labels.ClosedPanes);
            return Invalidate();
        }

        // ---------------------------------------------------------------- events

        /// <summary>
        /// Raised after every change of the arrangement — a pane moved, resized, closed, reopened, pinned or unpinned, a
        /// tab selected — with the layout as <see cref="GetLayout"/> writes it, ready to be saved.
        /// </summary>
        public DockPanel OnLayoutChanged(Action<string> onLayoutChanged)
        {
            _onLayoutChanged = onLayoutChanged;
            return this;
        }

        /// <summary>Raised when the user asks to close a pane. Return false to keep it open.</summary>
        public DockPanel OnCloseRequested(Func<DockPane, bool> onCloseRequested)
        {
            _onCloseRequested = onCloseRequested;
            return this;
        }

        /// <summary>Raised when an unpinned pane slides out from its rail.</summary>
        public DockPanel OnFlyoutOpened(Action<DockPane> onFlyoutOpened)
        {
            _onFlyoutOpened = onFlyoutOpened;
            return this;
        }

        // ------------------------------------------------------------- commands

        /// <summary>Opens a closed pane where it was, and selects it if it is a tab.</summary>
        public DockPanel Open(string id) => Change(id, p =>
        {
            p.IsOpen = true;
            if (p.TabGroup is object) _activeTab[GroupKey(p)] = p.Id;
        });

        /// <summary>Closes a pane. The restore menu lists it until it is opened again.</summary>
        public DockPanel Close(string id) => Change(id, p => p.IsOpen = false);

        /// <summary>Docks an unpinned pane beside the document again.</summary>
        public DockPanel Pin(string id) => Change(id, p => p.IsPinned = true);

        /// <summary>Unpins a pane to its edge's rail; it slides out over the document when its tab is clicked.</summary>
        public DockPanel Unpin(string id) => Change(id, p => p.IsPinned = false);

        /// <summary>Selects a pane's tab in its group.</summary>
        public DockPanel Activate(string id) => Change(id, p =>
        {
            if (p.TabGroup is object) _activeTab[GroupKey(p)] = p.Id;
        });

        /// <summary>Moves a pane to another edge, on its own (out of any tab group).</summary>
        public DockPanel Move(string id, DockEdge edge) => Change(id, p =>
        {
            p.Edge     = edge;
            p.TabGroup = null;
            p.IsPinned = true;
        });

        /// <summary>Slides an unpinned (or collapsed) pane out over the document.</summary>
        public DockPanel ShowFlyout(string id)
        {
            var pane = Pane(id);
            if (pane is object && pane.IsOpen && IsOnRail(pane)) OpenFlyout(pane);
            return this;
        }

        private DockPanel Change(string id, Action<DockPane> change)
        {
            var pane = Pane(id);
            if (pane is null) return this;

            change(pane);
            if (_flyoutPane == pane) CloseFlyout();
            return Changed();
        }

        // ---------------------------------------------------------- persistence

        /// <summary>Writes the arrangement — every pane's edge, size, share, tab group, pin and open state, and order — as JSON.</summary>
        public string GetLayout()
        {
            var state = new DockLayoutState
            {
                version = 1,
                panes   = _panes.Select(p => new DockPaneState
                {
                    id     = p.Id,
                    edge   = p.Edge.ToString(),
                    size   = Math.Round(p.Size),
                    weight = Math.Round(p.Weight * 1000) / 1000,
                    open   = p.IsOpen,
                    pinned = p.IsPinned,
                    tab    = p.TabGroup,
                    active = p.TabGroup is object && ActiveIn(p.Edge, p.TabGroup)?.Id == p.Id
                }).ToArray()
            };

            return es5.JSON.stringify(state);
        }

        /// <summary>
        /// Restores an arrangement written by <see cref="GetLayout"/>. Panes it does not mention keep their place after
        /// the ones it does, ids it mentions that are not panes are ignored, an edge the pane does not allow is ignored
        /// and sizes are clamped to the pane's limits. Returns false, changing nothing, if the text cannot be read.
        /// </summary>
        public bool LoadLayout(string layout)
        {
            if (string.IsNullOrWhiteSpace(layout)) return false;

            DockLayoutState state;

            try
            {
                state = es5.JSON.parse(layout).As<DockLayoutState>();
            }
            catch
            {
                return false;
            }

            if (state is null || Script.Write<bool>("!Array.isArray({0})", state.panes)) return false;

            var ordered = new List<DockPane>();

            foreach (var record in state.panes)
            {
                if (record is null) continue;

                var pane = _panes.FirstOrDefault(p => p.Id == record.id);
                if (pane is null || ordered.Contains(pane)) continue;

                if (TryParseEdge(record.edge, out var edge) && pane.AllowedEdges.Contains(edge)) pane.Edge = edge;
                if (record.size > 0)   pane.Size   = Math.Max(pane.MinSize, Math.Min(pane.MaxSize, record.size));
                if (record.weight > 0) pane.Weight = record.weight;

                pane.IsOpen   = record.open;
                pane.IsPinned = record.pinned;
                pane.TabGroup = string.IsNullOrEmpty(record.tab) ? null : record.tab;
                if (record.active && pane.TabGroup is object) _activeTab[GroupKey(pane)] = pane.Id;

                ordered.Add(pane);
            }

            foreach (var pane in _panes) if (!ordered.Contains(pane)) ordered.Add(pane);

            _panes.Clear();
            _panes.AddRange(ordered);
            CloseFlyout();
            Invalidate();
            return true;
        }

        private static bool TryParseEdge(string text, out DockEdge edge)
        {
            switch (text)
            {
                case "Left":   edge = DockEdge.Left;   return true;
                case "Right":  edge = DockEdge.Right;  return true;
                case "Top":    edge = DockEdge.Top;    return true;
                case "Bottom": edge = DockEdge.Bottom; return true;
                default:       edge = DockEdge.Left;   return false;
            }
        }

        // ------------------------------------------------------------- rendering

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            if (!_rendered)
            {
                _rendered = true;

                _resizeObserver = new ResizeObserver((entries, _) => UpdateNarrow(InnerElement.getBoundingClientRect().As<DOMRect>().width));
                DomObserver.WhenMounted(InnerElement, () =>
                {
                    _resizeObserver.observe(InnerElement);
                    UpdateNarrow(InnerElement.getBoundingClientRect().As<DOMRect>().width);
                });
                DomObserver.WhenRemoved(InnerElement, () =>
                {
                    _resizeObserver.disconnect();
                    CloseFlyout();
                    EndDrag();
                });
            }

            Flush();
            return InnerElement;
        }

        private void UpdateNarrow(double width)
        {
            if (width <= 0) return;

            // A little hysteresis, so a panel resting on the threshold does not flicker between the two.
            var narrow = _collapseBelow > 0 && (_narrow ? width < _collapseBelow + 24 : width < _collapseBelow);
            if (narrow == _narrow) return;

            _narrow = narrow;
            InnerElement.classList.toggle("tss-dockpanel-narrow", narrow);
            CloseFlyout();
            Invalidate();
        }

        private DockPanel Invalidate()
        {
            _dirty = true;

            if (_flushQueued || !_rendered) return this;

            _flushQueued = true;

            window.requestAnimationFrame(_ =>
            {
                _flushQueued = false;
                Flush();
            });

            return this;
        }

        // A change the user made, or asked for through a command: rebuilt now, so the next gesture sees it, and reported.
        private DockPanel Changed()
        {
            _dirty = true;
            if (_rendered) Flush();
            _onLayoutChanged?.Invoke(GetLayout());
            return this;
        }

        private void Flush()
        {
            if (!_dirty) return;

            _dirty = false;
            Build();
        }

        private static string GroupKey(DockPane p) => p.Edge + "|" + p.TabGroup;

        private bool IsOnRail(DockPane p) => p.IsOpen && (!p.IsPinned || _narrow);

        private DockPane ActiveIn(DockEdge edge, string group)
        {
            var members = _panes.Where(p => p.Edge == edge && p.TabGroup == group && p.IsOpen && !IsOnRail(p)).ToList();
            if (members.Count == 0) return null;

            if (_activeTab.TryGetValue(edge + "|" + group, out var id))
            {
                var active = members.FirstOrDefault(p => p.Id == id);
                if (active is object) return active;
            }

            return members[0];
        }

        private void Build()
        {
            // Pane bodies are moved, not rebuilt; a detached element forgets its scroll position, so carry it over.
            var scrolls = _panes.Where(p => p.Body.isConnected).Select(p => (p, p.Body.scrollTop, p.Body.scrollLeft)).ToList();

            ClearChildren(_main);
            _slots.Clear();

            _main.appendChild(BuildLevel(0));

            foreach (var rail in _rails) BuildRail(rail.Key, rail.Value);

            var closed = _panes.Where(p => !p.IsOpen).ToList();
            _restoreButton.style.display = _restoreMenu && closed.Count > 0 ? "" : "none";

            if (_flyoutPane is object)
            {
                if (IsOnRail(_flyoutPane)) OpenFlyout(_flyoutPane, raise: false);
                else CloseFlyout();
            }

            foreach (var (pane, top, left) in scrolls)
            {
                pane.Body.scrollTop  = top;
                pane.Body.scrollLeft = left;
            }
        }

        private HTMLElement BuildLevel(int level)
        {
            if (level >= _edgePriority.Length) return _documentHost;

            var edge  = _edgePriority[level];
            var inner = BuildLevel(level + 1);
            var panes = _panes.Where(p => p.Edge == edge && p.IsOpen && !IsOnRail(p)).ToList();

            if (panes.Count == 0) return inner;

            var horizontal = edge == DockEdge.Left || edge == DockEdge.Right;
            var split      = Div(Att("tss-dockpanel-split " + (horizontal ? "tss-dockpanel-split-h" : "tss-dockpanel-split-v")));
            var region     = BuildRegion(edge, panes);
            var splitter   = Div(Att("tss-dockpanel-splitter " + (horizontal ? "tss-dockpanel-splitter-v" : "tss-dockpanel-splitter-h"), role: "separator"));
            splitter.setAttribute("aria-orientation", horizontal ? "vertical" : "horizontal");

            // The edge is as deep as its first pane says; a splitter drag writes the new size to every pane on it.
            var size = panes[0].Size;
            region.style.flexBasis = size + "px";
            inner.classList.add("tss-dockpanel-fill");

            HookRegionSplitter(splitter, split, region, edge, panes);

            if (edge == DockEdge.Left || edge == DockEdge.Top)
            {
                split.appendChild(region);
                split.appendChild(splitter);
                split.appendChild(inner);
            }
            else
            {
                split.appendChild(inner);
                split.appendChild(splitter);
                split.appendChild(region);
            }

            return split;
        }

        private HTMLElement BuildRegion(DockEdge edge, List<DockPane> panes)
        {
            var horizontal = edge == DockEdge.Left || edge == DockEdge.Right;
            var region     = Div(Att("tss-dockpanel-region tss-dockpanel-region-" + edge.ToString().ToLower()));

            // One slot per pane, or per tab group, in the order their first pane appears.
            var slots = new List<Slot>();
            foreach (var pane in panes)
            {
                if (pane.TabGroup is object)
                {
                    var existing = slots.FirstOrDefault(s => s.Group == pane.TabGroup);
                    if (existing is object)
                    {
                        existing.Panes.Add(pane);
                        continue;
                    }
                }

                slots.Add(new Slot { Edge = edge, Group = pane.TabGroup, Panes = new List<DockPane> { pane } });
            }

            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                slot.Active  = slot.Group is object ? ActiveIn(edge, slot.Group) : slot.Panes[0];
                slot.Element = BuildSlot(slot);
                slot.Element.style.flexGrow = slot.Panes[0].Weight.ToString();
                _slots.Add(slot);

                if (i > 0)
                {
                    var splitter = Div(Att("tss-dockpanel-splitter " + (horizontal ? "tss-dockpanel-splitter-h" : "tss-dockpanel-splitter-v"), role: "separator"));
                    splitter.setAttribute("aria-orientation", horizontal ? "horizontal" : "vertical");
                    HookSlotSplitter(splitter, slots[i - 1], slot, horizontal);
                    region.appendChild(splitter);
                }

                region.appendChild(slot.Element);
            }

            return region;
        }

        private HTMLElement BuildSlot(Slot slot)
        {
            var el     = Div(Att("tss-dockpanel-slot"));
            var header = Div(Att("tss-dockpanel-header"));
            var active = slot.Active;

            if (slot.Panes.Count > 1)
            {
                var tabs = Div(Att("tss-dockpanel-tabs", role: "tablist"));

                foreach (var pane in slot.Panes)
                {
                    var p   = pane;
                    var tab = Button(Att("tss-dockpanel-tab", type: "button", role: "tab"));
                    if (p.PaneIcon.HasValue) tab.appendChild(I(p.PaneIcon.Value, cssClass: "tss-dockpanel-icon"));
                    tab.appendChild(Span(Att("tss-dockpanel-title", text: p.Title)));
                    tab.setAttribute("aria-selected", p == active ? "true" : "false");
                    tab.tabIndex = p == active ? 0 : -1;
                    if (p == active) tab.classList.add("tss-dockpanel-tab-active");

                    tab.addEventListener("click", _ =>
                    {
                        if (p == ActiveIn(slot.Edge, slot.Group)) return;
                        _activeTab[GroupKey(p)] = p.Id;
                        Changed();
                    });
                    tab.addEventListener("keydown", e => OnTabKey(e.As<KeyboardEvent>(), slot, p));
                    HookHeaderDrag(tab, p);
                    tabs.appendChild(tab);
                }

                header.appendChild(tabs);
                header.classList.add("tss-dockpanel-header-tabs");
            }
            else
            {
                var title = Div(Att("tss-dockpanel-caption"));
                if (active.PaneIcon.HasValue) title.appendChild(I(active.PaneIcon.Value, cssClass: "tss-dockpanel-icon"));
                title.appendChild(Span(Att("tss-dockpanel-title", text: active.Title)));
                header.appendChild(title);
                HookHeaderDrag(header, active);
            }

            header.appendChild(Div(Att("tss-dockpanel-header-spacer")));
            AppendPaneButtons(header, active, inFlyout: false);

            if (active.CanMove) header.classList.add("tss-dockpanel-movable");

            var body = Div(Att("tss-dockpanel-slot-body", role: slot.Panes.Count > 1 ? "tabpanel" : "region"));
            body.setAttribute("aria-label", active.Title);
            body.appendChild(active.Body);

            el.appendChild(header);
            el.appendChild(body);
            return el;
        }

        private void OnTabKey(KeyboardEvent e, Slot slot, DockPane pane)
        {
            var index = slot.Panes.IndexOf(pane);
            var next  = -1;

            if (e.key == "ArrowRight") next = (index + 1) % slot.Panes.Count;
            if (e.key == "ArrowLeft")  next = (index - 1 + slot.Panes.Count) % slot.Panes.Count;
            if (next < 0) return;

            e.preventDefault();
            var target = slot.Panes[next];
            _activeTab[GroupKey(target)] = target.Id;
            Changed();

            // the tab strip was rebuilt; put the focus on the newly selected tab
            var newSlot = _slots.FirstOrDefault(s => s.Panes.Contains(target));
            newSlot?.Element.querySelector(".tss-dockpanel-tab-active")?.As<HTMLElement>().focus();
        }

        private void AppendPaneButtons(HTMLElement header, DockPane pane, bool inFlyout)
        {
            if (pane.CanPin && !(inFlyout && _narrow))
            {
                var name = inFlyout ? _labels.Pin : _labels.AutoHide;
                var pin  = HeaderButton(UIcons.Thumbtack, name, inFlyout ? "tss-dockpanel-pin tss-dockpanel-pin-off" : "tss-dockpanel-pin");
                pin.addEventListener("click", _ =>
                {
                    pane.IsPinned = inFlyout;
                    CloseFlyout();
                    Changed();
                });
                header.appendChild(pin);
            }

            if (pane.CanClose)
            {
                var close = HeaderButton(UIcons.CrossSmall, _labels.Close, "tss-dockpanel-close");
                close.addEventListener("click", _ => RequestClose(pane));
                header.appendChild(close);
            }
        }

        private HTMLElement HeaderButton(UIcons icon, string name, string cssClass)
        {
            var button = Button(Att("tss-dockpanel-button " + cssClass, type: "button"), I(icon));
            Raw(button).Tooltip(name);
            button.setAttribute("aria-label", name);
            return button;
        }

        private void RequestClose(DockPane pane)
        {
            if (_onCloseRequested is object && !_onCloseRequested(pane)) return;

            pane.IsOpen = false;
            if (_flyoutPane == pane) CloseFlyout();
            Changed();
        }

        private void ShowRestoreMenu()
        {
            var closed = _panes.Where(p => !p.IsOpen).ToList();
            if (closed.Count == 0) return;

            var items = new List<ContextMenu.Item> { ContextMenuItem(_labels.Reopen).Header() };

            foreach (var pane in closed)
            {
                var p    = pane;
                var item = p.PaneIcon.HasValue
                    ? ContextMenuItem(HStack().AlignItemsCenter().Gap(8.px()).Children(UI.Icon(p.PaneIcon.Value), TextBlock(p.Title)))
                    : ContextMenuItem(p.Title);
                items.Add(item.OnClick(() => Open(p.Id)));
            }

            var rect = _restoreButton.getBoundingClientRect().As<DOMRect>();
            ContextMenu().Items(items.ToArray()).ShowAt((int)(rect.right - 200), (int)rect.bottom + 4, 200);
        }

        // ----------------------------------------------------------------- rails

        private void BuildRail(DockEdge edge, HTMLElement rail)
        {
            ClearChildren(rail);
            var panes = _panes.Where(p => p.Edge == edge && IsOnRail(p)).ToList();
            rail.classList.toggle("tss-dockpanel-rail-empty", panes.Count == 0);

            foreach (var pane in panes)
            {
                var p   = pane;
                var tab = Button(Att("tss-dockpanel-rail-tab", type: "button"));
                if (p.PaneIcon.HasValue) tab.appendChild(I(p.PaneIcon.Value, cssClass: "tss-dockpanel-icon"));
                tab.appendChild(Span(Att("tss-dockpanel-title", text: p.Title)));
                tab.setAttribute("aria-expanded", _flyoutPane == p ? "true" : "false");
                if (_flyoutPane == p) tab.classList.add("tss-dockpanel-rail-tab-open");
                tab.addEventListener("click", e =>
                {
                    e.stopPropagation();
                    if (_flyoutPane == p) CloseFlyout();
                    else OpenFlyout(p);
                });
                rail.appendChild(tab);
            }
        }

        // --------------------------------------------------------------- flyout

        private void OpenFlyout(DockPane pane, bool raise = true)
        {
            var reopen = _flyoutPane == pane && _flyout is object;
            CloseFlyout(keepPane: true);

            _flyoutPane = pane;
            var horizontal = pane.IsHorizontal;
            var mainRect   = _main.getBoundingClientRect().As<DOMRect>();
            var extent     = horizontal ? mainRect.width : mainRect.height;
            var size       = Math.Max(Math.Min(pane.MinSize, extent), Math.Min(pane.Size, extent * 0.85));

            var flyout = Div(Att("tss-dockpanel-flyout tss-dockpanel-flyout-" + pane.Edge.ToString().ToLower(), role: "dialog"));
            flyout.setAttribute("aria-label", pane.Title);
            if (horizontal) flyout.style.width = size + "px";
            else flyout.style.height = size + "px";
            if (reopen) flyout.classList.add("tss-dockpanel-flyout-shown");

            var slot = new Slot { Edge = pane.Edge, Panes = new List<DockPane> { pane }, Active = pane };
            var el   = BuildFlyoutSlot(slot);
            flyout.appendChild(el);

            var splitter = Div(Att("tss-dockpanel-splitter tss-dockpanel-flyout-splitter " + (horizontal ? "tss-dockpanel-splitter-v" : "tss-dockpanel-splitter-h"), role: "separator"));
            HookFlyoutSplitter(splitter, flyout, pane);
            flyout.appendChild(splitter);

            _flyout = flyout;
            _main.appendChild(flyout);

            // Slide in on the next frame, so the transition has a starting point.
            window.requestAnimationFrame(_ => flyout.classList.add("tss-dockpanel-flyout-shown"));

            _onFlyoutOutside = e =>
            {
                var target = e.target.As<Node>();
                if (_flyout is null || _flyout.contains(target)) return;
                if (_rails.Values.Any(r => r.contains(target))) return;
                if (target.As<HTMLElement>()?.closest(".tss-contextmenu, [data-tippy-root], .tss-layer") is object) return;
                CloseFlyout();
            };
            document.addEventListener("pointerdown", _onFlyoutOutside, true);
            document.addEventListener("keydown",     _onFlyoutKey,     true);

            foreach (var rail in _rails) BuildRail(rail.Key, rail.Value);

            if (raise) _onFlyoutOpened?.Invoke(pane);
        }

        private HTMLElement BuildFlyoutSlot(Slot slot)
        {
            var pane   = slot.Active;
            var el     = Div(Att("tss-dockpanel-slot"));
            var header = Div(Att("tss-dockpanel-header"));
            var title  = Div(Att("tss-dockpanel-caption"));
            if (pane.PaneIcon.HasValue) title.appendChild(I(pane.PaneIcon.Value, cssClass: "tss-dockpanel-icon"));
            title.appendChild(Span(Att("tss-dockpanel-title", text: pane.Title)));
            header.appendChild(title);
            header.appendChild(Div(Att("tss-dockpanel-header-spacer")));
            AppendPaneButtons(header, pane, inFlyout: true);

            var body = Div(Att("tss-dockpanel-slot-body", role: "region"));
            body.setAttribute("aria-label", pane.Title);
            body.appendChild(pane.Body);

            el.appendChild(header);
            el.appendChild(body);
            return el;
        }

        private void OnFlyoutKey(Event e)
        {
            if (e.As<KeyboardEvent>().key != "Escape" || _flyout is null) return;
            CloseFlyout();
        }

        private void CloseFlyout(bool keepPane = false)
        {
            if (_onFlyoutOutside is object)
            {
                document.removeEventListener("pointerdown", _onFlyoutOutside, true);
                document.removeEventListener("keydown",     _onFlyoutKey,     true);
                _onFlyoutOutside = null;
            }

            if (_flyout is object)
            {
                var flyout = _flyout;
                var pane   = _flyoutPane;
                _flyout = null;

                // Keep the pane body out of the closing flyout, so a rebuild that docks the pane finds it free.
                if (pane is object && flyout.contains(pane.Body)) pane.Body.remove();
                flyout.remove();
            }

            if (!keepPane)
            {
                _flyoutPane = null;
                foreach (var rail in _rails) BuildRail(rail.Key, rail.Value);
            }
        }

        // ------------------------------------------------------------- splitters

        private void HookRegionSplitter(HTMLElement splitter, HTMLElement split, HTMLElement region, DockEdge edge, List<DockPane> panes)
        {
            var min = panes.Max(p => p.MinSize);
            var max = panes.Min(p => p.MaxSize);
            double size = 0;

            HookDrag(splitter, (x, y) =>
            {
                var rect   = split.getBoundingClientRect().As<DOMRect>();
                var extent = edge == DockEdge.Left || edge == DockEdge.Right ? rect.width : rect.height;
                var raw    = edge == DockEdge.Left ? x - rect.left
                           : edge == DockEdge.Right ? rect.right - x
                           : edge == DockEdge.Top ? y - rect.top
                           : rect.bottom - y;

                size = Math.Max(min, Math.Min(Math.Min(max, extent - 80), raw));
                region.style.flexBasis = size + "px";
            }, () =>
            {
                if (size <= 0) return;
                foreach (var p in panes) p.Size = size;
                _onLayoutChanged?.Invoke(GetLayout());
            });
        }

        private void HookSlotSplitter(HTMLElement splitter, Slot before, Slot after, bool vertical)
        {
            double startA = 0, startB = 0, total = 0, origin = 0, ratio = -1;

            HookDrag(splitter, (x, y) =>
            {
                var a = before.Element.getBoundingClientRect().As<DOMRect>();
                var b = after.Element.getBoundingClientRect().As<DOMRect>();

                if (ratio < 0)
                {
                    startA = vertical ? a.height : a.width;
                    startB = vertical ? b.height : b.width;
                    origin = vertical ? a.top : a.left;
                    total  = before.Panes[0].Weight + after.Panes[0].Weight;
                }

                var sizeA = Math.Max(48, Math.Min(startA + startB - 48, (vertical ? y : x) - origin));
                ratio = sizeA / (startA + startB);

                before.Element.style.flexGrow = (total * ratio).ToString();
                after.Element.style.flexGrow  = (total * (1 - ratio)).ToString();
            }, () =>
            {
                if (ratio < 0) return;
                foreach (var p in before.Panes) p.Weight = total * ratio;
                foreach (var p in after.Panes)  p.Weight = total * (1 - ratio);
                ratio = -1;
                _onLayoutChanged?.Invoke(GetLayout());
            });
        }

        private void HookFlyoutSplitter(HTMLElement splitter, HTMLElement flyout, DockPane pane)
        {
            double size = 0;

            HookDrag(splitter, (x, y) =>
            {
                var rect = _main.getBoundingClientRect().As<DOMRect>();
                var raw  = pane.Edge == DockEdge.Left ? x - rect.left
                         : pane.Edge == DockEdge.Right ? rect.right - x
                         : pane.Edge == DockEdge.Top ? y - rect.top
                         : rect.bottom - y;
                var extent = pane.IsHorizontal ? rect.width : rect.height;

                size = Math.Max(pane.MinSize, Math.Min(Math.Min(pane.MaxSize, extent * 0.9), raw));
                if (pane.IsHorizontal) flyout.style.width = size + "px";
                else flyout.style.height = size + "px";
            }, () =>
            {
                if (size <= 0) return;
                pane.Size = size;
                _onLayoutChanged?.Invoke(GetLayout());
            });
        }

        // A pointer drag on a splitter: the move callback gets client coordinates, the end callback runs on release.
        private static void HookDrag(HTMLElement handle, Action<double, double> onMove, Action onEnd)
        {
            handle.addEventListener("pointerdown", ev =>
            {
                var e = ev.As<MouseEvent>();
                if (e.button != 0) return;
                ev.preventDefault();
                ev.stopPropagation();

                handle.classList.add("tss-dockpanel-splitter-active");
                document.body.classList.add("tss-dockpanel-resizing");

                Action<Event> move = null;
                Action<Event> up   = null;

                move = me =>
                {
                    var m = me.As<MouseEvent>();
                    onMove(m.clientX, m.clientY);
                };

                up = _ =>
                {
                    window.removeEventListener("pointermove",   move);
                    window.removeEventListener("pointerup",     up);
                    window.removeEventListener("pointercancel", up);
                    handle.classList.remove("tss-dockpanel-splitter-active");
                    document.body.classList.remove("tss-dockpanel-resizing");
                    onEnd();
                };

                window.addEventListener("pointermove",   move);
                window.addEventListener("pointerup",     up);
                window.addEventListener("pointercancel", up);
            });
        }

        // -------------------------------------------------------- drag to re-dock

        private void HookHeaderDrag(HTMLElement handle, DockPane pane)
        {
            if (!pane.CanMove) return;

            handle.addEventListener("pointerdown", ev =>
            {
                var e = ev.As<MouseEvent>();
                if (e.button != 0 || _dragPane is object) return;
                if (e.target.As<HTMLElement>().closest(".tss-dockpanel-button") is object) return;

                _dragPane   = pane;
                _dragStartX = e.clientX;
                _dragStartY = e.clientY;
                _dragActive = false;

                _onDragMove = me => OnDragMove(me.As<MouseEvent>());
                _onDragUp   = ue => OnDragUp(ue.type == "pointercancel");
                _onDragKey  = ke =>
                {
                    if (ke.As<KeyboardEvent>().key != "Escape") return;
                    ke.preventDefault();
                    ke.stopPropagation();
                    EndDrag();
                };

                window.addEventListener("pointermove",   _onDragMove);
                window.addEventListener("pointerup",     _onDragUp);
                window.addEventListener("pointercancel", _onDragUp);
                window.addEventListener("keydown",       _onDragKey, true);
            });
        }

        private void OnDragMove(MouseEvent e)
        {
            if (_dragPane is null) return;

            if (!_dragActive)
            {
                var dx = e.clientX - _dragStartX;
                var dy = e.clientY - _dragStartY;
                if (Math.Sqrt(dx * dx + dy * dy) < DragThreshold) return;
                BeginDrag();
            }

            e.preventDefault();

            _dragGhost.style.left = (e.clientX + 12) + "px";
            _dragGhost.style.top  = (e.clientY + 12) + "px";

            UpdateCompass(e.clientX, e.clientY);

            _hoverGuide = _guides.FirstOrDefault(g => Contains(g.Element.getBoundingClientRect().As<DOMRect>(), e.clientX, e.clientY));
            foreach (var g in _guides) g.Element.classList.toggle("tss-dockpanel-guide-hover", g == _hoverGuide);
            ShowPreview(_hoverGuide);
        }

        private static bool Contains(DOMRect r, double x, double y) => x >= r.left && x <= r.right && y >= r.top && y <= r.bottom;

        private void BeginDrag()
        {
            _dragActive = true;
            CloseFlyout();
            InnerElement.classList.add("tss-dockpanel-dragging");

            _dragGhost = Div(Att("tss-dockpanel-drag-ghost"));
            if (_dragPane.PaneIcon.HasValue) _dragGhost.appendChild(I(_dragPane.PaneIcon.Value, cssClass: "tss-dockpanel-icon"));
            _dragGhost.appendChild(Span(Att(text: _dragPane.Title)));
            document.body.appendChild(_dragGhost);

            _guideLayer = Div(Att("tss-dockpanel-guides"));
            _preview    = Div(Att("tss-dockpanel-preview"));
            _guideLayer.appendChild(_preview);
            _main.appendChild(_guideLayer);

            _guides.Clear();
            _compassSlot = null;

            // An edge marker in the middle of each border the pane may move to.
            foreach (var edge in _dragPane.AllowedEdges)
            {
                var guide = MakeGuide(DropKind.Edge, edge, null, "tss-dockpanel-guide-edge tss-dockpanel-guide-" + edge.ToString().ToLower());
                _guideLayer.appendChild(guide.Element);
            }

            HideTooltips();
        }

        private Guide MakeGuide(DropKind kind, DockEdge edge, Slot target, string cssClass)
        {
            var el = Div(Att("tss-dockpanel-guide " + cssClass));
            el.setAttribute("aria-hidden", "true");
            el.appendChild(Div(Att("tss-dockpanel-guide-fill")));
            var guide = new Guide { Kind = kind, Edge = edge, Target = target, Element = el };
            _guides.Add(guide);
            return guide;
        }

        // The compass over the pane slot under the pointer: the middle joins it as a tab, the two arrows along the
        // slot's edge put the pane before or after it.
        private void UpdateCompass(double x, double y)
        {
            var slot = _slots.FirstOrDefault(s => s.Element.isConnected && Contains(s.Element.getBoundingClientRect().As<DOMRect>(), x, y));

            // A slot that is only the dragged pane has nowhere to put it.
            if (slot is object && slot.Panes.Count == 1 && slot.Panes[0] == _dragPane) slot = null;
            if (slot is object && !_dragPane.AllowedEdges.Contains(slot.Edge)) slot = null;

            if (slot == _compassSlot) return;

            // keep the compass while the pointer is on one of its own buttons, which can overhang the slot
            if (slot is null && _compassSlot is object && _guides.Any(g => g.Target == _compassSlot && Contains(g.Element.getBoundingClientRect().As<DOMRect>(), x, y))) return;

            foreach (var g in _guides.Where(g => g.Target is object).ToList())
            {
                g.Element.remove();
                _guides.Remove(g);
            }

            _compassSlot = slot;
            if (slot is null) return;

            var mainRect = _main.getBoundingClientRect().As<DOMRect>();
            var rect     = slot.Element.getBoundingClientRect().As<DOMRect>();
            var cx       = rect.left + rect.width / 2 - mainRect.left;
            var cy       = rect.top + rect.height / 2 - mainRect.top;
            var vertical = slot.Edge == DockEdge.Left || slot.Edge == DockEdge.Right;

            void Place(Guide g, double dx, double dy)
            {
                g.Element.style.left = (cx + dx - GuideSize / 2) + "px";
                g.Element.style.top  = (cy + dy - GuideSize / 2) + "px";
                _guideLayer.appendChild(g.Element);
            }

            Place(MakeGuide(DropKind.Tab, slot.Edge, slot, "tss-dockpanel-guide-compass tss-dockpanel-guide-tab"), 0, 0);

            if (vertical)
            {
                Place(MakeGuide(DropKind.Before, slot.Edge, slot, "tss-dockpanel-guide-compass tss-dockpanel-guide-top"),    0, -GuideSize - 4);
                Place(MakeGuide(DropKind.After,  slot.Edge, slot, "tss-dockpanel-guide-compass tss-dockpanel-guide-bottom"), 0,  GuideSize + 4);
            }
            else
            {
                Place(MakeGuide(DropKind.Before, slot.Edge, slot, "tss-dockpanel-guide-compass tss-dockpanel-guide-left"),  -GuideSize - 4, 0);
                Place(MakeGuide(DropKind.After,  slot.Edge, slot, "tss-dockpanel-guide-compass tss-dockpanel-guide-right"),  GuideSize + 4, 0);
            }
        }

        private void ShowPreview(Guide guide)
        {
            if (guide is null)
            {
                _preview.style.display = "none";
                return;
            }

            var main = _main.getBoundingClientRect().As<DOMRect>();
            double left, top, width, height;

            if (guide.Kind == DropKind.Edge)
            {
                var horizontal = guide.Edge == DockEdge.Left || guide.Edge == DockEdge.Right;
                var size       = Math.Min(_dragPane.Size, (horizontal ? main.width : main.height) * 0.4);

                left   = guide.Edge == DockEdge.Right  ? main.width - size : 0;
                top    = guide.Edge == DockEdge.Bottom ? main.height - size : 0;
                width  = horizontal ? size : main.width;
                height = horizontal ? main.height : size;
            }
            else
            {
                var rect     = guide.Target.Element.getBoundingClientRect().As<DOMRect>();
                var vertical = guide.Target.Edge == DockEdge.Left || guide.Target.Edge == DockEdge.Right;

                left   = rect.left - main.left;
                top    = rect.top - main.top;
                width  = rect.width;
                height = rect.height;

                if (guide.Kind == DropKind.Before || guide.Kind == DropKind.After)
                {
                    if (vertical) height /= 2;
                    else width /= 2;

                    if (guide.Kind == DropKind.After)
                    {
                        if (vertical) top += height;
                        else left += width;
                    }
                }
            }

            _preview.style.display = "block";
            _preview.style.left    = left + "px";
            _preview.style.top     = top + "px";
            _preview.style.width   = width + "px";
            _preview.style.height  = height + "px";
        }

        private void OnDragUp(bool cancelled)
        {
            var pane   = _dragPane;
            var guide  = _hoverGuide;
            var active = _dragActive;
            EndDrag();

            if (!active || cancelled || guide is null || pane is null) return;

            Drop(pane, guide);
        }

        private void Drop(DockPane pane, Guide guide)
        {
            switch (guide.Kind)
            {
                case DropKind.Edge:
                {
                    pane.Edge     = guide.Edge;
                    pane.TabGroup = null;
                    pane.IsPinned = true;
                    _panes.Remove(pane);
                    _panes.Add(pane);
                    break;
                }
                case DropKind.Tab:
                {
                    var target = guide.Target;
                    var group  = target.Group;

                    if (group is null)
                    {
                        group = "tabs-" + target.Panes[0].Id;
                        target.Panes[0].TabGroup = group;
                    }

                    pane.Edge     = target.Edge;
                    pane.TabGroup = group;
                    pane.IsPinned = true;

                    _panes.Remove(pane);
                    var last = _panes.FindLastIndex(p => p.Edge == target.Edge && p.TabGroup == group);
                    _panes.Insert(last + 1, pane);
                    _activeTab[GroupKey(pane)] = pane.Id;
                    break;
                }
                default:
                {
                    var target = guide.Target;
                    pane.Edge     = target.Edge;
                    pane.TabGroup = null;
                    pane.IsPinned = true;

                    _panes.Remove(pane);

                    var members = target.Panes.Where(p => p != pane).ToList();
                    if (members.Count == 0) members = target.Panes;

                    var index = guide.Kind == DropKind.Before
                        ? _panes.IndexOf(members[0])
                        : _panes.IndexOf(members[members.Count - 1]) + 1;

                    _panes.Insert(Math.Max(0, Math.Min(_panes.Count, index)), pane);
                    break;
                }
            }

            Changed();
        }

        private void EndDrag()
        {
            if (_onDragMove is object)
            {
                window.removeEventListener("pointermove",   _onDragMove);
                window.removeEventListener("pointerup",     _onDragUp);
                window.removeEventListener("pointercancel", _onDragUp);
                window.removeEventListener("keydown",       _onDragKey, true);
                _onDragMove = null;
                _onDragUp   = null;
                _onDragKey  = null;
            }

            _dragPane    = null;
            _dragActive  = false;
            _hoverGuide  = null;
            _compassSlot = null;
            _guides.Clear();

            _dragGhost?.remove();
            _guideLayer?.remove();
            _dragGhost  = null;
            _guideLayer = null;
            _preview    = null;

            InnerElement.classList.remove("tss-dockpanel-dragging");
        }

        private static void HideTooltips() => Script.Write("try { document.querySelectorAll('[data-tippy-root]').forEach(function(t){ if (t._tippy) t._tippy.hide(); }); } catch (e) { }");
    }
}
