using Transpose;
using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;

namespace Tesserae
{
    [Transpose.Name("tss.domObs")]
    public static class DomObserver
    {
        // Marker attributes let us locate tracked elements in an added / removed subtree with
        // a single native querySelectorAll call instead of walking every tracked entry per
        // mutation. With many registrations (virtualized lists, deferred components, ...) this
        // turns an O(addedNodes * trackedEntries * treeDepth) hot path into O(addedNodes + matches).
        private const string MountAttr       = "data-tss-mount-pending";
        private const string UnmountAttr     = "data-tss-unmount-pending";
        private const string MountSelector   = "[data-tss-mount-pending]";
        private const string UnmountSelector = "[data-tss-unmount-pending]";

        private static readonly List<ElementAndCallback> _elementsToTrackMountingOf;
        private static readonly List<ElementAndCallback> _elementsToTrackRemovalOf;
        private static readonly MutationObserver         _observer;
        private static bool                              _isObserving;

        private class ElementAndCallback
        {
            // WeakRef is available in every evergreen browser (Chrome 84+, Firefox 79+,
            // Safari 14.1+), so we use it unconditionally. The callback is parked on the
            // element itself so that it survives as long as the element does (and gets
            // collected with it) without us holding a strong reference here.
            public HTMLElement ElementOrNullIfCollected  => Script.Write<HTMLElement>("{0}.ref.deref()",         this);
            public Action      CallbackOrNullIfCollected => Script.Write<Action>     ("{0}.callbackref.deref()", this);

            public ElementAndCallback(HTMLElement element, Action callback, string callbackBag)
            {
                // Park the callback on the element via an array bag so multiple registrations
                // on the same element don't accumulate ever-growing property names.
                Script.Write("var bag = {0}[{2}]; if (!bag) { bag = []; {0}[{2}] = bag; } bag.push({1});", element, callback, callbackBag);
                Script.Write("{0}.ref = new WeakRef({1})",         this, element);
                Script.Write("{0}.callbackref = new WeakRef({1})", this, callback);
            }
        }

        static DomObserver()
        {
            _elementsToTrackMountingOf = new List<ElementAndCallback>();
            _elementsToTrackRemovalOf  = new List<ElementAndCallback>();

            _observer = new MutationObserver((mutationRecords, _) =>
            {
                //First check all unmounted rules as they might modify the dom, then the mounted ones
                CheckUnmounted(mutationRecords);
                CheckMounted(mutationRecords);
                StopObservingIfNothingToTrack();
            });
        }

        // The MutationObserver receives every childList change on document.body. When
        // nothing is tracked, that's wasted work, so we start the observer lazily and
        // stop it as soon as both tracking lists drain.
        private static void StartObservingIfNeeded()
        {
            if (_isObserving) return;
            _observer.observe(document.body, new MutationObserverInit { childList = true, subtree = true });
            _isObserving = true;
        }

        private static void StopObservingIfNothingToTrack()
        {
            if (!_isObserving) return;
            if (_elementsToTrackMountingOf.Count > 0 || _elementsToTrackRemovalOf.Count > 0) return;
            _observer.disconnect();
            _isObserving = false;
        }

        public static void CleanUnusedReferences()
        {
            _elementsToTrackMountingOf.RemoveAll(e => e.ElementOrNullIfCollected is null);
            _elementsToTrackRemovalOf.RemoveAll(e => e.ElementOrNullIfCollected is null);
            StopObservingIfNothingToTrack();
        }

        private static void CheckMounted(MutationRecord[] mutationRecords)
        {
            if (_elementsToTrackMountingOf.Count == 0)
                return;

            HashSet<HTMLElement> matchedElements = null;

            foreach (var mutationRecord in mutationRecords)
            {
                var addedNodes = mutationRecord.addedNodes;
                if (addedNodes.length == 0) continue;

                foreach (var addedNode in addedNodes)
                {
                    // addedNodes can include Text / Comment nodes - we only care about elements
                    // (and only elements can carry attributes or have descendants to query).
                    if (Script.Write<bool>("{0}.nodeType !== 1", addedNode)) continue;

                    var addedElement = addedNode.As<HTMLElement>();

                    if (addedElement.hasAttribute(MountAttr))
                    {
                        if (matchedElements is null) matchedElements = new HashSet<HTMLElement>();
                        matchedElements.Add(addedElement);
                    }

                    // Native querySelectorAll on the added subtree is significantly faster than
                    // iterating every tracked entry and calling contains() for each one.
                    var descendants = addedElement.querySelectorAll(MountSelector);
                    var len = descendants.length;
                    if (len == 0) continue;

                    if (matchedElements is null) matchedElements = new HashSet<HTMLElement>();
                    for (uint i = 0; i < len; i++)
                    {
                        matchedElements.Add(descendants[i].As<HTMLElement>());
                    }
                }
            }

            if (matchedElements is null) return;

            // Clear the markers up front so a callback that re-registers the same element doesn't
            // see the stale marker and skip adding a fresh attribute.
            foreach (var el in matchedElements)
            {
                el.removeAttribute(MountAttr);
            }

            // Single pass: drop dead refs, capture matches, leave non-matches alone.
            List<ElementAndCallback> toFire = null;
            _elementsToTrackMountingOf.RemoveAll(entry =>
            {
                var element = entry.ElementOrNullIfCollected;

                if (element is null || entry.CallbackOrNullIfCollected is null)
                    return true;

                if (matchedElements.Contains(element))
                {
                    if (toFire is null) toFire = new List<ElementAndCallback>();
                    toFire.Add(entry);
                    return true;
                }

                return false;
            });

            if (toFire is null) return;

            window.requestAnimationFrame(_ =>
            {
                foreach (var entry in toFire)
                {
                    var element = entry.ElementOrNullIfCollected;

                    if (element is null) continue;

                    // The element may have been re-removed between the mutation firing and rAF.
                    if (!element.IsMounted()) continue;

                    entry.CallbackOrNullIfCollected?.Invoke();
                }
            });
        }

        private static void CheckUnmounted(MutationRecord[] mutationRecords)
        {
            if (_elementsToTrackRemovalOf.Count == 0)
                return;

            HashSet<HTMLElement> matchedElements = null;

            foreach (var mutationRecord in mutationRecords)
            {
                var removedNodes = mutationRecord.removedNodes;
                if (removedNodes.length == 0) continue;

                foreach (var removedNode in removedNodes)
                {
                    if (Script.Write<bool>("{0}.nodeType !== 1", removedNode)) continue;

                    var removedElement = removedNode.As<HTMLElement>();

                    // 2019-10-28 DWR: NotifyWhenRemoved should only fire when an element is actually gone from
                    // the document, not when it's been re-rendered elsewhere. isConnected is the native O(1)
                    // version of the "walk up to <html>" check we used to do.
                    if (removedElement.isConnected) continue;

                    if (removedElement.hasAttribute(UnmountAttr))
                    {
                        if (matchedElements is null) matchedElements = new HashSet<HTMLElement>();
                        matchedElements.Add(removedElement);
                    }

                    var descendants = removedElement.querySelectorAll(UnmountSelector);
                    var len = descendants.length;
                    if (len == 0) continue;

                    if (matchedElements is null) matchedElements = new HashSet<HTMLElement>();
                    for (uint i = 0; i < len; i++)
                    {
                        matchedElements.Add(descendants[i].As<HTMLElement>());
                    }
                }
            }

            if (matchedElements is null) return;

            foreach (var el in matchedElements)
            {
                el.removeAttribute(UnmountAttr);
            }

            List<ElementAndCallback> toFire = null;
            _elementsToTrackRemovalOf.RemoveAll(entry =>
            {
                var element = entry.ElementOrNullIfCollected;

                if (element is null || entry.CallbackOrNullIfCollected is null)
                    return true;

                if (matchedElements.Contains(element))
                {
                    if (toFire is null) toFire = new List<ElementAndCallback>();
                    toFire.Add(entry);
                    return true;
                }

                return false;
            });

            if (toFire is null) return;

            window.requestAnimationFrame(_ =>
            {
                foreach (var entry in toFire)
                {
                    var element = entry.ElementOrNullIfCollected;

                    if (element is null) continue;

                    // The element may have been re-added between the mutation firing and rAF.
                    if (element.IsMounted()) continue;

                    entry.CallbackOrNullIfCollected?.Invoke();
                }
            });
        }

        /// <summary>
        /// Some rendering libraries don't support rendering to a container until that container is mounted but the way that we commonly write components is to return an element that the caller will
        /// mount, which is a problem for componentizing those libraries. One workaround is to postpone the initialization until the element is mounted, which is made possible by this method. It
        /// will execute the specified action when the element is added to the document body. While there is at least one element being tracked in this manner, there is a marginal cost as all
        /// DOM manipulations will be tracked and any added elements will be checked (and all of their child elements checked) to see if they match one of the elements that we're interested
        /// in. The cost should be negligible but if there is a process that is going to make large and frequent updates to the DOM then it may be better to avoid having any elements in
        /// the notify-when-mounted list.
        /// </summary>
        public static void WhenMounted(HTMLElement element, Action callback)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (element.IsMounted())
            {
                callback();
                return;
            }

            element.setAttribute(MountAttr, "");
            _elementsToTrackMountingOf.Add(new ElementAndCallback(element, callback, "__tssMountCBs"));
            StartObservingIfNeeded();
        }

        /// <summary>
        /// When there is some relating tidying up that must be done when a component is removed from the DOM, this method may be used to enable that - it will execute the specified action when
        /// the element is removed. While there is at least one element being tracked in this manner, there is a marginal cost as all DOM manipulations will be tracked and any removed elements
        /// will be checked (and all of their child elements checked) to see if they match one of the elements that we're interested in. The cost should be negligible but if there is a process
        /// that is going to make large and frequent updates to the DOM then it may be better to avoid having any elements in the notify-when-removed list.
        /// </summary>
        public static void WhenRemoved(HTMLElement element, Action callback)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            // 2020-12-22 DWR: Tried adding a check here that would execute the callback immediately if the element wasn't currently in the DOM (on the premise that that indicated that it had
            // already been removed), similar to the check in WhenMounted - however, this doesn't work with a common pattern that we use where we want to register a WhenRemoved callback for
            // an element before its initial render / adding-to-the-DOM and so that check has had to be removed (as, in that case, the element would not be mounted because it hasn't been
            // added yet, not because it WAS added to the DOM and had already been removed again)
            element.setAttribute(UnmountAttr, "");
            _elementsToTrackRemovalOf.Add(new ElementAndCallback(element, callback, "__tssUnmountCBs"));
            StartObservingIfNeeded();
        }
    }
}
