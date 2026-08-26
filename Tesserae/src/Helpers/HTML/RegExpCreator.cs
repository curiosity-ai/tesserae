using System;
using System.Collections.Generic;
using System.Linq;
using Transpose;
using Transpose.Core;
using String = Transpose.Core.String;

namespace Tesserae
{
    /// <summary>
    /// Builds the regular expression <see cref="MarkHighlighter"/> marks with: the keyword is
    /// escaped, each letter is widened to a character class covering its diacritic variants, and
    /// whitespace runs are merged so a keyword typed with one space still matches text with many.
    /// Adapted from mark.js (https://github.com/julkue/mark.js, MIT).
    /// </summary>
    [Transpose.Name("tss.RegExpCreator")]
    public static class RegExpCreator
    {
        public static bool CaseSensitive { get; set; } = false;

        public static es5.RegExp Create(string str)
        {
            str = EscapeStr(str);
            str = CreateDiacriticsRegExp(str);
            str = CreateMergedBlanksRegExp(str);

            return new es5.RegExp(str, $"gm{(CaseSensitive ? "" : "i")}");
        }

        // One entry per base letter, holding every variant the letter should also match.
        // The entries are disjoint, so replacing one class can never touch text inserted by another.
        private static readonly string[] Diacritics = new[]
        {
            "aàáảãạăằắẳẵặâầấẩẫậäåāąAÀÁẢÃẠĂẰẮẲẴẶÂẦẤẨẪẬÄÅĀĄ",
            "cçćčCÇĆČ",
            "dđďDĐĎ",
            "eèéẻẽẹêềếểễệëěēęEÈÉẺẼẸÊỀẾỂỄỆËĚĒĘ",
            "iìíỉĩịîïīIÌÍỈĨỊÎÏĪ",
            "lłLŁ",
            "nñňńNÑŇŃ",
            "oòóỏõọôồốổỗộơởỡớờợöøōOÒÓỎÕỌÔỒỐỔỖỘƠỞỠỚỜỢÖØŌ",
            "rřRŘ",
            "sšśșşSŠŚȘŞ",
            "tťțţTŤȚŢ",
            "uùúủũụưừứửữựûüůūUÙÚỦŨỤƯỪỨỬỮỰÛÜŮŪ",
            "yýỳỷỹỵÿYÝỲỶỸỴŸ",
            "zžżźZŽŻŹ"
        };

        private static readonly string[] DiacriticsCaseSensitive = new[]
        {
            "aàáảãạăằắẳẵặâầấẩẫậäåāą",
            "AÀÁẢÃẠĂẰẮẲẴẶÂẦẤẨẪẬÄÅĀĄ",
            "cçćč",
            "CÇĆČ",
            "dđď",
            "DĐĎ",
            "eèéẻẽẹêềếểễệëěēę",
            "EÈÉẺẼẸÊỀẾỂỄỆËĚĒĘ",
            "iìíỉĩịîïī",
            "IÌÍỈĨỊÎÏĪ",
            "lł",
            "LŁ",
            "nñňń",
            "NÑŇŃ",
            "oòóỏõọôồốổỗộơởỡớờợöøō",
            "OÒÓỎÕỌÔỒỐỔỖỘƠỞỠỚỜỢÖØŌ",
            "rř",
            "RŘ",
            "sšśșş",
            "SŠŚȘŞ",
            "tťțţ",
            "TŤȚŢ",
            "uùúủũụưừứửữựûüůū",
            "UÙÚỦŨỤƯỪỨỬỮỰÛÜŮŪ",
            "yýỳỷỹỵÿ",
            "YÝỲỶỸỴŸ",
            "zžżź",
            "ZŽŻŹ"
        };

        public static string CreateDiacriticsRegExp(String str)
        {
            var flags   = CaseSensitive ? "gm" : "gmi";
            var classes = CaseSensitive ? DiacriticsCaseSensitive : Diacritics;
            var handled = new List<string>();

            foreach (var character in Script.Write<string[]>("Array.from({0})", str).Distinct())
            {
                foreach (var characterClass in classes)
                {
                    if (!characterClass.Contains(character)) continue;

                    if (!handled.Contains(characterClass))
                    {
                        handled.Add(characterClass);
                        str = str.replace(new es5.RegExp($"[{characterClass}]", flags), $"[{characterClass}]");
                    }
                    break;
                }
            }
            return str;
        }

        /// <summary>
        /// Escapes a string for usage within a regular expression
        /// </summary>
        private static string EscapeStr(string str)
        {
            return Script.Write<string>("{0}.replace(/[\\-\\[\\]\\/\\{\\}\\(\\)\\*\\+\\?\\.\\\\\\^\\$\\|]/g, '\\\\$&')", str);
        }

        /// <summary>
        /// Replaces each whitespace run in the pattern with one that matches one or more
        /// whitespace characters
        /// </summary>
        private static string CreateMergedBlanksRegExp(String str)
        {
            return str.replace(new es5.RegExp("[\\s]+", "gm"), "[\\s]+");
        }
    }
}
