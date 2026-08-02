using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Build.UpdateInterfaceIcons
{
    /// <summary>
    /// The woff2 transformed <c>glyf</c> table, and the one edit that is needed on it: moving a glyph.
    /// <para>
    /// Coordinates in this format are stored as deltas from the previous point, the first point being
    /// relative to the origin, so a glyph moves if its <em>first</em> point's delta changes and nothing
    /// else does. That is what this does, which is why every other property of the font survives intact:
    /// the declared bounding boxes, the side bearings, the flags, the instructions and every other glyph
    /// are never rewritten. Those declared boxes are the reason to work this way rather than round-tripping
    /// through the plain glyf format - in these fonts they disagree with the outlines, the rasterizer
    /// places each glyph from them, and any tool that recomputes them moves every glyph by tens of units.
    /// </para>
    /// </summary>
    internal sealed class TransformedGlyf
    {
        private const int HeaderSize = 36;

        private readonly byte[] _header;          // version, flags, numGlyphs, indexFormat and stream sizes
        private readonly byte[] _nContourStream;
        private readonly byte[] _nPointsStream;
        private readonly byte[] _flagStream;
        private readonly byte[] _glyphStream;
        private readonly byte[] _compositeStream;
        private byte[] _bboxBitmap;
        private byte[] _bboxStream;
        private readonly byte[] _instructionStream;

        /// <summary>Where each glyph's points live in the flag and glyph streams.</summary>
        private readonly List<GlyphSpan> _glyphs = new List<GlyphSpan>();

        private sealed class GlyphSpan
        {
            public int  FirstFlagAt         { get; set; } = -1;
            public int  PointCount          { get; set; }
            public int  ContourCount        { get; set; }
            public int  FirstTripletAt      { get; set; } = -1;
            public int  FirstTripletSize    { get; set; }
            public int  FirstX              { get; set; }
            public int  FirstY              { get; set; }
            public int  InstructionLength   { get; set; }
            public int  ComponentByteCount  { get; set; }
            public bool HasInstructions     { get; set; }
            public bool IsComposite         { get; set; }
            public bool IsEmpty             { get; set; }
        }

        public int NumGlyphs { get; }

        /// <summary>Whether this glyph has outline points of its own that a shift could move.</summary>
        public bool CanMove(int id) => !_glyphs[id].IsEmpty && !_glyphs[id].IsComposite;

        private TransformedGlyf(byte[] data)
        {
            if (data.Length < HeaderSize) throw new InvalidOperationException("transformed glyf table is too short");

            _header   = data.AsSpan(0, HeaderSize).ToArray();
            NumGlyphs = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(4));

            var sizes  = new int[7];
            for (int i = 0; i < 7; i++) sizes[i] = (int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(8 + i * 4));

            var at = HeaderSize;

            byte[] Slice(int size)
            {
                var slice = data.AsSpan(at, size).ToArray();
                at += size;
                return slice;
            }

            _nContourStream    = Slice(sizes[0]);
            _nPointsStream     = Slice(sizes[1]);
            _flagStream        = Slice(sizes[2]);
            _glyphStream       = Slice(sizes[3]);
            _compositeStream   = Slice(sizes[4]);
            var bboxBlob       = Slice(sizes[5]);
            var bitmapSize     = ((NumGlyphs + 31) >> 5) << 2;
            _bboxBitmap        = bboxBlob.AsSpan(0, bitmapSize).ToArray();
            _bboxStream        = bboxBlob.AsSpan(bitmapSize).ToArray();
            _instructionStream = Slice(sizes[6]);

            if (at != data.Length)
            {
                throw new InvalidOperationException($"transformed glyf: {data.Length - at} bytes left over after the streams");
            }

            Walk();
        }

        public static TransformedGlyf Parse(byte[] data) => new TransformedGlyf(data);

        /// <summary>
        /// Walks every glyph to find where its points sit in the streams. The walk has to visit them all,
        /// even the ones that will not move, because the streams are sequential.
        /// </summary>
        private void Walk()
        {
            int nPointsAt = 0, flagAt = 0, glyphAt = 0, compositeAt = 0, instructionAt = 0;

            for (int id = 0; id < NumGlyphs; id++)
            {
                var span      = new GlyphSpan();
                var nContours = BinaryPrimitives.ReadInt16BigEndian(_nContourStream.AsSpan(id * 2));

                if (nContours == 0)
                {
                    span.IsEmpty = true;
                    _glyphs.Add(span);
                    continue;
                }

                if (nContours < 0)
                {
                    // Composite: the components come from their own stream, and an instruction length
                    // follows in the glyph stream only if one of them asked for instructions.
                    span.IsComposite = true;
                    var componentsAt = compositeAt;
                    span.HasInstructions     = SkipComponents(ref compositeAt);
                    span.ComponentByteCount  = compositeAt - componentsAt;
                    if (span.HasInstructions) span.InstructionLength = ReadNext255UShort(_glyphStream, ref glyphAt);
                    instructionAt += span.InstructionLength;
                    _glyphs.Add(span);
                    continue;
                }

                var nPoints = 0;
                for (int c = 0; c < nContours; c++) nPoints += ReadNext255UShort(_nPointsStream, ref nPointsAt);

                span.FirstFlagAt    = flagAt;
                span.PointCount     = nPoints;
                span.ContourCount   = nContours;
                span.FirstTripletAt = glyphAt;
                span.HasInstructions = true;

                for (int p = 0; p < nPoints; p++)
                {
                    var flag  = (byte)(_flagStream[flagAt + p] & 0x7f);
                    var size  = TripletSize(flag);

                    if (p == 0)
                    {
                        span.FirstTripletSize = size;
                        (span.FirstX, span.FirstY) = DecodeTriplet(flag, _glyphStream, glyphAt);
                    }

                    glyphAt += size;
                }

                flagAt += nPoints;
                span.InstructionLength = ReadNext255UShort(_glyphStream, ref glyphAt);
                instructionAt         += span.InstructionLength;
                _glyphs.Add(span);
            }

            if (glyphAt != _glyphStream.Length)
            {
                throw new InvalidOperationException(
                    $"transformed glyf: walked {glyphAt} of {_glyphStream.Length} glyph stream bytes, so the walk is wrong");
            }

            if (instructionAt != _instructionStream.Length)
            {
                throw new InvalidOperationException(
                    $"transformed glyf: the glyphs account for {instructionAt} of {_instructionStream.Length} instruction bytes, so the walk is wrong");
            }
        }

        /// <summary>
        /// Moves glyphs by rewriting the first point of each. Returns the change in the size the
        /// un-transformed table would occupy, which the container reports as its allocation hint.
        /// </summary>
        public long Move(IReadOnlyDictionary<int, (int Dx, int Dy)> shifts)
        {
            var rewritten = new MemoryStream(_glyphStream.Length + shifts.Count * 4);
            var flags     = (byte[])_flagStream.Clone();
            var cursor    = 0;
            long plainDelta = 0;

            foreach (var (id, shift) in shifts.OrderBy(s => s.Key))
            {
                if (id < 0 || id >= NumGlyphs) throw new InvalidOperationException($"glyph {id} is outside this font");

                var span = _glyphs[id];

                if (span.IsEmpty) continue;

                if (span.IsComposite)
                {
                    throw new InvalidOperationException(
                        $"glyph {id} is composite; moving one means adjusting its component offsets, which this does not do");
                }

                var x = span.FirstX + shift.Dx;
                var y = span.FirstY + shift.Dy;

                if (Math.Abs(x) > ushort.MaxValue || Math.Abs(y) > ushort.MaxValue)
                {
                    throw new InvalidOperationException($"glyph {id} would move its first point to {x},{y}, too far to encode");
                }

                rewritten.Write(_glyphStream, cursor, span.FirstTripletAt - cursor);

                // The widest of the triplet forms, four bytes with both magnitudes as 16 bit and both
                // signs in the flag. Always valid, and only ever used for one point per moved glyph.
                var onCurveBit = (byte)(flags[span.FirstFlagAt] & 0x80);
                flags[span.FirstFlagAt] = (byte)(onCurveBit | 124 | (x >= 0 ? 1 : 0) | (y >= 0 ? 2 : 0));

                rewritten.WriteByte((byte)((Math.Abs(x) >> 8) & 0xff));
                rewritten.WriteByte((byte)(Math.Abs(x) & 0xff));
                rewritten.WriteByte((byte)((Math.Abs(y) >> 8) & 0xff));
                rewritten.WriteByte((byte)(Math.Abs(y) & 0xff));

                cursor      = span.FirstTripletAt + span.FirstTripletSize;
                plainDelta += Round4(PlainGlyphSize(span, shift.Dx, shift.Dy)) - Round4(PlainGlyphSize(span, 0, 0));
            }

            rewritten.Write(_glyphStream, cursor, _glyphStream.Length - cursor);

            _glyphStreamRewritten = rewritten.ToArray();
            _flagStreamRewritten  = flags;
            PinDownBoundingBoxes(shifts.Keys);
            return plainDelta;
        }

        /// <summary>
        /// The subtle half of moving a glyph. This format lets a glyph leave its bounding box out, in which
        /// case the decoder computes one from the points - so moving the points would move the box with them,
        /// and the two cancel out: the glyph renders exactly where it did before. Any glyph being moved that
        /// has no box of its own therefore gets one written now, computed from where its points were
        /// <em>before</em> the move, which is what makes the move visible. Glyphs that already carry a box
        /// keep it untouched, and in these fonts those boxes disagree with the outlines on purpose.
        /// </summary>
        private void PinDownBoundingBoxes(IEnumerable<int> movedGlyphs)
        {
            var moved  = new HashSet<int>(movedGlyphs);
            var bitmap = (byte[])_bboxBitmap.Clone();
            var entries = new MemoryStream(_bboxStream.Length + moved.Count * 8);
            var readAt  = 0;
            var added   = 0;
            var encoded = new byte[8];

            for (int id = 0; id < NumGlyphs; id++)
            {
                var hasBox = (_bboxBitmap[id >> 3] & (0x80 >> (id & 7))) != 0;

                if (hasBox)
                {
                    entries.Write(_bboxStream, readAt, 8);
                    readAt += 8;
                    continue;
                }

                var span = _glyphs[id];

                if (!moved.Contains(id) || span.IsEmpty || span.IsComposite) continue;

                var box = BoundingBoxBeforeMoving(span);
                BinaryPrimitives.WriteInt16BigEndian(encoded.AsSpan(0), (short)box.XMin);
                BinaryPrimitives.WriteInt16BigEndian(encoded.AsSpan(2), (short)box.YMin);
                BinaryPrimitives.WriteInt16BigEndian(encoded.AsSpan(4), (short)box.XMax);
                BinaryPrimitives.WriteInt16BigEndian(encoded.AsSpan(6), (short)box.YMax);
                entries.Write(encoded);

                bitmap[id >> 3] |= (byte)(0x80 >> (id & 7));
                added++;
            }

            if (readAt != _bboxStream.Length)
            {
                throw new InvalidOperationException(
                    $"transformed glyf: read {readAt} of {_bboxStream.Length} bounding box bytes, so the bitmap and the stream disagree");
            }

            _bboxBitmap = bitmap;
            _bboxStream = entries.ToArray();
            BoxesAdded  = added;
        }

        /// <summary>How many glyphs needed a bounding box written out because they had none.</summary>
        public int BoxesAdded { get; private set; }

        private (int XMin, int YMin, int XMax, int YMax) BoundingBoxBeforeMoving(GlyphSpan span)
        {
            int x = 0, y = 0, at = span.FirstTripletAt;
            int xMin = int.MaxValue, yMin = int.MaxValue, xMax = int.MinValue, yMax = int.MinValue;

            for (int p = 0; p < span.PointCount; p++)
            {
                var flag = (byte)(_flagStream[span.FirstFlagAt + p] & 0x7f);
                var (dx, dy) = DecodeTriplet(flag, _glyphStream, at);
                at += TripletSize(flag);
                x  += dx;
                y  += dy;
                xMin = Math.Min(xMin, x); xMax = Math.Max(xMax, x);
                yMin = Math.Min(yMin, y); yMax = Math.Max(yMax, y);
            }

            return (xMin, yMin, xMax, yMax);
        }

        private byte[] _glyphStreamRewritten;
        private byte[] _flagStreamRewritten;

        /// <summary>Serializes the table back, with the rewritten streams if <see cref="Move"/> was called.</summary>
        public byte[] Serialize()
        {
            var glyphStream = _glyphStreamRewritten ?? _glyphStream;
            var flagStream  = _flagStreamRewritten ?? _flagStream;
            var header      = (byte[])_header.Clone();

            // The bounding box "stream" is the bitmap saying which glyphs have a box, followed by the boxes
            // themselves, and its recorded size covers both.
            var sizes = new[]
            {
                _nContourStream.Length, _nPointsStream.Length, flagStream.Length, glyphStream.Length,
                _compositeStream.Length, _bboxBitmap.Length + _bboxStream.Length, _instructionStream.Length,
            };

            for (int i = 0; i < sizes.Length; i++) BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(8 + i * 4), (uint)sizes[i]);

            using var output = new MemoryStream();
            output.Write(header);
            output.Write(_nContourStream);
            output.Write(_nPointsStream);
            output.Write(flagStream);
            output.Write(glyphStream);
            output.Write(_compositeStream);
            output.Write(_bboxBitmap);
            output.Write(_bboxStream);
            output.Write(_instructionStream);
            return output.ToArray();
        }

        /// <summary>
        /// The size the plain <c>glyf</c> table takes once the decoder has rebuilt it from these streams: the
        /// value the container has to declare as the table's un-transformed length. It is not a constant - a
        /// moved glyph's first coordinate delta can change how many bytes it needs - so it is recomputed
        /// rather than carried over, and comparing it against what the vendor declared proves the model right.
        /// <para>It reads the streams as they came out of the file, so call it before <see cref="Move"/> and
        /// add what that returns.</para>
        /// </summary>
        public long ReconstructedLength() => _glyphs.Sum(g => (long)Round4(PlainGlyphSize(g, 0, 0)));

        /// <summary>
        /// How many bytes one glyph occupies in the plain <c>glyf</c> format, optionally with its first point
        /// shifted. Mirrors what the woff2 decoder writes: a fixed header, the contour ends, the instructions,
        /// then run-length compressed flags whose bits depend on the coordinate deltas, then the deltas.
        /// </summary>
        private int PlainGlyphSize(GlyphSpan span, int dx, int dy)
        {
            if (span.IsEmpty) return 0;

            const int Header = 10;                                          // numberOfContours and the bbox

            if (span.IsComposite)
            {
                return Header + span.ComponentByteCount + (span.HasInstructions ? 2 + span.InstructionLength : 0);
            }

            var size       = Header + 2 * span.ContourCount + 2 + span.InstructionLength;
            var coordinates = 0;
            var lastFlag   = -1;
            var repeats    = 0;
            var at         = span.FirstTripletAt;

            for (int p = 0; p < span.PointCount; p++)
            {
                var raw      = _flagStream[span.FirstFlagAt + p];
                var (x, y)   = DecodeTriplet((byte)(raw & 0x7f), _glyphStream, at);
                at          += TripletSize((byte)(raw & 0x7f));

                if (p == 0) { x += dx; y += dy; }

                var flag = (raw & 0x80) != 0 ? 0 : 0x01;                    // bit 7 set means off curve

                if (x == 0) flag |= 0x10;
                else if (Math.Abs(x) < 256) { flag |= 0x02 | (x > 0 ? 0x10 : 0); coordinates += 1; }
                else coordinates += 2;

                if (y == 0) flag |= 0x20;
                else if (Math.Abs(y) < 256) { flag |= 0x04 | (y > 0 ? 0x20 : 0); coordinates += 1; }
                else coordinates += 2;

                if (flag == lastFlag && repeats != 255)
                {
                    repeats++;
                }
                else
                {
                    if (repeats != 0) size++;                               // the run's repeat count
                    size++;                                                 // the flag itself
                    repeats = 0;
                }

                lastFlag = flag;
            }

            if (repeats != 0) size++;

            return size + coordinates;
        }

        private static int Round4(int value) => (value + 3) & ~3;

        /// <summary>Steps over one composite glyph's components, reporting whether any wants instructions.</summary>
        private bool SkipComponents(ref int at)
        {
            var haveInstructions = false;

            while (true)
            {
                var flags = BinaryPrimitives.ReadUInt16BigEndian(_compositeStream.AsSpan(at));
                at += 4;                                                    // flags and glyphIndex
                at += (flags & 0x0001) != 0 ? 4 : 2;                        // ARG_1_AND_2_ARE_WORDS
                if ((flags & 0x0008) != 0) at += 2;                         // WE_HAVE_A_SCALE
                else if ((flags & 0x0040) != 0) at += 4;                    // X_AND_Y_SCALE
                else if ((flags & 0x0080) != 0) at += 8;                    // TWO_BY_TWO
                if ((flags & 0x0100) != 0) haveInstructions = true;         // WE_HAVE_INSTRUCTIONS
                if ((flags & 0x0020) == 0) return haveInstructions;         // MORE_COMPONENTS
            }
        }

        /// <summary>Bytes this triplet occupies, which depends only on the flag.</summary>
        private static int TripletSize(byte flag) => flag < 84 ? 1 : flag < 120 ? 2 : flag < 124 ? 3 : 4;

        private static (int Dx, int Dy) DecodeTriplet(byte flag, byte[] stream, int at)
        {
            int Signed(int f, int magnitude) => (f & 1) != 0 ? magnitude : -magnitude;

            if (flag < 10) return (0, Signed(flag, ((flag & 14) << 7) + stream[at]));
            if (flag < 20) return (Signed(flag, (((flag - 10) & 14) << 7) + stream[at]), 0);

            if (flag < 84)
            {
                var b0 = flag - 20;
                var b1 = stream[at];
                return (Signed(flag, 1 + (b0 & 0x30) + (b1 >> 4)),
                        Signed(flag >> 1, 1 + ((b0 & 0x0c) << 2) + (b1 & 0x0f)));
            }

            if (flag < 120)
            {
                var b0 = flag - 84;
                return (Signed(flag, 1 + ((b0 / 12) << 8) + stream[at]),
                        Signed(flag >> 1, 1 + (((b0 % 12) >> 2) << 8) + stream[at + 1]));
            }

            if (flag < 124)
            {
                var b2 = stream[at + 1];
                return (Signed(flag, (stream[at] << 4) + (b2 >> 4)),
                        Signed(flag >> 1, ((b2 & 0x0f) << 8) + stream[at + 2]));
            }

            return (Signed(flag, (stream[at] << 8) + stream[at + 1]),
                    Signed(flag >> 1, (stream[at + 2] << 8) + stream[at + 3]));
        }

        private static int ReadNext255UShort(byte[] stream, ref int at)
        {
            var code = stream[at++];

            if (code == 253)
            {
                var value = BinaryPrimitives.ReadUInt16BigEndian(stream.AsSpan(at));
                at += 2;
                return value;
            }

            if (code == 254) return stream[at++] + 506;
            if (code == 255) return stream[at++] + 253;
            return code;
        }
    }
}
