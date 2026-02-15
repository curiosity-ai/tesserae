using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A Timeline component that displays events in chronological order.
    /// </summary>
    [H5.Name("tss.Timeline")]
    public class Timeline : IContainer<Timeline, IComponent>, IHasBackgroundColor, IHasMarginPadding
    {
        private readonly HTMLElement _timeline;
        private readonly HTMLElement _timelineOwner;

        /// <summary>
        /// Gets or sets the background color of the timeline.
        /// </summary>
        public string Background { get => _timelineOwner.style.background; set => _timelineOwner.style.background = value; }

        /// <summary>
        /// Gets or sets the margin of the timeline.
        /// </summary>
        public string Margin     { get => _timelineOwner.style.margin;     set => _timelineOwner.style.margin = value; }

        /// <summary>
        /// Gets or sets the padding of the timeline.
        /// </summary>
        public string Padding    { get => _timelineOwner.style.padding;    set => _timelineOwner.style.padding = value; }

        /// <summary>
        /// Gets or sets whether the timeline items should all be on the same side.
        /// </summary>
        public bool IsSameSide
        {
            get => _timelineOwner.classList.contains("tss-left");
            set => _timelineOwner.UpdateClassIf(value, "tss-left");
        }

        private bool left = true;

        /// <summary>
        /// Initializes a new instance of the Timeline class.
        /// </summary>
        public Timeline()
        {
            _timeline      = Div(_("tss-timeline"));
            _timelineOwner = Div(_("tss-timeline-owner"), _timeline);
        }

        /// <summary>
        /// Adds a component to the timeline.
        /// </summary>
        /// <param name="component">The component to add.</param>
        public void Add(IComponent component)
        {
            _timeline.appendChild(Wrap(component));
            Rebase(false);
        }

        /// <summary>
        /// Sets the timeline to display items on the same side.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public Timeline SameSide()
        {
            IsSameSide = true;
            return this;
        }

        /// <summary>
        /// Sets the timeline to display items on the same side if the width is less than the specified minimum width.
        /// </summary>
        /// <param name="minWidthPixels">The minimum width in pixels.</param>
        /// <returns>The current instance of the type.</returns>
        public Timeline SameSideIf(int minWidthPixels)
        {
            void Recompute()
            {
                var rect = (DOMRect)_timelineOwner.getBoundingClientRect();
                IsSameSide = rect.width <= minWidthPixels;
            }

            DomObserver.WhenMounted(_timelineOwner, () =>
            {
                var ro = new ResizeObserver((entries, obs) => Recompute());
                ro.observe(document.body);

                DomObserver.WhenRemoved(_timelineOwner, () =>
                {
                    ro.unobserve(document.body);
                });
            });

            return this;
        }

        /// <summary>
        /// Sets the maximum width of the timeline.
        /// </summary>
        /// <param name="maxWidth">The maximum width.</param>
        /// <returns>The current instance of the type.</returns>
        public Timeline TimelineWidth(UnitSize maxWidth)
        {
            _timeline.style.maxWidth = maxWidth.ToString();
            return this;
        }

        private void Rebase(bool rebaseAll)
        {
            var parent = _timeline;

            if (rebaseAll)
            {
                left = true;

                foreach (var n in parent.children)
                {
                    n.classList.remove("tss-left", "tss-right");
                    n.classList.add(left ? "tss-left" : "tss-right");
                    left = !left;
                }
            }
            else
            {
                //just do the final one
                _timeline.lastElementChild.classList.add(left ? "tss-left" : "tss-right");
                left = !left;
            }
        }

        private HTMLElement Wrap(IComponent component)
        {
            return Div(_("tss-timeline-container"), Div(_("tss-timeline-content"), component.Render()));
        }

        /// <summary>
        /// Clears all items from the timeline.
        /// </summary>
        public void Clear()
        {
            ClearChildren(_timeline);
        }

        /// <summary>
        /// Renders the timeline.
        /// </summary>
        /// <returns>The rendered HTMLElement.</returns>
        public HTMLElement Render() => _timelineOwner;

        /// <summary>
        /// Replaces an existing component in the timeline with a new one.
        /// </summary>
        /// <param name="newComponent">The new component.</param>
        /// <param name="oldComponent">The old component to be replaced.</param>
        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            _timeline.replaceChild(Wrap(newComponent), Wrap(oldComponent));
            Rebase(true);
        }
    }
}