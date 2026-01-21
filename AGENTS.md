# Guidelines for Building Tesserae Applications

Tesserae is a UI toolkit for building web applications entirely in C#. It leverages the [h5 C# to JavaScript compiler](https://github.com/theolivenbaum/h5) to provide a strongly typed development experience.

## Core Principles

### 1. Component Creation
- **Prefer Static Factory Methods**: Instead of using constructors with the `new` keyword, always use the static factory methods provided in the `Tesserae.UI` class.
  - *Correct*: `Button("Click Me")`, `VStack()`, `TextBox()`
  - *Incorrect*: `new Button("Click Me")`, `new Stack(Stack.Orientation.Vertical)`
- **Implicit UI Namespace**: Many components are available through `static Tesserae.UI`. Ensure you have `using static Tesserae.UI;` in your file.
- **Low-level HTML usage**: Direct access to the browser DOM APIs (such as `window`, `document`, etc...) can be had by importing `using static H5.Core.dom;`.
- **Low level HTML components**: You can instantiate HTML elements directly using the short forms, for example: `Div(_())`, `Div(_("my-css-class")), Span(_(text:"child span")))`. Use only if necessary, you should prefer to use Tesserae components instead.

### 2. Styling and Configuration
- **Fluent Interface**: Tesserae uses a fluent API for styling and layout.
- **Use Short Methods**: Always prefer short methods over their full-length counterparts. These methods can take a `UnitSize` or an `int` (which automatically maps to pixels).

#### Available Styling IComponent Extension Methods:
Short and full forms can be used interchangeably, but you should prefer short forms and pixel units without '.px()' if possible for code readability.

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

#### Tips
- **Layout Components**: Use `HStack` (horizontal) and `VStack` (vertical) for basic layout. For more complex grids, use `Grid`.
- **Alignment**: `HStack` content can be "visually centered" using `stack.AlignItemsCenter()`.
- **Unit Sizes**: Use the extension methods on integers for sizes: `10.px()`, `100.percent()`, `50.vh()`, `20.vw()`. Short form styling methods accept pixels directly too (example: `.W(200)`, `.ML(16)`).
- **Grid Sizing**: Grids can sized using the constructor, for example: `Grid(1.fr(), 200.px(), 1.fr())`.
- **Custom sizes**: Custom sizing units can be instantiated using for example: `new UnitSize("calc(100% - 10px)")`


### 3. State and Reactivity
- **Observables**: Use `SettableObservable<T>` for single values, `ObservableList<T>` for collections, `ObservableDictionary<TKey, TValue>` for maps.
- **Dynamic Content**: Use `Defer` or `DeferSync` to render content that depends on observables or asynchronous operations.
  - **`Defer`**: Used for asynchronous component generation. It expects a generator that returns a `Task<IComponent>`, and optionally takes observables as input.
    - *Example (Async)*: `Defer(async () => { var data = await FetchData(); return TextBlock(data); })`
    - *Example (Async + Observable)*: `Defer(myObservable, async value => await FetchDataAndRenderAsync(value))`
  - **`DeferSync`**: Used for synchronous component generation, used together with observables. It expects a generator that returns an `IComponent`.
    - *Example*: `DeferSync(myObservable, value => TextBlock(value))`
- **Manual Notifications**: If a property inside an object in an observable changes, you may need to call a notification mechanism (like `observable.Value = observable.Value` or `observableList.ReplaceAll(list.ToList())`) to trigger a UI refresh, as Tesserae's standard observables don't automatically watch nested property changes.

### 4. String translations
- Strings can be automatically translated using the TNT tool. You don't have to setup external package or tool, just to annotate the required strings.
- Ensure you have `using TNT;` and `using static TNT.T;` in your file.
- All strings that should be translated should be annotated with a `.t()` method as follows: `"String to Translate".t()`
- Interpolated strings should be instead wrapped in the method `t(...)` as follows: `t($"My Interpolated string {DateTime.UtcNow:u}")` 

## Environment & Build Setup

### 1. Requirements
- **.NET SDK**: **Always use the latest .NET version (currently .NET 10.0).**
- **h5-compiler**: This dotnet tool is essential for compilation. Always install it globally using the command: `dotnet tool update --global h5-compiler`. Do not specify the version so it always installs the latest.

### 2. Build Configuration
- **H5 Config**: Every app project needs an `h5.json` file to configure output paths, resources (CSS/JS), and reflection settings.
  - Set `"reflection": { "disabled": false, "target": "inline" }` if you use reflection-based features like the automatic sample loader.

## Project Structure
- **C# Source**: Typically in a `src` directory.
- **Assets**: Custom CSS should be stored in `h5/assets/css/` relative to the project root and referenced in `h5.json`. Same for extra JavaScript libraries (store under `h5/assets/js/`) or images (store under `h5/assets/img/`).

## Documentation Reference
- The Tesserae source-code is available in the [repository](https://github.com/curiosity-ai/tesserae/)
- The best way to find available components and their factory methods is to check [Tesserae/src/Base/UI.Components.cs](https://github.com/curiosity-ai/tesserae/blob/master/Tesserae/src/Base/UI.Components.cs).
- Specific components are available in the Components folder of the repository, such as [Button](https://github.com/curiosity-ai/tesserae/blob/master/Tesserae/src/Components/Button.cs)
