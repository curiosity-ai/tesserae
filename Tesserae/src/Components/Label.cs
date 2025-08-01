using H5;
using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Label")]
    public sealed class Label : TextBlock, ISpecialCaseStyling
    {
        private static int _labelForId = 0;

        private readonly HTMLLabelElement _label;
        private readonly HTMLDivElement   _content;

        private static          uint                            _callback;
        private static readonly Dictionary<HTMLElement, Action> _pendingCallbacks = new Dictionary<HTMLElement, Action>();

        public Label(string text = string.Empty)
        {
            _label       = Label(_("tss-fontsize-small tss-fontweight-semibold tss-fontcolor-default", text: text));
            _content     = Div(_("tss-label-content"));
            InnerElement = Div(_("tss-label tss-default-component-margin"), _label, _content);
        }

        public Label(IComponent component)
        {
            _label       = Label(_("tss-fontsize-small tss-fontweight-semibold tss-fontcolor-default"), component.Render());
            _content     = Div(_("tss-label-content"));
            InnerElement = Div(_("tss-label tss-default-component-margin"), _label, _content);
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
            get => InnerElement.classList.contains("tss-inline");
            set
            {
                if (value)
                {
                    InnerElement.classList.add("tss-inline");
                }
                else
                {
                    InnerElement.classList.remove("tss-inline");
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

                    if (!(value is Toggle) && (value as dynamic).InnerElement is HTMLInputElement el)
                    {
                        id = $"tss-label-for-{_labelForId}";
                        _labelForId++;
                        el.id = id;
                    }
                }
                _label.htmlFor = id;
            }
        }

        HTMLElement ISpecialCaseStyling.StylingContainer           => InnerElement;
        bool ISpecialCaseStyling.       PropagateToStackItemParent => true;

        public Label SetContent(IComponent content)
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

        public Label WithContentMargin()
        {
            InnerElement.classList.add("tss-label-with-content-margin");
            _content.classList.add("tss-default-component-margin");
            return this;
        }

        public Label AlignLabelTop()
        {
            _label.style.alignSelf = "flex-start";
            return this;
        }

        public Label AutoWidth(string parentSelector = null, bool alignRight = false)
        {
            _label.classList.add("tss-label-autowidth");

            if (alignRight)
            {
                _label.classList.add("tss-textalign-right");
            }

            DomObserver.WhenMounted(InnerElement, () =>
            {
                HTMLElement parent = string.IsNullOrEmpty(parentSelector) ? InnerElement.parentElement.parentElement : document.querySelector(parentSelector).As<HTMLElement>();

                if (parent is object)
                {
                    _pendingCallbacks.TryAdd(parent, () => AutoSizeChildrenLabels(parent));
                    window.cancelAnimationFrame(_callback);
                    _callback = window.requestAnimationFrame(_ => TriggerAll());
                }
            });

            return this;
        }
        public Label AutoWidth(IComponent parentElement, bool alignRight = false)
        {
            _label.classList.add("tss-label-autowidth");

            if (alignRight)
            {
                _label.classList.add("tss-textalign-right");
            }

            DomObserver.WhenMounted(InnerElement, () =>
            {
                var parent = parentElement.Render();

                if (parent is object)
                {
                    _pendingCallbacks.TryAdd(parent, () => AutoSizeChildrenLabels(parent));
                    window.cancelAnimationFrame(_callback);
                    _callback = window.requestAnimationFrame(_ => TriggerAll());
                }
            });

            return this;
        }

        private static void TriggerAll()
        {
            foreach (var kv in _pendingCallbacks)
            {
                kv.Value();
            }
            _pendingCallbacks.Clear();
        }

        private static void AutoSizeChildrenLabels(HTMLElement parent)
        {
            var found = new List<HTMLElement>();

            foreach(HTMLElement childLabel in parent.querySelectorAll(".tss-label-autowidth"))
            {
                if (childLabel.parentElement.classList.contains("tss-inline"))
                {
                    found.Add(childLabel);
                }
            }

            if (found.Count == 1)
            {
                found.First().classList.remove("tss-textalign-right");
            }
            else
            {
                int minWidth = 10;

                foreach (var f in found)
                {
                    var rect = f.getBoundingClientRect().As<DOMRect>();
                    minWidth = Math.Max(minWidth, Math.Max((int)rect.width, f.offsetWidth));
                }

                var mw = Script.Write<string>("({0}) + 'px'", minWidth + 4);

                foreach (var f in found)
                {
                    f.style.minWidth = mw;
                }
            }
        }
    }
}