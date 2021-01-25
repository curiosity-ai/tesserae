using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class SplitView : IComponent
    {
        private readonly HTMLElement InnerElement;
        private readonly Raw         LeftComponent;
        private readonly Raw         SplitterComponent;
        private readonly Raw         RightComponent;
        private bool?                leftIsSmaller;

        public SplitView(UnitSize splitterSize = null)
        {
            LeftComponent     = Raw(Div(_()));
            SplitterComponent = Raw(Div(_("tss-splitter")));
            RightComponent    = Raw(Div(_()));
            SplitterComponent.Width = (splitterSize is object && splitterSize.Unit != Unit.Auto && splitterSize.Unit != Unit.Inherit)
                                          ? splitterSize.ToString()
                                          : "8px";
            LeftComponent.Height     = "100%";
            SplitterComponent.Height = "100%";
            RightComponent.Height    = "100%";

            LeftComponent.Width   = "10px";
            RightComponent.Width = "10px";
            
            LeftComponent.FlexGrow = "1";
            RightComponent.FlexGrow = "1";

            InnerElement = Div(_("tss-splitview"), LeftComponent.Render(), SplitterComponent.Render(), RightComponent.Render());
        }

        public SplitView Left(IComponent component, string background = "")
        {
            LeftComponent.Content(component);

            LeftComponent.Background = background;
            return this;
        }

        public SplitView Right(IComponent component, string background = "")
        {
            RightComponent.Content(component);

            RightComponent.Background = background;
            return this;
        }

        public SplitView Splitter(IComponent component)
        {
            SplitterComponent.Content(component);
            return this;
        }

        public SplitView NoSplitter()
        {
            SplitterComponent.Width = "0px";
            return this;
        }

        public SplitView SplitInMiddle()
        {
            RightComponent.Width = "50%";
            RightComponent.MaxWidth = "";
            RightComponent.FlexGrow = "";
            LeftComponent.Width = "50%";
            LeftComponent.MaxWidth = "";
            LeftComponent.FlexGrow = "";
            leftIsSmaller = null;
            return this;
        }

        public SplitView Close()
        {
            if (leftIsSmaller.HasValue)
            {
                if (leftIsSmaller.Value)
                {
                    LeftComponent.Collapse();
                }
                else
                {
                    RightComponent.Collapse();
                }
            }
            else
            {
                throw new InvalidOperationException("Set one side as smaller first, to make this SplitView closable.");
            }


            return this;
        }

        public SplitView Open()
        {
            if (leftIsSmaller.HasValue)
            {
                if (leftIsSmaller.Value)
                {
                    LeftComponent.Show();
                }
                else
                {
                    RightComponent.Show();
                }
            }
            else
            {
                throw new InvalidOperationException("Set one side as smaller first, to make this SplitView closable.");
            }

            return this;
        }

        public SplitView LeftIsSmaller(UnitSize leftSize, UnitSize maxLeftSize = null)
        {
            LeftComponent.Width    = leftSize.ToString();
            LeftComponent.MaxWidth = maxLeftSize?.ToString() ?? "";
            LeftComponent.FlexGrow = "";

            RightComponent.Width = "10px";
            RightComponent.MaxWidth = "";
            RightComponent.FlexGrow = "1";
            leftIsSmaller = true;

            return this;
        }

        public SplitView RightIsSmaller(UnitSize rightSize, UnitSize maxRightSize = null)
        {
            RightComponent.Width = rightSize.ToString();
            RightComponent.MaxWidth = maxRightSize?.ToString() ?? "";
            RightComponent.FlexGrow = "";

            LeftComponent.Width = "10px";
            LeftComponent.MaxWidth = "";
            LeftComponent.FlexGrow = "1";
            leftIsSmaller = false;

            return this;
        }

        public HTMLElement Render()
        {
            return InnerElement;
        }
    }

    public enum SizeMode
    {
        Pixels,
        Percent
    }
}
