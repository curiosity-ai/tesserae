using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Tesserae.Analyzers
{
    /// <summary>
    /// Reports Router.Navigate calls whose (compile-time constant) path does not match any
    /// route registered with Router.Register in the same compilation.
    ///
    /// To avoid false positives the check stays silent when the route table cannot be fully
    /// known at compile time: when any Router.Register call uses a non-constant path, or when
    /// the compilation contains no Router.Register call at all (routes registered elsewhere,
    /// e.g. by a referenced library). Navigate calls with non-constant paths and navigations
    /// to absolute/external URLs are likewise skipped.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RouterNavigateAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TSS0001";

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Route passed to Router.Navigate is not registered",
            messageFormat: "The route '{0}' passed to Router.Navigate does not match any route registered with Router.Register in this project",
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

                var registeredPaths          = new ConcurrentBag<string>();
                var navigations              = new ConcurrentBag<(string Path, Location Location)>();
                var hasDynamicRegistration   = new StrongBox<bool>(false);

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
                            if (registerArgument.Value.ConstantValue is { HasValue: true, Value: string registeredPath })
                            {
                                registeredPaths.Add(registeredPath);
                            }
                            else
                            {
                                // A route built at runtime means the route table cannot be known at compile time
                                hasDynamicRegistration.Value = true;
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
                    if (hasDynamicRegistration.Value) return;

                    if (registeredPaths.IsEmpty) return;

                    var patterns = registeredPaths.Select(RouteMatcher.ParsePattern).ToArray();

                    foreach (var (path, location) in navigations)
                    {
                        if (!RouteMatcher.TryGetNavigationParts(path, out var parts)) continue;

                        if (patterns.Any(pattern => RouteMatcher.IsMatch(pattern, parts))) continue;

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
    }
}
