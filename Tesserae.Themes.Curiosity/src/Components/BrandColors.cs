namespace Tesserae.Themes.Curiosity
{
    /// <summary>
    /// The Curiosity brand palette (design-system/BRAND.md in the website repository), as CSS colours. The
    /// Curiosity components draw with these directly rather than through the theme's variables, so they look
    /// the same whether or not the theme is active.
    /// </summary>
    public static class BrandColors
    {
        /// <summary>The default light canvas.</summary>
        public const string Paper  = "#F4F4F2";
        /// <summary>Panels, the secondary canvas.</summary>
        public const string Stone  = "#E6E8E8";
        /// <summary>Hairlines on paper.</summary>
        public const string Line   = "#DEE0DF";
        /// <summary>Text on paper; the dark canvas.</summary>
        public const string Ink    = "#111418";
        /// <summary>Secondary text, dashes on paper, hairlines on ink.</summary>
        public const string Slate  = "#2A2F38";
        /// <summary>Dashes on ink, muted marks.</summary>
        public const string Ash    = "#8A9099";
        /// <summary>Muted marks and large type on paper.</summary>
        public const string Mute   = "#6B7280";
        /// <summary>Secondary text on ink.</summary>
        public const string Ink2   = "#C5C9CE";
        /// <summary>The Signal: the one electric blue.</summary>
        public const string Signal = "#2F6BFF";
        /// <summary>The deep call blue: primary actions and blue grounds.</summary>
        public const string Deep   = "#0029E7";
    }
}
