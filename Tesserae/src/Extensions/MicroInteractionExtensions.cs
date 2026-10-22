using System;
using Transpose;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Small, one-shot feedback animations and hover/gesture behaviours that work on any <see cref="IComponent"/>:
    /// a shake for a refused action, a pop for a toggled reaction, a nudge for a blocked dismissal, a spin for a
    /// refresh, a flash for a row that just changed, a shimmer while something loads, actions that appear on hover,
    /// and swipe-to-dismiss.
    /// <para>
    /// The one-shot methods (<see cref="Shake{T}"/>, <see cref="Pop{T}"/>, <see cref="Nudge{T}"/>,
    /// <see cref="SpinOnce{T}"/>, <see cref="Flash{T}"/>) are meant to be called from an event handler, at the
    /// moment the feedback is due, and restart the animation if it is already playing. They animate the
    /// component's rendered element only, with CSS (tss.microinteractions.css), and leave nothing behind. With
    /// reduced motion asked for, a shake becomes a brief red ring and the others do nothing, so no information is
    /// carried by motion alone.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.MicroInteractions")]
    public static class MicroInteractionExtensions
    {
        /// <summary>
        /// Shakes the component sideways once: the answer to an action that was refused (a locked command, an
        /// invalid submit, a wrong code). Pair it with a message that says why; the shake only draws the eye there.
        /// </summary>
        public static T Shake<T>(this T component) where T : IComponent => Play(component, "tss-mi-shake");

        /// <summary>
        /// Scales the component up and settles it back with a slight overshoot: a toggled reaction, a counter that
        /// just changed, a chip that was just added.
        /// </summary>
        public static T Pop<T>(this T component) where T : IComponent => Play(component, "tss-mi-pop");

        /// <summary>
        /// A small, quick scale pulse: "this is still here and still needs you". The answer to a click on the
        /// backdrop of a modal that cannot be dismissed that way.
        /// </summary>
        public static T Nudge<T>(this T component) where T : IComponent => Play(component, "tss-mi-nudge");

        /// <summary>
        /// Turns the component's icon (its first <c>&lt;i&gt;</c>, or the whole element when it has none) one full
        /// turn: a refresh that was just asked for.
        /// </summary>
        public static T SpinOnce<T>(this T component) where T : IComponent
        {
            var element = component.Render();
            var icon    = element.tagName == "I" ? element : element.querySelector("i").As<HTMLElement>();
            Play(icon ?? element, "tss-mi-spin");
            return component;
        }

        /// <summary>
        /// Washes the component's background with the primary colour and fades it out: a row that was just
        /// inserted, pinned or moved, so the eye finds where it landed.
        /// </summary>
        public static T Flash<T>(this T component) where T : IComponent => Play(component, "tss-mi-flash");

        /// <summary>
        /// Gives the component a slight press-in (scale 0.97) while it is held down, the tactile half of a click.
        /// Applies to anything clickable; buttons already have a hover state, this adds the press.
        /// </summary>
        public static T PressFeedback<T>(this T component, bool enabled = true) where T : IComponent
        {
            component.Render().UpdateClassIf(enabled, "tss-mi-press");
            return component;
        }

        /// <summary>
        /// Sweeps a highlight across the component for as long as <paramref name="enabled"/> is true: content that
        /// is on its way, in place, without swapping the component for a <see cref="Skeleton"/>. The component is
        /// clipped to its own box while it shimmers.
        /// </summary>
        public static T Shimmer<T>(this T component, bool enabled = true) where T : IComponent
        {
            var element = component.Render();
            element.UpdateClassIf(enabled, "tss-mi-shimmer");

            if (enabled) element.setAttribute("aria-busy", "true");
            else element.removeAttribute("aria-busy");

            return component;
        }

        /// <summary>
        /// Hides the component until the pointer is over <paramref name="host"/>, or focus is inside it, and then
        /// fades and lifts it in: the secondary actions of a card or a list row, kept out of the way until the row is
        /// the one being looked at. On a device without hover (a phone) the component is always shown, and it is
        /// never taken out of the tab order, so a keyboard reaches it (and focusing it reveals it).
        /// </summary>
        /// <param name="component">The actions to reveal.</param>
        /// <param name="host">The card or row whose hover and focus reveal them; usually an ancestor of <paramref name="component"/>.</param>
        public static T RevealOnHoverOf<T>(this T component, IComponent host) where T : IComponent
        {
            if (host is null) throw new ArgumentNullException(nameof(host));
            component.Render().classList.add("tss-mi-reveal");
            host.Render().classList.add("tss-mi-reveal-host");
            return component;
        }

        /// <summary>
        /// Lets the component be dragged sideways and flung away. It follows the pointer (fading as it goes), and on
        /// release either springs back or, past <paramref name="threshold"/> of its own width, slides off, collapses
        /// the space it took and calls <paramref name="onDismissed"/>. Vertical scrolling over it keeps working on
        /// touch screens.
        /// <para>
        /// Swiping is a shortcut, not the only way: keep a visible dismiss control (or a keyboard path) beside it.
        /// <see cref="DismissWithSwipe{T}"/> plays the same exit from code, so that control can look the same.
        /// </para>
        /// </summary>
        /// <param name="component">The card, toast or row to make swipeable.</param>
        /// <param name="onDismissed">Called once the exit animation has finished.</param>
        /// <param name="threshold">How far, as a fraction of the component's width, a drag must travel to dismiss on release.</param>
        /// <param name="removeOnDismiss">Whether to remove the element from the DOM once dismissed.</param>
        public static T SwipeToDismiss<T>(this T component, Action onDismissed, double threshold = 0.35, bool removeOnDismiss = true) where T : IComponent
        {
            var element = component.Render();
            element.classList.add("tss-mi-swipe");

            component.OnPan(g =>
            {
                if (element.classList.contains("tss-mi-swipe-out")) return;

                var width = Math.Max(1, element.getBoundingClientRect().As<DOMRect>().width);

                if (g.Phase == GesturePhase.End)
                {
                    element.classList.remove("tss-mi-swipe-dragging");

                    if (Math.Abs(g.OffsetX) >= width * threshold)
                    {
                        PlaySwipeOut(element, g.OffsetX < 0 ? -1 : 1, onDismissed, removeOnDismiss);
                    }
                    else
                    {
                        element.style.transform = "";
                        element.style.opacity   = "";
                    }
                    return;
                }

                element.classList.add("tss-mi-swipe-dragging");
                element.style.transform = $"translateX({g.OffsetX.ToString("0.#")}px) rotate({(g.OffsetX / width * 4).ToString("0.##")}deg)";
                element.style.opacity   = Math.Max(0.2, 1 - Math.Abs(g.OffsetX) / width).ToString("0.###");
            });

            // OnPan asks for touch-action: none; a horizontal swipe only needs the horizontal axis, so vertical
            // scrolling over the component is handed back to the browser.
            element.style.touchAction = "pan-y";

            return component;
        }

        /// <summary>
        /// Plays the exit of <see cref="SwipeToDismiss{T}"/> from code (from a close button, or a Delete key), so the
        /// component leaves the same way whichever path dismissed it.
        /// </summary>
        /// <param name="component">The component to dismiss.</param>
        /// <param name="onDismissed">Called once the exit animation has finished.</param>
        /// <param name="toTheLeft">Which way it leaves.</param>
        /// <param name="removeOnDismiss">Whether to remove the element from the DOM once dismissed.</param>
        public static T DismissWithSwipe<T>(this T component, Action onDismissed = null, bool toTheLeft = false, bool removeOnDismiss = true) where T : IComponent
        {
            var element = component.Render();
            element.classList.add("tss-mi-swipe");
            PlaySwipeOut(element, toTheLeft ? -1 : 1, onDismissed, removeOnDismiss);
            return component;
        }

        private static void PlaySwipeOut(HTMLElement element, int direction, Action onDismissed, bool removeOnDismiss)
        {
            if (element.classList.contains("tss-mi-swipe-out")) return;

            var rect = element.getBoundingClientRect().As<DOMRect>();
            element.classList.add("tss-mi-swipe-out");
            element.style.transform = $"translateX({(direction * (rect.width + 48)).ToString("0")}px) rotate({direction * 6}deg)";
            element.style.opacity   = "0";

            var reduced = PrefersReducedMotion;

            // Slide off, then give the space back so the items below close the gap rather than jump into it.
            window.setTimeout(_ =>
            {
                var height = element.getBoundingClientRect().As<DOMRect>().height;
                element.style.height = height.ToString("0.##") + "px";
                element.getBoundingClientRect();
                element.classList.add("tss-mi-swipe-collapse");
                element.style.height = "0px";

                window.setTimeout(__ =>
                {
                    if (removeOnDismiss) element.remove();
                    onDismissed?.Invoke();
                }, reduced ? 0 : 220);
            }, reduced ? 0 : 240);
        }

        /// <summary>True when the reader asked the system for less motion. Read every time, so a change applies.</summary>
        internal static bool PrefersReducedMotion
        {
            get
            {
                try
                {
                    return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Animates a change to an element's width: <paramref name="change"/> runs (usually swapping a label), and the
        /// element eases from its old width to its new one instead of snapping, the "fluid" button that grows to fit
        /// "Copied" or "Click again to confirm". The width is released once the transition ends, so later layout is
        /// unaffected.
        /// </summary>
        internal static void MorphWidth(HTMLElement element, Action change)
        {
            if (!element.IsMounted() || PrefersReducedMotion)
            {
                change();
                return;
            }

            var from = element.getBoundingClientRect().As<DOMRect>().width;
            element.style.width = "";
            change();
            var to = element.getBoundingClientRect().As<DOMRect>().width;

            if (Math.Abs(from - to) < 0.5) return;

            element.style.width = from.ToString("0.##") + "px";
            element.getBoundingClientRect();
            element.classList.add("tss-mi-morph-width");
            element.style.width = to.ToString("0.##") + "px";

            var token = Script.Write<int>("({0}.tssMorphToken = ({0}.tssMorphToken | 0) + 1)", element);

            window.setTimeout(_ =>
            {
                if (Script.Write<int>("({0}.tssMorphToken | 0)", element) != token) return;
                element.classList.remove("tss-mi-morph-width");
                element.style.width = "";
            }, 260);
        }

        /// <summary>
        /// Restarts a one-shot CSS animation on the element: the class comes off, a layout read makes the browser
        /// forget the old animation, and the class goes back on. It is taken off again when that animation ends.
        /// </summary>
        internal static T Play<T>(T component, string animationClass) where T : IComponent
        {
            Play(component.Render(), animationClass);
            return component;
        }

        internal static void Play(HTMLElement element, string animationClass)
        {
            if (element is null) return;

            element.classList.remove(animationClass);
            element.getBoundingClientRect();
            element.classList.add(animationClass);

            Action<Event> onEnd = null;

            onEnd = e =>
            {
                if (!ReferenceEquals(e.target, element)) return;
                element.classList.remove(animationClass);
                element.removeEventListener("animationend", onEnd);
            };

            element.addEventListener("animationend", onEnd);
        }
    }
}
