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
    public class DeltaComponent : IComponent, IReappliesStyling
    {
        private HTMLElement _root;
        private IComponent _currentContent;
        private bool _isAnimated;
        private ShadowRoot _shadowRoot;

        //Anything applied to this component from outside - a .Class(), an .Id(), a .Style() or a
        //.Tooltip() - lands on the element the content rendered, because that is the element this
        //component hands out. Swapping the root would take all of it out with the old node, so the
        //calls are recorded and made again against the node that replaces it.
        private List<Action> _reapply;
        private bool         _replaying;

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

            MarkAsReapplying();
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
        /// Reconciles what is on screen with <paramref name="newContent"/>, in place: the nodes that
        /// match are kept and patched, and only what genuinely differs is swapped. That is the point
        /// of this component - a streamed reply re-rendered per frame keeps its scroll position, its
        /// selection and its text nodes instead of flickering - but it also means a node that stays
        /// keeps the event listeners its component gave it, so nodes are only ever reconciled with
        /// nodes of the same component (see <see cref="CanReconcile"/>).
        /// </summary>
        public void ReplaceContent(IComponent newContent)
        {
            var newRoot = newContent.Render();

            UI.MarkComponent(newRoot, newContent);

            //The root on screen has to carry the component that rendered it, like the new one does,
            //and it cannot be trusted to: the container that took this component in marked the same
            //element with the DeltaComponent itself, because that is the component it was handed, and
            //in a shadow root nothing marked the content at all. Either way the first reconcile saw
            //two different components and swapped the whole content once, flashing all of it.
            var currentRoot = _shadowRoot != null ? _shadowRoot.firstChild : _root;

            if (currentRoot is HTMLElement currentRootElement) UI.MarkComponent(currentRootElement, _currentContent);

            _currentContent = newContent;

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
                //The root can be swapped rather than patched, and the swap detaches the node this
                //component was holding - so it has to keep hold of whichever node is now on screen.
                var previousRoot = _root;

                _root = DiffAndPatch(_root, newRoot).As<HTMLElement>();

                if (_root != previousRoot)
                {
                    //Everything a container and the fluent helpers wrote sits on the root, because this
                    //component has no element of its own - Render() hands out whatever the content
                    //rendered. A swap would drop the lot: the stack-item class its parent added and the
                    //width a .WS() on this component asked for. Both are recorded on the old root, so
                    //both can be carried over.
                    Stack.TransferItemStyles(previousRoot, _root);
                    Grid.TransferItemStyles(previousRoot, _root);

                    //A tooltip that was shown at least once left a tippy instance on the old node.
                    //Nothing will remove the node's popper now that the node itself is gone, so it
                    //is torn down here rather than left to the WhenRemoved further up the tree.
                    if (previousRoot.HasOwnProperty("_tippy"))
                    {
                        Transpose.Script.Write("{0}._tippy.destroy();", previousRoot);
                    }

                    MarkAsReapplying();
                    ReapplyStyling();
                }
            }
        }

        /// <summary>
        /// Patches <paramref name="current"/> into <paramref name="next"/>, or swaps it out when the
        /// two cannot be reconciled. Returns the node that is on screen afterwards, which is
        /// <paramref name="current"/> for a patch and <paramref name="next"/> for a swap.
        /// </summary>
        private Node DiffAndPatch(Node current, Node next)
        {
            if (!CanReconcile(current, next))
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

                //Nothing on screen to replace - this component was rendered but never added to a
                //parent. Adopting the new node is the only way the content is not simply lost: the
                //caller's next Render() then hands out the node that matches what it asked for.
                return next;
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

                return current;
            }

            if (current is HTMLElement currentElement && next is HTMLElement nextElement)
            {
                SyncAttributes(currentElement, nextElement);
                DiffChildren(currentElement, nextElement);
            }

            return current;
        }

        /// <summary>
        /// Whether two nodes may be reconciled with each other rather than swapped: the same kind of
        /// node, and the same component. A node carries the event listeners its component attached to
        /// it and no patch can move those, so re-purposing a node from one component to another leaves
        /// it answering to a component nobody can reach any more: the symptom is a control that looks
        /// right and does what its predecessor did - a tool call that became a "tools used" group
        /// mid-stream opening the chip it used to be instead of the group it is now.
        /// </summary>
        private static bool CanReconcile(Node current, Node next)
        {
            if (current.nodeType != next.nodeType || current.nodeName != next.nodeName) return false;

            //Nodes that are not elements carry no listeners of their own, so the text-append path that
            //makes streaming look like typing is never interrupted by this.
            if (!(current is HTMLElement currentElement) || !(next is HTMLElement nextElement)) return true;

            //The component that rendered the element, recorded by the container that took it in. This
            //is the signal that works for a component Tesserae cannot see the source of, and the only
            //one that separates two components which borrow the same kind of root: a SectionTitle and
            //a SearchableList are both a Stack's element, and so is anything an application composes
            //that way.
            var currentComponent = UI.ComponentOf(currentElement);
            var nextComponent    = UI.ComponentOf(nextElement);

            //An element that carries a component and one that does not were built differently enough
            //that patching one into the other is not worth the risk.
            if (currentComponent != nextComponent) return false;

            //An element built straight into its parent rather than added through a container carries
            //no component, and neither does anything inside a component's own markup. Its first CSS
            //class is what is left: the class a component puts on the root element it builds before
            //anything else is added to it. Checked either way, since a passthrough like Raw records
            //itself and says nothing about what it wraps.
            return RootClassOf(currentElement) == RootClassOf(nextElement);
        }

        /// <summary>
        /// The first class on <paramref name="element"/> that its component put there, skipping the
        /// ones the toolkit adds from outside.
        /// </summary>
        /// <remarks>
        /// <c>tss-fade-in</c> is added by this component to every node it inserts, and
        /// <c>tss-stack-item</c> by the container that took the element in. Both go on the node that
        /// is on screen and never on the freshly rendered one it is compared with, and on an element
        /// with no class of its own they <i>are</i> the first class. Counting them made the two sides
        /// of every classless element disagree: each <c>&lt;strong&gt;</c>, <c>&lt;code&gt;</c>,
        /// <c>&lt;p&gt;</c> and <c>&lt;table&gt;</c> of a streamed Markdown reply was swapped - and
        /// faded in again - on every chunk after the one that inserted it.
        /// </remarks>
        private static string RootClassOf(HTMLElement element)
        {
            var classes = element.classList;

            for (uint i = 0; i < classes.length; i++)
            {
                var name = classes[i];

                if (name != "tss-fade-in" && name != "tss-stack-item") return name;
            }

            return string.Empty;
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

                        if (CanReconcile(currentChild, nextChild))
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

        /// <summary>
        /// Puts this component on the element it currently renders, so that the fluent helpers can
        /// tell in one property read whether what they are applying needs remembering.
        /// </summary>
        /// <remarks>
        /// The alternative, testing the component's type in each helper, was measured at a fifth of
        /// the cost of <c>.Class()</c> itself - which a page calls thousands of times, virtually
        /// never on a DeltaComponent. A missing property on an element is a plain read.
        /// </remarks>
        private void MarkAsReapplying()
        {
            if (_root is object) _root[UI.ReappliesMarker] = this;
        }

        void IReappliesStyling.RememberStyling(Action reapply)
        {
            //A replayed call must not record itself, or every swap would double the list.
            if (_replaying || reapply is null) return;

            if (_reapply is null) _reapply = new List<Action>();

            _reapply.Add(reapply);
        }

        /// <summary>
        /// Applies everything recorded through <see cref="IReappliesStyling"/> to the element this
        /// component renders now, in the order it was originally applied.
        /// </summary>
        private void ReapplyStyling()
        {
            if (_reapply is null) return;

            _replaying = true;

            try
            {
                for (int i = 0; i < _reapply.Count; i++) _reapply[i]();
            }
            finally
            {
                _replaying = false;
            }
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
