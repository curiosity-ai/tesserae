using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// How a <see cref="DetailsGrid"/> arranges each label against its value.
    /// </summary>
    [Transpose.Name("tss.DetailsGridMode")]
    public enum DetailsGridMode
    {
        /// <summary>
        /// Label and value side by side, one row each, the labels sharing a column of a fixed width
        /// (see <see cref="DetailsGrid.LabelWidth(UnitSize)"/>) so the values line up.
        /// </summary>
        Rows,

        /// <summary>
        /// The label above its value: a small, semibold, uppercase header in the secondary color with the
        /// value under it - the shape a sheet of dates and references takes when the labels are longer than
        /// a column would like and the values are short.
        /// </summary>
        Stacked
    }

    /// <summary>
    /// A bordered table of label/value rows - the "Owner / Size / Modified / Pages" block a preview shows
    /// beside (or under) whatever it is previewing.
    /// <para>
    /// Rows are added with <see cref="Row(string, string)"/> or <see cref="Row(string, IComponent)"/>, and
    /// read as one column of labels in the secondary color followed by one column of values. Values can be
    /// plain text or a component of the host's own - a <see cref="Link"/>, a <see cref="Badge"/>, an
    /// <see cref="Avatar"/> - so a grid of metadata never forces the host to spell its values out as strings.
    /// </para>
    /// <para>
    /// <see cref="Stacked"/> turns that around: each label becomes a small semibold header over its value,
    /// which - with <see cref="Columns(int)"/> - is the two- or three-up sheet of dates and references a
    /// record's header block is made of.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.DetailsGrid")]
    public sealed class DetailsGrid : ComponentBase<DetailsGrid, HTMLElement>
    {
        private readonly List<HTMLElement> _rows = new List<HTMLElement>();

        /// <summary>
        /// Initializes a new instance of this class, with no rows in it yet, laid out in the given number of
        /// columns - one by default, which is the "one row each" block. Two or three suit a grid of short
        /// values that would otherwise leave most of its width empty.
        /// </summary>
        public DetailsGrid(int columns = 1)
        {
            InnerElement = Div(Att("tss-detailsgrid", role: "table"));

            Columns(columns);
        }

        /// <summary>
        /// Gets how many rows the grid has.
        /// </summary>
        /// <summary>The number of rows the grid is showing. A row a self-removing value took with it
        /// (see <see cref="InlineLabel"/>) is no longer counted.</summary>
        public int Count => _rows.Count(row => row.parentElement is object);

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;

        /// <summary>
        /// Adds a row showing the given label and its value as plain text. A null or empty value still gets
        /// its row, drawn as an em dash, so a grid of the same fields reads the same whether every one of
        /// them is known or not - pass <c>skipIfEmpty</c> to leave the row out instead.
        /// </summary>
        public DetailsGrid Row(string label, string value, bool skipIfEmpty = false)
        {
            var isEmpty = string.IsNullOrWhiteSpace(value);

            if (isEmpty && skipIfEmpty) return this;

            return AddRow(label, Span(Att("tss-detailsgrid-value-text", text: isEmpty ? "—" : value, title: isEmpty ? null : value)));
        }

        /// <summary>
        /// Adds a row showing the given label and a component of the host's own as its value. A null value
        /// leaves the row out, so a caller can build the component conditionally without branching around
        /// the call.
        /// </summary>
        public DetailsGrid Row(string label, IComponent value)
        {
            if (value is null) return this;

            return AddRow(label, value.Render());
        }

        /// <summary>
        /// Takes every row away again.
        /// </summary>
        public DetailsGrid Clear()
        {
            ClearChildren(InnerElement);
            _rows.Clear();

            return this;
        }

        /// <summary>
        /// Sets how wide the label column is - 120px by default, which is enough for the one or two words a
        /// field name usually is. The width is the same for every row, so the values line up.
        /// </summary>
        public DetailsGrid LabelWidth(UnitSize width)
        {
            InnerElement.style.setProperty("--tss-detailsgrid-label-width", width is object ? width.ToString() : "120px");

            return this;
        }

        /// <summary>
        /// Lays the rows out in the given number of columns rather than one under the other - two or three
        /// of them, for a grid of short values that would otherwise leave most of its width empty.
        /// </summary>
        public DetailsGrid Columns(int columns)
        {
            var count = columns < 1 ? 1 : columns;

            InnerElement.style.setProperty("--tss-detailsgrid-columns", $"{count}");

            // Past one column the rules between rows can't be drawn honestly - the last row of the grid is
            // several children, not the last child - so a multi-column grid separates its rows by space.
            InnerElement.UpdateClassIf(count > 1, "tss-detailsgrid-multicolumn");

            return this;
        }

        /// <summary>
        /// Chooses how each label sits against its value: <see cref="DetailsGridMode.Rows"/> (the default)
        /// puts them side by side, <see cref="DetailsGridMode.Stacked"/> puts the label above its value as a
        /// small semibold header in the secondary color.
        /// </summary>
        public DetailsGrid Mode(DetailsGridMode mode)
        {
            InnerElement.UpdateClassIf(mode == DetailsGridMode.Stacked, "tss-detailsgrid-stacked");

            return this;
        }

        /// <summary>
        /// Puts each label above its value as a small, semibold, uppercase header in the secondary color -
        /// the sheet of dates and references a record's header block is made of. Pair it with
        /// <see cref="Columns(int)"/> to read two or three of them across.
        /// </summary>
        public DetailsGrid Stacked(bool value = true) => Mode(value ? DetailsGridMode.Stacked : DetailsGridMode.Rows);

        /// <summary>
        /// Tightens the rows, for a grid that has to sit inside something small.
        /// </summary>
        public DetailsGrid Compact(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-detailsgrid-compact");

            return this;
        }

        /// <summary>
        /// Drops the border around the grid and the rules between its rows, leaving the labels and values
        /// alone - for a grid that already sits inside something bordered.
        /// </summary>
        public DetailsGrid NoBorder(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-detailsgrid-borderless");

            return this;
        }

        private DetailsGrid AddRow(string label, HTMLElement value)
        {
            var labelCell = Div(Att("tss-detailsgrid-label", role: "rowheader", text: label ?? string.Empty));
            var valueCell = Div(Att("tss-detailsgrid-value", role: "cell"), value);
            var row       = Div(Att("tss-detailsgrid-row", role: "row"), labelCell, valueCell);

            InnerElement.appendChild(row);
            _rows.Add(row);

            return this;
        }
    }
}
