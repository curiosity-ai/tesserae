using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Build.ImportInterfaceIcons
{
    class Program
    {
        static void Main()
        {
            var filePath = @"..\Tesserae\h5\assets\css\uicons-regular-rounded.css";
            filePath = string.Join(Path.DirectorySeparatorChar, filePath.Split("\\"));

            if (!File.Exists(filePath)) throw new ArgumentException("File does not exist: " + filePath);

            var css = File.ReadAllLines(filePath);

            var icons = css.Select(l => l.Trim())
               .Where(l => l.StartsWith(".fi-rr-") && l.EndsWith(":before {"))
               .Select(l => l.Substring(".fi-rr-".Length).Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries).First())
               .OrderBy(i => i)
               .ToArray();


            var uiconsCsPath = @"..\Tesserae\src\Icons\UIcons.cs";
            uiconsCsPath = string.Join(Path.DirectorySeparatorChar, uiconsCsPath.Split("\\"));

            File.WriteAllText(uiconsCsPath, CreateEnum(icons));
            Console.WriteLine($"Parsed uicons-regular-rounded.css, found {icons.Length} icons.");
        }

        private static Dictionary<string, string> IconAliases = new Dictionary<string, string>
        {
            { "hand", "ThumbsDown" }
        };

        private static string CreateEnum(string[] icons)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using H5;").AppendLine();
            sb.AppendLine("namespace Tesserae");
            sb.AppendLine("{").AppendLine();
            sb.AppendLine("    [Enum(Emit.Value)]");
            sb.AppendLine("    public enum UIcons");
            sb.AppendLine("    {");
            var maxLen = icons.Max(l => l.Length) + "        [Name(\"\"] ".Length + 1;

            foreach (var i in icons)
            {
                sb.Append(("        [Name(\"fi-rr-" + i + "\")] ").PadRight(maxLen, ' '));
                sb.AppendLine($"{ToValidName(i)},");

                if (IconAliases.ContainsKey(i))
                {
                    sb.Append(("        [Name(\"fi-rr-" + i + "\")] ").PadRight(maxLen, ' '));
                    sb.AppendLine($"{IconAliases[i]},");
                }
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string ToValidName(string icon)
        {
            var words = icon.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(i => i.Substring(0, 1).ToUpper() + i.Substring(1))
               .ToArray();

            var name = string.Join("", words);

            if (char.IsDigit(name[0]))
            {
                return "_" + name;
            }
            else
            {
                return name;
            }
        }
    }
}