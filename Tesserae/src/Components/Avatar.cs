using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public enum AvatarSize
    {
        XSmall,
        Small,
        Medium,
        Large,
        XLarge
    }

    public enum AvatarPresence
    {
        None,
        Online,
        Away,
        Busy,
        Offline
    }

    [H5.Name("tss.Avatar")]
    public sealed class Avatar : ComponentBase<Avatar, HTMLElement>
    {
        private readonly HTMLImageElement _image;
        private readonly HTMLSpanElement  _initials;
        private readonly HTMLSpanElement  _presence;
        private          AvatarSize       _size;
        private          AvatarPresence   _presenceState;

        public Avatar(string imageUrl = null, string initials = null)
        {
            _image    = new HTMLImageElement();
            _image.className = "tss-avatar-image";

            _initials = Span(_("tss-avatar-initials", text: initials ?? string.Empty));
            _presence = Span(_("tss-avatar-presence"));

            InnerElement = Div(_("tss-avatar"), _image, _initials, _presence);

            SetImage(imageUrl);
            SetInitials(initials);
            Size(AvatarSize.Medium);
            Presence(AvatarPresence.None);
        }

        public string ImageUrl
        {
            get => _image.src;
            set => SetImage(value);
        }

        public string Initials
        {
            get => _initials.innerText;
            set => SetInitials(value);
        }

        public AvatarSize SizeValue
        {
            get => _size;
            set => Size(value);
        }

        public AvatarPresence PresenceState
        {
            get => _presenceState;
            set => Presence(value);
        }

        public Avatar SetImage(string url)
        {
            _image.src = url ?? string.Empty;
            UpdateImageState();
            return this;
        }

        public Avatar SetInitials(string initials)
        {
            _initials.innerText = initials ?? string.Empty;
            UpdateImageState();
            return this;
        }

        public Avatar Size(AvatarSize size)
        {
            _size = size;
            InnerElement.classList.remove("tss-avatar-xs", "tss-avatar-sm", "tss-avatar-md", "tss-avatar-lg", "tss-avatar-xl");

            switch (size)
            {
                case AvatarSize.XSmall:
                    InnerElement.classList.add("tss-avatar-xs");
                    break;
                case AvatarSize.Small:
                    InnerElement.classList.add("tss-avatar-sm");
                    break;
                case AvatarSize.Medium:
                    InnerElement.classList.add("tss-avatar-md");
                    break;
                case AvatarSize.Large:
                    InnerElement.classList.add("tss-avatar-lg");
                    break;
                case AvatarSize.XLarge:
                    InnerElement.classList.add("tss-avatar-xl");
                    break;
            }

            return this;
        }

        public Avatar Presence(AvatarPresence presence)
        {
            _presenceState = presence;

            _presence.classList.remove(
                "tss-presence-online",
                "tss-presence-away",
                "tss-presence-busy",
                "tss-presence-offline");

            if (presence == AvatarPresence.None)
            {
                _presence.style.display = "none";
            }
            else
            {
                _presence.style.display = "block";

                switch (presence)
                {
                    case AvatarPresence.Online:
                        _presence.classList.add("tss-presence-online");
                        break;
                    case AvatarPresence.Away:
                        _presence.classList.add("tss-presence-away");
                        break;
                    case AvatarPresence.Busy:
                        _presence.classList.add("tss-presence-busy");
                        break;
                    case AvatarPresence.Offline:
                        _presence.classList.add("tss-presence-offline");
                        break;
                }
            }

            return this;
        }

        public Avatar Background(string color)
        {
            InnerElement.style.backgroundColor = color;
            return this;
        }

        public Avatar Foreground(string color)
        {
            _initials.style.color = color;
            return this;
        }

        private void UpdateImageState()
        {
            var hasImage = !string.IsNullOrEmpty(_image.src);
            InnerElement.UpdateClassIf(hasImage, "tss-avatar-has-image");
            _image.style.display    = hasImage ? "block" : "none";
            _initials.style.display = hasImage ? "none" : "flex";
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }

    [H5.Name("tss.Persona")]
    public sealed class Persona : ComponentBase<Persona, HTMLElement>
    {
        private          Avatar          _avatar;
        private readonly HTMLSpanElement _name;
        private readonly HTMLSpanElement _secondary;
        private readonly HTMLSpanElement _tertiary;
        private readonly HTMLElement     _textContainer;
        private readonly HTMLElement     _avatarContainer;

        public Persona(string name = null, string secondaryText = null, string tertiaryText = null, Avatar avatar = null)
        {
            _avatar = avatar ?? new Avatar(initials: string.Empty).Size(AvatarSize.Medium);
            _name = Span(_("tss-persona-name", text: name ?? string.Empty));
            _secondary = Span(_("tss-persona-secondary", text: secondaryText ?? string.Empty));
            _tertiary = Span(_("tss-persona-tertiary", text: tertiaryText ?? string.Empty));

            _textContainer = Div(_("tss-persona-text"), _name, _secondary, _tertiary);
            _avatarContainer = Div(_("tss-persona-avatar"), _avatar.Render());
            InnerElement = Div(_("tss-persona"), _avatarContainer, _textContainer);

            UpdateOptionalText();
        }

        public Persona SetAvatar(Avatar avatar)
        {
            if (avatar == null)
            {
                return this;
            }

            _avatar = avatar;
            ClearChildren(_avatarContainer);
            _avatarContainer.appendChild(_avatar.Render());
            return this;
        }

        public Persona SetName(string name)
        {
            _name.innerText = name ?? string.Empty;
            return this;
        }

        public Persona SetSecondaryText(string text)
        {
            _secondary.innerText = text ?? string.Empty;
            UpdateOptionalText();
            return this;
        }

        public Persona SetTertiaryText(string text)
        {
            _tertiary.innerText = text ?? string.Empty;
            UpdateOptionalText();
            return this;
        }

        public Persona Compact(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-persona-compact");
            return this;
        }

        private void UpdateOptionalText()
        {
            _secondary.style.display = string.IsNullOrEmpty(_secondary.innerText) ? "none" : "block";
            _tertiary.style.display = string.IsNullOrEmpty(_tertiary.innerText) ? "none" : "block";
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
