# AGENTS

## Repository overview
- Tesserae is a C# UI toolkit compiled to JavaScript via the **h5** compiler (see `Tesserae/Tesserae.csproj` and `Tesserae/h5*.json`).
- Core UI components live under `Tesserae/src`, with component factories and helpers in `Tesserae/src/Base/UI.Components.cs`.
- Samples and demos live in `Tesserae.Tests` (referenced in `README.md`).

## Build & packaging notes
- The main library project uses the `h5.Target` SDK and references `h5`, `h5.core`, and `h5.Newtonsoft.Json` packages.
- H5 build configuration and resource bundling are defined in `Tesserae/h5.json` (includes JS/CSS bundling, minified and non-minified outputs, and resource packaging).
- Local dev hosting is documented in `README.md` (build output under `bin/Debug/netstandard2.0/h5` and serve via `dotnet serve`).

## UI composition patterns
- UI creation is centered around the static `UI` class in `UI.Components.cs`, which provides factory methods for components (e.g., `UI.Button`, `UI.TextBlock`, etc.).
- **Static constructor pattern**: `UI` is a static partial class with a static constructor used as a central, static entry point for component creation and helpers.
- **Fluent APIs**: Many components are configured via fluent-style extension methods (see helpers like `UI.Id`, `UI.Class`, `UI.RemoveClass`, `UI.Do`, etc., in `UI.Components.cs`, plus additional extensions in `Tesserae/src/Extensions`).

## Conventions
- When adding a new component, consider adding:
  - The component implementation under `Tesserae/src/Components`.
  - A factory method in `UI.Components.cs` for consistency with existing usage.
  - Any fluent helper or extension methods in `Tesserae/src/Extensions` if needed.
