using Transpose;
using Transpose.Core;
using System;
using System.Collections.Generic;
using System.Text;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A floating positioning container used to overlay a small piece of content anchored to one corner of its
    /// parent.
    /// </summary>
    [Transpose.Name("tss.Float")]
    public sealed class Float : IComponent
    {
        private readonly IComponent  _child;
        private readonly HTMLElement _floatingContainer;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Float(IComponent child, Position position)
        {
            _child             = child;
            _floatingContainer = Div(Att($"tss-float {position}"), _child.Render());
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public dom.HTMLElement Render() => _floatingContainer;


        [Enum(Emit.StringName)] //Don't change the emit type without updating the FromClassList method
        public enum Position
        {
            [Name("tss-float-topleft")]      TopLeft,
            [Name("tss-float-topmiddle")]    TopMiddle,
            [Name("tss-float-topright")]     TopRight,
            [Name("tss-float-leftcenter")]   LeftCenter,
            [Name("tss-float-center")]       Center,
            [Name("tss-float-rightcenter")]  RightCenter,
            [Name("tss-float-bottomleft")]   BottomLeft,
            [Name("tss-float-bottonmiddle")] BottonMiddle,
            [Name("tss-float-bottomright")]  BottomRight
        }
    }
}