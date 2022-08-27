using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.ImageIcon")]
    public class ImageIcon : IComponent
    {
        private HTMLImageElement _img;
        public ImageIcon(string source)
        {
            _img = UI.Image(_("tss-image", src: source));
        }


        public string Source { get { return _img.src; } set { _img.src = value;  } }

        public HTMLElement Render() => _img;

        public ImageIcon Clone() => new ImageIcon(_img.src);
    }
}