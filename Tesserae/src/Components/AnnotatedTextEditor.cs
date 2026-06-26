using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A multi-line text editor that supports inline annotations / highlights over user-entered prose.
    /// </summary>
    [H5.Name("tss.AnnotatedTextEditor")]
    public class AnnotatedTextEditor : IComponent, IHasBackgroundColor, ITabIndex
    {
        public class Entity
        {
            /// <summary>
            /// Starts the component's operation.
            /// </summary>
            public int Start { get; set; }
            /// <summary>
            /// Gets or sets the length.
            /// </summary>
            public int Length { get; set; }
            /// <summary>
            /// Rich content shown in the hover tag. Rendered into the floating tooltip the first
            /// time the entity is hovered and cached afterwards. Each entity should own a
            /// distinct component instance — an <see cref="IComponent"/>'s element can only live
            /// in one place at a time, so sharing one across entities silently re-parents its
            /// DOM on every hover. Entities with no content are not hover-able.
            /// </summary>
            public IComponent LabelContent { get; set; }
            /// <summary>
            /// Gets or sets the CSS background of the component.
            /// </summary>
            public string Background { get; set; }
            /// <summary>
            /// Gets or sets the color of the component.
            /// </summary>
            public string Color { get; set; }
            /// <summary>
            /// Gets or sets the CSS border of the component.
            /// </summary>
            public string Border { get; set; }

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public Entity() { }

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public Entity(int start, int length, IComponent labelContent = null, string background = null, string color = null, string border = null)
            {
                Start        = start;
                Length       = length;
                LabelContent = labelContent;
                Background   = background;
                Color        = color;
                Border       = border;
            }

            /// <summary>
            /// Convenience constructor for plain-string labels. The text is wrapped in a default
            /// tooltip-styled component (small, semi-bold, theme-aware foreground, with newline
            /// support) so callers that don't need rich content get the same look the editor had
            /// before <see cref="LabelContent"/> existed.
            /// </summary>
            public Entity(int start, int length, string label, string background = null, string color = null, string border = null)
                : this(start, length, DefaultStringLabel(label), background, color, border)
            {
            }

            /// <summary>
            /// Gets or sets the end.
            /// </summary>
            public int End => Start + Length;
        }

        /// <summary>
        /// Builds the default IComponent shown for a plain string label — a small, semi-bold,
        /// theme-foreground div with <c>pre-line</c> whitespace so embedded <c>\n</c> renders as
        /// real line breaks. Public so callers wanting the default look can compose it with
        /// other components.
        /// </summary>
        public static IComponent DefaultStringLabel(string text)
        {
            var el = Div(_("tss-annotated-text-default-label"));
            el.textContent = text ?? string.Empty;
            return new Raw(el);
        }

        private readonly HTMLDivElement      _container;
        private readonly HTMLDivElement      _highlights;
        private readonly HTMLDivElement      _highlightsContent;
        private readonly HTMLTextAreaElement _textarea;
        private readonly HTMLDivElement      _hoverTag;

        private readonly Func<string, Task<Entity[]>> _annotator;
        private readonly int                          _debounceMs;
        private          int                          _debounceTimeoutId   = 0;
        private          int                          _annotationRequestId = 0;
        private          Entity[]                     _currentEntities     = new Entity[0];
        // The text exactly as last supplied to us — programmatically via Text/initialText, or
        // echoed back from the textarea on user input. Entity offsets returned by the annotator
        // are relative to THIS string; the textarea stores a CRLF-normalized copy of it, so we
        // keep the raw text around to translate those offsets. See RemapToNormalized.
        private          string                       _rawText             = string.Empty;
        private readonly List<HTMLElement>            _entitySpans         = new List<HTMLElement>();
        // Rendered hover element per entity. Populated on first hover, cleared whenever the
        // entity set is replaced so we don't pin stale component trees in memory.
        private readonly Dictionary<Entity, HTMLElement> _labelContentCache = new Dictionary<Entity, HTMLElement>();

        public delegate void AnnotationsChangedHandler(AnnotatedTextEditor sender, Entity[] entities);
        public delegate void TextChangedHandler(AnnotatedTextEditor        sender, string   text);
        public delegate void EntityClickHandler(AnnotatedTextEditor        sender, Entity   entity, MouseEvent e);

        /// <summary>
        /// Raised when annotations changed occurs.
        /// </summary>
        public event AnnotationsChangedHandler AnnotationsChanged;
        /// <summary>
        /// Raised when text changed occurs.
        /// </summary>
        public event TextChangedHandler TextChanged;
        /// <summary>
        /// Raised when entity clicked occurs.
        /// </summary>
        public event EntityClickHandler EntityClicked;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public AnnotatedTextEditor(Func<string, Task<Entity[]>> annotator, string initialText = null, int debounceMs = 500, string placeholder = null)
        {
            _annotator  = annotator;
            _debounceMs = debounceMs;

            _highlightsContent   = Div(_("tss-annotated-text-highlights-content"));
            _highlights          = Div(_("tss-annotated-text-highlights"), _highlightsContent);
            _textarea            = TextArea(_("tss-annotated-text-input", placeholder: placeholder ?? ""));
            _textarea.spellcheck = false;

            _container = Div(_("tss-annotated-text-container"), _highlights, _textarea);

            _hoverTag               = Div(_("tss-annotated-text-hover-tag"));
            _hoverTag.style.display = "none";
            document.body.appendChild(_hoverTag);

            _textarea.addEventListener("input", (e) =>
            {
                // The textarea already hands us a newline-normalized value, so the raw text the
                // annotator runs on and the textarea value are identical for typed input.
                _rawText = _textarea.value;
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

            _textarea.addEventListener("mousemove",  (e) => HandleHoverMove(e.As<MouseEvent>()));
            _textarea.addEventListener("mouseleave", (e) => HideHoverTag());
            _textarea.addEventListener("blur",       (e) => HideHoverTag());

            _textarea.addEventListener("click", (e) =>
            {
                if (EntityClicked == null) return;
                var me     = e.As<MouseEvent>();
                var entity = FindEntityAt(me.clientX, me.clientY);
                if (entity != null) EntityClicked.Invoke(this, entity, me);
            });

            if (!string.IsNullOrEmpty(initialText))
            {
                _rawText        = initialText;
                _textarea.value = NormalizeNewlines(initialText);
            }

            RenderHighlights();
            TriggerAnnotate();
        }

        /// <summary>
        /// Gets or sets the text shown in the component.
        /// </summary>
        public string Text
        {
            get => _textarea.value;
            set
            {
                _rawText        = value ?? string.Empty;
                _textarea.value = NormalizeNewlines(value);
                HideHoverTag();
                TextChanged?.Invoke(this, _textarea.value);
                RenderHighlights();
                TriggerAnnotate();
            }
        }

        /// <summary>
        /// Gets or sets the entities.
        /// </summary>
        public Entity[] Entities => _currentEntities;

        /// <summary>
        /// Gets or sets the placeholder text shown when the component is empty.
        /// </summary>
        public string Placeholder
        {
            get => _textarea.placeholder;
            set => _textarea.placeholder = value;
        }

        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public string Background { get => _container.style.background; set => _container.style.background = value; }

        /// <summary>
        /// Sets the keyboard tab order of the component.
        /// </summary>
        public int TabIndex { set => _textarea.tabIndex = value; }

        /// <summary>
        /// Gets or sets a value indicating whether the component is interactive (enabled).
        /// </summary>
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

        /// <summary>
        /// Returns a value indicating whether the component is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get => _textarea.readOnly;
            set
            {
                _textarea.readOnly = value;

                if (value) _container.classList.add("tss-annotated-text-readonly");
                else _container.classList.remove("tss-annotated-text-readonly");
            }
        }

        /// <summary>
        /// Sets the text of the component.
        /// </summary>
        public AnnotatedTextEditor SetText(string text)
        {
            Text = text;
            return this;
        }
        /// <summary>
        /// Sets the placeholder of the component.
        /// </summary>
        public AnnotatedTextEditor SetPlaceholder(string placeholder)
        {
            Placeholder = placeholder;
            return this;
        }
        /// <summary>
        /// Disables the component.
        /// </summary>
        public AnnotatedTextEditor Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }
        /// <summary>
        /// Configures the read only on the component.
        /// </summary>
        public AnnotatedTextEditor ReadOnly(bool value = true)
        {
            IsReadOnly = value;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the text changed event fires.
        /// </summary>
        public AnnotatedTextEditor OnTextChanged(TextChangedHandler handler)
        {
            TextChanged += handler;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the annotations changed event fires.
        /// </summary>
        public AnnotatedTextEditor OnAnnotationsChanged(AnnotationsChangedHandler handler)
        {
            AnnotationsChanged += handler;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the entity click event fires.
        /// </summary>
        public AnnotatedTextEditor OnEntityClick(EntityClickHandler handler)
        {
            EntityClicked += handler;
            return this;
        }

        /// <summary>
        /// Gets or sets the CSS height of the component.
        /// </summary>
        public AnnotatedTextEditor Height(UnitSize unitSize)
        {
            var h = unitSize.ToString();
            _container.style.height = h;
            return this;
        }

        /// <summary>
        /// Shortcut for setting the height in pixels.
        /// </summary>
        public AnnotatedTextEditor H(int px) => Height(px.px());

        /// <summary>
        /// Gets or sets the CSS min-height of the component.
        /// </summary>
        public AnnotatedTextEditor MinHeight(UnitSize unitSize)
        {
            _container.style.minHeight = unitSize.ToString();
            return this;
        }

        /// <summary>
        /// Moves keyboard focus to the component.
        /// </summary>
        public AnnotatedTextEditor Focus()
        {
            DomObserver.WhenMounted(_textarea, () => _textarea.focus());
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _container;

        /// <summary>
        /// Configures the trigger annotate on the component.
        /// </summary>
        public void TriggerAnnotate()
        {
            if (_annotator == null) return;

            window.clearTimeout(_debounceTimeoutId);

            _debounceTimeoutId = (int)window.setTimeout(_ =>
            {
                Task.Run(async () =>
                {

                    var      requestId = ++_annotationRequestId;
                    // Run the annotator over the RAW text, not the textarea value: detection
                    // engines expect the original (un-normalized) string and return offsets into
                    // it. We translate those offsets back into the textarea's normalized space
                    // below so the highlights line up. For typed input raw == textarea value.
                    var      raw       = _rawText;
                    Entity[] entities;

                    try
                    {
                        entities = await _annotator(raw);
                    }
                    catch (Exception ex)
                    {
                        console.error("AnnotatedTextEditor: annotator threw", ex);
                        return;
                    }

                    if (requestId != _annotationRequestId) return; // stale

                    _currentEntities = RemapToNormalized(entities ?? new Entity[0], raw);
                    _labelContentCache.Clear();
                    HideHoverTag();
                    RenderHighlights();
                    AnnotationsChanged?.Invoke(this, _currentEntities);
                }).FireAndForget();
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

                    var end        = Math.Min(text.Length, entity.End);
                    var entityText = text.Substring(entity.Start, end - entity.Start);

                    var span                                                                 = Span(_("tss-annotated-text-entity"));
                    if (!string.IsNullOrEmpty(entity.Background)) span.style.backgroundColor = entity.Background;
                    if (!string.IsNullOrEmpty(entity.Border)) span.style.outlineColor        = entity.Border;
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

            if (entity == null || entity.LabelContent == null)
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
            if (!_labelContentCache.TryGetValue(entity, out var cached))
            {
                cached                     = entity.LabelContent.Render();
                _labelContentCache[entity] = cached;
            }

            _hoverTag.textContent = string.Empty;
            _hoverTag.appendChild(cached);
            _hoverTag.style.display = "block";

            // "transparent" on the entity is used to mute the inline highlight (e.g. for a
            // token that is hoverable but should look like plain text). The tooltip would be
            // unreadable if it inherited that, so explicitly substitute a safe fallback
            // (the same values used by the default .tss-annotated-text-hover-tag CSS rule)
            // instead of relying on the cascade to win, which is fragile to user style
            // overrides.
            _hoverTag.style.backgroundColor = ResolveHoverColor(entity.Background, "var(--tss-colors-blue-100)");
            _hoverTag.style.color           = ResolveHoverColor(entity.Color,      "var(--tss-colors-blue-900)");
            _hoverTag.style.borderColor     = ResolveHoverColor(entity.Border,     "var(--tss-colors-blue-500)");

            // Position near the cursor (slightly below-right). Use fixed positioning relative to viewport.
            var px = e.clientX + 12;
            var py = e.clientY + 18;

            // Keep inside viewport on the right edge
            var tagRect                                          = _hoverTag.getBoundingClientRect().As<DOMRect>();
            if (px + tagRect.width > window.innerWidth - 4) px   = window.innerWidth - tagRect.width - 4;
            if (py + tagRect.height > window.innerHeight - 4) py = e.clientY - tagRect.height - 8;

            _hoverTag.style.left = px + "px";
            _hoverTag.style.top  = py + "px";
        }

        private void HideHoverTag()
        {
            if (_hoverTag != null) _hoverTag.style.display = "none";
        }

        /// <summary>
        /// Collapses Windows ("\r\n") and old-Mac ("\r") line endings to "\n", matching the
        /// normalization a &lt;textarea&gt; applies to its value. Used so the string we hand the
        /// textarea and the string our offset map is built from agree on length and indices.
        /// </summary>
        private static string NormalizeNewlines(string text)
            => (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');

        /// <summary>
        /// Translates entity offsets that were computed against the raw text into offsets against
        /// the textarea's newline-normalized value.
        ///
        /// Token-detection engines run over the original, un-normalized text, so an entity's
        /// Start/Length index into a string that may still contain "\r\n". A &lt;textarea&gt;
        /// stores "\r\n" as a single "\n", which shifts every offset after a CRLF one column to
        /// the left per line break. We render against (and expose <see cref="Text"/> as) the
        /// normalized value, so raw offsets must be rewritten into normalized space or the
        /// highlights drift. A lone "\r" maps to "\n" without changing length, so only "\r\n"
        /// pairs actually shift anything — when none are present the map is the identity and the
        /// original entities are returned untouched.
        /// </summary>
        private static Entity[] RemapToNormalized(Entity[] entities, string raw)
        {
            if (entities == null || entities.Length == 0) return entities ?? new Entity[0];

            raw        = raw ?? string.Empty;
            int rawLen = raw.Length;

            // map[i] = index in the normalized string for raw index i. Length is rawLen + 1 so an
            // entity's End (exclusive) is always mappable.
            var map  = new int[rawLen + 1];
            int norm = 0;
            for (int i = 0; i < rawLen; i++)
            {
                map[i] = norm;
                if (raw[i] == '\r' && i + 1 < rawLen && raw[i + 1] == '\n') continue; // the '\r' of a CRLF is dropped
                norm++;
            }
            map[rawLen] = norm;

            if (norm == rawLen) return entities; // no "\r\n" removed anything — offsets already line up

            var result = new Entity[entities.Length];
            for (int i = 0; i < entities.Length; i++)
            {
                var e = entities[i];
                if (e == null) { result[i] = null; continue; }

                var start = Math.Max(0, Math.Min(e.Start, rawLen));
                var end   = Math.Max(start, Math.Min(e.End, rawLen));

                result[i] = new Entity(map[start], map[end] - map[start], e.LabelContent, e.Background, e.Color, e.Border);
            }
            return result;
        }

        private static bool IsTransparent(string color)
        {
            if (string.IsNullOrEmpty(color)) return false;
            var trimmed = color.Trim();
            if (string.Equals(trimmed, "transparent", StringComparison.OrdinalIgnoreCase)) return true;
            // rgba(..., 0) / rgb(..., 0) with a trailing zero alpha — rough check; covers the
            // most common "fully transparent" shorthand without parsing CSS.
            if (trimmed.EndsWith(",0)",  StringComparison.Ordinal)) return true;
            if (trimmed.EndsWith(", 0)", StringComparison.Ordinal)) return true;
            return false;
        }

        private static string ResolveHoverColor(string entityColor, string fallback)
        {
            if (string.IsNullOrEmpty(entityColor)) return fallback;
            if (IsTransparent(entityColor)) return fallback;
            return entityColor;
        }
    }
}