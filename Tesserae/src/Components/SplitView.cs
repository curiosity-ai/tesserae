using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
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

        public SplitView Left(IComponent component, string background = "")
        {
            _leftComponent.Content(component);

            if (!string.IsNullOrEmpty(background))
            {
                _leftComponent.Background = background;
            }

            return this;
        }

        public SplitView Right(IComponent component, string background = "")
        {
            _rightComponent.Content(component);

            if (!string.IsNullOrEmpty(background))
            {
                _rightComponent.Background = background;
            }

            return this;
        }

        public SplitView PanelStyle()
        {
            _splitContainer.classList.add("tss-splitview-panel-style");
            return this;
        }

        public SplitView NotResizable()
        {
            _splitterComponent.Class("tss-no-splitter");
            _splitterComponent.Width = "";
            return this;
        }

        public SplitView SplitInMiddle()
        {
            _rightComponent.MaxWidth = "";
            _leftComponent.MaxWidth  = "";
            _splitContainer.classList.remove("tss-split-left");
            _splitContainer.classList.remove("tss-split-right");
            return this;
        }

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

        public HTMLElement Render()
        {
            return _splitContainer;
        }
    }
}
