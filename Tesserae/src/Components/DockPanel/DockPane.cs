using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>The side of a <see cref="DockPanel"/>'s document a <see cref="DockPane"/> is docked to.</summary>
    [Transpose.Name("tss.DockEdge")]
    public enum DockEdge
    {
        /// <summary>Docked to the left of the document; its size is a width.</summary>
        Left,

        /// <summary>Docked to the right of the document; its size is a width.</summary>
        Right,

        /// <summary>Docked above the document; its size is a height.</summary>
        Top,

        /// <summary>Docked below the document; its size is a height.</summary>
        Bottom
    }

    /// <summary>
    /// One pane of a <see cref="DockPanel"/>: a titled piece of content docked to an edge of the document, alone or as
    /// a tab of a group. The pane describes where it is (<see cref="Edge"/>, <see cref="TabGroup"/>, <see cref="Size"/>)
    /// and what the user may do with it; the panel reads it on every rebuild, and writes the user's changes back to it.
    /// </summary>
    [Transpose.Name("tss.DockPane")]
    public sealed class DockPane
    {
        internal HTMLElement Body;

        /// <summary>Initializes a new instance of the <see cref="DockPane"/> class.</summary>
        /// <param name="id">A stable identity, used by the layout <see cref="DockPanel.GetLayout"/> saves. Do not derive it from position.</param>
        /// <param name="title">The header, tab and rail text.</param>
        /// <param name="content">The pane's body.</param>
        public DockPane(string id, string title, IComponent content)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            Id      = id;
            Title   = title ?? id;
            Content = content;
            Body    = Div(Att("tss-dockpanel-pane-body"));
            if (content is object) Body.appendChild(content.Render());
        }

        /// <summary>The pane's stable identity.</summary>
        public string     Id           { get; }

        /// <summary>The header, tab and rail text.</summary>
        public string     Title        { get; private set; }

        /// <summary>An icon drawn before the title, or null.</summary>
        public UIcons?    PaneIcon     { get; private set; }

        /// <summary>The pane's body.</summary>
        public IComponent Content      { get; private set; }

        /// <summary>The edge the pane is docked to. Defaults to <see cref="DockEdge.Left"/>.</summary>
        public DockEdge   Edge         { get; internal set; } = DockEdge.Left;

        /// <summary>Panes on the same edge with the same group name share one slot as tabs. Null for a pane of its own.</summary>
        public string     TabGroup     { get; internal set; }

        /// <summary>
        /// The pane's width on the left and right edges, its height on the top and bottom ones, in pixels. The edge
        /// takes the size of the first pane on it; dragging the edge's splitter writes the new size to all of them.
        /// </summary>
        public double     Size         { get; internal set; } = 260;

        /// <summary>The smallest the user can drag the pane to.</summary>
        public double     MinSize      { get; private set; } = 120;

        /// <summary>The largest the user can drag the pane to.</summary>
        public double     MaxSize      { get; private set; } = 720;

        /// <summary>The pane's share of its edge when several panes are stacked there. Defaults to 1.</summary>
        public double     Weight       { get; internal set; } = 1;

        /// <summary>Whether the pane is shown at all.</summary>
        public bool       IsOpen       { get; internal set; } = true;

        /// <summary>Whether the pane takes space beside the document; an unpinned pane sits on its edge's rail and slides out over the document.</summary>
        public bool       IsPinned     { get; internal set; } = true;

        /// <summary>Whether the header offers a close button.</summary>
        public bool       CanClose     { get; private set; } = true;

        /// <summary>Whether the header offers the auto-hide pin.</summary>
        public bool       CanPin       { get; private set; } = true;

        /// <summary>Whether the pane can be dragged by its header to another edge or into a tab group.</summary>
        public bool       CanMove      { get; private set; } = true;

        /// <summary>The edges the user may drag the pane to.</summary>
        public IReadOnlyList<DockEdge> AllowedEdges { get; private set; } = new[] { DockEdge.Left, DockEdge.Right, DockEdge.Top, DockEdge.Bottom };

        /// <summary>Docks the pane to the left of the document.</summary>
        public DockPane Left()   => DockTo(DockEdge.Left);

        /// <summary>Docks the pane to the right of the document.</summary>
        public DockPane Right()  => DockTo(DockEdge.Right);

        /// <summary>Docks the pane above the document.</summary>
        public DockPane Top()    => DockTo(DockEdge.Top);

        /// <summary>Docks the pane below the document.</summary>
        public DockPane Bottom() => DockTo(DockEdge.Bottom);

        /// <summary>Docks the pane to <paramref name="edge"/>.</summary>
        public DockPane DockTo(DockEdge edge)
        {
            Edge = edge;
            return this;
        }

        /// <summary>Makes the pane a tab of <paramref name="group"/>: panes on the same edge with the same name share one slot.</summary>
        public DockPane InTabGroup(string group)
        {
            TabGroup = string.IsNullOrEmpty(group) ? null : group;
            return this;
        }

        /// <summary>Sets the pane's size, in pixels (a width on the left and right edges, a height on the others). Defaults to 260.</summary>
        public DockPane SetSize(double pixels)
        {
            Size = pixels;
            return this;
        }

        /// <summary>Sets how small and how large the user can drag the pane. Defaults to 120 and 720.</summary>
        public DockPane SizeLimits(double min, double max)
        {
            MinSize = Math.Max(24, min);
            MaxSize = Math.Max(MinSize, max);
            return this;
        }

        /// <summary>Sets the pane's share of its edge when several panes are stacked there. Defaults to 1.</summary>
        public DockPane SetWeight(double weight)
        {
            Weight = weight > 0 ? weight : 1;
            return this;
        }

        /// <summary>Sets the icon drawn before the title.</summary>
        public DockPane Icon(UIcons icon)
        {
            PaneIcon = icon;
            return this;
        }

        /// <summary>Starts the pane unpinned: on its edge's rail, sliding out over the document when its tab is clicked.</summary>
        public DockPane Unpinned(bool value = true)
        {
            IsPinned = !value;
            return this;
        }

        /// <summary>Starts the pane closed. The panel's restore menu, or <see cref="DockPanel.Open"/>, brings it back.</summary>
        public DockPane Closed(bool value = true)
        {
            IsOpen = !value;
            return this;
        }

        /// <summary>Removes the close button.</summary>
        public DockPane NotClosable()
        {
            CanClose = false;
            return this;
        }

        /// <summary>Removes the auto-hide pin.</summary>
        public DockPane NotPinnable()
        {
            CanPin = false;
            return this;
        }

        /// <summary>Stops the pane from being dragged to another place.</summary>
        public DockPane NotMovable()
        {
            CanMove = false;
            return this;
        }

        /// <summary>Limits the edges the user may drag the pane to. A pane always may stay where it is.</summary>
        public DockPane AllowEdges(params DockEdge[] edges)
        {
            AllowedEdges = edges is object && edges.Length > 0 ? edges.Distinct().ToArray() : new[] { Edge };
            return this;
        }

        /// <summary>Replaces the pane's title.</summary>
        public DockPane SetTitle(string title)
        {
            Title = title ?? Id;
            return this;
        }

        /// <summary>Replaces the pane's body.</summary>
        public DockPane SetContent(IComponent content)
        {
            Content = content;
            ClearChildren(Body);
            if (content is object) Body.appendChild(content.Render());
            return this;
        }

        internal bool IsHorizontal => Edge == DockEdge.Left || Edge == DockEdge.Right;
    }

    /// <summary>
    /// The text a <see cref="DockPanel"/> draws on its own, so an application can hand in its translations.
    /// </summary>
    [Transpose.Name("tss.DockPanelLabels")]
    public sealed class DockPanelLabels
    {
        /// <summary>The name of the pin on a docked pane, which slides it to the rail.</summary>
        public string AutoHide    { get; set; } = "Auto-hide";

        /// <summary>The name of the pin on a pane slid out from the rail, which docks it again.</summary>
        public string Pin         { get; set; } = "Pin";

        /// <summary>The name of a pane's close button.</summary>
        public string Close       { get; set; } = "Close";

        /// <summary>The name of the button that lists the closed panes.</summary>
        public string ClosedPanes { get; set; } = "Closed panes";

        /// <summary>The accessible name of the pane toggles in the panel's bar.</summary>
        public string Panes       { get; set; } = "Panes";

        /// <summary>The header of the restore menu.</summary>
        public string Reopen      { get; set; } = "Reopen";

        /// <summary>The start of a splitter's accessible name: "Resize Explorer".</summary>
        public string Resize      { get; set; } = "Resize";

        /// <summary>Joins the two panes a splitter between stacked panes resizes: "Resize Explorer and Outline".</summary>
        public string And         { get; set; } = "and";

        /// <summary>The header of the menu that moves a pane from the keyboard.</summary>
        public string MovePane    { get; set; } = "Move pane";

        /// <summary>The menu item that docks the pane to the left edge.</summary>
        public string MoveLeft    { get; set; } = "Move to left";

        /// <summary>The menu item that docks the pane to the right edge.</summary>
        public string MoveRight   { get; set; } = "Move to right";

        /// <summary>The menu item that docks the pane to the top edge.</summary>
        public string MoveTop     { get; set; } = "Move to top";

        /// <summary>The menu item that docks the pane to the bottom edge.</summary>
        public string MoveBottom  { get; set; } = "Move to bottom";

        /// <summary>The start of the menu item that makes the pane a tab beside another: "Tab with Output".</summary>
        public string TabWith     { get; set; } = "Tab with";
    }

    /// <summary>The layout of one pane, as <see cref="DockPanel.GetLayout"/> writes it.</summary>
    [Transpose.ObjectLiteral]
    internal sealed class DockPaneState
    {
        public string id;
        public string edge;
        public double size;
        public double weight;
        public bool   open;
        public bool   pinned;
        public string tab;
        public bool   active;
    }

    /// <summary>A whole <see cref="DockPanel"/> layout, as <see cref="DockPanel.GetLayout"/> writes it.</summary>
    [Transpose.ObjectLiteral]
    internal sealed class DockLayoutState
    {
        public int             version;
        public DockPaneState[] panes;
    }
}
