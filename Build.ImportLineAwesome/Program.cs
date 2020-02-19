using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Build.ImportLineAwesome
{
    class Program
    {
        static void Main(string[] args)
        {
            var css = File.ReadAllLines(@".\FromGit\line-awesome\dist\line-awesome\css\line-awesome.css");

            var icons = css.Where(l => l.StartsWith(".la-") && l.EndsWith(":before {"))
                           .Select(l => l.Substring(".la-".Length).Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries).First())
                           .OrderBy(i => i)
                           .ToArray();

            File.WriteAllText(@"..\Tesserae\src\Icons\LineAwesome.cs", CreateEnum(icons));
            Console.WriteLine($"Parsed line-awesome.css, found {icons.Length} icons.");
        }

        private static string CreateEnum(string[] icons)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Bridge;").AppendLine();
            sb.AppendLine("namespace Tesserae");
            sb.AppendLine("{").AppendLine();
            sb.AppendLine("    [Enum(Emit.Value)]");
            sb.AppendLine("    public enum LineAwesome");
            sb.AppendLine("    {");
            var maxLen = icons.Max(l => l.Length) + "        [Name(\"\"] ".Length + 1;
            foreach (var i in icons)
            {
                sb.Append(("        [Name(\"la-" + i + "\")] ").PadRight(maxLen, ' '));
                sb.AppendLine($"{ToValidName(i)},");
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string ToValidName(string icon)
        {
            var words = icon.Split(new char []{ '-' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(i => i.Substring(0,1).ToUpper() + i.Substring(1))
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
