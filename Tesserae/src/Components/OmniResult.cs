using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static Transpose.Core.dom;
using Transpose.Core;
using static Tesserae.UI;

namespace Tesserae
{
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
    public sealed class OmniResult<T> : ComponentBase<OmniResult<T>, HTMLElement>
    {
        private readonly HTMLElement _selectContainer;
        private readonly HTMLElement _iconContainer;
        private readonly HTMLElement _titleElement;
        private readonly HTMLElement _badgeContainer;
        private readonly HTMLElement _headerContainer;
        private readonly HTMLElement _bodyContainer;
        private readonly HTMLElement _sourceContainer;
        private readonly HTMLElement _sourceSquare;
        private readonly HTMLElement _sourceText;
        private readonly HTMLElement _footerContainer;
        private readonly HTMLElement _contributionContainer;
        private readonly HTMLElement _mainContainer;
        private readonly HTMLElement _railContainer;
        private readonly HTMLElement _commandsContainer;
        private readonly HTMLElement _inlineCommandsContainer;

        private CheckBox          _checkBox;
        private ContributionBar   _contribution;
        private HTMLButtonElement _menuButton;
        private PagesStack        _pages;

        private string                       _title;
        private string                       _text;
        private Regex                        _highlighter;
        private OmniResultSelectionMode      _selectionMode = OmniResultSelectionMode.OnHoverBeforeIcon;
        private bool                         _selectionEnabled;
        private bool                         _pagesFanOnHover = true;
        private Action<OmniResult<T>>        _commandsHandler;
        private Action<OmniResult<T>>        _sourceClickHandler;
        private Func<OmniResult<T>, ContextMenu.Item[]> _menuGenerator;
        private MouseEvent                   _lastPointerEvent;

        private event Action<OmniResult<T>, bool> SelectionChanged;
        private event Action<OmniResult<T>>       RangeSelectionRequested;

        /// <summary>
        /// Initializes a new instance of this class standing for the given result.
        /// </summary>
        public OmniResult(T result, string title = null)
        {
            Result = result;

            _selectContainer = Div(Att("tss-omniresult-select"));
            _iconContainer   = Div(Att("tss-omniresult-icon"));

            _titleElement   = Span(Att("tss-omniresult-title"));
            _badgeContainer = Div(Att("tss-omniresult-badge"));

            _headerContainer = Div(Att("tss-omniresult-header"), _titleElement, _badgeContainer);

            _bodyContainer = Div(Att("tss-omniresult-body"));

            _sourceSquare    = Span(Att("tss-omniresult-source-square"));
            _sourceText      = Span(Att("tss-omniresult-source-text"));
            _sourceContainer = Div(Att("tss-omniresult-source"), _sourceSquare, _sourceText);

            _footerContainer = Div(Att("tss-omniresult-footer"));

            _contributionContainer = Div(Att("tss-omniresult-contribution"));

            _mainContainer = Div(Att("tss-omniresult-main"), _headerContainer, _bodyContainer, _footerContainer, _contributionContainer);

            _railContainer           = Div(Att("tss-omniresult-rail"));
            _inlineCommandsContainer = Div(Att("tss-omniresult-inline-commands"));
            _commandsContainer       = Div(Att("tss-omniresult-commands"), _inlineCommandsContainer);

            InnerElement = Div(Att("tss-omniresult"), _selectContainer, _iconContainer, _mainContainer, _railContainer, _commandsContainer);

            SetTitle(title);
            SetText(null);
            SetBadge((string)null);
            SetSource(null, null);
            SetContributionBar(null);

            HookEvents();
        }

        /// <summary>
        /// Gets the result this card stands for - the search hit, document, record or row it was built from.
        /// </summary>
        public T Result { get; }

        /// <summary>
        /// Gets or sets the title of the result.
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetTitle(value);
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
        /// Sets the title of the result. The title is ellipsized to one line, and carries the full text as
        /// its native tooltip.
        /// </summary>
        public OmniResult<T> SetTitle(string title)
        {
            _title = title ?? string.Empty;

            _titleElement.textContent = _title;
            _titleElement.setAttribute("title", _title);

            return this;
        }

        /// <summary>
        /// Puts the given icon on the tile, in the given color, over a paler wash of that same color. Pass
        /// the full-strength color the glyph should be - the background is computed from it (and cached), a
        /// light tint of it under a light theme and a deep one under a dark theme.
        /// </summary>
        public OmniResult<T> SetIcon(UIcons icon, string color = null, UIconsWeight weight = UIconsWeight.Regular)
        {
            ClearChildren(_iconContainer);

            _iconContainer.appendChild(I(icon, weight, "tss-omniresult-icon-glyph"));

            return TintIcon(color);
        }

        /// <summary>
        /// Puts the given short text on the tile in place of an icon - a file type, "PPTX" or "CSV", where
        /// no glyph says it as plainly - in the given color, over a paler wash of that same color.
        /// </summary>
        public OmniResult<T> SetIcon(string text, string color = null)
        {
            ClearChildren(_iconContainer);

            _iconContainer.appendChild(Span(Att("tss-omniresult-icon-text", text: text ?? string.Empty)));

            return TintIcon(color);
        }

        /// <summary>
        /// Puts the given component on the tile - an <see cref="Image"/> thumbnail, an <see cref="Avatar"/>,
        /// an emoji - optionally tinting the tile with the given color.
        /// </summary>
        public OmniResult<T> SetIcon(IComponent iconOrImage, string color = null)
        {
            ClearChildren(_iconContainer);

            if (iconOrImage != null) _iconContainer.appendChild(iconOrImage.Render());

            return TintIcon(color);
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
        /// Marks every match of the given expression in the excerpt - the same pattern a search backend
        /// hands back for highlighting. Matching is done against the text itself and the matches are wrapped
        /// in their own elements, so the excerpt is never treated as markup.
        /// </summary>
        public OmniResult<T> Highlight(Regex highlighter)
        {
            _highlighter = highlighter;

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
            var isEmpty = string.IsNullOrEmpty(text);

            _sourceText.textContent        = isEmpty ? string.Empty : text;
            _sourceSquare.style.background = color ?? string.Empty;

            // Only when one is given, so a later SetSource can't silently drop a handler an earlier
            // OnSourceClick registered. OnSourceClick(null) is how a source stops being clickable.
            if (onClick != null) OnSourceClick(onClick);

            // Detached rather than hidden when empty, so the dot separators - which are drawn by CSS off
            // :first-child - don't leave a leading dot in a footer that has no source.
            if (isEmpty)
            {
                if (_sourceContainer.parentElement is object) _footerContainer.removeChild(_sourceContainer);
            }
            else if (_sourceContainer.parentElement is null)
            {
                _footerContainer.insertBefore(_sourceContainer, _footerContainer.firstChild);
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

            var isClickable = onClick != null;

            _sourceContainer.UpdateClassIf(isClickable, "tss-omniresult-source-clickable");

            if (isClickable)
            {
                _sourceContainer.setAttribute("tabindex", "0");
                _sourceContainer.setAttribute("role", "button");
            }
            else
            {
                _sourceContainer.removeAttribute("tabindex");
                _sourceContainer.removeAttribute("role");
            }

            return this;
        }

        /// <summary>
        /// Sets the metadata shown after the source in the footer - a path, a size, an owner, a date - each
        /// entry separated from the next by a dot. Replaces whatever entries were there.
        /// </summary>
        public OmniResult<T> SetFooterEntries(params IComponent[] entries)
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
        /// Sets the metadata shown after the source in the footer, as plain text entries.
        /// </summary>
        public OmniResult<T> SetFooterEntries(params string[] entries)
        {
            if (entries is null) return SetFooterEntries((IComponent[])null);

            var components = new List<IComponent>();

            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry)) components.Add(TextBlock(entry).XSmall());
            }

            return SetFooterEntries(components.ToArray());
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

            // Listened for once, whether the source is clickable or not: with no handler the click keeps
            // bubbling and the row treats it as its own, which is what a plain source should do.
            _sourceContainer.addEventListener("click", e =>
            {
                if (_sourceClickHandler is null) return;

                StopEvent(e);

                _sourceClickHandler(this);
            });

            _sourceContainer.addEventListener("keydown", e =>
            {
                if (_sourceClickHandler is null) return;

                var keyboardEvent = e.As<KeyboardEvent>();

                if (keyboardEvent.key != "Enter" && keyboardEvent.key != " ") return;

                StopEvent(keyboardEvent);

                _sourceClickHandler(this);
            });

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

        private OmniResult<T> TintIcon(string color)
        {
            _iconContainer.classList.remove("tss-omniresult-icon-plain");

            if (string.IsNullOrEmpty(color))
            {
                _iconContainer.classList.add("tss-omniresult-icon-plain");
                return this;
            }

            var tint = OmniResultTints.For(color);

            _iconContainer.style.setProperty("--tss-omniresult-icon-background",      tint.Background);
            _iconContainer.style.setProperty("--tss-omniresult-icon-foreground",      tint.Foreground);
            _iconContainer.style.setProperty("--tss-omniresult-icon-background-dark", tint.BackgroundDark);
            _iconContainer.style.setProperty("--tss-omniresult-icon-foreground-dark", tint.ForegroundDark);

            return this;
        }

        // The excerpt is built out of text nodes and marker spans, never out of markup, so a result whose
        // text happens to contain angle brackets renders them rather than obeying them.
        private void RenderText()
        {
            ClearChildren(_bodyContainer);

            var isEmpty = string.IsNullOrEmpty(_text);

            _bodyContainer.style.display = isEmpty ? "none" : "";

            if (isEmpty) return;

            if (_highlighter is null)
            {
                _bodyContainer.textContent = _text;
                return;
            }

            var position = 0;
            var matches  = _highlighter.Matches(_text);

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];

                if (match.Length == 0) continue;

                if (match.Index > position)
                {
                    _bodyContainer.appendChild(document.createTextNode(_text.Substring(position, match.Index - position)));
                }

                _bodyContainer.appendChild(Span(Att("tss-omniresult-highlight", text: match.Value)));

                position = match.Index + match.Length;
            }

            if (position < _text.Length)
            {
                _bodyContainer.appendChild(document.createTextNode(_text.Substring(position)));
            }
        }
    }

    /// <summary>
    /// The colors an <see cref="OmniResult{T}"/> icon tile is drawn with, derived from the one color the
    /// host passed: the glyph in that color and the tile in a wash of it, in a light and a dark variant.
    /// </summary>
    internal sealed class OmniResultTint
    {
        internal OmniResultTint(string background, string foreground, string backgroundDark, string foregroundDark)
        {
            Background     = background;
            Foreground     = foreground;
            BackgroundDark = backgroundDark;
            ForegroundDark = foregroundDark;
        }

        internal string Background     { get; }
        internal string Foreground     { get; }
        internal string BackgroundDark { get; }
        internal string ForegroundDark { get; }
    }

    /// <summary>
    /// Computes - and remembers - the tile colors derived from a given icon color. A list of results
    /// usually draws the same handful of colors over and over (one per file type), and every one of them
    /// costs a parse and two HSL round-trips, so the results are cached by the color they came from.
    /// </summary>
    internal static class OmniResultTints
    {
        private static readonly Dictionary<string, OmniResultTint> _cache = new Dictionary<string, OmniResultTint>();

        internal static OmniResultTint For(string color)
        {
            if (_cache.TryGetValue(color, out var cached)) return cached;

            var tint = Compute(color);

            _cache[color] = tint;

            return tint;
        }

        private static OmniResultTint Compute(string color)
        {
            try
            {
                var parsed     = Color.FromString(color);
                var hue        = parsed.GetHue();
                var saturation = parsed.GetSaturation();
                var lightness  = parsed.GetBrightness();

                // Light theme: a pale wash of the color under the glyph, which keeps the color it was given.
                var background = Color.FromHsl(hue, Math.Min(saturation, 0.85f), 0.925f).ToHex();

                // Dark theme: the wash goes deep instead of pale, and the glyph is lifted until it reads
                // against it. A grey (unsaturated) color stays grey through both.
                var backgroundDark = Color.FromHsl(hue, Math.Min(saturation, 0.5f),  0.19f).ToHex();
                var foregroundDark = Color.FromHsl(hue, Math.Min(saturation, 0.85f), Math.Max(lightness, 0.68f)).ToHex();

                return new OmniResultTint(background, color, backgroundDark, foregroundDark);
            }
            catch (Exception)
            {
                // Not a color this can take apart (a gradient, a color function, an unknown keyword): mix it
                // down for the wash instead of computing one, and let the glyph keep it as it was given.
                return new OmniResultTint(
                    $"color-mix(in srgb, {color} 14%, transparent)",
                    color,
                    $"color-mix(in srgb, {color} 24%, transparent)",
                    color);
            }
        }
    }
}
