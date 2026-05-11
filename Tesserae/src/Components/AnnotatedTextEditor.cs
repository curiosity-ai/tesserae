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
        private readonly HTMLTextAreaElement _textarea;

        private readonly Func<string, Task<Entity[]>> _annotator;
        private readonly int                          _debounceMs;
        private int                                   _debounceTimeoutId   = 0;
        private int                                   _annotationRequestId = 0;
        private Entity[]                              _currentEntities     = new Entity[0];

        public delegate void AnnotationsChangedHandler(AnnotatedTextEditor sender, Entity[] entities);
        public delegate void TextChangedHandler(AnnotatedTextEditor sender, string text);

        public event AnnotationsChangedHandler AnnotationsChanged;
        public event TextChangedHandler        TextChanged;

        public AnnotatedTextEditor(Func<string, Task<Entity[]>> annotator, string initialText = null, int debounceMs = 500, string placeholder = null)
        {
            _annotator  = annotator;
            _debounceMs = debounceMs;

            _highlights = Div(_("tss-annotated-text-highlights"));
            _textarea   = TextArea(_("tss-annotated-text-input", placeholder: placeholder ?? ""));
            _textarea.spellcheck = false;

            _container = Div(_("tss-annotated-text-container"), _highlights, _textarea);

            _textarea.addEventListener("input", (e) =>
            {
                TextChanged?.Invoke(this, _textarea.value);
                RenderHighlights();
                TriggerAnnotate();
            });

            _textarea.addEventListener("scroll", (e) =>
            {
                _highlights.scrollTop  = _textarea.scrollTop;
                _highlights.scrollLeft = _textarea.scrollLeft;
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

        public AnnotatedTextEditor SetText(string text) { Text = text; return this; }
        public AnnotatedTextEditor SetPlaceholder(string placeholder) { Placeholder = placeholder; return this; }
        public AnnotatedTextEditor Disabled(bool value = true) { IsEnabled = !value; return this; }

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

        private void TriggerAnnotate()
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
                RenderHighlights();
                AnnotationsChanged?.Invoke(this, _currentEntities);
            }, _debounceMs);
        }

        private void RenderHighlights()
        {
            _highlights.innerHTML = "";

            var text = _textarea.value ?? string.Empty;

            if (_currentEntities == null || _currentEntities.Length == 0)
            {
                _highlights.appendChild(BuildTextNode(text));
                return;
            }

            var sorted = _currentEntities
                .Where(e => e != null && e.Length > 0 && e.Start >= 0 && e.Start < text.Length)
                .OrderBy(e => e.Start)
                .ToArray();

            int cursor = 0;
            foreach (var entity in sorted)
            {
                if (entity.Start < cursor) continue; // skip overlaps (caller guarantees no overlap, but be safe)

                if (entity.Start > cursor)
                {
                    _highlights.appendChild(BuildTextNode(text.Substring(cursor, entity.Start - cursor)));
                }

                var end = Math.Min(text.Length, entity.End);
                var entityText = text.Substring(entity.Start, end - entity.Start);

                var span = Span(_("tss-annotated-text-entity"));
                ApplyEntityStyles(span, entity);
                span.appendChild(document.createTextNode(entityText));

                if (!string.IsNullOrEmpty(entity.Label))
                {
                    var labelEl = Span(_("tss-annotated-text-entity-label"));
                    labelEl.textContent = entity.Label;
                    if (!string.IsNullOrEmpty(entity.Color))      labelEl.style.color           = entity.Color;
                    if (!string.IsNullOrEmpty(entity.Background)) labelEl.style.backgroundColor = entity.Background;
                    span.appendChild(labelEl);
                }

                _highlights.appendChild(span);

                cursor = end;
            }

            if (cursor < text.Length)
            {
                _highlights.appendChild(BuildTextNode(text.Substring(cursor)));
            }
        }

        private static void ApplyEntityStyles(HTMLSpanElement span, Entity entity)
        {
            if (!string.IsNullOrEmpty(entity.Background)) span.style.backgroundColor = entity.Background;
            if (!string.IsNullOrEmpty(entity.Color))      span.style.color           = entity.Color;
            if (!string.IsNullOrEmpty(entity.Border))     span.style.outlineColor    = entity.Border;
        }

        private static Node BuildTextNode(string text)
        {
            // Ensure trailing newline renders a line (textarea quirks)
            return document.createTextNode(text);
        }
    }
}
