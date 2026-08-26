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
    /// <see cref="MarkOptions"/> adds whole-word boundaries, per-word matching, wildcards, a
    /// minimum keyword length, and folding of soft hyphens / zero-width joiners.
    /// Adapted from mark.js (https://github.com/julkue/mark.js, MIT).
    /// </summary>
    [Transpose.Name("tss.RegExpCreator")]
    public static class RegExpCreator
    {
        /// <summary>Default case sensitivity, used when <see cref="MarkOptions.CaseSensitive"/> is unset.</summary>
        public static bool CaseSensitive { get; set; } = false;

        // Placeholders for the pipeline: they survive EscapeStr and are turned back into their
        // pattern at the end, exactly like mark.js does it. The wildcard ones are spelled as the
        // regex escape for the placeholder character, ready for the RegExp that swaps them out.
        private const char   JOINER_PLACEHOLDER       = '\u0000';
        private const string WILDCARD_ONE_PLACEHOLDER = "\\u0001";
        private const string WILDCARD_ANY_PLACEHOLDER = "\\u0002";

        // Word characters for the whole-word boundaries: ASCII word chars plus the Latin ranges the
        // diacritics table covers, so 'café' bounded by punctuation still counts as one word
        private const string WORD_CHARACTER_CLASS = "0-9A-Za-z_\\u00C0-\\u024F\\u1E00-\\u1EFF";

        public static es5.RegExp Create(string str) => Create(str, null);

        /// <summary>
        /// Builds the marking pattern for a keyword. Returns null when nothing remains to match
        /// (empty input, or every word shorter than <see cref="MarkOptions.MinLength"/>).
        /// </summary>
        public static es5.RegExp Create(string str, MarkOptions options)
        {
            var caseSensitive = options?.CaseSensitive ?? CaseSensitive;
            var pattern       = CreatePattern(str, options, caseSensitive);

            if (pattern is null) return null;
            return new es5.RegExp(pattern, $"gm{(caseSensitive ? "" : "i")}");
        }

        private static string CreatePattern(string str, MarkOptions options, bool caseSensitive)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;

            var keywords = GetKeywords(str, options);
            if (keywords.Length == 0) return null;

            var parts   = keywords.Select(keyword => CreateKeywordPattern(keyword, options, caseSensitive)).ToArray();
            var pattern = (parts.Length == 1) ? parts[0] : "(?:" + string.Join("|", parts) + ")";

            if (options?.WholeWord ?? false)
            {
                pattern = $"(?<![{WORD_CHARACTER_CLASS}])(?:{pattern})(?![{WORD_CHARACTER_CLASS}])";
            }
            return pattern;
        }

        private static string[] GetKeywords(string str, MarkOptions options)
        {
            var separate  = options?.SeparateWordSearch ?? false;
            var minLength = options?.MinLength ?? 0;

            var keywords = separate
                ? str.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                : new[] { str };

            // Longest first, so with per-word search an overlapping shorter word never wins the match
            return keywords
               .Where(keyword => !string.IsNullOrWhiteSpace(keyword) && keyword.Length >= minLength)
               .Distinct()
               .OrderByDescending(keyword => keyword.Length)
               .ToArray();
        }

        private static string CreateKeywordPattern(string keyword, MarkOptions options, bool caseSensitive)
        {
            var wildcards     = options?.Wildcards ?? false;
            var ignoreJoiners = options?.IgnoreJoiners ?? false;
            var diacritics    = options?.Diacritics ?? true;

            var str = keyword;

            if (wildcards)     str = SetupWildcardsRegExp(str);
            str                    = EscapeStr(str);
            if (ignoreJoiners) str = SetupIgnoreJoinersRegExp(str);
            if (diacritics)    str = CreateDiacriticsRegExp(str, caseSensitive);
            str                    = CreateMergedBlanksRegExp(str);
            if (ignoreJoiners) str = CreateJoinersRegExp(str);
            if (wildcards)     str = CreateWildcardsRegExp(str);
            return str;
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

        public static string CreateDiacriticsRegExp(String str) => CreateDiacriticsRegExp(str, CaseSensitive);

        private static string CreateDiacriticsRegExp(String str, bool caseSensitive)
        {
            var flags   = caseSensitive ? "gm" : "gmi";
            var classes = caseSensitive ? DiacriticsCaseSensitive : Diacritics;
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

        /// <summary>
        /// Turns unescaped '*' and '?' into placeholders that survive escaping; a backslash-escaped
        /// wildcard stays a literal character
        /// </summary>
        private static string SetupWildcardsRegExp(string str)
        {
            str = Script.Write<string>("{0}.replace(/(?:\\\\)*\\?/g, (val) => val.charAt(0) === '\\\\' ? '?' : '\\u0001')", str);
            return Script.Write<string>("{0}.replace(/(?:\\\\)*\\*/g, (val) => val.charAt(0) === '\\\\' ? '*' : '\\u0002')", str);
        }

        private static string CreateWildcardsRegExp(String str)
        {
            str = str.replace(new es5.RegExp(WILDCARD_ONE_PLACEHOLDER, "g"), "[^\\s]?");
            return str.replace(new es5.RegExp(WILDCARD_ANY_PLACEHOLDER, "g"), "[^\\s]*");
        }

        /// <summary>
        /// Inserts a placeholder between the pattern's characters (skipping escape sequences,
        /// group syntax, and whitespace - a placeholder inside a blank run would keep
        /// <see cref="CreateMergedBlanksRegExp"/> from merging it), which
        /// <see cref="CreateJoinersRegExp"/> later widens to an optional joiner character
        /// </summary>
        private static string SetupIgnoreJoinersRegExp(string str)
        {
            return Script.Write<string>("{0}.replace(/[^\\s(|)\\\\]/g, (val, indx, original) => (/[\\s(|)\\\\]/.test(original.charAt(indx + 1)) || original.charAt(indx + 1) === '') ? val : val + '\\u0000')", str);
        }

        private static string CreateJoinersRegExp(string str)
        {
            // Soft hyphen, zero-width space, and the zero-width (non-)joiners
            return string.Join("[\\u00ad\\u200b\\u200c\\u200d]?", str.Split(JOINER_PLACEHOLDER));
        }
    }
}
