using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Build.UIconsOpticalCentering
{
    /// <summary>
    /// Renders every bundled UIcons glyph in a browser, one by one, works out how far each one is from
    /// being optically centred in the box it is laid out in, and writes the corrections to
    /// <c>Tesserae/tps/assets/css/tss.uicons.adjustments.css</c>.
    /// </summary>
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var options = Options.Parse(args);

            if (options.ShowHelp)
            {
                Options.PrintUsage();
                return 0;
            }

            var repoRoot = LocateRepositoryRoot();
            var cssDir   = Path.Combine(repoRoot, "Tesserae", "tps", "assets", "css");
            var assets   = Path.Combine(repoRoot, "Tesserae", "tps", "assets");
            var output   = Path.Combine(cssDir, "tss.uicons.adjustments.css");

            if (!Directory.Exists(cssDir)) throw new InvalidOperationException($"Could not find the bundled stylesheets at {cssDir}");

            Console.WriteLine($"Reading icon fonts from {cssDir}");
            var fonts = IconFontReader.ReadAll(cssDir);

            if (options.OnlyFonts.Count > 0)
            {
                fonts = fonts.Where(f => options.OnlyFonts.Any(o => f.FontFamily.Contains(o, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            foreach (var font in fonts) Console.WriteLine($"  {font.FontFamily,-26} {font.Glyphs.Count,5} glyphs  ({font.ClassPrefix}*)");

            if (fonts.Count == 0) throw new InvalidOperationException("No icon fonts to measure.");

            using var server = new AssetServer(assets);
            server.AddPage(MeasurementPage.Path, MeasurementPage.BuildHtml(fonts));
            Console.WriteLine($"Serving {assets} at {server.BaseUrl}");

            var warnings    = new List<string>();
            var adjustments = new List<FontAdjustments>();

            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = !options.Headed });

            var page = await browser.NewPageAsync(new BrowserNewPageOptions
            {
                ViewportSize      = new ViewportSize { Width = 1280, Height = 900 },
                DeviceScaleFactor = 1,
            });

            page.SetDefaultTimeout(0);
            page.Console += (_, message) => Console.WriteLine($"  [browser] {message.Text}");

            var pageUrl = server.BaseUrl + MeasurementPage.Path;
            Console.WriteLine($"Opening {pageUrl}");
            await page.GotoAsync(pageUrl, new PageGotoOptions { WaitUntil = WaitUntilState.Load });

            foreach (var font in fonts)
            {
                var measurement = await MeasureFont(page, font, options.Settings, options.ChunkSize);
                adjustments.Add(OpticalCentering.Compute(font, measurement, options.Settings, warnings));
            }

            var rules = AdjustmentCss.BuildRules(adjustments, options.Settings);
            var css   = AdjustmentCss.Render(rules, adjustments, options.Settings);

            var ok = CoherenceReport.Print(adjustments, options.Settings);
            ok &= await VerifyCssApplies(page, css, adjustments, options.Settings);

            if (warnings.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"{warnings.Count} warnings:");
                foreach (var warning in warnings.Take(40)) Console.WriteLine($"  {warning}");
                if (warnings.Count > 40) Console.WriteLine($"  ... and {warnings.Count - 40} more");
            }

            if (options.Preview)
            {
                await WritePreview(page, css, adjustments, Path.Combine(AppContext.BaseDirectory, "preview"));
            }

            if (options.DumpPath != null)
            {
                MeasurementDump.Write(options.DumpPath, adjustments);
                Console.WriteLine();
                Console.WriteLine($"Wrote measurements {options.DumpPath}");
            }

            File.WriteAllText(output, css, new UTF8Encoding(false));

            Console.WriteLine();
            Console.WriteLine($"Wrote {output}");
            Console.WriteLine($"  {rules.Sum(r => r.Selectors.Count)} icons in {rules.Count} groups, {new FileInfo(output).Length / 1024} KB");
            Console.WriteLine(ok ? "All checks passed." : "Some checks FAILED, see above.");

            return ok ? 0 : 1;
        }

        /// <summary>Rasterizes and measures a font's glyphs in chunks, so progress is visible and no single call runs long.</summary>
        private static async Task<FontMeasurement> MeasureFont(IPage page, IconFont font, CenteringSettings settings, int chunkSize)
        {
            FontMeasurement measurement = null;

            for (int start = 0; start < font.Glyphs.Count; start += chunkSize)
            {
                var chunk = font.Glyphs.Skip(start).Take(chunkSize).ToArray();

                var csv = await page.EvaluateAsync<string>(MeasurementPage.MeasureFontScript, new
                {
                    family       = font.FontFamily,
                    em           = settings.RasterEm,
                    trim         = settings.Trim,
                    inkThreshold = settings.InkThreshold,
                    maxCanvas    = settings.MaxCanvas,
                    glyphs       = chunk.Select(g => new { n = g.IconName, c = g.CodePoint }).ToArray(),
                });

                var parsed = FontMeasurement.Parse(csv);

                if (measurement is null)
                {
                    measurement = parsed;
                }
                else
                {
                    measurement.Glyphs.AddRange(parsed.Glyphs);
                }

                Console.Write($"\r  measuring {font.FontFamily,-26} {measurement.Glyphs.Count,5}/{font.Glyphs.Count}");
            }

            Console.WriteLine($"\r  measured  {font.FontFamily,-26} {measurement.Glyphs.Count,5}/{font.Glyphs.Count}   " +
                              $"ascent {measurement.Ascent:0.0} descent {measurement.Descent:0.0} " +
                              $"cell {measurement.CellWidth}x{measurement.CellHeight}px");

            return measurement;
        }

        /// <summary>
        /// Loads the generated stylesheet in the browser and reads the offsets back off the icons, which
        /// proves the selectors match real icon markup and that the em values resolve to the intended pixels.
        /// </summary>
        private static async Task<bool> VerifyCssApplies(IPage page, string css, List<FontAdjustments> fonts, CenteringSettings settings)
        {
            const double probeFontSize = 100;

            var pinned = new HashSet<string>(AlignmentGroups.All.SelectMany(g => g.Icons), StringComparer.Ordinal);

            var adjusted = fonts.SelectMany(f => f.Glyphs).Where(g => g.IsAdjusted).ToList();

            // Every pinned icon, plus an even sample of the rest.
            var sample = adjusted.Where(g => pinned.Contains(g.Glyph.IconName)).ToList();
            var stride = Math.Max(1, adjusted.Count / 500);
            sample.AddRange(adjusted.Where((_, i) => i % stride == 0));
            sample = sample.Distinct().ToList();

            if (sample.Count == 0)
            {
                Console.WriteLine();
                Console.WriteLine("Nothing to verify: no icon needed an adjustment.");
                return true;
            }

            var readback = await page.EvaluateAsync<string>(MeasurementPage.ReadAppliedOffsetsScript, new
            {
                css,
                classNames = sample.Select(g => g.Glyph.CssClass).ToArray(),
            });

            var applied = readback
               .Split('\n', StringSplitOptions.RemoveEmptyEntries)
               .Select(line => line.Split(';'))
               .ToDictionary(parts => parts[0], parts => (Position: parts[1], Left: Pixels(parts[2]), Top: Pixels(parts[3])), StringComparer.Ordinal);

            var problems = new List<string>();

            foreach (var glyph in sample)
            {
                if (!applied.TryGetValue(glyph.Glyph.CssClass, out var actual))
                {
                    problems.Add($"{glyph.Glyph.CssClass}: no computed style came back");
                    continue;
                }

                if (actual.Position != "relative") problems.Add($"{glyph.Glyph.CssClass}: position is '{actual.Position}', expected 'relative'");

                Check("left", actual.Left, glyph.X);
                Check("top", actual.Top, glyph.Y);

                // The offsets resolve through round(..., 1px), so the computed value must be a whole number
                // of pixels, and the nearest one to the em value asked for. Checking both catches a selector
                // that never matched and a round() the browser did not understand.
                void Check(string property, double resolved, double em)
                {
                    var wanted = em * probeFontSize;

                    if (Math.Abs(resolved - Math.Round(resolved)) > 0.01)
                    {
                        problems.Add($"{glyph.Glyph.CssClass}: {property} is {resolved}px, which is not a whole pixel");
                    }
                    else if (Math.Abs(resolved - wanted) > 0.5 + 0.01)
                    {
                        problems.Add($"{glyph.Glyph.CssClass}: {property} is {resolved}px, further than a rounding step from {wanted}px");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Generated css applied in the browser: checked {sample.Count} icons, {problems.Count} problems");
            foreach (var problem in problems.Take(20)) Console.WriteLine($"  FAILED: {problem}");

            return problems.Count == 0;
        }

        private static double Pixels(string value)
        {
            value = value.Trim();
            if (value.EndsWith("px", StringComparison.Ordinal)) value = value.Substring(0, value.Length - 2);
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var pixels) ? pixels : 0;
        }

        /// <summary>
        /// Screenshots the icons that moved the most, unadjusted next to adjusted, plus the icons that have
        /// to keep overlapping drawn on top of each other. Local artefact for eyeballing, never committed.
        /// </summary>
        private static async Task WritePreview(IPage page, string css, List<FontAdjustments> fonts, string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);

            var font = fonts.FirstOrDefault(f => f.Font.ClassPrefix == "fi-rr-") ?? fonts.First();

            object Cell(bool raw, params string[] classNames) => new { raw, classNames };

            var largest = font.Glyphs
               .Where(g => g.IsAdjusted)
               .OrderByDescending(g => Math.Max(Math.Abs(g.X), Math.Abs(g.Y)))
               .Take(24)
               .Select(g => new
               {
                   label  = g.Glyph.CssClass,
                   detail = $"{OpticalCentering.Format(g.X)} / {OpticalCentering.Format(g.Y)}",
                   cells  = new[] { Cell(true, g.Glyph.CssClass), Cell(false, g.Glyph.CssClass) },
               })
               .ToArray();

            var overlaps = AlignmentGroups.All
               .Select(group => new
               {
                   group,
                   classNames = group.Icons.Where(n => font.Glyphs.Any(g => g.Glyph.IconName == n)).Select(n => font.Font.ClassPrefix + n).ToArray(),
               })
               .Where(g => g.classNames.Length > 1)
               .Select(g => new
               {
                   label  = g.group.Name,
                   detail = g.group.Kind.ToString(),
                   cells  = new[] { Cell(true, g.classNames), Cell(false, g.classNames) },
               })
               .ToArray();

            // Icons whose names share a stem are usually the same drawing with different decoration, so
            // they are the place to look for a family that has drifted apart.
            var families = new[] { "volume", "wifi", "signal-bars", "square", "circle", "battery", "arrow-small", "chevron-double", "user" }
               .Select(stem => new
               {
                   stem,
                   members = font.Glyphs
                      .Where(g => g.Glyph.IconName == stem || g.Glyph.IconName.StartsWith(stem + "-", StringComparison.Ordinal))
                      .OrderBy(g => g.Glyph.IconName, StringComparer.Ordinal)
                      .Take(10)
                      .ToArray(),
               })
               .Where(f => f.members.Length > 1)
               .SelectMany(f => new[]
               {
                   new
                   {
                       label  = f.stem + "-*",
                       detail = "unadjusted",
                       cells  = f.members.Select(m => Cell(true, m.Glyph.CssClass)).ToArray(),
                   },
                   new
                   {
                       label  = "",
                       detail = "adjusted",
                       cells  = f.members.Select(m => Cell(false, m.Glyph.CssClass)).ToArray(),
                   },
               })
               .ToArray();

            var sections = new (string Slug, string Title, object Rows)[]
            {
                ("largest", $"Largest adjustments in {font.Font.FontFamily} (left: unadjusted, right: adjusted)", largest),
                ("overlapping", "Icons that must keep overlapping (left: unadjusted, right: adjusted)", overlaps),
                ("families", "Name families, to spot a family drifting apart", families),
            };

            Console.WriteLine();

            // One screenshot per section, so each image stays legible instead of one very tall page.
            foreach (var (slug, title, rows) in sections)
            {
                await page.EvaluateAsync(MeasurementPage.BuildPreviewScript, new
                {
                    css,
                    sections = new object[] { new { title, rows } },
                });

                var file = Path.Combine(outputDirectory, $"{font.Font.FontFamily}-{slug}.png");
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = file, FullPage = true });
                Console.WriteLine($"Wrote preview {file}");
            }
        }

        private static string LocateRepositoryRoot()
        {
            foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            {
                var directory = new DirectoryInfo(start);

                while (directory != null)
                {
                    if (File.Exists(Path.Combine(directory.FullName, "Tesserae.sln"))) return directory.FullName;
                    directory = directory.Parent;
                }
            }

            throw new InvalidOperationException("Could not find Tesserae.sln; run this from inside the repository.");
        }
    }

    /// <summary>Command line options; the defaults are what the committed stylesheet was generated with.</summary>
    internal sealed class Options
    {
        public CenteringSettings Settings  { get; } = new CenteringSettings();
        public List<string>      OnlyFonts { get; } = new List<string>();
        public int               ChunkSize { get; private set; } = 1500;
        public bool              Preview   { get; private set; }
        public bool              Headed    { get; private set; }
        public bool              ShowHelp  { get; private set; }
        public string            DumpPath  { get; private set; }

        public static Options Parse(string[] args)
        {
            var options = new Options();
            var index   = 0;

            string Value(string option) => index + 1 < args.Length
                ? args[++index]
                : throw new ArgumentException($"Option '{option}' needs a value.");

            for (; index < args.Length; index++)
            {
                var option = args[index];

                switch (option)
                {
                    case "--help":
                    case "-h":                options.ShowHelp = true; break;
                    case "--preview":         options.Preview = true; break;
                    case "--headed":          options.Headed = true; break;
                    case "--font":            options.OnlyFonts.Add(Value(option)); break;
                    case "--dump":            options.DumpPath = Value(option); break;
                    case "--chunk":           options.ChunkSize = int.Parse(Value(option), CultureInfo.InvariantCulture); break;
                    case "--em":              options.Settings.RasterEm = int.Parse(Value(option), CultureInfo.InvariantCulture); break;
                    case "--mass-weight":     options.Settings.MassWeight = Number(Value(option)); break;
                    case "--optical-cap":     options.Settings.MaxOpticalPull = Number(Value(option)); break;
                    case "--step":            options.Settings.Step = Number(Value(option)); break;
                    case "--dead-zone":       options.Settings.DeadZone = Number(Value(option)); break;
                    case "--cap":             options.Settings.MaxAdjustment = Number(Value(option)); break;
                    case "--frame-tolerance": options.Settings.FrameTolerance = Number(Value(option)); break;
                    case "--frame-spread":    options.Settings.MaxSharedFrameSpread = Number(Value(option)); break;
                    case "--trim":            options.Settings.Trim = Number(Value(option)); break;
                    default:                  throw new ArgumentException($"Unknown option '{option}'. Try --help.");
                }
            }

            return options;
        }

        private static double Number(string value) => double.Parse(value, CultureInfo.InvariantCulture);

        public static void PrintUsage()
        {
            Console.WriteLine("""
                Measures every bundled UIcons glyph in a real browser and writes the optical centering
                corrections to Tesserae/tps/assets/css/tss.uicons.adjustments.css.

                  dotnet run --project Build.UIconsOpticalCentering [options]

                  --preview             also screenshot the biggest adjustments and the overlap checks
                  --headed              run the browser visibly
                  --font <substring>    only measure fonts whose family matches (repeatable)
                  --dump <file.csv>     write every measurement and intermediate number to csv
                  --chunk <n>           glyphs measured per browser call (default 1500)
                  --em <px>             font size the glyphs are rasterized at (default 80)
                  --mass-weight <f>     how much optical weight counts against the frame (default 0.25)
                  --optical-cap <em>    cap on the optical pull (default 0.01)
                  --step <em>           rounding step for the emitted offsets (default 0.005)
                  --dead-zone <em>      offsets below this are dropped (default 0.01)
                  --cap <em>            cap on the emitted offset (default 0.06)
                  --frame-tolerance <em> how close two ink boxes must be to count as the same frame (default 0.004)
                  --frame-spread <em>   how closely same-frame icons must agree to be pinned (default 0.005)
                  --trim <f>            ink mass trimmed off each side to find the frame (default 0.02)
                """);
        }
    }
}
