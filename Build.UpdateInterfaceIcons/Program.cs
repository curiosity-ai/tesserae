using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Build.UpdateInterfaceIcons
{
    class Program
    {
        static async Task<string> FetchVersion()
        {
            Console.WriteLine($"Fetching Version");
            var updateFontsJsUrlFromGithub = "https://raw.githubusercontent.com/freepik-company/flaticon-uicons/main/utils/update-fonts.js";

            using var client = new HttpClient();
            var       s      = await client.GetStringAsync(updateFontsJsUrlFromGithub);

            foreach (var line in s.Split("\n"))
            {
                var prefix = "const CDN_URL = 'https://cdn-uicons.flaticon.com/";

                if (line.StartsWith(prefix))
                {
                    return line.Substring(prefix.Length, line.Length - prefix.Length - "';".Length);
                }
            }

            throw new Exception("version not found");
        }


        static async Task Main()
        {
            var version = await FetchVersion();

            var tempDir = Path.GetTempPath();

            if (!Directory.GetCurrentDirectory().EndsWith("Build.UpdateInterfaceIcons")) throw new InvalidOperationException("make sure to set the working directory to Build.UpdateInterfaceIcons");


            var types = new string[]
            {
                "uicons-brands",
                "uicons-regular-straight",
                "uicons-regular-rounded",
                "uicons-bold-straight",
                "uicons-bold-rounded",
                "uicons-solid-rounded",
                "uicons-solid-straight",
                "uicons-thin-straight",
                "uicons-thin-rounded"
            };


            var tesseraeFontsDir = Path.Combine("..", "Tesserae", "h5", "assets", "fonts");
            var tesseraeCssDir   = Path.Combine("..", "Tesserae", "h5", "assets", "css");
            if (!Directory.Exists(tesseraeFontsDir)) throw new InvalidOperationException("tesserae dir does not exit");
            if (!Directory.Exists(tesseraeCssDir)) throw new InvalidOperationException("tesserae dir does not exit");


            Console.WriteLine("download fonts");

            foreach (var type in types)
            {
                var fontFileName = $"{type}.woff2";

                await DownloadFileAsync(GetWoff2Url(version, type), Path.Combine(tempDir, fontFileName));
                System.IO.File.Copy(Path.Combine(tempDir, fontFileName), Path.Combine(tesseraeFontsDir, fontFileName), overwrite: true);
            }

            Console.WriteLine("download css");

            foreach (var type in types)
            {
                await DownloadFileAsync(GetCssUrl(version, type), Path.Combine(tempDir, $"{type}.css"));
            }

            var icons = new Dictionary<string, List<string>>();

            var ps = Path.DirectorySeparatorChar;

            foreach (var type in types)
            {
                var file = Path.Combine(tempDir, $"{type}.css");

                Console.WriteLine("Parsing CSS: " + file);

                bool isRegularRounded = Path.GetFileName(file) == "uicons-regular-rounded.css";

                // rpalce line-height: 1; with line-height: inherit;

                var lines = File.ReadAllLines(file);

                Console.WriteLine($"Found {lines.Length} lines in CSS {file}.");
                var extraLines = new List<string>();

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    foreach (var (replace, with) in IconsToFixInCss)
                    {
                        if (line.Contains(replace))
                        {
                            line     = line.Replace(replace, with);
                            lines[i] = line;
                        }
                    }

                    if (line.Contains("line-height: 1;"))
                    {
                        var startIndex = line.IndexOf("line-height: 1;");
                        var newLine    = line.Substring(0, startIndex) + "line-height: inherit;" + line.Substring(startIndex + "line-height: 1;".Length);
                        lines[i] = newLine;
                    }

                    if (line.Contains("""eot#iefix") format("embedded-opentype")"""))
                    {
                        lines[i] = "";
                    }

                    if (line.Contains(""".woff") format("woff")"""))
                    {
                        lines[i] = "";
                    }

                    if (line.Contains(""".woff2") format("woff2")"""))
                    {
                        lines[i] = $"""     src: url("../fonts/{type}.woff2") format("woff2"); """;
                    }

                    var iconLine = line.Trim();

                    if (iconLine.StartsWith(".fi") && iconLine.EndsWith(":before {"))
                    {
                        var prefix = IconPrefixes.First(p => iconLine.Contains($".fi-{p}-"));

                        string iconName = iconLine.Substring($".fi-{prefix}-".Length).Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).First();


                        var typesIconList = icons.GetValueOrDefault(iconName, new List<string>());
                        typesIconList.Add(type);
                        icons[iconName] = typesIconList;
                        Console.WriteLine($"Found icon {iconName} in {type}");

                        if (isRegularRounded && ExportAsVariables.Contains(iconName))
                        {
                            var contentLineParts = lines[i + 1].Trim().Split(new char[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
                            var contentValue     = contentLineParts[1];

                            extraLines.Add($"--uicon-var-{iconName}: '{contentValue}';");
                            Console.WriteLine($"Exporting CSS variable --uicon-var-{iconName}: '{contentValue}';");
                        }
                    }
                }

                if (extraLines.Count > 0)
                {
                    extraLines.Insert(0, ":root {");
                    extraLines.Add("}");
                }


                File.WriteAllLines(Path.Combine(tesseraeCssDir, type + ".css"), extraLines.Concat(lines));

                Console.WriteLine("Copying " + file);
            }

            Console.WriteLine($"Found {icons.Count} icons from css");

            var uiconsCsPath = Path.Combine("..", "Tesserae", "src", "Icons", "UIcons.cs");
            var allIcons     = icons.OrderBy(i => i.Key).ToArray();


            foreach (var i in icons)
            {
                if (!i.Value.Contains(_brandsPrefix) && i.Value.Count != (IconPrefixes.Length - 1))
                {
                    Console.WriteLine($"icon {i.Key} does not have all versions. It has : {string.Join(",", i.Value)}");
                }
            }


            File.WriteAllText(uiconsCsPath, CreateEnum(
                allIcons.Where(i => !i.Value.Any(v => v.Contains(_brandsPrefix))).Select(i => i.Key).ToArray(),
                allIcons.Where(i => i.Value.Any(v => v.Contains(_brandsPrefix))).Select(i => i.Key).ToArray()
            ));
            Console.WriteLine($"Parsed css files, found {allIcons.Length} icons.");
        }




        public static async Task DownloadFileAsync(string url, string filename)
        {
            Console.WriteLine($"Downloading {url} to {filename}");

            using var client = new HttpClient();
            using var s      = await client.GetStreamAsync(url);
            using var fs     = new FileStream(filename, FileMode.Create);
            await s.CopyToAsync(fs);
        }

        public static string GetCssUrl(string version, string type)
        {
            return $"https://cdn-uicons.flaticon.com/{version}/{type}/css/{type}.css";
        }

        public static string GetWoff2Url(string version, string type)
        {
            return $"https://cdn-uicons.flaticon.com/{version}/{type}/webfonts/{type}.woff2";
        }

        public static string GetWoffUrl(string version, string type)
        {
            return $"https://cdn-uicons.flaticon.com/{version}/{type}/webfonts/{type}.woff";
        }

        public static string GetEmbeddedOpenTypeUrl(string version, string type)
        {
            return $"https://cdn-uicons.flaticon.com/{version}/{type}/webfonts/{type}.eot#iefix";
        }

        private const string _brandsPrefix           = "brands";
        private const string _regularRoundPrefix     = "rr";
        private const string _solidRoundPrefix       = "sr";
        private const string _thinRoundPrefix        = "tr";
        private const string _boldRoundPrefix        = "br";
        private const string _regularStraiightPrefix = "rs";
        private const string _boldStraiightPrefix    = "bs";
        private const string _solidStraiightPrefix   = "ss";
        private const string _thinStraiightPrefix    = "ts";

        public static readonly string[] IconPrefixes = new string[]
        {
            _brandsPrefix,
            _boldRoundPrefix,
            _thinRoundPrefix,
            _solidRoundPrefix,
            _regularRoundPrefix,
            _regularStraiightPrefix,
            _boldStraiightPrefix,
            _solidStraiightPrefix,
            _thinStraiightPrefix
        };

        private static Dictionary<string, string> IconsToFixInCss = new Dictionary<string, string>
        {
            { "-social-network:before", "-thumbs-up:before" },
            { "-hand:before", "-thumbs-down:before" },
        };

        private static HashSet<string> ExportAsVariables = new HashSet<string>()
        {
            "checkbox",
            "square",
            "sidebar",
            "sidebar-flip",
            "angle-right",
            "angle-left",
            "angle-top",
            "angle-bottom",
            "slash",
            "lock",
            "lock-open-alt",
            "unlock",
            "upload",
            "download",
            "cloud-upload-alt",
            "cloud-upload",
            "refresh"
        };

        private static string CreateEnum(string[] iconsRegular, string[] iconsBrands)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using H5;").AppendLine();
            sb.AppendLine("namespace Tesserae");
            sb.AppendLine("{").AppendLine();
            sb.AppendLine("    [Enum(Emit.Value)]");
            sb.AppendLine("    public enum UIcons");
            sb.AppendLine("    {");

            var maxLen = new[] { iconsBrands.Max(l => "fi-brands-".Length + l.Length), iconsRegular.Max(l => "fi-rr-".Length + l.Length) }.Max() + "        [Name(\"\")] ".Length;

            sb.Append(("        [Name(\"fi-rr-default-empty\")] ").PadRight(maxLen, ' '));
            sb.AppendLine($"Default,");

            foreach (var i in iconsRegular)
            {
                sb.Append(("        [Name(\"fi-rr-" + i + "\")] ").PadRight(maxLen, ' '));
                sb.AppendLine($"{ToValidName(i)},");

                //if (IconAliases.ContainsKey(i))
                //{
                //    sb.Append(("        [Name(\"fi-rr-" + i + "\")] ").PadRight(maxLen, ' '));
                //    sb.AppendLine($"{IconAliases[i]},");
                //}
            }

            foreach (var i in iconsBrands)
            {
                sb.Append(("        [Name(\"fi-brands-" + i + "\")] ").PadRight(maxLen, ' '));
                sb.AppendLine($"{ToValidBrandsName(i)},");

                //if (IconAliases.ContainsKey(i))
                //{
                //    sb.Append(("        [Name(\"fi-rr-" + i + "\")] ").PadRight(maxLen, ' '));
                //    sb.AppendLine($"{IconAliases[i]},");
                //}
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string ToValidBrandsName(string icon)
        {
            var words = icon.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(i => i.Substring(0, 1).ToUpper() + i.Substring(1))
               .ToArray();

            var name = string.Join("", words);

            if (char.IsDigit(name[0]))
            {
                return "Brands" + "_" + name;
            }
            else
            {
                return "Brands" + name;
            }
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
