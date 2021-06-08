using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class SplitView : IComponent
    {
        private readonly HTMLElement _splitContainer;
        private readonly Raw _leftComponent;
        private readonly Raw _splitterComponent;
        private readonly Raw _rightComponent;

        public SplitView(UnitSize splitterSize = null)
        {
            _leftComponent     = Raw(Div(_()));
            _splitterComponent = Raw(Div(_("tss-splitter")));
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

        public SplitView Splitter(IComponent component)
        {
            _splitterComponent.Content(component);
            return this;
        }

        public SplitView PanelStyle()
        {
            _splitContainer.classList.add("tss-splitview-panel-style");
            return this;
        }

        public SplitView NoSplitter()
        {
            _splitterComponent.Width = "0px";
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

    public enum SizeMode
    {
        Pixels,
        Percent
    }
}
