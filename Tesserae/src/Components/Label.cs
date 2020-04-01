using System;
using static Tesserae.UI;
using static Retyped.dom;
using Tesserae.HTML;
using System.Collections.Generic;

namespace Tesserae.Components
{
    public class Label : TextBlock
    {
        private static int _labelForId = 0;

        private readonly HTMLLabelElement _label;
        private readonly HTMLDivElement _content;

        private static uint _callback;
        private static Dictionary<HTMLElement, Action> _pendingCallbacks = new Dictionary<HTMLElement, Action>();

        public Label(string text = string.Empty)
        {
            _label = Label(_("tss-fontsize-small tss-fontweight-semibold", text: text));
            _content = Div(_());
            InnerElement = Div(_("tss-label"), _label, _content);
        }

        public Label(IComponent component)
        {
            _label = Label(_("tss-fontsize-small tss-fontweight-semibold"), component.Render());
            _content = Div(_());
            InnerElement = Div(_("tss-label"), _label, _content);
        }


        public override string Text
        {
            get => _label.innerText;
            set => _label.innerText = value;
        }

        public override bool IsRequired
        {
            get => _label.classList.contains("tss-required");
            set
            {
                if (value)
                {
                    _label.classList.add("tss-required");
                }
                else
                {
                    _label.classList.remove("tss-required");
                }
            }
        }

        public bool IsInline
        {
            get => InnerElement.classList.contains("inline");
            set
            {
                if (value)
                {
                    InnerElement.classList.add("inline");
                }
                else
                {
                    InnerElement.classList.add("remove");
                }
            }
        }

        public IComponent Content
        {
            set
            {
                var id = string.Empty;
                ClearChildren(_content);
                if (value != null)
                {
                    _content.appendChild(value.Render());

                    if ((value as dynamic).InnerElement is HTMLInputElement el)
                    {
                        id = $"tss-label-for-{_labelForId}";
                        _labelForId++;
                        el.id = id;
                    }
                }
                _label.htmlFor = id;
            }
        }
        public Label SetContent( IComponent content)
        {
            Content = content;
            return this;
        }

        public Label Inline()
        {
            IsInline = true;
            return this;
        }

        public Label SetMinLabelWidth(UnitSize unitSize)
        {
            _label.style.minWidth = unitSize.ToString();
            return this;
        }

        public Label AutoWidth(int nestingLevels = 2)
        {
            _label.classList.add("tss-label-autowidth");
            DomObserver.WhenMounted(_label, () =>
            {
                HTMLElement parent = _label;
                int levels = nestingLevels;
                do
                {
                    parent = parent.parentElement;
                    if (!parent.classList.contains("tss-stack-item"))
                    {
                        levels--;
                    }
                    else
                    {
                        nestingLevels++;
                    }
                } while (levels > 0 && parent.parentElement is object);

                if(parent is object)
                {
                    _pendingCallbacks.TryAdd(parent, () => AutoSizeChildrenLabels(parent, nestingLevels));
                    window.cancelAnimationFrame(_callback);
                    _callback = window.requestAnimationFrame(_ => TriggerAll());
                }
            });

            return this;
        }

        private static void TriggerAll()
        {
            foreach(var kv in _pendingCallbacks)
            {
                kv.Value();
            }
            _pendingCallbacks.Clear();
        }

        private static void AutoSizeChildrenLabels(HTMLElement parent, int nestingLevels)
        {
            var found = new List<HTMLElement>();

            var stack = new Stack<HTMLElement>();
            stack.Push(parent);
            do
            {
                var new_stack = new Stack<HTMLElement>();
                while (stack.Count > 0)
                {
                    var el = stack.Pop();
                    foreach(HTMLElement e in el.children)
                    {
                        if(e.classList.contains("tss-label-autowidth"))
                        {
                            found.Add(e);
                        }
                        else
                        {
                            new_stack.Push(e);
                        }
                    }
                }
                stack = new_stack;

                nestingLevels--;
            } while (nestingLevels > 0);

            double minWidth = 10;
            foreach(var f in found)
            {
                var rect = (DOMRect)f.getBoundingClientRect();
                minWidth = Math.Max(minWidth, rect.width);
            }
            var mw = (minWidth + 4).px().ToString();
            foreach (var f in found)
            {
                f.style.minWidth = mw;
            }
        }
    }
}
