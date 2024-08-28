using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.HorizontalSplitView")]
    public class HorizontalSplitView : IComponent
    {
        private readonly HTMLElement _splitContainer;
        private readonly string      _splitterSize;
        private readonly Raw         _topComponent;
        private readonly Raw         _splitterComponent;
        private readonly Raw         _bottomComponent;
        private          bool        _resizable;
        private          Action<int> _onResizeEnd;

        public HorizontalSplitView(UnitSize splitterSize = null)
        {
            _splitterSize = (splitterSize is object && splitterSize.Unit != Unit.Auto && splitterSize.Unit != Unit.Inherit)
                ? splitterSize.ToString()
                : "8px";
            _topComponent = Raw(Div(_()));
            var splitter = Div(_("tss-splitter  tss-no-splitter"));
            splitter.draggable        = false;
            _splitterComponent        = Raw(splitter);
            _bottomComponent          = Raw(Div(_()));
            _splitterComponent.Height = _splitterSize;
            _topComponent.Width       = "100%";
            _splitterComponent.Width  = "100%";
            _bottomComponent.Width    = "100%";

            _topComponent.Height    = "10px";
            _bottomComponent.Height = "10px";

            _topComponent.FlexGrow    = "1";
            _bottomComponent.FlexGrow = "1";

            _splitContainer = Div(_("tss-splitview tss-splitview-horizontal"), _topComponent.Render(), _splitterComponent.Render(), _bottomComponent.Render());
        }

        public HorizontalSplitView Resizable(Action<int> onResizeEnd = null)
        {
            _resizable   = true;
            _onResizeEnd = onResizeEnd;
            _splitterComponent.RemoveClass("tss-no-splitter");
            _splitterComponent.Height = _splitterSize;
            HookDragEvents(_splitterComponent);
            return this;
        }

        private void HookDragEvents(IComponent dragArea)
        {
            var el = dragArea.Render();

            double      height = 0;
            DOMRect     rect;
            HTMLElement current;

            el.onmousedown += (me) =>
            {
                if (_splitContainer.classList.contains("tss-split-bottom"))
                {
                    current = _bottomComponent.Render();
                }
                else
                {
                    current = _topComponent.Render();
                }
                rect               =  _splitContainer.getBoundingClientRect().As<DOMRect>();
                window.onmousemove += Resize;
                window.onmouseup   += StopResize;
                StopEvent(me);
            };

            void Resize(MouseEvent me)
            {
                if (_splitContainer.classList.contains("tss-split-bottom"))
                {
                    height = Math.Min(rect.height - 16, Math.Max(16, (rect.bottom - me.clientY)));
                }
                else
                {
                    height = Math.Min(rect.height - 16, Math.Max(16, (me.clientY - rect.top)));
                }
                current.style.height     = height + "px";
                current.style.flexGrow   = "0";
                current.style.flexShrink = "1";
                StopEvent(me);
            }

            void StopResize(MouseEvent me)
            {
                window.onmousemove -= Resize;
                window.onmouseup   -= StopResize;
                _onResizeEnd?.Invoke((int)height);
                rect = null;
                StopEvent(me);
            }
        }

        public HorizontalSplitView Top(IComponent component, string background = "")
        {
            _topComponent.Content(component);

            if (!string.IsNullOrEmpty(background))
            {
                _topComponent.Background = background;
            }

            return this;
        }

        public HorizontalSplitView Bottom(IComponent component, string background = "")
        {
            _bottomComponent.Content(component);

            if (!string.IsNullOrEmpty(background))
            {
                _bottomComponent.Background = background;
            }

            return this;
        }

        public HorizontalSplitView PanelStyle()
        {
            _splitContainer.classList.add("tss-splitview-panel-style");
            return this;
        }

        public HorizontalSplitView NotResizable()
        {
            _splitterComponent.Class("tss-no-splitter");
            _splitterComponent.Height = "";
            return this;
        }

        public HorizontalSplitView SplitInMiddle()
        {
            _bottomComponent.MaxHeight = "";
            _topComponent.MaxHeight    = "";
            _splitContainer.classList.remove("tss-split-top");
            _splitContainer.classList.remove("tss-split-bottom");
            return this;
        }

        public HorizontalSplitView Close()
        {
            if (_splitContainer.classList.contains("tss-split-top"))
            {
                _topComponent.Collapse();
            }
            else if (_splitContainer.classList.contains("tss-split-bottom"))
            {
                _bottomComponent.Collapse();
            }
            else
            {
                throw new Exception("Only valid for left or right splits");
            }

            return this;
        }

        public HorizontalSplitView Open()
        {
            if (_splitContainer.classList.contains("tss-split-top"))
            {
                _topComponent.Show();
            }
            else if (_splitContainer.classList.contains("tss-split-bottom"))
            {
                _bottomComponent.Show();
            }
            else
            {
                throw new Exception("Only valid for left or right splits");
            }

            return this;
        }

        public HorizontalSplitView TopIsSmaller(UnitSize topSize, UnitSize maxTopSize = null)
        {
            _topComponent.Height     = topSize.ToString();
            _topComponent.MaxHeight  = maxTopSize?.ToString() ?? "";
            _topComponent.FlexGrow   = "";
            _topComponent.FlexShrink = "";

            _bottomComponent.Height     = "10px";
            _bottomComponent.MaxHeight  = "";
            _bottomComponent.FlexGrow   = "1";
            _bottomComponent.FlexShrink = "";
            _splitContainer.classList.add("tss-split-top");
            _splitContainer.classList.remove("tss-split-bottom");

            return this;
        }

        public HorizontalSplitView BottomIsSmaller(UnitSize bottomSize, UnitSize maxBottomSize = null)
        {
            _bottomComponent.Height     = bottomSize.ToString();
            _bottomComponent.MaxHeight  = maxBottomSize?.ToString() ?? "";
            _bottomComponent.FlexGrow   = "";
            _bottomComponent.FlexShrink = "";

            _topComponent.Height     = "10px";
            _topComponent.MaxHeight  = "";
            _topComponent.FlexGrow   = "1";
            _topComponent.FlexShrink = "";
            _splitContainer.classList.add("tss-split-bottom");
            _splitContainer.classList.remove("tss-split-top");
            return this;
        }

        public HTMLElement Render()
        {
            return _splitContainer;
        }
    }
}