using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Keeps an <see cref="OmniResult{T}"/> footer on one line however many entries a host puts in it. The
    /// entries give up width first - each one ellipsizes, down to a floor that still says something - and
    /// once even that does not fit, the entries at the end of the line go behind a [...] button that shows
    /// them, still live and pressable, in a popover when hovered or focused.
    /// <para>
    /// The cost is kept to what a list of hundreds of rows can carry: one <see cref="ResizeObserver"/> and one
    /// <see cref="MutationObserver"/> are shared by every footer, so a window resize or a page of new rows
    /// is one callback, and every footer it names is measured in one pass - all the writes, then all the
    /// reads, then all the writes - which is one layout for the whole batch rather than one per row. A
    /// footer whose entries fit at their natural width costs that single read and nothing else.
    /// </para>
    /// <para>
    /// The mutation observer is what makes this work for anything in the footer, not just an
    /// <see cref="InlineLabel"/>: a component added through <see cref="OmniResult{T}.AddFooterEntry"/> that
    /// changes what it shows later - a label that looked something up, a list that collapsed itself -
    /// changes the footer's content, not the footer's box, so a resize observer alone would never hear of it.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.OmniResultFooterFit")]
    internal sealed class OmniResultFooterFit
    {
        private const string Key = "tssFooterFit";

        // How far an entry may be ellipsized before it goes behind the [...] button instead: its separating
        // dot, a mark and a few letters - enough that what is left still reads as the thing it was.
        private const double EntryFloor = 64;

        // A path label ("Box > sample-files") never shrinks its head, so its floor is wherever the head ends
        // plus this much of the tail.
        private const double TailFloor = 32;

        private static readonly ResizeObserver   Resizes;
        private static readonly MutationObserver Mutations;

        private readonly HTMLElement       _footer;
        private readonly HTMLElement       _more;
        private readonly HTMLElement       _popup;
        private readonly List<HTMLElement> _items      = new List<HTMLElement>();
        private readonly List<double>      _minimums   = new List<double>();
        private readonly List<HTMLElement> _overflowed = new List<HTMLElement>();

        private int    _visible = -1;
        private bool   _popupOpen;
        private bool   _dirty;
        private object _tippy;

        static OmniResultFooterFit()
        {
            Resizes = new ResizeObserver((entries, _) =>
            {
                var fits = new List<OmniResultFooterFit>();

                foreach (var entry in entries)
                {
                    var fit = Of(entry.target);

                    if (fit is null) continue;

                    //A footer that left the document is let go of, so a row thrown away is not kept alive by
                    //an observer that lives as long as the page. It is picked up again if it comes back.
                    if (!fit._footer.isConnected)
                    {
                        Resizes.unobserve(fit._footer);
                        DomObserver.WhenMounted(fit._footer, () => Resizes.observe(fit._footer));
                        continue;
                    }

                    fits.Add(fit);
                }

                FitAll(fits);
            });

            Mutations = new MutationObserver((records, _) =>
            {
                var fits = new List<OmniResultFooterFit>();

                foreach (var record in records)
                {
                    //The record names the node that changed, which can be anywhere under the footer - a text
                    //node inside a label inside an entry - so walk up to the footer it belongs to.
                    Node node = record.target;

                    while (node is object && Of(node) is null) node = node.parentNode;

                    if (node is object) fits.Add(Of(node));
                }

                FitAll(fits);
            });
        }

        private OmniResultFooterFit(HTMLElement footer)
        {
            _footer = footer;

            _more = Span(Att("tss-omniresult-footer-more", role: "button"), I(UIcons.MenuDots, UIconsWeight.Regular, "tss-omniresult-footer-more-glyph"));
            _more.setAttribute("tabindex", "0");

            _popup = Div(Att("tss-omniresult-footer tss-omniresult-footer-popup"));

            //The button is part of the row, so pressing it must not count as opening the result. Pressing it
            //shows the popover the same way hovering it does, for a pointer that doesn't hover.
            _more.addEventListener("click", e =>
            {
                StopEvent(e);
                ShowPopup();
            });

            _more.addEventListener("keydown", e =>
            {
                var keyboardEvent = e.As<KeyboardEvent>();

                if (keyboardEvent.key != "Enter" && keyboardEvent.key != " ") return;

                StopEvent(keyboardEvent);
                ShowPopup();
            });

            //Built on the first hover or focus - and shown there and then, since the popover's own listeners
            //only exist from this moment on and so never saw the event that built it.
            _more.addEventListener("mouseenter", _ => { if (EnsureTippy()) Transpose.Script.Write("{0}.show();", _tippy); });
            _more.addEventListener("focus",      _ => { if (EnsureTippy()) Transpose.Script.Write("{0}.show();", _tippy); });

            footer[Key] = this;

            Resizes.observe(footer);
            Mutations.observe(footer, new MutationObserverInit { childList = true, subtree = true, characterData = true, attributes = true, attributeFilter = new[] { "class", "style" } });
        }

        /// <summary>
        /// Starts keeping the given footer on one line. Safe to call more than once for the same footer.
        /// </summary>
        public static OmniResultFooterFit Attach(HTMLElement footer)
        {
            return Of(footer) ?? new OmniResultFooterFit(footer);
        }

        /// <summary>
        /// The fit keeping the given element on one line, or null when the element is not a fitted footer.
        /// </summary>
        public static OmniResultFooterFit Of(object element)
        {
            return element is null ? null : element.As<HTMLElement>()[Key].As<OmniResultFooterFit>();
        }

        /// <summary>
        /// Whether the element is the [...] button a fit puts at the end of the footer rather than an entry.
        /// </summary>
        public static bool IsMoreButton(Element element)
        {
            return element is object && element.classList.contains("tss-omniresult-footer-more");
        }

        /// <summary>
        /// Puts the entries the popover is showing back on the footer line, so the footer's own children are
        /// all of them again - which is what a host replacing its entries has to be able to count on.
        /// </summary>
        public void ClosePopup()
        {
            if (_tippy is object && _popupOpen) Transpose.Script.Write("{0}.hide();", _tippy);

            ReturnOverflowed();
        }

        private static void FitAll(List<OmniResultFooterFit> fits)
        {
            if (fits.Count == 0) return;

            var batch = new List<OmniResultFooterFit>();

            foreach (var fit in fits)
            {
                if (batch.Contains(fit)) continue;

                //The entries at the end of the line are in the popover right now - measuring the line without
                //them would decide they fit. The fit runs once they are back.
                if (fit._popupOpen)
                {
                    fit._dirty = true;
                    continue;
                }

                batch.Add(fit);
            }

            //All the writes, then all the reads, then all the writes: one layout for the whole batch.
            foreach (var fit in batch) fit.BeginMeasure();
            foreach (var fit in batch) fit.Measure();
            foreach (var fit in batch) fit.Apply();

            //What the fit itself just changed - classes, widths, the button's place - is not news to it.
            Mutations.takeRecords();
        }

        private void BeginMeasure()
        {
            //The button is measured at the end of the line, where it would go, so the space it takes is known
            //before deciding whether it is needed.
            if (_footer.lastElementChild != _more) _footer.appendChild(_more);

            _footer.classList.add("tss-omniresult-footer-measuring");
        }

        private void Measure()
        {
            _items.Clear();
            _minimums.Clear();
            _visible = -1;

            if (!_footer.isConnected) return;

            var footerRect = _footer.getBoundingClientRect().As<DOMRect>();

            //Hidden - collapsed away, or behind a sheet that only shows its title - so there is nothing to fit.
            if (footerRect.width <= 0) return;

            var available = footerRect.width;
            DOMRect last  = null;

            foreach (HTMLElement element in _footer.children)
            {
                if (element == _more) continue;

                var rect = element.getBoundingClientRect().As<DOMRect>();

                //An entry that hid itself (see the :empty and :has rules in tss.omniresult.css) takes no room.
                if (rect.width <= 0) continue;

                _items.Add(element);
                _minimums.Add(Math.Min(rect.width, FloorOf(element, rect)));

                last = rect;
            }

            if (_items.Count == 0)
            {
                _visible = 0;
                return;
            }

            var moreRect = _more.getBoundingClientRect().As<DOMRect>();

            //The gap is the footer's own column gap, read off the line rather than out of the stylesheet.
            var gap       = Math.Max(0, moreRect.left - last.right);
            var moreWidth = moreRect.width;

            var natural = last.right - _items[0].getBoundingClientRect().As<DOMRect>().left;

            //Fits as it is - by far the common case, and it costs nothing more.
            if (natural <= available + 0.5)
            {
                _visible = _items.Count;
                return;
            }

            var squeezed = 0.0;

            for (var i = 0; i < _minimums.Count; i++) squeezed += _minimums[i] + (i > 0 ? gap : 0);

            //Fits once every entry gives up what it can.
            if (squeezed <= available + 0.5)
            {
                _visible = _items.Count;
                return;
            }

            //Doesn't: keep as many as fit beside the button, and put the rest behind it.
            var used    = 0.0;
            var visible = 0;

            for (var i = 0; i < _minimums.Count; i++)
            {
                var next = used + (i > 0 ? gap : 0) + _minimums[i];

                if (next + gap + moreWidth > available + 0.5) break;

                used    = next;
                visible = i + 1;
            }

            //The first entry is always on the line, and when even it doesn't fit beside the button it gives up
            //whatever it has to - the line has to say something before it says "more".
            if (visible == 0)
            {
                visible      = 1;
                _minimums[0] = 0;
            }

            _visible = visible;
        }

        private static double FloorOf(HTMLElement element, DOMRect rect)
        {
            var separator = element.querySelector(".tss-inlinelabel-text-separator").As<HTMLElement>();

            if (separator is null) return EntryFloor;

            return separator.getBoundingClientRect().As<DOMRect>().right - rect.left + TailFloor;
        }

        private void Apply()
        {
            _footer.classList.remove("tss-omniresult-footer-measuring");

            if (_visible < 0) return;

            _overflowed.Clear();

            for (var i = 0; i < _items.Count; i++)
            {
                var element = _items[i];

                if (i < _visible)
                {
                    element.classList.remove("tss-omniresult-footer-overflowed");

                    //Rounded up: a floor a fraction under the entry's natural width would shave that fraction off a
                    //short entry and ellipsize "Box" to "B…".
                    var minWidth = Math.Ceiling(_minimums[i]) + "px";

                    if (element.style.minWidth != minWidth) element.style.minWidth = minWidth;
                }
                else
                {
                    element.classList.add("tss-omniresult-footer-overflowed");
                    element.style.minWidth = "";

                    _overflowed.Add(element);
                }
            }

            var hasMore = _overflowed.Count > 0;

            _more.UpdateClassIf(hasMore, "tss-omniresult-footer-more-visible");
            _more.setAttribute("aria-label", _overflowed.Count + " more");

            if (!hasMore && _tippy is object && _popupOpen) Transpose.Script.Write("{0}.hide();", _tippy);
        }

        private void ShowPopup()
        {
            EnsureTippy();

            Transpose.Script.Write("{0}.show();", _tippy);
        }

        // The popover is built the first time the button is pointed at, focused or pressed, so a list of rows
        // that never overflow - or that nobody hovers - never creates one. True when this call built it.
        private bool EnsureTippy()
        {
            if (_tippy is object) return false;

            Action onShow   = () =>
            {
                //Above whatever layer is open by the time it shows, not the one that was open when it was built.
                if (!int.TryParse(Layers.AboveCurrent(), out var zIndex)) zIndex = 9999;

                Transpose.Script.Write("{0}.setProps({ zIndex: {1} });", _tippy, zIndex);
            };

            Action onMount  = () => MoveOverflowedToPopup();
            Action onHidden = () => ReturnOverflowed();

            Func<bool> onShowAllowed = () => _overflowed.Count > 0;

            _tippy = Transpose.Script.Write<object>(
                "tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, trigger: 'mouseenter focus', placement: 'bottom-start', delay: [150, 100], maxWidth: 360, appendTo: document.body, theme: 'tss-popover', arrow: false, onShow: function() { if (!{2}()) return false; {3}(); }, onMount: {4}, onHidden: {5} })",
                _more, _popup, onShowAllowed, onShow, onMount, onHidden);

            return true;
        }

        // The popover shows the entries themselves, not copies: a label's click handler, its link and its
        // tooltip keep working, and one that is still looking something up fills in where it is.
        private void MoveOverflowedToPopup()
        {
            if (_popupOpen) return;

            _popupOpen = true;

            foreach (var element in _overflowed)
            {
                element.classList.remove("tss-omniresult-footer-overflowed");
                _popup.appendChild(element);
            }

            Mutations.takeRecords();
        }

        private void ReturnOverflowed()
        {
            if (!_popupOpen) return;

            _popupOpen = false;

            foreach (var element in _overflowed)
            {
                //An entry the host took out while it was in the popover stays out.
                if (element.parentElement != _popup) continue;

                element.classList.add("tss-omniresult-footer-overflowed");
                _footer.insertBefore(element, _more.parentElement == _footer ? _more : null);
            }

            Mutations.takeRecords();

            if (_dirty)
            {
                _dirty = false;
                FitAll(new List<OmniResultFooterFit> { this });
            }
        }
    }
}
