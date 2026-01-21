using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A vertical split view component.
    /// </summary>
    [H5.Name("tss.SplitView")]
    public class SplitView : IComponent
    {
        private readonly HTMLElement _splitContainer;
        private readonly string      _splitterSize;
        private readonly Raw         _leftComponent;
        private readonly Raw         _splitterComponent;
        private readonly Raw         _rightComponent;
        private          bool        _resizable;
        private          Action<int> _onResizeEnd;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitView"/> class.
        /// </summary>
        /// <param name="splitterSize">The size of the splitter.</param>
        public SplitView(UnitSize splitterSize = null)
        {
            _splitterSize = (splitterSize is object && splitterSize.Unit != Unit.Auto && splitterSize.Unit != Unit.Inherit)
                ? splitterSize.ToString()
                : "8px";
            _leftComponent = Raw(Div(_()));
            var splitter = Div(_("tss-splitter tss-no-splitter"));
            splitter.draggable        = false;
            _splitterComponent        = Raw(splitter);
            _rightComponent           = Raw(Div(_()));
            _splitterComponent.Width  = _splitterSize;
            _leftComponent.Height     = "100%";
            _splitterComponent.Height = "100%";
            _rightComponent.Height    = "100%";

            _leftComponent.Width  = "10px";
            _rightComponent.Width = "10px";

            _leftComponent.FlexGrow  = "1";
            _rightComponent.FlexGrow = "1";

            _splitContainer = Div(_("tss-splitview tss-splitview-vertical"), _leftComponent.Render(), _splitterComponent.Render(), _rightComponent.Render());
        }

        /// <summary>
        /// Sets the split view to be resizable.
        /// </summary>
        /// <param name="onResizeEnd">An optional action to perform when resizing ends.</param>
        /// <returns>The current instance.</returns>
        public SplitView Resizable(Action<int> onResizeEnd = null)
        {
            _resizable   = true;
            _onResizeEnd = onResizeEnd;
            _splitterComponent.RemoveClass("tss-no-splitter");
            _splitterComponent.Width = _splitterSize;
            HookDragEvents(_splitterComponent);
            return this;
        }

        private void HookDragEvents(IComponent dragArea)
        {
            var el = dragArea.Render();

            double      width = 0;
            DOMRect     rect;
            HTMLElement current;

            el.onmousedown += (me) =>
            {
                if (_splitContainer.classList.contains("tss-split-right"))
                {
                    current = _rightComponent.Render();
                }
                else
                {
                    current = _leftComponent.Render();
                }
                rect               =  _splitContainer.getBoundingClientRect().As<DOMRect>();
                window.onmousemove += Resize;
                window.onmouseup   += StopResize;
                StopEvent(me);
            };

            void Resize(MouseEvent me)
            {
                width                    = Math.Min(rect.width - 16, Math.Max(16, (me.clientX - rect.left)));
                current.style.width      = width + "px";
                current.style.flexGrow   = "0";
                current.style.flexShrink = "1";
                StopEvent(me);
            }

            void StopResize(MouseEvent me)
            {
                window.onmousemove -= Resize;
                window.onmouseup   -= StopResize;
                _onResizeEnd?.Invoke((int)width);
                rect = null;
                StopEvent(me);
            }
        }

        /// <summary>
        /// Sets the left component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="background">The background color.</param>
        /// <returns>The current instance.</returns>
        public SplitView Left(IComponent component, string background = "")
        {
            _leftComponent.Content(component);

            if (!string.IsNullOrEmpty(background))
            {
                _leftComponent.Background = background;
            }

            return this;
        }

        /// <summary>
        /// Sets the right component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="background">The background color.</param>
        /// <returns>The current instance.</returns>
        public SplitView Right(IComponent component, string background = "")
        {
            _rightComponent.Content(component);

            if (!string.IsNullOrEmpty(background))
            {
                _rightComponent.Background = background;
            }

            return this;
        }

        /// <summary>
        /// Sets the panel style for the split view.
        /// </summary>
        /// <returns>The current instance.</returns>
        public SplitView PanelStyle()
        {
            _splitContainer.classList.add("tss-splitview-panel-style");
            return this;
        }

        /// <summary>
        /// Sets the split view to be not resizable.
        /// </summary>
        /// <returns>The current instance.</returns>
        public SplitView NotResizable()
        {
            _splitterComponent.Class("tss-no-splitter");
            _splitterComponent.Width = "";
            return this;
        }

        /// <summary>
        /// Splits the view in the middle.
        /// </summary>
        /// <returns>The current instance.</returns>
        public SplitView SplitInMiddle()
        {
            _rightComponent.MaxWidth = "";
            _leftComponent.MaxWidth  = "";
            _splitContainer.classList.remove("tss-split-left");
            _splitContainer.classList.remove("tss-split-right");
            return this;
        }

        /// <summary>
        /// Closes the split view.
        /// </summary>
        /// <returns>The current instance.</returns>
        public SplitView Close()
        {
            if (_splitContainer.classList.contains("tss-split-left"))
            {
                _leftComponent.Collapse();
            }
            else if (_splitContainer.classList.contains("tss-split-right"))
            {
                _rightComponent.Collapse();
            }
            else
            {
                throw new Exception("Only valid for left or right splits");
            }

            return this;
        }

        /// <summary>
        /// Opens the split view.
        /// </summary>
        /// <returns>The current instance.</returns>
        public SplitView Open()
        {
            if (_splitContainer.classList.contains("tss-split-left"))
            {
                _leftComponent.Show();
            }
            else if (_splitContainer.classList.contains("tss-split-right"))
            {
                _rightComponent.Show();
            }
            else
            {
                throw new Exception("Only valid for left or right splits");
            }

            return this;
        }

        /// <summary>
        /// Sets the left component to be smaller.
        /// </summary>
        /// <param name="leftSize">The left size.</param>
        /// <param name="maxLeftSize">The maximum left size.</param>
        /// <param name="minLeftSize">The minimum left size.</param>
        /// <returns>The current instance.</returns>
        public SplitView LeftIsSmaller(UnitSize leftSize, UnitSize maxLeftSize = null, UnitSize minLeftSize = null)
        {
            _leftComponent.Width      = leftSize.ToString();
            _leftComponent.MinWidth   = minLeftSize?.ToString() ?? "";
            _leftComponent.MaxWidth   = maxLeftSize?.ToString() ?? "";
            _leftComponent.FlexGrow   = "";
            _leftComponent.FlexShrink = "";

            bool fullLeft = _leftComponent.Width == "100%";
            _rightComponent.Width      = fullLeft ? "0px" :"1px";
            _rightComponent.FlexGrow   = fullLeft ? "0" : "1";
            _rightComponent.MaxWidth   = "";
            _rightComponent.FlexShrink = "";
            _splitContainer.classList.add("tss-split-left");
            _splitContainer.classList.remove("tss-split-right");

            return this;
        }

        /// <summary>
        /// Sets the right component to be smaller.
        /// </summary>
        /// <param name="rightSize">The right size.</param>
        /// <param name="maxRightSize">The maximum right size.</param>
        /// <param name="minRightSize">The minimum right size.</param>
        /// <returns>The current instance.</returns>
        public SplitView RightIsSmaller(UnitSize rightSize, UnitSize maxRightSize = null, UnitSize minRightSize = null)
        {
            _rightComponent.Width      = rightSize.ToString();
            _rightComponent.MinWidth   = minRightSize?.ToString() ?? "";
            _rightComponent.MaxWidth   = maxRightSize?.ToString() ?? "";
            _rightComponent.FlexGrow   = "";
            _rightComponent.FlexShrink = "";

            bool fullRight = _rightComponent.Width == "100%";
            
            _leftComponent.Width      = fullRight ? "0px" : "1px";
            _leftComponent.FlexGrow   = fullRight ? "0" : "1";
            _leftComponent.MaxWidth   = "";
            _leftComponent.FlexShrink = "";
            _splitContainer.classList.add("tss-split-right");
            _splitContainer.classList.remove("tss-split-left");
            return this;
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <returns>The rendered HTML element.</returns>
        public HTMLElement Render()
        {
            return _splitContainer;
        }
    }
}
