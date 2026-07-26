namespace Tesserae.Tests
{
    /// <summary>
    /// The key the PixelAvatar artwork is packed with.
    ///
    /// It lives here, in the application, and deliberately nowhere in the Tesserae library: the
    /// toolkit ships the sprite sheet scrambled and cannot read it until something hands the key
    /// over through <see cref="PixelAvatarSprites.Unlock"/>. <c>Build.PackPixelSprites</c> takes
    /// the same value on its command line when regenerating the packed literal - the two have to
    /// match, and this is the copy of record.
    ///
    /// Being a key that has to reach the browser, it is readable by anyone who looks; the
    /// scrambling keeps the artwork out of casual sight and nothing more. See
    /// <see cref="PackedText"/>.
    /// </summary>
    internal static class PixelAvatarSpriteKey
    {
        internal const string Value = "tesserae-pixel-cats";
    }
}
