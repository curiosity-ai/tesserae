using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Avatar")]
    public class Avatar : ComponentBase<Avatar, HTMLDivElement>
    {
        private readonly HTMLImageElement _image;
        private readonly HTMLSpanElement _initials;

        public Avatar(string initials = string.Empty, string imageSrc = null)
        {
            _image = Image(_(src: imageSrc));
            _initials = Span(_("tss-avatar-initials", text: initials));

            InnerElement = Div(_("tss-avatar tss-avatar-size-medium"));

            if (string.IsNullOrEmpty(imageSrc))
            {
                InnerElement.appendChild(_initials);
            }
            else
            {
                InnerElement.appendChild(_image);
            }

            _image.onerror = (e) =>
            {
                if (InnerElement.contains(_image))
                {
                    InnerElement.replaceChild(_initials, _image);
                }
            };

            AttachClick();
        }

        public Avatar SetInitials(string initials)
        {
            _initials.innerText = initials;
            return this;
        }

        public Avatar SetImage(string src)
        {
            _image.src = src;
            if (!InnerElement.contains(_image))
            {
                ClearChildren(InnerElement);
                InnerElement.appendChild(_image);
            }
            return this;
        }

        public Avatar Small()
        {
            InnerElement.classList.remove("tss-avatar-size-medium", "tss-avatar-size-large", "tss-avatar-size-huge");
            InnerElement.classList.add("tss-avatar-size-small");
            return this;
        }

        public Avatar Medium()
        {
            InnerElement.classList.remove("tss-avatar-size-small", "tss-avatar-size-large", "tss-avatar-size-huge");
            InnerElement.classList.add("tss-avatar-size-medium");
            return this;
        }

        public Avatar Large()
        {
            InnerElement.classList.remove("tss-avatar-size-small", "tss-avatar-size-medium", "tss-avatar-size-huge");
            InnerElement.classList.add("tss-avatar-size-large");
            return this;
        }

        public Avatar Huge()
        {
            InnerElement.classList.remove("tss-avatar-size-small", "tss-avatar-size-medium", "tss-avatar-size-large");
            InnerElement.classList.add("tss-avatar-size-huge");
            return this;
        }

        public Avatar Square()
        {
            InnerElement.classList.add("tss-avatar-square");
            return this;
        }

        public Avatar Circle()
        {
            InnerElement.classList.remove("tss-avatar-square");
            return this;
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
