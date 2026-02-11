using System;
using System.Collections.Generic;
using H5;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.DeltaComponent")]
    public class DeltaComponent : IComponent
    {
        private HTMLElement _root;
        private IComponent _currentContent;
        private bool _isAnimated;
        private ShadowRoot _shadowRoot;

        // Node.TEXT_NODE is 3 in DOM
        private const int TEXT_NODE = 3;

        public DeltaComponent(IComponent initial, bool useShadowDom = false)
        {
            _currentContent = initial;
            if (useShadowDom)
            {
                _root = document.createElement("div");
                _shadowRoot = _root.attachShadow(new ShadowRootInit { mode = H5.Core.dom.Literals.Options.mode.open });
                _shadowRoot.appendChild(_currentContent.Render());
            }
            else
            {
                _root = _currentContent.Render();
            }
        }

        public DeltaComponent Animated()
        {
            _isAnimated = true;
            return this;
        }

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
                    var newChild = next.cloneNode(true);
                    if (_isAnimated && newChild is HTMLElement newElement)
                    {
                        newElement.classList.add("tss-fade-in");
                    }
                    current.parentNode.replaceChild(newChild, current);
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
            var nextChildren = nextParent.childNodes;

            int currentIndex = 0;
            int nextIndex = 0;
            int nextLen = (int)nextChildren.length;

            // Console.WriteLine($"Diffing Children: CurrentLen={currentChildren.length}, NextLen={nextLen}");

            while (nextIndex < nextLen)
            {
                var nextChild = (Node)nextChildren[nextIndex];

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
                    nextIndex++;
                }
                else
                {
                    // Element
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
                            var newChild = nextChild.cloneNode(true);
                            if (_isAnimated && newChild is HTMLElement newElement)
                            {
                                newElement.classList.add("tss-fade-in");
                            }
                            currentParent.replaceChild(newChild, currentChild);
                            currentIndex++;
                        }
                    }
                    else
                    {
                        var newChild = nextChild.cloneNode(true);
                        if (_isAnimated && newChild is HTMLElement newElement)
                        {
                            newElement.classList.add("tss-fade-in");
                        }
                        currentParent.appendChild(newChild);
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

        public HTMLElement Render()
        {
            return _root;
        }
    }
}
