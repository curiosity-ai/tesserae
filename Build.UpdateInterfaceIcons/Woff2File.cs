using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Build.UpdateInterfaceIcons
{
    /// <summary>One entry of a woff2 table directory, with the table's bytes as stored in the file.</summary>
    internal sealed class Woff2Table
    {
        public string Tag              { get; set; }
        public int    TransformVersion { get; set; }

        /// <summary>Length of the table once un-transformed. Metadata: the decoder allocates from it.</summary>
        public uint OriginalLength { get; set; }

        /// <summary>The bytes as they sit in the file, still transformed if this table is.</summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// glyf and loca use 3 for "no transform" and 0 for the woff2 glyph encoding; every other table
        /// uses 0 for "no transform". The transform length is only stored when a transform is in effect.
        /// </summary>
        public bool IsTransformed => Tag is "glyf" or "loca" ? TransformVersion != 3 : TransformVersion != 0;
    }

    /// <summary>
    /// Reads and writes the woff2 container: header, table directory, and the single Brotli stream that
    /// holds every table back to back. Table contents are handed over untouched, so a caller can replace
    /// one and write the file back without disturbing any of the others.
    /// </summary>
    internal sealed class Woff2File
    {
        // Directory entries name their table by an index into this list; 0x3f means the tag follows inline.
        private static readonly string[] KnownTags =
        {
            "cmap", "head", "hhea", "hmtx", "maxp", "name", "OS/2", "post", "cvt ", "fpgm", "glyf", "loca",
            "prep", "CFF ", "VORG", "EBDT", "EBLC", "gasp", "hdmx", "kern", "LTSH", "PCLT", "VDMX", "vhea",
            "vmtx", "BASE", "GDEF", "GPOS", "GSUB", "EBSC", "JSTF", "MATH", "CBDT", "CBLC", "COLR", "CPAL",
            "SVG ", "sbix", "acnt", "avar", "bdat", "bloc", "bsln", "cvar", "fdsc", "feat", "fmtx", "fvar",
            "gvar", "hsty", "just", "lcar", "mort", "morx", "opbd", "prop", "trak", "Zapf", "Silf", "Glat",
            "Gloc", "Feat", "Sill",
        };

        private uint   _flavor;
        private ushort _majorVersion;
        private ushort _minorVersion;
        private uint   _totalSfntSize;
        private byte[] _metadata = Array.Empty<byte>();
        private uint   _metadataOriginalLength;
        private byte[] _privateData = Array.Empty<byte>();

        public List<Woff2Table> Tables { get; } = new List<Woff2Table>();

        public Woff2Table this[string tag] => Tables.FirstOrDefault(t => t.Tag == tag)
                                          ?? throw new InvalidOperationException($"the font has no '{tag}' table");

        public bool Has(string tag) => Tables.Any(t => t.Tag == tag);

        public static Woff2File Read(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var file  = new Woff2File();

            if (bytes.Length < 48 || BinaryPrimitives.ReadUInt32BigEndian(bytes) != 0x774F4632)
            {
                throw new InvalidOperationException($"{Path.GetFileName(path)} is not a woff2 file");
            }

            file._flavor        = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(4));
            var numTables       = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(12));
            file._totalSfntSize = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(16));
            var compressedSize  = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(20));
            file._majorVersion  = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(24));
            file._minorVersion  = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(26));
            var metaOffset      = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(28));
            var metaLength      = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(32));
            file._metadataOriginalLength = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(36));
            var privOffset      = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(40));
            var privLength      = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(44));

            var at      = 48;
            var lengths = new List<uint>();

            for (int i = 0; i < numTables; i++)
            {
                var flags = bytes[at++];
                var index = flags & 0x3f;
                var table = new Woff2Table { TransformVersion = (flags >> 6) & 0x3 };

                if (index == 0x3f)
                {
                    table.Tag = System.Text.Encoding.ASCII.GetString(bytes, at, 4);
                    at       += 4;
                }
                else
                {
                    table.Tag = KnownTags[index];
                }

                table.OriginalLength = ReadBase128(bytes, ref at);
                lengths.Add(table.IsTransformed ? ReadBase128(bytes, ref at) : table.OriginalLength);
                file.Tables.Add(table);
            }

            var block = BrotliDecompress(bytes.AsSpan(at, (int)compressedSize).ToArray());

            if (block.Length != lengths.Sum(l => (long)l))
            {
                throw new InvalidOperationException(
                    $"{Path.GetFileName(path)}: the table data block is {block.Length} bytes but the directory adds up to {lengths.Sum(l => (long)l)}");
            }

            // Tables sit back to back in the block, in directory order, with no padding.
            var cursor = 0;

            for (int i = 0; i < file.Tables.Count; i++)
            {
                file.Tables[i].Data = block.AsSpan(cursor, (int)lengths[i]).ToArray();
                cursor            += (int)lengths[i];
            }

            if (metaLength > 0) file._metadata = bytes.AsSpan((int)metaOffset, (int)metaLength).ToArray();
            if (privLength > 0) file._privateData = bytes.AsSpan((int)privOffset, (int)privLength).ToArray();

            return file;
        }

        public void Write(string path)
        {
            var directory = new MemoryStream();

            foreach (var table in Tables)
            {
                var index = Array.IndexOf(KnownTags, table.Tag);
                var flags = (byte)(((table.TransformVersion & 0x3) << 6) | (index < 0 ? 0x3f : index));
                directory.WriteByte(flags);

                if (index < 0) directory.Write(System.Text.Encoding.ASCII.GetBytes(table.Tag));

                WriteBase128(directory, table.OriginalLength);
                if (table.IsTransformed) WriteBase128(directory, (uint)table.Data.Length);
            }

            var block      = Tables.SelectMany(t => t.Data).ToArray();
            var compressed = BrotliCompress(block);
            var header     = new byte[48];
            var directoryBytes = directory.ToArray();

            // Every block after the compressed one starts on a four byte boundary, and the file as a whole
            // ends on one. Not decoration: a decoder rounds the end of the compressed block up to four and
            // reads that as the start of what follows, so an unpadded file looks like it runs off its own
            // end. Chromium rejects one outright, which - the length being what it is - happens to three
            // fonts in four.
            var afterBlock  = (uint)(48 + directoryBytes.Length + compressed.Length);
            var metaOffset  = Round4(afterBlock);
            var afterMeta   = metaOffset + (uint)_metadata.Length;
            var privOffset  = _privateData.Length == 0 ? 0 : Round4(afterMeta);
            var total       = Round4(_privateData.Length == 0 ? afterMeta : privOffset + (uint)_privateData.Length);

            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(0), 0x774F4632);
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4), _flavor);
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(8), total);
            BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(12), (ushort)Tables.Count);
            BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(14), 0);
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(16), _totalSfntSize);
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(20), (uint)compressed.Length);
            BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(24), _majorVersion);
            BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(26), _minorVersion);
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(28), _metadata.Length == 0 ? 0 : metaOffset);
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(32), (uint)_metadata.Length);
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(36), _metadataOriginalLength);
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(40), privOffset);
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(44), (uint)_privateData.Length);

            using var output = File.Create(path);
            output.Write(header);
            output.Write(directoryBytes);
            output.Write(compressed);
            Pad(output, metaOffset - afterBlock);
            output.Write(_metadata);
            if (_privateData.Length > 0) Pad(output, privOffset - afterMeta);
            output.Write(_privateData);
            Pad(output, total - (uint)output.Position);
        }

        private static uint Round4(uint value) => (value + 3) & ~3u;

        private static void Pad(Stream output, uint count)
        {
            for (var i = 0u; i < count; i++) output.WriteByte(0);
        }

        /// <summary>Adjusts the allocation hint by however much the un-transformed font grew or shrank.</summary>
        public void AdjustTotalSfntSize(long delta) => _totalSfntSize = (uint)(_totalSfntSize + delta);

        private static uint ReadBase128(byte[] bytes, ref int at)
        {
            uint value = 0;

            for (int i = 0; i < 5; i++)
            {
                var b = bytes[at++];
                value = (value << 7) | (uint)(b & 0x7f);

                if ((b & 0x80) == 0) return value;
            }

            throw new InvalidOperationException("malformed UIntBase128 in the table directory");
        }

        private static void WriteBase128(Stream output, uint value)
        {
            var size = 1;
            for (uint probe = value; probe >= 0x80; probe >>= 7) size++;

            for (int i = size - 1; i >= 0; i--)
            {
                var b = (byte)((value >> (7 * i)) & 0x7f);
                if (i > 0) b |= 0x80;
                output.WriteByte(b);
            }
        }

        private static byte[] BrotliDecompress(byte[] compressed)
        {
            using var input  = new MemoryStream(compressed);
            using var brotli = new BrotliStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            brotli.CopyTo(output);
            return output.ToArray();
        }

        private static byte[] BrotliCompress(byte[] raw)
        {
            using var output = new MemoryStream();

            using (var brotli = new BrotliStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
            {
                brotli.Write(raw);
            }

            return output.ToArray();
        }
    }
}
