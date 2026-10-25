/* Tesserae: unmount a hidden tippy even when no transitionend arrives.
 *
 * Tippy removes a hidden popup from the DOM - and only then runs onHidden - when the box's
 * hide transition ends. A stylesheet with no transition on .tippy-box (a theme that turns motion
 * off, a prefers-reduced-motion rule, an app's own "* { transition: none }") never fires
 * transitionend, so every popup ever shown stayed in the document, invisible, and every onHidden
 * callback (Tippy.ShowFor destroys its instance there) never ran.
 *
 * This default plugin reaches every tippy() call - the toolkit's and an application's own - unless
 * the call passes its own plugins list. It waits for the hide duration plus a margin and then
 * unmounts the popup if tippy has not: with a working transition it finds nothing to do, so the
 * fade-out looks exactly as before. */
(function () {
    if (typeof tippy === "undefined" || !tippy.setDefaultProps) return;

    var margin = 100;

    function hideDuration(instance) {
        var d = instance.props.duration;
        if (Array.isArray(d)) d = d[1];
        if (typeof d !== "number") d = tippy.defaultProps.duration[1];
        return d || 0;
    }

    var unmountWhenHidden = {
        name: "tssUnmountWhenHidden",
        fn: function (instance) {
            var timer = 0;
            function clear() { if (timer) { clearTimeout(timer); timer = 0; } }
            return {
                onShow: clear,
                onHidden: clear,
                onDestroy: clear,
                onHide: function () {
                    clear();
                    timer = setTimeout(function () {
                        timer = 0;
                        var s = instance.state;
                        // onHide can still be cancelled (a consumer's onHide returning false), and the
                        // popup can be shown again before this fires - both leave it visible.
                        if (!s.isDestroyed && !s.isVisible && s.isMounted) instance.unmount();
                    }, hideDuration(instance) + margin);
                }
            };
        }
    };

    tippy.setDefaultProps({ plugins: (tippy.defaultProps.plugins || []).concat([unmountWhenHidden]) });
})();
