using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tesserae;

namespace Build.PackPixelSprites
{
    /// <summary>
    /// Packs the PixelAvatar sprite artwork into the single obfuscated literal that ships inside
    /// Tesserae, using the very same <see cref="PackedText"/> the browser unpacks it with.
    ///
    /// The key is never stored here or anywhere in the library - it is passed on the command line
    /// and lives, in this repository, only in the samples project that unlocks the sprites at
    /// startup.
    ///
    ///   dotnet run --project Build.PackPixelSprites --
    ///       --frames Build.PackPixelSprites/sprite-frames.txt
    ///       --key    the-key-from-the-samples-project
    ///       --out    Tesserae/src/Components/PixelAvatar.Sprites.cs
    ///
    /// Without --out it just prints the literal and the statistics.
    /// </summary>
    internal static class Program
    {
        private const int    FrameWidth  = 10;
        private const int    FrameHeight = 8;
        private const string Alphabet    = ".123456789ab";

        // Where the generated literal goes in the target file. The marker comments are what make
        // re-running this tool idempotent.
        private const string BeginMarker = "// <packed-frames>";
        private const string EndMarker   = "// </packed-frames>";

        private static int Main(string[] args)
        {
            var options = ParseArguments(args);

            if (!options.TryGetValue("frames", out var framesPath) || !options.TryGetValue("key", out var key))
            {
                Console.Error.WriteLine("usage: --frames <path> --key <key> [--out <PixelAvatar.Sprites.cs>] [--width N] [--height N]");
                return 1;
            }

            var width  = options.TryGetValue("width",  out var w) ? int.Parse(w) : FrameWidth;
            var height = options.TryGetValue("height", out var h) ? int.Parse(h) : FrameHeight;

            List<string> frames;
            List<string> animations;

            try
            {
                frames = ReadFrames(framesPath, width * height, out animations);
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(error.Message);
                return 1;
            }

            // Every frame is exactly width*height characters, so they concatenate without a
            // separator and the unpacker slices them back apart by length.
            var plain  = string.Concat(frames);
            var packed = PackedText.Pack(plain, key);

            // Never emit a literal without proving it comes back: a silently corrupt blob would
            // only surface as a blank cat in someone else's browser.
            if (PackedText.Unpack(packed, key) != plain)
            {
                Console.Error.WriteLine("round-trip failed: the packed literal does not unpack to the input.");
                return 1;
            }

            if (WrongKeyIsAccepted(packed, key))
            {
                Console.Error.WriteLine("round-trip failed: a wrong key was accepted instead of rejected.");
                return 1;
            }

            Console.Error.WriteLine($"{animations.Count} animations, {frames.Count} frames, {width}x{height}");
            Console.Error.WriteLine($"plain {plain.Length} chars -> packed {packed.Length} chars ({100.0 * packed.Length / plain.Length:0.0}%)");
            Console.Error.WriteLine($"frame counts: {string.Join(", ", animations)}");

            if (!options.TryGetValue("out", out var outPath))
            {
                Console.WriteLine(packed);
                return 0;
            }

            if (!Rewrite(outPath, packed))
            {
                Console.Error.WriteLine($"{outPath} has no {BeginMarker} / {EndMarker} block to replace.");
                return 1;
            }

            Console.Error.WriteLine($"wrote {outPath}");
            return 0;
        }

        // Feeds it a key that is deliberately not the right one and checks the checksum notices.
        private static bool WrongKeyIsAccepted(string packed, string key)
        {
            try
            {
                PackedText.Unpack(packed, key + "!");
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Reads the frame file: one frame per line, `# Name` starting a new animation, blank
        /// lines ignored. Frames are returned flattened in file order, which is the order the
        /// animation table in PixelAvatar.Sprites.cs slices them back out in.
        /// </summary>
        private static List<string> ReadFrames(string path, int frameLength, out List<string> animations)
        {
            var frames  = new List<string>();
            var counts  = new List<string>();
            var current = 0;
            var name    = (string)null;
            var line    = 0;

            foreach (var raw in File.ReadAllLines(path))
            {
                line++;
                var text = raw.Trim();

                if (text.Length == 0) continue;

                if (text.StartsWith("#"))
                {
                    if (name != null) counts.Add($"{name}={current}");
                    name    = text.Substring(1).Trim();
                    current = 0;
                    continue;
                }

                if (text.Length != frameLength)
                {
                    throw new FormatException($"{path}:{line}: expected {frameLength} characters, found {text.Length}.");
                }

                var bad = text.FirstOrDefault(c => !Alphabet.Contains(c));
                if (bad != '\0')
                {
                    throw new FormatException($"{path}:{line}: '{bad}' is not one of \"{Alphabet}\".");
                }

                frames.Add(text);
                current++;
            }

            if (name != null) counts.Add($"{name}={current}");
            if (frames.Count == 0) throw new FormatException($"{path}: no frames found.");

            animations = counts;
            return frames;
        }

        /// <summary>
        /// Replaces whatever sits between the marker comments with the packed literal, wrapped
        /// across several source lines so the generated file stays readable in a diff.
        /// </summary>
        private static bool Rewrite(string path, string packed)
        {
            var source = File.ReadAllText(path);
            var begin  = source.IndexOf(BeginMarker, StringComparison.Ordinal);
            var end    = source.IndexOf(EndMarker, StringComparison.Ordinal);

            if (begin < 0 || end < 0 || end < begin) return false;

            // Reuse the indentation the begin marker already sits at.
            var lineStart = source.LastIndexOf('\n', begin) + 1;
            var indent    = source.Substring(lineStart, begin - lineStart);

            var builder = new StringBuilder();
            builder.Append(BeginMarker).Append('\n');
            builder.Append(indent).Append("private const string PackedFrames =");

            const int chunk = 100;
            for (var i = 0; i < packed.Length; i += chunk)
            {
                var piece = packed.Substring(i, Math.Min(chunk, packed.Length - i));
                var last  = i + chunk >= packed.Length;
                builder.Append('\n').Append(indent).Append("    \"").Append(piece).Append(last ? "\";" : "\" +");
            }

            builder.Append('\n').Append(indent).Append(EndMarker);

            var replaced = source.Substring(0, begin) + builder + source.Substring(end + EndMarker.Length);
            File.WriteAllText(path, replaced);
            return true;
        }

        private static Dictionary<string, string> ParseArguments(string[] args)
        {
            var options = new Dictionary<string, string>();

            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i].StartsWith("--")) options[args[i].Substring(2)] = args[i + 1];
            }

            return options;
        }
    }
}
