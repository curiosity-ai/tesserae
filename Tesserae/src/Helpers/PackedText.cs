using System;
using System.Collections.Generic;
using System.Text;

namespace Tesserae
{
    /// <summary>
    /// Squeezes a string into a shorter, scrambled, ASCII-safe one and back again. Built for
    /// bulky repetitive literals that would otherwise sit in source in plain sight - the
    /// <see cref="PixelAvatarSprites"/> artwork is the one the toolkit itself uses.
    ///
    /// <see cref="Pack"/> runs the text through UTF-8, an LZSS compressor, a repeating-key XOR
    /// and Base64; <see cref="Unpack"/> undoes all four. A checksum of the original text rides
    /// along in the header, so an unpack with the wrong key fails loudly instead of quietly
    /// handing back rubbish.
    ///
    /// The XOR is <b>obfuscation, not security</b>. A repeating key over a known-plaintext
    /// header is broken by anyone who cares to, and a key that has to reach the browser is
    /// readable there by definition. Use it to keep bulk data out of casual sight, never to
    /// protect anything that matters.
    /// </summary>
    public static class PackedText
    {
        // Bumped only if the byte layout below changes; Unpack refuses anything it does not know.
        private const byte FormatVersion = 1;

        // 12 bits of offset and 4 of length per match, which is what makes a match two bytes.
        private const int WindowSize = 4096;
        private const int MinMatch   = 3;
        private const int MaxMatch   = MinMatch + 15;

        // version + length + checksum, all before the compressed stream.
        private const int HeaderSize = 1 + 4 + 4;

        private const string Base64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        /// <summary>
        /// Compresses, scrambles with <paramref name="key"/> and Base64-encodes
        /// <paramref name="text"/>. The result is safe to paste into a source file as a plain
        /// string literal.
        ///
        /// Packing is a build-time operation - the match search is a straightforward scan of the
        /// window, which is fine for the literals this is meant for but is not something to run
        /// on megabytes in a render loop. Unpacking is linear and cheap.
        /// </summary>
        public static string Pack(string text, string key)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var plain      = ToUtf8(text);
            var compressed = Compress(plain);
            var payload    = new byte[HeaderSize + compressed.Length];

            payload[0] = FormatVersion;
            WriteInt32(payload, 1, plain.Length);
            WriteInt32(payload, 5, (int)Checksum(plain));

            for (var i = 0; i < compressed.Length; i++) payload[HeaderSize + i] = compressed[i];

            Scramble(payload, key);
            return ToBase64(payload);
        }

        /// <summary>
        /// Reverses <see cref="Pack"/>. Throws <see cref="FormatException"/> when
        /// <paramref name="key"/> is not the one the text was packed with, or when the packed
        /// string has been damaged - the two are indistinguishable, and both mean the same thing
        /// to a caller.
        /// </summary>
        public static string Unpack(string packed, string key)
        {
            if (packed == null) throw new ArgumentNullException(nameof(packed));

            var payload = FromBase64(packed);
            Scramble(payload, key);

            if (payload.Length < HeaderSize || payload[0] != FormatVersion) throw WrongKey();

            var length   = ReadInt32(payload, 1);
            var expected = ReadInt32(payload, 5);

            // A wrong key turns the length into an arbitrary number, so sanity-check it before
            // trying to allocate it.
            if (length < 0 || length > (payload.Length - HeaderSize) * MaxMatch + MaxMatch) throw WrongKey();

            var plain = Decompress(payload, HeaderSize, length);

            if ((int)Checksum(plain) != expected) throw WrongKey();

            return FromUtf8(plain);
        }

        private static FormatException WrongKey()
        {
            return new FormatException("The packed text could not be unpacked: wrong key, or the text has been damaged.");
        }

        // ---- LZSS ---------------------------------------------------------------------------
        //
        // The stream is groups of eight items led by a flag byte, one bit per item, least
        // significant first: a set bit is a literal byte, a clear one a two-byte back-reference
        // holding a 12-bit distance and a 4-bit length. Nothing is entropy-coded afterwards,
        // which costs some ratio and buys a decoder that is twenty lines long.

        private static byte[] Compress(byte[] input)
        {
            var output = new List<byte>();
            var position = 0;

            while (position < input.Length)
            {
                // Reserve the flag byte; its bits are only known once the group is filled in.
                var flagIndex = output.Count;
                output.Add(0);
                var flags = 0;

                for (var bit = 0; bit < 8 && position < input.Length; bit++)
                {
                    var maxLength = input.Length - position;
                    if (maxLength > MaxMatch) maxLength = MaxMatch;

                    var bestLength = 0;
                    var bestOffset = 0;

                    if (maxLength >= MinMatch)
                    {
                        var oldest = position - WindowSize;
                        if (oldest < 0) oldest = 0;

                        for (var candidate = position - 1; candidate >= oldest; candidate--)
                        {
                            var length = 0;
                            while (length < maxLength && input[candidate + length] == input[position + length]) length++;

                            if (length > bestLength)
                            {
                                bestLength = length;
                                bestOffset = position - candidate;
                                if (length == maxLength) break;
                            }
                        }
                    }

                    if (bestLength >= MinMatch)
                    {
                        var distance = bestOffset - 1;
                        var run      = bestLength - MinMatch;

                        output.Add((byte)(distance >> 4));
                        output.Add((byte)(((distance & 0x0F) << 4) | run));
                        position += bestLength;
                    }
                    else
                    {
                        flags |= 1 << bit;
                        output.Add(input[position]);
                        position++;
                    }
                }

                output[flagIndex] = (byte)flags;
            }

            return output.ToArray();
        }

        private static byte[] Decompress(byte[] input, int start, int length)
        {
            var output   = new byte[length];
            var produced = 0;
            var position = start;
            var flags    = 0;
            var bit      = 8;

            while (produced < length)
            {
                if (bit == 8)
                {
                    if (position >= input.Length) throw WrongKey();
                    flags = input[position];
                    position++;
                    bit = 0;
                }

                if ((flags & (1 << bit)) != 0)
                {
                    if (position >= input.Length) throw WrongKey();
                    output[produced] = input[position];
                    produced++;
                    position++;
                }
                else
                {
                    if (position + 1 >= input.Length) throw WrongKey();

                    var high = input[position];
                    var low  = input[position + 1];
                    position += 2;

                    var distance = ((high << 4) | (low >> 4)) + 1;
                    var run      = (low & 0x0F) + MinMatch;
                    var from     = produced - distance;

                    if (from < 0 || produced + run > length) throw WrongKey();

                    // Copied one byte at a time on purpose: a run may overlap what it is still
                    // writing, which is how LZSS encodes a repeat of a short pattern.
                    for (var i = 0; i < run; i++)
                    {
                        output[produced] = output[from + i];
                        produced++;
                    }
                }

                bit++;
            }

            return output;
        }

        // ---- Scrambling ---------------------------------------------------------------------

        // Its own inverse, so Pack and Unpack call the same thing.
        private static void Scramble(byte[] data, string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            var keyBytes = ToUtf8(key);
            if (keyBytes.Length == 0) return;

            for (var i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
            }
        }

        // FNV-1a. Not a cryptographic hash - it is here to tell "the key was wrong" from "the
        // key was right", which it does perfectly well.
        private static uint Checksum(byte[] data)
        {
            var hash = 2166136261u;

            for (var i = 0; i < data.Length; i++)
            {
                hash = (hash ^ data[i]) * 16777619u;
            }

            return hash;
        }

        private static void WriteInt32(byte[] target, int offset, int value)
        {
            target[offset]     = (byte)((value >> 24) & 0xFF);
            target[offset + 1] = (byte)((value >> 16) & 0xFF);
            target[offset + 2] = (byte)((value >> 8) & 0xFF);
            target[offset + 3] = (byte)(value & 0xFF);
        }

        private static int ReadInt32(byte[] source, int offset)
        {
            return (source[offset] << 24) | (source[offset + 1] << 16) | (source[offset + 2] << 8) | source[offset + 3];
        }

        // ---- Base64 -------------------------------------------------------------------------
        //
        // Hand-rolled rather than Convert.ToBase64String so the same source compiles unchanged
        // both here and in the build-time CLI that generates the packed literals.

        private static string ToBase64(byte[] data)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < data.Length; i += 3)
            {
                var remaining = data.Length - i;
                var chunk     = data[i] << 16;

                if (remaining > 1) chunk |= data[i + 1] << 8;
                if (remaining > 2) chunk |= data[i + 2];

                builder.Append(Base64Alphabet[(chunk >> 18) & 0x3F]);
                builder.Append(Base64Alphabet[(chunk >> 12) & 0x3F]);
                builder.Append(remaining > 1 ? Base64Alphabet[(chunk >> 6) & 0x3F] : '=');
                builder.Append(remaining > 2 ? Base64Alphabet[chunk & 0x3F] : '=');
            }

            return builder.ToString();
        }

        private static byte[] FromBase64(string text)
        {
            var bytes = new List<byte>();
            var chunk = 0;
            var bits  = 0;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '=') break;

                var value = Base64Alphabet.IndexOf(c);
                if (value < 0) continue;   // tolerate line breaks and stray whitespace

                chunk = (chunk << 6) | value;
                bits += 6;

                if (bits >= 8)
                {
                    bits -= 8;
                    bytes.Add((byte)((chunk >> bits) & 0xFF));
                }
            }

            return bytes.ToArray();
        }

        // ---- UTF-8 --------------------------------------------------------------------------
        //
        // Also hand-rolled, for the same reason as Base64, and because compressing UTF-16 code
        // units would waste a zero byte on every character of the ASCII this mostly sees.

        private static byte[] ToUtf8(string text)
        {
            var bytes = new List<byte>();

            for (var i = 0; i < text.Length; i++)
            {
                int point = text[i];

                // Recombine a surrogate pair into the code point it stands for.
                if (point >= 0xD800 && point <= 0xDBFF && i + 1 < text.Length)
                {
                    int low = text[i + 1];

                    if (low >= 0xDC00 && low <= 0xDFFF)
                    {
                        point = 0x10000 + ((point - 0xD800) << 10) + (low - 0xDC00);
                        i++;
                    }
                }

                if (point < 0x80)
                {
                    bytes.Add((byte)point);
                }
                else if (point < 0x800)
                {
                    bytes.Add((byte)(0xC0 | (point >> 6)));
                    bytes.Add((byte)(0x80 | (point & 0x3F)));
                }
                else if (point < 0x10000)
                {
                    bytes.Add((byte)(0xE0 | (point >> 12)));
                    bytes.Add((byte)(0x80 | ((point >> 6) & 0x3F)));
                    bytes.Add((byte)(0x80 | (point & 0x3F)));
                }
                else
                {
                    bytes.Add((byte)(0xF0 | (point >> 18)));
                    bytes.Add((byte)(0x80 | ((point >> 12) & 0x3F)));
                    bytes.Add((byte)(0x80 | ((point >> 6) & 0x3F)));
                    bytes.Add((byte)(0x80 | (point & 0x3F)));
                }
            }

            return bytes.ToArray();
        }

        private static string FromUtf8(byte[] bytes)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < bytes.Length; i++)
            {
                int first = bytes[i];
                int point;
                int extra;

                if (first < 0x80)      { point = first;        extra = 0; }
                else if (first < 0xE0) { point = first & 0x1F; extra = 1; }
                else if (first < 0xF0) { point = first & 0x0F; extra = 2; }
                else                   { point = first & 0x07; extra = 3; }

                if (i + extra >= bytes.Length) throw WrongKey();

                for (var k = 0; k < extra; k++)
                {
                    i++;
                    point = (point << 6) | (bytes[i] & 0x3F);
                }

                if (point < 0x10000)
                {
                    builder.Append((char)point);
                }
                else
                {
                    // Back into a surrogate pair for the UTF-16 world a C# string lives in.
                    point -= 0x10000;
                    builder.Append((char)(0xD800 + (point >> 10)));
                    builder.Append((char)(0xDC00 + (point & 0x3FF)));
                }
            }

            return builder.ToString();
        }
    }
}
