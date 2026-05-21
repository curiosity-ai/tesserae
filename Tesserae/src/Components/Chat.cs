using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A chat transcript surface that lays out a sequence of messages with sender attribution, avatars and
    /// timestamps.
    /// </summary>
    [H5.Name("tss.ChatArea")]
    public class ChatArea : IComponent
    {
        private HTMLElement _innerElement;
        private ObservableList<IComponentWithID> _messages;
        private ObservableStack _stack;

        private bool _stopAutoScroll = false;
        private string _bubbleBackground = null;

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

            _stack = new ObservableStack(_messages, Stack.Orientation.Vertical, debounce: false).S();

            _innerElement = Div(_("tss-chatarea"), _stack.Render());

            _innerElement.addEventListener("scroll", (e) =>
            {
                // If user scrolls up (meaning not at the bottom), stop auto-scrolling
                var scrollHeight = _innerElement.scrollHeight;
                var scrollTop = _innerElement.scrollTop;
                var clientHeight = _innerElement.clientHeight;

                // Allow a small threshold (e.g. 5px) for being "at the bottom"
                if (scrollHeight - scrollTop - clientHeight > 5)
                {
                    _stopAutoScroll = true;
                }
                else
                {
                    _stopAutoScroll = false;
                }

                Scrolled?.Invoke(this, e);
            });

            _innerElement.addEventListener("focusin", (e) => ReceivedFocus?.Invoke(this, e));
            _innerElement.addEventListener("focusout", (e) => LostFocus?.Invoke(this, e));
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
        /// Adds the given item to the component.
        /// </summary>
        public ChatArea Add(ChatMessage message)
        {
            if (_bubbleBackground != null && message.BubbleBackground == null)
            {
                message.Background(_bubbleBackground);
            }
            message.SetParent(this);
            _messages.Add(message);

            // Reset stop flag on new message so it auto-scrolls to it
            _stopAutoScroll = false;

            // Delay slighty to let DOM update
            window.setTimeout((_) => EnsureVisible(message), 10);

            return this;
        }

        /// <summary>
        /// Clears the component's current state.
        /// </summary>
        public ChatArea Clear()
        {
            _messages.Clear();
            return this;
        }

        internal void EnsureVisible(ChatMessage message)
        {
            if (_stopAutoScroll) return;

            var el = message.Render();
            if (el != null)
            {
                _innerElement.scrollTop = _innerElement.scrollHeight;
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

    [H5.Name("tss.ChatMessage")]
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
        /// Initializes a new instance of this class.
        /// </summary>
        public ChatMessage(IComponent content, IComponent avatar = null, IComponent commands = null)
        {
            Identifier = Guid.NewGuid().ToString();
            ContentHash = Guid.NewGuid().ToString();

            _currentContent = content;
            _deltaComponent = DeltaComponent(content);

            _contentContainer = Div(_("tss-chatmessage-content"), _deltaComponent.Render());
            _referencesContainer = Div(_("tss-chatmessage-references"));
            _referencesContainer.style.display = "none";

            _bubbleContainer = Div(_("tss-chatmessage-bubble"), _contentContainer, _referencesContainer);

            _innerElement = Div(_("tss-chatmessage-container tss-chat-left"));

            if (avatar != null)
            {
                _innerElement.appendChild(Div(_("tss-chatmessage-avatar"), avatar.Render()));
            }

            _innerElement.appendChild(_bubbleContainer);

            if (commands != null)
            {
                _innerElement.appendChild(Div(_("tss-chatmessage-commands"), commands.Render()));
            }
        }

        internal void SetParent(ChatArea parent)
        {
            _parent = parent;
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