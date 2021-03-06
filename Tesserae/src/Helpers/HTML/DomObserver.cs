﻿using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;

namespace Tesserae.HTML
{
    public static class DomObserver
    {
        private static List<(HTMLElement element, Action callback)> _elementsToTrackMountingOf;
        private static List<(HTMLElement element, Action callback)> _elementsToTrackRemovalOf;

        static DomObserver()
        {
            _elementsToTrackMountingOf = new List<(HTMLElement, Action)>();
            _elementsToTrackRemovalOf = new List<(HTMLElement, Action)>();

            var observer = new MutationObserver((mutationRecords, _) =>
            {
                CheckMounted(mutationRecords);
                CheckUnmounted(mutationRecords);

            });
            observer.observe(document.body, new MutationObserverInit { childList = true, subtree = true });
        }

        private static void CheckMounted(MutationRecord[] mutationRecords)
        {
            if (_elementsToTrackMountingOf.Count == 0)
                return;

            var elementsMountedThatWeCareAbout = new List<(HTMLElement element, Action callback)>();

            foreach (var mutationRecord in mutationRecords)
            {
                foreach (var mountedElement in mutationRecord.addedNodes)
                {
                    foreach (var elementToTrackMountingOf in _elementsToTrackMountingOf)
                    {
                        if (elementToTrackMountingOf.element.IsEqualToOrIsChildOf(mountedElement))
                            elementsMountedThatWeCareAbout.Add(elementToTrackMountingOf);
                    }
                }
            }
            if (elementsMountedThatWeCareAbout.Count == 0)
                return;

            _elementsToTrackMountingOf = _elementsToTrackMountingOf.Except(elementsMountedThatWeCareAbout).ToList();
            
            window.requestAnimationFrame(_ =>
            {
                foreach (var entry in elementsMountedThatWeCareAbout)
                {
                    if (!entry.element.IsMounted())
                    {
                        // Ensure that the element wasn't removed from the DOM while we were waiting for the next animation frame
                        continue;
                    }

                    entry.callback();
                }
            });
        }

        private static void CheckUnmounted(MutationRecord[] mutationRecords)
        {
            if (_elementsToTrackRemovalOf.Count == 0)
                return;

            var elementsRemovedThatWeCareAbout = new List<(HTMLElement element, Action callback)>();
            foreach (var mutationRecord in mutationRecords)
            {
                foreach (var removedElement in mutationRecord.removedNodes)
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

                    foreach (var elementToTrackRemovalOf in _elementsToTrackRemovalOf)
                    {
                        if (elementToTrackRemovalOf.element.IsEqualToOrIsChildOf(removedElement))
                        {
                            elementsRemovedThatWeCareAbout.Add(elementToTrackRemovalOf);
                        }
                    }
                }
            }

            if (elementsRemovedThatWeCareAbout.Count == 0)
            {
                return;
            }

            _elementsToTrackRemovalOf = _elementsToTrackRemovalOf.Except(elementsRemovedThatWeCareAbout).ToList();
            window.requestAnimationFrame(_ =>
            {
                foreach (var entry in elementsRemovedThatWeCareAbout)
                {
                    if (entry.element.IsMounted())
                    {
                        // Ensure that the element wasn't re-added to the DOM while we were waiting for the next animation frame
                        continue;
                    }

                    entry.callback();
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
                _elementsToTrackMountingOf.Add((element, callback));
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
            _elementsToTrackRemovalOf.Add((element, callback));
        }
    }
}
