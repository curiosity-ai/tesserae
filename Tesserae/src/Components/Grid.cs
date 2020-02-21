using System.Linq;
using System.Collections.Generic;
using Retyped;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Grid : IContainer<Grid, IComponent>, IHasBackgroundColor, IHasMarginPadding
    {
        public HTMLElement InnerElement { get; private set; }
        public string Background { get => InnerElement.style.background; set => InnerElement.style.background = value; }
        public string Margin { get => InnerElement.style.margin; set => InnerElement.style.margin = value; }
        public string Padding { get => InnerElement.style.padding; set => InnerElement.style.padding = value; }


        public Grid(params UnitSize[] columns)
        {
            InnerElement = Div(_("tss-grid").WithRole("grid"));
            if(columns is object && columns.Any(c => c is object))
            {
                InnerElement.style.gridTemplateColumns = string.Join(" ", columns.Where(c => c is object).Select(c => c.ToString()));
            }
        }

        public void Add(IComponent component)
        {
            InnerElement.appendChild(Stack.GetItem(component, true));
        }

        public virtual void Clear()
        {
            ClearChildren(InnerElement);
        }
        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            InnerElement.replaceChild(Stack.GetItem(newComponent), Stack.GetItem(oldComponent));
        }

        public virtual HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
