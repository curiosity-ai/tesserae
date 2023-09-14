using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Build.UpdateInterfaceIcons
{
    class Program
    {
        static void Main()
        {
            if (!Directory.GetCurrentDirectory().EndsWith("Build.UpdateInterfaceIcons")) throw new InvalidOperationException("make sure to set the working directory to Build.UpdateInterfaceIcons");

            try
            {
                Directory.Delete("flaticon-uicons", recursive: true);
            }
            catch (DirectoryNotFoundException e)
            {
                //ignore
            }

            var p = Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName        = "git",
                Arguments       = "clone https://github.com/freepik-company/flaticon-uicons.git",
            });

            p.WaitForExit();

            Console.WriteLine("Cloned git repo");
            var webfontsDir = Path.Combine("flaticon-uicons", "src", "uicons", "webfonts");

            var tesseraeFontsDir = Path.Combine("..", "Tesserae", "h5", "assets", "fonts");
            var tesseraeCssDir   = Path.Combine("..", "Tesserae", "h5", "assets", "css");
            if (!Directory.Exists(tesseraeFontsDir)) throw new InvalidOperationException("tesserae dir does not exit");
            if (!Directory.Exists(tesseraeCssDir)) throw new InvalidOperationException("tesserae dir does not exit");

            foreach (var file in Directory.EnumerateFiles(webfontsDir))
            {
                if (file.EndsWith("rounded.woff2") || file.EndsWith("brands.woff2"))
                {
                    Console.WriteLine("Copying " + file);
                    System.IO.File.Copy(file, Path.Combine(tesseraeFontsDir, Path.GetFileName(file)), overwrite: true);
                }
            }

            var cssDir = Path.Combine("flaticon-uicons", "src", "uicons", "css");
            var icons  = new List<string>();

            foreach (string file in Directory.EnumerateFiles(cssDir, "*.*", SearchOption.AllDirectories))
            {
                if (File.Exists(file))
                {
                    if (file.EndsWith("rounded.css") || (file.Contains("brands") && file.EndsWith("all.css")))
                    {
                        string name;

                        if (file.Contains("/solid/")) name        = "uicons-solid-rounded";
                        else if (file.Contains("/regular/")) name = "uicons-regular-rounded";
                        else if (file.Contains("/bold/")) name    = "uicons-bold-rounded";
                        else if (file.Contains("/brands/")) name  = "uicons-brands";
                        else continue;

                        // rpalce line-height: 1; with line-height: inherit;

                        var lines = File.ReadAllLines(file);


                        for (int i = 0; i < lines.Length; i++)
                        {
                            var line = lines[i];

                            if (line.Contains("line-height: 1;"))
                            {
                                var startIndex = line.IndexOf("line-height: 1;");
                                var newLine    = line.Substring(0, startIndex) + "line-height: inherit;" + line.Substring(startIndex + "line-height: 1;".Length);
                                lines[i] = newLine;
                            }

                            if (line.Contains("""eot#iefix") format("embedded-opentype"),"""))
                            {
                                lines[i] = "";
                            }

                            if (line.Contains(""".woff") format("woff");"""))
                            {
                                lines[i] = "";
                            }

                            if (line.Contains(""".woff2") format("woff2"),"""))
                            {
                                lines[i] = $"""     src: url("../fonts/{name}.woff2") format("woff2"); """;
                            }

                            var iconLine = line.Trim();

                            if (iconLine.StartsWith(".fi-rr-") && iconLine.EndsWith(":before {"))
                            {
                                icons.Add(iconLine.Substring(".fi-rr-".Length).Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries).First());
                            }
                        }

                        File.WriteAllLines(Path.Combine(tesseraeCssDir, name + ".css"), lines);

                        Console.WriteLine("Copying " + file);
                    }
                }
            }

            var uiconsCsPath = Path.Combine("..", "Tesserae", "src", "Icons", "UIcons.cs");
            var allIcons     = icons.Distinct().OrderBy(i => i).ToArray();

            File.WriteAllText(uiconsCsPath, CreateEnum(allIcons));
            Console.WriteLine($"Parsed uicons-regular-rounded.css, found {allIcons.Length} icons.");


            try
            {
                Directory.Delete("flaticon-uicons", recursive: true);
            }
            catch (DirectoryNotFoundException e)
            {
                //ignore
            }
        }

        private static Dictionary<string, string> IconAliases = new Dictionary<string, string>
        {
            { "hand", "ThumbsDown" }
            { "social-network", "ThumbsUp" }
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