using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Message")]
    public class Message : ComponentBase<Message, HTMLDivElement>
    {
        private readonly HTMLDivElement _iconContainer;
        private readonly HTMLDivElement _titleContainer;
        private readonly HTMLDivElement _textContainer;
        private readonly HTMLDivElement _noteContainer;

        public Message(string title = null, string message = null)
        {
            InnerElement = Div(_("tss-message"));

            _iconContainer = Div(_("tss-message-icon"));
            _titleContainer = Div(_("tss-message-title"));
            _textContainer = Div(_("tss-message-text"));
            _noteContainer = Div(_("tss-message-note"));

            if (!string.IsNullOrEmpty(title)) Title(title);
            if (!string.IsNullOrEmpty(message)) Text(message);

            InnerElement.appendChild(_iconContainer);
            InnerElement.appendChild(_titleContainer);
            InnerElement.appendChild(_textContainer);
        }

        public Message Icon(UIcons icon, string color = null, TextSize size = TextSize.Large)
        {
            _iconContainer.innerHTML = "";
            var i = Tesserae.UI.Icon(icon, size: size).Foreground(color ?? "");
            _iconContainer.appendChild(i.Render());
            return this;
        }

        public Message Icon(Image image)
        {
            _iconContainer.innerHTML = "";
            _iconContainer.appendChild(image.Render());
            return this;
        }

        public Message Title(string title)
        {
            _titleContainer.textContent = title;
            return this;
        }

        public Message Title(IComponent title)
        {
            _titleContainer.innerHTML = "";
            _titleContainer.appendChild(title.Render());
            return this;
        }

        public Message Text(string text)
        {
            _textContainer.textContent = text;
            return this;
        }

        public Message Text(IComponent text)
        {
            _textContainer.innerHTML = "";
            _textContainer.appendChild(text.Render());
            return this;
        }

        public Message Note(string note)
        {
            _noteContainer.innerHTML = "";
            _noteContainer.appendChild(TextBlock(note).Render());
            if(!InnerElement.contains(_noteContainer)) InnerElement.appendChild(_noteContainer);
            return this;
        }

        public Message Note(IComponent note)
        {
            _noteContainer.innerHTML = "";
            _noteContainer.appendChild(note.Render());
            if(!InnerElement.contains(_noteContainer)) InnerElement.appendChild(_noteContainer);
            return this;
        }

        public Message Variant(MessageVariant variant)
        {
            InnerElement.classList.remove("tss-message-default", "tss-message-success", "tss-message-warning", "tss-message-error");
            InnerElement.classList.add($"tss-message-{variant.ToString().ToLower()}");
            return this;
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }

    [H5.Name("tss.MessageVariant")]
    public enum MessageVariant
    {
        Default,
        Success,
        Warning,
        Error
    }
}
