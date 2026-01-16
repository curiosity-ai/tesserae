# Guidelines for Building Tesserae Applications

Tesserae is a UI toolkit for building web applications entirely in C#. It leverages the [h5 C# to JavaScript compiler](https://github.com/theolivenbaum/h5) to provide a strongly typed development experience.

## Core Principles

### 1. Component Creation
- **Prefer Static Factory Methods**: Instead of using constructors with the `new` keyword, always use the static factory methods provided in the `Tesserae.UI` class.
  - *Correct*: `Button("Click Me")`, `VStack()`, `TextBox()`
  - *Incorrect*: `new Button("Click Me")`, `new Stack(Stack.Orientation.Vertical)`
- **Implicit UI Namespace**: Many components are available through `static Tesserae.UI`. Ensure you have `using static Tesserae.UI;` in your file.

### 2. Styling and Configuration
- **Fluent Interface**: Tesserae uses a fluent API for styling and layout.
- **Use Short Methods**: Always prefer short methods over their full-length counterparts. These methods can take a `UnitSize` or an `int` (which automatically maps to pixels).

#### Available Short Methods:
| Short Method | Full Method | Description |
| :--- | :--- | :--- |
| `.W(val)` | `.Width(val)` | Set width |
| `.H(val)` | `.Height(val)` | Set height |
| `.S()` | `.Stretch()` | Stretch both width and height to 100% |
| `.WS()` | `.WidthStretch()` | Stretch width to 100% |
| `.HS()` | `.HeightStretch()` | Stretch height to 100% |
| `.M(val)` | `.Margin(val)` | Set margin on all sides |
| `.ML(val)` | `.MarginLeft(val)` | Set left margin |
| `.MR(val)` | `.MarginRight(val)` | Set right margin |
| `.MT(val)` | `.MarginTop(val)` | Set top margin |
| `.MB(val)` | `.MarginBottom(val)` | Set bottom margin |
| `.P(val)` | `.Padding(val)` | Set padding on all sides |
| `.PL(val)` | `.PaddingLeft(val)` | Set left padding |
| `.PR(val)` | `.PaddingRight(val)` | Set right padding |
| `.PT(val)` | `.PaddingTop(val)` | Set top padding |
| `.PB(val)` | `.PaddingBottom(val)` | Set bottom padding |

- **Layout Components**: Use `HStack` (horizontal) and `VStack` (vertical) for basic layout. For more complex grids, use `Grid`.
- **Unit Sizes**: Use the extension methods on integers for sizes: `10.px()`, `100.percent()`, `50.vh()`, `20.vw()`.

### 3. State and Reactivity
- **Observables**: Use `SettableObservable<T>` for single values and `ObservableList<T>` for collections.
- **Dynamic Content**: Use `Defer` or `DeferSync` to render content that depends on observables or asynchronous operations.
  - **`Defer`**: Used for asynchronous component generation. It expects a generator that returns a `Task<IComponent>`.
    - *Example (Async)*: `Defer(async () => { var data = await FetchData(); return TextBlock(data); })`
    - *Example (Observable)*: `Defer(myObservable, value => Task.FromResult((IComponent)TextBlock(value)))`
  - **`DeferSync`**: Used for synchronous component generation. It expects a generator that returns an `IComponent`.
    - *Example*: `DeferSync(myObservable, value => TextBlock(value))`
- **Manual Notifications**: If a property inside an object in an `ObservableList` changes, you may need to call a notification mechanism (like `list.ReplaceAll(list.ToList())`) to trigger a UI refresh, as Tesserae's standard `ObservableList` doesn't automatically watch nested property changes.

### 4. Persistence
- Use the browser's `localStorage` for simple state persistence.
- Avoid external services unless explicitly requested.

## Environment & Build Setup

### 1. Requirements
- **.NET SDK**: **Always use .NET 10.0.**
- **h5-compiler**: This dotnet tool is essential for compilation. Version `0.0.34002` is a verified stable fallback if the latest version has issues.

### 2. Build Configuration
- **Local Tool Manifest**: Ensure a `.config/dotnet-tools.json` exists with the `h5-compiler`.
- **Project Settings**: To build against local source and use the local tool, add `<UseLocalH5>true</UseLocalH5>` to your `.csproj`.
- **H5 Config**: Every app project needs an `h5.json` file to configure output paths, resources (CSS/JS), and reflection settings.
  - Set `"reflection": { "disabled": false, "target": "inline" }` if you use reflection-based features like the automatic sample loader.

### 3. Troubleshooting
- **Linux Environments**: If the `h5` tool fails with library loading errors (e.g., `libdl.so`), set `LD_LIBRARY_PATH`:
  `export LD_LIBRARY_PATH=/usr/lib/x86_64-linux-gnu:$LD_LIBRARY_PATH`

## Project Structure
- **C# Source**: Typically in a `src` directory.
- **Assets**: Custom CSS should be stored in `h5/assets/css/` relative to the project root and referenced in `h5.json`.

## Documentation Reference
- The best way to find available components and their factory methods is to check `Tesserae/src/Base/UI.Components.cs` and other partial files of the `UI` class.
