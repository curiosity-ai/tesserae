using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using Transpose.Core;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Which corner of an <see cref="OmniResult{T}"/> icon tile a badge is pinned to.
    /// </summary>
    public enum OmniResultBadgeCorner
    {
        /// <summary>The top-left corner - where a "pinned" marker usually goes.</summary>
        TopLeft,
        /// <summary>The top-right corner.</summary>
        TopRight,
        /// <summary>The bottom-left corner.</summary>
        BottomLeft,
        /// <summary>The bottom-right corner - where a "where it came from" marker usually goes.</summary>
        BottomRight
    }

    /// <summary>
    /// Where the selection checkbox of an <see cref="OmniResult{T}"/> lives, and when it shows. A selected
    /// result always shows its checkbox, whatever the mode.
    /// </summary>
    public enum OmniResultSelectionMode
    {
        /// <summary>A checkbox in its own column before the icon, revealed while the result is hovered.</summary>
        OnHoverBeforeIcon,
        /// <summary>A checkbox over the icon tile, revealed - and covering the icon - while the result is hovered.</summary>
        OnHoverOverIcon,
        /// <summary>A checkbox in its own column before the icon, always visible.</summary>
        AlwaysBeforeIcon,
        /// <summary>A checkbox in place of the icon tile, which is not drawn at all.</summary>
        ReplacingIcon
    }

    /// <summary>
    /// How the commands of an <see cref="OmniResult{T}"/> are reached.
    /// </summary>
    public enum OmniResultCommandsMode
    {
        /// <summary>Right-clicking the result, and nothing else - no button is drawn.</summary>
        RightClickOnly,
        /// <summary>Right-clicking the result, or a [...] button revealed while it is hovered.</summary>
        ButtonOnHover,
        /// <summary>Right-clicking the result, or a [...] button that is always visible.</summary>
        ButtonAlwaysVisible
    }

    /// <summary>
    /// When the inline commands of an <see cref="OmniResult{T}"/> show.
    /// </summary>
    public enum OmniResultCommandsVisibility
    {
        /// <summary>Revealed while the result is hovered (or focused).</summary>
        OnHover,
        /// <summary>Always visible.</summary>
        AlwaysVisible
    }

    /// <summary>
    /// A search-result card: an icon tile, a title with an optional badge, an optional excerpt with the
    /// matched terms highlighted, an optional footer naming the source and whatever metadata the host wants
    /// beside it, and an optional <see cref="PagesStack"/> preview pinned to its right.
    /// <para>
    /// The result it stands for is carried as <see cref="Result"/>, so a click, selection or command handler
    /// shared by a whole list of results can act on the right one without a closure per card.
    /// </para>
    /// <para>
    /// Rows are selectable (<see cref="Selectable(OmniResultSelectionMode)"/>) with a checkbox that can sit
    /// beside or over the icon; commands are reached by right-click and, optionally, a [...] button
    /// (<see cref="OnContextMenu(Action{OmniResult{T}}, OmniResultCommandsMode)"/>), with room for a few
    /// inline commands before it (<see cref="InlineCommands(OmniResultCommandsVisibility, IComponent[])"/>).
    /// </para>
    /// </summary>
    [Transpose.Name("tss.OmniResult")]
    public class OmniResult<T> : ComponentBase<OmniResult<T>, HTMLElement>
    {
        private readonly HTMLElement _selectContainer;
        private readonly HTMLElement _iconHolder;
        private readonly IconTile    _iconTile;
        private readonly HTMLElement _iconContainer;
        private readonly HTMLElement _idContainer;
        private readonly HTMLElement _idText;
        private readonly HTMLElement _titleElement;
        private readonly HTMLElement _badgeContainer;
        private readonly HTMLElement _headerContainer;
        private readonly HTMLElement _bodyContainer;
        private readonly HTMLElement _contentContainer;
        private readonly InlineLabel _source;
        private readonly HTMLElement _footerContainer;
        private readonly HTMLElement _contributionContainer;
        private readonly HTMLElement _mainContainer;
        private readonly HTMLElement _railContainer;
        private readonly HTMLElement _commandsContainer;
        private readonly HTMLElement _inlineCommandsContainer;

        private readonly Dictionary<string, HTMLElement> _iconBadges = new Dictionary<string, HTMLElement>();

        private CheckBox          _checkBox;
        private ContributionBar   _contribution;
        private HTMLButtonElement _menuButton;
        private PagesStack        _pages;

        private string                       _id;
        private string                       _title;
        private IComponent                   _titleComponent;
        private string                       _text;
        private Regex                        _highlighter;
        private OmniResultSelectionMode      _selectionMode = OmniResultSelectionMode.OnHoverBeforeIcon;
        private bool                         _selectionEnabled;
        private bool                         _textSelectable;
        private bool                         _pagesFanOnHover = true;
        private Action<OmniResult<T>>        _commandsHandler;
        private Action<OmniResult<T>>        _sourceClickHandler;
        private Func<OmniResult<T>, ContextMenu.Item[]> _menuGenerator;
        private MouseEvent                   _lastPointerEvent;

        private readonly List<OmniResultOpenAction<T>> _openActions = new List<OmniResultOpenAction<T>>();

        private Func<OmniResult<T>, Task<IComponent>> _modalContent;
        private Func<OmniResult<T>, IComponent>       _modalHeader;
        private UnitSize                              _modalWidth     = UnitSize.Auto();
        private UnitSize                              _modalHeight    = UnitSize.Auto();
        private bool                                  _modalKeepsIcon;
        private bool                                  _modalKeepsFooter;

        private HTMLElement            _footerStandIn;
        private Modal                  _modal;
        private Action<OmniResult<T>>  _modalCommands;
        private Action<OmniResult<T>>  _modalFullScreen;
        private bool                   _modalHasFullScreen = true;
        private Action<OmniResult<T>>  _modalPrevious;
        private Action<OmniResult<T>>  _modalNext;
        private int                    _modalPosition;
        private int                    _modalCount;
        private Func<int, int, string> _modalCountFormat;
        private bool                   _modalShortcuts = true;

        private event Action<OmniResult<T>, bool> SelectionChanged;
        private event Action<OmniResult<T>>       RangeSelectionRequested;

        /// <summary>
        /// Initializes a new instance of this class standing for the given result.
        /// </summary>
        public OmniResult(T result, string title = null)
        {
            Result = result;

            _selectContainer = Div(Att("tss-omniresult-select"));

            // The tile is the shared IconTile - the row only adds the class its own rules hang off.
            _iconTile        = new IconTile();
            _iconContainer   = _iconTile.Render();
            _iconContainer.classList.add("tss-omniresult-icon");

            _iconHolder      = Div(Att("tss-omniresult-icon-holder"), _iconContainer);

            _idText       = Span(Att("tss-omniresult-id-value"));
            _idContainer  = Div(Att("tss-omniresult-id"), _idText, I(UIcons.AngleRight, UIconsWeight.Regular, "tss-omniresult-id-chevron"));

            _titleElement   = Span(Att("tss-omniresult-title"));
            _badgeContainer = Div(Att("tss-omniresult-badge"));

            _inlineCommandsContainer = Div(Att("tss-omniresult-inline-commands"));
            _commandsContainer       = Div(Att("tss-omniresult-commands"), _inlineCommandsContainer);

            //The commands sit at the end of the title's own line rather than in a column of their own beside
            //the whole card: a column would narrow the excerpt and the footer too, for buttons that are only
            //ever level with the title. Here the title is the only thing that gives up room for them.
            _headerContainer = Div(Att("tss-omniresult-header"), _idContainer, _titleElement, _badgeContainer, _commandsContainer);

            _bodyContainer    = Div(Att("tss-omniresult-body"));
            _contentContainer = Div(Att("tss-omniresult-content"));

            //The source is an InlineLabel like every other footer entry - it just leads the line.
            _source = InlineLabel().Class("tss-omniresult-source");

            _footerContainer = Div(Att("tss-omniresult-footer"));

            _contributionContainer = Div(Att("tss-omniresult-contribution"));

            _mainContainer = Div(Att("tss-omniresult-main"), _headerContainer, _bodyContainer, _contentContainer, _footerContainer, _contributionContainer);

            _railContainer = Div(Att("tss-omniresult-rail"));

            InnerElement = Div(Att("tss-omniresult"), _selectContainer, _iconHolder, _mainContainer, _railContainer);

            SetId(null);
            SetTitle(title);
            SetText(null);
            SetContent(null);
            SetBadge((string)null);
            SetSource((string)null, null);
            SetContributionBar(null);

            HookEvents();
        }

        /// <summary>
        /// Gets the result this card stands for - the search hit, document, record or row it was built from.
        /// </summary>
        public T Result { get; }

        //The parts a row is built from, for a subclass that has something of its own to put between them - a
        //line above the header, an area below the footer, actions beside the title. Reading and adding to
        //them is a subclass's business; the slots the row manages itself are still set through the methods
        //above them, which keep track of what is in them and whether they are shown.

        /// <summary>The text column: the header, the excerpt, the content, the footer and the contribution bar.</summary>
        protected HTMLElement MainContainer => _mainContainer;

        /// <summary>The line the identifier, the title, the badge and the commands share.</summary>
        protected HTMLElement HeaderContainer => _headerContainer;

        /// <summary>What <see cref="SetText(string)"/> writes the excerpt into.</summary>
        protected HTMLElement BodyContainer => _bodyContainer;

        /// <summary>What <see cref="SetContent(IComponent)"/> puts the rich preview into.</summary>
        protected HTMLElement ContentContainer => _contentContainer;

        /// <summary>The line the source and the footer entries share.</summary>
        protected HTMLElement FooterContainer => _footerContainer;

        /// <summary>The tile, and whatever <see cref="SetIconBadge"/> pinned to its corners.</summary>
        protected HTMLElement IconHolder => _iconHolder;

        /// <summary>The tile itself, inside <see cref="IconHolder"/>.</summary>
        protected HTMLElement IconContainer => _iconContainer;

        /// <summary>What <see cref="SetBadge(IComponent)"/> puts the badge into.</summary>
        protected HTMLElement BadgeContainer => _badgeContainer;

        /// <summary>What <see cref="SetContributionBar"/> puts the bar into.</summary>
        protected HTMLElement ContributionContainer => _contributionContainer;

        /// <summary>The strip between the text column and the commands.</summary>
        protected HTMLElement RailContainer => _railContainer;

        /// <summary>The commands at the end of the header line, inline ones included.</summary>
        protected HTMLElement CommandsContainer => _commandsContainer;

        /// <summary>Where the selection checkbox goes when it has a column of its own.</summary>
        protected HTMLElement SelectContainer => _selectContainer;

        /// <summary>
        /// Gets or sets the title of the result.
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetTitle(value);
        }

        /// <summary>
        /// Gets or sets the identifier shown before the title - an issue number, a ticket key, a row number -
        /// followed by a chevron pointing at the title. A null or empty value drops both.
        /// </summary>
        public string Id
        {
            get => _id;
            set => SetId(value);
        }

        /// <summary>
        /// Gets or sets the excerpt shown under the title, or null when the result has none. Plain text: the
        /// only markup it gets is the highlighting of <see cref="Highlight(Regex)"/>.
        /// </summary>
        public string Text
        {
            get => _text;
            set => SetText(value);
        }

        /// <summary>
        /// Returns a value indicating whether the result can be selected.
        /// </summary>
        public bool IsSelectionEnabled => _selectionEnabled;

        /// <summary>
        /// Gets or sets whether the result is selected. Setting it runs the
        /// <see cref="OnSelectionChanged(Action{OmniResult{T}, bool})"/> handlers, so a host list can keep its
        /// own selection in step whether the change came from the user or from code.
        /// </summary>
        public bool IsSelected
        {
            get => _selectionEnabled && InnerElement.classList.contains("tss-omniresult-selected");
            set
            {
                if (!_selectionEnabled)
                {
                    InnerElement.classList.remove("tss-omniresult-selected");
                    return;
                }

                if (IsSelected == value) return;

                InnerElement.UpdateClassIf(value, "tss-omniresult-selected");
                InnerElement.setAttribute("aria-selected", value ? "true" : "false");

                if (_checkBox is object) _checkBox.IsChecked = value;

                SelectionChanged?.Invoke(this, value);
            }
        }

        /// <summary>
        /// Gets or sets whether the result is the active one - the row a keyboard-driven list has moved to.
        /// It is styled like a hovered row, and reveals whatever the row reveals on hover.
        /// </summary>
        public bool IsActive
        {
            get => InnerElement.classList.contains("tss-omniresult-active");
            set => InnerElement.UpdateClassIf(value, "tss-omniresult-active");
        }

        /// <summary>
        /// Gets the <see cref="PagesStack"/> preview shown at the end of the row, or null when it has none.
        /// </summary>
        public PagesStack Pages => _pages;

        /// <summary>
        /// Gets the <see cref="Tesserae.ContributionBar"/> shown under the footer, or null when it has none.
        /// </summary>
        public ContributionBar Contribution => _contribution;

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;

        /// <summary>
        /// Sets the title of the result. The title is ellipsized to one line, carries the full text as its
        /// native tooltip, and has the terms of <see cref="Highlight(Regex)"/> marked in it.
        /// </summary>
        public OmniResult<T> SetTitle(string title)
        {
            _titleComponent = null;
            _title          = title ?? string.Empty;

            RenderTitle();

            return this;
        }

        /// <summary>
        /// Puts a component in the title slot in place of the plain title - the escape hatch for a result
        /// whose title genuinely isn't text, such as one built from fields an administrator configured. The
        /// text passed alongside it stays the row's <see cref="Title"/>, so the tooltip, the modal header and
        /// anything else reading the title still have something to say. Pass null to go back to plain text.
        /// <para>
        /// A component title is drawn as it was given: <see cref="Highlight(Regex)"/> does not reach inside it.
        /// </para>
        /// </summary>
        public OmniResult<T> SetTitle(IComponent title, string text = null)
        {
            _titleComponent = title;
            _title          = text ?? _title ?? string.Empty;

            RenderTitle();

            return this;
        }

        /// <summary>
        /// Sets the identifier shown before the title - an issue number, a ticket key, a row number - drawn in
        /// the quiet way an identifier reads and followed by a chevron pointing at the title. A null or empty
        /// value drops the identifier and the chevron with it.
        /// </summary>
        public OmniResult<T> SetId(string id)
        {
            _id = id;

            var isEmpty = string.IsNullOrEmpty(id);

            _idText.textContent      = isEmpty ? string.Empty : id;
            _idContainer.style.display = isEmpty ? "none" : "";

            return this;
        }

        /// <summary>
        /// Puts the given icon on the tile, in the given color, over a paler wash of that same color. Pass
        /// the full-strength color the glyph should be - the background is computed from it (and cached), a
        /// light tint of it under a light theme and a deep one under a dark theme.
        /// </summary>
        public OmniResult<T> SetIcon(UIcons icon, string color = null, UIconsWeight weight = UIconsWeight.Regular)
        {
            _iconTile.SetIcon(icon, color, weight);

            return this;
        }

        /// <summary>
        /// Puts the given short text on the tile in place of an icon - a file type, "PPTX" or "CSV", where
        /// no glyph says it as plainly - in the given color, over a paler wash of that same color. It is drawn
        /// at the size the tile is sized for unless <paramref name="size"/> asks for another one - text longer
        /// than the three or four letters a type name usually is wants <see cref="TextSize.Tiny"/>.
        /// </summary>
        public OmniResult<T> SetIcon(string text, string color = null, TextSize? size = null)
        {
            _iconTile.SetIcon(text, color, size);

            return this;
        }

        /// <summary>
        /// Puts the given component on the tile - an <see cref="Image"/> thumbnail, an <see cref="Avatar"/>,
        /// an emoji - optionally tinting the tile with the given color.
        /// </summary>
        public OmniResult<T> SetIcon(IComponent iconOrImage, string color = null)
        {
            _iconTile.SetIcon(iconOrImage, color);

            return this;
        }

        /// <summary>
        /// Puts a badge next to the title - what matched, how many times, how the result was found.
        /// A null or empty value hides it.
        /// </summary>
        public OmniResult<T> SetBadge(string text)
        {
            ClearChildren(_badgeContainer);

            var isEmpty = string.IsNullOrEmpty(text);

            // A Badge, tinted the quiet way a "what matched" label wants to read - a host that needs a tone
            // of its own passes its own Badge to the IComponent overload instead.
            if (!isEmpty) _badgeContainer.appendChild(Badge(text).Pill().Class("tss-omniresult-badge-pill").Render());

            _badgeContainer.style.display = isEmpty ? "none" : "";

            return this;
        }

        /// <summary>
        /// Puts the given component next to the title in place of the plain badge - a <see cref="Badge"/>
        /// with a tone of its own, a <see cref="Spinner"/>, a small button. A null value empties the slot.
        /// </summary>
        public OmniResult<T> SetBadge(IComponent badge)
        {
            ClearChildren(_badgeContainer);

            if (badge != null) _badgeContainer.appendChild(badge.Render());

            _badgeContainer.style.display = badge is null ? "none" : "";

            return this;
        }

        /// <summary>
        /// Sets the excerpt shown under the title. It is plain text, not a component: whatever the search
        /// returned as the matching passage, ellipsized to the two lines the row gives it, with the terms of
        /// <see cref="Highlight(Regex)"/> marked in it. A null or empty value drops the line entirely.
        /// </summary>
        public OmniResult<T> SetText(string text)
        {
            _text = text;

            RenderText();

            return this;
        }

        /// <summary>
        /// Caps how many lines of the excerpt are shown before it is ellipsized. Two by default.
        /// </summary>
        public OmniResult<T> TextLines(int lines)
        {
            _bodyContainer.style.setProperty("-webkit-line-clamp", $"{(lines < 1 ? 1 : lines)}");

            return this;
        }

        /// <summary>
        /// Puts a component under the excerpt, in the text column: the rich preview a result has of its own -
        /// a thumbnail, a quoted message, a table of the fields that matched - for everything a plain excerpt
        /// can't say. Pass null to take it away.
        /// <para>
        /// Cap how tall it is allowed to be with <see cref="ContentMaxHeight(UnitSize)"/>, which fades the
        /// overflow out rather than cutting it off.
        /// </para>
        /// </summary>
        public OmniResult<T> SetContent(IComponent content)
        {
            ClearChildren(_contentContainer);

            if (content is object) _contentContainer.appendChild(content.Render());

            _contentContainer.style.display = content is null ? "none" : "";

            return this;
        }

        /// <summary>
        /// Caps how tall the <see cref="SetContent(IComponent)"/> preview is allowed to grow, fading whatever
        /// runs past it out instead of cutting it off. Pass null to un-cap it.
        /// </summary>
        public OmniResult<T> ContentMaxHeight(UnitSize maxHeight)
        {
            var isCapped = maxHeight is object;

            _contentContainer.style.maxHeight = isCapped ? maxHeight.ToString() : "";
            _contentContainer.UpdateClassIf(isCapped, "tss-omniresult-content-masked");

            return this;
        }

        /// <summary>
        /// Pins a badge to a corner of the icon tile - where a result came from, that it is pinned - drawn
        /// over the tile's corner and outside its clipping. Pass null to clear that corner.
        /// </summary>
        public OmniResult<T> SetIconBadge(IComponent badge, OmniResultBadgeCorner corner = OmniResultBadgeCorner.BottomRight)
        {
            var cornerClass = CornerClass(corner);

            if (_iconBadges.TryGetValue(cornerClass, out var stale))
            {
                _iconHolder.removeChild(stale);
                _iconBadges.Remove(cornerClass);
            }

            if (badge is null) return this;

            var holder = Div(Att("tss-omniresult-icon-badge " + cornerClass), badge.Render());

            _iconHolder.appendChild(holder);
            _iconBadges[cornerClass] = holder;

            return this;
        }

        private static string CornerClass(OmniResultBadgeCorner corner)
        {
            switch (corner)
            {
                case OmniResultBadgeCorner.TopLeft:     return "tss-omniresult-icon-badge-tl";
                case OmniResultBadgeCorner.TopRight:    return "tss-omniresult-icon-badge-tr";
                case OmniResultBadgeCorner.BottomLeft:  return "tss-omniresult-icon-badge-bl";
                default:                                return "tss-omniresult-icon-badge-br";
            }
        }

        /// <summary>
        /// Marks every match of the given expression in the title and the excerpt - the same pattern a search
        /// backend hands back for highlighting. Matching is done against the text itself and the matches are
        /// wrapped in their own elements, so neither is ever treated as markup.
        /// </summary>
        public OmniResult<T> Highlight(Regex highlighter)
        {
            _highlighter = highlighter;

            RenderTitle();
            RenderText();

            return this;
        }

        /// <summary>
        /// Marks every match of the given regular expression in the excerpt, case-insensitively by default.
        /// </summary>
        public OmniResult<T> Highlight(string pattern, bool ignoreCase = true)
        {
            return Highlight(string.IsNullOrEmpty(pattern)
                ? null
                : new Regex(pattern, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None));
        }

        /// <summary>
        /// Marks every occurrence of the given words in the excerpt, case-insensitively - the convenience
        /// form of <see cref="Highlight(Regex)"/> for a host that has the query terms rather than a pattern.
        /// </summary>
        public OmniResult<T> HighlightWords(params string[] words)
        {
            if (words is null || words.Length == 0) return Highlight((Regex)null);

            var escaped = new List<string>();

            foreach (var word in words)
            {
                if (!string.IsNullOrWhiteSpace(word)) escaped.Add(Regex.Escape(word));
            }

            if (escaped.Count == 0) return Highlight((Regex)null);

            return Highlight(new Regex(string.Join("|", escaped), RegexOptions.IgnoreCase));
        }

        /// <summary>
        /// Names where the result came from: a small rounded square in the given color, followed by the
        /// text, at the start of the footer. A null or empty text hides it.
        /// <para>
        /// Passing a handler makes the source itself clickable - scoping a search to that source is the
        /// usual thing to do with it - without the click counting as opening the result. It takes a tab
        /// stop of its own and answers Enter and Space, and the result is handed to the handler so one
        /// shared handler can read <see cref="Result"/>.
        /// </para>
        /// </summary>
        public OmniResult<T> SetSource(string color, string text, Action<OmniResult<T>> onClick = null)
        {
            //No colour and no marker: the source is its name alone, rather than a name behind a blank square.
            if (string.IsNullOrEmpty(color)) _source.NoMark();
            else                             _source.SetColor(color);

            return SetSourceText(text, onClick);
        }

        /// <summary>
        /// Names where the result came from with a marker of the host's own - the source's logo, an avatar -
        /// in place of the plain colored square, followed by the text. Everything else behaves as
        /// <see cref="SetSource(string, string, Action{OmniResult{T}})"/>: a null or empty text hides it, and
        /// passing a handler makes the source clickable.
        /// </summary>
        public OmniResult<T> SetSource(IComponent marker, string text, Action<OmniResult<T>> onClick = null)
        {
            _source.SetIcon(marker);

            return SetSourceText(text, onClick);
        }

        private OmniResult<T> SetSourceText(string text, Action<OmniResult<T>> onClick)
        {
            var isEmpty = string.IsNullOrEmpty(text);

            _source.SetText(text);

            // Only when one is given, so a later SetSource can't silently drop a handler an earlier
            // OnSourceClick registered. OnSourceClick(null) is how a source stops being clickable.
            if (onClick != null) OnSourceClick(onClick);

            // Detached rather than hidden when empty, so the dot separators - which are drawn by CSS off
            // :first-child - don't leave a leading dot in a footer that has no source.
            var element = _source.Render();

            if (isEmpty)
            {
                if (element.parentElement is object) _footerContainer.removeChild(element);
            }
            else if (element.parentElement is null)
            {
                _footerContainer.insertBefore(element, _footerContainer.firstChild);
            }

            return UpdateFooterVisibility();
        }

        /// <summary>
        /// Registers what clicking the source in the footer does - scope the search to it, open it, filter
        /// by it - and marks the source as clickable. Clicking it does not also count as opening the
        /// result. Pass null to make the source plain text again.
        /// </summary>
        public OmniResult<T> OnSourceClick(Action<OmniResult<T>> onClick)
        {
            _sourceClickHandler = onClick;

            //The label owns the tab stop, the Enter/Space handling and stopping the click from counting as
            //opening the row; the class is kept for hosts styling a clickable source.
            _source.Render().UpdateClassIf(onClick != null, "tss-omniresult-source-clickable");
            _source.OnClick(onClick is null ? null : (Action<InlineLabel>)(_ => _sourceClickHandler(this)));

            return this;
        }

        /// <summary>
        /// Sets the metadata shown after the source in the footer - a path, a size, an owner, a date - as
        /// <see cref="InlineLabel"/>s, each separated from the next by a dot. Each one can carry a mark (a
        /// glyph, an image, a square of colour), be pressable, or be a real link, and they are all drawn at
        /// one size so the line reads as one row of facts. Replaces whatever entries were there.
        /// </summary>
        public OmniResult<T> SetFooterEntries(params InlineLabel[] entries)
        {
            foreach (var stale in _footerContainer.querySelectorAll(".tss-omniresult-footer-entry"))
            {
                _footerContainer.removeChild(stale.As<HTMLElement>());
            }

            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    if (entry is null) continue;

                    _footerContainer.appendChild(Div(Att("tss-omniresult-footer-entry"), entry.Render()));
                }
            }

            return UpdateFooterVisibility();
        }

        /// <summary>
        /// Adds one more entry to the end of the footer line, as a component of the host's own rather than
        /// an <see cref="InlineLabel"/> - a badge, a chip, a small control a label cannot be. It gets the
        /// same box and the same separating dot as every other entry.
        /// </summary>
        public OmniResult<T> AddFooterEntry(IComponent entry)
        {
            if (entry is null) return this;

            _footerContainer.appendChild(Div(Att("tss-omniresult-footer-entry"), entry.Render()));

            return UpdateFooterVisibility();
        }

        /// <summary>
        /// Sets the metadata shown after the source in the footer, as plain text entries.
        /// </summary>
        public OmniResult<T> SetFooterEntries(params string[] entries)
        {
            if (entries is null) return SetFooterEntries((InlineLabel[])null);

            var labels = new List<InlineLabel>();

            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry)) labels.Add(InlineLabel(entry));
            }

            return SetFooterEntries(labels.ToArray());
        }

        /// <summary>
        /// Makes the result selectable, with its checkbox shown as the given mode says. The checkbox toggles
        /// the selection on click; ctrl-clicking the row does the same, and shift-clicking it asks for a
        /// range (see <see cref="OnRangeSelectionRequested(Action{OmniResult{T}})"/>).
        /// </summary>
        public OmniResult<T> Selectable(OmniResultSelectionMode mode = OmniResultSelectionMode.OnHoverBeforeIcon)
        {
            _selectionEnabled = true;
            _selectionMode    = mode;

            EnsureCheckBox();
            ApplySelectionMode();

            return this;
        }

        /// <summary>
        /// Takes the checkbox away again, unselecting the result if it was selected.
        /// </summary>
        public OmniResult<T> NotSelectable()
        {
            IsSelected        = false;
            _selectionEnabled = false;

            ApplySelectionMode();

            return this;
        }

        /// <summary>
        /// Makes the row's own text - the title, the excerpt, the content and the footer - selectable, for a
        /// row that is read rather than glanced at: a message in a thread, a comment, a note someone copies a
        /// line out of. Off by default, so dragging across an ordinary list of results never leaves half an
        /// excerpt highlighted.
        /// <para>
        /// A click that ended a selection inside the row does not count as a click on the row, so copying a
        /// line out of one never opens it. The tile, the checkbox and the commands stay unselectable: they are
        /// controls, not text.
        /// </para>
        /// <para>
        /// A host that also makes the row a drag handle (<c>draggable</c>) takes this away again, whatever is
        /// asked for here: the browser starts a drag where the selection would have begun. The two are one
        /// choice - a row is dragged around or it is read, not both.
        /// </para>
        /// </summary>
        public OmniResult<T> TextSelectable(bool value = true)
        {
            _textSelectable = value;

            InnerElement.UpdateClassIf(value, "tss-omniresult-text-selectable");

            return this;
        }

        /// <summary>
        /// Selects (or unselects) the result. Does nothing on a result that isn't selectable.
        /// </summary>
        public OmniResult<T> Selected(bool value = true)
        {
            IsSelected = value;

            return this;
        }

        /// <summary>
        /// Registers a callback invoked whenever the result is selected or unselected, by the user or by
        /// code, with the new state.
        /// </summary>
        public OmniResult<T> OnSelectionChanged(Action<OmniResult<T>, bool> onSelectionChanged)
        {
            SelectionChanged += onSelectionChanged;

            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the user shift-clicks the result, i.e. asks for everything
        /// between the last result they selected and this one. A single card knows nothing about its
        /// siblings, so the host list owns what "between" means - and selects them itself.
        /// </summary>
        public OmniResult<T> OnRangeSelectionRequested(Action<OmniResult<T>> onRangeSelectionRequested)
        {
            RangeSelectionRequested += onRangeSelectionRequested;

            return this;
        }

        /// <summary>
        /// Registers the handler that opens the commands of this result, and says how it is reached: by
        /// right-click alone, or also by a [...] button at the top-right of the row - shown always, or only
        /// while the row is hovered. The handler is given the result, so it can build a menu from
        /// <see cref="Result"/> and show it with <see cref="ShowMenu(ContextMenu)"/>.
        /// </summary>
        public OmniResult<T> OnContextMenu(Action<OmniResult<T>> handler, OmniResultCommandsMode mode = OmniResultCommandsMode.RightClickOnly)
        {
            _commandsHandler = handler;
            _menuGenerator   = null;

            return CommandsMode(mode);
        }

        /// <summary>
        /// Attaches a <see cref="ContextMenu"/> of actions to the result: the generator runs on every open
        /// and is given the result, and the items it returns are shown at the pointer (or under the [...]
        /// button). Returning null or an empty array opens nothing.
        /// </summary>
        public OmniResult<T> OnContextMenu(Func<OmniResult<T>, ContextMenu.Item[]> menu, OmniResultCommandsMode mode = OmniResultCommandsMode.RightClickOnly)
        {
            _menuGenerator   = menu;
            _commandsHandler = null;

            return CommandsMode(mode);
        }

        /// <summary>
        /// Changes how the commands registered with <c>OnContextMenu</c> are reached, without touching the
        /// handler itself.
        /// </summary>
        public OmniResult<T> CommandsMode(OmniResultCommandsMode mode)
        {
            if (mode == OmniResultCommandsMode.RightClickOnly)
            {
                if (_menuButton is object)
                {
                    _commandsContainer.removeChild(_menuButton);
                    _menuButton = null;
                }
            }
            else
            {
                EnsureMenuButton();
            }

            _commandsContainer.UpdateClassIf(mode == OmniResultCommandsMode.ButtonAlwaysVisible, "tss-omniresult-commands-visible");

            return UpdateCommandsVisibility();
        }

        /// <summary>
        /// Puts the given components in the row's command area, before the [...] button - the one or two
        /// actions worth reaching without opening a menu. They show only while the row is hovered by
        /// default; the space they take is reserved either way, so revealing them never shifts the row.
        /// </summary>
        public OmniResult<T> InlineCommands(OmniResultCommandsVisibility visibility, params IComponent[] commands)
        {
            ClearChildren(_inlineCommandsContainer);

            if (commands != null)
            {
                foreach (var command in commands)
                {
                    if (command != null) _inlineCommandsContainer.appendChild(command.Render());
                }
            }

            _inlineCommandsContainer.UpdateClassIf(visibility == OmniResultCommandsVisibility.AlwaysVisible, "tss-omniresult-commands-visible");

            return UpdateCommandsVisibility();
        }

        /// <summary>
        /// Puts the given components in the row's command area, revealed while the row is hovered.
        /// </summary>
        public OmniResult<T> InlineCommands(params IComponent[] commands) => InlineCommands(OmniResultCommandsVisibility.OnHover, commands);

        /// <summary>
        /// Puts a <see cref="Tesserae.ContributionBar"/> under the footer, spanning the text column: what
        /// the result's score is made of - a title match, a content match, recency, popularity - as one
        /// stacked bar. Pass null to take it away.
        /// <para>
        /// A bar with many segments is worth collapsing (<see cref="Tesserae.ContributionBar.Collapsable"/>),
        /// so a list of results reads as one line each until the breakdown is asked for.
        /// </para>
        /// </summary>
        public OmniResult<T> SetContributionBar(ContributionBar bar)
        {
            ClearChildren(_contributionContainer);

            _contribution = bar;

            if (bar is object) _contributionContainer.appendChild(bar.Render());

            _contributionContainer.style.display = bar is null ? "none" : "";

            return this;
        }

        /// <summary>
        /// Pins the given <see cref="PagesStack"/> preview to the end of the row, in a rail wide enough for
        /// it to fan into. Pass null to take it away.
        /// </summary>
        public OmniResult<T> SetPages(PagesStack pages)
        {
            ClearChildren(_railContainer);

            _pages = pages;

            if (pages is object) _railContainer.appendChild(pages.Render());

            _railContainer.style.display = pages is null ? "none" : "";

            return this;
        }

        /// <summary>
        /// Configures whether the <see cref="PagesStack"/> fans while the row is hovered, rather than only
        /// while the pointer is over the pages themselves. On by default.
        /// </summary>
        public OmniResult<T> PagesFanOnHover(bool value = true)
        {
            _pagesFanOnHover = value;

            if (!value) _pages?.Fanned(false);

            return this;
        }

        /// <summary>
        /// Gets the pointer event that last asked for this result's commands - the right-click, or the click
        /// on the [...] button - or null when they were asked for from the keyboard. A host that shows its own
        /// command surface (rather than a <see cref="ContextMenu"/> through <see cref="ShowMenu"/>) reads this
        /// to place it where the user asked.
        /// </summary>
        public MouseEvent CommandsEvent => _lastPointerEvent;

        /// <summary>
        /// Gets a value indicating whether the result has content to open as a modal
        /// (see <see cref="ToModal"/>).
        /// </summary>
        public bool HasModalContent => _modalContent is object;

        /// <summary>
        /// Gets the ways this result can be opened where it actually lives, in the order they were added -
        /// the first one is the primary, and the rest hang off the arrow beside it.
        /// </summary>
        public IReadOnlyList<OmniResultOpenAction<T>> OpenActions => _openActions;

        /// <summary>
        /// Gets a value indicating whether the result can be opened at its source.
        /// </summary>
        public bool CanOpenInSource => _openActions.Count > 0;

        /// <summary>
        /// Adds a way to open the result where it actually lives - "Open in Dropbox", "Open in Outlook",
        /// "Reveal in folder" - as a named button in the modal's header. The handler is told whether the
        /// user asked for a new tab (they shift-clicked the button, or pressed Shift+Enter).
        /// <para>
        /// Call it more than once to offer several: the first one stays the button, and the rest are reached
        /// through an arrow beside it.
        /// </para>
        /// </summary>
        public OmniResult<T> OpenInSource(string name, Action<bool> onOpen, UIcons? icon = null)
        {
            return OpenInSource(name, onOpen, icon is null ? null : (Func<IComponent>)(() => Icon(icon.Value)));
        }

        /// <summary>
        /// Adds a way to open the result where it actually lives, marked with an icon of the host's own -
        /// the source's logo, usually. The factory runs every time the icon is drawn, so one action can be
        /// shown more than once without the two fighting over the same element.
        /// </summary>
        public OmniResult<T> OpenInSource(string name, Action<bool> onOpen, Func<IComponent> icon)
        {
            if (onOpen is null) return this;

            _openActions.Add(new OmniResultOpenAction<T>(name, icon, onOpen, null));

            return this;
        }

        /// <summary>
        /// Adds a way to open the result at an address computed from what it stands for - the usual shape of
        /// "open this where it came from" when the source is a web address. The result is opened in a new
        /// tab either way: an external address replacing the page the user is on would lose their place.
        /// </summary>
        public OmniResult<T> OpenInSource(string name, Func<T, Uri> url, UIcons? icon = null)
        {
            return OpenInSource(name, url, icon is null ? null : (Func<IComponent>)(() => Icon(icon.Value)));
        }

        /// <summary>
        /// Adds a way to open the result at a computed address, marked with an icon of the host's own.
        /// </summary>
        public OmniResult<T> OpenInSource(string name, Func<T, Uri> url, Func<IComponent> icon)
        {
            if (url is null) return this;

            _openActions.Add(new OmniResultOpenAction<T>(name, icon, null, url));

            return this;
        }

        /// <summary>
        /// Takes every way of opening the result at its source away again.
        /// </summary>
        public OmniResult<T> NoOpenInSource()
        {
            _openActions.Clear();

            return this;
        }

        /// <summary>
        /// Opens the result at its source the way the primary action says to, as pressing the button (or
        /// Ctrl+Enter in the modal) would. Does nothing on a result that has no such action.
        /// </summary>
        public OmniResult<T> Open(bool inNewTab = false)
        {
            if (_openActions.Count > 0) _openActions[0].Invoke(this, inNewTab);

            return this;
        }

        /// <summary>
        /// Registers what the [...] button in the modal's header opens - the same commands the row's own
        /// [...] opens, usually. Read <see cref="CommandsEvent"/> in the handler to place a command surface
        /// of the host's own where the user clicked. Pass null to leave the button out.
        /// </summary>
        public OmniResult<T> ModalCommands(Action<OmniResult<T>> onCommands)
        {
            _modalCommands = onCommands;

            return this;
        }

        /// <summary>
        /// Registers what the full-screen button in the modal's header does - open the result on a page of
        /// its own, usually. Without one the button is still there and simply grows the modal to fill the
        /// window (and back), which is what "full screen" means to a modal that has nowhere else to go.
        /// </summary>
        public OmniResult<T> ModalFullScreen(Action<OmniResult<T>> onFullScreen)
        {
            _modalFullScreen    = onFullScreen;
            _modalHasFullScreen = true;

            return this;
        }

        /// <summary>
        /// Leaves the full-screen button out of the modal's header.
        /// </summary>
        public OmniResult<T> NoModalFullScreen()
        {
            _modalHasFullScreen = false;
            _modalFullScreen    = null;

            return this;
        }

        /// <summary>
        /// Puts the previous/next arrows in the modal's header, so a result opened out of a list can be
        /// stepped through without going back to it. A null handler greys its arrow out - that is how the
        /// first and the last result say so. Passing a position and a count (both 1-based, the count being
        /// how many results there are) draws "3 of 27" between the arrows, and a <paramref name="format"/>
        /// writes that label another way - "3 / 27", or "3 of many" for a count too large to be worth
        /// spelling out.
        /// </summary>
        public OmniResult<T> ModalNavigation(
            Action<OmniResult<T>>  onPrevious,
            Action<OmniResult<T>>  onNext,
            int                    position = 0,
            int                    count    = 0,
            Func<int, int, string> format   = null)
        {
            _modalPrevious    = onPrevious;
            _modalNext        = onNext;
            _modalPosition    = position;
            _modalCount       = count;
            _modalCountFormat = format;

            return this;
        }

        /// <summary>
        /// Configures whether the modal shows the keyboard shortcuts it answers along its bottom edge - what
        /// closes it, what steps through the results, what opens the result at its source. On by default.
        /// </summary>
        public OmniResult<T> ModalShortcuts(bool value = true)
        {
            _modalShortcuts = value;

            return this;
        }

        /// <summary>
        /// Sets what <see cref="ToModal"/> puts inside the modal for this result - the full view of the thing
        /// the row stands for. Pass null to make the result modal-less again.
        /// </summary>
        public OmniResult<T> SetModalContent(IComponent content)
        {
            _modalContent = content is null ? (Func<OmniResult<T>, Task<IComponent>>)null : (_ => Task.FromResult(content));

            return this;
        }

        /// <summary>
        /// Sets what <see cref="ToModal"/> puts inside the modal, built on open and given the result - for
        /// content that shouldn't be paid for until someone asks to see it.
        /// </summary>
        public OmniResult<T> SetModalContent(Func<OmniResult<T>, Task<IComponent>> content)
        {
            _modalContent = content;

            return this;
        }

        /// <summary>
        /// Builds the modal content on its own, for a host that shows it somewhere other than in a modal - a
        /// side panel, a page, a pane of its own. Returns null when the result has no modal content.
        /// </summary>
        public Task<IComponent> GetModalContentAsync() => _modalContent is null ? Task.FromResult((IComponent)null) : _modalContent(this);

        /// <summary>
        /// Replaces the modal's header - by default the same identifier, title and badge the row shows - with
        /// one built from the result. Pass null to go back to the default header.
        /// </summary>
        public OmniResult<T> SetModalHeader(Func<OmniResult<T>, IComponent> header)
        {
            _modalHeader = header;

            return this;
        }

        /// <summary>
        /// Gets a value indicating whether a header of the host's own was set with
        /// <see cref="SetModalHeader(Func{OmniResult{T}, IComponent})"/> - so a caller applying a default one
        /// can tell whether anyone got there first.
        /// </summary>
        public bool HasModalHeader => _modalHeader is object;

        /// <summary>
        /// Sets the size the modal of <see cref="ToModal"/> opens at. Auto by default, which lets the modal
        /// size itself to its content (and to whatever bounds the caller sets on it afterwards).
        /// </summary>
        public OmniResult<T> ModalSize(UnitSize width, UnitSize height)
        {
            _modalWidth  = width  ?? UnitSize.Auto();
            _modalHeight = height ?? UnitSize.Auto();

            return this;
        }

        /// <summary>
        /// Keeps the icon tile in the modal's header, before the identifier and the title - so an opened
        /// result still shows what kind of thing it is, and the row and the modal read as one thing rather
        /// than two. Whatever the tile carries comes with it: the glyph or the thumbnail, the color it is
        /// tinted with, and any corner badges. Off by default.
        /// </summary>
        public OmniResult<T> ModalKeepsIcon(bool value = true)
        {
            _modalKeepsIcon = value;

            return this;
        }

        /// <summary>
        /// Keeps the footer - the source and the metadata beside it - as a second line under the title in the
        /// modal's header, so where a result came from is still said once it is open. Off by default.
        /// <para>
        /// The modal shows the row's own footer line, not a snapshot of it: entries that only arrive once a
        /// query answers show up in the open modal, and every entry stays as clickable as it is in the row.
        /// The row keeps a copy of the line in its place while the modal holds the original, and gets the
        /// original back when the modal hides.
        /// </para>
        /// </summary>
        public OmniResult<T> ModalKeepsFooter(bool value = true)
        {
            _modalKeepsFooter = value;

            return this;
        }

        /// <summary>
        /// Builds a <see cref="Tesserae.Modal"/> showing this result: the row's identifier, title and badge as
        /// the header, and whatever <c>SetModalContent</c> was given as the body, at the size
        /// <see cref="ModalSize(UnitSize, UnitSize)"/> asked for. Everything else - commands, dismissal,
        /// bounds, how it is shown - is left to the caller to chain on the returned modal.
        /// <para>
        /// Returns null when the result has no modal content, so a caller can treat "this result has no
        /// preview" as one check.
        /// </para>
        /// </summary>
        public Modal ToModal()
        {
            if (_modalContent is null) return null;

            var content = _modalContent;

            var modal = UI.Modal()
               .Class("tss-omniresult-modal")
               .SetHeader(_modalHeader is object ? _modalHeader(this) : ModalTitle())
               .Width(_modalWidth)
               .Height(_modalHeight);

            _modal = modal;

            modal.HideCloseButton().SetHeaderCommands(ModalHeaderCommands());

            //The header borrowed the row's own footer line; the row gets it back when the sheet closes.
            if (_modalKeepsFooter) modal.OnHide(_ => ReturnFooterToRow());

            if (_modalShortcuts) modal.SetFooter(ModalShortcutsBar());

            modal.Content(Defer(() => content(this)).WS());

            HookModalKeys(modal);

            return modal;
        }

        /// <summary>
        /// Gets the modal <see cref="ToModal"/> last built for this result, or null when it has not built
        /// one yet - what a host reaches for to close, resize or otherwise get at the surface it opened.
        /// </summary>
        public Modal CurrentModal => _modal;

        /// <summary>
        /// The commands <see cref="ToModal"/> puts at the end of the modal's header: the way to open the
        /// result at its source, the arrows stepping through the results, the [...] commands, the
        /// full-screen button and the close button - whichever of them this result was configured for.
        /// Useful to a caller building a header of its own around them.
        /// </summary>
        public IComponent[] ModalHeaderCommands()
        {
            var commands = new List<IComponent>();

            var open = BuildOpenInSource();

            if (open is object) commands.Add(open);

            var navigation = BuildModalNavigation();

            if (navigation is object) commands.Add(navigation);

            if (_modalCommands is object)
            {
                commands.Add(Raw(ModalButton(UIcons.MenuDots, "More commands", e =>
                {
                    _lastPointerEvent = e;

                    _modalCommands(this);
                })));
            }

            if (_modalHasFullScreen)
            {
                commands.Add(Raw(ModalButton(UIcons.ArrowUpRightAndArrowDownLeftFromCenter, "Open full screen", _ => ToggleModalFullScreen())));
            }

            commands.Add(Raw(ModalButton(UIcons.Cross, "Close", _ => _modal?.Hide())));

            return commands.ToArray();
        }

        /// <summary>
        /// The keyboard shortcuts <see cref="ToModal"/> lists along the bottom of the modal - only the ones
        /// this result actually answers, so a modal that can't be stepped through never says it can.
        /// </summary>
        public IComponent ModalShortcutsBar()
        {
            var bar = Div(Att("tss-omniresult-modal-shortcuts"));

            bar.appendChild(Shortcut("Close", KeyboardShortcut("Esc")));

            if (_modalPrevious is object || _modalNext is object)
            {
                bar.appendChild(Shortcut("Navigate results", KeyboardShortcut("←"), KeyboardShortcut("→")));
            }

            if (_openActions.Count > 0)
            {
                bar.appendChild(Shortcut("Open in source", KeyboardShortcut("Ctrl", "Enter")));
                bar.appendChild(Shortcut("Open in a new tab", KeyboardShortcut("Shift", "Enter")));
            }

            return Raw(bar);
        }

        private static HTMLElement Shortcut(string label, params IComponent[] keys)
        {
            var shortcut = Div(Att("tss-omniresult-modal-shortcut"));

            foreach (var key in keys)
            {
                shortcut.appendChild(key.Render());
            }

            shortcut.appendChild(Span(Att("tss-omniresult-modal-shortcut-label", text: label)));

            return shortcut;
        }

        private IComponent BuildOpenInSource()
        {
            if (_openActions.Count == 0) return null;

            var primary = _openActions[0];

            var button = UI.Button(Att("tss-omniresult-modal-open-primary", type: "button", title: primary.Name));

            if (primary.Icon is object) button.appendChild(Div(Att("tss-omniresult-modal-open-icon"), primary.Icon().Render()));

            button.appendChild(Span(Att("tss-omniresult-modal-open-text", text: primary.Name)));

            button.addEventListener("click", e =>
            {
                StopEvent(e);

                // Shift-clicking asks for a new tab, the same as Shift+Enter does.
                primary.Invoke(this, e.As<MouseEvent>().shiftKey);
            });

            var holder = Div(Att("tss-omniresult-modal-open"), button);

            if (_openActions.Count == 1) return Raw(holder);

            var more = UI.Button(Att("tss-omniresult-modal-open-more", type: "button", ariaLabel: "More ways to open", title: "More ways to open"), I(UIcons.AngleSmallDown, UIconsWeight.Regular));

            more.addEventListener("click", e =>
            {
                StopEvent(e);

                ShowOpenInSourceMenu(more);
            });

            holder.appendChild(more);

            return Raw(holder);
        }

        private void ShowOpenInSourceMenu(HTMLElement anchor)
        {
            var menu = UI.ContextMenu();

            for (int i = 1; i < _openActions.Count; i++)
            {
                var action = _openActions[i];

                var item = action.Icon is object
                    ? ContextMenuItem(HStack().AlignItemsCenter().Children(action.Icon().PR(8), TextBlock(action.Name)))
                    : ContextMenuItem(action.Name);

                menu.Add(item.OnClick(() => action.Invoke(this, false)));
            }

            menu.ShowFor(anchor);
        }

        private IComponent BuildModalNavigation()
        {
            if (_modalPrevious is null && _modalNext is null) return null;

            return InlinePagination(_modalPosition, _modalCount)
               .Class("tss-omniresult-modal-nav")
               .SetFormat(_modalCountFormat)
               .SetTooltips("Previous result", "Next result")
               .OnPrevious(_modalPrevious is null ? null : (Action<InlinePagination>)(_ => _modalPrevious(this)))
               .OnNext(_modalNext is null ? null : (Action<InlinePagination>)(_ => _modalNext(this)));
        }

        private static HTMLButtonElement ModalButton(UIcons icon, string label, Action<MouseEvent> onClick)
        {
            var button = UI.Button(Att("tss-omniresult-modal-button", type: "button", ariaLabel: label, title: label), I(icon, UIconsWeight.Regular));

            button.addEventListener("click", e =>
            {
                StopEvent(e);

                onClick(e.As<MouseEvent>());
            });

            return button;
        }

        private void ToggleModalFullScreen()
        {
            if (_modalFullScreen is object)
            {
                _modalFullScreen(this);
                return;
            }

            if (_modal is null) return;

            var surface = _modal.StylingContainer;

            surface.UpdateClassIf(!surface.classList.contains("tss-omniresult-modal-full"), "tss-omniresult-modal-full");
        }

        // The modal answers the shortcuts its footer says it does. Escape is left to ModalStack when the
        // modal is one of its sheets, so a chain of them closes one sheet per press rather than all of them.
        private void HookModalKeys(Modal modal)
        {
            modal.StylingContainer.addEventListener("keydown", e =>
            {
                var keyboardEvent = e.As<KeyboardEvent>();

                if (IsEditing(keyboardEvent.target.As<HTMLElement>())) return;

                if (keyboardEvent.key == "Escape")
                {
                    if (ModalStack.IsStacked(modal)) return;

                    StopEvent(keyboardEvent);
                    modal.Hide();
                }
                else if (keyboardEvent.key == "ArrowLeft" && _modalPrevious is object)
                {
                    StopEvent(keyboardEvent);
                    _modalPrevious(this);
                }
                else if (keyboardEvent.key == "ArrowRight" && _modalNext is object)
                {
                    StopEvent(keyboardEvent);
                    _modalNext(this);
                }
                else if (keyboardEvent.key == "Enter" && _openActions.Count > 0 && (keyboardEvent.ctrlKey || keyboardEvent.metaKey || keyboardEvent.shiftKey))
                {
                    StopEvent(keyboardEvent);
                    _openActions[0].Invoke(this, keyboardEvent.shiftKey);
                }
            });
        }

        private static bool IsEditing(HTMLElement target)
        {
            if (target is null) return false;

            return target.isContentEditable || target.tagName == "INPUT" || target.tagName == "TEXTAREA" || target.tagName == "SELECT";
        }

        /// <summary>
        /// The header <see cref="ToModal"/> uses by default: the identifier and the title, drawn the way the
        /// row draws them, plus whatever <see cref="ModalKeepsIcon"/> and <see cref="ModalKeepsFooter"/>
        /// asked to keep. Useful to a caller building its own header around it.
        /// </summary>
        public IComponent ModalTitle()
        {
            var titleRow = Div(Att("tss-omniresult-modal-title"));

            if (!string.IsNullOrEmpty(_id))
            {
                titleRow.appendChild(Span(Att("tss-omniresult-id-value", text: _id)));
                titleRow.appendChild(I(UIcons.AngleRight, UIconsWeight.Regular, "tss-omniresult-id-chevron"));
            }

            titleRow.appendChild(Span(Att("tss-omniresult-modal-title-text", text: _title, title: _title)));

            var main = Div(Att("tss-omniresult-modal-main"), titleRow);

            //Not gated on the footer having entries yet: the line hides itself while it is empty, so one that
            //is still waiting on a query shows up in the open modal once the query answers.
            if (_modalKeepsFooter)
            {
                main.appendChild(FooterForModal());
            }

            var header = Div(Att("tss-omniresult-modal-header"));

            if (_modalKeepsIcon) header.appendChild(CopyOfIcon());

            header.appendChild(main);

            return Raw(header);
        }

        // The modal shows the row's own footer rather than a copy of it: an entry that only arrives once a
        // query answers lands in the open modal, and the handlers on the entries already there still answer.
        // A copy takes its place in the row behind - so nothing disappears from under the sheet - and the row
        // gets the original back when the modal hides.
        private HTMLElement FooterForModal()
        {
            if (_footerContainer.parentElement == _mainContainer)
            {
                _footerStandIn = CopyOfFooter();

                _mainContainer.replaceChild(_footerStandIn, _footerContainer);
            }

            return _footerContainer;
        }

        private void ReturnFooterToRow()
        {
            if (_footerStandIn is null) return;

            var standIn = _footerStandIn;

            _footerStandIn = null;

            //The modal keeps a copy of the line it is handing back, so showing it again doesn't show a header
            //with a gap where the footer was.
            _footerContainer.parentElement?.replaceChild(CopyOfFooter(), _footerContainer);

            standIn.parentElement?.replaceChild(_footerContainer, standIn);

            UpdateFooterVisibility();
        }

        // The row keeps its own tile - the modal gets a copy of it, so opening a result never takes the tile
        // out of the row behind it.
        private HTMLElement CopyOfIcon()
        {
            var copy = _iconHolder.cloneNode(true).As<HTMLElement>();

            copy.classList.add("tss-omniresult-modal-icon");

            return copy;
        }

        private HTMLElement CopyOfFooter()
        {
            var copy = _footerContainer.cloneNode(true).As<HTMLElement>();

            copy.style.display = "";

            // A clone carries no listeners, so a clickable source is re-hooked onto the copy rather than
            // silently becoming plain text in the modal.
            if (_sourceClickHandler is object)
            {
                var source = copy.querySelector(".tss-omniresult-source").As<HTMLElement>();

                if (source is object)
                {
                    source.addEventListener("click", e =>
                    {
                        StopEvent(e);

                        _sourceClickHandler(this);
                    });

                    source.addEventListener("keydown", e =>
                    {
                        var keyboardEvent = e.As<KeyboardEvent>();

                        if (keyboardEvent.key != "Enter" && keyboardEvent.key != " ") return;

                        StopEvent(keyboardEvent);

                        _sourceClickHandler(this);
                    });
                }
            }

            return copy;
        }

        /// <summary>
        /// Shows the given menu where the commands were asked for: at the pointer when the row was
        /// right-clicked, and under the [...] button when it was pressed. This is what a
        /// <see cref="OnContextMenu(Action{OmniResult{T}}, OmniResultCommandsMode)"/> handler uses to put its
        /// menu in the right place without tracking the event itself.
        /// </summary>
        public OmniResult<T> ShowMenu(ContextMenu menu)
        {
            if (menu is null) return this;

            InnerElement.classList.add("tss-omniresult-menu-open");

            menu.OnHide(() => InnerElement.classList.remove("tss-omniresult-menu-open"));

            if (_lastPointerEvent is object)
            {
                menu.ShowAt((int)_lastPointerEvent.clientX, (int)_lastPointerEvent.clientY, 0);
            }
            else if (_menuButton is object)
            {
                menu.ShowFor(_menuButton);
            }
            else
            {
                menu.ShowFor(InnerElement);
            }

            return this;
        }

        private void HookEvents()
        {
            InnerElement.setAttribute("tabindex", "0");
            InnerElement.setAttribute("role", "option");

            // The contribution bar rebuilds itself when its toggle is pressed, so by the time the click
            // reaches the row the element it started on is detached and walking up from it finds nothing.
            // Stopping here instead keeps expanding a breakdown from counting as opening the result.
            _contributionContainer.addEventListener("click", e =>
            {
                if (_contribution is null) return;

                StopEvent(e);
            });

            InnerElement.addEventListener("click", e =>
            {
                var mouseEvent = e.As<MouseEvent>();

                // A click on the checkbox, or on a command, is that control's business - not the row's.
                if (IsWithinControl(mouseEvent)) return;

                // On a row whose text can be selected, the click that ends a drag across it is the end of a
                // selection rather than a click on the row: opening the result under the text someone is
                // copying out of it is never what they asked for.
                if (_textSelectable && HasTextSelectionInside()) return;

                if (_selectionEnabled && mouseEvent.ctrlKey)
                {
                    StopEvent(mouseEvent);
                    ClearTextSelection();
                    IsSelected = !IsSelected;
                    return;
                }

                if (_selectionEnabled && mouseEvent.shiftKey)
                {
                    StopEvent(mouseEvent);
                    ClearTextSelection();
                    RangeSelectionRequested?.Invoke(this);
                    return;
                }

                RaiseOnClick(mouseEvent);
            });

            InnerElement.addEventListener("mousedown", e => _lastPointerEvent = e.As<MouseEvent>());

            InnerElement.addEventListener("contextmenu", e =>
            {
                var mouseEvent = e.As<MouseEvent>();

                _lastPointerEvent = mouseEvent;

                if (HasCommands())
                {
                    StopEvent(mouseEvent);
                    OpenCommands();
                    return;
                }

                RaiseOnContextMenu(mouseEvent);
            });

            InnerElement.addEventListener("mouseenter", e =>
            {
                if (_pagesFanOnHover) _pages?.Fanned();

                RaiseOnMouseOver(e.As<MouseEvent>());
            });

            InnerElement.addEventListener("mouseleave", e =>
            {
                if (_pagesFanOnHover) _pages?.Fanned(false);

                RaiseOnMouseOut(e.As<MouseEvent>());
            });

            InnerElement.addEventListener("keydown", e =>
            {
                var keyboardEvent = e.As<KeyboardEvent>();

                if (keyboardEvent.key == "Enter")
                {
                    StopEvent(keyboardEvent);
                    InnerElement.click();
                }
                else if (keyboardEvent.key == " " && _selectionEnabled)
                {
                    StopEvent(keyboardEvent);
                    IsSelected = !IsSelected;
                }
                else if ((keyboardEvent.key == "ContextMenu" || (keyboardEvent.key == "F10" && keyboardEvent.shiftKey)) && HasCommands())
                {
                    StopEvent(keyboardEvent);
                    _lastPointerEvent = null;
                    OpenCommands();
                }
            });
        }

        private bool HasCommands() => _commandsHandler is object || _menuGenerator is object;

        private void OpenCommands()
        {
            if (_menuGenerator is object)
            {
                var items = _menuGenerator(this);

                if (items is null || items.Length == 0) return;

                ShowMenu(UI.ContextMenu().Items(items));

                return;
            }

            _commandsHandler?.Invoke(this);
        }

        // Clicks inside the checkbox, a command, or the contribution bar (whose toggle opens the
        // breakdown) are handled by that control, and must not also count as a click on (or a selection
        // of) the row itself.
        private bool IsWithinControl(MouseEvent e)
        {
            var target = e.target.As<HTMLElement>();

            while (target is object && target != InnerElement)
            {
                if (target.classList.contains("tss-omniresult-select")
                 || target.classList.contains("tss-omniresult-commands")
                 || target.classList.contains("tss-omniresult-contribution"))
                {
                    return true;
                }

                target = target.parentElement;
            }

            return false;
        }

        private static void ClearTextSelection() => window.getSelection()?.removeAllRanges();

        private bool HasTextSelectionInside()
        {
            var selection = window.getSelection();

            if (selection is null || selection.isCollapsed) return false;

            //What a selection is anchored to is the text node itself, so the row is asked about the element
            //holding it - and contains() answers for the element itself as well as for its descendants.
            var anchor = selection.anchorNode?.parentElement;

            return anchor is object && InnerElement.contains(anchor);
        }

        private void EnsureCheckBox()
        {
            if (_checkBox is object) return;

            _checkBox = CheckBox().Class("tss-omniresult-checkbox");

            _checkBox.OnChange((cb, _) => IsSelected = cb.IsChecked);

            _selectContainer.appendChild(_checkBox.Render());
        }

        private void ApplySelectionMode()
        {
            InnerElement.UpdateClassIf(_selectionEnabled, "tss-omniresult-selectable");

            if (_selectionEnabled)
            {
                InnerElement.setAttribute("aria-selected", IsSelected ? "true" : "false");
            }
            else
            {
                InnerElement.removeAttribute("aria-selected");
            }

            InnerElement.classList.remove("tss-omniresult-select-hover-before",
                "tss-omniresult-select-hover-over",
                "tss-omniresult-select-always-before",
                "tss-omniresult-select-replacing");

            if (!_selectionEnabled) return;

            switch (_selectionMode)
            {
                case OmniResultSelectionMode.OnHoverBeforeIcon:  InnerElement.classList.add("tss-omniresult-select-hover-before"); break;
                case OmniResultSelectionMode.OnHoverOverIcon:    InnerElement.classList.add("tss-omniresult-select-hover-over"); break;
                case OmniResultSelectionMode.AlwaysBeforeIcon:   InnerElement.classList.add("tss-omniresult-select-always-before"); break;
                case OmniResultSelectionMode.ReplacingIcon:      InnerElement.classList.add("tss-omniresult-select-replacing"); break;
            }
        }

        private void EnsureMenuButton()
        {
            if (_menuButton is object) return;

            _menuButton = UI.Button(Att("tss-omniresult-menu-button", type: "button", ariaLabel: "More commands"), I(UIcons.MenuDots, UIconsWeight.Solid));

            _menuButton.addEventListener("click", e =>
            {
                StopEvent(e);

                _lastPointerEvent = e.As<MouseEvent>();

                OpenCommands();
            });

            _commandsContainer.appendChild(_menuButton);
        }

        private OmniResult<T> UpdateCommandsVisibility()
        {
            var isEmpty = _menuButton is null && _inlineCommandsContainer.childElementCount == 0;

            _commandsContainer.style.display = isEmpty ? "none" : "";

            return this;
        }

        private OmniResult<T> UpdateFooterVisibility()
        {
            _footerContainer.style.display = _footerContainer.childElementCount == 0 ? "none" : "";

            return this;
        }

        private void RenderTitle()
        {
            _titleElement.setAttribute("title", _title ?? string.Empty);
            _titleElement.UpdateClassIf(_titleComponent is object, "tss-omniresult-title-custom");

            if (_titleComponent is null)
            {
                RenderHighlighted(_titleElement, _title);
                return;
            }

            ClearChildren(_titleElement);

            _titleElement.appendChild(_titleComponent.Render());
        }

        private void RenderText()
        {
            var isEmpty = string.IsNullOrEmpty(_text);

            _bodyContainer.style.display = isEmpty ? "none" : "";

            RenderHighlighted(_bodyContainer, _text);
        }

        // Text is built out of text nodes and marker spans, never out of markup, so a title or an excerpt
        // that happens to contain angle brackets renders them rather than obeying them.
        private void RenderHighlighted(HTMLElement target, string text)
        {
            ClearChildren(target);

            if (string.IsNullOrEmpty(text)) return;

            if (_highlighter is null)
            {
                target.textContent = text;
                return;
            }

            var position = 0;
            var matches  = _highlighter.Matches(text);

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];

                if (match.Length == 0) continue;

                if (match.Index > position)
                {
                    target.appendChild(document.createTextNode(text.Substring(position, match.Index - position)));
                }

                target.appendChild(Span(Att("tss-omniresult-highlight", text: match.Value)));

                position = match.Index + match.Length;
            }

            if (position < text.Length)
            {
                target.appendChild(document.createTextNode(text.Substring(position)));
            }
        }
    }

    /// <summary>
    /// One named way of opening an <see cref="OmniResult{T}"/> where it actually lives - "Open in Dropbox",
    /// "Open in Outlook", "Reveal in folder" - either as something the host does itself or as an address
    /// computed from the result.
    /// </summary>
    [Transpose.Name("tss.OmniResultOpenAction")]
    public sealed class OmniResultOpenAction<T>
    {
        private readonly Action<bool> _handler;
        private readonly Func<T, Uri> _url;

        internal OmniResultOpenAction(string name, Func<IComponent> icon, Action<bool> handler, Func<T, Uri> url)
        {
            Name     = name ?? string.Empty;
            Icon     = icon;
            _handler = handler;
            _url     = url;
        }

        /// <summary>Gets what this way of opening the result is called.</summary>
        public string Name { get; }

        /// <summary>
        /// Gets what draws the mark shown before the name, or null when it has none. It is a factory rather
        /// than a component so that showing the action twice never moves one element between two places.
        /// </summary>
        public Func<IComponent> Icon { get; }

        /// <summary>
        /// Gets the address this action opens for the given result, or null when it isn't an address at all
        /// but something the host does itself.
        /// </summary>
        public Uri UrlFor(OmniResult<T> result) => _url is null || result is null ? null : _url(result.Result);

        /// <summary>
        /// Opens the given result this way. An address is always opened in a new tab, whatever was asked for:
        /// replacing the page with somewhere else entirely would lose the user's place.
        /// </summary>
        public void Invoke(OmniResult<T> result, bool inNewTab = false)
        {
            if (_handler is object)
            {
                _handler(inNewTab);
                return;
            }

            var url = UrlFor(result);

            if (url is null) return;

            window.open(url.ToString(), "_blank", "noopener,noreferrer");
        }
    }

}
