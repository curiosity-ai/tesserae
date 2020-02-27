using System.Linq;
using System.Collections.Generic;
using Retyped;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Grid : IContainer<Grid, IComponent>, IHasBackgroundColor, IHasMarginPadding, ISpecialCaseStyling
    {
        private HTMLElement InnerElement;

        private HTMLElement _grid;

        public string Background { get => InnerElement.style.background; set => InnerElement.style.background = value; }
        public string Margin { get => InnerElement.style.margin; set => InnerElement.style.margin = value; }
        public string Padding { get => InnerElement.style.padding; set => InnerElement.style.padding = value; }

        public HTMLElement StylingContainer => _grid;

        public bool PropagateToStackItemParent => false;

        public Grid(params UnitSize[] columns)
        {
            _grid = Div(_("tss-grid").WithRole("grid"));
            InnerElement = DIV(_grid);
            JustifyContent(ItemJustify.Start);
            if (columns is object && columns.Any(c => c is object))
            {
                _grid.style.gridTemplateColumns = string.Join(" ", columns.Where(c => c is object).Select(c => c.ToString()));
            }
        }
        public Grid EnableInvisibleScroll()
        {
            ScrollBar.EnableInvisibleScroll(InnerElement);
            return this;
        }

        public void Add(IComponent component)
        {
            _grid.appendChild(Stack.GetItem(component, true));
        }

        /// <summary>
        /// Sets the align-items css property for this stack
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public Grid AlignItems(ItemAlign align)
        {
            string cssAlign = align.ToString().ToLower();
            if (cssAlign == "end" || cssAlign == "start") cssAlign = $"flex-{cssAlign}";
            _grid.style.alignItems = cssAlign;
            return this;
        }

        /// <summary>
        /// Sets the align-items css property for this stack
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public Grid AlignContent(ItemAlign align)
        {
            string cssAlign = align.ToString().ToLower();
            if (cssAlign == "end" || cssAlign == "start") cssAlign = $"flex-{cssAlign}";
            _grid.style.alignContent = cssAlign;
            return this;
        }

        /// <summary>
        /// Sets the justify-content css property for this stack
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public Grid JustifyContent(ItemJustify justify)
        {
            string cssJustify = justify.ToString().ToLower();
            if (cssJustify == "end" || cssJustify == "start") cssJustify = $"flex-{cssJustify}";
            if (cssJustify == "between" || cssJustify == "around" || cssJustify == "evenly") cssJustify = $"space-{cssJustify}";
            _grid.style.justifyContent = cssJustify;
            return this;
        }

        /// <summary>
        /// Sets the justify-content css property for this stack
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public Grid JustifyItems(ItemJustify justify)
        {
            string cssJustify = justify.ToString().ToLower();
            if (cssJustify == "end" || cssJustify == "start") cssJustify = $"flex-{cssJustify}";
            if (cssJustify == "between" || cssJustify == "around" || cssJustify == "evenly") cssJustify = $"space-{cssJustify}";
            _grid.style.justifyItems = cssJustify;
            return this;
        }

        public virtual void Clear()
        {
            ClearChildren(_grid);
        }
        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            _grid.replaceChild(Stack.GetItem(newComponent), Stack.GetItem(oldComponent));
        }

        public virtual HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
