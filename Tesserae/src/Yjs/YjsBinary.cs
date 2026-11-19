using Transpose;
using static Transpose.Core.es5;

namespace Tesserae
{
    /// <summary>
    /// Conversions between the <see cref="Uint8Array"/> Yjs reads and writes and what a transport carries: a C#
    /// <c>byte[]</c> or a base64 string. Base64 goes straight from and to the typed array (lib0's encoder, which is
    /// in the Yjs bundle), so a transport that speaks text never builds a <c>byte[]</c> in between.
    /// </summary>
    public static class YjsBinary
    {
        /// <summary>Copies a typed array into a C# <c>byte[]</c>.</summary>
        public static byte[] ToBytes(Uint8Array data)
        {
            var bytes = new byte[data.length];

            for (uint i = 0; i < data.length; i++)
            {
                bytes[i] = data[i];
            }

            return bytes;
        }

        /// <summary>Copies a C# <c>byte[]</c> into a typed array.</summary>
        public static Uint8Array FromBytes(byte[] bytes) => new Uint8Array(ArrayLike<byte>.From(bytes));

        /// <summary>Encodes as base64. Requires <see cref="Yjs.LoadAsync"/>.</summary>
        public static string ToBase64(Uint8Array data) => YProtocols.buffer.toBase64(data);

        /// <summary>Decodes base64. Requires <see cref="Yjs.LoadAsync"/>.</summary>
        public static Uint8Array FromBase64(string base64) => YProtocols.buffer.fromBase64(base64);
    }
}
