using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Predefined size presets used by the <see cref="Avatar"/> component.
    /// </summary>
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

    [Transpose.Name("tss.Avatar")]
    public sealed class Avatar : ComponentBase<Avatar, HTMLElement>
    {
        private const string EmptyImage = "data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==";
        private readonly HTMLImageElement _image;
        private readonly HTMLSpanElement  _initials;
        private readonly HTMLSpanElement  _presence;
        private          AvatarSize       _size;
        private          AvatarPresence   _presenceState;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Avatar(string image = null, string initials = null)
        {
            _image    = new HTMLImageElement();
            _image.className = "tss-avatar-image";

            _image.onerror += (e) =>
            {
                _image.src = EmptyImage;
                UpdateImageState();
            };

            _initials = Span(Att("tss-avatar-initials", text: initials ?? string.Empty));
            _presence = Span(Att("tss-avatar-presence"));

            InnerElement = Div(Att("tss-avatar"), _image, _initials, _presence);

            SetImage(image);
            SetInitials(initials);
            Size(AvatarSize.Small);
            Presence(AvatarPresence.None);
        }

        /// <summary>
        /// Gets or sets the image url.
        /// </summary>
        public string ImageUrl
        {
            get => _image.src;
            set => SetImage(value);
        }

        /// <summary>
        /// Gets or sets the initials.
        /// </summary>
        public string Initials
        {
            get => _initials.innerText;
            set => SetInitials(value);
        }

        /// <summary>
        /// Gets or sets the size value.
        /// </summary>
        public AvatarSize SizeValue
        {
            get => _size;
            set => Size(value);
        }

        /// <summary>
        /// Gets or sets the presence state.
        /// </summary>
        public AvatarPresence PresenceState
        {
            get => _presenceState;
            set => Presence(value);
        }

        /// <summary>
        /// Sets the image of the component.
        /// </summary>
        public Avatar SetImage(string url)
        {
            _image.src = url ?? string.Empty;
            UpdateImageState();
            return this;
        }

        /// <summary>
        /// Sets the initials of the component.
        /// </summary>
        public Avatar SetInitials(string initials)
        {
            _initials.innerText = initials ?? string.Empty;
            InnerElement.style.background = ""; // clear so we recalculate gradient
            UpdateImageState();
            return this;
        }

        /// <summary>
        /// Gets or sets the size of the component.
        /// </summary>
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

        /// <summary>
        /// Configures the component to presence.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public Avatar Background(string color)
        {
            InnerElement.style.backgroundColor = color;
            return this;
        }

        /// <summary>
        /// Gets or sets the CSS color (foreground) of the component.
        /// </summary>
        public Avatar Foreground(string color)
        {
            _initials.style.color = color;
            return this;
        }

        private static string GetGradientForInitials(string initials)
        {
            if (string.IsNullOrWhiteSpace(initials))
                return "";

            int hash = 0;
            for (int i = 0; i < initials.Length; i++)
            {
                hash = initials[i] + ((hash << 5) - hash);
            }

            var h1 = System.Math.Abs(hash) % 360;
            var h2 = (h1 + 40) % 360;

            return $"linear-gradient(135deg, hsl({h1}, 60%, 50%), hsl({h2}, 80%, 40%))";
        }

        private void UpdateImageState()
        {
            var hasImage = !string.IsNullOrWhiteSpace(_image.src) && _image.src != EmptyImage;
            InnerElement.UpdateClassIf(hasImage, "tss-avatar-has-image");
            _image.style.display    = hasImage ? "block" : "none";
            _initials.style.display = hasImage ? "none" : "flex";

            if (!hasImage && !string.IsNullOrWhiteSpace(Initials))
            {
                if (string.IsNullOrEmpty(InnerElement.style.background) && string.IsNullOrEmpty(InnerElement.style.backgroundColor))
                {
                    InnerElement.style.background = GetGradientForInitials(Initials);
                    _initials.style.color = "white";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(InnerElement.style.background) && InnerElement.style.background.Contains("linear-gradient"))
                {
                    InnerElement.style.background = "";
                    _initials.style.color = "";
                }
            }
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }

    [Transpose.Name("tss.Persona")]
    public sealed class Persona : ComponentBase<Persona, HTMLElement>
    {
        private          Avatar          _avatar;
        private readonly HTMLSpanElement _name;
        private readonly HTMLSpanElement _secondary;
        private readonly HTMLSpanElement _tertiary;
        private readonly HTMLElement     _textContainer;
        private readonly HTMLElement     _avatarContainer;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Persona(string name = null, string secondaryText = null, string tertiaryText = null, Avatar avatar = null)
        {
            _avatar = avatar ?? new Avatar(initials: string.Empty).Size(AvatarSize.Medium);
            _name = Span(Att("tss-persona-name", text: name ?? string.Empty));
            _secondary = Span(Att("tss-persona-secondary", text: secondaryText ?? string.Empty));
            _tertiary = Span(Att("tss-persona-tertiary", text: tertiaryText ?? string.Empty));

            _textContainer = Div(Att("tss-persona-text"), _name, _secondary, _tertiary);
            _avatarContainer = Div(Att("tss-persona-avatar"), _avatar.Render());
            InnerElement = Div(Att("tss-persona"), _avatarContainer, _textContainer);

            UpdateOptionalText();
        }

        /// <summary>
        /// Sets the avatar of the component.
        /// </summary>
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

        /// <summary>
        /// Sets the name of the component.
        /// </summary>
        public Persona SetName(string name)
        {
            _name.innerText = name ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the secondary text of the component.
        /// </summary>
        public Persona SetSecondaryText(string text)
        {
            _secondary.innerText = text ?? string.Empty;
            UpdateOptionalText();
            return this;
        }

        /// <summary>
        /// Sets the tertiary text of the component.
        /// </summary>
        public Persona SetTertiaryText(string text)
        {
            _tertiary.innerText = text ?? string.Empty;
            UpdateOptionalText();
            return this;
        }

        /// <summary>
        /// Renders the component in a compact form.
        /// </summary>
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

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
