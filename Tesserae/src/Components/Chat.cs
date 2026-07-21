using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A chat transcript surface that lays out a sequence of messages with sender attribution, avatars and
    /// timestamps. It keeps streamed replies in view while the reader is at the live edge, stops following
    /// the moment the reader scrolls away, and offers a scroll-to-latest button to re-engage.
    /// </summary>
    [Transpose.Name("tss.ChatArea")]
    public class ChatArea : IComponent
    {
        /// <summary>
        /// Where the transcript settles when it is first populated (or re-populated after <see cref="Clear"/>).
        /// </summary>
        public enum StartPosition
        {
            /// <summary>Start at the top of the transcript.</summary>
            Start,
            /// <summary>Start at the bottom (the most recent message). This is the default.</summary>
            End,
            /// <summary>Start at the last anchored turn (e.g. the reader's most recent message), with a peek of the prior content above it.</summary>
            LastAnchor
        }

        private const int BOTTOM_THRESHOLD = 5;

        private readonly HTMLElement _outerElement;
        private readonly HTMLElement _scrollContainer;
        private readonly HTMLElement _contentElement;
        private readonly HTMLElement _scrollButton;
        private readonly ObservableList<IComponentWithID> _messages;
        private readonly KeyedObservableStack _stack;

        private bool _followOutput = true;
        private bool _hasPositioned = false;
        private double _positionTimer = 0;
        private string _bubbleBackground = null;

        private StartPosition _defaultScrollPosition = StartPosition.End;
        private int _previousItemPeek = 64;

        private ChatMessage _pendingAnchor = null;
        private ChatMessage _reservedMessage = null;

        /// <summary>
        /// Raised when scrolled occurs.
        /// </summary>
        public event ComponentEventHandler<ChatArea, Event> Scrolled;
        /// <summary>
        /// Raised when received focus occurs.
        /// </summary>
        public event ComponentEventHandler<ChatArea, Event> ReceivedFocus;
        /// <summary>
        /// Raised when lost focus occurs.
        /// </summary>
        public event ComponentEventHandler<ChatArea, Event> LostFocus;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ChatArea()
        {
            _messages = new ObservableList<IComponentWithID>();

            _stack = new KeyedObservableStack(_messages, Stack.Orientation.Vertical, debounce: false).S();

            _contentElement = _stack.Render();
            _contentElement.setAttribute("role", "log");
            _contentElement.setAttribute("aria-relevant", "additions");

            _scrollContainer = Div(Att("tss-chatarea"), _contentElement);
            _scrollContainer.setAttribute("role", "region");
            _scrollContainer.setAttribute("aria-label", "Messages");
            _scrollContainer.tabIndex = 0;

            _scrollButton = BuildScrollButton();

            _outerElement = Div(Att("tss-chatarea-wrapper"), _scrollContainer, _scrollButton);

            _scrollContainer.addEventListener("scroll", (e) =>
            {
                // Follow the live edge only while the reader is at (or within a few px of) the bottom.
                var distanceFromBottom = _scrollContainer.scrollHeight - _scrollContainer.scrollTop - _scrollContainer.clientHeight;
                _followOutput = distanceFromBottom <= BOTTOM_THRESHOLD;

                UpdateScrollButton();
                Scrolled?.Invoke(this, e);
            });

            _scrollContainer.addEventListener("focusin", (e) => ReceivedFocus?.Invoke(this, e));
            _scrollContainer.addEventListener("focusout", (e) => LostFocus?.Invoke(this, e));
        }

        private HTMLElement BuildScrollButton()
        {
            var button = Button(UIcons.ArrowDown).OnClick(() => ScrollToEnd(smooth: true));
            var el = button.Render();

            el.classList.add("tss-chatarea-scrollbtn");
            el.setAttribute("aria-label", "Scroll to latest");
            el.setAttribute("tabindex", "-1");
            el.setAttribute("inert", "");

            return el;
        }

        /// <summary>
        /// Registers a callback invoked when the scroll event fires.
        /// </summary>
        public ChatArea OnScroll(ComponentEventHandler<ChatArea, Event> onScroll)
        {
            Scrolled += onScroll;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the focus event fires.
        /// </summary>
        public ChatArea OnFocus(ComponentEventHandler<ChatArea, Event> onFocus)
        {
            ReceivedFocus += onFocus;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the blur event fires.
        /// </summary>
        public ChatArea OnBlur(ComponentEventHandler<ChatArea, Event> onBlur)
        {
            LostFocus += onBlur;
            return this;
        }

        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public ChatArea Background(string color)
        {
            _bubbleBackground = color;
            foreach (var msg in _messages.Value)
            {
                if (msg is ChatMessage chatMsg)
                {
                    chatMsg.Background(color);
                }
            }
            return this;
        }

        /// <summary>
        /// Sets where the transcript settles when it is first populated (or re-populated after <see cref="Clear"/>).
        /// </summary>
        public ChatArea DefaultScrollPosition(StartPosition position)
        {
            _defaultScrollPosition = position;
            return this;
        }

        /// <summary>
        /// Sets how many pixels of the previous turn stay visible above an anchored message when a new turn begins.
        /// </summary>
        public ChatArea PreviousItemPeek(int pixels)
        {
            _previousItemPeek = pixels < 0 ? 0 : pixels;
            return this;
        }

        /// <summary>
        /// Toggles the <c>aria-busy</c> state on the transcript; set it while a turn is streaming so screen readers
        /// can defer announcements until the reply completes.
        /// </summary>
        public ChatArea Busy(bool isBusy)
        {
            if (isBusy)
            {
                _contentElement.setAttribute("aria-busy", "true");
            }
            else
            {
                _contentElement.removeAttribute("aria-busy");
            }
            return this;
        }

        /// <summary>
        /// Adds the given item to the component.
        /// </summary>
        public ChatArea Add(ChatMessage message)
        {
            if (_bubbleBackground != null && message.BubbleBackground == null)
            {
                message.Background(_bubbleBackground);
            }
            message.SetParent(this);

            if (message.IsAnchor)
            {
                // A new turn boundary: collapse the previous turn's reserved space and re-engage following.
                ReleaseReservation();
                _pendingAnchor = message;
                _followOutput = true;
            }

            _messages.Add(message);

            // Delay slightly to let the DOM update before measuring / scrolling.
            _positionTimer = window.setTimeout((_) => OnMessageAdded(message), 10);

            return this;
        }

        /// <summary>
        /// Inserts a batch of older messages at the top of the transcript while preserving the reader's current
        /// scroll position (so loading history above the fold does not move what they are reading).
        /// </summary>
        public ChatArea PrependRange(IEnumerable<ChatMessage> older)
        {
            var beforeHeight = _scrollContainer.scrollHeight;
            var beforeTop = _scrollContainer.scrollTop;

            int index = 0;
            foreach (var message in older)
            {
                if (_bubbleBackground != null && message.BubbleBackground == null)
                {
                    message.Background(_bubbleBackground);
                }
                message.SetParent(this);
                _messages.Insert(index, message);
                index++;
            }

            window.setTimeout((_) =>
            {
                var afterHeight = _scrollContainer.scrollHeight;
                _scrollContainer.scrollTop = beforeTop + (afterHeight - beforeHeight);
                UpdateScrollButton();
            }, 10);

            return this;
        }

        /// <summary>
        /// Runs a repopulation (typically <see cref="Clear"/> followed by a batch of <see cref="Add"/> calls) while
        /// preserving the reader's scroll position: if they were at the live edge it stays pinned to the bottom,
        /// otherwise their current position is restored so re-rendering the transcript does not yank them around.
        /// </summary>
        public ChatArea RebuildPreservingScroll(Action populate)
        {
            var wasAtBottom = IsAtBottom;
            var previousTop = _scrollContainer.scrollTop;

            populate();

            // The batch's own positioning must not fight the restore below.
            _hasPositioned = true;
            _followOutput  = wasAtBottom;

            window.setTimeout((_) =>
            {
                _scrollContainer.scrollTop = wasAtBottom ? _scrollContainer.scrollHeight : previousTop;
                UpdateScrollButton();
            }, 20);

            return this;
        }

        /// <summary>
        /// Clears the component's current state.
        /// </summary>
        public ChatArea Clear()
        {
            _messages.Clear();
            _pendingAnchor = null;
            _reservedMessage = null;
            _hasPositioned = false;
            _followOutput = true;
            return this;
        }

        /// <summary>
        /// Gets whether the transcript is currently scrolled to (or within a few pixels of) the bottom.
        /// </summary>
        public bool IsAtBottom
        {
            get
            {
                var distanceFromBottom = _scrollContainer.scrollHeight - _scrollContainer.scrollTop - _scrollContainer.clientHeight;
                return distanceFromBottom <= BOTTOM_THRESHOLD;
            }
        }

        /// <summary>
        /// Gets whether the transcript is currently following streamed output (auto-scrolling to the live edge).
        /// </summary>
        public bool IsFollowingOutput => _followOutput;

        /// <summary>
        /// Scrolls to the bottom of the transcript and re-engages following of streamed output.
        /// </summary>
        public ChatArea ScrollToEnd(bool smooth = false)
        {
            _followOutput = true;

            if (smooth && _messages.Value.Count > 0)
            {
                var last = _messages.Value[_messages.Value.Count - 1].Render();
                if (last != null)
                {
                    last.scrollIntoView(new ScrollIntoViewOptions() { behavior = ScrollBehavior.smooth, block = ScrollLogicalPosition.end });
                }
            }
            else
            {
                _scrollContainer.scrollTop = _scrollContainer.scrollHeight;
            }

            UpdateScrollButton();
            return this;
        }

        /// <summary>
        /// Scrolls to the top of the transcript.
        /// </summary>
        public ChatArea ScrollToStart(bool smooth = false)
        {
            _followOutput = false;

            if (smooth && _messages.Value.Count > 0)
            {
                var first = _messages.Value[0].Render();
                if (first != null)
                {
                    first.scrollIntoView(new ScrollIntoViewOptions() { behavior = ScrollBehavior.smooth, block = ScrollLogicalPosition.start });
                }
            }
            else
            {
                _scrollContainer.scrollTop = 0;
            }

            UpdateScrollButton();
            return this;
        }

        /// <summary>
        /// Scrolls the message with the given identifier into view. Returns false if no such message is mounted.
        /// </summary>
        public bool ScrollToMessage(string identifier, bool smooth = true)
        {
            foreach (var msg in _messages.Value)
            {
                if (msg.Identifier == identifier)
                {
                    var el = msg.Render();
                    if (el == null) return false;

                    _followOutput = false;
                    var options = new ScrollIntoViewOptions() { block = ScrollLogicalPosition.start };
                    if (smooth) { options.behavior = ScrollBehavior.smooth; } else { options.behavior = ScrollBehavior.instant; }
                    el.scrollIntoView(options);
                    UpdateScrollButton();
                    return true;
                }
            }
            return false;
        }

        internal void EnsureVisible(ChatMessage message)
        {
            if (!_followOutput) return;

            _scrollContainer.scrollTop = _scrollContainer.scrollHeight;
            UpdateScrollButton();
        }

        private void OnMessageAdded(ChatMessage message)
        {
            if (!message.IsAnchor && _pendingAnchor != null)
            {
                ReserveFor(_pendingAnchor, message);
                _pendingAnchor = null;
            }

            if (!_hasPositioned)
            {
                ApplyStartPosition();
                ArmPositionLock();
            }
            else if (_followOutput)
            {
                _scrollContainer.scrollTop = _scrollContainer.scrollHeight;
            }

            UpdateScrollButton();
        }

        // Reserve enough vertical space on the answer bubble that scrolling to the bottom lands the anchor (the
        // turn's opening message) near the top with a small peek above it, letting the reply grow into the screen.
        private void ReserveFor(ChatMessage anchor, ChatMessage answer)
        {
            var viewport = _scrollContainer.clientHeight;
            if (viewport <= 0) return;

            var reserve = viewport - anchor.MeasuredHeight - _previousItemPeek;
            if (reserve < 0) reserve = 0;

            answer.ReserveMinHeight(reserve);
            _reservedMessage = answer;
        }

        private void ReleaseReservation()
        {
            if (_reservedMessage != null)
            {
                _reservedMessage.ReserveMinHeight(0);
                _reservedMessage = null;
            }
        }

        private void ApplyStartPosition()
        {
            switch (_defaultScrollPosition)
            {
                case StartPosition.Start:
                    _scrollContainer.scrollTop = 0;
                    break;
                case StartPosition.LastAnchor:
                    var anchor = FindLastAnchor();
                    if (anchor != null)
                    {
                        var target = OffsetWithin(anchor.Render()) - _previousItemPeek;
                        _scrollContainer.scrollTop = target < 0 ? 0 : target;
                    }
                    else
                    {
                        _scrollContainer.scrollTop = _scrollContainer.scrollHeight;
                    }
                    break;
                default:
                    _scrollContainer.scrollTop = _scrollContainer.scrollHeight;
                    break;
            }
        }

        private ChatMessage FindLastAnchor()
        {
            for (int i = _messages.Value.Count - 1; i >= 0; i--)
            {
                if (_messages.Value[i] is ChatMessage chatMsg && chatMsg.IsAnchor)
                {
                    return chatMsg;
                }
            }
            return null;
        }

        private double OffsetWithin(HTMLElement el)
        {
            return ((DOMRect)el.getBoundingClientRect()).top - ((DOMRect)_scrollContainer.getBoundingClientRect()).top + _scrollContainer.scrollTop;
        }

        // Keep re-applying the start position for the whole initial batch, then lock it in shortly after the last add.
        private void ArmPositionLock()
        {
            if (_positionTimer != 0)
            {
                window.clearTimeout(_positionTimer);
            }
            _positionTimer = window.setTimeout((_) =>
            {
                _hasPositioned = true;
                _positionTimer = 0;
            }, 150);
        }

        private void UpdateScrollButton()
        {
            if (IsAtBottom)
            {
                _scrollButton.classList.remove("tss-visible");
                _scrollButton.setAttribute("inert", "");
            }
            else
            {
                _scrollButton.classList.add("tss-visible");
                _scrollButton.removeAttribute("inert");
            }
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render()
        {
            return _outerElement;
        }
    }

    [Transpose.Name("tss.ChatMessage")]
    public class ChatMessage : IComponentWithID, IComponent
    {
        private HTMLElement _innerElement;
        private HTMLElement _bubbleContainer;
        private HTMLElement _contentContainer;
        private HTMLElement _referencesContainer;

        private DeltaComponent _deltaComponent;
        private IComponent _currentContent;
        private ChatArea _parent;

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public string Identifier { get; private set; }
        /// <summary>
        /// Gets or sets the content hash.
        /// </summary>
        public string ContentHash { get; private set; }

        /// <summary>
        /// Gets or sets the bubble background.
        /// </summary>
        public string BubbleBackground { get; private set; }

        /// <summary>
        /// Gets whether this message is a turn boundary that the transcript should settle near the top when a new turn begins.
        /// </summary>
        public bool IsAnchor { get; private set; }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ChatMessage(IComponent content, IComponent avatar = null, IComponent commands = null)
        {
            Identifier = Guid.NewGuid().ToString();
            ContentHash = Guid.NewGuid().ToString();

            _currentContent = content;
            _deltaComponent = DeltaComponent(content);

            _contentContainer = Div(Att("tss-chatmessage-content"), _deltaComponent.Render());
            _referencesContainer = Div(Att("tss-chatmessage-references"));
            _referencesContainer.style.display = "none";

            _bubbleContainer = Div(Att("tss-chatmessage-bubble"), _contentContainer, _referencesContainer);

            _innerElement = Div(Att("tss-chatmessage-container tss-chat-left"));

            if (avatar != null)
            {
                _innerElement.appendChild(Div(Att("tss-chatmessage-avatar"), avatar.Render()));
            }

            _innerElement.appendChild(_bubbleContainer);

            if (commands != null)
            {
                _innerElement.appendChild(Div(Att("tss-chatmessage-commands"), commands.Render()));
            }
        }

        /// <summary>
        /// Configures the component to animated.
        /// </summary>
        public ChatMessage Animated()
        {
            _deltaComponent.Animated();
            return this;
        }
        internal void SetParent(ChatArea parent)
        {
            _parent = parent;
        }

        internal double MeasuredHeight => _innerElement.offsetHeight;

        internal void ReserveMinHeight(double pixels)
        {
            _innerElement.style.minHeight = pixels > 0 ? pixels + "px" : "";
        }

        /// <summary>
        /// Marks this message as a turn boundary. When it is added, the transcript treats it as the start of a new
        /// turn: it re-engages following and settles the message near the top of the viewport (with a peek of the
        /// previous turn above), so the reply that follows grows into the screen below it.
        /// </summary>
        public ChatMessage ScrollAnchor()
        {
            IsAnchor = true;
            _innerElement.setAttribute("tss-chat-anchor", "true");
            return this;
        }

        /// <summary>
        /// Configures the left aligned on the component.
        /// </summary>
        public ChatMessage LeftAligned()
        {
            _innerElement.classList.remove("tss-chat-right");
            _innerElement.classList.add("tss-chat-left");
            return this;
        }

        /// <summary>
        /// Configures the right aligned on the component.
        /// </summary>
        public ChatMessage RightAligned()
        {
            _innerElement.classList.remove("tss-chat-left");
            _innerElement.classList.add("tss-chat-right");
            return this;
        }

        /// <summary>
        /// Stretches the component to the full width of its parent.
        /// </summary>
        public ChatMessage FullWidth()
        {
            _innerElement.classList.remove("tss-chat-maxwidth");
            _innerElement.classList.add("tss-chat-fullwidth");
            return this;
        }

        /// <summary>
        /// Gets or sets the CSS max-width of the component.
        /// </summary>
        public ChatMessage MaxWidth()
        {
            _innerElement.classList.remove("tss-chat-fullwidth");
            _innerElement.classList.add("tss-chat-maxwidth");
            return this;
        }

        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public ChatMessage Background(string color)
        {
            BubbleBackground = color;
            _bubbleContainer.style.background = color;
            return this;
        }

        /// <summary>
        /// Returns the component configured with the given references.
        /// </summary>
        public ChatMessage WithReferences(IEnumerable<IComponent> references)
        {
            _referencesContainer.innerHTML = "";
            bool hasRef = false;
            foreach (var r in references)
            {
                _referencesContainer.appendChild(r.Render());
                hasRef = true;
            }
            _referencesContainer.style.display = hasRef ? "flex" : "none";
            return this;
        }

        /// <summary>
        /// Returns the component configured with the given references.
        /// </summary>
        public ChatMessage WithReferences(IComponent reference)
        {
            return WithReferences(new[] { reference });
        }

        /// <summary>
        /// Replaces the content in the component.
        /// </summary>
        public ChatMessage ReplaceContent(IComponent newContent)
        {
            ContentHash = Guid.NewGuid().ToString();
            _currentContent = newContent;
            _deltaComponent.ReplaceContent(newContent);
            return this;
        }

        /// <summary>
        /// Configures the keep visible on the component.
        /// </summary>
        public void KeepVisible()
        {
            if (_parent != null)
            {
                _parent.EnsureVisible(this);
            }
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render()
        {
            return _innerElement;
        }
    }
}
