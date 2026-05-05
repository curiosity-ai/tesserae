# CLAUDE.md

## Repository overview

Tesserae is a C# UI toolkit for building web applications, compiled to JavaScript via the **h5** compiler.

- Core UI components: `Tesserae/src/Components`
- Component factories and helpers: `Tesserae/src/Base/UI.Components.cs`
- Fluent extensions: `Tesserae/src/Extensions`
- Samples and demos: `Tesserae.Tests/`
- Project and build config: `Tesserae/Tesserae.csproj`, `Tesserae/h5.json`

## Installing h5

Install or update the h5 compiler and templates globally:

```bash
dotnet tool update --global h5-compiler
```

## Build

```bash
dotnet build
```

The h5 compiler translates C# to JavaScript. Output lands in `bin/Debug/netstandard2.0/h5/` (or `bin/Release/...`).

To serve locally:

```bash
cd bin/Debug/netstandard2.0/h5/
dotnet serve --port 5000
```

## UI composition patterns

- Component creation goes through the static `UI` class (`UI.Components.cs`), which exposes factory methods like `UI.Button`, `UI.TextBlock`, etc.
- `UI` is a static partial class with a static constructor used as the central entry point.
- Components are configured via fluent-style extension methods (e.g., `UI.Id`, `UI.Class`, `UI.Do`).

## Conventions

When adding a new component:

1. Add the implementation under `Tesserae/src/Components`.
2. Add a factory method in `UI.Components.cs`.
3. Add fluent helpers or extension methods in `Tesserae/src/Extensions` if needed.
4. Add a sample in `Tesserae.Tests` demonstrating usage.
