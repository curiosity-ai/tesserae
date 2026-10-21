using System.Runtime.CompilerServices;

// The Route State sample tests RouteLocation directly. Written here rather than as an <InternalsVisibleTo> item in the csproj,
// which the Transpose compiler skips.
[assembly: InternalsVisibleTo("Tesserae.Tests")]
