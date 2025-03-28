﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Build.InjectSamplesCode
{
    static class Program
    {
        static void Main()
        {
            var samplesFolder = @"..\Tesserae.Tests\src\Samples\";

            var allFiles = Directory.GetFiles(samplesFolder, "*.cs", SearchOption.AllDirectories);

            var allCode = GetCode(allFiles);

            File.WriteAllText(@"..\Tesserae.Tests\src\SamplesSourceCode.cs", CreateCode(allCode));
            Console.WriteLine($"Parsed samples code, found {allCode.Count} samples.");
        }

        private static readonly Regex RE_GetName = new Regex(@"SampleHeader\(nameof\(([^)]*?)\)");
        private static Dictionary<string, string> GetCode(string[] files)
        {
            var dict = new Dictionary<string, string>();

            foreach (var f in files)
            {
                var code  = File.ReadAllText(f);
                var match = RE_GetName.Match(code);

                if (match.Success)
                {
                    var name = match.Groups[1].Value;
                    dict[name] = code;
                }
            }
            return dict;
        }

        private static string CreateCode(Dictionary<string, string> code)
        {
            var sb = new StringBuilder();
            sb.AppendLine("//This file is automatically generated during the build process").AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("namespace Tesserae.Tests");
            sb.AppendLine("{").AppendLine();
            sb.AppendLine("    internal class SamplesSourceCode");
            sb.AppendLine("    {");
            sb.AppendLine("        public static string GetCodeForSample(string sampleName)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch(sampleName)");
            sb.AppendLine("            {");

            foreach (var kv in code)
            {
                sb.AppendLine("                case \"§§§§\": return \"$$$$\";".Replace("§§§§", kv.Key).Replace("$$$$", EscapeCode(kv.Value)));
            }
            sb.AppendLine("                default: return \"Missing sample code\";");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string EscapeCode(string value)
        {
            const string beginMarker = "//begin-sample-code";
            const string endMarker   = "//end-sample-code";

            if (value.Contains(beginMarker) && value.Contains(endMarker))
            {
                var innerCode = value.Replace(beginMarker, "⁋").Split(new char[] { '⁋' }, 2).Skip(1).First()
                   .Replace(endMarker, "⁋").Split(new char[] { '⁋' }, 2).First();


                innerCode =
                    @"
// -- Necessary 'using' headers 
// using System;
// using H5;
// using static H5.dom;
// using Tesserae;
// using static Tesserae.UI;

"
                   +
                    RemovePadding(innerCode);

                return EscapeCode(innerCode);
            }
            else
            {
                return value.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"");
            }
        }

        private static string RemovePadding(string innerCode)
        {
            return string.Join("\r\n", innerCode.Split(new char[] { '\r', '\n' }).Select(l => l.TrimStart(' ', '\t')));
        }
    }
}