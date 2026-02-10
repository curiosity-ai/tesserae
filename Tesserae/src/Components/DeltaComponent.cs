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

        // Node.TEXT_NODE is 3 in DOM
        private const int TEXT_NODE = 3;

        public DeltaComponent(IComponent initial)
        {
            _currentContent = initial;
            _root = _currentContent.Render();
        }

        public void ReplaceContent(IComponent newContent)
        {
            var newRoot = newContent.Render();
            DiffAndPatch(_root, newRoot);
        }

        private void DiffAndPatch(Node current, Node next)
        {
            if (current.nodeType != next.nodeType || current.nodeName != next.nodeName)
            {
                if (current.parentNode != null)
                {
                    current.parentNode.replaceChild(next.cloneNode(true), current);
                }
                return;
            }

            if (current.nodeType == TEXT_NODE)
            {
                if (current.textContent != next.textContent)
                {
                    current.textContent = next.textContent;
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
            for (int i = (int)currentAttributes.length - 1; i >= 0; i--)
            {
                var attr = currentAttributes[(uint)i];
                if (!next.hasAttribute(attr.name))
                {
                    current.removeAttribute(attr.name);
                }
            }

            var nextAttributes = next.attributes;
            for (int i = 0; i < (int)nextAttributes.length; i++)
            {
                var attr = nextAttributes[(uint)i];
                if (current.getAttribute(attr.name) != attr.value)
                {
                    current.setAttribute(attr.name, attr.value);
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
                var nextChild = (Node)nextChildren[(uint)nextIndex];

                if (nextChild.nodeType == TEXT_NODE)
                {
                    string targetText = nextChild.textContent;
                    // Console.WriteLine($"Processing Text Node: '{targetText}'");

                    while (targetText.Length > 0 && currentIndex < (int)currentChildren.length)
                    {
                        var currentChild = (Node)currentChildren[(uint)currentIndex];

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

                        if (currentIndex < (int)currentChildren.length)
                        {
                            currentParent.insertBefore(deltaSpan, currentChildren[(uint)currentIndex]);
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
                    if (currentIndex < (int)currentChildren.length)
                    {
                        var currentChild = (Node)currentChildren[(uint)currentIndex];

                        if (IsSameNodeType(currentChild, nextChild))
                        {
                            DiffAndPatch(currentChild, nextChild);
                            currentIndex++;
                        }
                        else
                        {
                            var newChild = nextChild.cloneNode(true);
                            currentParent.replaceChild(newChild, currentChild);
                            currentIndex++;
                        }
                    }
                    else
                    {
                        currentParent.appendChild(nextChild.cloneNode(true));
                        currentIndex++;
                    }
                    nextIndex++;
                }
            }

            // Remove extra children
            while (currentIndex < (int)currentChildren.length)
            {
                currentParent.removeChild(currentChildren[(uint)currentIndex]);
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
