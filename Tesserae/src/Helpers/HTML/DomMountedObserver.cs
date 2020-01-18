﻿using System;
using System.Collections.Generic;
using System.Linq;
using static Retyped.dom;

namespace Tesserae.HTML
{
    public static class DomMountedObserver
    {
        private static List<(HTMLElement element, Action callback)> _elementsToTrackMountingOf;
        static DomMountedObserver()
        {
            _elementsToTrackMountingOf = new List<(HTMLElement, Action)>();
            var observer = new MutationObserver((mutationRecords, _) =>
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
                            if (IsEqualToOrIsChildOf(elementToTrackMountingOf.element, mountedElement))
                                elementsMountedThatWeCareAbout.Add(elementToTrackMountingOf);
                        }
                    }
                }
                if (elementsMountedThatWeCareAbout.Count == 0)
                    return;

                _elementsToTrackMountingOf = _elementsToTrackMountingOf.Except(elementsMountedThatWeCareAbout).ToList();
                foreach (var callbackToMake in elementsMountedThatWeCareAbout.Select(entry => entry.callback))
                    callbackToMake();
            });
            observer.observe(document.body, new MutationObserverInit { childList = true, subtree = true });
        }

        /// <summary>
        /// Some rendering libraries don't support rendering to a container until that container is mounted but the way that we commonly write components is to return an element that the caller will
        /// mount, which is a problem for componentizing those libraries. One workaround is to postpone the initialization until the element is mounted, which is made possible by this method. It
        /// will execute the specified action when the element is added to the document body. While there is at least one element being tracked in this manner, there is a marginal cost as all
        /// DOM manipulations will be tracked and any added elements will be checked (and all of their child elements checked) to see if they match one of the elements that we're interested
        /// in. The cost should be negligible but if there is a process that is going to make large and frequent updates to the DOM then it may be better to avoid having any elements in
        /// the notify-when-mounted list.
        /// </summary>
        public static void NotifyWhenMounted(HTMLElement element, Action callback)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            _elementsToTrackMountingOf.Add((element, callback));
        }

        private static bool IsEqualToOrIsChildOf(HTMLElement ele, Node possibleSelfOrParentEle)
        {
            while (ele != null)
            {
                if (ele == possibleSelfOrParentEle)
                    return true;
                ele = ele.parentElement;
            }
            return false;
        }
    }
}
