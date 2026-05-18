using H5;
using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.domObs")]
    public static class DomObserver
    {
        private static readonly List<ElementAndCallback> _elementsToTrackMountingOf;
        private static readonly List<ElementAndCallback> _elementsToTrackRemovalOf;
        private static readonly MutationObserver         _observer;
        private static bool                              _isObserving;

        private class ElementAndCallback
        {
            // Tri-state cache for the WeakRef feature probe: Unknown until we've tested,
            // then either Available or NotAvailable. Using an int + consts avoids the
            // Nullable<bool> boxing that h5 emits for bool?.
            private const int WeakRefUnknown      = 0;
            private const int WeakRefNotAvailable = 1;
            private const int WeakRefAvailable    = 2;

            private static int _weakrefState;
            private static int _count;

            private static bool IsAvailable()
            {
                if (_weakrefState != WeakRefUnknown) return _weakrefState == WeakRefAvailable;

                try
                {
                    Script.Write("let ref = new WeakRef({0})", new object());
                    _weakrefState = WeakRefAvailable;
                }
                catch
                {
                    _weakrefState = WeakRefNotAvailable;
                }

                return _weakrefState == WeakRefAvailable;
            }

            public HTMLElement ElementOrNullIfCollected => dereference();

            private HTMLElement dereference()
            {
                if (IsAvailable())
                {
                    return Script.Write<HTMLElement>("{0}.ref.deref()", this);
                }
                else
                {
                    return Script.Write<HTMLElement>("{0}.ref", this);
                }
            }

            public Action CallbackOrNullIfCollected => dereferenceCallback();

            private Action dereferenceCallback()
            {
                if (IsAvailable())
                {
                    return Script.Write<Action>("{0}.callbackref.deref()", this);
                }
                else
                {
                    return Script.Write<Action>("{0}.callbackref", this);
                }
            }

            public ElementAndCallback(HTMLElement element, Action callback)
            {
                if (IsAvailable())
                {
                    _count++;

                    if (_count < 0) { _count = 0; }
                    Script.Write("{0}['callbackRefN' + {2}] = {1}",    element, callback, _count); //We need to store the callback reference on the object otherwise it can be collected before the element
                    Script.Write("{0}.ref = new WeakRef({1})",         this,    element);
                    Script.Write("{0}.callbackref = new WeakRef({1})", this,    callback);
                }
                else
                {
                    Script.Write("{0}.ref = {1}",         this, element);
                    Script.Write("{0}.callbackref = {1}", this, callback);
                }
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

            HashSet<ElementAndCallback> matched = null;

            foreach (var mutationRecord in mutationRecords)
            {
                var addedNodes = mutationRecord.addedNodes;
                if (addedNodes.length == 0) continue;

                foreach (var mountedElement in addedNodes)
                {
                    for (int i = 0; i < _elementsToTrackMountingOf.Count; i++)
                    {
                        var entry   = _elementsToTrackMountingOf[i];
                        var element = entry.ElementOrNullIfCollected;

                        if (element != null && element.IsEqualToOrIsChildOf(mountedElement))
                        {
                            if (matched is null) matched = new HashSet<ElementAndCallback>();
                            matched.Add(entry);
                        }
                    }
                }
            }

            if (matched is null) return;

            // Remove matched and collected entries in a single pass.
            _elementsToTrackMountingOf.RemoveAll(e => matched.Contains(e) || e.ElementOrNullIfCollected is null || e.CallbackOrNullIfCollected is null);

            window.requestAnimationFrame(_ =>
            {
                foreach (var entry in matched)
                {
                    var element = entry.ElementOrNullIfCollected;

                    if (element != null)
                    {
                        if (!element.IsMounted())
                        {
                            // Ensure that the element wasn't removed from the DOM while we were waiting for the next animation frame
                            continue;
                        }

                        entry.CallbackOrNullIfCollected?.Invoke();
                    }
                }
            });
        }

        private static void CheckUnmounted(MutationRecord[] mutationRecords)
        {
            if (_elementsToTrackRemovalOf.Count == 0)
                return;

            HashSet<ElementAndCallback> matched = null;

            foreach (var mutationRecord in mutationRecords)
            {
                var removedNodes = mutationRecord.removedNodes;
                if (removedNodes.length == 0) continue;

                foreach (var removedElement in removedNodes)
                {
                    // 2019-10-28 DWR: The intent behind the NotifyWhenRemoved method is to fire a callback when an element is removed from the document, so that any related tidy-up / disposal
                    // may be performed. However, this will also be fired if an element (or one of its ancestors) is RE-rendered somewhere and that's not really what we want, so if the element
                    // that has been identified as being "removed" is actually still part of a branch that reaches back up to the html element then don't consider it removed.

                    var highestAncestorElementIfAny = removedElement.parentElement;

                    while (highestAncestorElementIfAny?.parentElement != null)
                    {
                        highestAncestorElementIfAny = highestAncestorElementIfAny.parentElement;
                    }

                    if ((highestAncestorElementIfAny != null) && highestAncestorElementIfAny.tagName.Equals("HTML", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    for (int i = 0; i < _elementsToTrackRemovalOf.Count; i++)
                    {
                        var entry   = _elementsToTrackRemovalOf[i];
                        var element = entry.ElementOrNullIfCollected;

                        if (element is object && element.IsEqualToOrIsChildOf(removedElement))
                        {
                            if (matched is null) matched = new HashSet<ElementAndCallback>();
                            matched.Add(entry);
                        }
                    }
                }
            }

            if (matched is null) return;

            _elementsToTrackRemovalOf.RemoveAll(e => matched.Contains(e) || e.ElementOrNullIfCollected is null || e.CallbackOrNullIfCollected is null);

            window.requestAnimationFrame(_ =>
            {
                foreach (var entry in matched)
                {
                    var element = entry.ElementOrNullIfCollected;

                    if (element is object)
                    {
                        if (element.IsMounted())
                        {
                            // Ensure that the element wasn't re-added to the DOM while we were waiting for the next animation frame
                            continue;
                        }

                        entry.CallbackOrNullIfCollected?.Invoke();
                    }
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
            }
            else
            {
                _elementsToTrackMountingOf.Add(new ElementAndCallback(element, callback));
                StartObservingIfNeeded();
            }
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
            _elementsToTrackRemovalOf.Add(new ElementAndCallback(element, callback));
            StartObservingIfNeeded();
        }
    }
}