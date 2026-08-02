using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace Build.UpdateInterfaceIcons
{
    /// <summary>
    /// Codepoint to glyph id, read out of a font's <c>cmap</c>. Only the two subtable formats these fonts
    /// use are handled - segmented coverage for the basic plane, and its 32 bit equivalent - and the widest
    /// subtable wins, so the lookup agrees with what the browser resolves a character to.
    /// </summary>
    internal static class CmapLookup
    {
        public static Dictionary<int, int> Read(byte[] cmap)
        {
            var numTables = BinaryPrimitives.ReadUInt16BigEndian(cmap.AsSpan(2));
            var best      = new Dictionary<int, int>();

            for (int i = 0; i < numTables; i++)
            {
                var offset  = (int)BinaryPrimitives.ReadUInt32BigEndian(cmap.AsSpan(4 + i * 8 + 4));
                var format  = BinaryPrimitives.ReadUInt16BigEndian(cmap.AsSpan(offset));
                var mapping = format switch
                {
                    4  => ReadFormat4(cmap, offset),
                    12 => ReadFormat12(cmap, offset),
                    _  => null,
                };

                if (mapping != null && mapping.Count > best.Count) best = mapping;
            }

            if (best.Count == 0) throw new InvalidOperationException("no usable cmap subtable (expected format 4 or 12)");

            return best;
        }

        private static Dictionary<int, int> ReadFormat4(byte[] cmap, int offset)
        {
            var segCountX2 = BinaryPrimitives.ReadUInt16BigEndian(cmap.AsSpan(offset + 6));
            var segCount   = segCountX2 / 2;
            var ends       = offset + 14;
            var starts     = ends + segCountX2 + 2;
            var deltas     = starts + segCountX2;
            var ranges     = deltas + segCountX2;
            var mapping    = new Dictionary<int, int>();

            for (int s = 0; s < segCount; s++)
            {
                var end   = BinaryPrimitives.ReadUInt16BigEndian(cmap.AsSpan(ends + s * 2));
                var start = BinaryPrimitives.ReadUInt16BigEndian(cmap.AsSpan(starts + s * 2));
                var delta = BinaryPrimitives.ReadInt16BigEndian(cmap.AsSpan(deltas + s * 2));
                var range = BinaryPrimitives.ReadUInt16BigEndian(cmap.AsSpan(ranges + s * 2));

                if (start == 0xffff) continue;

                for (int c = start; c <= end && c != 0x10000; c++)
                {
                    int glyph;

                    if (range == 0)
                    {
                        glyph = (c + delta) & 0xffff;
                    }
                    else
                    {
                        var at = ranges + s * 2 + range + (c - start) * 2;
                        glyph  = BinaryPrimitives.ReadUInt16BigEndian(cmap.AsSpan(at));
                        if (glyph != 0) glyph = (glyph + delta) & 0xffff;
                    }

                    if (glyph != 0) mapping[c] = glyph;
                }
            }

            return mapping;
        }

        private static Dictionary<int, int> ReadFormat12(byte[] cmap, int offset)
        {
            var groups  = (int)BinaryPrimitives.ReadUInt32BigEndian(cmap.AsSpan(offset + 12));
            var mapping = new Dictionary<int, int>();

            for (int g = 0; g < groups; g++)
            {
                var at    = offset + 16 + g * 12;
                var start = (int)BinaryPrimitives.ReadUInt32BigEndian(cmap.AsSpan(at));
                var end   = (int)BinaryPrimitives.ReadUInt32BigEndian(cmap.AsSpan(at + 4));
                var glyph = (int)BinaryPrimitives.ReadUInt32BigEndian(cmap.AsSpan(at + 8));

                for (int c = start; c <= end; c++) mapping[c] = glyph + (c - start);
            }

            return mapping;
        }
    }
}
