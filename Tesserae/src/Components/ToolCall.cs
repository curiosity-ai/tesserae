using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Inline tool-call indicator that expands accordion-style to show the
    /// associated content. The content component is created lazily the first
    /// time the user expands the tool call.
    /// </summary>
    [Transpose.Name("tss.ToolCall")]
    public sealed class ToolCall : ComponentBase<ToolCall, HTMLElement>
    {
        private readonly HTMLElement      _header;
        private readonly HTMLElement      _iconContainer;
        private readonly HTMLElement      _textContainer;
        private readonly HTMLElement      _chevron;
        private readonly HTMLElement      _content;
        private          UIcons           _icon;
        private          string           _text;
        private          Func<IComponent> _contentFactory;
        private          IComponent       _renderedContent;
        private          LiveProgress     _progress;
        private          bool             _isExpanded;
        private          bool             _expandable = true;
        private          Action<ToolCall> _onToggle;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ToolCall(UIcons icon, string text, Func<IComponent> contentFactory = null)
        {
            _icon           = icon;
            _text           = text ?? string.Empty;
            _contentFactory = contentFactory;

            _iconContainer = Div(Att("tss-toolcall-icon"), I(icon));
            _textContainer = Div(Att("tss-toolcall-text", text: _text));
            _chevron       = I(UIcons.AngleDown, cssClass: "tss-toolcall-chevron");

            _header = Div(Att("tss-toolcall-header", role: "button", ariaLabel: "Toggle tool call"),
                          _iconContainer, _textContainer, _chevron);

            _content = Div(Att("tss-toolcall-content"));
            _content.style.display = "none";

            InnerElement = Div(Att("tss-toolcall"), _header, _content);

            _header.addEventListener("click", _ =>
            {
                if (CanExpand) Toggle();
            });

            UpdateExpandableUI();
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ToolCall(UIcons icon, string text, IComponent content)
            : this(icon, text, content != null ? (Func<IComponent>)(() => content) : null)
        {
        }

        /// <summary>
        /// Gets or sets the icon shown by the component.
        /// </summary>
        public UIcons  Icon            => _icon;
        /// <summary>
        /// Gets or sets the text shown in the component.
        /// </summary>
        public string  Text            => _text;
        /// <summary>
        /// Gets or sets a value indicating whether the component is expanded.
        /// </summary>
        public bool    IsExpanded      => _isExpanded;
        /// <summary>
        /// Returns a value indicating whether the component has the given content.
        /// </summary>
        public bool    HasContent      => _contentFactory != null;

        // The expand affordance (chevron + clickable header) is only rendered when there is
        // actually something to show — a tool call without content stays a plain chip.
        private bool   CanExpand       => _expandable && _contentFactory != null;

        /// <summary>
        /// Returns a fresh IComponent built from the content factory, or null
        /// if no factory was provided. Used by <see cref="ToolsUsed"/> to
        /// render the detail pane independently of this inline view.
        /// </summary>
        public IComponent CreateContent()
        {
            return _contentFactory?.Invoke();
        }

        /// <summary>
        /// Sets the content of the component.
        /// </summary>
        public ToolCall SetContent(Func<IComponent> contentFactory)
        {
            _contentFactory  = contentFactory;
            _renderedContent = null;
            ClearChildren(_content);
            UpdateExpandableUI();
            if (_isExpanded)
            {
                EnsureContentRendered();
            }
            return this;
        }

        /// <summary>
        /// Sets the content of the component.
        /// </summary>
        public ToolCall SetContent(IComponent content)
        {
            return SetContent(content != null ? (Func<IComponent>)(() => content) : null);
        }

        /// <summary>
        /// Sets the text of the component.
        /// </summary>
        public ToolCall SetText(string text)
        {
            _text = text ?? string.Empty;
            _textContainer.innerText = _text;
            return this;
        }

        /// <summary>
        /// Sets the icon of the component.
        /// </summary>
        public ToolCall SetIcon(UIcons icon)
        {
            _icon = icon;
            ClearChildren(_iconContainer);
            _iconContainer.appendChild(I(icon));
            return this;
        }

        /// <summary>
        /// Gets the live progress line shown on the header while the call runs, creating it on first
        /// use. The line lives inside the header row, so expanding the call leaves it untouched.
        /// </summary>
        public LiveProgress Progress => EnsureProgress();

        /// <summary>
        /// Writes the given progress onto the header of this call, next to its name. Meant to be
        /// called as often as the progress arrives - only the text of the line changes.
        /// </summary>
        public ToolCall SetProgress(string progress)
        {
            EnsureProgress().SetText(progress);
            return this;
        }

        /// <summary>
        /// Streams the progress shown on the header from the given observable.
        /// </summary>
        public ToolCall SetProgress(IObservable<string> progress)
        {
            EnsureProgress().Stream(progress);
            return this;
        }

        /// <summary>
        /// Clears the progress shown on the header, leaving the call as a plain chip again.
        /// </summary>
        public ToolCall ClearProgress()
        {
            _progress?.StopStreaming().Clear();
            return this;
        }

        private LiveProgress EnsureProgress()
        {
            if (_progress is null)
            {
                _progress = new LiveProgress().Class("tss-toolcall-progress");
                _header.insertBefore(_progress.Render(), _chevron);
            }

            return _progress;
        }

        /// <summary>
        /// Configures the not expandable on the component.
        /// </summary>
        public ToolCall NotExpandable()
        {
            _expandable = false;
            UpdateExpandableUI();
            return this;
        }

        /// <summary>
        /// Expands the component.
        /// </summary>
        public ToolCall Expanded(bool value = true)
        {
            if (value) Expand();
            else Collapse();
            return this;
        }

        /// <summary>
        /// Expands the component.
        /// </summary>
        public ToolCall Expand()
        {
            if (_isExpanded || !CanExpand) return this;
            _isExpanded = true;
            EnsureContentRendered();
            UpdateExpandedState();
            _onToggle?.Invoke(this);
            return this;
        }

        /// <summary>
        /// Collapses the component.
        /// </summary>
        public ToolCall Collapse()
        {
            if (!_isExpanded) return this;
            _isExpanded = false;
            UpdateExpandedState();
            _onToggle?.Invoke(this);
            return this;
        }

        /// <summary>
        /// Toggles the component's state.
        /// </summary>
        public ToolCall Toggle()
        {
            return _isExpanded ? Collapse() : Expand();
        }

        /// <summary>
        /// Registers a callback invoked when the toggle event fires.
        /// </summary>
        public ToolCall OnToggle(Action<ToolCall> onToggle)
        {
            _onToggle += onToggle;
            return this;
        }

        private void EnsureContentRendered()
        {
            if (_renderedContent != null) return;
            if (_contentFactory == null) return;

            _renderedContent = _contentFactory();
            if (_renderedContent != null)
            {
                _content.appendChild(_renderedContent.Render());
            }
        }

        private void UpdateExpandedState()
        {
            InnerElement.UpdateClassIf(_isExpanded, "tss-expanded");
            _content.style.display = _isExpanded ? "block" : "none";
            _header.setAttribute("aria-expanded", _isExpanded ? "true" : "false");
        }

        private void UpdateExpandableUI()
        {
            var canExpand = CanExpand;
            _chevron.style.display = canExpand ? "" : "none";
            _header.UpdateClassIf(!canExpand, "tss-toolcall-not-expandable");
            if (!canExpand && _isExpanded) Collapse();
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }

    /// <summary>
    /// Compact summary of multiple tool calls that opens a popup showing the
    /// list of tools on the left. Clicking a tool slides the list to the left
    /// and shows that tool's content on the right with a back button to
    /// return to the list.
    /// <para>
    /// <see cref="Inline()"/> swaps the popup for an in-place disclosure: the summary expands
    /// underneath itself into the list of <see cref="ToolCall"/>s, each one opening its own content
    /// inline the way a standalone call does.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.ToolsUsed")]
    public sealed class ToolsUsed : ComponentBase<ToolsUsed, HTMLElement>
    {
        private readonly HTMLElement      _header;
        private readonly HTMLElement      _summaryIcon;
        private readonly HTMLElement      _summaryText;
        private readonly HTMLElement      _summaryChevron;
        private readonly HTMLElement      _inlineList;
        private readonly List<ToolCall>   _tools;
        private          LiveProgress     _progress;
        private          string           _summaryLabel;
        private          UIcons           _summaryIconKind = UIcons.Tools;
        private          string           _modalTitle      = "Tools used";
        private          bool             _inline;
        private          bool             _isExpanded;
        private          Modal            _modal;
        private          HTMLElement      _slider;
        private          HTMLElement      _listPanel;
        private          HTMLElement      _detailPanel;
        private          HTMLElement      _detailContent;
        private          HTMLElement      _detailTitle;
        private          HTMLElement      _detailIconHolder;
        private          HTMLElement      _backButton;
        private          HTMLElement      _titleEl;

        private event Action<ToolsUsed> Toggled;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ToolsUsed(IEnumerable<ToolCall> tools = null)
        {
            _tools = new List<ToolCall>();

            _summaryIcon    = Div(Att("tss-toolsused-icon"), I(_summaryIconKind));
            _summaryText    = Div(Att("tss-toolsused-text"));
            _summaryChevron = I(UIcons.AngleRight, cssClass: "tss-toolsused-chevron");

            _header = Div(Att("tss-toolsused-header", role: "button", ariaLabel: "Show tools used"),
                          _summaryIcon, _summaryText, _summaryChevron);

            // Only ever filled in inline mode, but the element is always there so the list has a place to
            // land whenever Inline() is called - before or after the summary is on screen.
            _inlineList = Div(Att("tss-toolsused-inline-list"));
            _inlineList.style.display = "none";

            InnerElement = Div(Att("tss-toolsused"), _header, _inlineList);

            // Open on a tap gesture rather than a raw "click": in a live-streaming chat the surrounding
            // content re-renders and auto-scrolls under the pointer, which moves this element between
            // mousedown and mouseup so the browser never fires a "click" and the summary looked dead.
            // OnTapped captures the pointer and keys off the (stationary) pointer position, so a press-
            // release still opens the popup while the page scrolls, and a scroll-drag off the pill does not.
            // The gesture is on the header rather than the root, so tapping an expanded inline list does
            // not fold it back up.
            Raw(_header).OnTapped(() => Toggle());

            _header.tabIndex = 0;

            _header.addEventListener("keydown", ev =>
            {
                var ke = ev.As<KeyboardEvent>();

                if (ke.key == "Enter" || ke.key == " ")
                {
                    StopEvent(ev);
                    Toggle();
                }
            });

            if (tools != null)
            {
                foreach (var t in tools)
                {
                    Add(t);
                }
            }

            UpdateSummary();
        }

        /// <summary>
        /// Adds the given item to the component.
        /// </summary>
        public ToolsUsed Add(ToolCall tool)
        {
            if (tool == null) return this;
            _tools.Add(tool);
            UpdateSummary();
            // A call arriving while the group is open inline joins the list on screen, the way a live
            // transcript appends to it as the calls come in.
            if (_inline && _isExpanded) RebuildInlineList();
            return this;
        }

        /// <summary>
        /// Adds the given range to the component.
        /// </summary>
        public ToolsUsed AddRange(IEnumerable<ToolCall> tools)
        {
            if (tools == null) return this;
            foreach (var t in tools) Add(t);
            return this;
        }

        /// <summary>
        /// Adds the given item to the component.
        /// </summary>
        public ToolsUsed Add(UIcons icon, string text, Func<IComponent> contentFactory)
        {
            return Add(new ToolCall(icon, text, contentFactory));
        }

        /// <summary>
        /// Adds the given item to the component.
        /// </summary>
        public ToolsUsed Add(UIcons icon, string text, IComponent content)
        {
            return Add(new ToolCall(icon, text, content));
        }

        /// <summary>
        /// Clears the component's current state.
        /// </summary>
        public ToolsUsed Clear()
        {
            _tools.Clear();
            UpdateSummary();
            ClearChildren(_inlineList);
            return this;
        }

        /// <summary>
        /// Sets the summary of the component.
        /// </summary>
        public ToolsUsed SetSummary(string label)
        {
            _summaryLabel = label;
            UpdateSummary();
            return this;
        }

        /// <summary>
        /// Sets the icon of the component.
        /// </summary>
        public ToolsUsed SetIcon(UIcons icon)
        {
            _summaryIconKind = icon;
            ClearChildren(_summaryIcon);
            _summaryIcon.appendChild(I(icon));
            return this;
        }

        /// <summary>
        /// Sets the title of the component.
        /// </summary>
        public ToolsUsed SetTitle(string title)
        {
            _modalTitle = title ?? string.Empty;
            if (_titleEl != null && _backButton != null && _backButton.style.visibility == "hidden")
            {
                _titleEl.innerText = _modalTitle;
            }
            return this;
        }

        /// <summary>
        /// Renders the tools in place instead of in a popup: the summary becomes an accordion that
        /// expands into the list of <see cref="ToolCall"/>s underneath itself, each one opening its own
        /// content inline the way a standalone call does. For a transcript where sending the reader to a
        /// modal for a one-line result is too much ceremony.
        /// </summary>
        public ToolsUsed Inline(bool value = true)
        {
            _inline = value;
            InnerElement.UpdateClassIf(value, "tss-toolsused-inline");

            // Sideways for "this opens somewhere else", down-and-rotating for a disclosure that happens
            // right here.
            _summaryChevron.className = $"{Tesserae.Icon.Transform(value ? UIcons.AngleDown : UIcons.AngleRight, UIconsWeight.Regular)} tss-toolsused-chevron";
            _header.setAttribute("aria-label", value ? "Toggle tools used" : "Show tools used");

            if (!value)
            {
                _isExpanded = false;
                ClearChildren(_inlineList);
            }

            UpdateExpandedState();

            return this;
        }

        /// <summary>
        /// Returns a value indicating whether the group renders its tools in place rather than in a popup.
        /// </summary>
        public bool IsInline   => _inline;

        /// <summary>
        /// Returns a value indicating whether the inline list is open. Always false while the group opens
        /// its tools in the popup instead.
        /// </summary>
        public bool IsExpanded => _isExpanded;

        /// <summary>
        /// Expands or collapses the group.
        /// </summary>
        public ToolsUsed Expanded(bool value = true) => value ? Expand() : Collapse();

        /// <summary>
        /// Opens the group: the inline list when <see cref="Inline()"/> is set, the popup otherwise.
        /// </summary>
        public ToolsUsed Expand()
        {
            if (!_inline) return ShowModal();
            if (_isExpanded) return this;

            _isExpanded = true;
            RebuildInlineList();
            UpdateExpandedState();
            Toggled?.Invoke(this);

            return this;
        }

        /// <summary>
        /// Closes the group: the inline list when <see cref="Inline()"/> is set, the popup otherwise.
        /// </summary>
        public ToolsUsed Collapse()
        {
            if (!_inline) return HideModal();
            if (!_isExpanded) return this;

            _isExpanded = false;
            UpdateExpandedState();
            Toggled?.Invoke(this);

            return this;
        }

        /// <summary>
        /// Toggles the group between open and closed.
        /// </summary>
        public ToolsUsed Toggle() => _isExpanded ? Collapse() : Expand();

        /// <summary>
        /// Registers a callback invoked whenever the inline list is expanded or collapsed.
        /// </summary>
        public ToolsUsed OnToggle(Action<ToolsUsed> onToggle)
        {
            Toggled += onToggle;
            return this;
        }

        /// <summary>
        /// Gets the live progress line shown on the summary while the group's calls run, creating it
        /// on first use.
        /// </summary>
        public LiveProgress Progress => EnsureProgress();

        /// <summary>
        /// Writes the given progress onto the summary, next to the count of tools used. Meant to be
        /// called as often as the progress arrives - only the text of the line changes.
        /// </summary>
        public ToolsUsed SetProgress(string progress)
        {
            EnsureProgress().SetText(progress);
            return this;
        }

        /// <summary>
        /// Streams the progress shown on the summary from the given observable.
        /// </summary>
        public ToolsUsed SetProgress(IObservable<string> progress)
        {
            EnsureProgress().Stream(progress);
            return this;
        }

        /// <summary>
        /// Clears the progress shown on the summary.
        /// </summary>
        public ToolsUsed ClearProgress()
        {
            _progress?.StopStreaming().Clear();
            return this;
        }

        private LiveProgress EnsureProgress()
        {
            if (_progress is null)
            {
                _progress = new LiveProgress().Class("tss-toolsused-progress");
                _header.insertBefore(_progress.Render(), _summaryChevron);
            }

            return _progress;
        }

        /// <summary>
        /// Shows the component: the popup, or the inline list when <see cref="Inline()"/> is set.
        /// </summary>
        public ToolsUsed Show()
        {
            return _inline ? Expand() : ShowModal();
        }

        /// <summary>
        /// Hides the component: the popup, or the inline list when <see cref="Inline()"/> is set.
        /// </summary>
        public ToolsUsed Hide()
        {
            return _inline ? Collapse() : HideModal();
        }

        private ToolsUsed ShowModal()
        {
            BuildModalIfNeeded();
            RebuildList();
            ShowList(animate: false);
            _modal.Show();
            return this;
        }

        private ToolsUsed HideModal()
        {
            _modal?.Hide();
            return this;
        }

        // The inline list holds the calls themselves, so each one carries its own accordion and builds its
        // content lazily on first open - the same element the caller holds a reference to.
        private void RebuildInlineList()
        {
            ClearChildren(_inlineList);

            foreach (var tool in _tools)
            {
                _inlineList.appendChild(tool.Render());
            }
        }

        private void UpdateExpandedState()
        {
            InnerElement.UpdateClassIf(_isExpanded, "tss-expanded");
            _header.setAttribute("aria-expanded", _isExpanded ? "true" : "false");
            _inlineList.style.display = _isExpanded ? "" : "none";
        }

        private void UpdateSummary()
        {
            if (!string.IsNullOrEmpty(_summaryLabel))
            {
                _summaryText.innerText = _summaryLabel;
                return;
            }
            _summaryText.innerText = _tools.Count == 1
                ? "Used 1 tool"
                : $"Used {_tools.Count} tools";
        }

        private void BuildModalIfNeeded()
        {
            if (_modal != null) return;

            _titleEl          = Div(Att("tss-toolsused-modal-title"));
            _detailTitle      = Div(Att("tss-toolsused-modal-detail-title"));
            _detailIconHolder = Div(Att("tss-toolsused-modal-detail-icon"));
            _detailIconHolder.style.display = "none";

            _backButton = UI.Button(Att("tss-toolsused-back", type: "button", ariaLabel: "Back to list"), I(UIcons.AngleLeft));
            _backButton.addEventListener("click", _ => ShowList(animate: true));
            _backButton.style.visibility = "hidden";

            var header = Div(Att("tss-toolsused-modal-header"),
                             _backButton,
                             _detailIconHolder,
                             _titleEl,
                             _detailTitle);

            _listPanel     = Div(Att("tss-toolsused-modal-panel tss-toolsused-modal-list"));
            _detailContent = Div(Att("tss-toolsused-modal-detail-content"));
            _detailPanel   = Div(Att("tss-toolsused-modal-panel tss-toolsused-modal-detail"), _detailContent);

            _slider = Div(Att("tss-toolsused-modal-slider"), _listPanel, _detailPanel);

            _modal = Modal(Raw(header));
            _modal.Content = Raw(Div(Att("tss-toolsused-modal-body"), _slider));
            _modal.NoFooter();
            _modal.CanLightDismiss = true;
            _modal.InnerElement.classList.add("tss-toolsused-modal");
        }

        private void RebuildList()
        {
            ClearChildren(_listPanel);
            for (int i = 0; i < _tools.Count; i++)
            {
                var tool = _tools[i];

                var iconEl  = Div(Att("tss-toolsused-list-icon"), I(tool.Icon));
                var labelEl = Div(Att("tss-toolsused-list-text", text: tool.Text));
                var chevron = I(UIcons.AngleRight, cssClass: "tss-toolsused-list-chevron");
                var row     = Div(Att("tss-toolsused-list-row", role: "button"), iconEl, labelEl, chevron);

                var capturedTool = tool;
                row.addEventListener("click", _ => ShowDetail(capturedTool));

                _listPanel.appendChild(row);
            }
        }

        private void ShowList(bool animate)
        {
            ClearChildren(_detailContent);
            _slider.classList.remove("tss-toolsused-show-detail");
            _backButton.style.visibility       = "hidden";
            _detailIconHolder.style.display    = "none";
            ClearChildren(_detailIconHolder);
            _detailTitle.innerText             = string.Empty;
            _titleEl.innerText                 = _modalTitle;

            if (!animate)
            {
                _slider.classList.add("tss-toolsused-no-anim");
                window.setTimeout(__ => _slider.classList.remove("tss-toolsused-no-anim"), 30);
            }
        }

        private void ShowDetail(ToolCall tool)
        {
            ClearChildren(_detailContent);

            var content = tool.CreateContent();
            if (content != null)
            {
                _detailContent.appendChild(content.Render());
            }
            else
            {
                _detailContent.appendChild(Div(Att("tss-toolsused-empty", text: "No content")));
            }

            _titleEl.innerText      = string.Empty;
            _detailTitle.innerText  = tool.Text;
            ClearChildren(_detailIconHolder);
            _detailIconHolder.style.display = "flex";
            _detailIconHolder.appendChild(I(tool.Icon));

            _backButton.style.visibility = "visible";
            _slider.classList.add("tss-toolsused-show-detail");
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
