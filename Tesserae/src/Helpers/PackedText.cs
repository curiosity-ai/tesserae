using System.Text;

namespace Tesserae
{
    internal static class PackedText
    {
        private const string Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        internal static string Unpack(string packed, string alphabet, byte key)
        {
            var text = new StringBuilder();

            for (var i = 0; i < packed.Length; i++)
            {
                var value  = Digits.IndexOf(packed[i]) ^ key;
                var symbol = alphabet[value % alphabet.Length];

                for (var run = value / alphabet.Length; run >= 0; run--) text.Append(symbol);
            }

            return text.ToString();
        }
    }
}
