using System;
using TNT;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The row that says who is signed in: a picture, a name, an optional second line for whatever
    /// identifies the account beside the name - an e-mail address, a company, a tenant - and the commands
    /// that belong to it, settings and signing out.
    /// <para>
    /// The picture falls back to the account's initials, so a photo URL that only answers for accounts that
    /// have uploaded one can be passed unconditionally. On the collapsed rail the row is that picture alone,
    /// with the name and the second line in its tooltip.
    /// </para>
    /// </summary>
    public class SidebarProfile : SidebarIdentityRow<SidebarProfile>
    {
        private readonly Avatar _openAvatar;
        private readonly Avatar _closedAvatar;

        private bool _hasExplicitInitials;

        /// <summary>
        /// Initializes a new instance of the SidebarProfile class.
        /// </summary>
        /// <param name="identifier">The identifier for the item.</param>
        /// <param name="name">The name to show on the first line.</param>
        /// <param name="subtitle">What identifies the account beside the name - an e-mail address, a company, a tenant. None leaves the row a single line.</param>
        /// <param name="pictureUrl">The picture to show. None - or one that fails to load - falls back to the initials.</param>
        /// <param name="initials">The initials to fall back to. None takes them from the name.</param>
        public SidebarProfile(string identifier, string name, string subtitle = null, string pictureUrl = null, string initials = null)
            : this(identifier, name, subtitle, InitialAvatars(pictureUrl, string.IsNullOrWhiteSpace(initials) ? InitialsFor(name) : initials))
        {
            _hasExplicitInitials = !string.IsNullOrWhiteSpace(initials);
        }

        private SidebarProfile(string identifier, string name, string subtitle, Avatar[] avatars)
            : base(identifier, "tss-sidebar-profile", avatars[0], avatars[1], name, subtitle)
        {
            _openAvatar   = avatars[0];
            _closedAvatar = avatars[1];
        }

        /// <summary>
        /// The two avatars a row needs - one for the open row, one for the collapsed rail, since a single
        /// element cannot be in two places. Built before the base constructor runs, which is what the array
        /// is for. They are the same size: the collapsed rail shows the row's picture in a box of the row's
        /// own height, not a smaller row.
        /// </summary>
        private static Avatar[] InitialAvatars(string pictureUrl, string initials)
        {
            return new[]
            {
                new Avatar(pictureUrl, initials).Size(AvatarSize.Small),
                new Avatar(pictureUrl, initials).Size(AvatarSize.Small)
            };
        }

        /// <inheritdoc/>
        protected override SidebarProfile Self => this;

        /// <summary>
        /// Sets the name shown on the first line, and the initials taken from it where none were given.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarProfile SetName(string name)
        {
            SetTitleText(name);

            if (!_hasExplicitInitials)
            {
                var initials = InitialsFor(TitleText);
                _openAvatar.SetInitials(initials);
                _closedAvatar.SetInitials(initials);
            }

            return this;
        }

        /// <summary>
        /// Sets the picture. One that fails to load falls back to the initials, so a URL that answers only
        /// for accounts that have uploaded a photo can be passed unconditionally.
        /// </summary>
        /// <param name="pictureUrl">The picture URL.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarProfile SetPicture(string pictureUrl)
        {
            _openAvatar.SetImage(pictureUrl);
            _closedAvatar.SetImage(pictureUrl);
            return this;
        }

        /// <summary>
        /// Sets the initials the picture falls back to. None - the default - takes them from the name.
        /// </summary>
        /// <param name="initials">The initials.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarProfile SetInitials(string initials)
        {
            _hasExplicitInitials = !string.IsNullOrWhiteSpace(initials);

            var value = _hasExplicitInitials ? initials : InitialsFor(TitleText);

            _openAvatar.SetInitials(value);
            _closedAvatar.SetInitials(value);
            return this;
        }

        /// <summary>
        /// Shows a presence dot on the picture - online, away, busy, offline.
        /// </summary>
        /// <param name="presence">The presence state.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarProfile Presence(AvatarPresence presence)
        {
            _openAvatar.Presence(presence);
            _closedAvatar.Presence(presence);
            return this;
        }

        /// <summary>
        /// Adds the command that opens the account's settings.
        /// </summary>
        /// <param name="onClick">What pressing it does.</param>
        /// <param name="tooltip">The tooltip. None uses "Settings".</param>
        /// <param name="icon">The icon. Defaults to a gear.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarProfile Settings(Action onClick, string tooltip = null, UIcons icon = UIcons.Settings)
        {
            return Settings(new SidebarCommand(icon).Tooltip(string.IsNullOrWhiteSpace(tooltip) ? "Settings".t() : tooltip).OnClick(onClick));
        }

        /// <summary>
        /// Sets the settings command, built by the caller - for one that opens a menu
        /// (<see cref="SidebarCommand.OnClickMenu"/>) rather than going straight to a settings page, or one
        /// that is an <c>href</c>. Passing null removes it.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarProfile Settings(SidebarCommand command)
        {
            SetPrimaryCommand(command);
            return this;
        }

        /// <summary>
        /// Adds the command that signs the account out. Optional: an application whose only way out is a menu
        /// item, or that has no way out at all, leaves it off.
        /// </summary>
        /// <param name="onClick">What pressing it does.</param>
        /// <param name="tooltip">The tooltip. None uses "Log out".</param>
        /// <param name="icon">The icon. Defaults to the sign-out arrow.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarProfile Logout(Action onClick, string tooltip = null, UIcons icon = UIcons.SignOutAlt)
        {
            return Logout(new SidebarCommand(icon).Tooltip(string.IsNullOrWhiteSpace(tooltip) ? "Log out".t() : tooltip).OnClick(onClick));
        }

        /// <summary>
        /// Sets the logout command, built by the caller. Passing null removes it.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarProfile Logout(SidebarCommand command)
        {
            SetSecondaryCommand(command);
            return this;
        }

        /// <summary>
        /// The initials a name falls back to when there is no picture: the first letter of the first word and
        /// of the last, so "M. Okafor" reads MO and "Okafor" reads O. Anything that is not a letter or a digit
        /// is skipped, which is what keeps an initial out of "M." and out of an emoji.
        /// </summary>
        public static string InitialsFor(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            var words  = name.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var result = string.Empty;

            for (int i = 0; i < words.Length; i++)
            {
                if (i != 0 && i != words.Length - 1) continue;

                var letter = FirstLetterOrDigit(words[i]);

                if (letter != '\0') result += char.ToUpper(letter);
            }

            return result;
        }

        private static char FirstLetterOrDigit(string word)
        {
            for (int i = 0; i < word.Length; i++)
            {
                if (char.IsLetterOrDigit(word[i])) return word[i];
            }

            return '\0';
        }
    }
}
