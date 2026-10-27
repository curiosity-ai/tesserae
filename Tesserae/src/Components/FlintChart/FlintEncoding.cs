using Transpose;

namespace Tesserae
{
    /// <summary>
    /// How one field is drawn on one channel of a <see cref="FlintChart"/>, for when a field name alone does not say
    /// enough (<see cref="FlintChart.Encode(FlintChannel, FlintEncoding)"/>). Every property is optional: what is left
    /// unset is decided by Flint from the field's semantic type and the theme.
    /// </summary>
    [Transpose.Name("tss.FlintEncoding")]
    public sealed class FlintEncoding
    {
        /// <param name="field">The field of the data the channel draws.</param>
        public FlintEncoding(string field) => Field = field;

        /// <summary>The field of the data the channel draws.</summary>
        public string Field { get; set; }

        /// <summary>The encoding type, in place of the one the semantic type implies.</summary>
        public FlintEncodingType? Type { get; set; }

        /// <summary>How the rows behind one mark are summed.</summary>
        public FlintAggregate? Aggregate { get; set; }

        /// <summary>Sorts the channel's categories by their value, ascending (<c>true</c>) or descending (<c>false</c>).</summary>
        public bool? SortAscending { get; set; }

        /// <summary>The field the channel's categories are sorted by, with <see cref="SortAscending"/>.</summary>
        public string SortBy { get; set; }

        /// <summary>Colour channel: a Vega scheme name (<c>viridis</c>, <c>redblue</c>), in place of the theme's ramp.</summary>
        public string Scheme { get; set; }

        /// <summary>Colour channel: the stops a ramp or palette runs through, low end first, as <c>#rrggbb</c>.</summary>
        public string[] SchemeColors { get; set; }

        /// <summary>Colour channel: runs the scale the other way round.</summary>
        public bool Reverse { get; set; }

        /// <summary>The name the channel's axis or key prints, in place of the field's; an empty string prints none.</summary>
        public string Title { get; set; }

        /// <summary>The encoding as Flint reads it.</summary>
        internal object ToSpec()
        {
            var spec = Script.Write<object>("{ field: {0} }", Field);

            if (Type.HasValue)          Script.Write("{0}.type = {1};", spec, FlintNames.Of(Type.Value));
            if (Aggregate.HasValue)     Script.Write("{0}.aggregate = {1};", spec, FlintNames.Of(Aggregate.Value));
            if (SortAscending.HasValue) Script.Write("{0}.sortOrder = {1} ? 'ascending' : 'descending';", spec, SortAscending.Value);
            if (SortBy != null)         Script.Write("{0}.sortBy = {1};", spec, SortBy);
            if (SchemeColors != null)   Script.Write("{0}.scheme = {1}.slice();", spec, SchemeColors);
            else if (Scheme != null)    Script.Write("{0}.scheme = {1};", spec, Scheme);
            if (Reverse)                Script.Write("{0}.reverse = true;", spec);
            if (Title != null)          Script.Write("{0}.title = {1} === '' ? null : {1};", spec, Title);

            return spec;
        }
    }
}
