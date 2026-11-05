/* Tesserae: once a tooltip is open, the next one opens at once.
 *
 * A tooltip waits a moment before it appears so that a pointer passing over a toolbar does not set
 * every one of them off. After one is open that wait has done its job: moving to the neighbouring
 * control should show its tooltip with no delay and no animation, which feels faster without
 * defeating the point of the first delay.
 *
 * "Open" includes the instant after one closed (GRACE): moving from one control to the next closes
 * the first and opens the second a few milliseconds later, and that gap must not count as cold.
 *
 * This is a default plugin, so it reaches every tippy() call - the toolkit's and an application's
 * own - unless the call passes its own plugins list. It only touches hover tooltips (a trigger that
 * includes mouseenter and a show delay); menus and popovers shown by hand are left alone.
 *
 * It also publishes tippy.tssShowWhenHovered, for a tooltip that is created lazily on the first
 * mouseenter (tippy's own mouseenter has already gone by, so nothing would show it): it applies the
 * show delay itself, or skips it when the tooltips are warm. */
(function () {
    if (typeof tippy === "undefined" || !tippy.setDefaultProps) return;

    var GRACE = 300;
    var mounted = [];     // instances between onMount and onHidden
    var warm = false;
    var coolTimer = 0;

    function isHoverTooltip(instance) {
        var t = instance.props.trigger || "";
        var d = instance.props.delay;
        var show = Array.isArray(d) ? d[0] : d;
        return t.indexOf("mouseenter") >= 0 && show > 0;
    }

    function heat() {
        warm = true;
        if (coolTimer) { clearTimeout(coolTimer); coolTimer = 0; }
    }

    function scheduleCool() {
        if (mounted.length) return;
        if (coolTimer) clearTimeout(coolTimer);
        coolTimer = setTimeout(function () { coolTimer = 0; if (!mounted.length) warm = false; }, GRACE);
    }

    var warmUp = {
        name: "tssWarmTooltips",
        fn: function (instance) {
            var saved = null;
            var isMounted = false;

            function applyWarm() {
                if (saved || !isHoverTooltip(instance)) return;
                var d = instance.props.delay;
                saved = { delay: d, duration: instance.props.duration };
                instance.setProps({ delay: [0, Array.isArray(d) ? d[1] : 0], duration: 0 });
            }

            function restore() {
                if (!saved) return;
                var s = saved;
                saved = null;
                if (!instance.state.isDestroyed) instance.setProps({ delay: s.delay, duration: s.duration });
            }

            function leave() {
                if (!isMounted) return;
                isMounted = false;
                var i = mounted.indexOf(instance);
                if (i >= 0) mounted.splice(i, 1);
                scheduleCool();
            }

            var instant = false;
            instance._tssWarm = applyWarm;
            // The next tooltip is already up: this one goes without fading, whenever it gets to hide.
            instance._tssInstant = function () { instant = true; };

            return {
                // Runs before tippy schedules the show, which is when it reads the delay.
                onTrigger: function () { if (warm) applyWarm(); },
                onHide: function () {
                    if (!instant) return;
                    instant = false;
                    if (!saved) saved = { delay: instance.props.delay, duration: instance.props.duration };
                    instance.setProps({ duration: 0 });
                    // tippy may already have read the duration for this hide: unmount it ourselves.
                    setTimeout(function () {
                        var st = instance.state;
                        if (!st.isDestroyed && !st.isVisible && st.isMounted) instance.unmount();
                    }, 0);
                },
                onMount: function () {
                    if (!isHoverTooltip(instance) && !saved) return;
                    isMounted = true;
                    mounted.push(instance);
                    if (saved) {
                        // The tooltip being left is fading out, or about to hide: take it away rather
                        // than cross-fading the two.
                        mounted.slice().forEach(function (o) {
                            if (o === instance) return;
                            if (!o.state.isVisible && o.state.isMounted) o.unmount();
                            else if (o._tssInstant) o._tssInstant();
                        });
                    }
                    heat();
                },
                onHidden: function () { leave(); restore(); },
                onDestroy: function () { leave(); saved = null; }
            };
        }
    };

    /* Show a lazily created tooltip for a pointer that is already over its element. */
    tippy.tssShowWhenHovered = function (instance, element, delay, stillValid) {
        function ok() {
            return !instance.state.isDestroyed && element.matches(":hover") && (!stillValid || stillValid());
        }
        if (warm) {
            if (ok()) { if (instance._tssWarm) instance._tssWarm(); instance.show(); }
            return;
        }
        setTimeout(function () { if (ok()) instance.show(); }, delay);
    };

    tippy.setDefaultProps({ plugins: (tippy.defaultProps.plugins || []).concat([warmUp]) });
})();
