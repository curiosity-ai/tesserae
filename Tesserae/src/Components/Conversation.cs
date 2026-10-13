using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A discussion thread in the shape of an issue tracker's conversation: a column of
    /// <see cref="ConversationComment"/> cards joined by a vertical rail, with <see cref="ConversationEvent"/>
    /// rows for what happened in between (labels added, someone assigned, the thread closed) and an optional
    /// <see cref="ConversationComposer"/> at the foot. Any other <see cref="IComponent"/> can be added as an item
    /// too, and sits on the rail like the rest.
    /// </summary>
    /// <remarks>
    /// Pair it with a <see cref="MetadataPanel"/> beside it for the assignees / labels / milestone column.
    /// </remarks>
    [Transpose.Name("tss.Conversation")]
    public sealed class Conversation : IComponent, IHasMarginPadding
    {
        private readonly HTMLElement      _root;
        private readonly HTMLElement      _items;
        private readonly HTMLElement      _footer;
        private readonly List<IComponent>  _children = new List<IComponent>();
        private readonly List<HTMLElement> _wrappers = new List<HTMLElement>();

        /// <summary>Initializes a new, empty conversation.</summary>
        public Conversation()
        {
            _items  = Div(Att("tss-conversation-items"));
            _items.setAttribute("role", "list");
            _footer = Div(Att("tss-conversation-footer"));
            _root   = Div(Att("tss-conversation"), _items, _footer);
        }

        /// <inheritdoc />
        public string Margin  { get => _root.style.margin;  set => _root.style.margin  = value; }

        /// <inheritdoc />
        public string Padding { get => _root.style.padding; set => _root.style.padding = value; }

        /// <summary>The items in the conversation, in the order they were added.</summary>
        public IReadOnlyList<IComponent> Items => _children;

        /// <summary>Appends an item - a comment, an event or any other component - to the end of the thread.</summary>
        public Conversation Add(IComponent item)
        {
            if (item is null) return this;
            var wrapper = Wrap(item);
            _children.Add(item);
            _wrappers.Add(wrapper);
            _items.appendChild(wrapper);
            return this;
        }

        /// <summary>Appends several items to the end of the thread.</summary>
        public Conversation Children(params IComponent[] items)
        {
            foreach (var item in items) Add(item);
            return this;
        }

        /// <summary>Removes an item from the thread.</summary>
        public Conversation Remove(IComponent item)
        {
            var index = _children.IndexOf(item);
            if (index < 0) return this;
            _items.removeChild(_wrappers[index]);
            _children.RemoveAt(index);
            _wrappers.RemoveAt(index);
            return this;
        }

        /// <summary>Removes every item from the thread. The composer stays.</summary>
        public Conversation Clear()
        {
            _children.Clear();
            _wrappers.Clear();
            ClearChildren(_items);
            return this;
        }

        /// <summary>
        /// Sets what sits under the last item, off the rail: usually a <see cref="ConversationComposer"/>. Pass
        /// <c>null</c> to remove it.
        /// </summary>
        public Conversation Composer(IComponent composer)
        {
            ClearChildren(_footer);
            if (composer is object) _footer.appendChild(composer.Render());
            return this;
        }

        // Each item gets a list-item element of its own so the rail can be drawn between items (and stop at the
        // last one) without asking anything of the component - a consumer's own item gets the rail for free.
        private static HTMLElement Wrap(IComponent item)
        {
            var element = item.Render();
            var kind    = element.classList.contains("tss-conversation-event") ? "tss-conversation-item tss-conversation-item-event" : "tss-conversation-item";
            var wrapper = Div(Att(kind), element);
            wrapper.setAttribute("role", "listitem");
            return wrapper;
        }

        /// <summary>Renders the component.</summary>
        public HTMLElement Render() => _root;
    }

    /// <summary>
    /// One comment in a <see cref="Conversation"/>: a card whose header names the author (with their
    /// <see cref="Avatar"/>), what they did and when, carries their role badges and an overflow menu, and whose
    /// body is Markdown (rendered by a <see cref="MarkdownBlock"/>) or any component. Images or other
    /// attachments go under the body, and emoji reactions in a row at the foot.
    /// </summary>
    [Transpose.Name("tss.ConversationComment")]
    public sealed class ConversationComment : ComponentBase<ConversationComment, HTMLElement>
    {
        /// <summary>The reactions offered by the add-reaction picker, unless <see cref="ReactionChoices"/> changes them.</summary>
        public static readonly string[] DefaultReactions = { "👍", "👎", "😄", "🎉", "😕", "❤️", "🚀", "👀" };

        private readonly HTMLElement     _header;
        private readonly HTMLElement     _avatarHost;
        private readonly HTMLElement     _author;
        private readonly HTMLElement     _action;
        private readonly HTMLElement     _time;
        private readonly HTMLElement     _edited;
        private readonly HTMLElement     _roles;
        private readonly HTMLElement     _menuHost;
        private readonly HTMLElement     _body;
        private readonly HTMLElement     _attachments;
        private readonly HTMLElement     _reactions;
        private readonly HTMLElement     _reactionList;
        private readonly HTMLElement     _reactionAdd;

        private readonly List<ReactionState> _reactionItems = new List<ReactionState>();
        private          string[]        _reactionChoices = DefaultReactions;
        private          Action<ConversationComment, string, bool> _onReact;
        private          MarkdownBlock   _markdown;

        private sealed class ReactionState
        {
            public string            Emoji;
            public int               Count;
            public bool              Reacted;
            public HTMLButtonElement Element;
        }

        /// <summary>
        /// Initializes a new comment.
        /// </summary>
        /// <param name="author">The name shown in bold at the start of the header.</param>
        /// <param name="markdown">The body, as Markdown. Pass <c>null</c> to set a component body later with <see cref="Body(IComponent)"/>.</param>
        /// <param name="timestamp">When it was posted, as display text (e.g. <c>"on Jul 22"</c>, <c>"3 days ago"</c>).</param>
        /// <param name="avatar">The author's avatar. Defaults to one made from the author's initials.</param>
        public ConversationComment(string author, string markdown = null, string timestamp = null, Avatar avatar = null)
        {
            _avatarHost   = Span(Att("tss-conversation-comment-avatar"));
            _author       = Span(Att("tss-conversation-comment-author", text: author ?? string.Empty));
            _action       = Span(Att("tss-conversation-comment-action", text: "commented"));
            _time         = Span(Att("tss-conversation-comment-time"));
            _edited       = Span(Att("tss-conversation-comment-edited", text: "edited"));
            _roles        = Span(Att("tss-conversation-comment-roles"));
            _menuHost     = Span(Att("tss-conversation-comment-menu"));

            var meta      = Span(Att("tss-conversation-comment-meta"), _author, _action, _time, _edited);
            _header       = Div(Att("tss-conversation-comment-header"), _avatarHost, meta, _roles, _menuHost);

            _body         = Div(Att("tss-conversation-comment-body"));
            _attachments  = Div(Att("tss-conversation-comment-attachments"));
            _reactionList = Span(Att("tss-conversation-reaction-list"));
            _reactionAdd  = Span(Att("tss-conversation-reaction-add"));
            _reactions    = Div(Att("tss-conversation-reactions"), _reactionList, _reactionAdd);

            InnerElement  = Div(Att("tss-conversation-comment"), _header, _body, _attachments, _reactions);
            InnerElement.setAttribute("role", "article");

            SetAvatar(avatar ?? new Avatar(initials: InitialsOf(author)));
            Timestamp(timestamp);
            Edited(false);
            UpdateOptionalParts();

            if (markdown is object) Body(markdown);
        }

        /// <summary>The author's name.</summary>
        public string Author => _author.textContent;

        /// <summary>The Markdown body, or <c>null</c> when the body is a component.</summary>
        public string Markdown => _markdown?.Text;

        /// <summary>Sets the author's name.</summary>
        public ConversationComment SetAuthor(string author)
        {
            _author.textContent = author ?? string.Empty;
            UpdateAriaLabel();
            return this;
        }

        /// <summary>Sets the avatar shown at the start of the header. It is drawn at <see cref="AvatarSize.XSmall"/>.</summary>
        public ConversationComment SetAvatar(Avatar avatar)
        {
            ClearChildren(_avatarHost);
            if (avatar is object) _avatarHost.appendChild(avatar.Size(AvatarSize.XSmall).Render());
            return this;
        }

        /// <summary>
        /// Sets the verb between the author and the time - <c>"commented"</c> by default, <c>"opened"</c> for the first
        /// post of an issue, <c>"replied"</c>, ...
        /// </summary>
        public ConversationComment Action(string action)
        {
            _action.textContent = action ?? string.Empty;
            return this;
        }

        /// <summary>Sets when the comment was posted, as display text, with an optional tooltip (typically the full date and time).</summary>
        public ConversationComment Timestamp(string text, string tooltip = null)
        {
            _time.textContent   = text ?? string.Empty;
            _time.style.display = string.IsNullOrEmpty(text) ? "none" : "";
            if (string.IsNullOrEmpty(tooltip)) _time.removeAttribute("title");
            else _time.setAttribute("title", tooltip);
            UpdateAriaLabel();
            return this;
        }

        /// <summary>Shows (or hides) the "edited" marker after the time, with an optional tooltip such as who edited it and when.</summary>
        public ConversationComment Edited(bool edited = true, string tooltip = null)
        {
            _edited.style.display = edited ? "" : "none";
            if (string.IsNullOrEmpty(tooltip)) _edited.removeAttribute("title");
            else _edited.setAttribute("title", tooltip);
            return this;
        }

        /// <summary>
        /// Adds an outlined role badge to the right of the header - <c>"Author"</c>, <c>"Member"</c>,
        /// <c>"Contributor"</c>, <c>"Bot"</c>, ...
        /// </summary>
        public ConversationComment Role(string role)
        {
            if (string.IsNullOrEmpty(role)) return this;
            _roles.appendChild(new Badge(role).Outline().Pill().Render());
            return this;
        }

        /// <summary>Removes every role badge.</summary>
        public ConversationComment ClearRoles()
        {
            ClearChildren(_roles);
            return this;
        }

        /// <summary>
        /// Gives the header an overflow ("…") button that opens a menu with these items - copy link, quote reply,
        /// edit, delete, ...
        /// </summary>
        public ConversationComment Menu(params ContextMenu.Item[] items)
        {
            ClearChildren(_menuHost);
            if (items is null || items.Length == 0) return this;

            var button = new Button().SetIcon(UIcons.MenuDots).NoBorder().NoBackground().Compact().Tooltip("Comment actions");
            button.OnClick(() => new ContextMenu().Items(items).ShowFor(button));
            _menuHost.appendChild(button.Render());
            return this;
        }

        /// <summary>Sets the body to Markdown, rendered and sanitized by a <see cref="MarkdownBlock"/>.</summary>
        public ConversationComment Body(string markdown, MarkdownSanitization sanitization = MarkdownSanitization.Default)
        {
            _markdown = new MarkdownBlock(markdown ?? string.Empty, sanitization).TableCopyButtons(false);
            ClearChildren(_body);
            _body.appendChild(PlainWhitespace(_markdown.Render()));
            return this;
        }

        /// <summary>Sets the body to any component.</summary>
        public ConversationComment Body(IComponent content)
        {
            _markdown = null;
            ClearChildren(_body);
            if (content is object) _body.appendChild(content.Render());
            return this;
        }

        /// <summary>
        /// Adds an attachment under the body - an <see cref="Image"/> (a screenshot), a file card, anything. Several
        /// attachments wrap in a row.
        /// </summary>
        public ConversationComment Attachment(IComponent attachment)
        {
            if (attachment is null) return this;
            _attachments.appendChild(Div(Att("tss-conversation-comment-attachment"), attachment.Render()));
            UpdateOptionalParts();
            return this;
        }

        /// <summary>
        /// Adds (or updates) a reaction chip at the foot of the comment. <paramref name="reacted"/> marks the chip as
        /// one the current user has added. A chip whose count reaches zero is removed.
        /// </summary>
        public ConversationComment Reaction(string emoji, int count, bool reacted = false)
        {
            if (string.IsNullOrEmpty(emoji)) return this;

            var existing = Find(emoji);

            if (count <= 0)
            {
                if (existing is object)
                {
                    _reactionList.removeChild(existing.Element);
                    _reactionItems.Remove(existing);
                    UpdateOptionalParts();
                }
                return this;
            }

            if (existing is null)
            {
                existing = new ReactionState { Emoji = emoji };
                existing.Element = Button(Att("tss-conversation-reaction", type: "button"));
                var r = existing;
                existing.Element.addEventListener("click", ev =>
                {
                    StopEvent(ev);
                    Toggle(r.Emoji);
                });
                _reactionItems.Add(existing);
                _reactionList.appendChild(existing.Element);
            }

            existing.Count   = count;
            existing.Reacted = reacted;
            RenderReaction(existing);
            UpdateOptionalParts();
            return this;
        }

        /// <summary>
        /// Registers a callback for when the current user adds or removes a reaction, by clicking a chip or picking one
        /// from the add-reaction button (which this also shows). The arguments are the comment, the emoji and whether
        /// it is now added. The chip has already been updated when it is called.
        /// </summary>
        public ConversationComment OnReact(Action<ConversationComment, string, bool> onReact)
        {
            _onReact += onReact;
            EnsureReactionPicker();
            return this;
        }

        /// <summary>Sets the emoji the add-reaction button offers. Defaults to <see cref="DefaultReactions"/>.</summary>
        public ConversationComment ReactionChoices(params string[] emojis)
        {
            _reactionChoices = emojis is object && emojis.Length > 0 ? emojis : DefaultReactions;
            return this;
        }

        /// <summary>The count of a reaction, and whether the current user is one of those who added it.</summary>
        public (int count, bool reacted) GetReaction(string emoji)
        {
            var r = Find(emoji);
            return r is null ? (0, false) : (r.Count, r.Reacted);
        }

        /// <summary>
        /// Marks the comment as highlighted - a primary border, as for the comment a link points to, or the one a
        /// reader is replying to.
        /// </summary>
        public ConversationComment Highlighted(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-conversation-comment-highlighted");
            return this;
        }

        private ReactionState Find(string emoji)
        {
            foreach (var r in _reactionItems)
            {
                if (r.Emoji == emoji) return r;
            }
            return null;
        }

        private void Toggle(string emoji)
        {
            var r       = Find(emoji);
            var reacted = !(r?.Reacted ?? false);
            var count   = (r?.Count ?? 0) + (reacted ? 1 : -1);
            Reaction(emoji, count, reacted);
            _onReact?.Invoke(this, emoji, reacted);
        }

        private static void RenderReaction(ReactionState r)
        {
            ClearChildren(r.Element);
            r.Element.appendChild(Span(Att("tss-conversation-reaction-emoji", text: r.Emoji)));
            r.Element.appendChild(Span(Att("tss-conversation-reaction-count", text: r.Count.ToString())));
            r.Element.UpdateClassIf(r.Reacted, "tss-conversation-reaction-active");
            r.Element.setAttribute("aria-pressed", r.Reacted ? "true" : "false");
            r.Element.setAttribute("aria-label", $"{r.Emoji} {r.Count} {(r.Count == 1 ? "reaction" : "reactions")}");
        }

        private void EnsureReactionPicker()
        {
            if (_reactionAdd.childElementCount > 0) return;

            var button = new Button().SetIcon(UIcons.SmilePlus).NoBorder().NoBackground().Compact().Tooltip("Add reaction");
            button.Class("tss-conversation-reaction-picker");
            button.OnClick(() =>
            {
                var items = new ContextMenu.Item[_reactionChoices.Length];
                for (var i = 0; i < items.Length; i++)
                {
                    var emoji = _reactionChoices[i];
                    items[i] = new ContextMenu.Item(emoji).OnClick(() => Toggle(emoji));
                }
                new ContextMenu().Items(items).ShowFor(button);
            });
            _reactionAdd.appendChild(button.Render());
            UpdateOptionalParts();
        }

        private void UpdateOptionalParts()
        {
            _attachments.style.display = _attachments.childElementCount > 0 ? "" : "none";
            _reactions.style.display   = _reactionItems.Count > 0 || _reactionAdd.childElementCount > 0 ? "" : "none";
        }

        private void UpdateAriaLabel()
        {
            var when = _time.textContent;
            InnerElement.setAttribute("aria-label", string.IsNullOrEmpty(when) ? $"Comment by {Author}" : $"Comment by {Author} {when}");
        }

        // MarkdownBlock keeps the source's whitespace (break-spaces), which suits a block of prose on its own but
        // turns the newlines marked leaves between block elements into blank lines - a tall gap under every
        // comment's last paragraph. Code blocks keep theirs through <pre>.
        internal static HTMLElement PlainWhitespace(HTMLElement markdown)
        {
            markdown.style.whiteSpace = "normal";
            return markdown;
        }

        internal static string InitialsOf(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            var parts = name.Trim().Split(new[] { ' ', '-', '_', '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        /// <summary>Renders the component.</summary>
        public override HTMLElement Render() => InnerElement;
    }

    /// <summary>
    /// A one-line event on a <see cref="Conversation"/>'s rail, between comments: an icon on the rail, then who did
    /// what and when - "<b>Ana</b> added the <c>bug</c> label 2 days ago". Components such as <see cref="Tag"/>s
    /// can be placed inline in the sentence, and a detail line (a commit, a linked pull request) under it.
    /// </summary>
    [Transpose.Name("tss.ConversationEvent")]
    public sealed class ConversationEvent : ComponentBase<ConversationEvent, HTMLElement>
    {
        private readonly HTMLElement _icon;
        private readonly HTMLElement _line;
        private readonly HTMLElement _avatarHost;
        private readonly HTMLElement _actor;
        private readonly HTMLElement _text;
        private readonly HTMLElement _inline;
        private readonly HTMLElement _time;
        private readonly HTMLElement _detail;
        private          string      _toneClass;

        /// <summary>
        /// Initializes a new event.
        /// </summary>
        /// <param name="icon">The icon drawn on the rail (a tag for a label change, a check for a close, ...).</param>
        /// <param name="actor">Who did it, shown in bold. Pass <c>null</c> for an event nobody did.</param>
        /// <param name="text">What they did, e.g. <c>"added"</c> or <c>"closed this as completed"</c>.</param>
        /// <param name="timestamp">When, as display text.</param>
        public ConversationEvent(UIcons icon, string actor, string text, string timestamp = null)
        {
            _icon       = Span(Att("tss-conversation-event-icon"), I(icon));
            _avatarHost = Span(Att("tss-conversation-event-avatar"));
            _actor      = Span(Att("tss-conversation-event-actor", text: actor ?? string.Empty));
            _text       = Span(Att("tss-conversation-event-text", text: text ?? string.Empty));
            _inline     = Span(Att("tss-conversation-event-inline"));
            _time       = Span(Att("tss-conversation-event-time"));
            _line       = Div(Att("tss-conversation-event-line"), _avatarHost, _actor, _text, _inline, _time);
            _detail     = Div(Att("tss-conversation-event-detail"));

            InnerElement = Div(Att("tss-conversation-event"), _icon, Div(Att("tss-conversation-event-content"), _line, _detail));

            if (!string.IsNullOrEmpty(actor)) SetAvatar(new Avatar(initials: ConversationComment.InitialsOf(actor)));
            _actor.style.display  = string.IsNullOrEmpty(actor) ? "none" : "";
            _detail.style.display = "none";
            Timestamp(timestamp);
        }

        /// <summary>Sets the actor's avatar, drawn small before their name. Pass <c>null</c> to show none.</summary>
        public ConversationEvent SetAvatar(Avatar avatar)
        {
            ClearChildren(_avatarHost);
            if (avatar is object)
            {
                var element = avatar.Size(AvatarSize.XSmall).Render();
                element.classList.add("tss-conversation-event-avatar-image");
                _avatarHost.appendChild(element);
            }
            return this;
        }

        /// <summary>
        /// Places components inline in the sentence, after the text - the label <see cref="Tag"/>s that were added,
        /// a milestone link, the name of a branch.
        /// </summary>
        public ConversationEvent Inline(params IComponent[] components)
        {
            foreach (var c in components)
            {
                if (c is object) _inline.appendChild(c.Render());
            }
            return this;
        }

        /// <summary>Places text inline in the sentence, after anything already there.</summary>
        public ConversationEvent Inline(string text)
        {
            _inline.appendChild(Span(Att("tss-conversation-event-inline-text", text: text)));
            return this;
        }

        /// <summary>Sets when it happened, as display text, with an optional tooltip.</summary>
        public ConversationEvent Timestamp(string text, string tooltip = null)
        {
            _time.textContent   = text ?? string.Empty;
            _time.style.display = string.IsNullOrEmpty(text) ? "none" : "";
            if (string.IsNullOrEmpty(tooltip)) _time.removeAttribute("title");
            else _time.setAttribute("title", tooltip);
            return this;
        }

        /// <summary>Sets a line under the sentence - the commit that referenced the issue, a pull request, a quote.</summary>
        public ConversationEvent Detail(IComponent detail)
        {
            ClearChildren(_detail);
            if (detail is object) _detail.appendChild(detail.Render());
            _detail.style.display = detail is object ? "" : "none";
            return this;
        }

        /// <summary>
        /// Fills the icon on the rail with a tone, for the events that change the state of the thread: success when
        /// it is reopened, danger when it is closed as not planned, AI or primary when it is completed.
        /// <see cref="BadgeTone.Neutral"/> (the default) is the plain outlined icon.
        /// </summary>
        public ConversationEvent Tone(BadgeTone tone)
        {
            if (_toneClass is object) _icon.classList.remove(_toneClass);
            _toneClass = tone == BadgeTone.Neutral ? null : $"tss-conversation-tone-{tone.ToString().ToLower()}";
            if (_toneClass is object) _icon.classList.add(_toneClass);
            return this;
        }

        /// <summary>Renders the component.</summary>
        public override HTMLElement Render() => InnerElement;
    }

    /// <summary>
    /// The reply box at the foot of a <see cref="Conversation"/>: the current user's <see cref="Avatar"/> beside a
    /// card with Write / Preview tabs, a Markdown <see cref="TextArea"/>, a live <see cref="MarkdownBlock"/> preview
    /// and a submit button (plus any extra actions, such as "Close issue").
    /// </summary>
    [Transpose.Name("tss.ConversationComposer")]
    public sealed class ConversationComposer : ComponentBase<ConversationComposer, HTMLElement>
    {
        private readonly TextArea      _textArea;
        private readonly MarkdownBlock _preview;
        private readonly HTMLElement   _editorHost;
        private readonly HTMLElement   _previewHost;
        private readonly HTMLElement   _writeTab;
        private readonly HTMLElement   _previewTab;
        private readonly HTMLElement   _actions;
        private readonly HTMLElement   _avatarHost;
        private readonly Button        _submit;
        private          Action<ConversationComposer, string> _onSubmit;
        private          bool          _previewing;

        /// <summary>
        /// Initializes a new composer.
        /// </summary>
        /// <param name="avatar">The current user's avatar, drawn to the left of the box. Pass <c>null</c> for none.</param>
        /// <param name="placeholder">The placeholder of the text area.</param>
        /// <param name="submitText">The text of the submit button.</param>
        public ConversationComposer(Avatar avatar = null, string placeholder = "Add your comment here...", string submitText = "Comment")
        {
            _textArea = new TextArea().SetPlaceholder(placeholder).AutoResize(minHeight: 96, maxHeight: 400);
            _textArea.Render().classList.add("tss-conversation-composer-input");
            _textArea.AriaLabel("Comment");

            _preview     = new MarkdownBlock().TableCopyButtons(false);
            _editorHost  = Div(Att("tss-conversation-composer-editor"), _textArea.Render());
            _previewHost = Div(Att("tss-conversation-composer-preview"), ConversationComment.PlainWhitespace(_preview.Render()));

            _writeTab    = Tab("Write",   () => ShowPreview(false));
            _previewTab  = Tab("Preview", () => ShowPreview(true));
            var tabs     = Div(Att("tss-conversation-composer-tabs"), _writeTab, _previewTab);
            tabs.setAttribute("role", "tablist");

            _submit = new Button(submitText).Primary().Disabled().OnClick(Submit);
            _actions = Div(Att("tss-conversation-composer-actions"), _submit.Render());

            _textArea.OnInput((_, __) => _submit.Disabled(string.IsNullOrWhiteSpace(_textArea.Text)));
            _textArea.OnKeyDown((_, e) =>
            {
                if (e.key == "Enter" && (e.ctrlKey || e.metaKey))
                {
                    StopEvent(e);
                    Submit();
                }
            });

            var box     = Div(Att("tss-conversation-composer-box"), tabs, _editorHost, _previewHost, Div(Att("tss-conversation-composer-footer"), Span(Att("tss-conversation-composer-hint", text: "Markdown is supported")), _actions));
            _avatarHost = Span(Att("tss-conversation-composer-avatar"));
            InnerElement = Div(Att("tss-conversation-composer"), _avatarHost, box);

            SetAvatar(avatar);
            ShowPreview(false);
        }

        /// <summary>The text currently in the box.</summary>
        public string Text => _textArea.Text;

        /// <summary>The underlying text area, for anything this component does not expose.</summary>
        public TextArea TextArea => _textArea;

        /// <summary>Sets the current user's avatar. Pass <c>null</c> to show none.</summary>
        public ConversationComposer SetAvatar(Avatar avatar)
        {
            ClearChildren(_avatarHost);
            _avatarHost.style.display = avatar is null ? "none" : "";
            if (avatar is object) _avatarHost.appendChild(avatar.Size(AvatarSize.Small).Render());
            return this;
        }

        /// <summary>Sets the text in the box.</summary>
        public ConversationComposer SetText(string text)
        {
            _textArea.SetText(text ?? string.Empty);
            _submit.Disabled(string.IsNullOrWhiteSpace(_textArea.Text));
            if (_previewing) _preview.Text = _textArea.Text;
            return this;
        }

        /// <summary>
        /// Registers the callback for the submit button (and Ctrl/⌘+Enter). It gets the composer and the Markdown
        /// text; the box is cleared and back on the Write tab when it is called.
        /// </summary>
        public ConversationComposer OnSubmit(Action<ConversationComposer, string> onSubmit)
        {
            _onSubmit += onSubmit;
            return this;
        }

        /// <summary>Adds buttons to the left of the submit button - "Close issue", "Cancel", ...</summary>
        public ConversationComposer Actions(params Button[] buttons)
        {
            foreach (var b in buttons)
            {
                if (b is object) _actions.insertBefore(b.Render(), _submit.Render());
            }
            return this;
        }

        /// <summary>Moves focus into the text area.</summary>
        public ConversationComposer Focus()
        {
            ShowPreview(false);
            _textArea.Focus();
            return this;
        }

        private void Submit()
        {
            var text = _textArea.Text;
            if (string.IsNullOrWhiteSpace(text)) return;
            SetText(string.Empty);
            ShowPreview(false);
            _onSubmit?.Invoke(this, text);
        }

        private void ShowPreview(bool preview)
        {
            _previewing = preview;
            if (preview) _preview.Text = string.IsNullOrWhiteSpace(_textArea.Text) ? "*Nothing to preview*" : _textArea.Text;
            _editorHost.style.display  = preview ? "none" : "";
            _previewHost.style.display = preview ? "" : "none";
            _writeTab.UpdateClassIf(!preview, "tss-conversation-composer-tab-active");
            _previewTab.UpdateClassIf(preview, "tss-conversation-composer-tab-active");
            _writeTab.setAttribute("aria-selected", preview ? "false" : "true");
            _previewTab.setAttribute("aria-selected", preview ? "true" : "false");
        }

        private static HTMLElement Tab(string text, Action onClick)
        {
            var tab = Button(Att("tss-conversation-composer-tab", type: "button", text: text));
            tab.setAttribute("role", "tab");
            tab.addEventListener("click", ev =>
            {
                StopEvent(ev);
                onClick();
            });
            return tab;
        }

        /// <summary>Renders the component.</summary>
        public override HTMLElement Render() => InnerElement;
    }
}
