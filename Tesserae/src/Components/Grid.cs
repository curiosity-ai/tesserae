using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Grid")]
    public class Grid : IContainer<Grid, IComponent>, IHasBackgroundColor, IHasMarginPadding, ISpecialCaseStyling
    {
        private readonly HTMLElement _grid;

        public string Background { get => _grid.style.background; set => _grid.style.background = value; }
        public string Margin     { get => _grid.style.margin;     set => _grid.style.margin = value; }
        public string Padding    { get => _grid.style.padding;    set => _grid.style.padding = value; }

        public HTMLElement StylingContainer => _grid;

        public bool PropagateToStackItemParent { get; private set; } = true;

        public Grid(params UnitSize[] columns)
        {
            _grid = Div(_("tss-grid").WithRole("grid"));
            JustifyContent(ItemJustify.Start);

            if (columns is object && columns.Any(c => c is object))
            {
                _grid.style.gridTemplateColumns = string.Join(" ", columns.Where(c => c is object).Select(c => c.ToString()));
            }
            else
            {
                _grid.style.gridTemplateColumns = "100%";
            }
        }

        public Grid(UnitSize[] columns, UnitSize[] rows)
        {
            _grid = Div(_("tss-grid").WithRole("grid"));
            JustifyContent(ItemJustify.Start);

            if (columns is object && columns.Any(c => c is object))
            {
                _grid.style.gridTemplateColumns = string.Join(" ", columns.Where(c => c is object).Select(c => c.ToString()));
            }
            else
            {
                _grid.style.gridTemplateColumns = "100%";
            }

            if (rows is object && rows.Any(c => c is object))
            {
                _grid.style.gridTemplateRows = string.Join(" ", rows.Where(c => c is object).Select(c => c.ToString()));
            }
            else
            {
                _grid.style.gridTemplateRows = "100%";
            }
        }

        public Grid Columns(params UnitSize[] columns)
        {
            if (columns is object && columns.Any(c => c is object))
            {
                _grid.style.gridTemplateColumns = string.Join(" ", columns.Where(c => c is object).Select(c => c.ToString()));
            }
            else
            {
                _grid.style.gridTemplateColumns = "100%";
            }
            return this;
        }

        public Grid Rows(UnitSize[] rows)
        {
            if (rows is object && rows.Any(c => c is object))
            {
                _grid.style.gridTemplateRows = string.Join(" ", rows.Where(c => c is object).Select(c => c.ToString()));
            }
            else
            {
                _grid.style.gridTemplateRows = "100%";
            }
            return this;
        }

        public void Add(IComponent component)
        {
            _grid.appendChild(GetItem(component, true));
        }

        internal static HTMLElement GetItem(IComponent component, bool forceAdd = false)
        {
            HTMLElement item = null;

            if (component.HasOwnProperty("GridItem"))
            {
                item = component["GridItem"].As<HTMLElement>();
            }

            if (item is null)
            {
                var rendered = component.Render();

                if (forceAdd || (rendered.parentElement is object && rendered.parentElement.classList.contains("tss-stack")))
                {
                    item = Div(_("tss-stack-item", styles: s =>
                    {
                        s.alignSelf = "auto";
                        s.width = "auto";
                        s.height = "auto";
                        s.flexShrink = "1";
                    }), component.Render());

                    component["GridItem"] = item;

                    if (forceAdd)
                    {
                        CopyStylesDefinedWithExtension(rendered, item);
                    }
                }
                else
                {
                    item = rendered;
                }
            }
            return item;
        }

        internal static void CopyStylesDefinedWithExtension(HTMLElement from, HTMLElement to)
        {
            //Copy base-styles using same method from Stack
            Stack.CopyStylesDefinedWithExtension(from, to);

            var fs = from.style;
            var ts = to.style;

            bool has(string att)
            {
                bool ha = from.hasAttribute(att);

                if (ha)
                {
                    from.removeAttribute(att);
                }
                return ha;
            }

            if (has("tss-grd-c"))
            {
                ts.gridColumn = fs.gridColumn;
                fs.gridColumn = "";
            }

            if (has("tss-grd-r"))
            {
                ts.gridRow = fs.gridRow;
                fs.gridRow = "";
            }
        }

        /// <summary>
        /// Needs to be called before the component is added to the Grid.
        /// </summary>
        public static void SetGridColumn(IComponent component, int start, int end)
        {
            var (item, rememberAndPropagate) = GetCorrectItemToApplyStyle(component);
            item.style.gridColumn = $"{start} / {end}";

            if (rememberAndPropagate)
            {
                item.setAttribute("tss-grd-c", "");
                if (component.HasOwnProperty("GridItem"))
                {
                    component["GridItem"].As<HTMLElement>().style.gridColumn = item.style.gridColumn;
                }
            }
        }

        /// <summary>
        /// Needs to be called before the component is added to the Grid.
        /// </summary>
        public static void SetGridRow(IComponent component, int start, int end)
        {
            var (item, rememberAndPropagate) = GetCorrectItemToApplyStyle(component);
            item.style.gridRow = $"{start} / {end}";
            if (rememberAndPropagate)
            {
                item.setAttribute("tss-grd-r", "");

                if (component.HasOwnProperty("GridItem"))
                {
                    component["GridItem"].As<HTMLElement>().style.gridRow = item.style.gridRow;
                }
            }
        }

        internal static (HTMLElement item, bool remember) GetCorrectItemToApplyStyle(IComponent component)
        {
            if (component is ISpecialCaseStyling specialCase)
            {
                return (specialCase.StylingContainer, specialCase.PropagateToStackItemParent);
            }
            else
            {
                return (GetItem(component), true);
            }
        }

        /// <summary>
        /// Sets the align-items css property for this stack
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public Grid AlignItems(ItemAlign align)
        {
            _grid.style.alignItems = align.ToString();
            return this;
        }

        /// <summary>
        /// Sets the align-items css property for this stack
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public Grid AlignContent(ItemAlign align)
        {
            _grid.style.alignContent = align.ToString();
            return this;
        }

        /// <summary>
        /// Sets the justify-content css property for this stack
        /// </summary>
        /// <param name="justify"></param>
        /// <returns></returns>
        public Grid JustifyContent(ItemJustify justify)
        {
            _grid.style.justifyContent = justify.ToString();
            return this;
        }

        /// <summary>
        /// Sets the justify-content css property for this stack
        /// </summary>
        /// <param name="justify"></param>
        /// <returns></returns>
        public Grid JustifyItems(ItemJustify justify)
        {
            _grid.style.justifyItems = justify.ToString();
            return this;
        }

        /// <summary>
        /// Make this grid relative (i.e. position:relative)
        /// </summary>
        /// <returns></returns>
        public Grid Relative()
        {
            _grid.classList.add("tss-relative");
            return this;
        }

        /// <summary>
        /// Sets the align-items css property for this grid to 'center'
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public Grid AlignItemsCenter() => AlignItems(ItemAlign.Center);

        public Grid OverflowHidden()
        {
            _grid.style.overflow = "hidden";
            return this;
        }
        public Grid Gap(UnitSize gapSize)
        {
            _grid.style.gap = gapSize.ToString();
            return this;
        }

        public Grid RowGap(UnitSize gapSize)
        {
            _grid.style.rowGap = gapSize.ToString();
            return this;
        }

        public Grid ColumnGap(UnitSize gapSize)
        {
            _grid.style.columnGap = gapSize.ToString();
            return this;
        }

        public Grid FlowColumn()
        {
            _grid.style.gridAutoFlow = "column";
            return this;
        }

        public virtual void Clear()
        {
            ClearChildren(_grid);
        }

        public Grid RemovePropagation()
        {
            PropagateToStackItemParent = false;
            return this;
        }

        public Grid NoDefaultMargin()
        {
            _grid.classList.add("tss-default-component-no-margin");
            return this;
        }

        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            _grid.replaceChild(GetItem(newComponent), GetItem(oldComponent));
        }

        public void Remove(IComponent component)
        {
            _grid.removeChild(GetItem(component));
        }

        public virtual HTMLElement Render() => _grid;
    }
}