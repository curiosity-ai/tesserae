using System;
using System.Collections.Generic;
using System.Linq;
using static Retyped.dom;

namespace Tesserae.HTML
{
    public static class DomRemovalObserver
    {
        private static List<(HTMLElement element, Action callback)> _elementsToTrackRemovalOf;
        static DomRemovalObserver()
        {
            _elementsToTrackRemovalOf = new List<(HTMLElement, Action)>();
            var observer = new MutationObserver((mutationRecords, _) =>
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
                            highestAncestorElementIfAny = highestAncestorElementIfAny.parentElement;
                        if ((highestAncestorElementIfAny != null) && highestAncestorElementIfAny.tagName.Equals("HTML", StringComparison.OrdinalIgnoreCase))
                            continue;

                        foreach (var elementToTrackRemovalOf in _elementsToTrackRemovalOf)
                        {
                            if (IsEqualToOrIsChildOf(elementToTrackRemovalOf.element, removedElement))
                                elementsRemovedThatWeCareAbout.Add(elementToTrackRemovalOf);
                        }
                    }
                }
                if (elementsRemovedThatWeCareAbout.Count == 0)
                    return;

                _elementsToTrackRemovalOf = _elementsToTrackRemovalOf.Except(elementsRemovedThatWeCareAbout).ToList();
                foreach (var callbackToMake in elementsRemovedThatWeCareAbout.Select(entry => entry.callback))
                    callbackToMake();
            });
            observer.observe(document.body, new MutationObserverInit { childList = true, subtree = true });
        }

        /// <summary>
        /// When there is some relating tidying up that must be done when a component is removed from the DOM, this method may be used to enable that - it will execute the specified action when
        /// the element is removed. While there is at least one element being tracked in this manner, there is a marginal cost as all DOM manipulations will be tracked and any removed elements
        /// will be checked (and all of their child elements checked) to see if they match one of the elements that we're interested in. The cost should be negligible but if there is a process
        /// that is going to make large and frequent updates to the DOM then it may be better to avoid having any elements in the notify-when-removed list.
        /// </summary>
        public static void NotifyWhenRemoved(HTMLElement element, Action callback)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            _elementsToTrackRemovalOf.Add((element, callback));
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
