using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public partial class Diagram
    {
        /// <summary>
        /// A single node within a <see cref="Diagram"/>, rendered as a pill with an optional icon (or image) and text.
        /// Supports the button-like tonal styles (default, primary, secondary, success, danger), arbitrary
        /// foreground/background colors, dragging, and click / context-menu events. Icon-only nodes render as circles.
        /// </summary>
        [Transpose.Name("tss.DiagramNode")]
        public class Node : IComponent, IHasBackgroundColor, IHasForegroundColor
        {
            private static readonly string[] _styleClasses = new[] { "tss-diagram-node-default", "tss-diagram-node-primary", "tss-diagram-node-secondary", "tss-diagram-node-success", "tss-diagram-node-danger" };

            private readonly HTMLElement     _element;
            private readonly HTMLSpanElement _textSpan;
            private          HTMLElement     _iconElement;
            private          Diagram         _owner;
            private          bool            _suppressClick;

            private event ComponentEventHandler<Node, MouseEvent> Clicked;
            private event ComponentEventHandler<Node, MouseEvent> ContextMenu;

            internal double X;
            internal double Y;
            internal double MeasuredWidth;
            internal double MeasuredHeight;

            /// <summary>
            /// True after the node is positioned explicitly (via <see cref="At"/>) or dragged by the user - pinned
            /// nodes keep their position when the automatic layout runs.
            /// </summary>
            internal bool IsPinned;

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public Node(string text = "")
            {
                _textSpan = Span(_("tss-diagram-node-text", text: text));
                _element  = Div(_("tss-diagram-node tss-diagram-node-default"), _textSpan);

                MoveTo(0, 0);
                HookDragEvents();

                _element.addEventListener("click", e =>
                {
                    if (_suppressClick)
                    {
                        _suppressClick = false;
                        return;
                    }
                    Clicked?.Invoke(this, e.As<MouseEvent>());
                });

                _element.addEventListener("contextmenu", e => ContextMenu?.Invoke(this, e.As<MouseEvent>()));
            }

            /// <summary>
            /// Gets or sets the CSS background of the node.
            /// </summary>
            public string Background
            {
                get => _element.style.background;
                set => _element.style.background = value;
            }

            /// <summary>
            /// Gets or sets the CSS color (foreground) of the node.
            /// </summary>
            public string Foreground
            {
                get => _element.style.color;
                set => _element.style.color = value;
            }

            /// <summary>
            /// Gets or sets the node text.
            /// </summary>
            public string Text
            {
                get => _textSpan.innerText;
                set
                {
                    _textSpan.innerText = value;
                    UpdateIconOnlyState();
                    _owner?.RequestAutoArrange();
                }
            }

            /// <summary>
            /// Renders the component's root HTML element.
            /// </summary>
            public HTMLElement Render() => _element;

            /// <summary>
            /// Sets the text of the node.
            /// </summary>
            public Node SetText(string text)
            {
                Text = text;
                return this;
            }

            /// <summary>
            /// Sets the icon of the node.
            /// </summary>
            public Node SetIcon(UIcons icon, string color = "", TextSize size = TextSize.Small, UIconsWeight weight = UIconsWeight.Regular)
            {
                var i = I(_($"tss-diagram-node-icon {Tesserae.Icon.Transform(icon, weight)} {size}"));

                if (!string.IsNullOrEmpty(color))
                {
                    i.style.color = color;
                }

                ReplaceIcon(i);
                return this;
            }

            /// <summary>
            /// Sets the icon of the node from an emoji glyph.
            /// </summary>
            public Node SetIcon(Emoji icon, TextSize size = TextSize.Small)
            {
                ReplaceIcon(I(_($"tss-diagram-node-icon ec {icon} {size}")));
                return this;
            }

            /// <summary>
            /// Sets an image (rendered as a small rounded icon) on the node.
            /// </summary>
            public Node SetImage(string source)
            {
                ReplaceIcon(Image(_("tss-diagram-node-image", src: source)));
                return this;
            }

            /// <summary>
            /// Removes the node's icon or image.
            /// </summary>
            public Node ClearIcon()
            {
                if (_iconElement is object)
                {
                    _iconElement.remove();
                    _iconElement = null;
                    UpdateIconOnlyState();
                }
                return this;
            }

            /// <summary>
            /// Styles the node using the default tone.
            /// </summary>
            public Node Default() => SetStyle("tss-diagram-node-default");

            /// <summary>
            /// Styles the node using the primary tone.
            /// </summary>
            public Node Primary() => SetStyle("tss-diagram-node-primary");

            /// <summary>
            /// Styles the node using the secondary tone.
            /// </summary>
            public Node Secondary() => SetStyle("tss-diagram-node-secondary");

            /// <summary>
            /// Styles the node using the success tone.
            /// </summary>
            public Node Success() => SetStyle("tss-diagram-node-success");

            /// <summary>
            /// Styles the node using the danger tone.
            /// </summary>
            public Node Danger() => SetStyle("tss-diagram-node-danger");

            /// <summary>
            /// Configures the node with arbitrary background, foreground and border colors.
            /// </summary>
            public Node Color(string background, string foreground = "", string borderColor = "")
            {
                _element.style.background = background;

                if (!string.IsNullOrEmpty(foreground))
                {
                    _element.style.color = foreground;
                }

                if (!string.IsNullOrEmpty(borderColor))
                {
                    _element.style.borderColor = borderColor;
                }
                return this;
            }

            /// <summary>
            /// Pins the node to an explicit position (in diagram coordinates), excluding it from the automatic layout.
            /// </summary>
            public Node At(double x, double y)
            {
                MoveTo(x, y);
                IsPinned = true;
                _owner?.RequestAutoArrange();
                return this;
            }

            /// <summary>
            /// Registers a callback invoked when the node is clicked (not fired after dragging).
            /// </summary>
            public Node OnClick(ComponentEventHandler<Node, MouseEvent> onClick)
            {
                Clicked += onClick;
                return this;
            }

            /// <summary>
            /// Registers a callback invoked when the node is clicked (not fired after dragging).
            /// </summary>
            public Node OnClick(Action action) => OnClick((_, e) =>
            {
                StopEvent(e);
                action.Invoke();
            });

            /// <summary>
            /// Registers a callback invoked when the context menu event fires on the node.
            /// </summary>
            public Node OnContextMenu(ComponentEventHandler<Node, MouseEvent> onContextMenu)
            {
                ContextMenu += onContextMenu;
                return this;
            }

            /// <summary>
            /// Registers a callback invoked when the context menu event fires on the node (suppresses the browser menu).
            /// </summary>
            public Node OnContextMenu(Action action) => OnContextMenu((_, e) =>
            {
                StopEvent(e);
                action.Invoke();
            });

            internal void AttachTo(Diagram owner)
            {
                _owner = owner;
            }

            internal void MoveTo(double x, double y)
            {
                X = x;
                Y = y;
                _element.style.left = $"{x}px";
                _element.style.top  = $"{y}px";
            }

            internal void Measure()
            {
                MeasuredWidth  = _element.offsetWidth;
                MeasuredHeight = _element.offsetHeight;
            }

            private Node SetStyle(string styleClass)
            {
                foreach (var cls in _styleClasses)
                {
                    _element.classList.remove(cls);
                }

                _element.classList.add(styleClass);
                return this;
            }

            private void ReplaceIcon(HTMLElement newIcon)
            {
                if (_iconElement is object)
                {
                    _iconElement.remove();
                }

                _iconElement = newIcon;
                _element.insertBefore(_iconElement, _textSpan);
                UpdateIconOnlyState();
                _owner?.RequestAutoArrange();
            }

            private void UpdateIconOnlyState()
            {
                _element.UpdateClassIf(string.IsNullOrEmpty(_textSpan.innerText) && _iconElement is object, "tss-diagram-node-icon-only");
            }

            private void HookDragEvents()
            {
                double startX = 0, startY = 0, origX = 0, origY = 0;
                bool   moved  = false;

                _element.onmousedown += (me) =>
                {
                    if (me.button != 0) return;

                    startX = me.clientX;
                    startY = me.clientY;
                    origX  = X;
                    origY  = Y;
                    moved  = false;

                    window.onmousemove += Drag;
                    window.onmouseup   += StopDrag;
                    StopEvent(me);
                };

                void Drag(MouseEvent me)
                {
                    var dx = me.clientX - startX;
                    var dy = me.clientY - startY;

                    if (!moved && (Math.Abs(dx) + Math.Abs(dy)) > 3)
                    {
                        moved    = true;
                        IsPinned = true;
                        _element.classList.add("tss-diagram-node-dragging");
                    }

                    if (moved)
                    {
                        MoveTo(origX + dx, origY + dy);
                        _owner?.OnNodeDragged();
                    }
                    StopEvent(me);
                }

                void StopDrag(MouseEvent me)
                {
                    window.onmousemove -= Drag;
                    window.onmouseup   -= StopDrag;
                    _element.classList.remove("tss-diagram-node-dragging");

                    if (moved)
                    {
                        //The click event fires right after mouseup - swallow it so dragging doesn't trigger OnClick
                        _suppressClick = true;
                        window.setTimeout((_) => _suppressClick = false, 100);
                    }
                    StopEvent(me);
                }
            }
        }
    }
}
