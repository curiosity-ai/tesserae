using H5.Core;
using System;
using System.Collections.Generic;
using System.Text;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public sealed class Float : IComponent
    {
        private readonly IComponent _child;
        private readonly HTMLElement _floatingContainer;

        public Float(IComponent child, Position position)
        {
            _child = child;
            _floatingContainer = Div(_("tss-float tss-float-" + position.ToString().ToLower()), _child.Render());
        }

        public dom.HTMLElement Render() => _floatingContainer;

        public enum Position
        {
            TopLeft,
            TopMiddle,
            TopRight,
            LeftCenter,
            Center,
            RightCenter,
            BottomLeft,
            BottonMiddle,
            BottomRight
        }
    }
}
