// Sprite data for the PixelAvatar component, from the source cat sprite sheets. Every sheet
// shares the same silhouette, so the artwork is stored once as a grid of palette indices and
// each design is only a palette.

using System;
using System.Collections.Generic;

namespace Tesserae
{
    /// <summary>
    /// The built-in sprite data shared by every <see cref="PixelAvatar"/>: one frame grid
    /// per animation, where each cell is an index into a <see cref="PixelAvatarPalette"/>
    /// (0 meaning transparent).
    /// </summary>
    [Transpose.Name("tss.pavs")]
    public static class PixelAvatarSprites
    {
        /// <summary>Width, in pixels, of every sprite frame.</summary>
        internal const int FrameWidth = 10;
        /// <summary>Height, in pixels, of every sprite frame.</summary>
        internal const int FrameHeight = 8;
        /// <summary>Number of colors in a palette (palette index 0 is always transparent).</summary>
        public const int PaletteSize = 11;
        /// <summary>Highest palette index belonging to the highlight shade.</summary>
        internal const int LastHighlightIndex = 3;
        /// <summary>Highest palette index belonging to the base shade; the rest are shadow.</summary>
        internal const int LastBaseIndex = 9;

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
        internal static readonly int[] PixelCounts = new[] { 0, 128, 89, 43, 133, 133, 66, 43, 43, 2, 73, 38 };

        private const string Alphabet = ".123456789ab";

        private const string PackedFrames =
            "aaaaaaarqpynms6akvtva69aqigvgaaaaaOrqpams6mnmovtvOks9a6qvgaigaaaaaaaqnyrqpOkqs6O6ovtvyigqsjhaOvgaaaa" +
            "aaaayrmrqpaks6a6vtvaigvgaaaaaaayrqpynms6akvtva69aqigvgaaaaaaaaOnmrqpaks6a6vtvaigvgaaaaaaaaayrqpynms6" +
            "akvtvaigvgaaaaaaaOrqpynms6akvtva698yigqvaaaaaaaOrqpynms6akvtva69aigqvqgaaaaaaaaaqrqpynks6a6vtvaigvga" +
            "aaaaOrqpams6mnmovtvOks9a6qvgaigaaaaaaaqnyrqpOkqs6O6ovtvyigqsjhaOvgaaaaaaaamnmrqpaks6a6vtvaigvgaaaaaa" +
            "aaayrqpynks6a6vtvaigvgaqrqrqparqs6asovtva69aiOvaaaaaaaaamrqpOrms6Ormvtvak9aq64aqimvaaaaaaaaaaayrqpyn" +
            "ms6akvtvaigvgaaaaaaamrqpynms6akvtva69aqigqvgaaaaaayraOrmrqpaks6a6vtvaqigqvgaaaayraOraakrqpau46aquvtv" +
            "aqigqvgaaaayraOraakrqpau46aquvtvaqigqvgaaaaaayraOrmrqpaks6a6vtvaqigqvgaaaaaaamrqpynms6akvtva69aqigqv" +
            "gaaaaaaayrqpams6ynqovtvaou9aqigvgaaaaamrqpams6amvtvaqo9arqo4aqrig7aaaaamrqpams6amvtvaqo9arqo4aqrig7a" +
            "aaaamrqpams6amvtvaqo9arqo4aqrig7aaaaamrqpams6amvtvaqo9arqo4aqrig7aaaaamrqpams6amvtvaqo9arqo4aqrig7aa" +
            "aaamrqpams6amvtvaqo9ano4amig7aaaaamrqpams6amvtvOrqo9aqro4amig7aaaaaaaaOnmrqpaks6a6vtvaigvgaaaaaaaaay" +
            "rqpynms6akvtvaigvgaaaaaaaaayrqpynms6akvtvaigvgaaaaaaaaayrqpynms6akvtvaigvgaaaaaaaaayrqpynms6akvtvaig" +
            "vgaaaaaaaaayrqpynms6akvtvaigvgaaaaaaaaayrqpyrys6Orkvtvaigvgaaaaaaaaayrqpams6ynkvtvaigvgaaaaaaaaOryrq" +
            "pOrks6a6vtvaigvgaaaaaaaaaaOryrqpOrks6aigvtvaaaaaaaaaaamrqpaks6ynigvtvaaaaaaaaaaamrqpaks6ynigvtvm";

        private static Dictionary<PixelAvatarAnimation, PixelSpriteAnimation> _animations;

        // Called by PixelAvatar's constructor with the key the application handed it. The first
        // avatar decodes the sheet; the rest find it already there.
        internal static void Unlock(byte key)
        {
            if (_animations == null) _animations = Build(PackedText.Unpack(PackedFrames, Alphabet, key));
        }

        /// <summary>
        /// Returns the frames and timing for the requested animation. Only available once a
        /// <see cref="PixelAvatar"/> has been constructed, since that is what supplies the key
        /// the artwork is scrambled with.
        /// </summary>
        public static PixelSpriteAnimation Get(PixelAvatarAnimation animation)
        {
            if (_animations == null)
            {
                throw new InvalidOperationException("The PixelAvatar artwork has not been decoded yet - construct a PixelAvatar, which takes the key, before asking for its sprites.");
            }

            // AutoIdle is a behaviour rather than artwork: it drifts between the resting
            // poses, and starts from Idle.
            if (animation == PixelAvatarAnimation.AutoIdle) animation = PixelAvatarAnimation.Idle;
            return _animations[animation];
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

        private static int Add(Dictionary<PixelAvatarAnimation, PixelSpriteAnimation> animations, string packed, int offset, PixelAvatarAnimation animation, int frameCount, int frameDurationMs, bool loops, PixelAvatarAnimation next, int restMinMs, int restMaxMs)
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
