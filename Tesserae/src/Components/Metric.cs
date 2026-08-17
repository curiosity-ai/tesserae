using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A large numeric KPI tile used inside dashboards, showing a value with optional label and trend indicator.
    /// <para>
    /// Beside the number it can lead with an <see cref="IconTile"/> - the same rounded, tinted square an
    /// <see cref="OmniResult{T}"/> row leads with - saying what is being counted, and it can hold a chart
    /// (a <see cref="Sparkline"/>, a <see cref="ContributionBar"/>) under the value to show how the number
    /// got there.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.Metric")]
    public class Metric : ComponentBase<Metric, HTMLElement>
    {
        private readonly HTMLElement _container;
        private readonly HTMLElement _iconContainer;
        private readonly HTMLElement _bodyContainer;
        private readonly HTMLElement _headerContainer;
        private readonly HTMLElement _titleContainer;
        private readonly HTMLElement _valueContainer;
        private readonly HTMLElement _changeContainer;
        private readonly HTMLElement _chartContainer;

        private IconTile _iconTile;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Metric(string title, string value)
            : this(TextBlock(title).SmallPlus().SemiBold().Foreground(Theme.Secondary.Foreground).Render(),
                   TextBlock(value).XLarge().SemiBold().Render())
        {
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Metric(IComponent title, IComponent value) : this(title.Render(), value.Render())
        {
        }

        private Metric(HTMLElement title, HTMLElement value)
        {
            _titleContainer  = Div(Att("tss-metric-title"), title);
            _valueContainer  = Div(Att("tss-metric-value"), value);
            _changeContainer = Div(Att("tss-metric-change"));
            _chartContainer  = Div(Att("tss-metric-chart"));

            // The header is the title's own line, so a change indicator can be pulled up level with it
            // (see ChangeInHeader) without the title and the value ending up side by side.
            _headerContainer = Div(Att("tss-metric-header"), _titleContainer);

            // The tile lives outside the body, so it stands beside the whole title/value block rather than
            // above it - and is only put in the DOM once something is actually on it.
            _iconContainer   = Div(Att("tss-metric-icon"));

            _bodyContainer   = Div(Att("tss-metric-body"), _headerContainer, _valueContainer, _chartContainer, _changeContainer);

            _container       = Div(Att("tss-metric"), _bodyContainer);

            InnerElement = _container;
        }

        /// <summary>
        /// Puts an icon on a tile in front of the title and the value, in the given color, over a paler wash
        /// of that same color - the same tile an <see cref="OmniResult{T}"/> row leads with. Pass the
        /// full-strength color the glyph should be; a null color leaves the tile neutral.
        /// </summary>
        public Metric SetIcon(UIcons icon, string color = null, UIconsWeight weight = UIconsWeight.Regular)
        {
            EnsureIconTile().SetIcon(icon, color, weight);

            return ShowIcon();
        }

        /// <summary>
        /// Puts a few letters on the tile in place of a glyph - a unit, a currency, a file type - in the
        /// given color, over a paler wash of that same color.
        /// </summary>
        public Metric SetIcon(string text, string color = null, TextSize? size = null)
        {
            EnsureIconTile().SetIcon(text, color, size);

            return ShowIcon();
        }

        /// <summary>
        /// Puts the given component on the tile - an <see cref="Image"/>, an <see cref="Avatar"/>, an emoji.
        /// A null value takes the tile away again.
        /// </summary>
        public Metric SetIcon(IComponent iconOrImage, string color = null)
        {
            if (iconOrImage is null)
            {
                if (_iconContainer.parentElement is object) _container.removeChild(_iconContainer);

                return this;
            }

            EnsureIconTile().SetIcon(iconOrImage, color);

            return ShowIcon();
        }

        /// <summary>
        /// Sets how big the leading tile is drawn - 44px square by default, which is the size a KPI wants
        /// beside a number two lines tall.
        /// </summary>
        public Metric IconSize(UnitSize size)
        {
            EnsureIconTile().Size(size);

            return this;
        }

        /// <summary>
        /// Draws the value above the title rather than under it - the shape a counter takes when the number
        /// is the point and the words under it only say what was counted ("5 / In my scope").
        /// </summary>
        public Metric ValueFirst(bool value = true)
        {
            _container.UpdateClassIf(value, "tss-metric-value-first");

            return this;
        }

        /// <summary>
        /// Pulls the change indicator up onto the title's own line, pushed to its far end - for a card whose
        /// trend belongs beside the label rather than under the chart.
        /// </summary>
        public Metric ChangeInHeader(bool value = true)
        {
            if (value)
            {
                _headerContainer.appendChild(_changeContainer);
            }
            else if (_changeContainer.parentElement != _bodyContainer)
            {
                _bodyContainer.appendChild(_changeContainer);
            }

            _container.UpdateClassIf(value, "tss-metric-change-in-header");

            return this;
        }

        /// <summary>
        /// Configures the component to chart.
        /// </summary>
        public Metric Chart(IComponent chart)
        {
            ClearChildren(_chartContainer);
            if (chart != null)
            {
                var rendered = chart.Render();

                // The chart reads as a trend under the number, so it spans the tile rather than keeping
                // whatever intrinsic size it came with — a Sparkline's default 100x30 would sit in the
                // corner of the card. Height goes back to auto so the chart keeps its own proportions at
                // that width. A caller who asked for a size with .W()/.WS()/.H() left the marker Stack
                // writes, and that size is kept.
                if (!rendered.hasAttribute("tss-stk-w")) rendered.style.width  = "100%";
                if (!rendered.hasAttribute("tss-stk-h")) rendered.style.height = "auto";

                _chartContainer.appendChild(rendered);
            }
            return this;
        }

        /// <summary>
        /// Configures the component to change.
        /// </summary>
        public Metric Change(IComponent change)
        {
            ClearChildren(_changeContainer);
            _changeContainer.appendChild(change.Render());
            return this;
        }

        // The tile is built on the first call that needs one, but only put in the DOM once something is
        // actually on it - so sizing it before filling it doesn't leave an empty square in the card.
        private IconTile EnsureIconTile()
        {
            if (_iconTile is null)
            {
                _iconTile = new IconTile();

                _iconContainer.appendChild(_iconTile.Render());
            }

            return _iconTile;
        }

        private Metric ShowIcon()
        {
            if (_iconContainer.parentElement is null) _container.insertBefore(_iconContainer, _bodyContainer);

            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return _container;
        }
    }
}
