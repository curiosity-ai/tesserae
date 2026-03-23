using System;
using System.Collections.Generic;
using H5;
using Tesserae;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.Sortable")]
    public class Sortable
    {
        private          HTMLElement     _element;
        private          object          _sortable;
        private readonly SortableOptions _options;

        public Sortable(HTMLElement element, SortableOptions options)
        {
            _element = element;
            _options = options;

            _sortable = _options is object
                ? Script.Write<object>("new Sortable({0}, {1})", _element, _options)
                : Script.Write<object>("new Sortable({0})",      _element);
        }
    }

    [ObjectLiteral]
    public class SortableEvent
    {

        public HTMLElement item              { get; set; } // dragged HTMLElement
        public HTMLElement to                { get; set; } // target list
        public HTMLElement from              { get; set; } // previous list
        public int         oldIndex          { get; set; } // element's old index within old parent
        public int         newIndex          { get; set; } // element's new index within new parent
        public int         oldDraggableIndex { get; set; } // element's old index within old parent, only counting draggable elements
        public int         newDraggableIndex { get; set; } // element's new index within new parent, only counting draggable elements
        public HTMLElement clone             { get; set; } // the clone element
        public object      pullMode          { get; set; } // when item is in another sortable: `"clone"` if cloning, `true` if moving

        public PullMode GetPullMode()
        {
            switch (pullMode)
            {
                case bool pullModeBool:
                    return pullModeBool
                        ? PullMode.moving
                        : PullMode.none;
                case string pullModeString when pullModeString == "clone": return PullMode.clone;
                default:                                                   return PullMode.none;
            }
        }

        public enum PullMode
        {
            clone,
            moving,
            none,
        }
    }

    [ObjectLiteral]
    public class SortableOptions
    {
        public string                            group                 { get; set; } // = "name"; // or { name: "...", pull: [true, false, 'clone', array], put: [true, false, array] }
        public bool                              sort                  { get; set; } // = true; // sorting inside list
        public int                               delay                 { get; set; } // = 0; // time in milliseconds to define when the sorting should start
        public bool                              delayOnTouchOnly      { get; set; } // = false; // only delay if user is using touch
        public int                               touchStartThreshold   { get; set; } // = 0; // px, how many pixels the point should move before cancelling a delayed drag event
        public bool                              disabled              { get; set; } // = false; // Disables the sortable if set to true.
        public object                            store                 { get; set; } // = null; // @see Store
        public int                               animation             { get; set; } // = 150; // ms, animation speed moving items when sorting, `0` — without animation
        public string                            easing                { get; set; } // = "cubic-bezier(1, 0, 0, 1)"; // Easing for animation. Defaults to null. See https://easings.net/ for examples.
        public string                            handle                { get; set; } // = ".my-handle"; // Drag handle selector within list items
        public string                            filter                { get; set; } // = ".ignore-elements"; // Selectors that do not lead to dragging (String or Function)
        public bool                              preventOnFilter       { get; set; } // = true; // Call `event.preventDefault()` when triggered `filter`
        public string                            draggable             { get; set; } // = ".item"; // Specifies which items inside the element should be draggable
        public string                            dataIdAttr            { get; set; } // = "data-id"; // HTML attribute that is used by the `toArray()` method
        public string                            ghostClass            { get; set; } // = "sortable-ghost"; // Class name for the drop placeholder
        public string                            chosenClass           { get; set; } // = "sortable-chosen"; // Class name for the chosen item
        public string                            dragClass             { get; set; } // = "sortable-drag"; // Class name for the dragging item
        public double                            swapThreshold         { get; set; } // = 1; // Threshold of the swap zone
        public bool                              invertSwap            { get; set; } // = false; // Will always use inverted swap zone if set to true
        public int                               invertedSwapThreshold { get; set; } // = 1; // Threshold of the inverted swap zone (will be set to swapThreshold value by default)
        public string                            direction             { get; set; } // = "horizontal"; // Direction of Sortable (will be detected automatically if not given)
        public bool                              forceFallback         { get; set; } // = false; // ignore the HTML5 DnD behaviour and force the fallback to kick in
        public string                            fallbackClass         { get; set; } // = "sortable-fallback"; // Class name for the cloned DOM Element when using forceFallback
        public bool                              fallbackOnBody        { get; set; } // = false; // Appends the cloned DOM Element into the Document's Body
        public int                               fallbackTolerance     { get; set; } // = 0; // Specify in pixels how far the mouse should move before it's considered as a drag.
        public bool                              dragoverBubble        { get; set; } // = false,
        public bool                              removeCloneOnHide     { get; set; } // = true; // Remove the clone element when it is not showing, rather than just hiding it
        public int                               emptyInsertThreshold  { get; set; } // = 5, // px, distance mouse must be from empty sortable to insert drag element into it
        public Action<SortableEvent>             onChoose              { get; set; }
        public Action<DataTransfer, HTMLElement> setData               { get; set; }
        public Action<SortableEvent>             onUnchoose            { get; set; }
        public Action<SortableEvent>             onStart               { get; set; }
        public Action<SortableEvent>             onEnd                 { get; set; }
        public Action<SortableEvent>             onAdd                 { get; set; }
        public Action<SortableEvent>             onUpdate              { get; set; }
        public Action<SortableEvent>             onSort                { get; set; }
        public Action<SortableEvent>             onRemove              { get; set; }
        public Action<SortableEvent>             onFilter              { get; set; }
        public Action<SortableEvent>             onMove                { get; set; }
        public Action<SortableEvent>             onClone               { get; set; }
        public Action<SortableEvent>             onChange              { get; set; }
    }
}