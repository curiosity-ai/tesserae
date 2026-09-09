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
    /// else does. That is what this does, which is why almost every property of the font survives intact:
    /// the flags, the instructions and every glyph that is not moved are never rewritten, and no declared
    /// bounding box is ever recomputed from the moved points. That last point is the reason to work this way
    /// rather than round-tripping through the plain glyf format - in these fonts the declared boxes disagree
    /// with the outlines, the rasterizer places each glyph from them, and any tool that recomputes them
    /// moves every glyph by tens of units.
    /// </para>
    /// <para>
    /// A box is nonetheless <em>widened</em> where its outline has left it, so it still bounds the ink it
    /// claims to bound - see <see cref="MakeBoxesCoverTheirOutlines"/>, which also explains why the side
    /// bearing has to follow the one edge of it that places the glyph.
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
            MakeBoxesCoverTheirOutlines(shifts);
            return plainDelta;
        }

        /// <summary>
        /// The subtle half of moving a glyph: its declared bounding box, which is never recomputed from the
        /// moved points, and must not be.
        /// <para>
        /// Two separate things hang off that box. The format lets a glyph leave it out, in which case the
        /// decoder computes one from the points - so moving the points would move the computed box with them,
        /// the two cancel out, and the glyph renders exactly where it did before. And the rasterizer places a
        /// glyph from the box and the side bearing together, as <c>outline xMin - declared xMin + lsb</c>, so
        /// lowering a declared <c>xMin</c> on its own slides the glyph right by as much and undoes the very
        /// correction being baked in. Both are why a glyph being moved that has no box of its own is given one
        /// computed from where its points were <em>before</em> the move.
        /// </para>
        /// <para>
        /// What that leaves, on its own, is a box that no longer contains its own outline: an icon drawn to the
        /// edge of its box and then shifted by the 0.04em cap - 12 units on a 300 unit em - declares a box 12
        /// units short of its ink. Chromium draws the ink regardless, because it rasterizes the points and not
        /// the box, but the declaration is false: every reader that trusts it is told the wrong extent (canvas
        /// <c>measureText().actualBoundingBox*</c> is one), and a rasterizer that allocated from the box would
        /// crop the icon. So every box here is widened until it covers its outline - only ever widened, never
        /// shrunk - and where that means lowering <c>xMin</c>, the <c>lsb</c> is lowered with it, which holds
        /// <c>outline xMin - declared xMin + lsb</c>, and so the rendered position, exactly where it was.
        /// </para>
        /// <para>
        /// Every glyph is covered, not only the ones moving now, so a font that a previous run left with a box
        /// short of its ink is repaired rather than carried forward, and running this twice changes nothing the
        /// second time. Composites are the exception: their extent comes from glyphs they reference rather than
        /// from points of their own, they are never moved, and so they are passed through untouched.
        /// </para>
        /// </summary>
        private void MakeBoxesCoverTheirOutlines(IReadOnlyDictionary<int, (int Dx, int Dy)> shifts)
        {
            var bitmap  = (byte[])_bboxBitmap.Clone();
            var entries = new MemoryStream(_bboxStream.Length + shifts.Count * 8);
            var lowered = new Dictionary<int, int>();
            var readAt  = 0;
            var added   = 0;
            var grown   = 0;
            var encoded = new byte[8];
            Box? bounds = null;

            for (int id = 0; id < NumGlyphs; id++)
            {
                var hasBox = (_bboxBitmap[id >> 3] & (0x80 >> (id & 7))) != 0;
                var span   = _glyphs[id];
                var boxAt  = readAt;

                if (hasBox) readAt += 8;

                if (span.IsEmpty || span.IsComposite)
                {
                    if (hasBox) entries.Write(_bboxStream, boxAt, 8);
                    continue;
                }

                shifts.TryGetValue(id, out var shift);
                var points  = BoundingBoxBeforeMoving(span);
                var outline = points.Offset(shift.Dx, shift.Dy);

                Box declared;

                if (hasBox)
                {
                    declared = new Box(
                        BinaryPrimitives.ReadInt16BigEndian(_bboxStream.AsSpan(boxAt)),
                        BinaryPrimitives.ReadInt16BigEndian(_bboxStream.AsSpan(boxAt + 2)),
                        BinaryPrimitives.ReadInt16BigEndian(_bboxStream.AsSpan(boxAt + 4)),
                        BinaryPrimitives.ReadInt16BigEndian(_bboxStream.AsSpan(boxAt + 6)));
                }
                else if (shift.Dx == 0 && shift.Dy == 0)
                {
                    // The box the decoder computes for this glyph is its outline, so it already covers it.
                    bounds = bounds is null ? outline : bounds.Value.Union(outline);
                    continue;
                }
                else
                {
                    declared = points;
                    added++;
                }

                var box = declared.Union(outline);

                if (!box.FitsInt16)
                {
                    throw new InvalidOperationException($"glyph {id}: the bounding box {box} does not fit the format's 16 bits");
                }

                if (!box.Contains(outline))
                {
                    throw new InvalidOperationException(
                        $"glyph {id}: the box {box} does not cover the outline {outline}, so the union is wrong");
                }

                BinaryPrimitives.WriteInt16BigEndian(encoded.AsSpan(0), (short)box.XMin);
                BinaryPrimitives.WriteInt16BigEndian(encoded.AsSpan(2), (short)box.YMin);
                BinaryPrimitives.WriteInt16BigEndian(encoded.AsSpan(4), (short)box.XMax);
                BinaryPrimitives.WriteInt16BigEndian(encoded.AsSpan(6), (short)box.YMax);
                entries.Write(encoded);

                if (hasBox)
                {
                    if (!box.Equals(declared)) grown++;
                }
                else
                {
                    bitmap[id >> 3] |= (byte)(0x80 >> (id & 7));
                }

                // Only xMin places the glyph, so it is the only edge whose growth the lsb has to follow.
                if (box.XMin < declared.XMin) lowered[id] = declared.XMin - box.XMin;

                bounds = bounds is null ? box : bounds.Value.Union(box);
            }

            if (readAt != _bboxStream.Length)
            {
                throw new InvalidOperationException(
                    $"transformed glyf: read {readAt} of {_bboxStream.Length} bounding box bytes, so the bitmap and the stream disagree");
            }

            _bboxBitmap         = bitmap;
            _bboxStream         = entries.ToArray();
            BoxesAdded          = added;
            BoxesGrown          = grown;
            SideBearingsToLower = lowered;
            DeclaredGlyphBounds = bounds;
        }

        /// <summary>How many glyphs needed a bounding box written out because they had none.</summary>
        public int BoxesAdded { get; private set; }

        /// <summary>How many glyphs already had a box that had to be widened to cover their outline.</summary>
        public int BoxesGrown { get; private set; }

        /// <summary>
        /// Glyphs whose declared <c>xMin</c> was lowered to cover their outline, and by how much. The
        /// <c>hmtx</c> left side bearing has to drop by the same amount, or the glyph slides right by it.
        /// </summary>
        public IReadOnlyDictionary<int, int> SideBearingsToLower { get; private set; } = new Dictionary<int, int>();

        /// <summary>Every glyph's box, unioned, so the font wide box in <c>head</c> can be grown to hold them.</summary>
        public Box? DeclaredGlyphBounds { get; private set; }

        /// <summary>A glyph bounding box in font units, in the order the format stores its four values.</summary>
        public readonly struct Box : IEquatable<Box>
        {
            public Box(int xMin, int yMin, int xMax, int yMax)
            {
                XMin = xMin;
                YMin = yMin;
                XMax = xMax;
                YMax = yMax;
            }

            public int XMin { get; }
            public int YMin { get; }
            public int XMax { get; }
            public int YMax { get; }

            public Box Offset(int dx, int dy) => new Box(XMin + dx, YMin + dy, XMax + dx, YMax + dy);

            public Box Union(Box other) => new Box(
                Math.Min(XMin, other.XMin), Math.Min(YMin, other.YMin),
                Math.Max(XMax, other.XMax), Math.Max(YMax, other.YMax));

            public bool Contains(Box other) => other.XMin >= XMin && other.YMin >= YMin
                                            && other.XMax <= XMax && other.YMax <= YMax;

            public bool FitsInt16 => XMin >= short.MinValue && YMin >= short.MinValue
                                  && XMax <= short.MaxValue && YMax <= short.MaxValue;

            public bool Equals(Box other) => XMin == other.XMin && YMin == other.YMin
                                          && XMax == other.XMax && YMax == other.YMax;

            public override bool Equals(object obj) => obj is Box other && Equals(other);

            public override int GetHashCode() => (XMin, YMin, XMax, YMax).GetHashCode();

            public override string ToString() => $"({XMin},{YMin},{XMax},{YMax})";
        }

        private Box BoundingBoxBeforeMoving(GlyphSpan span)
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

            return new Box(xMin, yMin, xMax, yMax);
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
