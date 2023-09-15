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

            var p = Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName        = "git",
                Arguments       = "clone https://github.com/freepik-company/flaticon-uicons.git",
                WorkingDirectory = Path.GetTempPath()
            });

            p.WaitForExit();

            Console.WriteLine("Cloned git repo");
            var repoDir = Path.Combine(Path.GetTempPath(), "flaticon-uicons");

            var webfontsDir = Path.Combine(repoDir, "src", "uicons", "webfonts");

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

            var cssDir = Path.Combine(repoDir, "src", "uicons", "css");
            var icons  = new HashSet<string>();
            
            var ps = Path.DirectorySeparatorChar;

            foreach (var file in Directory.EnumerateFiles(cssDir, "*.*", SearchOption.AllDirectories))
            {
                if (file.Contains($"{ps}all{ps}")) continue;

                if (file.EndsWith("rounded.css") || (file.Contains("brands") && file.EndsWith("all.css")))
                {
                    Console.WriteLine("Parsing CSS: " + file);

                    string name;

                    if (file.Contains($"{ps}solid{ps}"))        { name = "uicons-solid-rounded"; }
                    else if (file.Contains($"{ps}regular{ps}")) { name = "uicons-regular-rounded"; }
                    else if (file.Contains($"{ps}bold{ps}"))    { name = "uicons-bold-rounded"; }
                    else if (file.Contains($"{ps}brands{ps}"))  { name = "uicons-brands"; }
                    else { continue; }

                    bool isRegularRounded = file.Contains($"{ps}regular{ps}") && file.EndsWith("rounded.css");

                    // rpalce line-height: 1; with line-height: inherit;

                    var lines = File.ReadAllLines(file);

                    Console.WriteLine($"Found {lines.Length} lines in CSS {file}.");
                    var extraLines = new List<string>();
                    
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

                        if ((iconLine.StartsWith(".fi-rr-") || iconLine.StartsWith(".fi-br-") || iconLine.StartsWith(".fi-sr-") ) && iconLine.EndsWith(":before {"))
                        {
                            var iconName = iconLine.Substring(".fi-rr-".Length).Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries).First();
                            if (icons.Add(iconName))
                            {
                                Console.WriteLine($"Found icon {iconName}");

                                if (isRegularRounded && ExportAsVariables.Contains(iconName))
                                {
                                    var contentLineParts = lines[i + 1].Trim().Split(new char[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
                                    var contentValue = contentLineParts[1];

                                    extraLines.Add($"--var-{iconName}: {contentValue};");
                                }
                            }
                        }
                    }

                    if (extraLines.Count > 0)
                    {
                        extraLines.Insert(0, ":root {");
                        extraLines.Add("}");
                    }

                    File.WriteAllLines(Path.Combine(tesseraeCssDir, name + ".css"), extraLines.Concat(lines));

                    Console.WriteLine("Copying " + file);
                }
            }

            Console.WriteLine($"Found {icons.Count} icons from css");

            var uiconsCsPath = Path.Combine("..", "Tesserae", "src", "Icons", "UIcons.cs");
            var allIcons     = icons.OrderBy(i => i).ToArray();

            File.WriteAllText(uiconsCsPath, CreateEnum(allIcons));
            Console.WriteLine($"Parsed css files, found {allIcons.Length} icons.");
        }

        private static Dictionary<string, string> IconAliases = new Dictionary<string, string>
        {
            //Example: (not needed anymore on latest uicons version): { "social-network", "ThumbsUp" }
        };

        private static HashSet<string> ExportAsVariables = new HashSet<string>()
        {
            "checkbox",
            "square",
            "sidebar",
            "sidebar-flip",
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