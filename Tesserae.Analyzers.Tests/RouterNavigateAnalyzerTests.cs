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

        private static Task VerifyAsync(string source, params DiagnosticResult[] expected)
        {
            var test = new CSharpAnalyzerTest<RouterNavigateAnalyzer, DefaultVerifier>
            {
                TestCode = source + RouterStub,
            };
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

        [Fact]
        public Task DynamicRegistration_SuppressesAllDiagnostics() => VerifyAsync(@"
static class App
{
    static void Main(string[] args)
    {
        Tesserae.Router.Register(""view/"" + args[0], _ => { });
        Tesserae.Router.Navigate(""#/definitely-not-registered"");
    }
}
");

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
