using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// An inline informational message strip with predefined tones (info, success, warning, error) and an optional
    /// dismiss button.
    /// </summary>
    [H5.Name("tss.Message")]
    public class Message : ComponentBase<Message, HTMLDivElement>
    {
        private readonly HTMLDivElement _iconContainer;
        private readonly HTMLDivElement _contentContainer;
        private readonly HTMLDivElement _titleContainer;
        private readonly HTMLDivElement _textContainer;
        private readonly HTMLDivElement _noteContainer;

        private IComponent  _action;
        private HTMLElement _actionContainer;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Message(string title = null, string message = null)
        {
            InnerElement = Div(_("tss-message"));

            _iconContainer    = Div(_("tss-message-icon"));
            _contentContainer = Div(_("tss-message-content"));
            _titleContainer   = Div(_("tss-message-title"));
            _textContainer    = Div(_("tss-message-text"));
            _noteContainer    = Div(_("tss-message-note"));
            _actionContainer  = Div(_("tss-message-action"));

            if (!string.IsNullOrEmpty(title)) Title(title);
            if (!string.IsNullOrEmpty(message)) Text(message);

            _contentContainer.appendChild(_titleContainer);
            _contentContainer.appendChild(_textContainer);

            InnerElement.appendChild(_iconContainer);
            InnerElement.appendChild(_contentContainer);
            InnerElement.appendChild(_actionContainer);
        }

        /// <summary>
        /// Configures the component to icon.
        /// </summary>
        public Message Icon(UIcons icon, string color = null, TextSize size = TextSize.Large)
        {
            _iconContainer.innerHTML = "";
            var i = Tesserae.UI.Icon(icon, size: size).Foreground(color ?? "");
            _iconContainer.appendChild(i.Render());
            return this;
        }

        /// <summary>
        /// Configures the component to icon.
        /// </summary>
        public Message Icon(Image image)
        {
            _iconContainer.innerHTML = "";
            _iconContainer.appendChild(image.Render());
            return this;
        }

        /// <summary>
        /// Gets or sets the title of the component.
        /// </summary>
        public Message Title(string title)
        {
            _titleContainer.textContent = title;
            return this;
        }

        /// <summary>
        /// Gets or sets the title of the component.
        /// </summary>
        public Message Title(IComponent title)
        {
            _titleContainer.innerHTML = "";
            _titleContainer.appendChild(title.Render());
            return this;
        }

        /// <summary>
        /// Gets or sets the text shown in the component.
        /// </summary>
        public Message Text(string text)
        {
            _textContainer.textContent = text;
            return this;
        }

        /// <summary>
        /// Gets or sets the text shown in the component.
        /// </summary>
        public Message Text(IComponent text)
        {
            _textContainer.innerHTML = "";
            _textContainer.appendChild(text.Render());
            return this;
        }

        /// <summary>
        /// Configures the component to note.
        /// </summary>
        public Message Note(string note)
        {
            _noteContainer.innerHTML = "";
            _noteContainer.appendChild(TextBlock(note).Render());
            if(!_contentContainer.contains(_noteContainer)) _contentContainer.appendChild(_noteContainer);
            return this;
        }

        /// <summary>
        /// Configures the component to note.
        /// </summary>
        public Message Note(IComponent note)
        {
            _noteContainer.innerHTML = "";
            _noteContainer.appendChild(note.Render());
            if(!_contentContainer.contains(_noteContainer)) _contentContainer.appendChild(_noteContainer);
            return this;
        }

        /// <summary>
        /// Configures the component to variant.
        /// </summary>
        public Message Variant(MessageVariant variant)
        {
            InnerElement.classList.remove("tss-message-default", "tss-message-info", "tss-message-success", "tss-message-warning", "tss-message-error");
            InnerElement.classList.add($"tss-message-{variant.ToString().ToLower()}");
            return this;
        }

        /// <summary>
        /// Lays the message out horizontally, with the icon beside the text instead of stacked above it.
        /// </summary>
        public Message Horizontal(bool horizontal = true)
        {
            if (horizontal) InnerElement.classList.add("tss-message-horizontal");
            else            InnerElement.classList.remove("tss-message-horizontal");
            return this;
        }

        /// <summary>
        /// Adds an action component to the message. On the default (vertical) layout it is rendered below the
        /// message content; on a horizontal message it is rendered on the right side, beside the content.
        /// </summary>
        public Message Action(IComponent action)
        {
            _action       = action;
            ClearChildren(_actionContainer);
            _actionContainer.appendChild(action.Render());
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }

    [H5.Name("tss.MessageVariant")]
    public enum MessageVariant
    {
        Default,
        Info,
        Success,
        Warning,
        Error
    }
}
