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
        /// <summary>
        /// How many indices above <see cref="PaletteSize"/> the artwork keeps for props - the
        /// things the cat handles rather than the cat itself, such as the laptop in
        /// <see cref="PixelAvatarAnimation.Work"/>. A prop is not part of a coat, so it is not in
        /// a <see cref="PixelAvatarPalette"/>: those indices are painted from the avatar's own CSS
        /// variables instead, which is what keeps one laptop grey across all fifteen designs.
        /// </summary>
        public const int PropSize = 3;
        /// <summary>The highest index a sprite cell can carry, props included.</summary>
        public const int HighestIndex = PaletteSize + PropSize;
        /// <summary>The index of a prop's body - the laptop's shell and keyboard.</summary>
        public const byte PropBodyIndex = PaletteSize + 1;
        /// <summary>The index of a prop's lit face - the laptop's screen.</summary>
        public const byte PropLitIndex = PaletteSize + 2;
        /// <summary>The index of a prop's shaded face - the lid while the laptop is closed.</summary>
        public const byte PropShadowIndex = PaletteSize + 3;
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
        /// How many pixels across the whole sprite sheet use each index, indexed by palette index
        /// (entry 0 is the transparent index and is always 0). Lets callers work out which color
        /// covers most of the artwork - see <see cref="PixelAvatarPalette.DominantColor"/>, which
        /// reads only the coat's own 1..<see cref="PaletteSize"/>; the prop entries above those
        /// are here for completeness.
        /// </summary>
        internal static readonly int[] PixelCounts = new[] { 0, 157, 109, 53, 154, 190, 94, 53, 53, 2, 84, 58, 57, 13, 6 };

        // Twelve coat indices (0 = transparent, 1..11 = the palette) followed by the three prop
        // indices. Fifteen symbols caps a run-length at four rather than five - see PackedText -
        // which is what the props cost the packed sheet.
        private const string Alphabet = ".123456789abcde";

        private const string PackedFrames =
            "HHHHHHHH0rqp06ls5Hq7vtvHq5wHligvgHHHHHHHqrqpH0s5l6lovtvH7swHq5qvgHqigHHHHHHHHH60rqpH7qs5H5ovtv0igqsj" +
            "hHHqvgHHHHHHHHHH0rlrqpHq7s5Hq5vtvHqigvgHHHHHHHHHlrqp06ls5Hq7vtvHq5wHligvgHHHHHHHHHHH6lrqpHq7s5Hq5vtv" +
            "HqigvgHHHHHHHHHHHHrqp06ls5Hq7vtvHqigvgHHHHHHHHH0rqp06ls5Hq7vtvHq5wz0igqvHHHHHHHHH0rqp06ls5Hq7vtvHq5w" +
            "HqigqvqgHHHHHHHHHHHlrqp067s5Hq5vtvHqigvgHHHHHHHqrqpH0s5l6lovtvH7swHq5qvgHqigHHHHHHHHH60rqpH7qs5H5ovt" +
            "v0igqsjhHHqvgHHHHHHHHHHl6lrqpHq7s5Hq5vtvHqigvgHHHHHHHHHHHHrqp067s5Hq5vtvHqigvgHlrqrqpHqrqs5HqsovtvHq" +
            "5wHqiHvHHHHHHHHHHH0rqpHrls5HrlvtvHq7wHl5/HlilvHHHHHHHHHHHHHHlrqp06ls5Hq7vtvHqigvgHHHHHHHHHqrqp06ls5H" +
            "q7vtvHq5wHligqvgHHHHHHHHqrHHqrlrqpHq7s5Hq5vtvHligqvgHHHHH0rHHqrHHl7rqpHqu/5HluvtvHligqvgHHHHH0rHHqrH" +
            "Hl7rqpHqu/5HluvtvHligqvgHHHHHHHHqrHHqrlrqpHq7s5Hq5vtvHligqvgHHHHHHHHHqrqp06ls5Hq7vtvHq5wHligqvgHHHHH" +
            "HHHHlrqpH0s506qovtvHqouwHligvgHHHHHH0rqpH0s5H0vtvHlowHqrqo/Hlrig+HHHHHH0rqpH0s5H0vtvHlowHqrqo/Hlrig+" +
            "HHHHHH0rqpH0s5H0vtvHlowHqrqo/Hlrig+HHHHHH0rqpH0s5H0vtvHlowHqrqo/Hlrig+HHHHHH0rqpH0s5H0vtvHlowHqrqo/H" +
            "lrig+HHHHHH0rqpH0s5H0vtvHlowHq6o/H0ig+HHHHHH0rqpH0s5H0vtvHrqowHlro/H0ig+HHHHHHHHHHH6lrqpHq7s5Hq5vtvH" +
            "qigvgHHHHHHHHHHHHrqp06ls5Hq7vtvHqigvgHHHHHHHHHHHHrqp06ls5Hq7vtvHqigvgHHHHHHHHHHHHrqp06ls5Hq7vtvHqigv" +
            "gHHHHHHHHHHHHrqp06ls5Hq7vtvHqigvgHHHHHHHHHHHHrqp06ls5Hq7vtvHqigvgHHHHHHHHHHHHrqp0r0s5Hr7vtvHqigvgHHH" +
            "HHHHHHHHHrqpH0s5067vtvHqigvgHHHHHHHHHHHr0rqpHr7s5Hq5vtvHqigvgHHHHHHHHHHHHHlr0rqpHr7s5HqigvtvHHHHHHHH" +
            "HHHHHHqrqpHq7s506igvtvHHHHHHHHHHHHHHqrqpHq7s506igvtvHHHHHHHHHrqpH0s506qovtvHqouwHligvgRHHHHH0rqpH0s5" +
            "H0vtvHlowHqrqo/+lmqrig+THHHHH0rqpH0s5H0vtvHlowHmrqo/+qnmqrig+THHHHH0rqpH0s5H0vtv0mlow0nmrqo/+qnmqrig" +
            "+THHHHH0rqpH0s5H0vtv0mlow0nmrqo/+qnmqrig+THHHHH0rqpH0s5H0vtv0mlow0nmrqo/vlnmqrigJAHHHHH0rqpH0s5H0vtv" +
            "0mlow0kmrqo/+qnmqrig+THHHHH0rqpH0s5H0vtv0mlow0kmrqo/vlnmqrigJAHHHHH0rqpH0s5H0vtv0mlow0nm6o/+qnmlig+T" +
            "HHHHH0rqpH0s5H0vtv0mlow0nmqro/vlnmligJA";

        private static Dictionary<PixelAvatarAnimation, PixelSpriteAnimation> _animations;

        // Called by PixelAvatar's constructor with the key the application handed it. The first
        // avatar decodes the sheet; the rest find it already there.
        internal static void Load(byte key)
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
            PixelAvatarAnimation.Work,
            PixelAvatarAnimation.WorkIdle,
        };

        private static Dictionary<PixelAvatarAnimation, PixelSpriteAnimation> Build(string frames)
        {
            var animations = new Dictionary<PixelAvatarAnimation, PixelSpriteAnimation>();
            var offset     = 0;

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
            offset = Add(animations, frames, offset, PixelAvatarAnimation.Work,       4, 150, false, PixelAvatarAnimation.WorkIdle,   0,    0);
            offset = Add(animations, frames, offset, PixelAvatarAnimation.WorkIdle,   6, 130, true,  PixelAvatarAnimation.WorkIdle,   1200, 3200);

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
