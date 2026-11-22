namespace Tesserae
{
    /// <summary>
    /// A stable colour per collaborator, for remote carets, selections and presence lists: the same name gets the same
    /// colour on every page and every peer, without anyone having to agree on one.
    /// </summary>
    /// <remarks>
    /// <para>It picks from a palette of ten hues that are clearly different from each other, rather than hashing the
    /// name onto a hue between 0 and 359. Two names that land eight degrees apart there are one colour rendered
    /// slightly inconsistently, not two colours - so two people are either clearly different here or exactly the same,
    /// and the name shown next to the caret settles the second case.</para>
    /// <para>The slot is taken from the whole range of the hash (<c>hash * 10 / modulus</c>), not from its remainder:
    /// the low digits of a short string's hash are barely mixed, and a remainder puts similar names in one slot.</para>
    /// <para>Every hue is dark enough to read as a 2px caret on a white editor and light enough on a dark one.</para>
    /// </remarks>
    public static class YjsUserColors
    {
        private const double Modulus = 2147483647; // 2^31 - 1, so every step of the hash is exact in a double

        /// <summary>The palette, in slot order.</summary>
        public static readonly string[] Palette = new[]
        {
            "#e5484d", // red
            "#f76b15", // orange
            "#b38600", // gold
            "#30a46c", // green
            "#12a594", // teal
            "#0090ff", // blue
            "#3e63dd", // indigo
            "#8e4ec6", // purple
            "#d6409f", // pink
            "#978365", // bronze
        };

        /// <summary>The palette slot of <paramref name="key"/> (a user's name or id), from 0 to 9.</summary>
        public static int SlotFor(string key)
        {
            var hash = 0.0;

            foreach (var c in key ?? "")
            {
                hash = (hash * 31 + c) % Modulus;
            }

            // Two rounds of a Lehmer step spread short strings over the whole range before the slot is read off its top.
            hash = (hash * 48271 + 1) % Modulus;
            hash = (hash * 48271)     % Modulus;

            var slot = (int)(hash * Palette.Length / Modulus);
            return slot < Palette.Length ? slot : Palette.Length - 1;
        }

        /// <summary>The colour of <paramref name="key"/>, as a <c>#rrggbb</c> CSS colour.</summary>
        public static string ColorFor(string key) => Palette[SlotFor(key)];

        /// <summary>The colour of <paramref name="key"/> at 20% opacity (<c>#rrggbb33</c>), for a selection.</summary>
        public static string LightColorFor(string key) => ColorFor(key) + "33";

        /// <summary>
        /// The awareness <c>user</c> field for <paramref name="name"/>, coloured by <paramref name="key"/> when given
        /// (a stable user id) or by the name otherwise.
        /// </summary>
        public static YjsUser UserFor(string name, string key = null)
        {
            var color = ColorFor(key ?? name);
            return new YjsUser { name = name, color = color, colorLight = color + "33" };
        }
    }
}
