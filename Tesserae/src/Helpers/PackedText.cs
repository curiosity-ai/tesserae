using System.Text;

namespace Tesserae
{
    internal static class PackedText
    {
        private const byte   Key    = 42;
        private const string Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        internal static string Pack(string text, string alphabet)
        {
            var maxRun = Digits.Length / alphabet.Length;
            var packed = new StringBuilder();
            var i      = 0;

            while (i < text.Length)
            {
                var run = 1;
                while (run < maxRun && i + run < text.Length && text[i + run] == text[i]) run++;

                packed.Append(Digits[(alphabet.IndexOf(text[i]) + alphabet.Length * (run - 1)) ^ Key]);
                i += run;
            }

            return packed.ToString();
        }

        internal static string Unpack(string packed, string alphabet)
        {
            var text = new StringBuilder();

            for (var i = 0; i < packed.Length; i++)
            {
                var value  = Digits.IndexOf(packed[i]) ^ Key;
                var symbol = alphabet[value % alphabet.Length];

                for (var run = value / alphabet.Length; run >= 0; run--) text.Append(symbol);
            }

            return text.ToString();
        }
    }
}
