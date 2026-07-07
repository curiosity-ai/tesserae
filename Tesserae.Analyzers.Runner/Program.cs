using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Tesserae.Analyzers.Runner
{
    /// <summary>
    /// Runs the Tesserae Roslyn analyzers over a project's sources purely to surface diagnostics.
    ///
    /// The h5 build replaces the normal C# CoreCompile with a transpiler CLI call, so Roslyn (and
    /// therefore the analyzers) never runs during <c>dotnet build</c>. This tool rebuilds a Roslyn
    /// <see cref="CSharpCompilation"/> from the source files and resolved references that MSBuild
    /// already computed, runs the analyzer, and reports only its diagnostics (any leftover CS errors
    /// from the shadow compilation are ignored on purpose - h5 owns the real compile).
    /// </summary>
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            try
            {
                var options = Options.Parse(args);

                if (options is null) return 0; // usage already printed; do not break the build

                return await RunAsync(options).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Fail open: a bug in the runner must not break an otherwise-valid build. Surface it
                // loudly as a warning so it is visible in the build log, but do not fail.
                Console.Error.WriteLine($"TesseraeAnalyzers: warning : analyzer runner failed and was skipped: {ex}");
                return 0;
            }
        }

        private static async Task<int> RunAsync(Options options)
        {
            var analyzers = LoadAnalyzers(options.AnalyzerPaths);

            if (analyzers.IsEmpty)
            {
                Console.Error.WriteLine("TesseraeAnalyzers: warning : no analyzers found; skipping");
                return 0;
            }

            var parseOptions = new CSharpParseOptions(options.LanguageVersion, preprocessorSymbols: options.Defines);

            var trees = options.SourceFiles
               .Where(File.Exists)
               .Select(path => CSharpSyntaxTree.ParseText(SourceText.From(File.ReadAllText(path), Encoding.UTF8), parseOptions, path))
               .ToImmutableArray();

            var references = options.ReferencePaths
               .Where(File.Exists)
               .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
               .ToImmutableArray();

            var compilation = CSharpCompilation.Create(
                "TesseraeAnalysis",
                trees,
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

            var additionalFiles = options.AdditionalFiles
               .Where(File.Exists)
               .Select(path => (AdditionalText)new PhysicalAdditionalText(path))
               .ToImmutableArray();

            var analyzerOptions = new AnalyzerOptions(additionalFiles, new GlobalConfigOptionsProvider(options.ConfigOptions));

            var withAnalyzers = compilation.WithAnalyzers(
                analyzers,
                new CompilationWithAnalyzersOptions(
                    analyzerOptions,
                    onAnalyzerException: (ex, analyzer, diagnostic) =>
                        Console.Error.WriteLine($"TesseraeAnalyzers: warning : analyzer '{analyzer}' threw: {ex.Message}"),
                    concurrentAnalysis: true,
                    logAnalyzerExecutionTime: false));

            var diagnostics = await withAnalyzers.GetAnalyzerDiagnosticsAsync(CancellationToken.None).ConfigureAwait(false);

            var errorCount = 0;

            foreach (var diagnostic in diagnostics.Where(d => d.Id.StartsWith("TSS", StringComparison.Ordinal))
                                                  .OrderBy(d => d.Location.SourceTree?.FilePath, StringComparer.Ordinal))
            {
                var escalate = options.WarningsAsErrors && diagnostic.Severity == DiagnosticSeverity.Warning;
                var isError  = diagnostic.Severity == DiagnosticSeverity.Error || escalate;

                if (isError) errorCount++;

                Console.WriteLine(Format(diagnostic, isError));
            }

            return errorCount > 0 ? 1 : 0;
        }

        private static ImmutableArray<DiagnosticAnalyzer> LoadAnalyzers(IReadOnlyList<string> paths)
        {
            var builder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();

            foreach (var path in paths)
            {
                if (!File.Exists(path)) continue;

                var reference = new AnalyzerFileReference(path, AssemblyLoader.Instance);
                builder.AddRange(reference.GetAnalyzers(LanguageNames.CSharp));
            }

            return builder.ToImmutable();
        }

        /// <summary>Canonical MSBuild diagnostic line so the build log / CI parses it correctly.</summary>
        private static string Format(Diagnostic diagnostic, bool asError)
        {
            var span     = diagnostic.Location.GetLineSpan();
            var file     = span.IsValid ? span.Path : "Tesserae.Analyzers";
            var line     = span.StartLinePosition.Line + 1;
            var column   = span.StartLinePosition.Character + 1;
            var severity = asError ? "error" : "warning";
            var message  = diagnostic.GetMessage();

            return span.IsValid
                ? $"{file}({line},{column}): {severity} {diagnostic.Id}: {message}"
                : $"{file}: {severity} {diagnostic.Id}: {message}";
        }

        private sealed class AssemblyLoader : IAnalyzerAssemblyLoader
        {
            public static readonly AssemblyLoader Instance = new AssemblyLoader();

            public void AddDependencyLocation(string fullPath) { }

            public Assembly LoadFromPath(string fullPath) => Assembly.LoadFrom(fullPath);
        }

        private sealed class PhysicalAdditionalText : AdditionalText
        {
            private readonly string _path;

            public PhysicalAdditionalText(string path) => _path = path;

            public override string Path => _path;

            public override SourceText? GetText(CancellationToken cancellationToken = default) =>
                File.Exists(_path) ? SourceText.From(File.ReadAllText(_path)) : null;
        }

        private sealed class GlobalConfigOptionsProvider : AnalyzerConfigOptionsProvider
        {
            private readonly AnalyzerConfigOptions _global;
            private readonly AnalyzerConfigOptions _empty = new DictionaryOptions(new Dictionary<string, string>());

            public GlobalConfigOptionsProvider(IReadOnlyDictionary<string, string> options) =>
                _global = new DictionaryOptions(options);

            public override AnalyzerConfigOptions GlobalOptions => _global;

            public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _empty;

            public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _empty;

            private sealed class DictionaryOptions : AnalyzerConfigOptions
            {
                private readonly IReadOnlyDictionary<string, string> _options;

                public DictionaryOptions(IReadOnlyDictionary<string, string> options) =>
                    _options = new Dictionary<string, string>(options.Count, StringComparer.OrdinalIgnoreCase)
                        .Also(d => { foreach (var kv in options) d[kv.Key] = kv.Value; });

                public override bool TryGetValue(string key, out string value) => _options.TryGetValue(key, out value!);
            }
        }

        private sealed class Options
        {
            public List<string>                     AnalyzerPaths   { get; } = new List<string>();
            public List<string>                     SourceFiles     { get; } = new List<string>();
            public List<string>                     ReferencePaths  { get; } = new List<string>();
            public List<string>                     AdditionalFiles { get; } = new List<string>();
            public Dictionary<string, string>       ConfigOptions   { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            public ImmutableArray<string>           Defines         { get; private set; } = ImmutableArray<string>.Empty;
            public LanguageVersion                  LanguageVersion { get; private set; } = LanguageVersion.CSharp7_2;
            public bool                             WarningsAsErrors { get; private set; }

            public static Options? Parse(string[] args)
            {
                var options = new Options();

                for (var i = 0; i < args.Length; i++)
                {
                    var arg = args[i];

                    string Next() => ++i < args.Length ? args[i] : throw new ArgumentException($"missing value after {arg}");

                    switch (arg)
                    {
                        case "--analyzer":        options.AnalyzerPaths.Add(Next()); break;
                        case "--sources":         options.SourceFiles.AddRange(ReadListFile(Next())); break;
                        case "--refs":            options.ReferencePaths.AddRange(ReadListFile(Next())); break;
                        case "--additionalfile":  options.AdditionalFiles.Add(Next()); break;
                        case "--define":          options.Defines = SplitDefines(Next()); break;
                        case "--langversion":     options.LanguageVersion = ParseLangVersion(Next()); break;
                        case "--warnaserror":     options.WarningsAsErrors = true; break;
                        case "--config":
                        {
                            var pair  = Next();
                            var split = pair.IndexOf('=');
                            if (split > 0) options.ConfigOptions[pair.Substring(0, split)] = pair.Substring(split + 1);
                            break;
                        }
                        default:
                            Console.Error.WriteLine($"TesseraeAnalyzers: warning : unknown argument '{arg}'");
                            break;
                    }
                }

                if (options.AnalyzerPaths.Count == 0 || options.SourceFiles.Count == 0)
                {
                    Console.Error.WriteLine("usage: Tesserae.Analyzers.Runner --analyzer <dll> --sources <listfile> --refs <listfile> [--define A;B] [--langversion 7.2] [--additionalfile <path>] [--config key=value] [--warnaserror]");
                    return null;
                }

                return options;
            }

            private static IEnumerable<string> ReadListFile(string path) =>
                File.Exists(path)
                    ? File.ReadAllLines(path).Select(l => l.Trim()).Where(l => l.Length > 0)
                    : Enumerable.Empty<string>();

            private static ImmutableArray<string> SplitDefines(string value) =>
                value.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                     .Select(s => s.Trim())
                     .Where(s => s.Length > 0)
                     .ToImmutableArray();

            private static LanguageVersion ParseLangVersion(string value) =>
                LanguageVersionFacts.TryParse(value, out var parsed) ? parsed : LanguageVersion.CSharp7_2;
        }
    }

    internal static class FunctionalExtensions
    {
        public static T Also<T>(this T value, Action<T> action)
        {
            action(value);
            return value;
        }
    }
}
