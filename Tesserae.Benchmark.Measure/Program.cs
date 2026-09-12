using Microsoft.Playwright;

namespace Tesserae.Benchmark.Measure;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var options = BenchmarkOptions.Parse(args);

        if (string.IsNullOrEmpty(options.HarnessPath))
        {
            Console.Error.WriteLine("Could not locate Tesserae.Benchmark.Tests harness. Pass --harness <path>.");
            return 2;
        }

        if (!options.SkipBuild)
        {
            try
            {
                await HarnessBuilder.BuildAsync(options.HarnessPath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to build harness: {ex.Message}");
                return 3;
            }
        }

        if (!Directory.Exists(options.HarnessPath) || !File.Exists(Path.Combine(options.HarnessPath, "index.html")))
        {
            Console.Error.WriteLine($"Harness output not found at {options.HarnessPath}. Pass --harness or omit --skip-build.");
            return 4;
        }

        // Make sure Playwright's bundled browsers are installed. This is a
        // no-op on subsequent runs and just prints a hint on the first run.
        var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
        if (exitCode != 0)
        {
            Console.Error.WriteLine($"Playwright browser install returned exit code {exitCode}. Continuing anyway.");
        }

        await using var server = await StaticFileServer.StartAsync(options.HarnessPath, options.Port);
        Console.WriteLine($"[server] serving {options.HarnessPath} on {server.BaseUrl}");

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            var runner = new BenchmarkRunner(options, server.BaseUrl);
            var report = await runner.RunAsync(cts.Token);
            ReportWriter.Write(report, options);
            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Cancelled.");
            return 130;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Benchmark failed: {ex}");
            return 1;
        }
    }
}
