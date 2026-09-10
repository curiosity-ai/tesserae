using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The tone a <see cref="StatefulItemRow"/> is washed in - the same set a <see cref="Banner"/> has, so a row and
    /// the notice explaining it can be told to mean the same thing.
    /// </summary>
    [Transpose.Name("tss.RowTone")]
    public enum RowTone
    {
        /// <summary>No wash: an ordinary row.</summary>
        None,

        /// <summary>The accent tone: the row is selected, or has changed.</summary>
        Primary,

        /// <summary>Something that was added, or went right.</summary>
        Success,

        /// <summary>Something that needs care but hasn't failed.</summary>
        Warning,

        /// <summary>Something that is being taken away, or failed.</summary>
        Danger,

        /// <summary>The row is still worth showing but cannot be acted on.</summary>
        Muted
    }

    /// <summary>
    /// One row of a list: a leading visual (an <see cref="Avatar"/>, an <see cref="Icon"/>, anything),
    /// a title with a quieter line under it, and whatever the row is acted on with at the far end.
    /// <para>
    /// Where <see cref="ListItemText"/> is only the two lines of text, a <see cref="StatefulItemRow"/> is the whole
    /// row: it fills its container's width, keeps the trailing content pinned right, ellipsizes the text
    /// rather than pushing the controls off the edge, and can be washed in a <see cref="RowTone"/> to say
    /// what is happening to it - a person being added to a list reads green before anything is saved.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// StatefulItemRow(Avatar(initials: "DK"))
    ///    .SetTitle("Dana Kaur")
    ///    .SetSubtitle("dana.kaur@curiosity.ai")
    ///    .Tone(RowTone.Success)
    ///    .Trailing(Badge("Added").Pill().Outline().Success(), Button("Undo").Link());
    /// </code>
    /// </example>
    [Transpose.Name("tss.StatefulItemRow")]
    public sealed class StatefulItemRow : ComponentBase<StatefulItemRow, HTMLElement>
    {
        private readonly HTMLElement _leadingContainer;
        private readonly HTMLElement _title;
        private readonly HTMLElement _subtitle;
        private readonly HTMLElement _content;
        private readonly HTMLElement _trailingContainer;

        private RowTone _tone = RowTone.None;

        /// <summary>
        /// Initializes a new instance of this class, with the given leading visual - which may be left out
        /// for a row that starts at its text.
        /// </summary>
        public StatefulItemRow(IComponent leading = null)
        {
            _leadingContainer  = Div(Att("tss-statefulitemrow-leading"));
            _title             = Div(Att("tss-statefulitemrow-title"));
            _subtitle          = Div(Att("tss-statefulitemrow-subtitle"));
            _content           = Div(Att("tss-statefulitemrow-content"), _title, _subtitle);
            _trailingContainer = Div(Att("tss-statefulitemrow-trailing"));

            InnerElement = Div(Att("tss-statefulitemrow"), _leadingContainer, _content, _trailingContainer);

            SetLeading(leading);
            SetTitle((string)null);
            SetSubtitle((string)null);

            AttachClick();
            AttachContextMenu();
        }

        /// <summary>
        /// Sets the leading visual. Passing null takes it out and the row starts at its text.
        /// </summary>
        public StatefulItemRow SetLeading(IComponent leading)
        {
            ClearChildren(_leadingContainer);

            if (leading != null) _leadingContainer.appendChild(leading.Render());

            _leadingContainer.style.display = leading is null ? "none" : "";

            return this;
        }

        /// <summary>Sets the title. Null or empty leaves the line out.</summary>
        public StatefulItemRow SetTitle(string title)
        {
            var isEmpty = string.IsNullOrEmpty(title);

            ClearChildren(_title);

            if (!isEmpty) _title.appendChild(Span(Att("tss-statefulitemrow-title-text", text: title)));

            _title.style.display = isEmpty ? "none" : "";

            return this;
        }

        /// <summary>Sets the title to whatever the host built - a name with a quieter suffix, a highlighted match.</summary>
        public StatefulItemRow SetTitle(IComponent title)
        {
            ClearChildren(_title);

            if (title != null) _title.appendChild(title.Render());

            _title.style.display = title is null ? "none" : "";

            return this;
        }

        /// <summary>Sets the quieter second line. Null or empty leaves the line out and the row draws as one line.</summary>
        public StatefulItemRow SetSubtitle(string subtitle)
        {
            var isEmpty = string.IsNullOrEmpty(subtitle);

            ClearChildren(_subtitle);

            if (!isEmpty) _subtitle.appendChild(Span(Att("tss-statefulitemrow-subtitle-text", text: subtitle)));

            _subtitle.style.display = isEmpty ? "none" : "";

            return this;
        }

        /// <summary>Sets the quieter second line to whatever the host built.</summary>
        public StatefulItemRow SetSubtitle(IComponent subtitle)
        {
            ClearChildren(_subtitle);

            if (subtitle != null) _subtitle.appendChild(subtitle.Render());

            _subtitle.style.display = subtitle is null ? "none" : "";

            return this;
        }

        /// <summary>
        /// Sets what the row is acted on with - a badge, a menu, a remove button - replacing whatever was
        /// there. It sits at the far end and keeps its size while the text ellipsizes.
        /// </summary>
        public StatefulItemRow Trailing(params IComponent[] trailing)
        {
            ClearChildren(_trailingContainer);

            return AddTrailing(trailing);
        }

        /// <summary>Appends to the trailing content, keeping what is already there.</summary>
        public StatefulItemRow AddTrailing(params IComponent[] trailing)
        {
            if (trailing != null)
            {
                foreach (var component in trailing)
                {
                    if (component != null) _trailingContainer.appendChild(component.Render());
                }
            }

            _trailingContainer.style.display = _trailingContainer.childElementCount == 0 ? "none" : "";

            return this;
        }

        /// <summary>Gets the tone the row is washed in.</summary>
        public RowTone CurrentTone => _tone;

        /// <summary>Washes the row in the given tone.</summary>
        public StatefulItemRow Tone(RowTone tone)
        {
            _tone = tone;

            InnerElement.classList.remove("tss-statefulitemrow-primary", "tss-statefulitemrow-success", "tss-statefulitemrow-warning", "tss-statefulitemrow-danger", "tss-statefulitemrow-muted");

            switch (tone)
            {
                case RowTone.Primary: InnerElement.classList.add("tss-statefulitemrow-primary"); break;
                case RowTone.Success: InnerElement.classList.add("tss-statefulitemrow-success"); break;
                case RowTone.Warning: InnerElement.classList.add("tss-statefulitemrow-warning"); break;
                case RowTone.Danger: InnerElement.classList.add("tss-statefulitemrow-danger"); break;
                case RowTone.Muted: InnerElement.classList.add("tss-statefulitemrow-muted"); break;
            }

            return this;
        }

        /// <summary>Draws the row with no wash.</summary>
        public StatefulItemRow Neutral() => Tone(RowTone.None);

        /// <summary>Draws the row in the theme's primary color - selected, or changed.</summary>
        public StatefulItemRow Primary() => Tone(RowTone.Primary);

        /// <summary>Draws the row as something that was added.</summary>
        public StatefulItemRow Success() => Tone(RowTone.Success);

        /// <summary>Draws the row as something that needs care.</summary>
        public StatefulItemRow Warning() => Tone(RowTone.Warning);

        /// <summary>Draws the row as something being taken away.</summary>
        public StatefulItemRow Danger() => Tone(RowTone.Danger);

        /// <summary>Draws the row faded - still worth showing, but not something that can be acted on.</summary>
        public StatefulItemRow Muted() => Tone(RowTone.Muted);

        /// <summary>
        /// Strikes the title through, for a row naming something that is on its way out. The subtitle is
        /// left alone: it is what identifies the row while it is being removed.
        /// </summary>
        public StatefulItemRow Struck(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-statefulitemrow-struck");
            return this;
        }

        /// <summary>Draws the row tighter, for a dense list.</summary>
        public StatefulItemRow Compact(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-statefulitemrow-compact");
            return this;
        }

        /// <summary>
        /// Gives the row a hover wash and a pointer cursor, for a row that is itself clickable. It only
        /// changes how the row looks - wire the click with <c>OnClick</c>.
        /// </summary>
        public StatefulItemRow Interactive(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-statefulitemrow-interactive");
            return this;
        }

        /// <summary>Runs the given action when the row is clicked.</summary>
        public StatefulItemRow OnClick(System.Action action) => OnClick((_, __) => action());

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
