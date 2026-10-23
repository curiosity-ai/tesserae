using System.Collections.Generic;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// How the content of a <see cref="DataTable"/> column sits in its cells.
    /// </summary>
    [Enum(Emit.StringName)]
    public enum DataTableAlign
    {
        Start,
        Center,
        End
    }

    /// <summary>
    /// A table whose cells are components: a header band over rows that hold whatever the columns call for -
    /// a label, a <see cref="Sparkline"/>, a <see cref="TrendBadge"/>, a <see cref="TargetBar"/>. A row can
    /// carry a colored accent on its leading edge, to flag its state at a glance.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="DetailsList{T}"/>, which is driven by item objects, the rows here are just the
    /// components you hand it; there is no sorting, selection or paging. It is the right size for a
    /// dashboard panel of a dozen rows.
    /// </remarks>
    [Transpose.Name("tss.DataTable")]
    public sealed class DataTable : ComponentBase<DataTable, HTMLElement>
    {
        private readonly HTMLElement          _header;
        private readonly HTMLElement          _body;
        private readonly List<string>         _widths  = new List<string>();
        private readonly List<DataTableAlign> _aligns  = new List<DataTableAlign>();

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public DataTable()
        {
            _header = Div(Att("tss-datatable-header"));
            _body   = Div(Att("tss-datatable-body"));

            InnerElement = Div(Att("tss-datatable"), _header, _body);
        }

        /// <summary>
        /// Adds a column. Call it for every column before adding rows.
        /// </summary>
        /// <param name="title">The heading of the column, drawn in small capitals in the header band.</param>
        /// <param name="width">The width of the column. Columns without one share what is left equally.</param>
        /// <param name="align">How the heading and the cells of the column are aligned.</param>
        public DataTable Column(string title, UnitSize width = null, DataTableAlign align = DataTableAlign.Start)
        {
            _widths.Add(width is object ? width.ToString() : "minmax(0, 1fr)");
            _aligns.Add(align);

            var cell = Div(Att("tss-datatable-cell tss-datatable-heading", text: title ?? string.Empty));
            ApplyAlign(cell, align);
            _header.appendChild(cell);

            InnerElement.style.setProperty("--tss-datatable-columns", string.Join(" ", _widths));

            return this;
        }

        /// <summary>
        /// Adds a row, one component per column.
        /// </summary>
        public DataTable Row(params IComponent[] cells) => Row(null, cells);

        /// <summary>
        /// Adds a row with a colored accent on its leading edge, one component per column.
        /// </summary>
        /// <param name="accent">Any CSS color, or a theme variable; null leaves the edge bare.</param>
        public DataTable Row(string accent, params IComponent[] cells)
        {
            var row = Div(Att("tss-datatable-row"));

            if (!string.IsNullOrEmpty(accent))
            {
                row.classList.add("tss-datatable-accented");
                row.style.setProperty("--tss-datatable-accent", accent);
            }

            for (int i = 0; i < cells.Length; i++)
            {
                var cell = Div(Att("tss-datatable-cell"));
                ApplyAlign(cell, i < _aligns.Count ? _aligns[i] : DataTableAlign.Start);

                if (cells[i] is object)
                {
                    cell.appendChild(cells[i].Render());
                }

                row.appendChild(cell);
            }

            _body.appendChild(row);

            return this;
        }

        /// <summary>
        /// Removes every row, keeping the columns.
        /// </summary>
        public DataTable Clear()
        {
            while (_body.firstChild != null)
            {
                _body.removeChild(_body.firstChild);
            }

            return this;
        }

        /// <summary>
        /// Tightens the padding of the header and the rows.
        /// </summary>
        public DataTable Compact(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-datatable-compact");

            return this;
        }

        private static void ApplyAlign(HTMLElement cell, DataTableAlign align)
        {
            cell.UpdateClassIf(align == DataTableAlign.Center, "tss-datatable-align-center");
            cell.UpdateClassIf(align == DataTableAlign.End,    "tss-datatable-align-end");
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
