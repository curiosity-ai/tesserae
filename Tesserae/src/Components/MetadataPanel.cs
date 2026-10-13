using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The column of facts that sits beside a <see cref="Conversation"/> in an issue tracker - assignees, labels,
    /// milestone, participants - as a stack of <see cref="MetadataSection"/>s divided by hairlines. Each section has a
    /// small title, an optional edit (gear) button, and either its content or a muted "None yet" line.
    /// </summary>
    [Transpose.Name("tss.MetadataPanel")]
    public sealed class MetadataPanel : IComponent, IHasMarginPadding
    {
        private readonly HTMLElement           _root;
        private readonly List<MetadataSection> _sections = new List<MetadataSection>();

        /// <summary>Initializes a new, empty panel.</summary>
        public MetadataPanel()
        {
            _root = Div(Att("tss-metadata-panel"));
            _root.setAttribute("role", "complementary");
            _root.setAttribute("aria-label", "Details");
        }

        /// <inheritdoc />
        public string Margin  { get => _root.style.margin;  set => _root.style.margin  = value; }

        /// <inheritdoc />
        public string Padding { get => _root.style.padding; set => _root.style.padding = value; }

        /// <summary>The sections in the panel, top to bottom.</summary>
        public IReadOnlyList<MetadataSection> Sections => _sections;

        /// <summary>Appends a section.</summary>
        public MetadataPanel Add(MetadataSection section)
        {
            if (section is null) return this;
            _sections.Add(section);
            _root.appendChild(section.Render());
            return this;
        }

        /// <summary>Appends several sections.</summary>
        public MetadataPanel Children(params MetadataSection[] sections)
        {
            foreach (var s in sections) Add(s);
            return this;
        }

        /// <summary>Removes a section.</summary>
        public MetadataPanel Remove(MetadataSection section)
        {
            if (section is null || !_sections.Remove(section)) return this;
            _root.removeChild(section.Render());
            return this;
        }

        /// <summary>Whether the panel is drawn in its dense form (see <see cref="Compact"/>).</summary>
        public bool IsCompact
        {
            get => _root.classList.contains("tss-metadata-panel-compact");
            set => _root.UpdateClassIf(value, "tss-metadata-panel-compact");
        }

        /// <summary>
        /// Draws the panel densely: less space between and inside sections, a shorter header row and smaller avatars.
        /// </summary>
        public MetadataPanel Compact(bool value = true)
        {
            IsCompact = value;
            return this;
        }

        /// <summary>Sets the accessible name of the panel (<c>"Details"</c> by default).</summary>
        public MetadataPanel AriaLabel(string label)
        {
            _root.setAttribute("aria-label", label ?? string.Empty);
            return this;
        }

        /// <summary>Renders the component.</summary>
        public HTMLElement Render() => _root;
    }

    /// <summary>
    /// One section of a <see cref="MetadataPanel"/>: a small title, an optional edit button, and its content - people,
    /// label tags, a list of linked items, a row of avatars, or any component. While it has no content it shows its
    /// empty text instead.
    /// </summary>
    [Transpose.Name("tss.MetadataSection")]
    public sealed class MetadataSection : ComponentBase<MetadataSection, HTMLElement>
    {
        private readonly HTMLElement _header;
        private readonly HTMLElement _title;
        private readonly HTMLElement _editHost;
        private readonly HTMLElement _content;
        private readonly HTMLElement _empty;
        private          Button      _editButton;
        private          Action<IComponent> _onEdit;

        /// <summary>
        /// Initializes a new section.
        /// </summary>
        /// <param name="title">The section's title, e.g. <c>"Assignees"</c>.</param>
        /// <param name="emptyText">What it says while it has no content, e.g. <c>"No one assigned"</c>. Defaults to <c>"None yet"</c>.</param>
        public MetadataSection(string title, string emptyText = null)
        {
            _title    = Div(Att("tss-metadata-section-title", text: title ?? string.Empty));
            _editHost = Span(Att("tss-metadata-section-edit"));
            _header   = Div(Att("tss-metadata-section-header"), _title, _editHost);
            _content  = Div(Att("tss-metadata-section-content"));
            _empty    = Div(Att("tss-metadata-section-empty", text: emptyText ?? "None yet"));

            InnerElement = Div(Att("tss-metadata-section"), _header, _content, _empty);
            InnerElement.setAttribute("role", "group");
            InnerElement.setAttribute("aria-label", title ?? string.Empty);
            UpdateEmpty();
        }

        /// <summary>The section's title.</summary>
        public string Title => _title.textContent;

        /// <summary>Whether the section has no content (and shows its empty text).</summary>
        public bool IsEmpty => _content.childElementCount == 0;

        /// <summary>Sets the text shown while the section has no content.</summary>
        public MetadataSection Empty(string emptyText)
        {
            _empty.textContent = emptyText ?? string.Empty;
            return this;
        }

        /// <summary>Appends components to the section's content.</summary>
        public MetadataSection Content(params IComponent[] components)
        {
            foreach (var c in components)
            {
                if (c is object) _content.appendChild(c.Render());
            }
            UpdateEmpty();
            return this;
        }

        /// <summary>Replaces the section's content with these components (none leaves it empty).</summary>
        public MetadataSection SetContent(params IComponent[] components)
        {
            ClearChildren(_content);
            return Content(components);
        }

        /// <summary>Appends a line of plain text.</summary>
        public MetadataSection Text(string text)
        {
            _content.appendChild(Div(Att("tss-metadata-section-text", text: text ?? string.Empty)));
            UpdateEmpty();
            return this;
        }

        /// <summary>Appends a person - their <see cref="Avatar"/> and name on one line, like an assignee or a reviewer.</summary>
        /// <param name="name">The name shown.</param>
        /// <param name="avatar">Their avatar; defaults to one made from the name's initials.</param>
        /// <param name="secondary">Muted text after the name, e.g. a login or a review state.</param>
        public MetadataSection Person(string name, Avatar avatar = null, string secondary = null)
        {
            avatar = (avatar ?? new Avatar(initials: ConversationComment.InitialsOf(name))).Size(AvatarSize.XSmall);
            var row = Div(Att("tss-metadata-person"), avatar.Render(), Span(Att("tss-metadata-person-name", text: name ?? string.Empty)));
            if (!string.IsNullOrEmpty(secondary)) row.appendChild(Span(Att("tss-metadata-person-secondary", text: secondary)));
            _content.appendChild(row);
            UpdateEmpty();
            return this;
        }

        /// <summary>
        /// Appends a wrapping row of tags - typically <see cref="Tag"/>s tinted with <c>Tint(color)</c>, the way an
        /// issue's labels look.
        /// </summary>
        public MetadataSection Labels(params IComponent[] labels)
        {
            if (labels is null || labels.Length == 0) return this;
            var row = Div(Att("tss-metadata-labels"));
            foreach (var l in labels)
            {
                if (l is object) row.appendChild(l.Render());
            }
            _content.appendChild(row);
            UpdateEmpty();
            return this;
        }

        /// <summary>
        /// Appends a linked item: an icon, a title and an optional muted line under it - a milestone and its progress,
        /// a linked pull request and its repository, a project and its column.
        /// </summary>
        /// <param name="icon">The icon before the title.</param>
        /// <param name="title">The item's title.</param>
        /// <param name="subtitle">The muted line under the title.</param>
        /// <param name="iconColor">A CSS colour for the icon (e.g. <c>Theme.Success.Background</c> for an open pull request).</param>
        /// <param name="onClick">Called when the title is clicked; makes the title a link-styled button.</param>
        public MetadataSection Item(UIcons icon, string title, string subtitle = null, string iconColor = null, Action onClick = null)
        {
            var glyph = I(icon, cssClass: "tss-metadata-item-icon");
            if (!string.IsNullOrEmpty(iconColor)) glyph.style.color = iconColor;

            HTMLElement titleElement;
            if (onClick is object)
            {
                titleElement = Button(Att("tss-metadata-item-title tss-metadata-item-link", type: "button", text: title ?? string.Empty));
                titleElement.addEventListener("click", ev =>
                {
                    StopEvent(ev);
                    onClick();
                });
            }
            else
            {
                titleElement = Span(Att("tss-metadata-item-title", text: title ?? string.Empty));
            }

            var text = Div(Att("tss-metadata-item-text"), titleElement);
            if (!string.IsNullOrEmpty(subtitle)) text.appendChild(Span(Att("tss-metadata-item-subtitle", text: subtitle)));

            _content.appendChild(Div(Att("tss-metadata-item"), glyph, text));
            UpdateEmpty();
            return this;
        }

        /// <summary>
        /// Appends a row of avatars, like an issue's participants. At most <paramref name="max"/> are drawn; the rest
        /// are counted in a trailing "+N", where N is <paramref name="total"/> (when given) less the avatars drawn.
        /// </summary>
        public MetadataSection Avatars(IEnumerable<Avatar> avatars, int max = 10, int? total = null)
        {
            var row   = Div(Att("tss-metadata-avatars"));
            var shown = 0;
            var count = 0;

            foreach (var a in avatars)
            {
                if (a is null) continue;
                count++;
                if (shown >= max) continue;
                row.appendChild(a.Size(AvatarSize.XSmall).Render());
                shown++;
            }

            var rest = (total ?? count) - shown;
            if (rest > 0) row.appendChild(Span(Att("tss-metadata-avatars-more", text: $"+{rest}")));
            if (shown == 0 && rest <= 0) return this;

            _content.appendChild(row);
            UpdateEmpty();
            return this;
        }

        /// <summary>Removes the section's content, so it shows its empty text.</summary>
        public MetadataSection Clear()
        {
            ClearChildren(_content);
            UpdateEmpty();
            return this;
        }

        /// <summary>
        /// Shows a gear button in the section's header and calls <paramref name="onEdit"/> when it (or the title) is
        /// clicked. The argument is the button, to anchor a <see cref="ContextMenu"/> or a <see cref="Popover"/> to:
        /// <c>.OnEdit(anchor =&gt; menu.ShowFor(anchor))</c>.
        /// </summary>
        public MetadataSection OnEdit(Action<IComponent> onEdit)
        {
            _onEdit += onEdit;

            if (_editButton is null)
            {
                _editButton = new Button().SetIcon(UIcons.Settings).NoBorder().NoBackground().Compact().Tooltip($"Edit {Title}");
                _editButton.OnClick(() => _onEdit?.Invoke(_editButton));
                _editHost.appendChild(_editButton.Render());

                _title.classList.add("tss-metadata-section-title-editable");
                _title.addEventListener("click", ev =>
                {
                    StopEvent(ev);
                    _onEdit?.Invoke(_editButton);
                });
            }

            return this;
        }

        private void UpdateEmpty()
        {
            var empty = IsEmpty;
            _content.style.display = empty ? "none" : "";
            _empty.style.display   = empty ? "" : "none";
        }

        /// <summary>Renders the component.</summary>
        public override HTMLElement Render() => InnerElement;
    }
}
