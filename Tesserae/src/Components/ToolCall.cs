using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Inline tool-call indicator that expands accordion-style to show the
    /// associated content. The content component is created lazily the first
    /// time the user expands the tool call.
    /// </summary>
    [H5.Name("tss.ToolCall")]
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
        private          bool             _isExpanded;
        private          bool             _expandable = true;
        private          Action<ToolCall> _onToggle;

        public ToolCall(UIcons icon, string text, Func<IComponent> contentFactory = null)
        {
            _icon           = icon;
            _text           = text ?? string.Empty;
            _contentFactory = contentFactory;

            _iconContainer = Div(_("tss-toolcall-icon"), I(icon));
            _textContainer = Div(_("tss-toolcall-text", text: _text));
            _chevron       = I(UIcons.AngleDown, cssClass: "tss-toolcall-chevron");

            _header = Div(_("tss-toolcall-header", role: "button", ariaLabel: "Toggle tool call"),
                          _iconContainer, _textContainer, _chevron);

            _content = Div(_("tss-toolcall-content"));
            _content.style.display = "none";

            InnerElement = Div(_("tss-toolcall"), _header, _content);

            _header.addEventListener("click", _ =>
            {
                if (_expandable) Toggle();
            });
        }

        public ToolCall(UIcons icon, string text, IComponent content)
            : this(icon, text, content != null ? (Func<IComponent>)(() => content) : null)
        {
        }

        public UIcons  Icon            => _icon;
        /// <summary>
        /// Gets or sets the text shown in the component.
        /// </summary>
        public string  Text            => _text;
        public bool    IsExpanded      => _isExpanded;
        public bool    HasContent      => _contentFactory != null;

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

        public ToolCall NotExpandable()
        {
            _expandable = false;
            _chevron.style.display = "none";
            _header.classList.add("tss-toolcall-not-expandable");
            return this;
        }

        public ToolCall Expanded(bool value = true)
        {
            if (value) Expand();
            else Collapse();
            return this;
        }

        public ToolCall Expand()
        {
            if (_isExpanded || !_expandable) return this;
            _isExpanded = true;
            EnsureContentRendered();
            UpdateExpandedState();
            _onToggle?.Invoke(this);
            return this;
        }

        public ToolCall Collapse()
        {
            if (!_isExpanded) return this;
            _isExpanded = false;
            UpdateExpandedState();
            _onToggle?.Invoke(this);
            return this;
        }

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
    /// </summary>
    [H5.Name("tss.ToolsUsed")]
    public sealed class ToolsUsed : ComponentBase<ToolsUsed, HTMLElement>
    {
        private readonly HTMLElement      _summaryIcon;
        private readonly HTMLElement      _summaryText;
        private readonly List<ToolCall>   _tools;
        private          string           _summaryLabel;
        private          UIcons           _summaryIconKind = UIcons.Tools;
        private          string           _modalTitle      = "Tools used";
        private          Modal            _modal;
        private          HTMLElement      _slider;
        private          HTMLElement      _listPanel;
        private          HTMLElement      _detailPanel;
        private          HTMLElement      _detailContent;
        private          HTMLElement      _detailTitle;
        private          HTMLElement      _detailIconHolder;
        private          HTMLElement      _backButton;
        private          HTMLElement      _titleEl;

        public ToolsUsed(IEnumerable<ToolCall> tools = null)
        {
            _tools = new List<ToolCall>();

            _summaryIcon = Div(_("tss-toolsused-icon"), I(_summaryIconKind));
            _summaryText = Div(_("tss-toolsused-text"));
            var chevron  = I(UIcons.AngleRight, cssClass: "tss-toolsused-chevron");

            InnerElement = Div(_("tss-toolsused", role: "button", ariaLabel: "Show tools used"),
                               _summaryIcon, _summaryText, chevron);

            InnerElement.addEventListener("click", _ => Show());

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

        public ToolsUsed Show()
        {
            BuildModalIfNeeded();
            RebuildList();
            ShowList(animate: false);
            _modal.Show();
            return this;
        }

        public ToolsUsed Hide()
        {
            _modal?.Hide();
            return this;
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

            _titleEl          = Div(_("tss-toolsused-modal-title"));
            _detailTitle      = Div(_("tss-toolsused-modal-detail-title"));
            _detailIconHolder = Div(_("tss-toolsused-modal-detail-icon"));
            _detailIconHolder.style.display = "none";

            _backButton = Button(_("tss-toolsused-back", type: "button", ariaLabel: "Back to list"), I(UIcons.AngleLeft));
            _backButton.addEventListener("click", _ => ShowList(animate: true));
            _backButton.style.visibility = "hidden";

            var header = Div(_("tss-toolsused-modal-header"),
                             _backButton,
                             _detailIconHolder,
                             _titleEl,
                             _detailTitle);

            _listPanel     = Div(_("tss-toolsused-modal-panel tss-toolsused-modal-list"));
            _detailContent = Div(_("tss-toolsused-modal-detail-content"));
            _detailPanel   = Div(_("tss-toolsused-modal-panel tss-toolsused-modal-detail"), _detailContent);

            _slider = Div(_("tss-toolsused-modal-slider"), _listPanel, _detailPanel);

            _modal = Modal(Raw(header));
            _modal.Content = Raw(Div(_("tss-toolsused-modal-body"), _slider));
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

                var iconEl  = Div(_("tss-toolsused-list-icon"), I(tool.Icon));
                var labelEl = Div(_("tss-toolsused-list-text", text: tool.Text));
                var chevron = I(UIcons.AngleRight, cssClass: "tss-toolsused-list-chevron");
                var row     = Div(_("tss-toolsused-list-row", role: "button"), iconEl, labelEl, chevron);

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
                _detailContent.appendChild(Div(_("tss-toolsused-empty", text: "No content")));
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
