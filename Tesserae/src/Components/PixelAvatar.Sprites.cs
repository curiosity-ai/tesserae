// Sprite data for the PixelAvatar component, from the source cat sprite sheets. Every sheet
// shares the same silhouette, so the artwork is stored once as a grid of palette indices and
// each design is only a palette.
//
// The block between the <packed-frames> markers is generated - run Build.PackPixelSprites to
// regenerate it from Build.PackPixelSprites/sprite-frames.txt. Everything else is hand-written.

using System;
using System.Collections.Generic;

namespace Tesserae
{
    /// <summary>
    /// The built-in sprite data shared by every <see cref="PixelAvatar"/>: one frame grid
    /// per animation, where each cell is an index into a <see cref="PixelAvatarPalette"/>
    /// (0 meaning transparent).
    /// </summary>
    [Transpose.Name("tss.PixelAvatarSprites")]
    public static class PixelAvatarSprites
    {
        /// <summary>Width, in pixels, of every sprite frame.</summary>
        public const int FrameWidth = 10;
        /// <summary>Height, in pixels, of every sprite frame.</summary>
        public const int FrameHeight = 8;
        /// <summary>Number of colors in a palette (palette index 0 is always transparent).</summary>
        public const int PaletteSize = 11;
        /// <summary>Highest palette index belonging to the highlight shade.</summary>
        public const int LastHighlightIndex = 3;
        /// <summary>Highest palette index belonging to the base shade; the rest are shadow.</summary>
        public const int LastBaseIndex = 9;

        /// <summary>
        /// The palette index of the right ear tip. Every frame contains exactly one pixel of
        /// this index, which is what lets a frame locate the ears without a silhouette scan.
        /// </summary>
        public const byte RightEarIndex = 3;
        /// <summary>How many cells left of the right ear tip the left one sits.</summary>
        public const int EarSpacing = 2;

        /// <summary>
        /// How many pixels across the whole sprite sheet use each palette index, indexed by
        /// palette index (entry 0 is the transparent index and is always 0). Lets callers work
        /// out which color covers most of the artwork - see
        /// <see cref="PixelAvatarPalette.DominantColor"/>.
        /// </summary>
        public static readonly int[] PixelCounts = new[] { 0, 128, 89, 43, 133, 133, 66, 43, 43, 2, 73, 38 };

        // Every frame is FrameWidth * FrameHeight characters laid out row by row, '.' meaning
        // transparent and '1'..'9','a','b' palette indices 1..11. The whole sheet is one string,
        // compressed and scrambled by PackedText - which is why it is a single literal rather
        // than the 43 readable grids it used to be, and why it needs Unlock before it can be
        // read. Build.PackPixelSprites is the CLI that generates it, from the plain frames in
        // its own sprite-frames.txt.
        private const string Alphabet = ".123456789ab";

        // <packed-frames>
        private const string PackedFrames =
            "dWVzfhU9nCiKyUd4amwgUk9HcyVUrF1LRFVRLGJbSpJZGlZh5kdABxFtZyFZBBgRasdhAi1iWXARcJR3FldAV2FJcfm6YQgVZ750" +
            "fHA3duZXQAdvKERdffFobVVYfnf0ZXN5enJub9V7T3GVY7lXYEB84WuMc2phnnbSY5Z4amwtbGWHeotsjHNqcm5vsnr2WGGTLWxl" +
            "G3x7anMSYfN4xS1xenhqaNJnnmB9cJd5bGV9Y2CiQXqHa5MurE2LX4tJjHNlfU2aAY9Fh0mTLWxDi1GLJX2MZX1haj6PbYdhmRxk" +
            "F2FCaGNFb2pKbHcrP2l31XRiQsZ043HwQUFQx1ViGUZfTjFqAmNudHxc6nNCmmGeZSJ0JlS6LMh+vnR8dEeiObpqvmHSdDZ4/mh9" +
            "Z5RwYLRhh11hjWFqKY9th2GTLWeedHx6mn2MYY1hajWPcYcldNJnnnR8Nep20GB0UzvS8Gl3YSMTh2yxXvFg817gQ2Fh3V32fJpo" +
            "0meecIx0aneMZXaeYdJwZnyaaNJnnnCMcJrzd5p2nmHScGZ8mmjSZ5RFc3CTboJhjWWaKY9gKUcpKZxhdHx0ai2MV21Gmi1/bEdX" +
            "wy0snnR8dGp3jGGNZZotf22HZWjSZ550fHRqd4xhjWWaLX9pfJpo0meedHx0aneEQTUZ9S10lnhqbCJKLj/gcJpzfGV9YT+ICbxf" +
            "mmjSY250fHH6duQldpFdyHBmeGpjslH7RTOFZXeMZX1haimPbYE=";
        // </packed-frames>

        private static string _key;
        private static Dictionary<PixelAvatarAnimation, PixelSpriteAnimation> _animations;

        /// <summary>
        /// Supplies the key the artwork was packed with, which the toolkit deliberately does not
        /// carry: the application holds it and hands it over. Call this once at startup, before
        /// the first <see cref="PixelAvatar"/> is created.
        ///
        /// Calling it again with a different key throws the decoded artwork away and unpacks it
        /// afresh on next use, so a wrong key is recoverable.
        /// </summary>
        public static void Unlock(string key)
        {
            _key        = key;
            _animations = null;
        }

        /// <summary>
        /// Gets whether the artwork has been decoded, or can be - that is, whether
        /// <see cref="Unlock"/> has been given a key.
        /// </summary>
        public static bool IsUnlocked => _key != null;

        /// <summary>
        /// Returns the frames and timing for the requested animation.
        /// </summary>
        public static PixelSpriteAnimation Get(PixelAvatarAnimation animation)
        {
            // AutoIdle is a behaviour rather than artwork: it drifts between the resting
            // poses, and starts from Idle.
            if (animation == PixelAvatarAnimation.AutoIdle) animation = PixelAvatarAnimation.Idle;
            return Animations()[animation];
        }

        /// <summary>
        /// Returns which of the three shading levels a palette index belongs to. The indices are
        /// ordered so that each shade is a contiguous run, which is why the single-hue designs
        /// (Black, Orange, White, Beige) are just three colors repeated. Only meaningful for
        /// indices 1..<see cref="PaletteSize"/>.
        /// </summary>
        public static PixelAvatarShade ShadeOf(byte paletteIndex)
        {
            if (paletteIndex <= LastHighlightIndex) return PixelAvatarShade.Highlight;
            if (paletteIndex <= LastBaseIndex) return PixelAvatarShade.Base;
            return PixelAvatarShade.Shadow;
        }

        /// <summary>
        /// Returns every built-in animation, in declaration order.
        /// </summary>
        public static PixelAvatarAnimation[] All => new[]
        {
            PixelAvatarAnimation.Move,
            PixelAvatarAnimation.Idle,
            PixelAvatarAnimation.Interact,
            PixelAvatarAnimation.JumpUp,
            PixelAvatarAnimation.JumpDown,
            PixelAvatarAnimation.Startle,
            PixelAvatarAnimation.Stretch,
            PixelAvatarAnimation.Sit,
            PixelAvatarAnimation.SitIdle,
            PixelAvatarAnimation.Crouch,
            PixelAvatarAnimation.CrouchIdle,
            PixelAvatarAnimation.Sleep,
            PixelAvatarAnimation.SleepIdle,
        };

        // Unpacked on first use rather than in a static initializer, because the key only
        // arrives once the application has had a chance to run.
        private static Dictionary<PixelAvatarAnimation, PixelSpriteAnimation> Animations()
        {
            if (_animations != null) return _animations;

            if (_key == null)
            {
                throw new InvalidOperationException(
                    "The PixelAvatar artwork is packed and no key has been supplied. Call PixelAvatarSprites.Unlock(key) at startup, before creating a PixelAvatar.");
            }

            _animations = Build(PackedText.Unpack(PackedFrames, _key));
            return _animations;
        }

        private static Dictionary<PixelAvatarAnimation, PixelSpriteAnimation> Build(string frames)
        {
            var animations = new Dictionary<PixelAvatarAnimation, PixelSpriteAnimation>();
            var offset     = 0;

            // Frame counts are part of the table rather than the payload, so the two have to
            // agree; the check at the end is what catches a payload regenerated from a frame
            // file this table has not caught up with.
            offset = Add(animations, frames, offset, PixelAvatarAnimation.Move,       4,  80, true,  PixelAvatarAnimation.Move,       0,    0);
            offset = Add(animations, frames, offset, PixelAvatarAnimation.Idle,       3, 260, true,  PixelAvatarAnimation.Idle,       5000, 10000);
            offset = Add(animations, frames, offset, PixelAvatarAnimation.Interact,   2, 140, false, PixelAvatarAnimation.Idle,       0,    0);
            offset = Add(animations, frames, offset, PixelAvatarAnimation.JumpUp,     2, 110, false, PixelAvatarAnimation.JumpDown,   0,    0);
            offset = Add(animations, frames, offset, PixelAvatarAnimation.JumpDown,   2, 110, false, PixelAvatarAnimation.Idle,       0,    0);
            offset = Add(animations, frames, offset, PixelAvatarAnimation.Startle,    4, 100, false, PixelAvatarAnimation.Idle,       0,    0);
            offset = Add(animations, frames, offset, PixelAvatarAnimation.Stretch,    6, 120, false, PixelAvatarAnimation.Sit,        0,    0);
            offset = Add(animations, frames, offset, PixelAvatarAnimation.Sit,        2, 130, false, PixelAvatarAnimation.SitIdle,    0,    0);
            offset = Add(animations, frames, offset, PixelAvatarAnimation.SitIdle,    6, 300, true,  PixelAvatarAnimation.SitIdle,    5000, 10000);
            offset = Add(animations, frames, offset, PixelAvatarAnimation.Crouch,     2, 130, false, PixelAvatarAnimation.CrouchIdle, 0,    0);
            offset = Add(animations, frames, offset, PixelAvatarAnimation.CrouchIdle, 6, 300, true,  PixelAvatarAnimation.CrouchIdle, 5000, 10000);
            offset = Add(animations, frames, offset, PixelAvatarAnimation.Sleep,      3, 200, false, PixelAvatarAnimation.SleepIdle,  0,    0);
            offset = Add(animations, frames, offset, PixelAvatarAnimation.SleepIdle,  1, 450, true,  PixelAvatarAnimation.SleepIdle,  0,    0);

            if (offset != frames.Length)
            {
                throw new InvalidOperationException($"The packed artwork holds {frames.Length} characters but the animation table accounts for {offset}.");
            }

            return animations;
        }

        private static int Add(
            Dictionary<PixelAvatarAnimation, PixelSpriteAnimation> animations,
            string                                                 packed,
            int                                                    offset,
            PixelAvatarAnimation                                   animation,
            int                                                    frameCount,
            int                                                    frameDurationMs,
            bool                                                   loops,
            PixelAvatarAnimation                                   next,
            int                                                    restMinMs,
            int                                                    restMaxMs)
        {
            var frames = new PixelSprite[frameCount];

            for (var i = 0; i < frameCount; i++)
            {
                frames[i] = Decode(packed, offset);
                offset   += FrameWidth * FrameHeight;
            }

            animations[animation] = new PixelSpriteAnimation(animation, frames, frameDurationMs, loops, next, restMinMs, restMaxMs);
            return offset;
        }

        private static PixelSprite Decode(string packed, int offset)
        {
            var pixels = new byte[FrameWidth * FrameHeight];

            for (var i = 0; i < pixels.Length; i++)
            {
                var index = Alphabet.IndexOf(packed[offset + i]);
                pixels[i] = (byte)(index < 0 ? 0 : index);
            }

            return new PixelSprite(FrameWidth, FrameHeight, pixels);
        }
    }
}
