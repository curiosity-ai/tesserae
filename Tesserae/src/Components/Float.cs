using H5;
using H5.Core;
using System;
using System.Collections.Generic;
using System.Text;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Float")]
    public sealed class Float : IComponent
    {
        private readonly IComponent _child;
        private readonly HTMLElement _floatingContainer;

        public Float(IComponent child, Position position)
        {
            _child = child;
            _floatingContainer = Div(_($"tss-float {position}"), _child.Render());
        }

        public dom.HTMLElement Render() => _floatingContainer;


        [Enum(Emit.StringName)] //Don't change the emit type without updating the FromClassList method
        public enum Position
        {
            [Name("tss-float-topleft")     ] TopLeft,
            [Name("tss-float-topmiddle")   ] TopMiddle,
            [Name("tss-float-topright")    ] TopRight,
            [Name("tss-float-leftcenter")  ] LeftCenter,
            [Name("tss-float-center")      ] Center,
            [Name("tss-float-rightcenter") ] RightCenter,
            [Name("tss-float-bottomleft")  ] BottomLeft,
            [Name("tss-float-bottonmiddle")] BottonMiddle,
            [Name("tss-float-bottomright") ] BottomRight
        }
    }
}
