using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Tesserae.Analyzers
{
    /// <summary>
    /// Reports Router.Navigate calls whose (compile-time constant) path does not match any known
    /// route. Known routes come from two sources so the check keeps working in real multi-project
    /// apps:
    ///
    /// <list type="bullet">
    /// <item>constant paths passed to <c>Router.Register</c> in this compilation;</item>
    /// <item>a route manifest (an <c>AdditionalFiles</c> entry named <c>TesseraeRoutes.txt</c>), which
    /// lets a project validate against routes registered in a different assembly.</item>
    /// </list>
    ///
    /// A single dynamic <c>Router.Register</c> no longer silences the whole compilation. Instead:
    /// <list type="bullet">
    /// <item>a dynamic registration with a knowable constant prefix (e.g. <c>Register(Base + suffix, …)</c>)
    /// only suppresses Navigate paths that fall under that prefix;</item>
    /// <item>a fully-opaque dynamic registration (no knowable prefix) falls back to the old conservative
    /// behavior and suppresses otherwise-unmatched Navigate paths;</item>
    /// <item>setting <c>dotnet_diagnostic.TSS0001.route_table_is_authoritative = true</c> declares the
    /// known route set complete and reports mismatches even in the presence of dynamic registrations.</item>
    /// </list>
    ///
    /// To avoid false positives the check still stays silent when no route is known at all (no constant
    /// registration and no manifest). Navigate calls with non-constant paths and navigations to
    /// absolute/external URLs are likewise skipped.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RouterNavigateAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TSS0001";

        internal const string ManifestFileName             = "TesseraeRoutes.txt";
        internal const string AuthoritativeConfigKey       = "dotnet_diagnostic.TSS0001.route_table_is_authoritative";

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Route passed to Router.Navigate is not registered",
            messageFormat: "The route '{0}' passed to Router.Navigate does not match any route registered with Router.Register (or declared in a TesseraeRoutes.txt manifest)",
            category: "Tesserae.Routing",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Router.Navigate only activates a view when the target path matches a route previously registered with Router.Register; navigating to an unregistered route ends up in OnNotMatched instead of showing a view.",
            helpLinkUri: "https://github.com/curiosity-ai/tesserae/blob/master/docs/ROUTING.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationStart =>
            {
                var routerType = compilationStart.Compilation.GetTypeByMetadataName("Tesserae.Router");

                if (routerType is null) return;

                var manifestRoutes = ReadManifestRoutes(compilationStart.Options, compilationStart.CancellationToken);
                var authoritative  = IsRouteTableAuthoritative(compilationStart.Options);

                var registeredPaths              = new ConcurrentBag<string>();
                var dynamicPrefixes              = new ConcurrentBag<string>();
                var navigations                  = new ConcurrentBag<(string Path, Location Location)>();
                var hasOpaqueDynamicRegistration = new StrongBox<bool>(false);

                compilationStart.RegisterOperationAction(operationContext =>
                {
                    var invocation = (IInvocationOperation)operationContext.Operation;
                    var method     = invocation.TargetMethod;

                    if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, routerType)) return;

                    if (method.Name == "Register")
                    {
                        // The route pattern is the last string parameter: Register(uniquePath, handler)
                        // matches on uniquePath itself, Register(uniqueIdentifier, path, handler[, replace]) on path
                        if (TryGetArgumentForParameter(invocation, method.Parameters.LastOrDefault(p => p.Type.SpecialType == SpecialType.System_String), out var registerArgument))
                        {
                            var (prefix, complete) = GetLeadingConstant(registerArgument.Value);

                            if (complete)
                            {
                                registeredPaths.Add(prefix);
                            }
                            else
                            {
                                // A route built at runtime: record the constant prefix we can see so we only
                                // suppress navigations that could actually be covered by it. With no knowable
                                // prefix the route could be anything, so fall back to conservative suppression.
                                var normalizedPrefix = RouteMatcher.NormalizeForPrefix(RouteMatcher.ParsePattern(prefix));

                                if (normalizedPrefix.Length > 0)
                                {
                                    dynamicPrefixes.Add(normalizedPrefix);
                                }
                                else
                                {
                                    hasOpaqueDynamicRegistration.Value = true;
                                }
                            }
                        }
                    }
                    else if (method.Name == "Navigate")
                    {
                        if (TryGetArgumentForParameter(invocation, method.Parameters.FirstOrDefault(p => p.Type.SpecialType == SpecialType.System_String), out var navigateArgument)
                            && navigateArgument.Value.ConstantValue is { HasValue: true, Value: string navigatePath })
                        {
                            navigations.Add((navigatePath, navigateArgument.Value.Syntax.GetLocation()));
                        }
                        // Non-constant Navigate paths cannot be checked - skip them
                    }
                }, OperationKind.Invocation);

                compilationStart.RegisterCompilationEndAction(compilationEnd =>
                {
                    var knownRoutes = registeredPaths.Concat(manifestRoutes).ToArray();

                    // Nothing is known about the route table (no constant Register in this compilation and no
                    // manifest): stay silent rather than emit false positives.
                    if (knownRoutes.Length == 0) return;

                    var patterns = knownRoutes.Select(RouteMatcher.ParsePattern).ToArray();

                    // In authoritative mode the known route set is declared complete, so dynamic registrations
                    // no longer buy any leniency.
                    var prefixes    = authoritative ? Array.Empty<string>() : dynamicPrefixes.Distinct().ToArray();
                    var suppressAll = !authoritative && hasOpaqueDynamicRegistration.Value;

                    foreach (var (path, location) in navigations)
                    {
                        if (!RouteMatcher.TryGetNavigationParts(path, out var parts)) continue;

                        if (patterns.Any(pattern => RouteMatcher.IsMatch(pattern, parts))) continue;

                        if (suppressAll) continue;

                        var navigationPrefix = RouteMatcher.NormalizeForPrefix(parts);

                        if (prefixes.Any(prefix => navigationPrefix.StartsWith(prefix, StringComparison.Ordinal))) continue;

                        compilationEnd.ReportDiagnostic(Diagnostic.Create(_rule, location, path));
                    }
                });
            });
        }

        private static bool TryGetArgumentForParameter(IInvocationOperation invocation, IParameterSymbol parameter, out IArgumentOperation argument)
        {
            argument = parameter is null
                ? null
                : invocation.Arguments.FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.Parameter, parameter));

            return argument is object;
        }

        /// <summary>
        /// Returns the leading compile-time-constant text of a (possibly runtime-built) string expression
        /// together with whether the whole expression is constant. For <c>"a" + var</c> this yields
        /// (<c>"a"</c>, false); for a fully constant expression it yields (value, true); for a fully opaque
        /// expression such as a bare variable it yields (<c>""</c>, false).
        /// </summary>
        private static (string prefix, bool complete) GetLeadingConstant(IOperation operation)
        {
            if (operation is null) return (string.Empty, false);

            if (operation.ConstantValue is { HasValue: true, Value: string constant }) return (constant, true);

            switch (operation)
            {
                case IConversionOperation conversion:
                    return GetLeadingConstant(conversion.Operand);

                case IParenthesizedOperation parenthesized:
                    return GetLeadingConstant(parenthesized.Operand);

                case IBinaryOperation binary when binary.OperatorKind == BinaryOperatorKind.Add:
                {
                    var (leftPrefix, leftComplete) = GetLeadingConstant(binary.LeftOperand);

                    if (!leftComplete) return (leftPrefix, false);

                    var (rightPrefix, rightComplete) = GetLeadingConstant(binary.RightOperand);

                    return (leftPrefix + rightPrefix, rightComplete);
                }

                case IInterpolatedStringOperation interpolated:
                {
                    var builder = new StringBuilder();

                    foreach (var part in interpolated.Parts)
                    {
                        switch (part)
                        {
                            case IInterpolatedStringTextOperation text when text.Text.ConstantValue is { HasValue: true, Value: string literal }:
                                builder.Append(literal);
                                break;

                            case IInterpolationOperation interpolation when interpolation.Expression.ConstantValue is { HasValue: true, Value: string value }:
                                builder.Append(value);
                                break;

                            default:
                                return (builder.ToString(), false);
                        }
                    }

                    return (builder.ToString(), true);
                }

                default:
                    return (string.Empty, false);
            }
        }

        private static ImmutableArray<string> ReadManifestRoutes(AnalyzerOptions options, CancellationToken cancellationToken)
        {
            var builder = ImmutableArray.CreateBuilder<string>();

            foreach (var file in options.AdditionalFiles)
            {
                if (!IsManifestFile(Path.GetFileName(file.Path))) continue;

                var text = file.GetText(cancellationToken);

                if (text is null) continue;

                foreach (var line in text.Lines)
                {
                    var route = line.ToString().Trim();

                    // Blank lines and comments (';' or '//') are skipped; '#' can start a real hash route.
                    if (route.Length == 0) continue;
                    if (route.StartsWith(";", StringComparison.Ordinal)) continue;
                    if (route.StartsWith("//", StringComparison.Ordinal)) continue;

                    builder.Add(route);
                }
            }

            return builder.ToImmutable();
        }

        private static bool IsManifestFile(string fileName) =>
            string.Equals(fileName, ManifestFileName, StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith("." + ManifestFileName, StringComparison.OrdinalIgnoreCase);

        private static bool IsRouteTableAuthoritative(AnalyzerOptions options) =>
            options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(AuthoritativeConfigKey, out var value)
            && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }
}
