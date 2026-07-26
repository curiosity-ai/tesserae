namespace Tesserae.Tests
{
    /// <summary>
    /// The key the PixelAvatar sprite artwork is scrambled with.
    ///
    /// It lives here, in the application, and deliberately nowhere in the Tesserae library: the
    /// toolkit ships the sheet obfuscated and only decodes it once a <see cref="PixelAvatar"/> is
    /// constructed with this value. It keeps the artwork out of casual sight in the shipped
    /// JavaScript and nothing more - a key the browser has to see is readable by anyone who looks.
    /// </summary>
    internal static class PixelAvatarSpriteKey
    {
        internal const byte Value = 42;
    }
}
