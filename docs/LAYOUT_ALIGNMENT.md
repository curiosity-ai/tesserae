# Layout, Alignment, Stack, and Grid

Tesserae provides flexible layout primitives via `Stack` (flexbox) and `Grid` (CSS grid). Use `HStack()`/`VStack()` factory methods or `Grid(...)` to build layouts, and then apply alignment, spacing, and placement via fluent APIs and extensions.

## Stack basics

A `Stack` is a flex container with an orientation:

- `HStack()` creates a horizontal stack.
- `VStack()` creates a vertical stack.
- `Stack(Orientation)` lets you specify a direction explicitly.

```csharp
var layout = HStack()
    .AlignItemsCenter()
    .JustifyContent(ItemJustify.Between)
    .Children(
        Button("Cancel"),
        Button("Save").Primary()
    );
```

`Stack` alignment APIs map to flexbox properties:

- `AlignItems(...)`, `AlignContent(...)`
- `JustifyContent(...)`, `JustifyItems(...)`
- `CanWrap`, `IsInline` for wrapping/inline behavior.

### Align individual items in a stack

Use extension methods on components to align individual items within a stack:

```csharp
var row = HStack().Children(
    TextBlock("Left"),
    TextBlock("Center").AlignCenter(),
    TextBlock("Right").AlignEnd()
);
```

Alignment helpers are defined on `IComponentExtensions` and map to `Stack.SetAlign`/`Stack.SetJustify` calls.

## Grid basics

`Grid` maps to CSS grid and supports explicit column/row templates:

```csharp
var grid = Grid(1.fr(), 2.fr())
    .Rows(64.px(), 1.fr())
    .Gap(16.px());
```

- `Grid(params UnitSize[] columns)` initializes columns.
- `Rows(...)`, `Columns(...)`, `Gap(...)`, `RowGap(...)`, `ColumnGap(...)` configure grid behavior.

### Placing items

Use `GridColumn`/`GridRow` extensions to place items inside grid cells:

```csharp
var grid = Grid(1.fr(), 1.fr()).Rows(1.fr(), 1.fr());

var header = TextBlock("Header").GridColumnStretch();
var left   = TextBlock("Left").GridRow(2, 3).GridColumn(1, 2);
var right  = TextBlock("Right").GridRow(2, 3).GridColumn(2, 3);

grid.Children(header, left, right);
```

These extensions map to `Grid.SetGridColumn` and `Grid.SetGridRow` and must be called before the component is added to the grid so the styles are propagated correctly.

## Stretching, grow, and spacing helpers

Use layout extensions to control how items size within stacks and grids:

```csharp
var form = VStack().Children(
    TextBlock("Name"),
    TextBox().Stretch(),
    HStack().Children(
        Button("Cancel"),
        Button("Save").Grow()
    )
);
```

- `Stretch()` sets width and height to 100%.
- `Grow(int)`/`Shrink()`/`NoShrink()` map to flex-grow/flex-shrink on stack items.
