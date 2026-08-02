using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers.Binary;
using Microsoft.Playwright;

namespace Build.UpdateInterfaceIcons
{
    /// <summary>
    /// Renders every bundled UIcons glyph in a browser, one by one, works out how far each one is from
    /// being optically centred in the box it is laid out in, and bakes the corrections into the glyph
    /// outlines of the woff2 files, so the icons arrive centred with no stylesheet involved.
    /// <para>
    /// Runs as the last stage of updating the icon set, because it edits the downloaded fonts and the
    /// codepoints it keys off change with every UIcons release.
    /// </para>
    /// </summary>
    internal static class OpticalCenteringStage
    {
        /// <summary>Measures every glyph, bakes the offsets into the fonts, and verifies the result.</summary>
        public static async Task<bool> RunAsync(string repoRoot, CenteringOptions options)
        {
            var cssDir   = Path.Combine(repoRoot, "Tesserae", "tps", "assets", "css");
            var assets   = Path.Combine(repoRoot, "Tesserae", "tps", "assets");
            var fontsDir = Path.Combine(assets, "fonts");

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

            var ok = CoherenceReport.Print(adjustments, options.Settings);

            if (warnings.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"{warnings.Count} warnings:");
                foreach (var warning in warnings.Take(40)) Console.WriteLine($"  {warning}");
                if (warnings.Count > 40) Console.WriteLine($"  ... and {warnings.Count - 40} more");
            }

            if (options.DumpPath != null)
            {
                MeasurementDump.Write(options.DumpPath, adjustments);
                Console.WriteLine();
                Console.WriteLine($"Wrote measurements {options.DumpPath}");
            }

            BakeIntoFontOutlines(fontsDir, adjustments);

            // Nothing downstream means anything if the browser will not take the files, so ask it first.
            if (!await VerifyFontsDecodeInTheBrowser(page, server.BaseUrl, fonts))
            {
                Console.WriteLine();
                Console.WriteLine("Some checks FAILED, see above.");
                return false;
            }

            // Re-measure the fonts that were just edited: the correction only counts if the glyphs now
            // sit at the centre of their box, measured the same way as before, from the shipped files.
            ok &= await VerifyFontsAreNowCentred(page, fonts, adjustments, options);

            Console.WriteLine();
            Console.WriteLine(ok ? "All checks passed." : "Some checks FAILED, see above.");

            return ok;
        }

        /// <summary>
        /// Shifts the glyph outlines in the woff2 files by the measured offsets. Done in the woff2 glyph
        /// encoding directly, where coordinates are deltas, so moving a glyph is a matter of rewriting its
        /// first point: nothing else in the font is re-encoded, which is what keeps the declared bounding
        /// boxes, the side bearings and every untouched glyph exactly as the vendor shipped them.
        /// </summary>
        private static void BakeIntoFontOutlines(string fontsDir, List<FontAdjustments> fonts)
        {

            Console.WriteLine();
            Console.WriteLine("Baking the offsets into the glyph outlines");

            foreach (var font in fonts)
            {
                var fontPath = Path.Combine(fontsDir, $"{font.Font.FontFamily}.woff2");
                var file     = Woff2File.Read(fontPath);
                var upem     = BinaryPrimitives.ReadUInt16BigEndian(file["head"].Data.AsSpan(18));
                var byCode   = CmapLookup.Read(file["cmap"].Data);
                var glyf     = TransformedGlyf.Parse(file["glyf"].Data);

                var shifts  = new Dictionary<int, (int Dx, int Dy)>();
                var worst   = 0.0;

                foreach (var glyph in font.Glyphs.Where(g => g.IsAdjusted))
                {
                    if (!byCode.TryGetValue(glyph.Glyph.CodePoint, out var id)) continue;

                    // css x grows right and y grows down; font units grow right and up
                    var dx = (int)Math.Round(glyph.X * upem);
                    var dy = (int)Math.Round(-glyph.Y * upem);
                    worst  = Math.Max(worst, Math.Max(Math.Abs(dx - glyph.X * upem), Math.Abs(dy + glyph.Y * upem)));

                    if (dx == 0 && dy == 0) continue;

                    if (shifts.ContainsKey(id))
                    {
                        throw new InvalidOperationException(
                            $"{font.Font.FontFamily}: glyph {id} is claimed by two icon classes, refusing to move it twice");
                    }

                    shifts[id] = (dx, dy);
                }

                // The container declares how big glyf is once rebuilt from the transformed streams. If the
                // model that computes it disagrees with what the vendor declared for the untouched font, it
                // is wrong, and the patched font would be rejected as malformed rather than looking off.
                var declared      = file["glyf"].OriginalLength;
                var reconstructed = glyf.ReconstructedLength();

                if (reconstructed != declared)
                {
                    throw new InvalidOperationException(
                        $"{font.Font.FontFamily}: the glyf table rebuilds to {reconstructed} bytes but the font declares " +
                        $"{declared}, so the size model is wrong and the patched length cannot be trusted");
                }

                var delta = glyf.Move(shifts);
                file["glyf"].Data           = glyf.Serialize();
                file["glyf"].OriginalLength = (uint)(declared + delta);
                file.AdjustTotalSfntSize(delta);
                file.Write(fontPath);

                Console.WriteLine($"  {font.Font.FontFamily + ".woff2",-34} {shifts.Count,5} glyphs moved, " +
                                  $"worst rounding {worst / upem:0.#####}em, " +
                                  $"{glyf.BoxesAdded} boxes pinned down, {new FileInfo(fontPath).Length / 1024} KB");
            }
        }

        /// <summary>
        /// Asks the browser to decode each patched font. Worth its own check because the woff2 container has
        /// consistency rules that a font library will read straight past - a stale table length, a file that
        /// does not end on a four byte boundary - and the browser will not. A font it rejects renders as
        /// nothing at all, which would show up in the next stage as every glyph being wildly off centre
        /// rather than as the malformed file it is.
        /// </summary>
        private static async Task<bool> VerifyFontsDecodeInTheBrowser(IPage page, string baseUrl, List<IconFont> fonts)
        {
            Console.WriteLine();
            Console.WriteLine("Checking the browser accepts the patched fonts");

            var broken = new List<string>();

            foreach (var font in fonts)
            {
                // The query string is ignored by the server and defeats the cache, so this is the file on
                // disk being decoded and not the copy the first pass loaded.
                var error = await page.EvaluateAsync<string>(
                    @"async url => {
                          const face = new FontFace('decode-probe', 'url(""' + url + '"") format(""woff2"")');
                          try { await face.load(); return ''; }
                          catch (e) { return String((e && e.message) || e); }
                      }",
                    $"{baseUrl}fonts/{font.FontFamily}.woff2?patched");

                if (error.Length > 0) broken.Add($"{font.FontFamily}.woff2 was rejected: {error}");
            }

            foreach (var problem in broken) Console.WriteLine($"  {problem}");

            Console.WriteLine(broken.Count == 0
                ? $"  all {fonts.Count} patched fonts decode"
                : $"  {broken.Count} of {fonts.Count} patched fonts are malformed, so the container was written wrong");

            return broken.Count == 0;
        }

        /// <summary>
        /// Measures the patched fonts again and checks that the corrections landed: for every glyph that
        /// was shifted, how far it sits from the centre of its box should have dropped by roughly the
        /// amount it was shifted by, and certainly not grown. This is what replaces reading offsets back
        /// out of a stylesheet - the proof is now in the rendered glyph rather than in a css declaration.
        /// <para>
        /// Glyphs the pass deliberately left alone are not held to anything: an icon sitting just inside
        /// the dead zone still wants a nudge afterwards, correctly, and one past the cap still wants a big
        /// one.
        /// </para>
        /// </summary>
        private static async Task<bool> VerifyFontsAreNowCentred(
            IPage page, List<IconFont> fonts, List<FontAdjustments> before, CenteringOptions options)
        {
            Console.WriteLine();
            Console.WriteLine("Re-measuring the patched fonts");

            // The first pass left these fonts in the browser's cache, so start from a clean context.
            await page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.Load });

            var wanted = before
               .SelectMany(f => f.Glyphs)
               .Where(g => g.IsAdjusted)
               .ToDictionary(g => g.Glyph.CssClass, g => g, StringComparer.Ordinal);

            // A font unit is 1/300 em, and the measurement itself is not exact to the last decimal.
            const double tolerance = 0.005;

            var problems = new List<string>();
            var landed   = 0;
            var registered = 0;
            double sumBefore = 0, sumAfter = 0;

            foreach (var font in fonts)
            {
                var after   = await MeasureFont(page, font, options.Settings, options.ChunkSize);
                var reduced = OpticalCentering.Compute(font, after, options.Settings, new List<string>());

                foreach (var glyph in reduced.Glyphs)
                {
                    if (!glyph.Measurement.IsUsable || !wanted.TryGetValue(glyph.Glyph.CssClass, out var was)) continue;

                    var offBefore = Math.Max(Math.Abs(was.OpticalX), Math.Abs(was.OpticalY));
                    var offAfter  = Math.Max(Math.Abs(glyph.OpticalX), Math.Abs(glyph.OpticalY));

                    sumBefore += offBefore;
                    sumAfter  += offAfter;
                    landed++;

                    if (offAfter <= offBefore + tolerance) continue;

                    // An icon holding station on another one - a state variant on the icon it is a state
                    // of - is meant to end up off its own centre. That is the whole point of anchoring it.
                    if (was.PinnedToPartner)
                    {
                        registered++;
                        continue;
                    }

                    problems.Add($"{glyph.Glyph.CssClass} was {offBefore:0.000}em off centre, is now {offAfter:0.000}em " +
                                 $"after being shifted by {was.X:0.000}/{was.Y:0.000}");
                }
            }

            if (landed == 0)
            {
                Console.WriteLine("  nothing was shifted, so there is nothing to re-measure");
                return true;
            }

            Console.WriteLine($"  {landed} shifted glyphs re-measured: mean distance from the centre of their box " +
                              $"{sumBefore / landed:0.0000}em -> {sumAfter / landed:0.0000}em");
            Console.WriteLine($"  {problems.Count} ended up further off centre than before, plus {registered} that " +
                              "moved off their own centre to hold station on the icon they are registered with");

            foreach (var problem in problems.Take(10)) Console.WriteLine($"  FAILED: {problem}");

            return problems.Count == 0;
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



        public static string LocateRepositoryRoot()
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
    internal sealed class CenteringOptions
    {
        public CenteringSettings Settings  { get; } = new CenteringSettings();
        public List<string>      OnlyFonts { get; } = new List<string>();
        public int               ChunkSize { get; private set; } = 1500;
        public bool              Headed    { get; private set; }

        /// <summary>Rebuild the fonts even when the vendor download has not changed.</summary>
        public bool Force { get; private set; }

        /// <summary>Re-run only the centering, against whatever fonts are already in the tree.</summary>
        public bool CentreOnly { get; private set; }
        public bool              ShowHelp  { get; private set; }
        public string            DumpPath  { get; private set; }

        public static CenteringOptions Parse(string[] args)
        {
            var options = new CenteringOptions();
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
                    case "--headed":          options.Headed = true; break;
                    case "--force":           options.Force = true; break;
                    case "--centre-only":
                    case "--center-only":     options.CentreOnly = true; break;
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
            var d = new CenteringSettings();

            Console.WriteLine($"""
                Downloads the UIcons webfonts and stylesheets, regenerates the UIcons enum, then measures
                every glyph in a real browser and bakes the optical centering into the glyph outlines.

                  dotnet run --project Build.UpdateInterfaceIcons [options]

                  --force                rebuild even if the vendor download has not changed
                  --centre-only          skip the download and only re-centre the fonts in the tree
                  --headed               run the browser visibly
                  --font <substring>     only measure fonts whose family matches (repeatable)
                  --dump <file.csv>      write every measurement and intermediate number to csv
                  --chunk <n>            glyphs measured per browser call (default {1500})
                  --em <px>              font size the glyphs are rasterized at (default {d.RasterEm})
                  --mass-weight <f>      how much optical weight counts against the frame (default {d.MassWeight})
                  --optical-cap <em>     cap on the optical pull (default {d.MaxOpticalPull})
                  --step <em>            rounding step for the offsets (default {d.Step})
                  --dead-zone <em>       offsets below this are dropped (default {d.DeadZone})
                  --cap <em>             offsets above this are left as drawn (default {d.MaxAdjustment})
                  --frame-tolerance <em> how close two ink boxes must be to count as the same frame (default {d.FrameTolerance})
                  --frame-spread <em>    how closely same-frame icons must agree to be pinned (default {d.MaxSharedFrameSpread})
                  --trim <f>             ink mass trimmed off each side to find the frame (default {d.Trim})
                """);
        }
    }
}
