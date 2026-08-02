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
        private const string MIN_Version = "4.0.0";

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

                    var versionFetched = new Version(line.Substring(prefix.Length, line.Length - prefix.Length - "';".Length));
                    var versionMin     = new Version(MIN_Version);

                    return versionFetched > versionMin ? versionFetched.ToString() : versionMin.ToString();
                }
            }

            throw new Exception("version not found");
        }


        static async Task<int> Main(string[] args)
        {
            var options = CenteringOptions.Parse(args);

            if (options.ShowHelp)
            {
                CenteringOptions.PrintUsage();
                return 0;
            }

            var repoRoot = OpticalCenteringStage.LocateRepositoryRoot();
            var tempDir  = Path.Combine(Path.GetTempPath(), "uicons-download");
            Directory.CreateDirectory(tempDir);


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


            var tesseraeFontsDir = Path.Combine(repoRoot, "Tesserae", "tps", "assets", "fonts");
            var tesseraeCssDir   = Path.Combine(repoRoot, "Tesserae", "tps", "assets", "css");
            if (!Directory.Exists(tesseraeFontsDir)) throw new InvalidOperationException($"no fonts directory at {tesseraeFontsDir}");
            if (!Directory.Exists(tesseraeCssDir)) throw new InvalidOperationException($"no css directory at {tesseraeCssDir}");

            // Deliberately not in the assets tree: tps.json bundles tps/assets/fonts/* into the package,
            // and this is build metadata, not something consumers should receive.
            var marker = Path.Combine(repoRoot, "Build.UpdateInterfaceIcons", SourceMarkerFile);

            if (options.CentreOnly)
            {
                // Right now the fonts in the tree are whatever is on disk; fingerprint them before they are
                // edited, so the marker still records what the tree was built from.
                var inTree = SourceFingerprint(VersionFromStylesheets(tesseraeCssDir),
                                               types.Select(t => Path.Combine(tesseraeFontsDir, $"{t}.woff2")));

                if (!options.Force && File.Exists(marker) && !SameFingerprint(File.ReadAllText(marker), inTree))
                {
                    Console.WriteLine("The fonts in the tree do not match the marker, which means they have already been");
                    Console.WriteLine("centred. Centring them again would shift the outlines a second time. Re-download");
                    Console.WriteLine("first, or pass --force if you know the fonts are pristine.");
                    return 1;
                }

                Console.WriteLine("Skipping the download, centring the fonts already in the tree.");
                var centredOnly = await OpticalCenteringStage.RunAsync(repoRoot, options);

                if (centredOnly) File.WriteAllText(marker, inTree);
                return centredOnly ? 0 : 1;
            }

            var version = await FetchVersion();

            Console.WriteLine("download fonts");

            foreach (var type in types)
            {
                await DownloadFileAsync(GetWoff2Url(version, type), Path.Combine(tempDir, $"{type}.woff2"));
            }

            // The fonts in the tree have had the optical centering baked into their outlines, so they no
            // longer match the vendor bytes. This records what was downloaded last time instead, which is
            // what makes "only run when the icon set actually changed" answerable.
            var downloaded = SourceFingerprint(version, types.Select(t => Path.Combine(tempDir, $"{t}.woff2")));

            if (!options.Force && File.Exists(marker) && SameFingerprint(File.ReadAllText(marker), downloaded))
            {
                Console.WriteLine();
                Console.WriteLine($"UIcons {version} is already what the tree was built from, and the woff2 files are byte for byte");
                Console.WriteLine("the same as last time. Nothing to do - the fonts in the tree are already centred.");
                Console.WriteLine("Pass --force to rebuild them anyway.");
                return 0;
            }

            foreach (var type in types)
            {
                System.IO.File.Copy(Path.Combine(tempDir, $"{type}.woff2"), Path.Combine(tesseraeFontsDir, $"{type}.woff2"), overwrite: true);
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

            var uiconsCsPath = Path.Combine(repoRoot, "Tesserae", "src", "Icons", "UIcons.cs");
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

            // Last, because it edits the fonts that were just written and keys off the codepoints in the
            // stylesheets that were just parsed. Both change with every UIcons release.
            var centred = await OpticalCenteringStage.RunAsync(repoRoot, options);

            if (!centred)
            {
                Console.WriteLine();
                Console.WriteLine("Optical centering failed its checks, so the source marker was not written:");
                Console.WriteLine("the next run will start over rather than treat these fonts as finished.");
                return 1;
            }

            File.WriteAllText(marker, downloaded);
            Console.WriteLine();
            Console.WriteLine($"UIcons {version} downloaded, centred and recorded in {Path.GetFileName(marker)}.");
            return 0;
        }

        /// <summary>File recording which vendor download the fonts in the tree were built from.</summary>
        private const string SourceMarkerFile = "uicons-source.txt";

        /// <summary>
        /// Compares two fingerprints ignoring line endings. The marker is committed and checked out again,
        /// and a Windows agent with core.autocrlf turns its newlines into CRLF - comparing the raw text
        /// would then never match, and the whole set would be rebuilt on every single build.
        /// </summary>
        private static bool SameFingerprint(string a, string b) =>
            a.Replace("\r\n", "\n").Trim() == b.Replace("\r\n", "\n").Trim();

        /// <summary>The UIcons version, read out of the banner the vendor puts in every stylesheet.</summary>
        private static string VersionFromStylesheets(string cssDir)
        {
            foreach (var file in Directory.GetFiles(cssDir, "uicons-*.css"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(File.ReadAllText(file), @"UIcons (\d+\.\d+\.\d+)");
                if (match.Success) return match.Groups[1].Value;
            }

            throw new InvalidOperationException($"could not find the UIcons version banner in any stylesheet under {cssDir}");
        }

        /// <summary>The version plus a hash of every downloaded woff2, so any change to the set shows up.</summary>
        private static string SourceFingerprint(string version, IEnumerable<string> woff2Files)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var parts = new List<string> { $"uicons {version}" };

            foreach (var file in woff2Files.OrderBy(f => f, StringComparer.Ordinal))
            {
                var hash = Convert.ToHexString(sha.ComputeHash(File.ReadAllBytes(file))).ToLowerInvariant();
                parts.Add($"{Path.GetFileName(file)} {hash[..16]}");
            }

            return string.Join("\n", parts);
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
            { "-hastag:before", "-hashtag:before" },
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
            "refresh",
            "square-a",
            "thumbtack",
            "thumbtack-slash",
            "heart",
            "heart-slash",
            "bookmark",
            "bookmark-slash",
            "thumbs-up",
            "thumbs-down",
            "block",
            "sparkles",
        };

        private static string CreateEnum(string[] iconsRegular, string[] iconsBrands)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Transpose;").AppendLine();
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
