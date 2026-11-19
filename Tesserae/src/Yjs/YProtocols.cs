using Transpose;
using static Transpose.Core.es5;

namespace Tesserae
{
    /// <summary>
    /// <c>window.YProtocols</c>: the y-protocols sync handlers and the lib0 encoders they write through, as bundled
    /// with Yjs. <see cref="YjsSyncProvider"/> is built on these; they are public for a host that frames its own
    /// messages. Declared, not emitted: see <see cref="Y"/> for the rules.
    /// </summary>
    [External]
    [Convention(Notation.None)]
    [Name("YProtocols")]
    public static class YProtocols
    {
        /// <summary>A growing binary buffer (lib0 <c>encoding.Encoder</c>). Opaque.</summary>
        [External]
        [Convention(Notation.None)]
        public interface Encoder
        {
        }

        /// <summary>A read cursor over a <see cref="Uint8Array"/> (lib0 <c>decoding.Decoder</c>). Opaque.</summary>
        [External]
        [Convention(Notation.None)]
        public interface Decoder
        {
        }

        /// <summary>lib0's <c>encoding</c> module.</summary>
        [External]
        [Convention(Notation.None)]
        [Name("YProtocols.encoding")]
        public static class encoding
        {
            public static extern Encoder createEncoder();

            public static extern void writeVarUint(Encoder encoder, int value);

            /// <summary>Writes the length, then the bytes.</summary>
            public static extern void writeVarUint8Array(Encoder encoder, Uint8Array data);

            /// <summary>The number of bytes written so far. A template, because the compiler escapes a static member named <c>length</c>.</summary>
            [Template("YProtocols.encoding.length({encoder})")]
            public static extern int length(Encoder encoder);

            public static extern Uint8Array toUint8Array(Encoder encoder);
        }

        /// <summary>lib0's <c>decoding</c> module.</summary>
        [External]
        [Convention(Notation.None)]
        [Name("YProtocols.decoding")]
        public static class decoding
        {
            public static extern Decoder createDecoder(Uint8Array data);

            public static extern int readVarUint(Decoder decoder);

            public static extern Uint8Array readVarUint8Array(Decoder decoder);

            public static extern string readVarString(Decoder decoder);

            /// <summary>Whether unread bytes remain.</summary>
            public static extern bool hasContent(Decoder decoder);
        }

        /// <summary>y-protocols' <c>sync</c> module: the three sync messages and the handler that answers them.</summary>
        [External]
        [Convention(Notation.None)]
        [Name("YProtocols.sync")]
        public static class sync
        {
            /// <summary>Sync step 1: this peer's state vector, asking for what it is missing.</summary>
            public const int messageYjsSyncStep1 = 0;

            /// <summary>Sync step 2: the answer to step 1, everything the asking peer is missing.</summary>
            public const int messageYjsSyncStep2 = 1;

            /// <summary>An incremental update.</summary>
            public const int messageYjsUpdate = 2;

            public static extern void writeSyncStep1(Encoder encoder, Y.Doc doc);

            public static extern void writeUpdate(Encoder encoder, Uint8Array update);

            /// <summary>
            /// Reads one sync message and applies it to <paramref name="doc"/> with <paramref name="transactionOrigin"/>;
            /// a step 1 writes its step 2 answer into <paramref name="encoder"/>. Returns which message it was.
            /// </summary>
            public static extern int readSyncMessage(Decoder decoder, Encoder encoder, Y.Doc doc, object transactionOrigin);
        }

        /// <summary>lib0's <c>buffer</c> module.</summary>
        [External]
        [Convention(Notation.None)]
        [Name("YProtocols.buffer")]
        public static class buffer
        {
            public static extern string toBase64(Uint8Array data);

            public static extern Uint8Array fromBase64(string base64);
        }
    }
}
