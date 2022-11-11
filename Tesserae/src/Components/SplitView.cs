using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.SplitView")]
    public class SplitView : IComponent
    {
        private readonly HTMLElement _splitContainer;
        private readonly Raw _leftComponent;
        private readonly Raw _splitterComponent;
        private readonly Raw _rightComponent;
        private bool _resizable;
        private Action<int> _onResizeEnd;

        public SplitView(UnitSize splitterSize = null)
        {
            _leftComponent     = Raw(Div(_()));
            _splitterComponent = Raw(Div(_("tss-splitter tss-no-splitter")));
            _rightComponent    = Raw(Div(_()));
            _splitterComponent.Width = (splitterSize is object && splitterSize.Unit != Unit.Auto && splitterSize.Unit != Unit.Inherit)
                                          ? splitterSize.ToString()
                                          : "8px";
            _leftComponent.Height     = "100%";
            _splitterComponent.Height = "100%";
            _rightComponent.Height    = "100%";

            _leftComponent.Width   = "10px";
            _rightComponent.Width = "10px";
            
            _leftComponent.FlexGrow = "1";
            _rightComponent.FlexGrow = "1";

            _splitContainer = Div(_("tss-splitview"), _leftComponent.Render(), _splitterComponent.Render(), _rightComponent.Render());
        }

        public SplitView Resizable(Action<int> onResizeEnd)
        {
            _resizable = true;
            _onResizeEnd = onResizeEnd;
            _splitterComponent.RemoveClass("tss-no-splitter");
            HookDragEvents(_splitterComponent);
            return this;
        }

        private void HookDragEvents(IComponent dragArea)
        {
            var el = dragArea.Render();

            double width = 0;

            el.onmousedown += (me) =>
            {
                window.onmousemove += Resize;
                window.onmouseup += StopResize;
            };

            void Resize(MouseEvent me)
            {
                HTMLElement current;

                if (_splitContainer.classList.contains("tss-split-right"))
                {
                    current = _rightComponent.Render();
                }
                else
                {
                    current = _leftComponent.Render();
                }


                width = Math.Max(200, (me.clientX - current.offsetLeft));
                current.style.width = width + "px";
            }

            void StopResize(MouseEvent me)
            {
                window.onmousemove -= Resize;
                window.onmouseup -= StopResize;
                _onResizeEnd?.Invoke((int)width);
            }
        }

        public SplitView Left(IComponent component, string background = "")
        {
            _leftComponent.Content(component);
            _leftComponent.Background = background;

            return this;
        }

        public SplitView Right(IComponent component, string background = "")
        {
            _rightComponent.Content(component);
            _rightComponent.Background = background;

            return this;
        }

        public SplitView PanelStyle()
        {
            _splitContainer.classList.add("tss-splitview-panel-style");
            return this;
        }

        public SplitView NoSplitter()
        {
            _splitterComponent.Class("tss-no-splitter");
            return this;
        }

        public SplitView SplitInMiddle()
        {
            _rightComponent.MaxWidth = "";
            _leftComponent.MaxWidth = "";
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
            else if(_splitContainer.classList.contains("tss-split-right"))
            {
                _rightComponent.Show();
            }
            else
            {
                throw new Exception("Only valid for left or right splits");
            }

            return this;
        }

        public SplitView LeftIsSmaller(UnitSize leftSize, UnitSize maxLeftSize = null)
        {
            _leftComponent.Width    = leftSize.ToString();
            _leftComponent.MaxWidth = maxLeftSize?.ToString() ?? "";
            _leftComponent.FlexGrow = "";

            _rightComponent.Width = "10px";
            _rightComponent.MaxWidth = "";
            _rightComponent.FlexGrow = "1";
            _splitContainer.classList.add("tss-split-left");
            _splitContainer.classList.remove("tss-split-right");

            return this;
        }

        public SplitView RightIsSmaller(UnitSize rightSize, UnitSize maxRightSize = null)
        {
            _rightComponent.Width = rightSize.ToString();
            _rightComponent.MaxWidth = maxRightSize?.ToString() ?? "";
            _rightComponent.FlexGrow = "";

            _leftComponent.Width = "10px";
            _leftComponent.MaxWidth = "";
            _leftComponent.FlexGrow = "1";
            _splitContainer.classList.add("tss-split-right");
            _splitContainer.classList.remove("tss-split-left");
            return this;
        }

        public HTMLElement Render()
        {
            return _splitContainer;
        }
    }

    [H5.Name("tss.sz")]
    public enum SizeMode
    {
        Pixels,
        Percent
    }
}
