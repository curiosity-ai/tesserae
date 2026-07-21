using System;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A CSS-Grid container with explicit row and column tracks, gap and alignment, for two-dimensional layouts.
    /// </summary>
    [Transpose.Name("tss.Grid")]
    public class Grid : IContainer<Grid, IComponent>, IHasBackgroundColor, IHasMarginPadding, ISpecialCaseStyling
    {
        private readonly HTMLElement _grid;

        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public string Background { get => _grid.style.background; set => _grid.style.background = value; }
        /// <summary>
        /// Gets or sets the CSS margin of the component.
        /// </summary>
        public string Margin     { get => _grid.style.margin;     set => _grid.style.margin = value; }
        /// <summary>
        /// Gets or sets the CSS padding of the component.
        /// </summary>
        public string Padding    { get => _grid.style.padding;    set => _grid.style.padding = value; }

        /// <summary>
        /// Gets or sets the styling container.
        /// </summary>
        public HTMLElement StylingContainer => _grid;

        /// <summary>
        /// Gets or sets the propagate to stack item parent.
        /// </summary>
        public bool PropagateToStackItemParent { get; private set; } = true;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Grid(params UnitSize[] columns)
        {
            _grid = Div(Att("tss-grid").WithRole("grid"));
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

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Grid(UnitSize[] columns, UnitSize[] rows)
        {
            _grid = Div(Att("tss-grid").WithRole("grid"));
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

        /// <summary>
        /// Defines the columns of the grid (track sizes).
        /// </summary>
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

        /// <summary>
        /// Defines the rows of the grid (track sizes).
        /// </summary>
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

        /// <summary>
        /// Adds the given item to the component.
        /// </summary>
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
                    item = Div(Att("tss-stack-item", styles: s =>
                    {
                        s.alignSelf  = "auto";
                        s.width      = "auto";
                        s.height     = "auto";
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
            item.style.gridColumn            = $"{start} / {end}";

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
            item.style.gridRow               = $"{start} / {end}";

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

        /// <summary>
        /// Hides any content that overflows the component's bounds.
        /// </summary>
        public Grid OverflowHidden()
        {
            _grid.style.overflow = "hidden";
            return this;
        }
        /// <summary>
        /// Sets the gap between rows and columns of the grid.
        /// </summary>
        public Grid Gap(UnitSize gapSize)
        {
            _grid.style.gap = gapSize.ToString();
            return this;
        }

        /// <summary>
        /// Sets the gap between rows of the grid.
        /// </summary>
        public Grid RowGap(UnitSize gapSize)
        {
            _grid.style.rowGap = gapSize.ToString();
            return this;
        }

        /// <summary>
        /// Sets the gap between columns of the grid.
        /// </summary>
        public Grid ColumnGap(UnitSize gapSize)
        {
            _grid.style.columnGap = gapSize.ToString();
            return this;
        }

        /// <summary>
        /// Sets the auto-generated row size of the grid.
        /// </summary>
        public Grid AutoRows(UnitSize autoRowValue)
        {
            _grid.style.gridAutoRows = autoRowValue.ToString();
            return this;
        }

        /// <summary>
        /// Sets the auto-generated column size of the grid.
        /// </summary>
        public Grid AutoColumn(UnitSize autoColumnValue)
        {
            _grid.style.gridAutoColumns = autoColumnValue.ToString();
            return this;
        }

        /// <summary>
        /// Switches the grid to column flow.
        /// </summary>
        public Grid FlowColumn()
        {
            _grid.style.gridAutoFlow = "column";
            return this;
        }

        /// <summary>
        /// Clears the component's current state.
        /// </summary>
        public virtual void Clear()
        {
            ClearChildren(_grid);
        }

        /// <summary>
        /// Removes the given propagation from the component.
        /// </summary>
        public Grid RemovePropagation()
        {
            PropagateToStackItemParent = false;
            return this;
        }

        /// <summary>
        /// Removes / disables the default margin on the component.
        /// </summary>
        public Grid NoDefaultMargin()
        {
            _grid.classList.add("tss-default-component-no-margin");
            return this;
        }

        /// <summary>
        /// Replaces an existing item with a new one.
        /// </summary>
        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            _grid.replaceChild(GetItem(newComponent), GetItem(oldComponent));
        }

        /// <summary>
        /// Removes the given item from the component.
        /// </summary>
        public void Remove(IComponent component)
        {
            TryRemoveChild(_grid, GetItem(component));
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public virtual HTMLElement Render() => _grid;
    }
}