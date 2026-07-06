using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Tesserae.Analyzers;
using Xunit;

namespace Tesserae.Analyzers.Tests
{
    public class RouterNavigateAnalyzerTests
    {
        // Minimal stand-in for the real Tesserae.Router surface, so the tests don't need the h5-compiled assembly
        private const string RouterStub = @"
namespace Tesserae
{
    public class Parameters { }

    public static class Router
    {
        public static void Register(string uniquePath, System.Action<Parameters> action) { }
        public static void Register(string uniquePath, System.Func<Parameters, bool> action) { }
        public static void Register(string uniqueIdentifier, string path, System.Action<Parameters> action, bool replace = false) { }
        public static void Navigate(string path, bool reload = false) { }
    }
}
";

        private static Task VerifyAsync(string source, params DiagnosticResult[] expected) =>
            VerifyAsync(source, manifest: null, authoritative: false, expected);

        private static Task VerifyAsync(string source, string manifest, bool authoritative, params DiagnosticResult[] expected)
        {
            var test = new CSharpAnalyzerTest<RouterNavigateAnalyzer, DefaultVerifier>
            {
                TestCode = source + RouterStub,
            };

            if (manifest is object)
            {
                test.TestState.AdditionalFiles.Add(("TesseraeRoutes.txt", manifest));
            }

            if (authoritative)
            {
                test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig",
                    "is_global = true\ndotnet_diagnostic.TSS0001.route_table_is_authoritative = true\n"));
            }

            test.ExpectedDiagnostics.AddRange(expected);
            return test.RunAsync();
        }

        [Fact]
        public Task NavigateToRegisteredRoute_NoDiagnostic() => VerifyAsync(@"
static class App
{
    static void Main()
    {
        Tesserae.Router.Register(""home"", _ => { });
        Tesserae.Router.Navigate(""#/home"");
    }
}
");

        [Fact]
        public Task NavigateToUnregisteredRoute_Warns() => VerifyAsync(@"
static class App
{
    static void Main()
    {
        Tesserae.Router.Register(""home"", _ => { });
        Tesserae.Router.Navigate({|#0:""#/settings""|});
    }
}
",
            new DiagnosticResult(RouterNavigateAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(0).WithArguments("#/settings"));

        [Fact]
        public Task NavigateToParameterizedRoute_NoDiagnostic() => VerifyAsync(@"
static class App
{
    static void Main()
    {
        Tesserae.Router.Register(""view/:id"", _ => { });
        Tesserae.Router.Navigate(""#/view/42"");
    }
}
");

        [Fact]
        public Task NavigateWithWrongSegmentCount_Warns() => VerifyAsync(@"
static class App
{
    static void Main()
    {
        Tesserae.Router.Register(""view/:id"", _ => { });
        Tesserae.Router.Navigate({|#0:""#/view""|});
    }
}
",
            new DiagnosticResult(RouterNavigateAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(0).WithArguments("#/view"));

        [Fact]
        public Task MatchingIsCaseInsensitiveAndIgnoresHashAndSlashes_NoDiagnostic() => VerifyAsync(@"
static class App
{
    static void Main()
    {
        Tesserae.Router.Register(""#/Documents/List"", _ => { });
        Tesserae.Router.Navigate(""documents/list"");
    }
}
");

        [Fact]
        public Task TwoArgumentRegister_MatchesOnPathNotIdentifier() => VerifyAsync(@"
static class App
{
    static void Main()
    {
        Tesserae.Router.Register(""docs"", ""documents/:id"", _ => { });
        Tesserae.Router.Navigate(""#/documents/5"");
        Tesserae.Router.Navigate({|#0:""#/docs""|});
    }
}
",
            new DiagnosticResult(RouterNavigateAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(0).WithArguments("#/docs"));

        [Fact]
        public Task QueryStringIsIgnoredWhenMatching_NoDiagnostic() => VerifyAsync(@"
static class App
{
    static void Main()
    {
        Tesserae.Router.Register(""search"", _ => { });
        Tesserae.Router.Navigate(""#/search?term=computer&page=2"");
    }
}
");

        [Fact]
        public Task RootRouteMatchesEmptyHash_NoDiagnostic() => VerifyAsync(@"
static class App
{
    static void Main()
    {
        Tesserae.Router.Register(""home"", ""/"", _ => { });
        Tesserae.Router.Navigate(""#/"");
    }
}
");

        [Fact]
        public Task ExternalUrl_IsIgnored() => VerifyAsync(@"
static class App
{
    static void Main()
    {
        Tesserae.Router.Register(""home"", _ => { });
        Tesserae.Router.Navigate(""https://example.com/other"");
    }
}
");

        [Fact]
        public Task NonConstantNavigatePath_IsIgnored() => VerifyAsync(@"
static class App
{
    static void Main(string[] args)
    {
        Tesserae.Router.Register(""home"", _ => { });
        Tesserae.Router.Navigate(""#/view/"" + args[0]);
    }
}
");

        // Limitation A fix: a dynamic Register with a knowable constant prefix no longer silences the
        // whole compilation - a Navigate that falls outside that prefix (and matches no constant route)
        // is still reported.
        [Fact]
        public Task DynamicRegisterWithPrefix_NavigateOutsidePrefix_Warns() => VerifyAsync(@"
static class App
{
    static void Main(string[] args)
    {
        Tesserae.Router.Register(""home"", _ => { });
        Tesserae.Router.Register(""#/admin/"" + args[0], _ => { });
        Tesserae.Router.Navigate({|#0:""#/settings""|});
    }
}
",
            new DiagnosticResult(RouterNavigateAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(0).WithArguments("#/settings"));

        // A Navigate under a dynamic registration's constant prefix could be handled at runtime, so it is
        // not reported.
        [Fact]
        public Task DynamicRegisterWithPrefix_NavigateUnderPrefix_NoDiagnostic() => VerifyAsync(@"
static class App
{
    static void Main(string[] args)
    {
        Tesserae.Router.Register(""home"", _ => { });
        Tesserae.Router.Register(""#/admin/"" + args[0], _ => { });
        Tesserae.Router.Navigate(""#/admin/users"");
    }
}
");

        // A fully-opaque dynamic registration (no knowable prefix) could match anything, so the check
        // falls back to conservative silence for otherwise-unmatched navigations.
        [Fact]
        public Task OpaqueDynamicRegistration_SuppressesUnmatchedNavigations() => VerifyAsync(@"
static class App
{
    static void Main(string[] args)
    {
        Tesserae.Router.Register(""home"", _ => { });
        Tesserae.Router.Register(args[0], _ => { });
        Tesserae.Router.Navigate(""#/definitely-not-registered"");
    }
}
");

        // ...unless the route table is declared authoritative, in which case dynamic registrations no
        // longer buy leniency and the mismatch is reported.
        [Fact]
        public Task OpaqueDynamicRegistration_Authoritative_Warns() => VerifyAsync(@"
static class App
{
    static void Main(string[] args)
    {
        Tesserae.Router.Register(""home"", _ => { });
        Tesserae.Router.Register(args[0], _ => { });
        Tesserae.Router.Navigate({|#0:""#/definitely-not-registered""|});
    }
}
",
            manifest: null,
            authoritative: true,
            new DiagnosticResult(RouterNavigateAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(0).WithArguments("#/definitely-not-registered"));

        // Limitation B fix: routes declared in a manifest (routes registered in another assembly) are
        // validated even though this compilation has no Router.Register call of its own.
        [Fact]
        public Task ManifestRoutes_UnregisteredNavigate_Warns() => VerifyAsync(@"
static class App
{
    static void Main()
    {
        Tesserae.Router.Navigate({|#0:""#/typo""|});
    }
}
",
            manifest: "#/home\n#/space/:uid\n; a comment\n\n// another comment\n",
            authoritative: false,
            new DiagnosticResult(RouterNavigateAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(0).WithArguments("#/typo"));

        [Fact]
        public Task ManifestRoutes_ValidNavigate_NoDiagnostic() => VerifyAsync(@"
static class App
{
    static void Main()
    {
        Tesserae.Router.Navigate(""#/home"");
        Tesserae.Router.Navigate(""#/space/abc"");
    }
}
",
            manifest: "#/home\n#/space/:uid\n",
            authoritative: false);

        [Fact]
        public Task NoRegistrationsInCompilation_StaysSilent() => VerifyAsync(@"
static class App
{
    static void Main()
    {
        Tesserae.Router.Navigate(""#/registered-by-a-library"");
    }
}
");

        [Fact]
        public Task ConstantsPropagateIntoNavigate() => VerifyAsync(@"
static class App
{
    const string Route = ""#/set"" + ""tings"";

    static void Main()
    {
        Tesserae.Router.Register(""home"", _ => { });
        Tesserae.Router.Navigate({|#0:Route|});
    }
}
",
            new DiagnosticResult(RouterNavigateAnalyzer.DiagnosticId, Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(0).WithArguments("#/settings"));
    }
}
