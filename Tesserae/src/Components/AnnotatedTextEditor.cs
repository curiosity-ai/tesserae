using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.AnnotatedTextEditor")]
    public class AnnotatedTextEditor : IComponent, IHasBackgroundColor, ITabIndex
    {
        public class Entity
        {
            public int Start { get; set; }
            public int Length { get; set; }
            public string Label { get; set; }
            public string Background { get; set; }
            public string Color { get; set; }
            public string Border { get; set; }

            public Entity() { }

            public Entity(int start, int length, string label = null, string background = null, string color = null, string border = null)
            {
                Start = start;
                Length = length;
                Label = label;
                Background = background;
                Color = color;
                Border = border;
            }

            public int End => Start + Length;
        }

        private readonly HTMLDivElement      _container;
        private readonly HTMLDivElement      _highlights;
        private readonly HTMLDivElement      _highlightsContent;
        private readonly HTMLTextAreaElement _textarea;
        private readonly HTMLDivElement      _hoverTag;

        private readonly Func<string, Task<Entity[]>> _annotator;
        private readonly int                          _debounceMs;
        private int                                   _debounceTimeoutId   = 0;
        private int                                   _annotationRequestId = 0;
        private Entity[]                              _currentEntities     = new Entity[0];
        private readonly List<HTMLElement>            _entitySpans         = new List<HTMLElement>();

        public delegate void AnnotationsChangedHandler(AnnotatedTextEditor sender, Entity[] entities);
        public delegate void TextChangedHandler(AnnotatedTextEditor sender, string text);
        public delegate void EntityClickHandler(AnnotatedTextEditor sender, Entity entity, MouseEvent e);

        public event AnnotationsChangedHandler AnnotationsChanged;
        public event TextChangedHandler        TextChanged;
        public event EntityClickHandler        EntityClicked;

        public AnnotatedTextEditor(Func<string, Task<Entity[]>> annotator, string initialText = null, int debounceMs = 500, string placeholder = null)
        {
            _annotator  = annotator;
            _debounceMs = debounceMs;

            _highlightsContent = Div(_("tss-annotated-text-highlights-content"));
            _highlights        = Div(_("tss-annotated-text-highlights"), _highlightsContent);
            _textarea   = TextArea(_("tss-annotated-text-input", placeholder: placeholder ?? ""));
            _textarea.spellcheck = false;

            _container = Div(_("tss-annotated-text-container"), _highlights, _textarea);

            _hoverTag = Div(_("tss-annotated-text-hover-tag"));
            _hoverTag.style.display = "none";
            document.body.appendChild(_hoverTag);

            _textarea.addEventListener("input", (e) =>
            {
                HideHoverTag();
                TextChanged?.Invoke(this, _textarea.value);
                RenderHighlights();
                TriggerAnnotate();
            });

            _textarea.addEventListener("scroll", (e) =>
            {
                SyncHighlightsScroll();
                HideHoverTag();
            });

            _textarea.addEventListener("mousemove", (e) => HandleHoverMove(e.As<MouseEvent>()));
            _textarea.addEventListener("mouseleave", (e) => HideHoverTag());
            _textarea.addEventListener("blur", (e) => HideHoverTag());

            _textarea.addEventListener("click", (e) =>
            {
                if (EntityClicked == null) return;
                var me = e.As<MouseEvent>();
                var entity = FindEntityAt(me.clientX, me.clientY);
                if (entity != null) EntityClicked.Invoke(this, entity, me);
            });

            if (!string.IsNullOrEmpty(initialText))
            {
                _textarea.value = initialText;
            }

            RenderHighlights();
            TriggerAnnotate();
        }

        public string Text
        {
            get => _textarea.value;
            set
            {
                _textarea.value = value ?? string.Empty;
                HideHoverTag();
                TextChanged?.Invoke(this, _textarea.value);
                RenderHighlights();
                TriggerAnnotate();
            }
        }

        public Entity[] Entities => _currentEntities;

        public string Placeholder
        {
            get => _textarea.placeholder;
            set => _textarea.placeholder = value;
        }

        public string Background { get => _container.style.background; set => _container.style.background = value; }

        public int TabIndex { set => _textarea.tabIndex = value; }

        public bool IsEnabled
        {
            get => !_container.classList.contains("tss-disabled");
            set
            {
                if (value)
                {
                    _container.classList.remove("tss-disabled");
                    _textarea.disabled = false;
                }
                else
                {
                    _container.classList.add("tss-disabled");
                    _textarea.disabled = true;
                }
            }
        }

        public bool IsReadOnly
        {
            get => _textarea.readOnly;
            set
            {
                _textarea.readOnly = value;
                if (value) _container.classList.add("tss-annotated-text-readonly");
                else       _container.classList.remove("tss-annotated-text-readonly");
            }
        }

        public AnnotatedTextEditor SetText(string text) { Text = text; return this; }
        public AnnotatedTextEditor SetPlaceholder(string placeholder) { Placeholder = placeholder; return this; }
        public AnnotatedTextEditor Disabled(bool value = true) { IsEnabled = !value; return this; }
        public AnnotatedTextEditor ReadOnly(bool value = true) { IsReadOnly = value; return this; }

        public AnnotatedTextEditor OnTextChanged(TextChangedHandler handler)
        {
            TextChanged += handler;
            return this;
        }

        public AnnotatedTextEditor OnAnnotationsChanged(AnnotationsChangedHandler handler)
        {
            AnnotationsChanged += handler;
            return this;
        }

        public AnnotatedTextEditor OnEntityClick(EntityClickHandler handler)
        {
            EntityClicked += handler;
            return this;
        }

        public AnnotatedTextEditor Height(UnitSize unitSize)
        {
            var h = unitSize.ToString();
            _container.style.height = h;
            return this;
        }

        public AnnotatedTextEditor H(int px) => Height(px.px());

        public AnnotatedTextEditor MinHeight(UnitSize unitSize)
        {
            _container.style.minHeight = unitSize.ToString();
            return this;
        }

        public AnnotatedTextEditor Focus()
        {
            DomObserver.WhenMounted(_textarea, () => _textarea.focus());
            return this;
        }

        public HTMLElement Render() => _container;

        public void TriggerAnnotate()
        {
            if (_annotator == null) return;

            window.clearTimeout(_debounceTimeoutId);
            _debounceTimeoutId = (int)window.setTimeout(async _ =>
            {
                var requestId = ++_annotationRequestId;
                var text      = _textarea.value;
                Entity[] entities;
                try
                {
                    entities = await _annotator(text);
                }
                catch (Exception ex)
                {
                    console.error("AnnotatedTextEditor: annotator threw", ex);
                    return;
                }

                if (requestId != _annotationRequestId) return; // stale

                _currentEntities = entities ?? new Entity[0];
                HideHoverTag();
                RenderHighlights();
                AnnotationsChanged?.Invoke(this, _currentEntities);
            }, _debounceMs);
        }

        private void RenderHighlights()
        {
            _highlightsContent.innerHTML = "";
            _entitySpans.Clear();

            var text = _textarea.value ?? string.Empty;

            if (_currentEntities == null || _currentEntities.Length == 0)
            {
                _highlightsContent.appendChild(document.createTextNode(text));
            }
            else
            {
                var sorted = _currentEntities
                    .Where(e => e != null && e.Length > 0 && e.Start >= 0 && e.Start < text.Length)
                    .OrderBy(e => e.Start)
                    .ToArray();

                int cursor = 0;
                foreach (var entity in sorted)
                {
                    if (entity.Start < cursor) continue; // caller guarantees no overlap, but be safe

                    if (entity.Start > cursor)
                    {
                        _highlightsContent.appendChild(document.createTextNode(text.Substring(cursor, entity.Start - cursor)));
                    }

                    var end = Math.Min(text.Length, entity.End);
                    var entityText = text.Substring(entity.Start, end - entity.Start);

                    var span = Span(_("tss-annotated-text-entity"));
                    if (!string.IsNullOrEmpty(entity.Background)) span.style.backgroundColor = entity.Background;
                    if (!string.IsNullOrEmpty(entity.Border))     span.style.outlineColor    = entity.Border;
                    span.appendChild(document.createTextNode(entityText));

                    _highlightsContent.appendChild(span);
                    _entitySpans.Add(span);

                    cursor = end;
                }

                if (cursor < text.Length)
                {
                    _highlightsContent.appendChild(document.createTextNode(text.Substring(cursor)));
                }
            }

            SyncHighlightsScroll();
        }

        private void SyncHighlightsScroll()
        {
            // Translate the inner content instead of setting scrollTop on the
            // highlights div. scrollTop sync is fragile: any mismatch in
            // scrollHeight between the textarea (which reserves a trailing
            // caret line) and the highlights div causes the browser to clamp
            // the assignment, so the overlay drifts behind during fast/inertial
            // (macOS touchpad) scrolling. Transform is paint-only and never
            // clamped, so the overlay always tracks the textarea exactly.
            var x = -_textarea.scrollLeft;
            var y = -_textarea.scrollTop;
            _highlightsContent.style.transform = "translate(" + x + "px, " + y + "px)";
        }

        private void HandleHoverMove(MouseEvent e)
        {
            var entity = FindEntityAt(e.clientX, e.clientY);
            if (entity == null || string.IsNullOrEmpty(entity.Label))
            {
                HideHoverTag();
                return;
            }
            ShowHoverTag(entity, e);
        }

        private Entity FindEntityAt(double x, double y)
        {
            if (_currentEntities == null || _currentEntities.Length == 0 || _entitySpans.Count == 0) return null;

            for (int i = 0; i < _entitySpans.Count && i < _currentEntities.Length; i++)
            {
                var rect = _entitySpans[i].getBoundingClientRect().As<DOMRect>();
                if (x >= rect.left && x <= rect.right && y >= rect.top && y <= rect.bottom)
                {
                    return _currentEntities[i];
                }
            }
            return null;
        }

        private void ShowHoverTag(Entity entity, MouseEvent e)
        {
            _hoverTag.textContent = entity.Label;
            _hoverTag.style.display = "block";

            if (!string.IsNullOrEmpty(entity.Background)) _hoverTag.style.backgroundColor = entity.Background;
            else                                          _hoverTag.style.backgroundColor = "";
            if (!string.IsNullOrEmpty(entity.Color))      _hoverTag.style.color           = entity.Color;
            else                                          _hoverTag.style.color           = "";
            if (!string.IsNullOrEmpty(entity.Border))     _hoverTag.style.borderColor     = entity.Border;
            else                                          _hoverTag.style.borderColor     = "";

            // Position near the cursor (slightly below-right). Use fixed positioning relative to viewport.
            var px = e.clientX + 12;
            var py = e.clientY + 18;

            // Keep inside viewport on the right edge
            var tagRect = _hoverTag.getBoundingClientRect().As<DOMRect>();
            if (px + tagRect.width > window.innerWidth - 4) px = window.innerWidth - tagRect.width - 4;
            if (py + tagRect.height > window.innerHeight - 4) py = e.clientY - tagRect.height - 8;

            _hoverTag.style.left = px + "px";
            _hoverTag.style.top  = py + "px";
        }

        private void HideHoverTag()
        {
            if (_hoverTag != null) _hoverTag.style.display = "none";
        }
    }
}
