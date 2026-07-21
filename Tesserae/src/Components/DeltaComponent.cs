using System;
using System.Collections.Generic;
using Transpose;
using Transpose.Core;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A small inline metric showing the change between two values (with up/down arrow and tone for positive /
    /// negative / neutral deltas).
    /// </summary>
    [Transpose.Name("tss.DeltaComponent")]
    public class DeltaComponent : IComponent
    {
        private HTMLElement _root;
        private IComponent _currentContent;
        private bool _isAnimated;
        private ShadowRoot _shadowRoot;

        // Node.TEXT_NODE is 3 in DOM
        private const int TEXT_NODE = 3;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public DeltaComponent(IComponent initial, bool useShadowDom = false)
        {
            _currentContent = initial;
            if (useShadowDom)
            {
                _root = document.createElement("div");
                _shadowRoot = _root.attachShadow(new ShadowRootInit { mode = Transpose.Core.dom.Literals.Options.mode.open });
                _shadowRoot.appendChild(_currentContent.Render());
            }
            else
            {
                _root = _currentContent.Render();
            }
        }

        /// <summary>
        /// Configures the component to animated.
        /// </summary>
        public DeltaComponent Animated()
        {
            _isAnimated = true;
            return this;
        }

        /// <summary>
        /// Replaces the content in the component.
        /// </summary>
        public void ReplaceContent(IComponent newContent)
        {
            var newRoot = newContent.Render();
            if (_shadowRoot != null)
            {
                // We are diffing against the content inside shadow root.
                // Assuming there is one child which is the root of the component.
                if (_shadowRoot.firstChild != null)
                {
                    DiffAndPatch(_shadowRoot.firstChild, newRoot);
                }
                else
                {
                    _shadowRoot.appendChild(newRoot);
                }
            }
            else
            {
                DiffAndPatch(_root, newRoot);
            }
        }

        private void DiffAndPatch(Node current, Node next)
        {
            if (current.nodeType != next.nodeType || current.nodeName != next.nodeName)
            {
                if (current.parentNode != null)
                {
                    // Use the live `next` node instead of cloneNode(true) so
                    // event listeners attached by component initialization
                    // survive a ReplaceContent (cloneNode never copies
                    // listeners added via addEventListener). The replaced node
                    // is detached from its previous parent automatically.
                    if (_isAnimated && next is HTMLElement)
                    {
                        next.As<HTMLElement>().classList.add("tss-fade-in");
                    }
                    current.parentNode.replaceChild(next, current);
                }
                return;
            }

            if (current.nodeType == TEXT_NODE)
            {
                var currentText = current.textContent;
                var nextText = next.textContent;

                if (currentText != nextText)
                {
                    if (nextText.StartsWith(currentText))
                    {
                        var delta = nextText.Substring(currentText.Length);
                        var deltaSpan = document.createElement("span");
                        deltaSpan.textContent = delta;

                        if (_isAnimated)
                        {
                            deltaSpan.classList.add("tss-fade-in");
                        }

                        if (current.parentNode != null)
                        {
                            if (current.nextSibling != null)
                            {
                                current.parentNode.insertBefore(deltaSpan, current.nextSibling);
                            }
                            else
                            {
                                current.parentNode.appendChild(deltaSpan);
                            }
                        }
                    }
                    else
                    {
                        current.textContent = nextText;
                    }
                }
                return;
            }

            if (current is HTMLElement currentElement && next is HTMLElement nextElement)
            {
                SyncAttributes(currentElement, nextElement);
                DiffChildren(currentElement, nextElement);
            }
        }

        private void SyncAttributes(HTMLElement current, HTMLElement next)
        {
            var currentAttributes = current.attributes;
            if(currentAttributes.length  > 0)
            {
                for (int i = (int)(currentAttributes.length) - 1; i >= 0; i--)
                {
                    var attr = currentAttributes[(uint)i];
                    if (!next.hasAttribute(attr.name))
                    {
                        current.removeAttribute(attr.name);
                    }
                }
            }

            var nextAttributes = next.attributes;
            for (uint i = 0; i < nextAttributes.length; i++)
            {
                var attr = nextAttributes[i];
                var curAttr = current.getAttribute(attr.name);
                if (curAttr != attr.value)
                {
                    bool addFadeIn = attr.name == "class" && curAttr.Contains("tss-fade-in");
                    current.setAttribute(attr.name, attr.value);
                    if (addFadeIn) current.classList.add("tss-fade-in");
                }
            }
        }

        private void DiffChildren(HTMLElement currentParent, HTMLElement nextParent)
        {
            var currentChildren = currentParent.childNodes;
            var nextChildrenLive = nextParent.childNodes;

            // Snapshot — moving a live node into currentParent via
            // appendChild / replaceChild detaches it from nextParent, which
            // shrinks nextChildrenLive while we iterate it. Capturing the
            // children up-front keeps indices stable.
            int nextLen = (int)nextChildrenLive.length;
            var nextChildrenSnapshot = new Node[nextLen];
            for (int i = 0; i < nextLen; i++) nextChildrenSnapshot[i] = (Node)nextChildrenLive[(uint)i];

            int currentIndex = 0;
            int nextIndex = 0;

            // Console.WriteLine($"Diffing Children: CurrentLen={currentChildren.length}, NextLen={nextLen}");

            while (nextIndex < nextLen)
            {
                var nextChild = nextChildrenSnapshot[nextIndex];

                if (nextChild.nodeType == TEXT_NODE)
                {
                    string targetText = nextChild.textContent;
                    // Console.WriteLine($"Processing Text Node: '{targetText}'");

                    while (targetText.Length > 0 && currentIndex < currentChildren.length)
                    {
                        var currentChild = (Node)currentChildren[currentIndex];

                        string currentContent = null;
                        if (currentChild.nodeType == TEXT_NODE)
                        {
                            currentContent = currentChild.textContent;
                        }
                        else if (currentChild.nodeName == "SPAN" && currentChild.childNodes.length == 1 && ((Node)currentChild.firstChild).nodeType == TEXT_NODE)
                        {
                            currentContent = currentChild.textContent;
                        }

                        if (currentContent != null && targetText.StartsWith(currentContent))
                        {
                            // Console.WriteLine($"Match Prefix: '{currentContent}'");
                            targetText = targetText.Substring(currentContent.Length);
                            currentIndex++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (targetText.Length > 0)
                    {
                        if (string.IsNullOrWhiteSpace(targetText))
                        {
                            // Do nothing
                        }
                        else
                        {
                            var deltaSpan = document.createElement("span");
                            deltaSpan.textContent = targetText;

                            if (_isAnimated)
                            {
                                deltaSpan.classList.add("tss-fade-in");
                            }

                            if (currentIndex < currentChildren.length)
                            {
                                currentParent.insertBefore(deltaSpan, currentChildren[currentIndex]);
                            }
                            else
                            {
                                currentParent.appendChild(deltaSpan);
                            }
                            currentIndex++;
                        }
                    }
                    nextIndex++;
                }
                else
                {
                    // Element. We move the live next-tree node instead of
                    // cloneNode(true)-ing it so component event listeners
                    // (e.g. ToolCall expand-on-click) survive a ReplaceContent.
                    if (currentIndex < currentChildren.length)
                    {
                        var currentChild = (Node)currentChildren[currentIndex];

                        if (IsSameNodeType(currentChild, nextChild))
                        {
                            DiffAndPatch(currentChild, nextChild);
                            currentIndex++;
                        }
                        else
                        {
                            if (_isAnimated && nextChild is HTMLElement newElement)
                            {
                                newElement.classList.add("tss-fade-in");
                            }
                            currentParent.replaceChild(nextChild, currentChild);
                            currentIndex++;
                            // replaceChild shifts the live NodeList — don't
                            // advance nextIndex against the old children, the
                            // next iteration's reread of childNodes handles it.
                        }
                    }
                    else
                    {
                        if (_isAnimated && nextChild is HTMLElement newElement)
                        {
                            newElement.classList.add("tss-fade-in");
                        }
                        currentParent.appendChild(nextChild);
                        currentIndex++;
                    }
                    nextIndex++;
                }
            }

            // Remove extra children
            while (currentIndex < currentChildren.length)
            {
                currentParent.removeChild(currentChildren[currentIndex]);
            }
        }

        private bool IsSameNodeType(Node a, Node b)
        {
            if (a.nodeType != b.nodeType) return false;
            if (a.nodeName != b.nodeName) return false;
            return true;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render()
        {
            return _root;
        }
    }
}
