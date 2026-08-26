using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Iterates the nodes of a DOM subtree, descending into same-origin iframes (waiting for them
    /// to load first). Adapted from mark.js (https://github.com/julkue/mark.js, MIT).
    /// </summary>
    [Transpose.Name("tss.DOMIterator")]
    public static class DOMIterator
    {
        // The NodeFilter constants from the DOM spec. The Transpose binding for the browser's
        // NodeFilter camel-cases the member names (NodeFilter.sHOW_TEXT), which is undefined at
        // run time - so the spec-frozen values are declared here instead.
        public const uint   SHOW_ELEMENT  = 0x1;
        public const uint   SHOW_TEXT     = 0x4;
        public const ushort FILTER_ACCEPT = 1;
        public const ushort FILTER_REJECT = 2;
        public const ushort FILTER_SKIP   = 3;

        private const int IFRAMES_TIMEOUT_MS = 5000;

        public static void QuerySelectorAllIframesRecursive(HTMLElement ctx, string selector, Action<HTMLElement> acceptNode)
        {
            if (ctx is null) return;

            if (ctx.tagName == "IFRAME")
            {
                QuerySelectorAllIframesRecursive(TryGetIframeDocument(ctx.As<HTMLIFrameElement>()), selector, acceptNode);
            }
            else
            {
                var elems = ctx.querySelectorAll(selector);

                foreach (var elem in elems)
                {
                    acceptNode(elem.As<HTMLElement>());
                }

                var iframes = ctx.querySelectorAll("iframe");

                foreach (var iframe in iframes)
                {
                    QuerySelectorAllIframesRecursive(iframe.As<HTMLIFrameElement>(), selector, acceptNode);
                }
            }
        }

        public static async Task ForEachNodeAsync<T>(HTMLElement ctx, Action<T> eachCb, uint whatToShow, NodeFilter nodeFilter, CancellationToken cancellationToken)
        {
            if (ctx is null)
            {
                return;
            }

            var iframeWaitTasks = new List<Task>();

            if (ctx.tagName == "IFRAME")
            {
                iframeWaitTasks.Add(HandleIframeElement(ctx.As<HTMLIFrameElement>(), whatToShow, nodeFilter, eachCb, iframeWaitTasks, cancellationToken));
            }
            else
            {
                var iframes = ctx.querySelectorAll("iframe").ToList().As<HTMLIFrameElement[]>();

                foreach (var iframeElement in iframes)
                {
                    iframeWaitTasks.Add(HandleIframeElement(iframeElement, whatToShow, nodeFilter, eachCb, iframeWaitTasks, cancellationToken));
                }

                foreach (var node in IterateThroughNodesInner<T>(ctx, whatToShow, nodeFilter))
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    eachCb(node);
                }
            }

            while (iframeWaitTasks.Any())
            {
                await Task.WhenAll(iframeWaitTasks);
                iframeWaitTasks.RemoveAll(iframeWaitTask => iframeWaitTask.IsCompleted);
            }
        }

        private static HTMLElement TryGetIframeDocument(HTMLIFrameElement ifr)
        {
            try
            {
                return ifr.contentWindow?.document?.As<HTMLElement>();
            }
            catch (Exception)
            {
                // A cross-origin or sandboxed iframe throws a SecurityError on document access -
                // its content can't be reached, so it is skipped
                return null;
            }
        }

        private static bool HasContent(HTMLIFrameElement ifr)
        {
            return !string.IsNullOrWhiteSpace(ifr.contentWindow?.document?.firstElementChild?.textContent);
        }

        private static bool IsIframeBlank(HTMLIFrameElement ifr)
        {
            var srcdoc = ifr.getAttribute("srcdoc");
            if (srcdoc != null) return false;

            var bl   = "about:blank";
            var src  = ifr.getAttribute("src")?.Trim();
            var href = ifr.contentWindow.location.href;
            return href == bl && src != bl && src != null;
        }

        private static Task<Document> WaitIframeLoadAsync(HTMLIFrameElement ifr, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<Document>();

            cancellationToken.Register(() => tcs.TrySetCanceled());

            var timeoutCTS = new CancellationTokenSource();
            timeoutCTS.CancelAfter(IFRAMES_TIMEOUT_MS);
            timeoutCTS.Token.Register(() => tcs.TrySetCanceled());

            ifr.addEventListener("load", () =>
            {
                try
                {
                    if (!IsIframeBlank(ifr))
                    {
                        var innerDocument = ifr.contentWindow?.document;

                        if (innerDocument is null)
                        {
                            throw new Exception("iframe inaccessible");
                        }
                        tcs.TrySetResult(innerDocument);
                    }
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
            });
            return tcs.Task;
        }

        private static async Task<Document> WaitIframeReadyAsync(HTMLIFrameElement ifr, CancellationToken cancellationToken)
        {
            if (ifr.contentWindow?.document?.readyState == "complete" || HasContent(ifr))
            {
                if (IsIframeBlank(ifr))
                {
                    return await WaitIframeLoadAsync(ifr, cancellationToken);
                }
                else
                {
                    return ifr.contentWindow?.document;
                }
            }
            else
            {
                return await WaitIframeLoadAsync(ifr, cancellationToken);
            }
        }

        private static IEnumerable<T> IterateThroughNodesInner<T>(HTMLElement ctx, uint whatToShow, NodeFilter nodeFilter = null)
        {
            NodeIterator nodeIterator = null;

            if (nodeFilter is object)
            {
                nodeIterator = document.createNodeIterator(
                    ctx,
                    whatToShow,
                    nodeFilter
                );
            }
            else
            {
                nodeIterator = document.createNodeIterator(
                    ctx,
                    whatToShow
                );
            }

            T currentNode = nodeIterator.nextNode().As<T>();

            while (Script.IsDefined(currentNode) && currentNode != null)
            {
                yield return currentNode;
                currentNode = nodeIterator.nextNode().As<T>();
            }
        }

        private static Task HandleIframeElement<T>(HTMLIFrameElement iframeElement, uint whatToShow, NodeFilter nodeFilter, Action<T> each, List<Task> iframeWaitTasks, CancellationToken cancellationToken)
        {
            var task = Task.Run(async () =>
            {
                Document innerDocument;

                try
                {
                    innerDocument = await WaitIframeReadyAsync(iframeElement, cancellationToken);
                }
                catch (Exception)
                {
                    // The frame never loaded within the timeout, the caller cancelled, or the frame
                    // is cross-origin/sandboxed - skip it so the rest of the page is still visited
                    return;
                }

                var iframes = innerDocument.querySelectorAll("iframe").ToList().As<HTMLIFrameElement[]>();

                foreach (var innerIframe in iframes)
                {
                    iframeWaitTasks.Add(HandleIframeElement(innerIframe, whatToShow, nodeFilter, each, iframeWaitTasks, cancellationToken));
                }

                foreach (var node in IterateThroughNodesInner<T>(innerDocument.As<HTMLHtmlElement>(), whatToShow, nodeFilter))
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    each(node);
                }
            });
            task.FireAndForget();

            return task;
        }
    }
}
