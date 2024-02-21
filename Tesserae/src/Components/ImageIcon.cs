using static H5.Core.dom;
using static Tesserae.UI;

#if DEBUG
using System;
using H5.Core;
#endif

namespace Tesserae
{
    [H5.Name("tss.EmojiImageIcon")]
    public class EmojiImageIcon : ISidebarIcon
    {
        private HTMLElement _img;

        public EmojiImageIcon(string icon)
        {
#if DEBUG
            var emojiregExp = new es5.RegExp("\\p{Extended_Pictographic}", "u");

            if (!emojiregExp.test(icon))
            {
                throw new ArgumentException("Not an emoji");
            }

#endif

            _img = Span(_("tss-image", text: icon));
        }
        public HTMLElement Render() => _img;

        public ISidebarIcon Clone() => new EmojiImageIcon(_img.textContent);
    }

    [H5.Name("tss.IconImageIcon")]
    public class IconImageIcon : ISidebarIcon
    {
        private HTMLElement _img;

        public IconImageIcon(string icon)
        {
            _img                 = I(_("tss-image tss-icon " + icon));
            _img.dataset["icon"] = icon;

        }
        public HTMLElement Render() => _img;

        public ISidebarIcon Clone() => new IconImageIcon(_img.dataset["icon"].As<string>());
    }


    [H5.Name("tss.ImageIcon")]
    public class ImageIcon : ISidebarIcon
    {
        private HTMLImageElement _img;
        public ImageIcon(string source, string backgroundColor = null)
        {
            _img = Image(string.IsNullOrWhiteSpace(backgroundColor)
                ? _("tss-image", src: source)
                : _("tss-image", src: source, styles: s => s.backgroundColor = backgroundColor));

        }
        public HTMLElement Render() => _img;

        public ISidebarIcon Clone() => new ImageIcon(_img.src, _img.style.backgroundColor);

        public void SetImageSrc(string src)
        {
            _img.src = src;
        }
    }

    public interface ISidebarIcon : IComponent
    {
        ISidebarIcon Clone();
    }
}